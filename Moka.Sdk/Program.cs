using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            // RSAHandler.GetKey("server");
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
                while (true)
                {
                    await Task.Delay(1000).ContinueWith(t =>
                    {
                        var resp =  client.SendMessage(new Message{LocalId = Guid.NewGuid().ToString(),Payload = ByteString.CopyFrom("hiii",Encoding.UTF8),ReceiverId = "hahahatwo",Type = MessageType.Text},headers:metadata);
                        Console.WriteLine(resp.Id);
                        Console.WriteLine(resp.CreatedAtTimeStamp);
                        Console.WriteLine(resp.DeliveredAtTimeStamp);
                    });
                }
                
            }

            if (ar == "two")
            {
                var resp = await client.GetOfflineMessageStreamAsync(new Empty(), headers: metadata);
                var msgs = resp.Messages.Select(x => x).ToList();
                var delv = msgs.Max(x => x.CreatedAtTimeStamp);
                var delvresp = await client.CumulativeAckAsync(new MessageAck {AckType = AckType.Deliver,TimeStamp = delv}, headers: metadata);

            }
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