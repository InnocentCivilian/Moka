using System;

namespace Moka.SharedKernel.Encryption
{
    public class KeyStorage
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
    }
}