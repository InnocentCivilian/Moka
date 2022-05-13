// /**
// Hooman Hamidpour ( hhamidpour7@gmail.com )
//  * Date: 202205137:04 PM
//  */

using System;
using System.Linq;
using Moka.SharedKernel.Encryption;
using Org.BouncyCastle.Crypto;
using Xunit;
using Xunit.Abstractions;

namespace Moka.SharedKernel.Tests.Encryption
{
    public class ChainOfTrustValidateTests
    {
        private HybridEncryption root;
        private HybridEncryption ross;
        private HybridEncryption rachel;
        private HybridEncryption monica;
        private HybridEncryption chandler;
        private HybridEncryption phoebe;
        private HybridEncryption janice;
        private readonly ITestOutputHelper _output;
        private readonly SignKeyParameters _issuerParams;
        private readonly SignKeyParameters _nonissuerParams;
        private readonly SignKeyParameters _userissuerParams;
        private readonly SignKeyParameters _expiredParams;
        private ChainOfTrust _rootchainOfTrust;
        private ChainOfTrust _rosschainOfTrust;
        private ChainOfTrust _rachelchainOfTrust;
        private ChainOfTrust _monicachainOfTrust;
        private ChainOfTrust _chandlerchainOfTrust;
        private ChainOfTrust _phoebechainOfTrust;
        private ChainOfTrust _janicechainOfTrust;


        public ChainOfTrustValidateTests(ITestOutputHelper output)
        {
            _output = output;
            root = new HybridEncryption(nameof(root));
            ross = new HybridEncryption(nameof(ross));
            rachel = new HybridEncryption(nameof(rachel));
            monica = new HybridEncryption(nameof(monica));
            chandler = new HybridEncryption(nameof(chandler));
            phoebe = new HybridEncryption(nameof(phoebe));
            janice = new HybridEncryption(nameof(janice));
            _rootchainOfTrust = new ChainOfTrust(root);
            _rosschainOfTrust = new ChainOfTrust(ross);
            _rachelchainOfTrust = new ChainOfTrust(rachel);
            _monicachainOfTrust = new ChainOfTrust(monica);
            _chandlerchainOfTrust = new ChainOfTrust(chandler);
            _phoebechainOfTrust = new ChainOfTrust(phoebe);
            _janicechainOfTrust = new ChainOfTrust(janice);
            // _rootPublic = root.Asymmetric.GetPublicKey();
            _issuerParams = new SignKeyParameters
            {
                CanIssue = true,
                ExpireAt = new DateTime(2030, 1, 1)
            };
            _userissuerParams = new SignKeyParameters
            {
                CanIssue = true,
                ExpireAt = new DateTime(2024, 1, 1)
            };
            _expiredParams = new SignKeyParameters
            {
                CanIssue = true,
                ExpireAt = new DateTime(2010, 1, 1)
            };
            _nonissuerParams = new SignKeyParameters
            {
                CanIssue = false,
                ExpireAt = new DateTime(2030, 1, 1)
            };
            MakeChains();
        }

        public void MakeChains()
        {
            var rootSelfSignResult = _rootchainOfTrust.GenerateRootSign(_issuerParams);
            _rootchainOfTrust.AddToTrusted(rootSelfSignResult);
            _rootchainOfTrust.SetOwnedKey(rootSelfSignResult);
        }

        [Fact]
        public void RootUserCanValidateTheirOwnSign()
        {
            var selfSignResult = _rootchainOfTrust.GenerateRootSign(_issuerParams);
            var validate = _rootchainOfTrust.Validate(selfSignResult);
            Assert.True(validate);
        }

        [Fact]
        public void RootUserCanValidateSomeSign()
        {
            var selfSignResult = _rootchainOfTrust.GenerateRootSign(_issuerParams);
            var valid = _rootchainOfTrust.Validate(selfSignResult, selfSignResult.KeyParam().ObjectifyKey());
            Assert.True(valid);
        }

        [Fact]
        public void RootChainGiven_RootAcceptsRootChain()
        {
            var a = _rootchainOfTrust.GetTrustedRoots().First().key ==
                    _rootchainOfTrust.GetChain().First().KeyParam().key;
            var isValid = _rootchainOfTrust.IsValidChain(_rootchainOfTrust.GetChain());
            Assert.True(isValid);
        }
    }
}