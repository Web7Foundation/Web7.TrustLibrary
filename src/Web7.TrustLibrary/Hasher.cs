using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Web7.TrustLibrary
{
    // The Hasher class is used to create Hashes of arbitrary Strings and Byte arrays.
    // This class is used primarily by the Signer class.
    // Keywords: Authenticity SHA SHA256 Hash
    public static class Hasher
    {
        public static byte[] Hash(string text)
        {
            return Hash(Encoding.UTF8.GetBytes(text));
        }

        public static byte[] Hash(byte[] bytes)
        {
            return SHA256.HashData(bytes);
        }

        public static byte[] SignText(ECDsa signerKeyPrivate, string text)
        {
            return signerKeyPrivate.SignHash(Hasher.Hash(text));
        }

        public static byte[] SignHash(ECDsa signerKeyPrivate, byte[] hash)
        {
            return signerKeyPrivate.SignHash(hash);
        }

        public static bool VerifyTextHash(ECDsa signerKeyPublic, string text, byte[] signature)
        {
            return signerKeyPublic.VerifyHash(Hasher.Hash(text), signature);
        }

        public static bool VerifyHash(ECDsa signerKeyPublic, byte[] hash, byte[] signature)
        {
            return signerKeyPublic.VerifyHash(hash, signature);
        }

    }
}
