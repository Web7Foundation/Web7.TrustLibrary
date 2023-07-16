using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Web7.TrustLibrary.Base;
using Web7.TrustLibrary.Did;
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
            Message message = new Message(
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
            message.attachments.Add(a);

            string messageJson = JsonSerializer.Serialize<Message>(message);
            Console.WriteLine( "0. messageJson: " + messageJson );
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

            // 3. Export keys as JWKs
            JsonWebKey signerKeyPrivateJWK = signer.KeyPrivateSecurityKeyToJsonWebKey();
            string signerKeyPrivateJWKString = Helper.JsonWebKeyToString(signerKeyPrivateJWK);
            Console.WriteLine("3. signerKeyPrivateJWKString: " + signerKeyPrivateJWKString);
            JsonWebKey signerKeyPublicJWK = signer.KeyPublicSecurityKeyToJsonWebKey();
            string signerKeyPublicJWKString = Helper.JsonWebKeyToString(signerKeyPublicJWK);
            Console.WriteLine("3. signerKeyPublicJWKString: " + signerKeyPublicJWKString);
            Console.WriteLine();

            // 4. Import keys from JWKs
            Signer signerKeyPrivateFromJWK = new Signer(signerKeyPrivateJWKString, true);
            Signer signerKeyPublicFromJWK = new Signer(signerKeyPublicJWKString, false);
            Console.WriteLine("4. Signer key import from JWK strings");
            Console.WriteLine();

            // 5. Generate and verify signature for a string using imported JWKs
            byte[] signatureJWK = signer.SignText(plaintext);
            bool verifyJWK = signer.VerifyTextHash(plaintext, signatureJWK);
            Console.WriteLine("5. VerifyTextHashJWK: " + verifyJWK.ToString());
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
            byte[] hash64decoded = Helper.Base64Decode64(hash64);
            string hash64copy = Helper.Base64Encode(hash64decoded);
            Console.WriteLine("8. Hash64:     " + hash64);
            Console.WriteLine("8. Hash64Copy: " + hash64copy);
            Console.WriteLine();

            // 9. Create and validate a JWE messageJWE
            JWEMessagePacker messagePacker = new JWEMessagePacker(Helper.DID_ALICE, signer, Helper.DID_BOB, encrypter);
            string messageJWE = messagePacker.CreateJWEMessage(messageJson);
            Console.WriteLine("9. MessageJWE: " + messageJWE);
            TokenValidationResult result = messagePacker.ValidateJWEMessage(messageJWE);
            Console.WriteLine("9. IsValid: " + result.IsValid.ToString());
            string token = result.SecurityToken.ToString();
            Console.WriteLine("9. SecurityToken: " + token);
            string[] tokenparts = token.Split('.');
            // https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.base64urlencoder?view=msal-web-dotnet-latest
            string spart = Base64UrlEncoder.Decode(tokenparts[0]);
            Console.WriteLine("9: JOSE Header: " + spart);
            int index = 0;
            foreach (string part in tokenparts)
            {
                Console.WriteLine("9. " + index.ToString() + ": " + part);
                index++;
            }
            string messageJson2 = result.Claims[Helper.CLAIM_MESSAGE].ToString();
            Console.WriteLine("9: CLAIM_MESSAGE: " + messageJson2);
            Message message2 = JsonSerializer.Deserialize<Message>(messageJson);
            string plaintext2 = Helper.Base64Decode64ToString(message2.body);
            Console.WriteLine("9: plaintext2: " + plaintext2);
            Console.WriteLine();

            // 10. Create a DID Document
            Did.ServiceMap service0 = new Did.ServiceMap(null, "#default", new List<string>() { "default" }, new List<string>() { "http://localhost:8080" } );
            Did.ServiceMap relationship0 = new Did.ServiceMap(null, "#verifiers-accepted", new List<string>() { "verifiers-accepted" }, new List<string>() { "did:web7:verifier:1111" });
            DIDDocumenter diddocer = new DIDDocumenter(Helper.DID_ALICE, new List<string> { Helper.DID_ALICE }, signer, encrypter,
                new List<Did.ServiceMap>() { service0 },
                new List<Did.ServiceMap>() { relationship0 } );
            DIDDocument diddoc = diddocer.DidDocument;
            string diddocJson = diddoc.ToJson();
            Console.WriteLine("10. Serialized DID Document: " + Helper.JsonPrettyPrint(diddocJson));
            Console.WriteLine();

            // 11. Deserialize DID Document from Json
            DIDDocumenter diddocer2 = new DIDDocumenter(diddocJson);
            DIDDocument diddoc2 = diddocer2.DidDocument;
            string diddocJson2 = diddoc2.ToJson();
            Console.WriteLine("11. Deserialized DID Document: " + Helper.JsonPrettyPrint(diddocJson2));
            Console.WriteLine();

            // 12. Deserialize Signer public key from deserialized DID Document
            JsonWebKeyDotnet6 signerKeyPublicJsonWebKey6 = (JsonWebKeyDotnet6)diddoc2.verificationMethod[0].keyPublicJsonWebKey;
            JsonWebKey signerKeyPublicJsonWebKey = signerKeyPublicJsonWebKey6.ToJsonWebKey();
            Signer signerPublicKey2 = new Signer(signerKeyPublicJsonWebKey, false);
            Console.WriteLine("12. Deserialize Signer public key from deserialized DID Document");
            Console.WriteLine();

            // 13. Generate and verify signature for a string
            byte[] signature2 = signer.SignText(plaintext); // need a Signer with a private key
            bool verify2 = signerPublicKey2.VerifyTextHash(plaintext, signature2);
            Console.WriteLine("13. VerifyTextHash: " + verify2.ToString());
            Console.WriteLine();

            // 14. Deserialize Encrypter public key from deserialized DID Document
            JsonWebKeyDotnet6 encrypterKeyPublicJsonWebKey6 = (JsonWebKeyDotnet6)diddoc2.keyAgreement[0].keyPublicJsonWebKey;
            JsonWebKey encrypterKeyPublicJsonWebKey = encrypterKeyPublicJsonWebKey6.ToJsonWebKey();
            Encrypter encrypterPublicKey2 = new Encrypter(encrypterKeyPublicJsonWebKey, false);
            Console.WriteLine("14. Deserialize Encrypter public key from deserialized DID Document");
            Console.WriteLine();

            // 15. Encrypt and decrypt a string
            byte[] bytesEncrypted2 = encrypterPublicKey2.Encrypt(plaintextbytes);
            byte[] bytesDecrypted2 = encrypter.Decrypt(bytesEncrypted2); // need an Encrypter with private key
            string stringDecrypted2 = Encoding.UTF8.GetString(bytesDecrypted2);
            Console.WriteLine("15. String encrypted/decrypted: " + stringDecrypted2);
            Console.WriteLine();

            // 16. Create a DIDComm Envelope (for use HTTPTransporter in Web7.TRustedPersonalAgent app)
            Envelope envelope = new Envelope(Helper.DID_ALICE, Helper.DID_BOB, "http://localhost:8080", messageJWE);
            Console.WriteLine("16. Envelope: " + JsonSerializer.Serialize<Envelope>(envelope));
            Console.WriteLine();

            // 19. MasterKeyMaker
            MasterKeyMaker mkm = new MasterKeyMaker();
            string masterPassphrase = "Hello world!";
            byte[] masterKey = mkm.RandomMasterKey(masterPassphrase);
            string wordString = mkm.WordString;
            Console.WriteLine("19. WordString: " + wordString);
            masterKey = mkm.RandomMasterKey(masterPassphrase);
            wordString = mkm.WordString;
            Console.WriteLine("19. WordString: " + wordString);

            // 20. Test Secret Keys SymEncrypter
            SymEncrypter syme = new SymEncrypter(masterPassphrase, masterKey);
            byte[] encryptedBytes = syme.Encrypt(plaintextbytes);
            SymEncrypter symd = new SymEncrypter(masterPassphrase, masterKey);
            byte[] decryptedBytes = symd.Decrypt(encryptedBytes);
            string t1 = Encoding.UTF8.GetString(decryptedBytes);
            Console.WriteLine("20. String: " + t1);

            masterKey = mkm.MakeMasterKey(masterPassphrase, mkm.WordString);
            wordString = mkm.WordString;
            Console.WriteLine("21. WordString: " + wordString);
            SymEncrypter sym = new SymEncrypter(masterPassphrase, masterKey);
            decryptedBytes = sym.Decrypt(encryptedBytes);
            string t2 = Encoding.UTF8.GetString(decryptedBytes);
            Console.WriteLine("21. String: " + t2);
            decryptedBytes = sym.Decrypt(encryptedBytes);
            string t3 = Encoding.UTF8.GetString(decryptedBytes);
            Console.WriteLine("22. String: " + t3);
            decryptedBytes = sym.Decrypt(encryptedBytes);
            string t4 = Encoding.UTF8.GetString(decryptedBytes);
            Console.WriteLine("22. String: " + t4);
            Console.WriteLine();

            byte[] b5 = sym.Decrypt(sym.Encrypt(plaintextbytes));
            string t5 = Encoding.UTF8.GetString(b5);
            Console.WriteLine("23. String: " + t5);
            string t6 = sym.DecryptFromString64(sym.EncryptToString64(plaintextbytes));
            Console.WriteLine("24. String: " + t6);
            string t7 = sym.DecryptFromString64(sym.EncryptToString64(plaintext));
            Console.WriteLine("25. String: " + t7);
            Console.WriteLine();

            // 26. Test Master Key Symcrypter
            SymEncrypter symMasterKey = new SymEncrypter(Helper.DID_ALICE, Encoding.UTF8.GetBytes("Password1!"));
            byte[] b5b = symMasterKey.Decrypt(symMasterKey.Encrypt(plaintextbytes));
            string t5b = Encoding.UTF8.GetString(b5b);
            Console.WriteLine("26. String: " + t5b);
            string t6b = symMasterKey.DecryptFromString64(symMasterKey.EncryptToString64(plaintextbytes));
            Console.WriteLine("27. String: " + t6b);
            string t7b = symMasterKey.DecryptFromString64(symMasterKey.EncryptToString64(plaintext));
            Console.WriteLine("28. String: " + t7b);

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
    }
}
