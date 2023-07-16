using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Web7.TrustLibrary.NBitcoin;
using Web7.TrustLibrary.NBitcoin.BIP39;
using Web7.TrustLibrary.NBitcoin;
using System.Text.Json;
using System.Text.Unicode;

namespace Web7.TrustLibrary.Base
{
    public class SymEncrypter
    {
        const int KEYGEN_ITERATIONS = 1000;

        Aes cipher = null;

        public SymEncrypter(string password, byte[] salt)
        {
            if (RandomUtils.Random == null)
            {
                RandomUtils.Random = new UnsecureRandom();
            }

            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, KEYGEN_ITERATIONS, HashAlgorithmName.SHA256);
            cipher = Aes.Create();
            cipher.Key = key.GetBytes(cipher.LegalKeySizes[0].MaxSize / 8);
            cipher.IV = cipher.IV;
            //Console.WriteLine("cipher: " + JsonSerializer.Serialize(cipher));
        }

        public byte[] Encrypt(string s)
        {
            return Encrypt(Encoding.UTF8.GetBytes(s));
        }

        public byte[] Encrypt(byte[] bytes)
        {
            byte[] encryptedBytes = cipher.EncryptCbc(bytes, cipher.IV);
            // IV is unique for each encryption - it must be preappended to each encyption so that decryption works later
            encryptedBytes = Helper.Combine(cipher.IV, encryptedBytes);
            return encryptedBytes;
        }

        public byte[] Decrypt(byte[] encryptedBytes)
        {
            // IV is unique for each encryption (random) - it must be restored for a specific decryption to work
            byte[] IV = new byte[16];
            Buffer.BlockCopy(encryptedBytes, 0, IV, 0, IV.Length);
            byte[] bytes = new byte[encryptedBytes.Length - IV.Length];
            Buffer.BlockCopy(encryptedBytes, IV.Length, bytes, 0, bytes.Length);

            cipher.IV = IV;
            byte[] decryptedBytes = cipher.DecryptCbc(bytes, cipher.IV);
            return decryptedBytes;
        }

        public string EncryptToString64(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            return EncryptToString64(bytes);
        }

        public string EncryptToString64(byte[] bytes)
        {
            byte[] encryptedBytes = Encrypt(bytes);
            return Helper.Base64Encode(encryptedBytes);
        }

        public byte[] DecryptFromString64ToBytes(string s64)
        {
            byte[] encryptedBytes = Helper.Base64Decode64(s64);
            byte[] bytes = Decrypt(encryptedBytes);
            return bytes;
        }

        public string DecryptFromString64(string s64)
        {
            byte[] bytes = DecryptFromString64ToBytes(s64);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
