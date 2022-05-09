// /**
// Hooman Hamidpour ( hhamidpour7@gmail.com )
//  * Date: 202205097:12 PM
//  */

using System;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto;

namespace Moka.SharedKernel.Encryption
{
    public class SignKeyParameters
    {
        public bool CanIssue { get; set; }
        public DateTime ExpireAt { get; set; }
        public enum KeyType
        {
            Root,User,Server
        }
    }
    public class ChainOfTrust
    {
        private List<AsymmetricKeyParameter> _trustedRoots;
        private readonly HybridEncryption _mykey;

        public ChainOfTrust(HybridEncryption key)
        {
            _trustedRoots = new List<AsymmetricKeyParameter>();
            key = _mykey;
        }

        public void AddTrustedRoot(AsymmetricKeyParameter parameter)
        {
            _trustedRoots.Add(parameter);
        }

        public string Sign(AsymmetricKeyParameter key, SignKeyParameters parameters)
        {
            // key.ToString()
            throw new NotImplementedException();

        }
        public string MyTrustChain()
        {
            throw new NotImplementedException();
        }
        
        
    }
}