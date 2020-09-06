using Grpc.Net.Client;

namespace Moka.Sdk
{
    public class ServerConsts
    {
        public const string Address = "https://localhost:5001";

        public  static MokaMessenger.MokaMessengerClient MessengerClient
        {
            get
            {
                 var channel = GrpcChannel.ForAddress(Address);
                return new MokaMessenger.MokaMessengerClient(channel);
            }
        }
        public static MokaUser.MokaUserClient UserClient
        {
            get
            {
                 var channel = GrpcChannel.ForAddress(Address);
                return new MokaUser.MokaUserClient(channel);
            }
        }
    }
}