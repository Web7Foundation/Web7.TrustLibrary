using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Web7.TrustLibrary.Base
{
    // The Signer class can be used to to support the creation of keys for digital signing
    // including key generation and serialization.
    // The Signer and Encrypter classes are used in the JWETokenizer class to create JWETokens.
    // Keywords: Authenticity ECDsa
    public class Signer
    {
        public const string DID_KEYID_SIGNER = "did:web7:keyid:signer:";

        private string keyID;
        private ECDsa keyPair;
        private ECDsa keyPrivate;
        private ECDsa keyPublic;
        private ECDsaSecurityKey keyPrivateSecurityKey;
        private ECDsaSecurityKey keyPublicSecurityKey;

        public Signer()
        {
            keyPair = ECDsa.Create(ECCurve.NamedCurves.nistP256);

            Initialize();
        }

        public Signer(JsonWebKey jsonWebKey, bool importPrivateKey)
        {
            ImportJsonWebKey(jsonWebKey, importPrivateKey);

            Initialize();
        }

        public Signer(string jsonWebKeyString, bool importPrivateKey)
        {
            JsonWebKey jsonWebKey = JsonSerializer.Deserialize<JsonWebKey>(jsonWebKeyString);
            ImportJsonWebKey(jsonWebKey, importPrivateKey);

            Initialize();
        }

        internal void Initialize()
        {
            keyID = DID_KEYID_SIGNER + Guid.NewGuid().ToString();

            keyPrivate = keyPair;
            keyPrivateSecurityKey = new ECDsaSecurityKey(keyPrivate) { KeyId = keyID };

            keyPublic = ECDsa.Create(keyPair.ExportParameters(false));
            keyPublicSecurityKey = new ECDsaSecurityKey(keyPublic) { KeyId = keyID };
        }

        public string KeyID { get => keyID; set => keyID = value; }
        public ECDsa KeyPair { get => keyPair; }
        public ECDsa KeyPrivate { get => keyPrivate; }
        public ECDsa KeyPublic { get => keyPublic; }
        public ECDsaSecurityKey KeyPrivateSecurityKey { get => keyPrivateSecurityKey;  }
        public ECDsaSecurityKey KeyPublicSecurityKey { get => keyPublicSecurityKey; }

        public JsonWebKey KeyPrivateSecurityKeyToJsonWebKey()
        {
            return JsonWebKeyConverter.ConvertFromECDsaSecurityKey(keyPrivateSecurityKey);
        }
        public string KeyPrivateJsonWebKeyAsString()
        {
            JsonWebKey jwk = KeyPrivateSecurityKeyToJsonWebKey();
            return Helper.JsonWebKeyToString(jwk);
        }

        public JsonWebKey KeyPublicSecurityKeyToJsonWebKey()
        {
            return JsonWebKeyConverter.ConvertFromECDsaSecurityKey(keyPublicSecurityKey);
        }

        public string KeyPublicJsonWebKeyAsString()
        {
            JsonWebKey jwk = KeyPublicSecurityKeyToJsonWebKey();
            return Helper.JsonWebKeyToString(jwk);
        }

        internal void ImportJsonWebKey(JsonWebKey jsonWebKey, bool importPrivateKey)
        {
            // https://www.scottbrady91.com/c-sharp/ecdsa-key-loading
            var curve = jsonWebKey.Crv switch
            {
                "P-256" => ECCurve.NamedCurves.nistP256,
                "P-384" => ECCurve.NamedCurves.nistP384,
                "P-521" => ECCurve.NamedCurves.nistP521,
                _ => throw new NotSupportedException()
            };

            var ecParameters = new ECParameters();
            // crv parameter - public modulus
            ecParameters.Curve = curve;
            // q parameter - second prime factor
            ecParameters.Q = new ECPoint()
            {
                X = Base64UrlEncoder.DecodeBytes(jsonWebKey.X),
                Y = Base64UrlEncoder.DecodeBytes(jsonWebKey.Y)
            };
            // d parameter - the private exponent value for the EC key 
            if (importPrivateKey) ecParameters.D = Base64UrlEncoder.DecodeBytes(jsonWebKey.D);

            keyPair = ECDsa.Create(ecParameters);
        }
        public byte[] SignText(string text)
        {
            return keyPrivate.SignHash(Hasher.Hash(text));
        }

        public byte[] SignHash(byte[] hash)
        {
            return keyPrivate.SignHash(hash);
        }

        public bool VerifyTextHash(string text, byte[] signature)
        {
            return keyPublic.VerifyHash(Hasher.Hash(text), signature);
        }

        public bool VerifyHash(byte[] hash, byte[] signature)
        {
            return keyPublic.VerifyHash(hash, signature);
        }
    }
}
