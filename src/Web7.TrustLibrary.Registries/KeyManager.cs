using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class KeyManager
    {
        string subjectID;
        string dpFolder;
        IDataProtectionProvider dpProvider;
        IDataProtector dpProtector;

        public KeyManager(string subjectID)
        {
            this.subjectID = subjectID;
            this.dpFolder = Path.Combine(System.Environment.GetEnvironmentVariable("LOCALAPPDATA"), "Web7\\KeyVault");
            this.dpProvider = DataProtectionProvider.Create(
                new DirectoryInfo(dpFolder),
                configuration => { configuration.SetApplicationName("Web7PTL"); }
            );
            this.dpProtector = dpProvider.CreateProtector(subjectID);
        }

        public void Save(Signer signer, Encrypter encrypter)
        {
            Save(new SubjectSecretKeys(signer, encrypter));
        }

        public void Save(SubjectSecretKeys dpKeys)
        {
            KeyManagerRegistry_Cell registryCell;
            if (Global.LocalStorage.IsKeyManagerRegistry_Cell(0))
            {
                registryCell = Global.LocalStorage.LoadKeyManagerRegistry_Cell(0);
            }
            else
            {
                KeyManagerRegistry registry = new KeyManagerRegistry(new List<KeyManagerRegistryEntry>() { });
                registryCell = new KeyManagerRegistry_Cell(registry);
                registryCell.CellId = 0;
                Global.LocalStorage.SaveKeyManagerRegistry_Cell(registryCell);
            }
            KeyManagerRegistry reg = registryCell.reg;

            KeyManagerRegistryEntry entryFound = new KeyManagerRegistryEntry();
            foreach (KeyManagerRegistryEntry entry in reg.registry)
            {
                if (entry.id == subjectID)
                {
                    entryFound = entry;
                    break;
                }
            }

            KeyManagerDataProtectedKeys kmdpKeys = new KeyManagerDataProtectedKeys(
                dpProtector.Protect(JsonSerializer.Serialize(dpKeys.SignerKeyPrivateJWK)),
                dpProtector.Protect(JsonSerializer.Serialize(dpKeys.SignerKeyPublicJWK)),
                dpProtector.Protect(JsonSerializer.Serialize(dpKeys.EncrypterKeyPrivateJWK)),
                dpProtector.Protect(JsonSerializer.Serialize(dpKeys.EncrypterKeyPublicJWK))
            );

            if (entryFound.id == subjectID)
            {
                KeyManagerDataProtectedKeys_Cell kmdpKeysCell = Global.LocalStorage.LoadKeyManagerDataProtectedKeys_Cell(entryFound.cellid);
                kmdpKeysCell.dpKeys = kmdpKeys;
                Global.LocalStorage.SaveKeyManagerDataProtectedKeys_Cell(kmdpKeysCell);
            }
            else
            {
                KeyManagerDataProtectedKeys_Cell kmdpKeysCell = new KeyManagerDataProtectedKeys_Cell(kmdpKeys);
                Global.LocalStorage.SaveKeyManagerDataProtectedKeys_Cell(kmdpKeysCell);

                KeyManagerRegistryEntry entryNew = new KeyManagerRegistryEntry(subjectID, kmdpKeysCell.CellId);
                reg.registry.Add(entryNew);
                Global.LocalStorage.SaveKeyManagerRegistry_Cell(registryCell);
            }

            Global.LocalStorage.SaveStorage();
            var celltype = Global.LocalStorage.GetCellType(registryCell.CellId);
            ulong cellcount = Global.LocalStorage.CellCount;
            Console.WriteLine(">>>> cellid: " + registryCell.CellId.ToString() + " celltype: " + celltype.ToString() + " cellcount: " + cellcount);
        }

        public SubjectSecretKeys Load()
        {
            KeyManagerDataProtectedKeys_Cell kmdpKeysCell = Global.LocalStorage.LoadKeyManagerDataProtectedKeys_Cell(0);

            SubjectSecretKeys dpKeys = new SubjectSecretKeys();
            dpKeys.SignerKeyPrivateJWK = new JsonWebKey(dpProtector.Unprotect(kmdpKeysCell.dpKeys.signerKeyPrivateJWKDP));
            dpKeys.SignerKeyPublicJWK = new JsonWebKey(dpProtector.Unprotect(kmdpKeysCell.dpKeys.signerKeyPublicJWKDP));
            dpKeys.EncrypterKeyPrivateJWK = new JsonWebKey(dpProtector.Unprotect(kmdpKeysCell.dpKeys.encrypterKeyPrivateJWKDP));
            dpKeys.EncrypterKeyPublicJWK = new JsonWebKey(dpProtector.Unprotect(kmdpKeysCell.dpKeys.encrypterKeyPublicJWKDP));

            return dpKeys;
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
