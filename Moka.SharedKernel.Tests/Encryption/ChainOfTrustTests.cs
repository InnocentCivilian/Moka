// /**
// Hooman Hamidpour ( hhamidpour7@gmail.com )
//  * Date: 202205097:21 PM
//  */

using System;
using System.Text;
using Moka.SharedKernel.Encryption;
using Org.BouncyCastle.Crypto;
using Xunit;
using Xunit.Abstractions;

namespace Moka.SharedKernel.Tests.Encryption
{
    public class ChainOfTrustTests
    {
        private ChainOfTrust _rootchainOfTrust;
        private HybridEncryption root;
        private readonly ITestOutputHelper output;
        private readonly AsymmetricKeyParameter _rootPublic;
        private readonly SignKeyParameters _rootParams;

        public ChainOfTrustTests(ITestOutputHelper output)
        {
            this.output = output;
            root = new HybridEncryption(nameof(root));
            _rootchainOfTrust = new ChainOfTrust(root);
            _rootPublic = root.Asymmetric.GetPublicKey();
            _rootParams = new SignKeyParameters
            {
                CanIssue = true,
                ExpireAt = new DateTime(2030, 1, 1)
            };
        }

        [Fact]
        public void KeyParamJsonStringifyAlwaysRemainSame()
        {
            var firstJson = _rootchainOfTrust.KeyParametersPair(_rootPublic, _rootParams);
            var secondJson = _rootchainOfTrust.KeyParametersPair(_rootPublic, _rootParams);
            Assert.Equal(firstJson, secondJson);
        }

        [Fact]
        public void KeyGivenSignKeyNotEmpty()
        {
            var rootSign = _rootchainOfTrust.SignHash(_rootPublic, _rootParams);
            Assert.NotEmpty(rootSign);
        }

        [Fact]
        public void KeyGivenSignReturned()
        {
            var rootSign = _rootchainOfTrust.SignHash(_rootPublic, _rootParams);
            var signedKeyObject = _rootchainOfTrust.SignedKeyParametersPair(_rootPublic, _rootParams, rootSign);
            output.WriteLine(signedKeyObject.ToJson());
            Assert.NotEmpty(rootSign);
            Assert.NotEmpty(signedKeyObject.ToJson());
        }

        [Fact]
        public void RootSelfSign_KeyGiven_SelfSigns_ValidatePass()
        {
            var rootSign = _rootchainOfTrust.SignHash(_rootPublic, _rootParams);
            var objfirst = _rootchainOfTrust.SignedKeyParametersPair(_rootPublic, _rootParams, rootSign);
            
            var objsecond = SignedKeyObject.FromJson(objfirst.ToJson());
            
            Assert.Equal(objfirst.Hash, objsecond.Hash);
            Assert.Equal(objfirst.Payload, objsecond.Payload);
            Assert.Equal(objfirst.Sign, objsecond.Sign);
            
            var reByteSign = Convert.FromBase64String(objfirst.Sign);
            
            Assert.Equal(rootSign, reByteSign);//fail

            // var validateSign = _chainOfTrust.Validate(obj.Hash, obj.Sign,_rootPublic);
            // Assert.True(validateSign);
        }

        [Fact]
        public void RootSelfSign_SignBytes_ValidatePass()
        {
            var toSign = _rootchainOfTrust.KeyParametersPair(_rootPublic, _rootParams);
            var toSignBytes = Encoding.UTF8.GetBytes(toSign);
            var hash = _rootchainOfTrust.ComputeSha256Hash(toSignBytes);
            // output.WriteLine(hash);
            var signed = _rootchainOfTrust.SignBytes(hash);
            var validate = _rootchainOfTrust.Validate(hash, signed);
            Assert.True(validate);
        }
        
    }
}