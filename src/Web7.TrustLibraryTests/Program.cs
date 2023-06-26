using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Web7.TrustLibrary;
using Web7.TrustLibrary.DIDComm;


namespace Web7.TrustLibrary
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
            Hasher hasher = new Hasher(signer);
            byte[] signature = hasher.SignText(plaintext);
            bool verify = hasher.VerifyTextHash(plaintext, signature);
            Console.WriteLine("2. VerifyTextHash: " + verify.ToString());
            Console.WriteLine();

            // 3. Export keys as JWTs
            JsonWebKey signerKeyPrivateJWT = signer.KeyPrivateJsonWebKey();
            string signerKeyPrivateJWTString = signer.KeyPrivateJsonWebKeyToString(signerKeyPrivateJWT);
            Console.WriteLine("3. signerKeyPrivateJWTString: " + signerKeyPrivateJWTString);
            JsonWebKey signerKeyPublicJWT = signer.KeyPublicJsonWebKey();
            string signerKeyPublicJWTString = signer.KeyPublicJsonWebKeyToString(signerKeyPublicJWT);
            Console.WriteLine("3. signerKeyPublicJWTString: " + signerKeyPublicJWTString);
            Console.WriteLine();

            // 4. Import keys from JWTs
            Signer signerKeyPrivateFromJWT = new Signer(signerKeyPrivateJWTString, true);
            Signer signerKeyPublicFromJWT = new Signer(signerKeyPublicJWTString, false);
            Console.WriteLine("4. Signer key import from JWT strings");
            Console.WriteLine();

            // 5. Generate and verify signature for a string using imported JWTs
            Hasher hasherJWT = new Hasher(signer);
            byte[] signatureJWT = hasherJWT.SignText(plaintext);
            bool verifyJWT = hasherJWT.VerifyTextHash(plaintext, signatureJWT);
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

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
    }
}
