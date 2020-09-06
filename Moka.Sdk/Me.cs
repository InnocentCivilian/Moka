using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Moka.Sdk.Helper;
using Moka.SharedKernel.Security;

namespace Moka.Sdk
{
    public class Me
    {
        public Me(User user, string password)
        {
            User = user;
            Password = password;
        }

        public User User { get; set; }

        public string Token { get; set; }
        public string Password { get; set; }

        public string Mac => MacAddress.GetDefaultMacAddress();

        public string Salt { get; set; }
        public string Totp { get; set; }

        private User opposit => new User{Username = (User.Username == "one") ? "two" : "one"};

        public Metadata headers
        {
            get
            {
                var h = new Metadata();
                h.Add("Authorization", $"Bearer {Token}");
                h.Add("Totp", CalculateTotp());
                return h;
            }
        }

        // public  async Task<string> Login()
        // {
        //     Console.WriteLine($"Authenticating as {user.Username}...");
        //     var httpClient = new HttpClient();
        //     var request = new HttpRequestMessage
        //     {
        //         RequestUri = new Uri($"{ServerConsts.Address}/api/users/generateJwtToken?name={HttpUtility.UrlEncode(username)}"),
        //         Method = HttpMethod.Get,
        //         Version = new Version(2, 0)
        //     };
        //     var tokenResponse = await httpClient.SendAsync(request);
        //     tokenResponse.EnsureSuccessStatusCode();
        //
        //     var token = await tokenResponse.Content.ReadAsStringAsync();
        //     Console.WriteLine("Successfully authenticated.");
        //
        //     return token;
        // }

        public async Task<bool> Register()
        {
            var client = ServerConsts.UserClient;
            var request = new RegisterRequest {MacAddress = Mac, Password = Password, User = User};
            var response = await client.RegisterAsync(request);
            return response.IsSuccess;
        }

        public async Task<bool> Login()
        {
            var client = ServerConsts.UserClient;
            var request = new LoginRequest {MacAddress = Mac, Password = Password, Username = User.Username};
            var response = await client.LoginAsync(request);
            if (!response.IsSuccess) return response.IsSuccess;
            this.Salt = response.Salt;
            this.Token = response.Token;
            this.Totp = response.Totp;
            this.User = response.User;
            MessageStream();
            return response.IsSuccess;
        }

        public string CalculateTotp()
        {
            var secret = TotpHelper.CalculateSecret(Mac, Salt, User.Id);
            var totp = TotpHelper.Generate(secret, Salt);
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
    }
}