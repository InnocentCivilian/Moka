using Moka.Server.Helper;
using Moka.SharedKernel.Encryption;
using Xunit;

namespace Moka.SharedKernel.Tests.Encryption
{
    public class RSAEncryptionTests
    {
        [Fact]
        public void CompareHast_FreshGeneratedAndStoredKey_Equal()
        {
            var uname = SecurityHelper.RandomString(6);
            var key1 = RSAHandler.GetOrGenerateKeyPair(uname);
            var key2 = RSAHandler.GetOrGenerateKeyPair(uname);
            var hash1 = RSAHandler.GetAsymPrivateHash(key1);
            var hash2 = RSAHandler.GetAsymPrivateHash(key2);
            Assert.Equal(hash1,hash2);
        }
        [Fact]
        public void CompareHast_TwoDiffrentUsers_NotEqual()
        {
            var uname1 = SecurityHelper.RandomString(6);
            var uname2 = SecurityHelper.RandomString(6);
            var key1 = RSAHandler.GetOrGenerateKeyPair(uname1);
            var key2 = RSAHandler.GetOrGenerateKeyPair(uname2);
            var hash1 = RSAHandler.GetAsymPrivateHash(key1);
            var hash2 = RSAHandler.GetAsymPrivateHash(key2);
            Assert.NotEqual(hash1,hash2);
        }
    }
}