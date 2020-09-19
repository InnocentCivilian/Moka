using System;
using Moka.Server.Helper;
using Moka.SharedKernel.Encryption;
using Moka.SharedKernel.Tests.Randoms;
using Xunit;

namespace Moka.SharedKernel.Tests.Encryption
{
    public class RSAEncryptionTests
    {
        private IAsymmetricEncryption _sender;
        private IAsymmetricEncryption _receiver;
        private IAsymmetricEncryption _third;
        private string senderName = "sender";
        private string receiverName = "receiver";
        private string thirdName = "third";

        public RSAEncryptionTests()
        {
            _sender = new RSAEncryption(senderName);
            _receiver = new RSAEncryption(receiverName);
            _third = new RSAEncryption(thirdName);
        }

        // [Fact]
        // public void CompareHast_FreshGeneratedAndStoredKey_Equal()
        // {
        //     var uname = SecurityHelper.RandomString(6);
        //     var key1 = RSAHandler.GetOrGenerateKeyPair(uname);
        //     var key2 = RSAHandler.GetOrGenerateKeyPair(uname);
        //     var hash1 = RSAHandler.GetAsymPrivateHash(key1);
        //     var hash2 = RSAHandler.GetAsymPrivateHash(key2);
        //     Assert.Equal(hash1,hash2);
        // }
        // [Fact]
        // public void CompareHast_TwoDiffrentUsers_NotEqual()
        // {
        //     var uname1 = SecurityHelper.RandomString(6);
        //     var uname2 = SecurityHelper.RandomString(6);
        //     var key1 = RSAHandler.GetOrGenerateKeyPair(uname1);
        //     var key2 = RSAHandler.GetOrGenerateKeyPair(uname2);
        //     var hash1 = RSAHandler.GetAsymPrivateHash(key1);
        //     var hash2 = RSAHandler.GetAsymPrivateHash(key2);
        //     Assert.NotEqual(hash1,hash2);
        // }
        [Fact]
        public void SameKey_EncryptSameData_DifferentCipher()
        {
            var plain = new RandomBufferGenerator(500).GenerateBufferFromSeed(200);
            var rec = SecurityHelper.RandomString(6);
            var keyres = new RSAEncryption(senderName).GenerateKey();
            var cip1 = _sender.EncryptWithPublic(plain, keyres.Public);
            var cip2 = _sender.EncryptWithPublic(plain, keyres.Public);
            Assert.NotEqual(cip1, cip2);
        }

        [Fact]
        public void SendAndRecieve()
        {
            var plain = new RandomBufferGenerator(500).GenerateBufferFromSeed(200);

            var cipher = _sender.EncryptWithPublic(plain, _receiver.GetPublicKey());

            var decrypted = _receiver.DecryptWithPrivate(cipher);
            
            Assert.Equal(plain,decrypted);
            
            // make and verify sign
            var sign = _sender.Sign(plain);
            
            Assert.True(_receiver.ValidateSign(plain,sign,_sender.GetPublicKey()));
            //just to make sure
            Assert.False(_receiver.ValidateSign(plain, sign, _receiver.GetPublicKey()));
                
            Assert.False(_receiver.ValidateSign(plain,sign,_third.GetPublicKey()));
            
            Assert.True(_sender.ValidateSign(plain,sign,_sender.GetPublicKey()));
        }
        ///////////////////////////////////////////////STORAGE KEYS//////////////////////////////////////////////////////
        [Fact]
        public void StorageTests()
        {
            var secondLoad = new RSAEncryption(senderName);
            Assert.Equal(_sender.GetPrivateKey().Private.ToString(),secondLoad.GetPrivateKey().Private.ToString());
            Assert.Throws<Exception>(() => _sender.GetKey(thirdName));
            _sender.AddToKeyRing(_third.GetPublicKey(),thirdName);
            
            var key = _sender.GetKey(thirdName);
            
            Assert.False(key.IsPrivate);
        }
    }
}