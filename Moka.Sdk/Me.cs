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
using Moka.Sdk.Helper;
using Moka.Sdk.SqlLite;
using Moka.SharedKernel.Security;

namespace Moka.Sdk
{
    public interface IMe
    {
        User User { get; set; }
        string Token { get; set; }
        string Password { get; set; }
        string Mac { get; }
        string Salt { get; set; }
        string Totp { get; set; }
        Metadata headers { get; }
        Task<bool> Register();
        Task<bool> Login();
        string CalculateTotp();
        void MessageStream();
        Task<Message> SendMessage(Message message);
        Task<Message> SendMessageToOpposit();
        Task<FindUserResult> FindUser(User user);
        void RunDb();
    }

    public class Me : IMe
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

        private User opposit => new User {Username = (User.Username == "one") ? "two" : "one"};

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

        public void RunDb()
        {
            // Tipp: SQLite Cipher databases can be created/explored manually using https://sqlitebrowser.org/ (or on GitHub https://github.com/sqlitebrowser/sqlitebrowser)
            const string newDatabaseFile = EnvConsts.DEFAULTDBFILE;


            // Initialize the database context (ensure the database is created, if it's a new database)
            using var db = new MokaClientContext(newDatabaseFile);

            db.Database.EnsureCreated();

            // Add a blog URI
            // db.Blogs.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
            db.Messages.Add(new MessageLite
            {
                Created_at = DateTime.Now,
                Data = new byte[] { },
                Delivered_at = DateTime.Now,
                From = Guid.Empty,
                To = Guid.Empty,
                LocalId = Guid.Empty,
                MessageType = MessageType.Text
            });
            // db.Users.Add(
            //     new User
            //     {
            //         Id = "123",
            //         Nickname = "hahaha",
            //         Username = "fuckme"
            //     });
            Console.WriteLine("{0} records saved to database", db.SaveChanges());
            Console.WriteLine("{0} records are in message table", db.Messages.Count());
            foreach (var messageLite in db.Messages)
            {
                Console.WriteLine(messageLite.Created_at);
            }

            //
            // // Display all blog URIs from the current database
            // Console.WriteLine("All {0} blogs in database:", db.Blogs.Count());
            // foreach (var blog in db.Blogs) Console.WriteLine(" - #{0} {1}", blog.BlogId, blog.Url);
        }
    }
}