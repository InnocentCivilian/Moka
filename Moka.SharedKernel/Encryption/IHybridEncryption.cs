using Org.BouncyCastle.Crypto;

namespace Moka.SharedKernel.Encryption
{
    public interface IHybridEncryption
    {
        public HybridEncryptionResult EncryptData(byte[] plain, AsymmetricKeyParameter receiverPublicKey);
        public HybridDecryptionResult DecryptData(byte[] cipher,byte[] keyBox,byte[] sign,AsymmetricKeyParameter senderPublicKey);
    }

    public class HybridDecryptionResult
    {
        public bool IsSuccess;
        public bool IsSignValid;
        public byte[] data;
        public override string ToString()
        {
            return base.ToString();
        }
    }

    public class HybridEncryptionResult
    {
        public byte[] Cipher;
        public byte[] key;
        // public byte[] IV;
        public byte[] Sign;
    }
}