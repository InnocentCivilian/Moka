using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Moka.SharedKernel.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Moka.SharedKernel.Encryption
{
    public class RSAHandler
    {
        public const int keySize = 2048;
        public static RSA GetKey(string owner)
        {
            //Generate a public/private key pair.  
            RSA rsa = RSA.Create();
            KeyStorage.WriteToFile(owner, rsa.ExportRSAPrivateKey(), KeyStorage.KeyTypes.AsymmetricPrivate);
            KeyStorage.WriteToFile(owner, rsa.ExportRSAPublicKey(), KeyStorage.KeyTypes.AsymmetricPublic);
            return rsa;
        }

        public static AsymmetricCipherKeyPair GetOrGenerateKeyPair(string owner)
        {
            if (File.Exists($@"C:\MokaKeys\{owner}_private.key"))
            {
                var key = LoadKey(owner);
                return key;
                
            }
            else
            {
                RsaKeyPairGenerator rsaKeyPairGenerator = new RsaKeyPairGenerator();
                rsaKeyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), keySize));
                AsymmetricCipherKeyPair keyPair = rsaKeyPairGenerator.GenerateKeyPair();
                StoreKeyPair(keyPair, owner);
                return keyPair;
            }
        }

        public static void StoreKeyPair(AsymmetricCipherKeyPair keyPair, string owner)
        {
            RsaKeyParameters privatekey = (RsaKeyParameters) keyPair.Private;
            RsaKeyParameters publickey = (RsaKeyParameters) keyPair.Public;


            //To print the public key in pem format
            TextWriter textWriter1 = new StringWriter();
            PemWriter pemWriter1 = new PemWriter(textWriter1);
            pemWriter1.WriteObject(publickey);
            pemWriter1.Writer.Flush();
            string print_publickey = textWriter1.ToString();
            StoreToFile(print_publickey, owner, "public");

            TextWriter textWriter2 = new StringWriter();
            PemWriter pemWriter2 = new PemWriter(textWriter2);
            pemWriter2.WriteObject(privatekey);
            pemWriter2.Writer.Flush();
            string print_privatekey = textWriter2.ToString();
            StoreToFile(print_privatekey, owner, "private");
        }

        public static string GetAsymPrivateHash(AsymmetricCipherKeyPair keyPair)
        {
            RsaKeyParameters privatekey = (RsaKeyParameters) keyPair.Private;
            return GetHash(privatekey);
        }
        public static string GetHash(RsaKeyParameters key)
        {
            TextWriter textWriter1 = new StringWriter();
            PemWriter pemWriter1 = new PemWriter(textWriter1);
            pemWriter1.WriteObject(key);
            pemWriter1.Writer.Flush();
            string keyprint = textWriter1.ToString();
            return HashMaker.ComputeSha256Hash(keyprint);
        }
        public static void StoreToFile(string key, string owner, string type)
        {
            File.WriteAllText($@"C:\MokaKeys\{owner}_{type}.key", key);
        }

        public static AsymmetricCipherKeyPair LoadKey(string owner, string type = "private")
        {
            var key = File.ReadAllText($@"C:\MokaKeys\{owner}_{type}.key");
            PemReader pr = new PemReader(new StringReader(key));
            AsymmetricCipherKeyPair KeyPair = (AsymmetricCipherKeyPair) pr.ReadObject();
            return KeyPair;
        }

        public string RsaEncryptWithPublic(string clearText, string publicKey)
        {
            var bytesToEncrypt = Encoding.UTF8.GetBytes(clearText);

            var encryptEngine = new Pkcs1Encoding(new RsaEngine());

            using (var txtreader = new StringReader(publicKey))
            {
                var keyParameter = (AsymmetricKeyParameter) new PemReader(txtreader).ReadObject();

                encryptEngine.Init(true, keyParameter);
            }

            var encrypted =
                Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
            return encrypted;
        }

        public string RsaEncryptWithPrivate(string clearText, string privateKey)
        {
            var bytesToEncrypt = Encoding.UTF8.GetBytes(clearText);

            var encryptEngine = new Pkcs1Encoding(new RsaEngine());

            using (var txtreader = new StringReader(privateKey))
            {
                var keyPair = (AsymmetricCipherKeyPair) new PemReader(txtreader).ReadObject();

                encryptEngine.Init(true, keyPair.Private);
            }

            var encrypted =
                Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
            return encrypted;
        }


        // Decryption:

        public string RsaDecryptWithPrivate(string base64Input, string privateKey)
        {
            var bytesToDecrypt = Convert.FromBase64String(base64Input);

            AsymmetricCipherKeyPair keyPair;
            var decryptEngine = new Pkcs1Encoding(new RsaEngine());

            using (var txtreader = new StringReader(privateKey))
            {
                keyPair = (AsymmetricCipherKeyPair) new PemReader(txtreader).ReadObject();

                decryptEngine.Init(false, keyPair.Private);
            }

            var decrypted =
                Encoding.UTF8.GetString(decryptEngine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length));
            return decrypted;
        }

        public string RsaDecryptWithPublic(string base64Input, string publicKey)
        {
            var bytesToDecrypt = Convert.FromBase64String(base64Input);

            var decryptEngine = new Pkcs1Encoding(new RsaEngine());

            using (var txtreader = new StringReader(publicKey))
            {
                var keyParameter = (AsymmetricKeyParameter) new PemReader(txtreader).ReadObject();

                decryptEngine.Init(false, keyParameter);
            }

            var decrypted =
                Encoding.UTF8.GetString(decryptEngine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length));
            return decrypted;
        }
        private static byte[] SignWithPrivateKey(string data,string privateKey)
        {
            RSACryptoServiceProvider rsa;

            using (var keyreader = new StringReader(privateKey))
            {
                var pemreader = new PemReader(keyreader);
                var y = (AsymmetricCipherKeyPair) pemreader.ReadObject();
                var rsaPrivKey = (RsaPrivateCrtKeyParameters)y.Private;
                rsa = (RSACryptoServiceProvider)RSACryptoServiceProvider.Create();
                var rsaParameters = DotNetUtilities.ToRSAParameters(rsaPrivKey);
                rsa.ImportParameters(rsaParameters);

            }

            // compute sha1 hash of the data
            var sha = new SHA1CryptoServiceProvider();
            byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(data));

            // actually compute the signature of the SHA1 hash of the data
            var sig = rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));

            // base64 encode the signature and write to compare to the python version
            String b64signature = Convert.ToBase64String(sig);
            using (var sigwriter = new StreamWriter(@"C:\scratch\csharp_sig2.txt"))
            {
                sigwriter.Write(b64signature);
            }

            return sig;
        }

        private static bool VerifyWithPublicKey(string data,byte[] sig,string publicKey)
        {
            RSACryptoServiceProvider rsa;

            using (var keyreader = new StringReader(publicKey))
            {
                var pemReader = new PemReader(keyreader);
                var y = (RsaKeyParameters) pemReader.ReadObject();
                rsa = (RSACryptoServiceProvider) RSACryptoServiceProvider.Create();
                var rsaParameters = new RSAParameters();
                rsaParameters.Modulus = y.Modulus.ToByteArray();
                rsaParameters.Exponent = y.Exponent.ToByteArray();
                rsa.ImportParameters(rsaParameters);
            }

            // compute sha1 hash of the data
            var sha = new SHA1CryptoServiceProvider();
            byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(data));

            // This always returns false
            return rsa.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"),sig);
        }
    }
}