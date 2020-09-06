using System;
using System.Security.Cryptography;
using System.Text;
using OtpNet;

namespace Moka.SharedKernel.Security
{
    public class TotpHelper
    {
        public static string Generate(byte[] secretKey, string salt)
        {
            var totp = new Totp(secretKey, mode: OtpHashMode.Sha512, step: 10);
            var totpCode = totp.ComputeTotp(DateTime.UtcNow);
            return ComputeSha256Hash(totpCode, salt);
        }

        private static string ComputeSha256Hash(string rawData, string salt)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData + salt));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public static byte[] CalculateSecret(string mac, string salt, string id)
        {
            // Console.WriteLine("mac"+mac);
            // Console.WriteLine("salt"+salt);
            // Console.WriteLine("id"+id);
            var secretKey = Encoding.ASCII.GetBytes(id + mac + salt);
            return secretKey;
        }

        public static bool Validate(byte[] secretKey, string input, string salt)
        {
            // var totp = new Totp(secretKey, mode: OtpHashMode.Sha512,step:10);
            // long timeMatch;
            // return totp.VerifyTotp(DateTime.UtcNow, input,out timeMatch);
            var original = Generate(secretKey, salt);
            Console.WriteLine("original"+original);
            Console.WriteLine("input"+input);
            return string.Equals(original, input, StringComparison.OrdinalIgnoreCase);
        }
    }
}