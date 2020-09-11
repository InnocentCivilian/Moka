using System;
using Moka.SharedKernel.Security;

namespace Moka.SharedKernel.Encryption
{
    public interface IKeyStorage
    {
        public bool IsPrivateKeyExist();

        public bool IsPublicKeyExist(string owner);

        public bool StorePrivateKey(Password password);
        
        public bool RemovePublicKey(string owner);

        public byte[] LoadKey(Password password);

        public byte[] ChangePassword(Password oldPassword, Password newPassword);
    }

    public class KeyStorage : IKeyStorage
    {
        public enum KeyTypes
        {
            AsymmetricPublic,
            AsymmetricPrivate,
            SymmetricPrivate
        }

        public static void WriteToFile(string owner, byte[] key, KeyTypes type)
        {
            var name = owner + type;
            System.IO.File.WriteAllText(name + ".key",
                System.Convert.ToBase64String(key)); //todo make it look better
            Console.WriteLine(name);
        }

        private string _owner;

        public KeyStorage(string owner)
        {
            _owner = owner;
        }

        public bool IsPrivateKeyExist()
        {
            throw new NotImplementedException();
        }

        public bool IsPublicKeyExist(string owner)
        {
            throw new NotImplementedException();
        }

        public bool StorePrivateKey(Password password)
        {
            throw new NotImplementedException();
        }

        public bool RemovePublicKey(string owner)
        {
            throw new NotImplementedException();
        }

        public byte[] LoadKey(Password password)
        {
            throw new NotImplementedException();
        }

        public byte[] ChangePassword(Password oldPassword, Password newPassword)
        {
            throw new NotImplementedException();
        }
    }
}