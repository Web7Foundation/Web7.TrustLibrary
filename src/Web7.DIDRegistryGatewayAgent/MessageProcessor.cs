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
    public class MessageProcessor : IMessageProcessor
    {
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

        static System.Reflection.Assembly assembly = typeof(Program).Assembly;

        public string ProcessMessage(Message message)
        {
            string response = ""; // for non-queued message requests
            string subjectID = Helper.Base64Decode64ToString(message.body);
            Console.WriteLine("44. " + message.type + " " + subjectID);
            if (message.attachments.Count > 0)
            {
                AttachmentData ad = message.attachments[0].data;
                string data = Helper.Base64Decode64ToString(ad.body64);
                Console.WriteLine("9: attachment: " + data);
            }
            response = GetDIDDocument(subjectID);
            
            return response;
        }

        string GetDIDDocument(string subjectID) // TODO
        {
            return Helper.GetTemplate(assembly, "Web7.DIDRegistryGatewayAgent.resources.DIDDocument-sample1.json");
        }
    }
}
