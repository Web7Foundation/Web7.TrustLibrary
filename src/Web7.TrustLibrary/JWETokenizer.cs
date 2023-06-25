using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web7.TrustLibrary
{
    // The JWETokenizer class is used to support the creation, verification, and serialization of JWE tokens.
    // This class uses keys created or deserialized from the Signer and Encrypter classes.
    // Keywords: Authenticated-Encryption JWE JWE-Token
    public class JWETokenizer
    {
        private JsonWebTokenHandler handler;

        public JWETokenizer() 
        {
            handler = new JsonWebTokenHandler();
        }

        public string CreateToken(string senderDID, string receiverDID, string messageBody64,
            ECDsaSecurityKey senderKeyPrivateSecurityKey, ECDsaSecurityKey receiverKeyPublicSecurityKey)
        {
            string token = "";
            token = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = senderDID,
                Audience = receiverDID,
                Claims = new Dictionary<string, object> { { "body", messageBody64 } },

                // private key for signing
                SigningCredentials = new SigningCredentials(senderKeyPrivateSecurityKey, SecurityAlgorithms.EcdsaSha256),

                // public key for encryption
                EncryptingCredentials = new EncryptingCredentials(receiverKeyPublicSecurityKey, SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512)
            });

            return token;
        }

        public TokenValidationResult ValidateToken(string token, string senderDID, string receiverDID, 
            RsaSecurityKey senderKeyPublicSecurityKey, RsaSecurityKey receiverKeyPrivateSecurityKey)
        {
            TokenValidationResult result;
            result = handler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidIssuer = senderDID,
                    ValidAudience = receiverDID,

                    // Alice's public key to verify signature
                    IssuerSigningKey = senderKeyPublicSecurityKey,

                    // Bob's private key for decryption
                    TokenDecryptionKey = receiverKeyPrivateSecurityKey
                });

            return result;
        }
    }
}
