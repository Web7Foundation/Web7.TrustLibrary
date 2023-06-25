using Microsoft.IdentityModel.Tokens;
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
    // The Encrypter and Encrypter classes are used in the JWETokenizer class to create JWETokens.
    // Keywords: Confidentiality RSA
    public class Encrypter
    {
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

        internal void Initialize()
        {
            keyID = Helper.DID_KEYID_SIGN + Guid.NewGuid().ToString();

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

        public JsonWebKey KeyPrivateJsonWebKey()
        {
            return JsonWebKeyConverter.ConvertFromRSASecurityKey(keyPrivateSecurityKey);
        }

        public JsonWebKey KeyPublicJsonWebKey()
        {
            return JsonWebKeyConverter.ConvertFromRSASecurityKey(keyPublicSecurityKey);
        }

        public byte[] Encrypt(RSA keyPublic, byte[] bytes)
        {
            // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsacryptoserviceprovider?view=net-7.0
            byte[] encryptedData;
            //Create a new instance of RSACryptoServiceProvider.
            using (RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider())
            {

                //Import the RSA Key information. This only needs
                //toinclude the public key information.
                RSAParameters keyPublicParamters = keyPublic.ExportParameters(false);
                rsaProvider.ImportParameters(keyPublicParamters);

                //Encrypt the passed byte array and specify OAEP padding.  
                //OAEP padding is only available on Microsoft Windows XP or
                //later.  
                encryptedData = rsaProvider.Encrypt(bytes, false);
            }
            return encryptedData;
        }

        public byte[] Encrypt(byte[] bytes)
        {
            // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsacryptoserviceprovider?view=net-7.0
            byte[] bytesEncrypted;
            //Create a new instance of RSACryptoServiceProvider.
            using (RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider())
            {

                //Import the RSA Key information. This only needs
                //toinclude the public key information.
                RSAParameters keyPublicParameters = keyPublic.ExportParameters(false);
                rsaProvider.ImportParameters(keyPublicParameters);

                //Encrypt the passed byte array and specify OAEP padding.  
                //OAEP padding is only available on Microsoft Windows XP or
                //later.  
                bytesEncrypted = rsaProvider.Encrypt(bytes, false);
            }
            return bytesEncrypted;
        }

        public byte[] Decrypt(byte[] bytesEncrypted)
        {
            // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsacryptoserviceprovider?view=net-7.0
            byte[] bytesDecrypted;
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
