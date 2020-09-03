using System.Security.Cryptography;

namespace Moka.SharedKernel.Encryption
{
    public class RSAHandler
    {
        public static RSA GetKey(string owner)
        {
            //Generate a public/private key pair.  
            RSA rsa = RSA.Create();
            KeyStorage.WriteToFile(owner,rsa.ExportRSAPrivateKey(),KeyStorage.KeyTypes.AsymmetricPrivate);
            KeyStorage.WriteToFile(owner,rsa.ExportRSAPublicKey(),KeyStorage.KeyTypes.AsymmetricPublic);
            return rsa;
        }
    }
}