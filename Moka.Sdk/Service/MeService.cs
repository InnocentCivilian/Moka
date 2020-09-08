using System;
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
        Task<FindUserResult> FindUser(User user);
        Task GetOfflineMessage();
    }

    public class MeService : IMeService
    {
        public MeService(Me me, IMessageService messageService, ILogger<MeService> logger)
        {
            _me = me;
            _MessageService = messageService;
            _logger = logger;
        }

        public ILogger<MeService> _logger;
        public IMessageService _MessageService;
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
            var request = new LoginRequest {MacAddress = _me.Mac, Password = _me.Password, Username = _me.User.Username};
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
            var oppositUser = await FindUser(opposit);
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

        public async Task<FindUserResult> FindUser(User user)
        {
            var client = ServerConsts.UserClient;
            var resp = await client.GetUserInfoAsync(user);
            return resp;
        }

        public async Task GetOfflineMessage()
        {
            var client = ServerConsts.MessengerClient;
            var messages = await client.GetOfflineMessageStreamAsync(new Empty(), headers: headers);
            await _MessageService.StoreMany(messages.Messages.ToMessageLiteList());
        }
    }
}