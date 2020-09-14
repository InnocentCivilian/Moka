using Org.BouncyCastle.Crypto;

namespace Moka.SharedKernel.Encryption
{
    public class HybridEncryption : IHybridEncryption
    {
        public IAsymmetricEncryption Asymmetric;
        public ISymmetricEncryption Symmetric;

        public HybridEncryption(ISymmetricEncryption symmetric, IAsymmetricEncryption asymmetric)
        {
            Symmetric = symmetric;
            Asymmetric = asymmetric;
        }

        public HybridEncryption(string owner)
        {
            Symmetric = new AesEncryption();
            Asymmetric = new RSAEncryption(owner);
        }


        public HybridEncryptionResult EncryptData(byte[] plain, AsymmetricKeyParameter receiverPublicKey)
        {
            byte[] secert = Symmetric.GenerateSecretKey();
            var cipher = Symmetric.Encrypt(plain, secert);
            byte[] ciphredKey = Asymmetric.EncryptWithPublic(secert, receiverPublicKey);
            byte[] sign = Asymmetric.Sign(plain);
            return new HybridEncryptionResult
            {
                Cipher = cipher,
                key = ciphredKey,
                Sign = sign
            };
        }

        public HybridDecryptionResult DecryptData(byte[] cipher, byte[] keyBox, byte[] sign,
            AsymmetricKeyParameter senderPublicKey)
        {
            var key = Asymmetric.DecryptWithPrivate(keyBox);
            var plain = Symmetric.Decrypt(cipher, key);
            var verified = Asymmetric.ValidateSign(plain, sign, senderPublicKey);
            return new HybridDecryptionResult
            {
                data = plain,
                IsSuccess = verified && plain.Length > 0,
                IsSignValid = verified
            };
        }
    }
}