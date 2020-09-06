using System;

namespace Moka.Server.Models
{
    public class Device
    {
        public Device(string name, string macAddress, string totp, string salt, DateTime firstLogin, DateTime lastConnect)
        {
            Name = name;
            MacAddress = macAddress;
            Totp = totp;
            Salt = salt;
            FirstLogin = firstLogin;
            LastConnect = lastConnect;
        }

        public string Name { get; set; }
        public string MacAddress { get; set; }
        public string Totp { get; set; }
        public string Salt { get; set; }
        public DateTime FirstLogin { get; set; }
        public DateTime LastConnect { get; set; }
    }
}