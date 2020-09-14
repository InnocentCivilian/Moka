using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Moka.SharedKernel.Encryption
{
    public class AesEncryption : ISymmetricEncryption
    {

        public byte[] GenerateSecretKey()
        {
            return GetCryptor().Key;
            // AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            // SecureRandom _random = new SecureRandom();
            // byte[] key = new byte[ISymmetricEncryption.KEYSIZE];
            // _random.NextBytes(key);
            // return key;
            // aes.
            // aes.
            // CipherKeyGenerator gen = new CipherKeyGenerator();
            //
            // gen = GeneratorUtilities.GetKeyGenerator("AES128"); // using AES
            //
            // byte[] k = gen.GenerateKey(); // 256 bit key
            // return k;
        }
        private int IVsize = ISymmetricEncryption.KEYSIZE / 8;

        private AesManaged GetCryptor()
        {
            AesManaged cryptor = new AesManaged();
            cryptor.Mode = CipherMode.CBC;
            cryptor.Padding = PaddingMode.PKCS7;
            cryptor.KeySize = ISymmetricEncryption.KEYSIZE;
            cryptor.BlockSize = ISymmetricEncryption.KEYSIZE;
            return cryptor;
        }

        public byte[] Encrypt(byte[] plain, byte[] key)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (AesManaged cryptor = GetCryptor())
                {
                    //We use the random generated iv created by AesManaged
                    byte[] iv = cryptor.IV;

                    using (CryptoStream cs = new CryptoStream(ms, cryptor.CreateEncryptor(key, iv),
                        CryptoStreamMode.Write))
                    {
                        cs.Write(plain, 0, plain.Length);
                    }

                    byte[] encryptedContent = ms.ToArray();

                    //Create new byte array that should contain both unencrypted iv and encrypted data
                    byte[] result = new byte[iv.Length + encryptedContent.Length];

                    //copy our 2 array into one
                    System.Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                    System.Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);

                    return result;
                }
            }
        }

        public byte[] Decrypt(byte[] cipher, byte[] secret)
        {
            byte[] iv = new byte[IVsize]; //initial vector is IVsize bytes
            byte[] encryptedContent = new byte[cipher.Length - IVsize]; //the rest should be encryptedcontent

            //Copy data to byte array
            System.Buffer.BlockCopy(cipher, 0, iv, 0, iv.Length);
            System.Buffer.BlockCopy(cipher, iv.Length, encryptedContent, 0, encryptedContent.Length);

            using (MemoryStream ms = new MemoryStream())
            {
                using (AesManaged cryptor = GetCryptor())
                {
                    using (CryptoStream cs = new CryptoStream(ms, cryptor.CreateDecryptor(secret, iv),
                        CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedContent, 0, encryptedContent.Length);
                    }

                    return ms.ToArray();
                }
            }
        }
    }
}