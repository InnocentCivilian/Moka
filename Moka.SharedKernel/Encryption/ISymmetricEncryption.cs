namespace Moka.SharedKernel.Encryption
{
    public interface ISymmetricEncryption
    {
        public const int KEYSIZE = 128;

        public byte[] GenerateSecretKey();
        public byte[] Encrypt(byte[] plain, byte[] secret);
        public byte[] Decrypt(byte[] cipher, byte[] secret);
        
    }
}