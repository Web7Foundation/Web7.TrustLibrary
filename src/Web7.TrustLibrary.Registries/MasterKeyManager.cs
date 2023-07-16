using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity;
using Web7.TrustLibrary.Base;

namespace Web7.TrustLibrary.Registries
{
    public class MasterKeyInfo
    {
        public MasterKeyInfo(string subjectID, byte[] masterKey, long enscryptedubjectKeysCellID) 
        {
            this.subjectID = subjectID;
            this.masterKey = masterKey;
            this.enscryptedSubjectKeysCellID = enscryptedubjectKeysCellID;
        }

        string subjectID;
        byte[] masterKey;
        long enscryptedSubjectKeysCellID;

        public string SubjectID { get => subjectID; }
        public byte[] MasterKey { get => masterKey; }
        public long EncryptedSubjectKeysCellID { get => enscryptedSubjectKeysCellID; }
    }

    public static class MasterKeyManager
    {
        public static MasterKeyInfo LoadMasterKeyEntry(string subjectID, string subjectPassphrase)
        {
            MasterKeyInfo masterKeyInfo;

            KeyManagerRegistry_Cell kmRegistryCell = LoadKeyManagerRegistryCell();
            int found = -1;
            for (int entry = 0; entry < kmRegistryCell.reg.registry.Count; entry++)
            {
                if (kmRegistryCell.reg.registry[entry].subjectID == subjectID)
                {
                    found = entry;
                    break;
                }
            }
            if (found == -1)
            {
                masterKeyInfo = null;
            }
            else
            {
                SymEncrypter symMasterKey = new SymEncrypter(subjectID, Encoding.UTF8.GetBytes(subjectPassphrase));
                byte[] masterKey = symMasterKey.DecryptFromString64ToBytes(kmRegistryCell.reg.registry[found].encryptedMasterKey64);
                masterKeyInfo = new MasterKeyInfo(subjectID, masterKey, kmRegistryCell.reg.registry[found].encryptedSubjectKeysCellID);
            }

            return masterKeyInfo;
        }

        public static void SaveMasterKey(string subjectID, string subjectPassphrase, byte[] masterKey)
        {
            SymEncrypter symMasterKey = new SymEncrypter(subjectID, Encoding.UTF8.GetBytes(subjectPassphrase));
            string encryptedMasterKey64 = symMasterKey.EncryptToString64(masterKey);

            KeyManagerRegistry_Cell kmRegistryCell = LoadKeyManagerRegistryCell();
            int found = -1;
            for(int entry = 0; entry < kmRegistryCell.reg.registry.Count; entry++)
            {
                if (kmRegistryCell.reg.registry[entry].subjectID == subjectID)
                {
                    found = entry;
                    break;
                }
            }
            if (found == -1)
            {
                // Create dummy keyManagerSubjectKeysEncrypted_Cell to ensure KeyManagerMasterKeyEntry has a cell ID
                KeyManagerSubjectKeysEncrypted keyManagerSubjectKeysEncrypted = new KeyManagerSubjectKeysEncrypted();
                keyManagerSubjectKeysEncrypted.hasValues = false;
                KeyManagerSubjectKeysEncrypted_Cell keyManagerSubjectKeysEncrypted_Cell = new KeyManagerSubjectKeysEncrypted_Cell(keyManagerSubjectKeysEncrypted);
                Global.LocalStorage.SaveKeyManagerSubjectKeysEncrypted_Cell(keyManagerSubjectKeysEncrypted_Cell);

                kmRegistryCell.reg.registry.Add(new KeyManagerMasterKeyEntry(subjectID, encryptedMasterKey64, keyManagerSubjectKeysEncrypted_Cell.CellId));
            }
            else
            {
                kmRegistryCell.reg.registry[found] = new KeyManagerMasterKeyEntry(subjectID, encryptedMasterKey64, kmRegistryCell.reg.registry[found].encryptedSubjectKeysCellID);
            }
            
            SaveKeyManagerRegistryCell(kmRegistryCell);
        }

        private static KeyManagerRegistry_Cell LoadKeyManagerRegistryCell()
        {
            KeyManagerRegistry_Cell kmRegistryCell;

            var celltype = Global.LocalStorage.GetCellType(Helper.ROOT_CELLID);
            ulong cellcount = Global.LocalStorage.CellCount;
            Console.WriteLine(">>>> cellid: " + Helper.ROOT_CELLID.ToString() + " celltype: " + celltype.ToString() + " cellcount: " + cellcount);

            if (Global.LocalStorage.IsKeyManagerRegistry_Cell(Helper.ROOT_CELLID))
            {
                kmRegistryCell = Global.LocalStorage.LoadKeyManagerRegistry_Cell(Helper.ROOT_CELLID);
            }
            else
            {
                KeyManagerRegistry reg = new KeyManagerRegistry(new List<KeyManagerMasterKeyEntry>() { });
                kmRegistryCell = new KeyManagerRegistry_Cell(reg);
                SaveKeyManagerRegistryCell(kmRegistryCell);
            }

            celltype = Global.LocalStorage.GetCellType(Helper.ROOT_CELLID);
            cellcount = Global.LocalStorage.CellCount;
            Console.WriteLine(">>>> cellid: " + Helper.ROOT_CELLID.ToString() + " celltype: " + celltype.ToString() + " cellcount: " + cellcount);

            return kmRegistryCell;
        }

        private static void SaveKeyManagerRegistryCell(KeyManagerRegistry_Cell kmRegistryCell)
        {
            kmRegistryCell.CellId = Helper.ROOT_CELLID;
            Global.LocalStorage.SaveKeyManagerRegistry_Cell(kmRegistryCell);
            Global.LocalStorage.SaveStorage();
        }
    }
}
