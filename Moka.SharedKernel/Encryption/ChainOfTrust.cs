// /**
// Hooman Hamidpour ( hhamidpour7@gmail.com )
//  * Date: 202205097:12 PM
//  */

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace Moka.SharedKernel.Encryption
{
    public class SignKeyParameters
    {
        public bool CanIssue { get; set; }
        public DateTime ExpireAt { get; set; }

        public enum KeyType
        {
            Root,
            User,
            Server
        }
    }

    public class ChainOfTrust
    {
        private List<AsymmetricKeyParameter> _trustedRoots;
        private readonly HybridEncryption _mykey;
        private readonly IKeyStorage _keyStorage;

        public ChainOfTrust(HybridEncryption key)
        {
            _trustedRoots = new List<AsymmetricKeyParameter>();
            _mykey = key;
            _keyStorage = new PlainFileKeyStorage();
        }

        private string ConvertBody(object model)
        {
            return JsonConvert.SerializeObject(model);
            // return Encoding.UTF8.GetBytes();
        }

        public void AddTrustedRoot(AsymmetricKeyParameter parameter)
        {
            _trustedRoots.Add(parameter);
        }

        public string KeyParametersPair(AsymmetricKeyParameter key, SignKeyParameters parameters)
        {
            return ConvertBody(new
            {
                key = _keyStorage.StringifyPublicKey((RsaKeyParameters) key),
                parameters
            });
        }
        public byte[] Sign(AsymmetricKeyParameter key, SignKeyParameters parameters)
        {
            var toSign = KeyParametersPair(key, parameters);
            return _mykey.Asymmetric.Sign(Encoding.UTF8.GetBytes(toSign));
        }

        public bool Validate(byte[] payload,byte[] sign)
        {
            return _mykey.Asymmetric.ValidateSign(payload, sign, _mykey.Asymmetric.GetPublicKey());
        }
        public string MyTrustChain()
        {
            throw new NotImplementedException();
        }
    }
}