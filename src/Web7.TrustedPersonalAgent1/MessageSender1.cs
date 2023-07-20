using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web7.TrustLibrary.Base;
using Web7.TrustLibrary.Did.DIDComm;
using Web7.TrustLibrary.Did;
using Web7.TrustLibrary.Transports;
using System.Text.Json;

namespace Web7.TrustedPersonalAgent1
{
    public class MessageSender1 : IMessageSender
    {
        public const string MESSAGE_HELLO = "https://example.org/example/1.0/hello";
        public const string MESSAGE_PRESENCE= "https://example.org/example/1.0/presence";
        public const string MESSAGE_SENDFILE = "https://example.org/example/1.0/sendfile";
        public enum PRESENCE_VALUES
        { 
            IDLE,
            BUSY,
            OFFLINE,
            HIDDEN
        };

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
                signerID,
                new List<string>() { encrypterID },
                Helper.DID_THID + Guid.NewGuid().ToString(),
                "",
                Helper.UNIX_time(now),
                Helper.UNIX_time(now.AddDays(30)),
                Helper.Base64Encode(body)
            );
            message.attachments = attachments;
            Console.WriteLine("0. msgJson: " + message.ToJson());
            Console.WriteLine();

            JWEMessagePacker messagePacker = new JWEMessagePacker(message.from, signer, message.to[0], encrypter); // TODO
            string messageJWE = messagePacker.CreateJWEMessage(message);
            Console.WriteLine("9. MessageJWE: " + messageJWE);

            // 16. Create a DIDComm Envelope
            Envelope envelope = new Envelope(signerID, encrypterID,
                "http://localhost:" + Trinity.TrinityConfig.HttpPort + "/DIDCommEndpoint/", messageJWE);
            Console.WriteLine("16. Envelope: " + envelope.ToJson());
            Console.WriteLine();

            // 17. Use HTTPTransporter to send the message
            string response = client.SendDIDCommEnvelope(envelope);
            return response;
        }
    }
}
