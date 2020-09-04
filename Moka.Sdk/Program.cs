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

        private static async Task<string> Authenticate()
        {
            Console.WriteLine($"Authenticating as {username}...");
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{Address}/generateJwtToken?name={HttpUtility.UrlEncode(username)}"),
                Method = HttpMethod.Get,
                Version = new Version(2, 0)
            };
            var tokenResponse = await httpClient.SendAsync(request);
            tokenResponse.EnsureSuccessStatusCode();

            var token = await tokenResponse.Content.ReadAsStringAsync();
            Console.WriteLine("Successfully authenticated.");

            return token;
        }

        static async Task Main(string[] args)
        {
            string? token = null;
            var ar = args.FirstOrDefault();
            ar = ar.Split("=").Last();
            username = "hahaha" + ar;
            using var channel = GrpcChannel.ForAddress(Address);
            var client = new MokaMessenger.MokaMessengerClient(channel);
            token = await Authenticate();
            if (token != null)
            {
                headers = new Metadata();
                // headers.Add("Authorization", $"Bearer {token}");
            }

            // var metadata = new Metadata
            // {
            //     // { "Authorization", "hahaha"+ar }
            //     {
            //         "Authorization",
            //         "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiaGFoYWhhdHdvIiwiZXhwIjoxNjA0NDEwNTcxLCJpc3MiOiJFeGFtcGxlU2VydmVyIiwiYXVkIjoiRXhhbXBsZUNsaWVudHMifQ.FcRUHwq_Pgl-JtvLCh5WdtPKwWgx_sh8pd82ssXJF8E"
            //     }
            // };
            // CallOptions callOptions = new CallOptions(metadata);

            // var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            // using var streamingCall = client.GetMessageStream(new Empty(), headers:metadata,cancellationToken: cts.Token);
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