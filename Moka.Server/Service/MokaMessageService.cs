using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moka.Server.Manager;
using Moka.Server.Models;
using UserModel = Moka.Server.Models.UserModel;

namespace Moka.Server.Service
{
    // [Authorize(Policy = "protectedScope")]
    public class MokaMessageService : MokaMessenger.MokaMessengerBase
    {
        private readonly ILogger<MokaMessageService> _logger;
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly OnlineUsersManager _onlineUsers;

        public MokaMessageService(ILogger<MokaMessageService> logger, IUserService userService,
            OnlineUsersManager onlineUsers, IMessageService messageService)
        {
            _logger = logger;
            _userService = userService;
            _onlineUsers = onlineUsers;
            _messageService = messageService;
        }

        public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            _logger.LogInformation("registeing user {Name}", request.User.Nickname);
            var res = await _userService.FindOrCreate(new UserModel(Guid.Empty, request.User.Nickname, request.Password,
                DateTime.Now));
            return await Task.FromResult(new RegisterResponse
            {
                Id = res.UserModel.Guid.ToString()
            });
        }

        public override async Task GetMessageStream(Empty _, IServerStreamWriter<RecievedMessageData> responseStream,
            ServerCallContext context)
        {
            _logger.LogInformation("user call: {Name}", context.RequestHeaders.Count);
            var headers = context.RequestHeaders;
            var user = await _userService.FindAsync(headers.GetValue("authorization"));
            _onlineUsers.Join(user.Name, responseStream);
            // foreach (var header in headers)
            // {
            //     _logger.LogInformation(header.Key);
            //     _logger.LogInformation(header.Value);
            // }
            // var rng = new Random();
            // var now = DateTime.UtcNow;
            // var Summaries = new string[] {"sunny", "cloudy", "rainy"};
            // var i = 0;
            // while (!context.CancellationToken.IsCancellationRequested && i < 20)
            // {
            //     await Task.Delay(500); // Gotta look busy
            //
            //     var forecast = new WeatherData
            //     {
            //         DateTimeStamp = Timestamp.FromDateTime(now.AddDays(i++)),
            //         TemperatureC = rng.Next(-20, 55),
            //         Summary = Summaries[rng.Next(Summaries.Length)]
            //     };
            //
            //     _logger.LogInformation("Sending WeatherData response");
            //
            //     await responseStream.WriteAsync(forecast);
            // }
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100);
            }
            _logger.LogInformation("connection closed",user.Name);
        }

        // public override async Task>
        public override async Task<SendMessageResponse> SendMessage(SendMessageRequest request,
            ServerCallContext context)
        {
            var headers = context.RequestHeaders;
            var token = headers.GetValue("authorization");
            var sender = await _userService.FindAsync(token);
            var reciever = await _userService.FindAsync(request.ReceiverId);

            MessageModel msgModel = new MessageModel(Guid.Empty, request.Payload.ToByteArray(),
                MessageModelType.Text, sender.ToUserModel(), reciever.ToUserModel(), DateTime.Now);
            var stored = await _messageService.Store(msgModel);
            Console.WriteLine(stored.Id);
            await _onlineUsers.SendMeesageIfOnline(reciever.Name,
                new RecievedMessageData
                {
                    CreateDateTime = stored.Created_at.ToString(), MessageId = stored.Id,
                    Payload = ByteString.CopyFrom(stored.Data), SenderId = sender.Name, Type = MessageType.Text
                });
            var resp = new SendMessageResponse
            {
                CreateDateTime = stored.ToString(),
                MessageId = stored.Id
            };
            return resp;
        }
    }
}