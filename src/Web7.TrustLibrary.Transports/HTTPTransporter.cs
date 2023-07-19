using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Web7.TrustLibrary.Did;
using Web7.TrustLibrary.Did.DIDComm;

namespace Web7.TrustLibrary.Transports
{
    // The HTTPTransporter class is used to transport a DIDCommMessage from a Sender agent's outbound service endpoint to a
    // Receiver agent's inbound service endpoint using the HTTP protocol.
    // The DIDComm class is used to create and serialize an in-memory Web 7.0 DIDComm Message with(or without) a DIDComm Attachment.
    // In addition the DIDComm class can create authenticated encrypted messages by internally using the JWEMessagePacker class.
    // Keywords: DIDComm HTTP Transport-Protocol
    public class HTTPTransporter
    {
        // https://www.thecodebuzz.com/using-httpclient-best-practices-and-anti-patterns/
        static readonly HttpClient httpClient = new HttpClient();

        public string SendDIDCommEnvelope(Envelope envelope)
        {
            DIDCommMessageEnvelope envDIDComm = new DIDCommMessageEnvelope(envelope.SenderID, envelope.ReceiverID, envelope.ReceiverServiceEndpointUrl, envelope.MessageJWE);
            DIDCommMessageRequest requestDIDComm = new DIDCommMessageRequest(envDIDComm);
            var task = Task.Run(() => SendHttpMessage(envelope.ReceiverServiceEndpointUrl, requestDIDComm.ToString()));
            var result = task.Result;
            DIDCommResponse responseDIDComm = JsonSerializer.Deserialize<DIDCommResponse>(result,
                new JsonSerializerOptions { IncludeFields = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
            string response = responseDIDComm.resp;
            return response;
        }

        private static string SendHttpMessage(string url, string jsonMessageRequest)
        {
            string jsonResponse = "";

            using (var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), url))
            {
                requestMessage.Headers.TryAddWithoutValidation("Accept", "application/json");
                requestMessage.Content = new StringContent(jsonMessageRequest);
                var task = httpClient.SendAsync(requestMessage);
                task.Wait();  // if exception is thrown here, you forgot to run Visual Studio in "Run as Administrator" mode
                var result = task.Result;
                jsonResponse = result.Content.ReadAsStringAsync().Result;
            }

            return jsonResponse;
        }
    }
}
