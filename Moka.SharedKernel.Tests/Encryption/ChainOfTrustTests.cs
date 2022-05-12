// /**
// Hooman Hamidpour ( hhamidpour7@gmail.com )
//  * Date: 202205097:21 PM
//  */

using System;
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
            output.WriteLine(firstJson);
            Assert.Equal(firstJson,secondJson);
        }

        // [Fact]
        // public void RootSelfSign_KeyGiven_SelfSigns()
        // {
        //     var publicKey = root.Asymmetric.GetPublicKey();
        //     var parameters = new SignKeyParameters
        //     {
        //         CanIssue = true,
        //         ExpireAt = new DateTime(2030, 1, 1)
        //     };
        //     var rootSign = _chainOfTrust.Sign(publicKey,parameters);
        //     Assert.True(_chainOfTrust.);
        //     output.WriteLine(Convert.ToBase64String(rootSign));
        // }
    }
}