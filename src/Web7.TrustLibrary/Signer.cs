using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Tokens;

namespace Web7.TrustLibrary
{
    // The Signer class can be used to to support the creation of keys for digital signing
    // including key generation and serialization.
    // The Signer and Encrypter classes are used in the JWETokenizer class to create JWETokens.
    // Keywords: Authenticity ECDsa
    public class Signer
    {
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

        internal void Initialize()
        {
            keyID = Helper.DID_KEYID_SIGN + Guid.NewGuid().ToString();

            keyPrivate = keyPair;
            keyPrivateSecurityKey = new ECDsaSecurityKey(keyPrivate) { KeyId = keyID };

            keyPublic = ECDsa.Create(keyPair.ExportParameters(false));
            keyPublicSecurityKey = new ECDsaSecurityKey(keyPublic) { KeyId = keyID };
        }

        public string KeyID { get => keyID; set => keyID = value; }
        public ECDsa KeyPair { get => keyPair; set => keyPair = value; }
        public ECDsa KeyPrivate { get => keyPrivate; set => keyPrivate = value; }
        public ECDsa KeyPublic { get => keyPublic; set => keyPublic = value; }
        public ECDsaSecurityKey KeyPrivateSecurityKey { get => keyPrivateSecurityKey; set => keyPrivateSecurityKey = value; }
        public ECDsaSecurityKey KeyPublicSecurityKey { get => keyPublicSecurityKey; set => keyPublicSecurityKey = value; }

        public JsonWebKey KeyPrivateJsonWebKey()
        {
            return JsonWebKeyConverter.ConvertFromECDsaSecurityKey(keyPrivateSecurityKey);
        }

        public JsonWebKey KeyPublicJsonWebKey()
        {
            return JsonWebKeyConverter.ConvertFromECDsaSecurityKey(keyPublicSecurityKey);
        }

        public void ImportFromJsonWebKey(JsonWebKey jsonWebKey, bool privateKey)
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
            // d parameter - the private exponent value for the EC key 
            if (privateKey) ecParameters.D = Base64UrlEncoder.DecodeBytes(jsonWebKey.D);
            // q parameter - second prime factor
            ecParameters.Q = new ECPoint()
            {
                X = Base64UrlEncoder.DecodeBytes(jsonWebKey.X),
                Y = Base64UrlEncoder.DecodeBytes(jsonWebKey.Y)
            };

            keyPair = ECDsa.Create(ecParameters);

            Initialize();
        }
    }
}
