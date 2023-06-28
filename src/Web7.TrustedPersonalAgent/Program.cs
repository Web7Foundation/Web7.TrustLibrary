using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Trinity;
using Web7.TrustLibrary.Base;
using Web7.TrustLibrary.Did.DIDComm;
using Web7.TrustLibrary.Transports;

namespace Web7.TrustedPersonalAgent
{
    internal class Program
    {
        const int masterPort = 8080;
        const string STORAGE_ROOT = "c:\\temp\\DIDMaster\\storage";

        static void Main(string[] args)
        {
            Trinity.TrinityConfig.HttpPort = masterPort;
#pragma warning disable CS0612 // Type or member is obsolete
            TrinityConfig.ServerPort = TrinityConfig.HttpPort + 1;
#pragma warning restore CS0612 // Type or member is obsolete
            TrinityConfig.StorageRoot = STORAGE_ROOT + "-" + TrinityConfig.HttpPort.ToString();

            DIDCommAgentImplementation agent = new DIDCommAgentImplementation();
            agent.Start();

            Signer signer = new Signer();
            Encrypter encrypter = new Encrypter();

            SendTestMessage(Helper.DID_ALICE, signer, Helper.DID_BOB, encrypter);

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
            agent.Stop();
        }

        public static void SendTestMessage(string signerID, Signer signer, string encrypterID, Encrypter encrypter)
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
            Console.WriteLine("0. msgJson: " + msgJson);
            Console.WriteLine();

            JWETokenizer jwter = new JWETokenizer(signerID, signer, encrypterID, encrypter);
            string token = jwter.CreateJWEToken(msgJson);
            Console.WriteLine("9. Token: " + token);

            // 16. Create a DIDComm Envelope (for use HTTPTransporter in Web7.TrustedPersonalAgent app)
            Envelope envelope = new Envelope(signerID, encrypterID, 
                "http://localhost:" + Trinity.TrinityConfig.HttpPort + "/DIDCommEndpoint/", 
                token);
            Console.WriteLine("16. Envelope: " + JsonSerializer.Serialize<Envelope>(envelope));
            Console.WriteLine();

            // 17. Use HTTPTransporter to send the message
            HTTPTransporter client = new HTTPTransporter();
            client.SendDIDCommEnvelope(envelope);
        }
    }
}