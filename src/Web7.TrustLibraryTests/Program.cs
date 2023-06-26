using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Web7.TrustLibrary.Base;
using Web7.TrustLibrary.Did.DIDComm;
using Web7.TrustLibrary.Did.DIDDocumemt;

namespace Web7.TrustLibrary.Base
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string MESSAGE_TYPE = "https://example.org/example/1.0/hello";

            string plaintext = "{ \"message\": \"Hello world!\" }";
            byte[] plaintextbytes = Encoding.UTF8.GetBytes(plaintext);

            // 0. DIDComm namespace
            DateTime now = DateTime.Now;
            Message msg = new Message(
                Helper.DID_MESSAGEID + Guid.NewGuid().ToString(),
                MESSAGE_TYPE,
                Helper.DID_ALICE,
                new List<string>() { Helper.DID_BOB },
                Helper.DID_THID + Guid.NewGuid().ToString(),
                "",
                Helper.UNIX_time(now),
                Helper.UNIX_time(now.AddDays(30)),
                Helper.Base64Encode(plaintext)
            );
            string text64 = Helper.Base64Encode("Foo bar!");
            AttachmentData d = new AttachmentData("", "", "", text64, "");
            Attachment a = new Attachment(
                Helper.DID_ATTACHMENTID + Guid.NewGuid().ToString(),
                "Attachment abc",
                "abc.txt",
                "text/plain",
                "",
                Helper.UNIX_time(now),
                d,
                0
            );
            msg.attachments.Add(a);

            string msgJson = JsonSerializer.Serialize<Message>(msg);
            Console.WriteLine( "0. msgJson: " + msgJson );
            Console.WriteLine();

            // 1. Signer key generation
            Signer signer = new Signer();
            ECDsa signerKeyPrivate = signer.KeyPrivate;
            ECDsa signerKeyPublic = signer.KeyPublic;
            ECDsaSecurityKey signerKeyPrivateSecurityKey = signer.KeyPrivateSecurityKey;
            ECDsaSecurityKey signerKeyPublicSecurityKey = signer.KeyPublicSecurityKey;
            Console.WriteLine("1. Signer key generation");
            Console.WriteLine();

            // 2. Generate and verify signature for a string
            byte[] signature = signer.SignText(plaintext);
            bool verify = signer.VerifyTextHash(plaintext, signature);
            Console.WriteLine("2. VerifyTextHash: " + verify.ToString());
            Console.WriteLine();

            // 3. Export keys as JWTs
            JsonWebKey signerKeyPrivateJWT = signer.KeyPrivateJsonWebKey();
            string signerKeyPrivateJWTString = Helper.JsonWebKeyToString(signerKeyPrivateJWT);
            Console.WriteLine("3. signerKeyPrivateJWTString: " + signerKeyPrivateJWTString);
            JsonWebKey signerKeyPublicJWT = signer.KeyPublicJsonWebKey();
            string signerKeyPublicJWTString = Helper.JsonWebKeyToString(signerKeyPublicJWT);
            Console.WriteLine("3. signerKeyPublicJWTString: " + signerKeyPublicJWTString);
            Console.WriteLine();

            // 4. Import keys from JWTs
            Signer signerKeyPrivateFromJWT = new Signer(signerKeyPrivateJWTString, true);
            Signer signerKeyPublicFromJWT = new Signer(signerKeyPublicJWTString, false);
            Console.WriteLine("4. Signer key import from JWT strings");
            Console.WriteLine();

            // 5. Generate and verify signature for a string using imported JWTs
            byte[] signatureJWT = signer.SignText(plaintext);
            bool verifyJWT = signer.VerifyTextHash(plaintext, signatureJWT);
            Console.WriteLine("5. VerifyTextHashJWT: " + verifyJWT.ToString());
            Console.WriteLine();

            // 6. Encrypter key generation
            Encrypter encrypter = new Encrypter();
            RSA encrypterKeyPrivate = encrypter.KeyPrivate;
            RSA encrypterKeyPublic = encrypter.KeyPublic;
            RsaSecurityKey encrypterKeyPrivateSecurityKey = encrypter.KeyPrivateSecurityKey;
            RsaSecurityKey encrypterKeyPublicSecurityKey = encrypter.KeyPublicSecurityKey;
            Console.WriteLine("6. Encrypter key generation");
            Console.WriteLine();

            // 7. Encrypt and decrypt a string
            byte[] bytesEncrypted = encrypter.Encrypt(plaintextbytes);
            byte[] bytesDecrypted = encrypter.Decrypt(bytesEncrypted);
            string stringDecrypted = Encoding.UTF8.GetString(bytesDecrypted);
            Console.WriteLine("7. String encrypted/decrypted: " + stringDecrypted);
            Console.WriteLine();

            // 8. Hash a string (and by implication, an byte array)
            byte[] hash = Hasher.Hash(plaintext);
            string hash64 = Helper.Base64Encode(hash);
            byte[] hash64decoded = Helper.Base64DecodeBytes(hash64);
            string hash64copy = Helper.Base64Encode(hash64decoded);
            Console.WriteLine("8. Hash64:     " + hash64);
            Console.WriteLine("8. Hash64Copy: " + hash64copy);
            Console.WriteLine();

            // 9. Create and validate a JWE token
            JWETokenizer jwter = new JWETokenizer(Helper.DID_ALICE, signer, Helper.DID_BOB, encrypter);
            string token = jwter.CreateJWEToken(msgJson);
            Console.WriteLine("9. Token: " + token);
            TokenValidationResult result = jwter.ValidateJWEToken(token);
            Console.WriteLine("9. IsValid: " + result.IsValid.ToString());
            Console.WriteLine("9. Body: " + result.Claims["body"].ToString());
            Console.WriteLine();

            // 10. Create a DID Document
            Did.ServiceMap service0 = new Did.ServiceMap(null, "#default", new List<string>() { "default" }, new List<string>() { "http://localhost:8080" } );
            Did.ServiceMap relationship0 = new Did.ServiceMap(null, "#verifiers-accepted", new List<string>() { "verifiers-accepted" }, new List<string>() { "did:web7:verifier:1111" });
            DIDDocumenter diddocer = new DIDDocumenter(Helper.DID_ALICE, new List<string> { Helper.DID_ALICE }, signer, encrypter,
                new List<Did.ServiceMap>() { service0 },
                new List<Did.ServiceMap>() { relationship0 } );
            string diddoc = diddocer.ToString();
            Console.WriteLine("10. DID Document: " + Helper.JsonPrettyPrint(diddoc));
            Console.WriteLine();


            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
    }
}
