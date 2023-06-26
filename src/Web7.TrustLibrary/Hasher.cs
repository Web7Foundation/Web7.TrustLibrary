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
    public class Hasher
    {
        Signer signer;

        public Hasher(Signer signer)
        {
            this.signer = signer;
        }

        public byte[] Hash(string text)
        {
            return Hash(Encoding.UTF8.GetBytes(text));
        }

        public byte[] Hash(byte[] bytes)
        {
            return SHA256.HashData(bytes);
        }

        public byte[] SignText(string text)
        {
            return signer.KeyPrivate.SignHash(Hash(text));
        }

        public byte[] SignHash(byte[] hash)
        {
            return signer.KeyPrivate.SignHash(hash);
        }

        public bool VerifyTextHash(string text, byte[] signature)
        {
            return signer.KeyPublic.VerifyHash(Hash(text), signature);
        }

        public bool VerifyHash(byte[] hash, byte[] signature)
        {
            return signer.KeyPublic.VerifyHash(hash, signature);
        }

    }
}
