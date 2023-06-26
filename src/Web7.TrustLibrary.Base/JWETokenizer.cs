using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web7.TrustLibrary.Base
{
    // The JWETokenizer class is used to support the creation, verification, and serialization of JWE tokens.
    // This class uses keys created or deserialized from the Signer and Encrypter classes.
    // Keywords: Authenticated-Encryption JWE JWE-Token
    public class JWETokenizer
    {
        JsonWebTokenHandler handler;
        string senderDID;
        Signer senderSigner;
        string receiverDID;
        Encrypter receiverEncrypter;

        public JWETokenizer(string senderDID, Signer senderSigner, string receiverDID, Encrypter receiverEncrypter) 
        {
            handler = new JsonWebTokenHandler();
            this.senderDID = senderDID;
            this.senderSigner = senderSigner;   
            this.receiverDID = receiverDID; 
            this.receiverEncrypter = receiverEncrypter;
        }

        public string SenderDID { get => senderDID; set => senderDID = value; }
        public Signer SenderSigner { get => senderSigner; set => senderSigner = value; }
        public string ReceiverDID { get => receiverDID; set => receiverDID = value; }
        public Encrypter ReceiverEncrypter { get => receiverEncrypter; set => receiverEncrypter = value; }

        public string CreateJWEToken(string body)
        {
            ECDsaSecurityKey senderSigningKeyPrivateSecurityKey = senderSigner.KeyPrivateSecurityKey;
            RsaSecurityKey receiverEncryptionKeyPublicSecurityKey = receiverEncrypter.KeyPublicSecurityKey;
            string token = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = senderDID,
                Audience = receiverDID,
                Claims = new Dictionary<string, object> { { "body", body } },

                // Alice's sender private key for signing
                SigningCredentials = new SigningCredentials(senderSigningKeyPrivateSecurityKey, SecurityAlgorithms.EcdsaSha256),

                // Bob's receiver public key for encryption
                EncryptingCredentials = new EncryptingCredentials(receiverEncryptionKeyPublicSecurityKey, SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512)
            });

            return token;
        }

        public TokenValidationResult ValidateJWEToken(string token)
        {
            ECDsaSecurityKey senderSigningKeyPublicSecurityKey = senderSigner.KeyPublicSecurityKey;
            RsaSecurityKey receiverEncryptionKeyPrivateSecurityKey = receiverEncrypter.KeyPrivateSecurityKey;
            TokenValidationResult result = handler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidIssuer = senderDID,
                    ValidAudience = receiverDID,

                    // Alice's sender public key to verify signature
                    IssuerSigningKey = senderSigningKeyPublicSecurityKey,

                    // Bob's receiver private key for decryption
                    TokenDecryptionKey = receiverEncryptionKeyPrivateSecurityKey
                });

            return result;
        }
    }
}
