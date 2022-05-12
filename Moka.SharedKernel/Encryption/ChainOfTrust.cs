// /**
// Hooman Hamidpour ( hhamidpour7@gmail.com )
//  * Date: 202205097:12 PM
//  */

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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

    public class SignedKeyObject
    {
        public string Payload { get; set; }
        public string Sign { get; set; }
        public string Hash { get; set; }

        public string ToJson()
        {
            return ChainOfTrust.ConvertBody(this);
        }
        public static SignedKeyObject FromJson(string json)
        {
            return JsonConvert.DeserializeObject<SignedKeyObject>(json);
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

        public static string ConvertBody(object model)
        {
            return JsonConvert.SerializeObject(model);
            // return Encoding.UTF8.GetBytes();
        }

        public void AddTrustedRoot(AsymmetricKeyParameter parameter)
        {
            _trustedRoots.Add(parameter);
        }

        public SignedKeyObject SignedKeyParametersPair(AsymmetricKeyParameter key, SignKeyParameters parameters, byte[] sign)
        {
            var payload = KeyParametersPair(key, parameters);
            return new SignedKeyObject
            {
                Payload = payload,
                Sign = Convert.ToBase64String(sign),
                Hash = ComputeSha256Hash(payload)
            };
        }

        // public SignedKeyObject DeserializeSignedObject(string json)
        // {
        //     return JsonConvert.DeserializeObject<SignedKeyObject>(json);
        // }

        public string KeyParametersPair(AsymmetricKeyParameter key, SignKeyParameters parameters)
        {
            return ConvertBody(new
            {
                key = _keyStorage.StringifyPublicKey((RsaKeyParameters) key),
                parameters
            });
        }

        public byte[] ComputeSha256Hash(byte[] rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
              return  sha256Hash.ComputeHash(rawData);
            }
        }
        public string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();  
            }
        }

        public byte[] SignHash(AsymmetricKeyParameter key, SignKeyParameters parameters)
        {
            var toSign = KeyParametersPair(key, parameters);

            return _mykey.Asymmetric.Sign(Encoding.UTF8.GetBytes(ComputeSha256Hash(toSign)));
        }
        public byte[] SignBytes(byte[] bytes)
        {
            return _mykey.Asymmetric.Sign(bytes);
        }
        public bool Validate(byte[] payload, byte[] sign)
        {
            return _mykey.Asymmetric.ValidateSign(payload,sign,
                _mykey.Asymmetric.GetPublicKey());
        }
        public bool Validate(string payload, string sign)
        {
            return _mykey.Asymmetric.ValidateSign(Encoding.UTF8.GetBytes(payload), Encoding.UTF8.GetBytes(sign),
                _mykey.Asymmetric.GetPublicKey());
        }

        public bool Validate(string payload, string sign, AsymmetricKeyParameter key)
        {
            return _mykey.Asymmetric.ValidateSign(Encoding.UTF8.GetBytes(payload), Encoding.UTF8.GetBytes(sign), key);
        }

        public string MyTrustChain()
        {
            throw new NotImplementedException();
        }
    }
}