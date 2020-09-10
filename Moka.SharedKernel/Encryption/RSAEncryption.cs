using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Moka.SharedKernel.Encryption
{
    public class RSAEncryption : IAsymmetricEncryption
    {
        public RSAEncryption(string owner)
        {
            Owner = owner;
            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(); // cspParams);
            var key = RSAHandler.GetOrGenerateKeyPair(owner);
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters) key.Private);
            csp.ImportParameters(rsaParams);
            Console.WriteLine(csp.PublicOnly);
        }

        private RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
        public string Owner { get; set; }
        public byte[] Encrypt(byte[] data, byte[] receiverPublickKey)
        {
            throw new NotImplementedException();
        }

        public byte[] Sign(byte[] data)
        {
            throw new NotImplementedException();
        }

        public bool ValidateSign(byte[] data)
        {
            throw new NotImplementedException();
        }

        public byte[] MakeHash(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}