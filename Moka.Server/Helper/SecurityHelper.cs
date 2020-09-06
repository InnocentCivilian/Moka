using System;
using System.Linq;
using System.Security;

namespace Moka.Server.Helper
{
    public class SecurityHelper
    {
        private static Random random = new Random();
        public static string RandomString(int length = 32)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}