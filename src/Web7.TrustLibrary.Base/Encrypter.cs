using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Web7.TrustLibrary.Base
{
    // The Encrypter class is used to support a set of public and private keys for encryption and decryption;
    // including key generation and serialization.
    // The Encrypter and Encrypter classes are used in the JWETokenizer class to create JWETokens.
    // Keywords: Confidentiality RSA
    public class Encrypter
    {
        public const string DID_KEYID_ENCRYPTER = "did:web7:keyid:encrypter:";

        private string keyID;
        private RSA keyPair;
        private RSA keyPrivate;
        private RSA keyPublic;
        private RsaSecurityKey keyPrivateSecurityKey;
        private RsaSecurityKey keyPublicSecurityKey;

        public Encrypter()
        {
            keyPair = RSA.Create();

            Initialize();
        }

        public Encrypter(JsonWebKey jsonWebKey, bool importPrivateKey)
        {
            ImportJsonWebKey(jsonWebKey, importPrivateKey);

            Initialize();
        }

        public Encrypter(string jsonWebKeyString, bool importPrivateKey)
        {
            JsonWebKey jsonWebKey = JsonSerializer.Deserialize<JsonWebKey>(jsonWebKeyString);
            ImportJsonWebKey(jsonWebKey, importPrivateKey);

            Initialize();
        }

        internal void Initialize()
        {
            keyID = DID_KEYID_ENCRYPTER + Guid.NewGuid().ToString();

            keyPrivate = keyPair;
            keyPrivateSecurityKey = new RsaSecurityKey(keyPrivate) { KeyId = keyID };

            keyPublic = RSA.Create(keyPair.ExportParameters(false));
            keyPublicSecurityKey = new RsaSecurityKey(keyPublic) { KeyId = keyID };
        }

        public string KeyID { get => keyID; set => keyID = value; }
        public RSA KeyPair { get => keyPair; }
        public RSA KeyPrivate { get => keyPrivate; }
        public RSA KeyPublic { get => keyPublic; }
        public RsaSecurityKey KeyPrivateSecurityKey { get => keyPrivateSecurityKey; }
        public RsaSecurityKey KeyPublicSecurityKey { get => keyPublicSecurityKey; }

        public JsonWebKey KeyPrivateSecurityKeyToJsonWebKey()
        {
            return JsonWebKeyConverter.ConvertFromRSASecurityKey(keyPrivateSecurityKey);
        }

        public string KeyPrivateJsonWebKeyAsString()
        {
            JsonWebKey jwk = KeyPrivateSecurityKeyToJsonWebKey();
            return Helper.JsonWebKeyToString(jwk);
        }

        public JsonWebKey KeyPublicSecurityKeyToJsonWebKey()
        {
            return JsonWebKeyConverter.ConvertFromRSASecurityKey(keyPublicSecurityKey);
        }

        public string KeyPublicJsonWebKeyAsString()
        {
            JsonWebKey jwk = KeyPublicSecurityKeyToJsonWebKey();
            return Helper.JsonWebKeyToString(jwk);
        }

        internal void ImportJsonWebKey(JsonWebKey jsonWebKey, bool importPrivateKey)
        {
            // https://www.scottbrady91.com/c-sharp/rsa-key-loading-dotnet
            RSAParameters rsaParameters = new RSAParameters();
            // PUBLIC KEY PARAMETERS
            // n parameter - public modulus
            rsaParameters.Modulus = Base64UrlEncoder.DecodeBytes(jsonWebKey.N);
            // e parameter - public exponent
            rsaParameters.Exponent = Base64UrlEncoder.DecodeBytes(jsonWebKey.E);

            if (importPrivateKey)
            {
                // PRIVATE KEY PARAMETERS (optional)
                // d parameter - the private exponent value for the RSA key 
                rsaParameters.D = Base64UrlEncoder.DecodeBytes(jsonWebKey.D);
                // dp parameter - CRT exponent of the first factor
                rsaParameters.DP = Base64UrlEncoder.DecodeBytes(jsonWebKey.DP);
                // dq parameter - CRT exponent of the second factor
                rsaParameters.DQ = Base64UrlEncoder.DecodeBytes(jsonWebKey.DQ);
                // p parameter - first prime factor
                rsaParameters.P = Base64UrlEncoder.DecodeBytes(jsonWebKey.P);
                // q parameter - second prime factor
                rsaParameters.Q = Base64UrlEncoder.DecodeBytes(jsonWebKey.Q);
                // qi parameter - CRT coefficient of the second factor
                rsaParameters.InverseQ = Base64UrlEncoder.DecodeBytes(jsonWebKey.QI);
            }

            keyPair = RSA.Create(rsaParameters);
        }

        public byte[] Encrypt(byte[] bytes)
        {
            // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsacryptoserviceprovider?view=net-7.0
            byte[] bytesEncrypted = null;
            // Create a new instance of RSACryptoServiceProvider.
            using (RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider())
            {

                // Import the RSA Key information. This only needs
                // to include the public key information.
                RSAParameters keyPublicParameters = keyPublic.ExportParameters(false);
                rsaProvider.ImportParameters(keyPublicParameters);

                // Encrypt the passed byte array and specify OAEP padding.  
                // OAEP padding is only available on Microsoft Windows XP or later.  
                bytesEncrypted = rsaProvider.Encrypt(bytes, false);
            }
            return bytesEncrypted;
        }

        public byte[] Decrypt(byte[] bytesEncrypted)
        {
            // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsacryptoserviceprovider?view=net-7.0
            byte[] bytesDecrypted = null;
            // Create a new instance of RSACryptoServiceProvider.
            using (RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider())
            {

                // Import the RSA Key information. This needs
                // to include the private key information.
                RSAParameters keyPrivateParameters = keyPrivate.ExportParameters(true);
                rsaProvider.ImportParameters(keyPrivateParameters);

                // Decrypt the passed byte array and specify OAEP padding.  
                // OAEP padding is only available on Microsoft Windows XP or later.  
                bytesDecrypted = rsaProvider.Decrypt(bytesEncrypted, false);
            }
            return bytesDecrypted;
        }
    }
}
