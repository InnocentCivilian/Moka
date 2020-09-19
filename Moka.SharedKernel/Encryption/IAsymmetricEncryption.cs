using Moka.SharedKernel.Security;
using Org.BouncyCastle.Crypto;

namespace Moka.SharedKernel.Encryption
{
    public interface IAsymmetricEncryption
    {
        //Operations
        public byte[] EncryptWithPublic(byte[] data, AsymmetricKeyParameter receiverPublickKey);
        public byte[] DecryptWithPrivate(byte[] cipher);

        public byte[] Sign(byte[] data);
        public bool ValidateSign(byte[] data,byte[] sig,AsymmetricKeyParameter senderPublic);
        public byte[] MakeHash(byte[] data);
        public AsymmetricKeyParameter GetPublicKey();
        AsymmetricCipherKeyPair GetPrivateKey();
        public AsymmetricCipherKeyPair GenerateKey();
        public void AddToKeyRing(AsymmetricKeyParameter newKey,string owner);

        public AsymmetricKeyParameter GetKey(string owner);
    }
}