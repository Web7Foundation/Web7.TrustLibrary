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

namespace Web7.DIDRegistryGatewayAgent
{
    public class MessageSender : IMessageSender
    {
        public const string MESSAGE_GETDIDDOC = "https://example.org/example/1.0/getdiddoc";
        public const string MESSAGE_UPDATEDIDDOC = "https://example.org/example/1.0/updatediddoc";

        public enum PRESENCE_VALUES
        { 
            IDLE,
            BUSY,
            OFFLINE,
            HIDDEN
        };

        // https://www.thecodebuzz.com/using-httpclient-best-practices-and-anti-patterns/
        static readonly HTTPTransporter client = new HTTPTransporter();

        public void SendMessage(string signerID, Signer signer, string encrypterID, Encrypter encrypter, string messageType, string body)
        {
            SendMessage(signerID, signer, encrypterID, encrypter, messageType, body, new List<Attachment>());
        }

        public void SendMessage(string signerID, Signer signer, string encrypterID, Encrypter encrypter, string messageType, string body, List<Attachment> attachments)
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

            foreach (string toID in message.to)
            {
                JWEMessagePacker messagePacker = new JWEMessagePacker(
                message.from, Program.SubjectCryptoActorsTable[message.from].Signer,
                toID, Program.SubjectCryptoActorsTable[toID].Encrypter); // TODO
                string messageJWE = messagePacker.CreateJWEMessage(message);
                Console.WriteLine("9. MessageJWE: " + messageJWE);

                // 16. Create a DIDComm Envelope
                Envelope envelope = new Envelope(signerID, encrypterID,
                    "http://localhost:" + Trinity.TrinityConfig.HttpPort + "/DIDCommEndpoint/", messageJWE);
                Console.WriteLine("16. Envelope: " + envelope.ToJson());
                Console.WriteLine();

                // 17. Use HTTPTransporter to send the message
                client.SendDIDCommEnvelope(envelope);
            }
        }
    }
}
