// /**
// Hooman Hamidpour ( hhamidpour7@gmail.com )
//  * Date: 202205137:04 PM
//  */

using System;
using Moka.SharedKernel.Encryption;
using Org.BouncyCastle.Crypto;
using Xunit;
using Xunit.Abstractions;

namespace Moka.SharedKernel.Tests.Encryption
{
    public class ChainOfTrustValidateTests
    {
        private ChainOfTrust _rootchainOfTrust;
        private HybridEncryption root;
        private readonly ITestOutputHelper output;
        private readonly AsymmetricKeyParameter _rootPublic;
        private readonly SignKeyParameters _rootParams;

        public ChainOfTrustValidateTests(ITestOutputHelper output)
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
        public void RootUserCanValidateTheirOwnSign()
        {
            var selfSignResult = _rootchainOfTrust.GenerateRootSign(_rootParams);
            var validate = _rootchainOfTrust.Validate(selfSignResult);
            Assert.True(validate);
        }

        [Fact]
        public void RootUserCanValidateSomeSign()
        {
            var selfSignResult = _rootchainOfTrust.GenerateRootSign(_rootParams);
            var valid = _rootchainOfTrust.Validate(selfSignResult, selfSignResult.KeyParam().ObjectifyKey());
            Assert.True(valid);

        }
        
    }
}