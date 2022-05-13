using System;
using System.IO;
using Moka.SharedKernel.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

namespace Moka.SharedKernel.Encryption
{
    public interface IKeyStorage
    {
        public bool IsPrivateKeyExist();
        public AsymmetricCipherKeyPair LoadPrivateKey(Password password);
        public AsymmetricKeyParameter LoadPublicKey(string owner);
        public bool IsPublicKeyExist(string owner);
        public void StorePrivateKey(AsymmetricCipherKeyPair keyPair, Password password);

        public void StorePublicKey(RsaKeyParameters publickey, string owner);

        string StringifyPublicKey(RsaKeyParameters publickey);
        //
        // public bool RemovePublicKey(string owner);
        //
        // public byte[] LoadKey(Password password);
        //
        // public byte[] ChangePassword(Password oldPassword, Password newPassword);
    }

    public class PlainFileKeyStorage : IKeyStorage
    {
        private string BASEPATH
        {
            get { return $"C:\\MokaKeys\\{_owner}\\"; }
        }

        public enum KeyTypes
        {
            AsymmetricPublic,
            AsymmetricPrivate,
            SymmetricPrivate
        }

        public static void WriteToFile(string owner, byte[] key, KeyTypes type)
        {
            var name = owner + type;
            File.WriteAllText(name + ".key",
                Convert.ToBase64String(key)); //todo make it look better
            Console.WriteLine(name);
        }

        private string _owner;

        public PlainFileKeyStorage()
        {
        }

        public PlainFileKeyStorage(string owner)
        {
            _owner = owner;
            bool exists = Directory.Exists(BASEPATH);

            if (!exists)
                Directory.CreateDirectory(BASEPATH);
        }

        public bool IsPrivateKeyExist()
        {
            return File.Exists($@"{BASEPATH}{_owner}_{KeyTypes.AsymmetricPrivate}.key");
        }

        public AsymmetricCipherKeyPair LoadPrivateKey(Password password = default)
        {
            if (password == default)
            {
                if (IsPrivateKeyExist())
                {
                    var key = File.ReadAllText($@"{BASEPATH}{_owner}_{KeyTypes.AsymmetricPrivate}.key");
                    PemReader pr = new PemReader(new StringReader(key));
                    AsymmetricCipherKeyPair KeyPair = (AsymmetricCipherKeyPair) pr.ReadObject();
                    return KeyPair;
                }
                // throw new FileNotFoundException();
            }

            throw new NotImplementedException(" password load not implemented");
        }

        public AsymmetricKeyParameter LoadPublicKey(string owner)
        {
            if (IsPublicKeyExist(owner))
            {
                var key = File.ReadAllText($@"{BASEPATH}{owner}_{KeyTypes.AsymmetricPublic}.key");

                return LoadFromString(key);
            }

            throw new FileNotFoundException();
        }

        public static AsymmetricKeyParameter LoadFromString(string key)
        {
            PemReader pr = new PemReader(new StringReader(key));
            AsymmetricKeyParameter KeyPair = (AsymmetricKeyParameter) pr.ReadObject();
            return KeyPair;
        }


        public bool IsPublicKeyExist(string owner)
        {
            return File.Exists($@"{BASEPATH}{owner}_{KeyTypes.AsymmetricPublic}.key");
        }

        public void StorePrivateKey(AsymmetricCipherKeyPair keyPair, Password password = default)
        {
            RsaKeyParameters privatekey = (RsaKeyParameters) keyPair.Private;
            RsaKeyParameters publickey = (RsaKeyParameters) keyPair.Public;


            //To print the public key in pem format
            TextWriter textWriter1 = new StringWriter();
            PemWriter pemWriter1 = new PemWriter(textWriter1);
            pemWriter1.WriteObject(publickey);
            pemWriter1.Writer.Flush();
            string print_publickey = textWriter1.ToString();
            StoreToFile(print_publickey, _owner, KeyTypes.AsymmetricPublic);

            TextWriter textWriter2 = new StringWriter();
            PemWriter pemWriter2 = new PemWriter(textWriter2);
            pemWriter2.WriteObject(privatekey);
            pemWriter2.Writer.Flush();
            string print_privatekey = textWriter2.ToString();
            StoreToFile(print_privatekey, _owner, KeyTypes.AsymmetricPrivate, password);
        }

        public string StringifyPublicKey(RsaKeyParameters publickey)
        {
            TextWriter textWriter1 = new StringWriter();
            PemWriter pemWriter1 = new PemWriter(textWriter1);
            pemWriter1.WriteObject(publickey);
            pemWriter1.Writer.Flush();
            return textWriter1.ToString();
        }

        public void StorePublicKey(RsaKeyParameters publickey, string owner)
        {
            var print_publickey = StringifyPublicKey(publickey);
            StoreToFile(print_publickey, owner, KeyTypes.AsymmetricPublic);
        }

        private void StoreToFile(string key, string owner, KeyTypes type, Password password = null)
        {
            if (password != default || type == KeyTypes.AsymmetricPrivate)
            {
                if (password == null)
                {
                    File.WriteAllText($@"{BASEPATH}{owner}_{type}.key", key);
                }
                else
                {
                    File.WriteAllText($@"{BASEPATH}{owner}_{type}_{password.ToString()}.key", key);
                    File.WriteAllText($@"{BASEPATH}{owner}_{type}.key", "");
                }
            }
            else
            {
                File.WriteAllText($@"{BASEPATH}{owner}_{type}.key", key);
            }
        }
    }
}