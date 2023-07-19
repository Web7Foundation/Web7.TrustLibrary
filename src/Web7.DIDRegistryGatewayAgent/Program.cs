using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Trinity;
using Web7.TrustLibrary.Base;
using Web7.TrustLibrary.Did;
using Web7.TrustLibrary.Did.DIDComm;
using Web7.TrustLibrary.Did.DIDDocumemt;
using Web7.TrustLibrary.Transports;

namespace Web7.DIDRegistryGatewayAgent
{
    internal class Program
    {
        const int masterPort = 8888;
        const string STORAGE_ROOT = "c:\\temp\\DIDMaster\\storage";

        public static Dictionary<string, SubjectCryptoActors> SubjectCryptoActorsTable = new Dictionary<string, SubjectCryptoActors>();

        static void Main(string[] args)
        {
            Trinity.TrinityConfig.HttpPort = masterPort;
#pragma warning disable CS0612 // Type or member is obsolete
            TrinityConfig.ServerPort = TrinityConfig.HttpPort + 1;
#pragma warning restore CS0612 // Type or member is obsolete
            TrinityConfig.StorageRoot = STORAGE_ROOT + "-" + TrinityConfig.HttpPort.ToString();

            Global.LocalStorage.LoadStorage();

            Signer signerAlice = new Signer();
            Encrypter encrypterDIDRegistryAgent = new Encrypter();
            SubjectCryptoActorsTable.Add(Helper.DID_ALICE, new SubjectCryptoActors( signerAlice, null ));
            SubjectCryptoActorsTable.Add(Helper.DID_DIDREGISTRYAGENT2222, new SubjectCryptoActors(null, encrypterDIDRegistryAgent));

            DIDCommAgentImplementation agent = new DIDCommAgentImplementation(false);
            agent.Start(new MessageSender(), new MessageProcessor());

            string response = agent.SendMessage(Helper.DID_ALICE, signerAlice, Helper.DID_DIDREGISTRYAGENT2222, encrypterDIDRegistryAgent, MessageProcessor.MESSAGE_GETDIDDOC, Helper.DID_BOB);
            Console.WriteLine("response: " + response);
            DIDDocument didDoc = new DIDDocument().FromJson(response);
            Console.WriteLine("didDoc: " + didDoc.ToJson());

            string text64 = Helper.Base64Encode(didDoc.ToJson());
            AttachmentData ad = new AttachmentData("", "", "", text64, "");
            Attachment didDocAttachment = new Attachment(
                                Helper.DID_ATTACHMENTID + Guid.NewGuid().ToString(),
                didDoc.comment,
                "diddocument.json",
                "application/json",
                "application/didcomm-encrypted+json",
                Helper.UNIX_time(DateTime.Now),
                ad,
                text64.Length
            );
            response = agent.SendMessage(Helper.DID_ALICE, signerAlice, Helper.DID_DIDREGISTRYAGENT2222, encrypterDIDRegistryAgent, MessageProcessor.MESSAGE_UPDATEDIDDOC, Helper.DID_BOB,
                new List<Attachment> { didDocAttachment } );
            Console.WriteLine("response: " + response);

            if (agent.QueueMessages)
            {
                agent.ProcessMessageQueues();
            }

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

            agent.Stop();
        }
    }
}