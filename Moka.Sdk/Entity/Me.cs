using Moka.Sdk.Helper;

namespace Moka.Sdk.Entity
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
    }
}