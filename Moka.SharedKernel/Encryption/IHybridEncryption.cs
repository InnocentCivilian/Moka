namespace Moka.SharedKernel.Encryption
{
    public interface IHybridEncryption
    {
        public HybridEncryptionResult EncryptData(byte[] plain, byte[] receiverPublicKey);
        public HybridDecryptionResult Decrypt(byte[] cipher,byte[] keyBox);
    }

    public class HybridDecryptionResult
    {
        public bool IsSuccess;
        public bool IsSignValid;
        public byte[] data;
    }

    public class HybridEncryptionResult
    {
        public byte[] Cipher;
        public byte[] key;
        public byte[] IV;
        public byte[] Sign;
    }
}