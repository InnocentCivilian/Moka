using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Grpc.Net.Client;
using Moka.Sdk.Helper;

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
            var request = new RegisterRequest{MacAddress = Mac,Password = Password,User = User};
            var response =  await client.RegisterAsync(request);
            return response.IsSuccess;
        }

    }
}