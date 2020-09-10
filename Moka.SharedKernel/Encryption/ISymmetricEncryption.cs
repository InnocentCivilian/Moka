namespace Moka.SharedKernel.Encryption
{
    public interface ISymmetricEncryption
    {
        public byte[] GenerateSecretKey();
        public byte[] GenerateIV();
        public byte[] Encrypt(byte[] plain, byte[] secret, byte[] IV);
        public byte[] Decrypt(byte[] cipher, byte[] secret, byte[] IV);
        
    }
}