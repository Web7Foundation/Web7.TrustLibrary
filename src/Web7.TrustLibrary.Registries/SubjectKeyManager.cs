using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Trinity;
using Web7.TrustLibrary.Base;

namespace Web7.TrustLibrary.Registries
{
    // The KeyManager class is used to manage a collection of Signer and Encrypter key pairs
    // stored locally on a device in a personal KeyVault.
    // Keywords: KeyManager KeyVault Key-Storage
    public class SubjectKeyManager
    {
        string subjectID;
        SymEncrypter symSecretKeys = null;
        byte[] masterKey = null;
        MasterKeyInfo masterKeyInfo;

        public SubjectKeyManager(string subjectID, string subjectPassphrase, MasterKeyInfo masterKeyInfo, string masterPassphrase) // Force creation of new Master Key
        {
            this.subjectID = subjectID;
            this.masterKeyInfo = masterKeyInfo;
            this.symSecretKeys = new SymEncrypter(masterPassphrase, masterKeyInfo.MasterKey);
        }

        public void SaveSubjectSecretKeys(Signer signer, Encrypter encrypter)
        {
            SaveSubjectSecretKeys(new SubjectSecretKeys(signer, encrypter));
        }

        public void SaveSubjectSecretKeys(SubjectSecretKeys subjectKeys)
        {
            KeyManagerSubjectKeysEncrypted kmEncryptedSubjectKeys = new KeyManagerSubjectKeysEncrypted();
            kmEncryptedSubjectKeys.hasValues = true;
            kmEncryptedSubjectKeys.signerKeyPrivateJWKEncrypted64 = symSecretKeys.EncryptToString64(JsonSerializer.Serialize(subjectKeys.SignerKeyPrivateJWK));
            kmEncryptedSubjectKeys.signerKeyPublicJWKEncrypted64 = symSecretKeys.EncryptToString64(JsonSerializer.Serialize(subjectKeys.SignerKeyPublicJWK));
            kmEncryptedSubjectKeys.encrypterKeyPrivateJWKEncrypted64 = symSecretKeys.EncryptToString64(JsonSerializer.Serialize(subjectKeys.EncrypterKeyPrivateJWK));
            kmEncryptedSubjectKeys.encrypterKeyPublicJWKEncrypted64 = symSecretKeys.EncryptToString64(JsonSerializer.Serialize(subjectKeys.EncrypterKeyPublicJWK));

            Console.WriteLine("Out64 Lengths: " + kmEncryptedSubjectKeys.signerKeyPrivateJWKEncrypted64.Length
                + " " + kmEncryptedSubjectKeys.signerKeyPublicJWKEncrypted64.Length
                + " " + kmEncryptedSubjectKeys.encrypterKeyPrivateJWKEncrypted64.Length
                + " " + kmEncryptedSubjectKeys.encrypterKeyPublicJWKEncrypted64.Length);

            if (masterKeyInfo.EncryptedSubjectKeysCellID == -1)
            {
                throw new ArgumentNullException();
            }

            KeyManagerSubjectKeysEncrypted_Cell kmSubjectKeysEncryptedCell = Global.LocalStorage.LoadKeyManagerSubjectKeysEncrypted_Cell(masterKeyInfo.EncryptedSubjectKeysCellID);
            kmSubjectKeysEncryptedCell.encryptedSubjectKeys = kmEncryptedSubjectKeys;
            Global.LocalStorage.SaveKeyManagerSubjectKeysEncrypted_Cell(kmSubjectKeysEncryptedCell);
            Global.LocalStorage.SaveStorage();

            var celltype = Global.LocalStorage.GetCellType(Helper.ROOT_CELLID);
            ulong cellcount = Global.LocalStorage.CellCount;
            Console.WriteLine(">>>> cellid: " + Helper.ROOT_CELLID.ToString() + " celltype: " + celltype.ToString() + " cellcount: " + cellcount);
        }

