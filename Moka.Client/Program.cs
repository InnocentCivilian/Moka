using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;

namespace Moka.Client
{
    class Program
    {
        public static async Task greet()
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new MokaMessenger.MokaMessengerClient(channel);
            
            var response = await client.RegisterAsync(
                new RegisterRequest{Password = "1234",User = new User{Id=Guid.Empty.ToString(),Nickname = "hahaha",Username = "hooman grpc"}});
            
            Console.WriteLine(response.Id);
        }
        static async Task Main(string[] args)
        {
            var ar =args.FirstOrDefault();
            ar = ar.Split("=").Last();
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new MokaMessenger.MokaMessengerClient(channel);
            var metadata = new Metadata
            {
                { "Authorization", "hahaha"+ar }
            };
            CallOptions callOptions = new CallOptions(metadata);

            // var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            // using var streamingCall = client.GetMessageStream(new Empty(), headers:metadata,cancellationToken: cts.Token);
            var streamingCall = client.GetMessageStream(new Empty(), headers:metadata);
            if (ar == "one")
            {
                await Task.Delay(1000).ContinueWith(t =>
                {
                    var resp =  client.SendMessage(new SendMessageRequest{Payload = ByteString.CopyFrom("hiii",Encoding.UTF8),ReceiverId = "hahahatwo",Type = MessageType.Text},headers:metadata);
                    Console.WriteLine(resp.MessageId);
                    Console.WriteLine(resp.CreateDateTime);
                });
            }
            try
            {
                await foreach (var messageData in streamingCall.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine($"{messageData.SenderId} | {messageData.MessageId} | {messageData.Payload} ");
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {               
                Console.WriteLine("Stream cancelled.");
            }

            if (ar == "one")
            {
                await Task.Delay(1000).ContinueWith(t =>
                {
                   var resp =  client.SendMessage(new SendMessageRequest{Payload = ByteString.CopyFrom("hiii",Encoding.UTF8),ReceiverId = "hahahatwo",Type = MessageType.Text},headers:metadata);
                   Console.WriteLine(resp.MessageId);
                   Console.WriteLine(resp.CreateDateTime);
                });
            }

            Console.ReadKey();
        }
    }
}