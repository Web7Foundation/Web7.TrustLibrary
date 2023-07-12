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

namespace Web7.TrustLibrary.Base
{
    public class SymEncrypter
    {
        const int KEYGEN_ITERATIONS = 1000;

        Mnemonic mnemonic;
        Aes cipher;

        byte[] masterKey;
        public byte[] MasterKey { get => masterKey; set => masterKey = value; }
        public string[] Words { get => mnemonic.Words; }
        public string WordString { get => string.Join(" ", mnemonic.Words); }

        public SymEncrypter(string masterPassphrase) // New Mnemonic to create new Master Key
        {
            Initialize(masterPassphrase, "");
        }

        public SymEncrypter(string masterPassphrase, string wordString) // Restore Mnemonic from string to recreate Restore Master Key
        {
            Initialize(masterPassphrase, wordString);
        }

        public SymEncrypter(string masterPassphrase, string[] mnemonicWords) // Restore Mnemonic from array of strings to recreate Master Key 
        {
            Initialize(masterPassphrase, String.Join(" ", mnemonicWords));
        }

        private void Initialize(string masterPassphrase, string wordString)
        {
            if (RandomUtils.Random == null)
            {
                RandomUtils.Random = new UnsecureRandom();
            }

            if (String.IsNullOrEmpty(wordString))
            {
                Mnemonic mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
                wordString = String.Join(" ", mnemonic.Words);
            }

            mnemonic = new Mnemonic(wordString, Wordlist.English);
            masterKey = mnemonic.DeriveSeed(masterPassphrase); // Compute masterKey (salt) from mnemonic & master passphrase

            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(masterPassphrase, masterKey, KEYGEN_ITERATIONS, HashAlgorithmName.SHA256);
            cipher = Aes.Create();
            cipher.Key = key.GetBytes(cipher.LegalKeySizes[0].MaxSize / 8);
            cipher.IV = cipher.IV;
            //Console.WriteLine("cipher: " + JsonSerializer.Serialize(cipher));
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
    }
}
