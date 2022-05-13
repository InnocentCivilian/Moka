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
            //root
            var rootSelfSignResult = _rootchainOfTrust.GenerateRootSign(_issuerParams);
            _rootchainOfTrust.AddToTrusted(rootSelfSignResult);
            _rootchainOfTrust.SetOwnedKey(rootSelfSignResult);
            //janice
            var janiceSelfSignResult = _janicechainOfTrust.GenerateRootSign(_issuerParams);
            _janicechainOfTrust.AddToTrusted(janiceSelfSignResult);
            _janicechainOfTrust.SetOwnedKey(janiceSelfSignResult);
            //ross : fine can issue
            var rossSignedByRoot = _rootchainOfTrust.SignKey(ross.Asymmetric.GetPublicKey(), _issuerParams);
            _rosschainOfTrust.AddParentChain(_rootchainOfTrust.GetChain());
            _rosschainOfTrust.AddToTrusted(rootSelfSignResult);
            _rosschainOfTrust.SetOwnedKey(rossSignedByRoot);
            //phoebe : fine can issue
            var phoebeSignedByRoot = _rootchainOfTrust.SignKey(phoebe.Asymmetric.GetPublicKey(), _issuerParams);
            _phoebechainOfTrust.AddParentChain(_rootchainOfTrust.GetChain());
            _phoebechainOfTrust.AddToTrusted(rootSelfSignResult);
            _phoebechainOfTrust.SetOwnedKey(phoebeSignedByRoot);
            //monica : fine can't issue
            var monicaSignedByRoot = _rosschainOfTrust.SignKey(monica.Asymmetric.GetPublicKey(), _nonissuerParams);
            _monicachainOfTrust.AddParentChain(_rosschainOfTrust.GetChain());
            _monicachainOfTrust.AddToTrusted(rossSignedByRoot);
            _monicachainOfTrust.SetOwnedKey(monicaSignedByRoot);
            
            //rachel : issued by ross / expired
            var rachelSignedByRoot = _rosschainOfTrust.SignKey(rachel.Asymmetric.GetPublicKey(), _expiredParams);
            _rachelchainOfTrust.AddParentChain(_rosschainOfTrust.GetChain());
            _rachelchainOfTrust.AddToTrusted(rossSignedByRoot);
            _rachelchainOfTrust.SetOwnedKey(rachelSignedByRoot);
            //chandler : issued by monika not cool
            var chandlerSignedByRoot = _monicachainOfTrust.SignKey(chandler.Asymmetric.GetPublicKey(), _issuerParams);
            _chandlerchainOfTrust.AddParentChain(_monicachainOfTrust.GetChain());
            _chandlerchainOfTrust.AddToTrusted(monicaSignedByRoot);
            _chandlerchainOfTrust.SetOwnedKey(chandlerSignedByRoot);
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
            var isValid = _rootchainOfTrust.IsValidChain(_rootchainOfTrust.GetChain());
            Assert.True(isValid);
        }

        [Fact]
        public void RossChainGiven_RossAcceptsRootChain()
        {
            var isValid = _rosschainOfTrust.IsValidChain(_rosschainOfTrust.GetChain());
            Assert.True(isValid);
        }
        [Fact]
        public void RootChainGiven_RossAcceptsRootChain()
        {
            _output.WriteLine(_rosschainOfTrust.GetChain().Count.ToString());
            _output.WriteLine(_rosschainOfTrust.DebugChain(_rootchainOfTrust.GetChain()));
            var isValid = _rosschainOfTrust.IsValidChain(_rootchainOfTrust.GetChain());
            
            var a = _rosschainOfTrust.GetChain()
                .Any(x => _rootchainOfTrust.GetChain().Exists(y => x.KeyParam().key == y.KeyParam().key));
            var b = _rootchainOfTrust.GetChain()
                .Any(x => _rosschainOfTrust.GetChain().Exists(y => x.KeyParam().key == y.KeyParam().key));
            Assert.True(a);
            Assert.True(b);
            Assert.True(isValid);
        }
        [Fact]
        public void RossChainGiven_RootAcceptsRootChain()
        {
            var isValid = _rootchainOfTrust.IsValidChain(_rosschainOfTrust.GetChain());
            Assert.True(isValid);
        }
        [Fact]
        public void RossAndPhoebeChainGiven_RossAndPhoebeAcceptsRootChain()
        {
            _output.WriteLine(_rosschainOfTrust.GetChain().Count.ToString());
            var isValid1 = _phoebechainOfTrust.IsValidChain(_rosschainOfTrust.GetChain());
            var isValid2 = _rosschainOfTrust.IsValidChain(_phoebechainOfTrust.GetChain());
            Assert.True(isValid1);
            Assert.True(isValid2);
        }
        [Fact]
        public void MonicaChainGiven_RossAcceptsRootChain()
        {
            var isValid = _rosschainOfTrust.IsValidChain(_monicachainOfTrust.GetChain());
            Assert.True(isValid);
        }
        [Fact]
        public void MonicaChainGiven_RootAcceptsRootChain()
        {
            var isValid = _rootchainOfTrust.IsValidChain(_monicachainOfTrust.GetChain());
            Assert.True(isValid);
        }
        [Fact]
        public void ChandlerChainGiven_RossNotAcceptsChain()
        {
            var isValid = _rosschainOfTrust.IsValidChain(_chandlerchainOfTrust.GetChain());
            Assert.False(isValid);
        }
        [Fact]
        public void RachelChainGiven_RossNotAcceptsChain()
        {
            var isValid = _rosschainOfTrust.IsValidChain(_rachelchainOfTrust.GetChain());
            Assert.False(isValid);
        }
        [Fact]
        public void JaniceChainGiven_RossNotAcceptsChain()
        {
            var isValid = _rosschainOfTrust.IsValidChain(_janicechainOfTrust.GetChain());
            Assert.False(isValid);
        }
        [Fact]
        public void JaniceChainGiven_RootNotAcceptsChain()
        {
            var isValid = _rootchainOfTrust.IsValidChain(_janicechainOfTrust.GetChain());
            Assert.False(isValid);
        }
        [Fact]
        public void JaniceChainGiven_MonicaNotAcceptsChain()
        {
            var isValid = _monicachainOfTrust.IsValidChain(_janicechainOfTrust.GetChain());
            Assert.False(isValid);
        }
    }
}

