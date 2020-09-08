using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Moka.Sdk.Entity;
using Moka.Sdk.Extenstion;
using Moka.Sdk.Helper;
using Moka.Sdk.Service;
using Moka.Sdk.SqlLite;
using Moka.SharedKernel.Security;

namespace Moka.Sdk
{
    public interface IMeService
    {
        Me _me { get; set; }
        Metadata headers { get; }
        Task<bool> Register();
        Task<bool> Login();
        string CalculateTotp();
        void MessageStream();
        Task<Message> SendMessage(Message message);
        Task<Message> SendMessageToOpposit();
        Task<FindUserResult> FindUserAsync(User user);
        Task GetOfflineMessage();
        Task SendDeliverAck(DateTime maxDateArrived);
    }

    public class MeService : IMeService
    {
        public MeService(Me me, IMessageService messageService, ILogger<MeService> logger, IUserService userService)
        {
            _me = me;
            _MessageService = messageService;
            _logger = logger;
            _UserService = userService;
        }

        public ILogger<MeService> _logger;
        public IMessageService _MessageService;
        public IUserService _UserService;
        public Me _me { get; set; }

        private User opposit => new User {Username = (_me.User.Username == "one") ? "two" : "one"};

        public Metadata headers
        {
            get
            {
                var h = new Metadata();
                h.Add("Authorization", $"Bearer {_me.Token}");
                h.Add("Totp", CalculateTotp());
                return h;
            }
        }


        public async Task<bool> Register()
        {
            var client = ServerConsts.UserClient;
            var request = new RegisterRequest {MacAddress = _me.Mac, Password = _me.Password, User = _me.User};
            var response = await client.RegisterAsync(request);
            return response.IsSuccess;
        }

        public async Task<bool> Login()
        {
            var client = ServerConsts.UserClient;
            var request = new LoginRequest
                {MacAddress = _me.Mac, Password = _me.Password, Username = _me.User.Username};
            var response = await client.LoginAsync(request);
            if (!response.IsSuccess) return response.IsSuccess;
            _me.Salt = response.Salt;
            _me.Token = response.Token;
            _me.Totp = response.Totp;
            _me.User = response.User;
            MessageStream();
            GetOfflineMessage();
            return response.IsSuccess;
        }

        public string CalculateTotp()
        {
            var secret = TotpHelper.CalculateSecret(_me.Mac, _me.Salt, _me.User.Id);
            var totp = TotpHelper.Generate(secret, _me.Salt);
            return totp;
        }

        public async void MessageStream()
        {
            var client = ServerConsts.MessengerClient;

            var streamingCall = client.GetMessageStream(new Empty(), headers: headers);

            try
            {
                await foreach (var messageData in streamingCall.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine($"{messageData.SenderId} | {messageData.Id} | {messageData.Payload} ");
                    await StoreMessages(new List<MessageLite>() {messageData.ToMessageLite()});
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("Stream cancelled.");
            }
        }

        public async Task<Message> SendMessage(Message message)
        {
            var client = ServerConsts.MessengerClient;

            var resp = await client.SendMessageAsync(message
                , headers: headers);

            return resp;
        }

        public async Task<Message> SendMessageToOpposit()
        {
            var oppositUser = await FindUserAsync(opposit);
            var msg = new Message
            {
                LocalId = Guid.NewGuid().ToString(),
                Payload = ByteString.CopyFrom("hiii", Encoding.UTF8),
                Type = MessageType.Text,
                ReceiverId = oppositUser.User.Id,
            };
            var resp = await SendMessage(msg);
            return resp;
        }

        public async Task<FindUserResult> FindUserAsync(User user)
        {
            var client = ServerConsts.UserClient;
            var resp = await client.GetUserInfoAsync(user);
            return resp;
        }

        public async Task GetOfflineMessage()
        {
            var client = ServerConsts.MessengerClient;
            var messages = await client.GetOfflineMessageStreamAsync(new Empty(), headers: headers);
            await StoreMessages(messages.Messages.ToMessageLiteList());
        }

        public async Task StoreMessages(List<MessageLite> messages)
        {
            await _MessageService.StoreMany(messages);

            var users = messages.Select(x => x.From).Distinct().ToList();
            await UpdateContactList(users);
            await SendDeliverAck(messages.Max(x => x.Created_at));
        }

        public async Task UpdateContactList(List<Guid> users)
        {
            var newUsers = _UserService.NewUsers(users);
            newUsers.ForEach(async u => await AddUserToContact(u));
        }

        public async Task AddUserToContact(Guid id)
        {
            var result = await FindUserAsync(new User {Id = id.ToString()});
            if (result.IsFound)
            {
                await _UserService.StoreAsync(result.User.ToUserLite());
            }
        }

        public async Task SendDeliverAck(DateTime maxDateArrived)
        {
            _logger.LogDebug($"Sending ack for max Date:{maxDateArrived}");
            var client = ServerConsts.MessengerClient;
            await client.CumulativeAckAsync(new MessageAck
            {
                AckType = AckType.Deliver,
                TimeStamp = Timestamp.FromDateTime(maxDateArrived)
            },headers: headers);
        }
    }
}