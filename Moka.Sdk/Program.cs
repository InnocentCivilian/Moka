using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Moka.Sdk.Helper;
using Moka.SharedKernel.Encryption;

namespace Moka.Sdk
{
    class Program
    {
        private const string Address = "https://localhost:5001";
        private static string username = "";
        static Metadata? headers = null;
        static string? token = null;
        private static string password = "1234";

        static async Task Main(string[] args)
        {
            var ar = args.FirstOrDefault();
            ar = ar.Split("=").Last();
            var me = new Me(
                new User
                {
                    Id = Guid.Empty.ToString(),
                    Nickname = ar + "name",
                    Username = ar
                },
                password
            );
            Console.WriteLine("gRPC MOKA CLI Client");
            Console.WriteLine();
            Console.WriteLine("Press a key:");
            Console.WriteLine("1: Register");
            Console.WriteLine("2: Login");
            Console.WriteLine("3: Login(FAIL)");
            Console.WriteLine("5: TOTP");
            // Console.WriteLine("3: Authenticate");
            Console.WriteLine("4: Exit");
            Console.WriteLine();
            var exiting = false;
            while (!exiting)
            {
                var consoleKeyInfo = Console.ReadKey(intercept: true);
                switch (consoleKeyInfo.KeyChar)
                {
                    case '1':
                        var registerResp = await me.Register();
                        Console.WriteLine("Register:" + registerResp);
                        break;
                    case '2':
                        me.Password = password;
                        var loginResponse = await me.Login();
                        Console.WriteLine("Login:" + loginResponse);
                        break;
                    case '3':
                        me.Password = "failPASSWORDwrong";
                        var failLoginResponse = await me.Login();
                        Console.WriteLine("Login:" + failLoginResponse);
                        break;
                    case '4':
                        exiting = true;
                        break;
                    case '5':
                        var totp = me.CalculateTotp();
                        Console.WriteLine("Totp:" + totp);
                        break;
                }
            }

            Console.WriteLine("Exiting");
            return;
            // var ar = args.FirstOrDefault();
            // ar = ar.Split("=").Last();
            // username = "hahaha" + ar;
            using var channel = GrpcChannel.ForAddress(Address);
            var client = new MokaMessenger.MokaMessengerClient(channel);
            // token = await Authenticate();
            if (token != null)
            {
                headers = new Metadata();
                headers.Add("Authorization", $"Bearer {token}");
            }

            var streamingCall = client.GetMessageStream(new Empty(), headers: headers);
            if (ar == "one")
            {
                while (true)
                {
                    await Task.Delay(1000).ContinueWith(t =>
                    {
                        var resp = client.SendMessage(
                            new Message
                            {
                                LocalId = Guid.NewGuid().ToString(),
                                Payload = ByteString.CopyFrom("hiii", Encoding.UTF8), ReceiverId = "hahahatwo",
                                Type = MessageType.Text
                            }, headers: headers);
                        Console.WriteLine(resp.Id);
                        Console.WriteLine(resp.CreatedAtTimeStamp);
                        Console.WriteLine(resp.DeliveredAtTimeStamp);
                    });
                }
            }

            // if (ar == "two")
            // {
            //     var resp = await client.GetOfflineMessageStreamAsync(new Empty(), headers: metadata);
            //     var msgs = resp.Messages.Select(x => x).ToList();
            //     var delv = msgs.Max(x => x.CreatedAtTimeStamp);
            //     var delvresp = await client.CumulativeAckAsync(new MessageAck {AckType = AckType.Deliver,TimeStamp = delv}, headers: metadata);
            //
            // }
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

            Console.ReadKey();
        }
    }
}