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
        private ChainOfTrust _chainOfTrust;
        private HybridEncryption root;
        private readonly ITestOutputHelper output;
        private readonly AsymmetricKeyParameter _rootPublic;
        private readonly SignKeyParameters _rootParams;

        public ChainOfTrustTests(ITestOutputHelper output)
        {
            this.output = output;
            root = new HybridEncryption(nameof(root));
            _chainOfTrust = new ChainOfTrust(root);
            _rootPublic = root.Asymmetric.GetPublicKey();
            _rootParams =  new SignKeyParameters
            {
                CanIssue = true,
                ExpireAt = new DateTime(2030, 1, 1)
            };
            
        }

        [Fact]
        public void KeyParamJsonStringifyAlwaysRemainSame()
        {
            var firstJson = _chainOfTrust.KeyParametersPair(_rootPublic, _rootParams);
            var secondJson = _chainOfTrust.KeyParametersPair(_rootPublic, _rootParams);
            Assert.Equal(firstJson,secondJson);
        }

        [Fact]
        public void KeyGivenSignKeyNotEmpty()
        {
            var rootSign = _chainOfTrust.SignHash(_rootPublic, _rootParams);
            Assert.NotEmpty(rootSign);
        }
        [Fact]
        public void KeyGivenSignReturned()
        {

            var rootSign = _chainOfTrust.SignHash(_rootPublic, _rootParams);
            var json = _chainOfTrust.SignedKeyParametersPair(_rootPublic, _rootParams,rootSign);
            output.WriteLine(json);
            Assert.NotEmpty(rootSign);
            Assert.NotEmpty(json);
        }
        [Fact]
        public void RootSelfSign_KeyGiven_SelfSigns_ValidatePass()
        {
            var rootSign = _chainOfTrust.SignHash(_rootPublic, _rootParams);
            var json = _chainOfTrust.SignedKeyParametersPair(_rootPublic, _rootParams,rootSign);
            var firstJson = _chainOfTrust.KeyParametersPair(_rootPublic, _rootParams);

            var obj = _chainOfTrust.DeserializeSignedObject(json);
            Assert.Equal(obj.Payload,firstJson);
            Assert.NotEmpty(obj.Payload);
            Assert.NotEmpty(obj.Sign);
            var validateSign = _chainOfTrust.Validate(obj.Hash, obj.Sign,_rootPublic);
            Assert.True(validateSign);
        }

        [Fact]
        public void RootSelfSign_SignBytes_ValidatePass()
        {
            var toSign = _chainOfTrust.KeyParametersPair(_rootPublic, _rootParams);
            var toSignBytes = Encoding.UTF8.GetBytes(toSign);
            var hash = _chainOfTrust.ComputeSha256Hash(toSignBytes);
            // output.WriteLine(hash);
            var signed = _chainOfTrust.SignBytes(hash);
            var validate = _chainOfTrust.Validate(hash, signed);
            Assert.True(validate);

        }
    }
}