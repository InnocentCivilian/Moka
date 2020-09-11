using Moka.SharedKernel.Security;

namespace Moka.SharedKernel.Encryption
{
    public interface IAsymmetricEncryption
    {
        //Operations
        public byte[] Encrypt(byte[] data, byte[] receiverPublickKey);
        public byte[] Sign(byte[] data);
        public bool ValidateSign(byte[] data);
        public byte[] MakeHash(byte[] data);
    }
}