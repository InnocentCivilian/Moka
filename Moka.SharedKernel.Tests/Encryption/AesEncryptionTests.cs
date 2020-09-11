using Moka.SharedKernel.Encryption;
using Moka.SharedKernel.Tests.Randoms;
using Xunit;

namespace Moka.SharedKernel.Tests.Encryption
{
    public class AesEncryptionTests
    {
        private ISymmetricEncryption _aes;

        public AesEncryptionTests()
        {
            _aes = new AesEncryption();
        }

        [Fact]
        public void GenerateKey_NothingGiven_ReturnKey()
        {
            var key = _aes.GenerateSecretKey();
            Assert.Equal(16, key.Length);
        }

        [Fact]
        public void GenerateMultiKey_NothingGiven_ReturnDiffrentKey()
        {
            var key1 = _aes.GenerateSecretKey();
            var key2 = _aes.GenerateSecretKey();
            Assert.NotEqual(key1, key2);
        }

        [Fact]
        void EncryptData_KeyGiven_EncryptSuccess()
        {
            var key = _aes.GenerateSecretKey();
            var data = new RandomBufferGenerator(1024).GenerateBufferFromSeed(1000);
            var cipher = _aes.Encrypt(data, key);
            Assert.NotEqual(data,cipher);
        }
        [Fact]
        void EncryptAndDecryptData_KeyGiven_GetInitialData()
        {
            var key = _aes.GenerateSecretKey();
            var data = new RandomBufferGenerator(1024).GenerateBufferFromSeed(1000);
            var cipher = _aes.Encrypt(data, key);
            var decrypted = _aes.Decrypt(cipher, key);
            Assert.Equal(data,decrypted);
            Assert.NotEqual(data,cipher);
            Assert.NotEqual(cipher,decrypted);
            Assert.NotEmpty(decrypted);
        }
    }
}