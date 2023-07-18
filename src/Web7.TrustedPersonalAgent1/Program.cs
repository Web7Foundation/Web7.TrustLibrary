using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading;
using Trinity;
using Web7.TrustLibrary.Base;
using Web7.TrustLibrary.Did;
using Web7.TrustLibrary.Did.DIDComm;
using Web7.TrustLibrary.Registries;
using Web7.TrustLibrary.Transports;

namespace Web7.TrustedPersonalAgent1
{
    internal class Program
    {
        const int masterPort = 8080;
        const string STORAGE_ROOT = "c:\\temp\\DIDMaster\\storage";
        const string plaintext = "{ \"message\": \"Hello world!\" }";

        public static SubjectSecretKeys secretKeysAlice = null;
        public static SubjectSecretKeys secretKeysBob = null;
        public static Signer signerAlice = null;
        public static Encrypter encrypterAlice = null;
        public static Signer signerBob = null;
        public static Encrypter encrypterBob = null;

        const string WORDSTRING_ALICE = "afford frequent topple wire escape party citizen will stem cream whisper install";
        const string WORDSTRING_BOB = "frost youth hub gauge legend slender fuel torch label floor trouble gather";
        public static string PASSPHRASE_ALICE = "AlicePassword1!";
        public static string PASSPHRASE_BOB = "BobPassword1!";
        public static string MASTER_PASSPHRASE_ALICE = "AliceMasterPassword1!";
        public static string MASTER_PASSPHRASE_BOB = "BobMasterPassword1!";

        static void Main(string[] args)
        {
            Trinity.TrinityConfig.HttpPort = masterPort;
#pragma warning disable CS0612 // Type or member is obsolete
            TrinityConfig.ServerPort = TrinityConfig.HttpPort + 1;
#pragma warning restore CS0612 // Type or member is obsolete
            TrinityConfig.StorageRoot = STORAGE_ROOT + "-" + TrinityConfig.HttpPort.ToString();

            Global.LocalStorage.LoadStorage();
            //Global.LocalStorage.ResetStorage();
            //Global.LocalStorage.SaveStorage();
            //Global.LocalStorage.LoadStorage();

            MasterKeyInfo masterKeyInfoAlice = MasterKeyManager.LoadMasterKeyEntry(Helper.DID_ALICE, PASSPHRASE_ALICE);
            if (masterKeyInfoAlice == null)
            {
                MasterKeyMaker mkm = new MasterKeyMaker();
                byte[] masterKeyAlice = mkm.RandomMasterKey(MASTER_PASSPHRASE_ALICE);
                Console.WriteLine("New wordString: " + mkm.WordString);
                MasterKeyManager.SaveMasterKey(Helper.DID_ALICE, PASSPHRASE_ALICE, masterKeyAlice);
                masterKeyInfoAlice = MasterKeyManager.LoadMasterKeyEntry(Helper.DID_ALICE, PASSPHRASE_ALICE);
            }
            MasterKeyInfo masterKeyInfoBob = MasterKeyManager.LoadMasterKeyEntry(Helper.DID_BOB, PASSPHRASE_BOB);
            if (masterKeyInfoBob == null)
            {
                MasterKeyMaker mkm = new MasterKeyMaker();
                byte[] masterKeyBob = mkm.RandomMasterKey(MASTER_PASSPHRASE_BOB);
                Console.WriteLine("New wordString: " + mkm.WordString);
                MasterKeyManager.SaveMasterKey(Helper.DID_BOB, PASSPHRASE_BOB, masterKeyBob);
                masterKeyInfoBob = MasterKeyManager.LoadMasterKeyEntry(Helper.DID_BOB, PASSPHRASE_BOB);
            }

            SubjectKeyManager kmAlice = new SubjectKeyManager(Helper.DID_ALICE, "Password1!", masterKeyInfoAlice, "Evlewt12!");
            if ((secretKeysAlice = kmAlice.LoadSubjectSecretKeys()) == null)
            {
                signerAlice = new Signer();
                encrypterAlice = new Encrypter();
                secretKeysAlice = new SubjectSecretKeys(signerAlice, encrypterAlice);
                kmAlice.SaveSubjectSecretKeys(secretKeysAlice);
            }
            else
            {
                signerAlice = new Signer(secretKeysAlice.SignerKeyPrivateJWK, true);
                encrypterAlice = new Encrypter(secretKeysAlice.EncrypterKeyPrivateJWK, true);
            }

            SubjectKeyManager kmBob = new SubjectKeyManager(Helper.DID_BOB, "Password1!", masterKeyInfoBob, "Evlewt12!");
            if ((secretKeysBob = kmBob.LoadSubjectSecretKeys()) == null)
            {
                signerBob = new Signer();
                encrypterBob = new Encrypter();
                secretKeysBob = new SubjectSecretKeys(signerBob, encrypterBob);
                kmBob.SaveSubjectSecretKeys(secretKeysBob);
            }
            else
            {
                signerBob = new Signer(secretKeysBob.SignerKeyPrivateJWK, true);
                encrypterBob = new Encrypter(secretKeysBob.EncrypterKeyPrivateJWK, true);
            }
                
            DIDCommAgentImplementation agent = new DIDCommAgentImplementation();
            agent.Start(new MessageSender1(), new MessageProcessor1());

            for (int i = 0; i < 10; i++)
            {
                agent.SendMessage(Helper.DID_ALICE, signerAlice, Helper.DID_BOB, encrypterBob, MessageSender1.MESSAGE_HELLO,
                    i.ToString() + ": " + plaintext + " " + DateTime.Now.ToString());
            }

            agent.SendMessage(Helper.DID_ALICE, signerAlice, Helper.DID_BOB, encrypterBob, MessageSender1.MESSAGE_PRESENCE, MessageSender1.PRESENCE_VALUES.BUSY.ToString());

            // Send a Message with an Attachment
            string text64 = Helper.Base64Encode("Foo bar!");
            AttachmentData attachmentData = new AttachmentData("", "", "", text64, "");
            Attachment attachment = new Attachment(
                Helper.DID_ATTACHMENTID + Guid.NewGuid().ToString(),
                "Attachment abc",
                "abc.txt",
                "text/plain",
                "",
                Helper.UNIX_time(DateTime.Now),
                attachmentData,
                0
            );
            // One off message
            (new MessageSender1()).SendMessage(Helper.DID_ALICE, signerAlice, Helper.DID_BOB, encrypterAlice, 
                MessageSender1.MESSAGE_HELLO, plaintext + " last " + DateTime.Now.ToString(),
                new List<Attachment> { attachment });

            agent.ProcessMessageQueues();

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
            agent.Stop();
        }
    }
}