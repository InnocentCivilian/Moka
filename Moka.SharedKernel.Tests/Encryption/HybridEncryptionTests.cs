using System.Text;
using Moka.SharedKernel.Encryption;
using Xunit;

namespace Moka.SharedKernel.Tests.Encryption
{
    public class HybridEncryptionTests
    {
        private IHybridEncryption _sender;
        private IAsymmetricEncryption _senderAs;

        private IHybridEncryption _receiver;
        private IAsymmetricEncryption _receiverAs;

        private IHybridEncryption _third;
        private IAsymmetricEncryption _thirdAs;


        private string senderName = "sender";
        private string receiverName = "receiver";
        private string thirdName = "third";

        public HybridEncryptionTests()
        {
            _senderAs = new RSAEncryption(senderName);
            _sender = new HybridEncryption(senderName);

            _receiver = new HybridEncryption(receiverName);
            _receiverAs = new RSAEncryption(receiverName);

            _third = new HybridEncryption(thirdName);
            _thirdAs = new RSAEncryption(thirdName);
        }

        [Fact]
        public void SendAndRecieve()
        {
            var data = Encoding.UTF8.GetBytes("out diry secret");
            var encrypyed = _sender.EncryptData(data, _receiverAs.GetPublicKey());

            var decryped =
                _receiver.DecryptData(encrypyed.Cipher, encrypyed.key, encrypyed.Sign, _senderAs.GetPublicKey());
            
            Assert.NotEmpty(encrypyed.key);
            Assert.NotEmpty(encrypyed.Cipher);
            Assert.NotEmpty(encrypyed.Sign);
            
            Assert.True(decryped.IsSuccess);
            Assert.True(decryped.IsSignValid);
            Assert.NotEmpty(decryped.data);
            
            Assert.Equal(data,decryped.data);
        }
    }
}