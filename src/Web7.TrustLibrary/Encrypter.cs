using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Web7.TrustLibrary
{
    // The Encrypter class is used to support a set of public and private keys for encryption and decryption;
    // including key generation and serialization.
    // The Signer and Encrypter classes are used in the JWETokenizer class to create JWETokens.
    // Keywords: Confidentiality RSA
    public class Encrypter
    {
        RSA keyPair = RSA.Create();
        string keyID = Helper.DID_KEYID_ENCRYPT + Guid.NewGuid().ToString();

        RSA GetKeyPair()
        {
            return keyPair;
        }


    }
}
