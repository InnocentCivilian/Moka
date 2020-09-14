using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO.Pem;
using PemReader = Org.BouncyCastle.OpenSsl.PemReader;

namespace Moka.SharedKernel.Encryption
{
    public class RSAEncryption : IAsymmetricEncryption
    {
        public RSAEncryption(string owner)
        {
            Owner = owner;
            // RSACryptoServiceProvider csp = new RSACryptoServiceProvider(); // cspParams);
            key = RSAHandler.GetOrGenerateKeyPair(owner);
            // RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters) key.Private);
            // csp.ImportParameters(rsaParams);
        }

        public AsymmetricKeyParameter GetPublicKey()
        {
            return key.Public;
        }

        private AsymmetricCipherKeyPair key;
        // private RSACryptoServiceProvider csp;
        // private Pkcs1Encoding Engine;

        public string Owner { get; set; }

        public byte[] EncryptWithPublic(byte[] data, AsymmetricKeyParameter receiverPublickKey)
        {
            var Engine = new Pkcs1Encoding(new RsaEngine());

            Engine.Init(true, receiverPublickKey);
            var encrypted =
                Convert.ToBase64String(Engine.ProcessBlock(data, 0, data.Length));
            return Engine.ProcessBlock(data, 0, data.Length);
        }

        public byte[] DecryptWithPrivate(byte[] cipher)
        {
            var decryptEngine = new Pkcs1Encoding(new RsaEngine());


            decryptEngine.Init(false, key.Private);


            var decrypted =
                decryptEngine.ProcessBlock(cipher, 0, cipher.Length);
            return decrypted;
        }

        public byte[] Sign(byte[] data)
        {
            ISigner sign = SignerUtilities.GetSigner(PkcsObjectIdentifiers.Sha256WithRsaEncryption.Id);
            sign.Init(true, (RsaKeyParameters) key.Private);
            sign.BlockUpdate(data, 0, data.Length);
            return sign.GenerateSignature();
            // RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            //
            // var rsaPrivKey = (RsaPrivateCrtKeyParameters) key.Private; 
            // var rsaParameters = DotNetUtilities.ToRSAParameters(rsaPrivKey);
            // rsa.ImportParameters(rsaParameters);
            //
            // // compute sha1 hash of the data
            // var sha = new SHA1CryptoServiceProvider();
            // byte[] hash = sha.ComputeHash(data);
            //
            // // actually compute the signature of the SHA1 hash of the data
            // var sig = rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));
            //
            // return sig;
        }
        // public void PrintKeys(AsymmetricCipherKeyPair keyPair)
        // {
        //     using (TextWriter textWriter1 = new StringWriter())
        //     {
        //         var pemWriter1 = new PemWriter(textWriter1);
        //         pemWriter1.WriteObject(keyPair.Private);
        //         pemWriter1.Writer.Flush();
        //
        //         string privateKey = textWriter1.ToString();
        //         Console.WriteLine(privateKey);
        //     }
        //
        //     using (TextWriter textWriter2 = new StringWriter())
        //     {
        //         var pemWriter2 = new PemWriter(textWriter2);
        //         pemWriter2.WriteObject(keyPair.Public);
        //         pemWriter2.Writer.Flush();
        //         string publicKey = textWriter2.ToString();
        //         Console.WriteLine(publicKey);
        //     }
        // }
        public bool ValidateSign(byte[] data, byte[] sig, AsymmetricKeyParameter senderPublic)
        {
            ISigner signClientSide = SignerUtilities.GetSigner(PkcsObjectIdentifiers.Sha256WithRsaEncryption.Id);
            signClientSide.Init(false, (RsaKeyParameters) senderPublic);
            signClientSide.BlockUpdate(data, 0, data.Length);
            return  signClientSide.VerifySignature(sig);
            // RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();

            // var rsaPrivKey = (RsaPrivateCrtKeyParameters) senderPublic; 
            // var rsaParameters = DotNetUtilities.ToRSAParameters((RsaKeyParameters) senderPublic);
            // rsa.ImportParameters(rsaParameters);


            // var rsaParameters = new RSAParameters();
            // rsaParameters.Modulus = senderPublic;

            // rsa.ImportParameters(rsaParameters);
            // compute sha1 hash of the data
            // var sha = new SHA1CryptoServiceProvider();
            // byte[] hash = sha.ComputeHash(data);
            //
            // return rsa.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), sig);

            // actually compute the signature of the SHA1 hash of the data
            // var sig = rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));
            //
            // return sig;
        }

        public byte[] MakeHash(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}