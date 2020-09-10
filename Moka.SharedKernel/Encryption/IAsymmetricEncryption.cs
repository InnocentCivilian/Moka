namespace Moka.SharedKernel.Encryption
{
    public interface IAsymmetricEncryption
    {
        public byte[] Encrypt(byte[] data, byte[] receiverPublickKey);
        public byte[] Sign(byte[] data);
        public bool ValidateSign(byte[] data);
        
        public byte[] MakeHash(byte[] data);
    }
}