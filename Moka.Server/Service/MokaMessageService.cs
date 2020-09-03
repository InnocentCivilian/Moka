using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moka.Server.Events;
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
        private MessageEvents _messageEvents;
        private UserEvents _userEvents;

        public MokaMessageService(ILogger<MokaMessageService> logger, IUserService userService,
            OnlineUsersManager onlineUsers, IMessageService messageService, MessageEvents messageEvents, UserEvents userEvents)
        {
            _logger = logger;
            _userService = userService;
            _onlineUsers = onlineUsers;
            _messageService = messageService;
            _messageEvents = messageEvents;
            _userEvents = userEvents;
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

        public override async Task GetMessageStream(Empty _, IServerStreamWriter<Message> responseStream,
            ServerCallContext context)
        {
            _logger.LogInformation("user call: {Name}", context.RequestHeaders.Count);
            var headers = context.RequestHeaders;
            var user = await _userService.FindAsync(headers.GetValue("authorization"));
            _userEvents.OnUserOnlined(new UserOnlineEventArgs{StreamWriter = responseStream,User = user.ToUserModel()});
            // _onlineUsers.Join(user.Guid, responseStream);
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100);
            }
            _userEvents.OnUserOfflined(user.ToUserModel());
            // _logger.LogInformation("connection closed", user.Name);
        }

        // public override async Task>
        public override async Task<Message> SendMessage(Message request,
            ServerCallContext context)
        {
            var headers = context.RequestHeaders;
            var token = headers.GetValue("authorization");
            var sender = await _userService.FindAsync(token);
            var reciever = await _userService.FindAsync(request.ReceiverId);
            request.SenderId = sender.Guid.ToString();
            request.ReceiverId = reciever.Guid.ToString();
            var stored = await _messageService.Store(request);
            Console.WriteLine(stored.Id);
            // await _onlineUsers.SendMeesageIfOnline(stored.ToMessage());
            _messageEvents.OnMessageReceived(stored.ToMessage());
            return stored.ToMessage();
        }

        public override  async Task<MessageArray> GetOfflineMessageStream(Empty request, ServerCallContext context)
        {
            var headers = context.RequestHeaders;
            var token = headers.GetValue("authorization");
            var user = await _userService.FindAsync(token);
            var messagesData = await _messageService.FindUserInboxMessages(user);
            var messages = messagesData.Select(x => x.ToMessage()).ToList();
            return await Task.FromResult(
                new MessageArray
                {
                    Messages = {messages}
                }
            );
        }

        public override async Task<Empty> CumulativeAck(MessageAck request, ServerCallContext context)
        {
            var headers = context.RequestHeaders;
            var token = headers.GetValue("authorization");
            var user = await _userService.FindAsync(token);
            await _messageService.UpdateAck(user.ToUserModel(), request);
            return new Empty();
        }
    }
}