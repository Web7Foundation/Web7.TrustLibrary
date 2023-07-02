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
using Web7.TrustLibrary.Transports;

namespace Web7.TrustedPersonalAgent1
{
    internal class Program
    {
        const int masterPort = 8080;
        const string STORAGE_ROOT = "c:\\temp\\DIDMaster\\storage";
        const string plaintext = "{ \"message\": \"Hello world!\" }";
        
        public static Signer signer = new Signer();
        public static Encrypter encrypter = new Encrypter();

        static void Main(string[] args)
        {
            Trinity.TrinityConfig.HttpPort = masterPort;
#pragma warning disable CS0612 // Type or member is obsolete
            TrinityConfig.ServerPort = TrinityConfig.HttpPort + 1;
#pragma warning restore CS0612 // Type or member is obsolete
            TrinityConfig.StorageRoot = STORAGE_ROOT + "-" + TrinityConfig.HttpPort.ToString();

            Global.LocalStorage.ResetStorage();

            DIDCommAgentImplementation agent = new DIDCommAgentImplementation();
            agent.Start(new MessageSender1(), new MessageProcessor1());

            for (int i = 0; i < 10; i++)
            {
                agent.SendMessage(Helper.DID_ALICE, signer, Helper.DID_BOB, encrypter, MessageSender1.MESSAGE_HELLO,
                    plaintext + " " + DateTime.Now.ToString());
            }

            agent.SendMessage(Helper.DID_ALICE, signer, Helper.DID_BOB, encrypter, MessageSender1.MESSAGE_PRESENCE, MessageSender1.PRESENCE_VALUES.BUSY.ToString());

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
            (new MessageSender1()).SendMessage(Helper.DID_ALICE, signer, Helper.DID_BOB, encrypter, 
                MessageSender1.MESSAGE_HELLO, plaintext + " last " + DateTime.Now.ToString(),
                new List<Attachment> { attachment });

            agent.ProcessMessageQueues();

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
            agent.Stop();
        }
    }
}