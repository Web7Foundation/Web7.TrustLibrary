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

namespace Web7.DIDNetworkRelayAgent
{
    public class MessageProcessor : IMessageProcessor
    {
        public const string MESSAGE_HELLO = "https://example.org/example/1.0/hello";
        public const string MESSAGE_RELAYPUSH = "https://example.org/example/1.0/relaypush";
        public const string MESSAGE_RELAYPOP = "https://example.org/example/1.0/relaypop";
        public const string MESSAGE_RELAYQUERY = "https://example.org/example/1.0/relayquery";
        public const string MESSAGE_RELAYCLEAR = "https://example.org/example/1.0/relayclear";

        public Message AuthenticateMessage(Envelope envelope)
        {
            Message message = null;

            Console.WriteLine("18. Authenticating envelope from: " + envelope.SenderID);
            Console.WriteLine("18. Authenticating envelope to: " + envelope.ReceiverID);
            Console.WriteLine("18. Authenticating envelope sent to: " + envelope.ReceiverServiceEndpointUrl);

            string messageJWE = envelope.MessageJWE;
            JWEMessagePacker messagePacker = new JWEMessagePacker(
                envelope.SenderID, Program.SubjectCryptoActorsTable[envelope.SenderID].Signer,      
                envelope.ReceiverID, Program.SubjectCryptoActorsTable[envelope.ReceiverID].Encrypter); // TODO
            var result = messagePacker.ValidateJWEMessage(messageJWE);
            Console.WriteLine("19. ValidateJWEMessage(messageJWE) result: " + result.IsValid.ToString());
            if (result.IsValid) // authenticated
            {
                string messageJson = result.Claims[Helper.CLAIM_MESSAGE].ToString();
                Console.WriteLine("20: JWE CLAIM_MESSAGE: " + messageJson);
                message = JsonSerializer.Deserialize<Message>(messageJson);
            }

            return message;
        }

        public string ProcessMessage(Message message)
        {
            string response = ""; // for non-queued message requests
            string textAttachment = null;

            string messageBody = Helper.Base64Decode64ToString(message.body);
            Console.WriteLine("44. " + message.type + " " + messageBody);
            if (message.attachments.Count > 0)
            {
                AttachmentData ad = message.attachments[0].data;
                textAttachment = Helper.Base64Decode64ToString(ad.text64);
            }
            switch(message.type)
            {
                case MESSAGE_HELLO:
                    {
                        response = "ACK";
                        break;
                    }
                case MESSAGE_RELAYPUSH:
                    {
                        response = PushMessage();
                        break;
                    }
                case MESSAGE_RELAYPOP:
                    {
                        response = PopMessage();
                        break;
                    }
                case MESSAGE_RELAYQUERY:
                    {
                        response = Query();
                        break;
                    }
                case MESSAGE_RELAYCLEAR:
                    {
                        response = ClearMessages();
                        break;
                    }
            }
            
            return response;
        }

        string GetDIDDocumentAsJson(string subjectID) // TODO
        {
            System.Reflection.Assembly assembly = typeof(Program).Assembly;
            return Helper.GetTemplate(assembly, "Web7.DIDNetworkRelayAgent.resources.DIDDocument-sample1.json"); // TODO
        }

        string PushMessage()
        {
            return "OK";
        }

        string PopMessage()
        {
            return "OK";
        }

        string Query()
        {
            return "OK";
        }

        string ClearMessages()
        {
            return "OK";
        }
    }
}