        public SubjectSecretKeys LoadSubjectSecretKeys()
        {
            SubjectSecretKeys subjectSecretKeys = null;

            if (masterKeyInfo.EncryptedSubjectKeysCellID == -1)
            {
                throw new ArgumentNullException();
            }

            KeyManagerSubjectKeysEncrypted_Cell kmSubjectKeysEncryptedCell = Global.LocalStorage.LoadKeyManagerSubjectKeysEncrypted_Cell(masterKeyInfo.EncryptedSubjectKeysCellID);

            if (kmSubjectKeysEncryptedCell.encryptedSubjectKeys.hasValues)
            { 
                Console.WriteLine("In64 Lengths: " + kmSubjectKeysEncryptedCell.encryptedSubjectKeys.signerKeyPrivateJWKEncrypted64.Length
                    + " " + kmSubjectKeysEncryptedCell.encryptedSubjectKeys.signerKeyPublicJWKEncrypted64.Length
                    + " " + kmSubjectKeysEncryptedCell.encryptedSubjectKeys.encrypterKeyPrivateJWKEncrypted64.Length
                    + " " + kmSubjectKeysEncryptedCell.encryptedSubjectKeys.encrypterKeyPublicJWKEncrypted64.Length);

                subjectSecretKeys = new SubjectSecretKeys();
                subjectSecretKeys.SignerKeyPrivateJWK =
                    new JsonWebKey(symSecretKeys.DecryptFromString64(kmSubjectKeysEncryptedCell.encryptedSubjectKeys.signerKeyPrivateJWKEncrypted64));
                subjectSecretKeys.SignerKeyPublicJWK =
                    new JsonWebKey(symSecretKeys.DecryptFromString64(kmSubjectKeysEncryptedCell.encryptedSubjectKeys.signerKeyPublicJWKEncrypted64));
                subjectSecretKeys.EncrypterKeyPrivateJWK =
                    new JsonWebKey(symSecretKeys.DecryptFromString64(kmSubjectKeysEncryptedCell.encryptedSubjectKeys.encrypterKeyPrivateJWKEncrypted64));
                subjectSecretKeys.EncrypterKeyPublicJWK =
                    new JsonWebKey(symSecretKeys.DecryptFromString64(kmSubjectKeysEncryptedCell.encryptedSubjectKeys.encrypterKeyPublicJWKEncrypted64));
            }

            return subjectSecretKeys;
        }

    }

    public class SubjectSecretKeys
    {
        JsonWebKey signerKeyPrivateJWK;
        JsonWebKey signerKeyPublicJWK;
        JsonWebKey encrypterKeyPrivateJWK;
        JsonWebKey encrypterKeyPublicJWK;

        public SubjectSecretKeys()
        {
            signerKeyPrivateJWK = null;
            signerKeyPublicJWK = null;
            encrypterKeyPrivateJWK = null;
            encrypterKeyPublicJWK = null;
        }

        public SubjectSecretKeys(Signer signer, Encrypter encrypter)
        {
            signerKeyPrivateJWK = signer.KeyPrivateSecurityKeyToJsonWebKey();
            signerKeyPublicJWK = signer.KeyPublicSecurityKeyToJsonWebKey();
            encrypterKeyPrivateJWK = encrypter.KeyPrivateSecurityKeyToJsonWebKey();
            encrypterKeyPublicJWK = encrypter.KeyPublicSecurityKeyToJsonWebKey();
        }

        public JsonWebKey SignerKeyPrivateJWK { get => signerKeyPrivateJWK; set => signerKeyPrivateJWK = value; }
        public JsonWebKey SignerKeyPublicJWK { get => signerKeyPublicJWK; set => signerKeyPublicJWK = value; }
        public JsonWebKey EncrypterKeyPrivateJWK { get => encrypterKeyPrivateJWK; set => encrypterKeyPrivateJWK = value; }
        public JsonWebKey EncrypterKeyPublicJWK { get => encrypterKeyPublicJWK; set => encrypterKeyPublicJWK = value; }
    }
}
