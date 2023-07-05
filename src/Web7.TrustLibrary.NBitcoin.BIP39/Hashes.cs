// Original Source: https://github.com/MetacoSA/NBitcoin/tree/master/NBitcoin/BIP39 (and sibling diretories) MIT License
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web7.TrustLibrary.NBitcoin.Crypto
{
    public static class Hashes
    {
        public static byte[] SHA256(byte[] data)
        {
            return SHA256(data, 0, data.Length);
        }
#if HAS_SPAN
		public static byte[] SHA256(ReadOnlySpan<byte> data)
		{
			return SHA256(data.ToArray(), 0, data.Length);
		}
#endif

        public static byte[] SHA256(byte[] data, int offset, int count)
        {
#if USEBC || WINDOWS_UWP || NETSTANDARD1X || NONATIVEHASH
			Sha256Digest sha256 = new Sha256Digest();
			sha256.BlockUpdate(data, offset, count);
			byte[] rv = new byte[32];
			sha256.DoFinal(rv, 0);
			return rv;
#else
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                return sha.ComputeHash(data, offset, count);
            }
#endif
        }
    }
}
