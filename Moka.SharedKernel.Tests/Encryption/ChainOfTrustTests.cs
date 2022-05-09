// /**
// Hooman Hamidpour ( hhamidpour7@gmail.com )
//  * Date: 202205097:21 PM
//  */

using System;
using Moka.SharedKernel.Encryption;
using Xunit;
using Xunit.Abstractions;

namespace Moka.SharedKernel.Tests.Encryption
{
    public class ChainOfTrustTests
    {
        private ChainOfTrust _chainOfTrust;
        private HybridEncryption root;
        private readonly ITestOutputHelper output;


        public ChainOfTrustTests(ITestOutputHelper output)
        {
            this.output = output;
            root = new HybridEncryption(nameof(root));
            _chainOfTrust = new ChainOfTrust(root);

        }

        // [Fact]
        // public void RootSelfSign_KeyGiven_SelfSigns()
        // {
        //    var sig
        // }
    }
}