using Microsoft.VisualBasic;
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

namespace Web7.TrustedPersonalAgent0
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
            agent.Start(new MessageSender0(), new MessageProcessor0());

            for (int i = 0; i < 10; i++)
            {
                agent.SendMessage(Helper.DID_ALICE, signer, Helper.DID_BOB, encrypter, MessageSender0.MESSAGE_HELLO,
                    plaintext + " " + DateTime.Now.ToString());
            }
            // ...or just create your own SendMessage() and call it directly...
            (new MessageSender0()).SendMessage(Helper.DID_ALICE, signer, Helper.DID_BOB, encrypter,
                    MessageSender0.MESSAGE_HELLO, plaintext + " last " + DateTime.Now.ToString());

            agent.ProcessMessageQueues();

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
            agent.Stop();
        }
    }

    public class MessageSender0 : IMessageSender
    {
        public const string MESSAGE_HELLO = "https://example.org/example/1.0/hello";

        // https://www.thecodebuzz.com/using-httpclient-best-practices-and-anti-patterns/
        static readonly HTTPTransporter client = new HTTPTransporter();

        public string SendMessage(string signerID, Signer signer, string encrypterID, Encrypter encrypter, string messageType, string body)
        {
            return SendMessage(signerID, signer, encrypterID, encrypter, messageType, body, new List<Attachment>());
        }

        public string SendMessage(string signerID, Signer signer, string encrypterID, Encrypter encrypter, string messageType, string body, List<Attachment> attachments)
        {
            // 0. DIDComm namespace
            DateTime now = DateTime.Now;
            Message message = new Message(
                Helper.DID_MESSAGEID + Guid.NewGuid().ToString(),
                messageType,
                Helper.DID_ALICE,
                new List<string>() { Helper.DID_BOB },
                Helper.DID_THID + Guid.NewGuid().ToString(),
                "",
                Helper.UNIX_time(now),
                Helper.UNIX_time(now.AddDays(30)),
                Helper.Base64Encode(body)
            );
            message.attachments = attachments;
            Console.WriteLine("0. messageJson: " + message.ToJson());
            Console.WriteLine();

            JWEMessagePacker messagePacker = new JWEMessagePacker(signerID, signer, encrypterID, encrypter);
            string messageJWE = messagePacker.CreateJWEMessage(message);
            Console.WriteLine("9. MessageJWE: " + messageJWE);

            // 16. Create a DIDComm Envelope 
            Envelope envelope = new Envelope(signerID, encrypterID, "http://localhost:" + Trinity.TrinityConfig.HttpPort + "/DIDCommEndpoint/", messageJWE);
            Console.WriteLine("16. Envelope: " + envelope.ToJson());
            Console.WriteLine();

            // 17. Use HTTPTransporter to send the message
            string response = client.SendDIDCommEnvelope(envelope);
            return response;
        }
    }

    public class MessageProcessor0 : IMessageProcessor
    {
        public Message AuthenticateMessage(Envelope envelope)
        {
            Message message = null;

            Console.WriteLine("18. Authenticating envelope from: " + envelope.SenderID);
            Console.WriteLine("18. Authenticating envelope to: " + envelope.ReceiverID);
            Console.WriteLine("18. Authenticating envelope sent to: " + envelope.ReceiverServiceEndpointUrl);

            string messageJWE = envelope.MessageJWE;
            JWEMessagePacker messagePacker = new JWEMessagePacker(Helper.DID_ALICE, Program.signer, Helper.DID_BOB, Program.encrypter);
            var result = messagePacker.ValidateJWEMessage(messageJWE);
            Console.WriteLine("9. ValidateJWEMessage(messageJWE) result: " + result.IsValid.ToString());
            if (result.IsValid) // authenticated
            {
                string messageJson = result.Claims[Helper.CLAIM_MESSAGE].ToString();
                Console.WriteLine("9: CLAIM_MESSAGE: " + messageJson);
                message = JsonSerializer.Deserialize<Message>(messageJson);
            }

            return message;
        }

        public string ProcessMessage(Message message)
        {
            string body = Helper.Base64Decode64ToString(message.body);
            Console.WriteLine("9: body: " + body);
            Console.WriteLine();
            return body;
        }
    }
}