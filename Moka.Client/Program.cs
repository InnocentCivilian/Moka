using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new MokaMessenger.MokaMessengerClient(channel);
            var metadata = new Metadata
            {
                { "Authorization", "hahaha" }
            };
            CallOptions callOptions = new CallOptions(metadata);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            using var streamingCall = client.GetWeatherStream(new Empty(), headers:metadata,cancellationToken: cts.Token);

            try
            {
                await foreach (var weatherData in streamingCall.ResponseStream.ReadAllAsync(cancellationToken: cts.Token))
                {
                    Console.WriteLine($"{weatherData.DateTimeStamp.ToDateTime():s} | {weatherData.Summary} | {weatherData.TemperatureC} C");
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {               
                Console.WriteLine("Stream cancelled.");
            }
        }
    }
}