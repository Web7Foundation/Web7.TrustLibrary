using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Web7.TrustLibrary.Base
{
    // The Hasher class is used to create Hashes of arbitrary Strings and Byte arrays.
    // This class uses the Signer class.
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
    }
}
