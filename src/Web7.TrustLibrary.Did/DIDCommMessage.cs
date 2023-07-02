using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Web7.TrustLibrary.Did.DIDComm
{
    // The DIDComm Message classes are used to create and serialize an in-memory Web 7.0 DIDComm Message with (or without) a DIDComm Attachment.
    // In addition, the DIDComm class can create authenticated encrypted messages by internally using the JWEMessagePacker class.
    // The HTTPTransporter class is used to transport a DIDCommMessage from a Sender agent's outbound service endpoint to a Receiver
    // agent's inbound service endpoint using the HTTP protocol.
    // Keywords: Secure-Messaging Authenticity Confidentiality DID DID-Communications DIDComm-Messaging
    public class AttachmentData
    {
        public string jws { get; set; }
        public string hash { get; set; }
        public string links { get; set; }
        public string base64 { get; set; }
        public string json { get; set; }

        public AttachmentData(string jws, string hash, string links, string base64, string json)
        {
            this.jws = jws;
            this.hash = hash;
            this.links = links;
            this.base64 = base64;
            this.json = json;
        }
    }

    public class Attachment
    {
        public string id { get; set; }
        public string description { get; set; }
        public string filename { get; set; }
        public string media_type { get; set; }
        public string format { get; set; }
        public long lastmod_time { get; set; }
        public AttachmentData data { get; set; }
        public long byte_count { get; set; }

        public Attachment(string id, string description, string filename, string media_type, string format, long lastmod_time, AttachmentData data, long byte_count)
        {
            this.id = id;
            this.description = description;
            this.filename = filename;
            this.media_type = media_type;
            this.format = format;
            this.lastmod_time = lastmod_time;
            this.data = data;
            this.byte_count = byte_count;
        }
    }

    public class Message
    {
        public string id { get; set; } // required
        public string type { get; set; } // required
        public List<string> to { get; set; }
        public string from { get; set; }
        public string thid { get; set; }
        public string pthid { get; set; }
        public long created_time { get; set; }
        public long expires_time { get; set; }
        public string body { get; set; }
        public List<Attachment> attachments { get; set; }

        public Message()
        {
            this.to = new List<string>();
            this.attachments = new List<Attachment>();
        }

        public Message(string id, string type, string from, List<string> to, string thid, string pthid, long created_time, long expires_time, string body)
        {
            this.id = id;
            this.type = type;
            this.to = to;
            this.from = from;
            this.thid = thid;
            this.pthid = pthid;
            this.created_time = created_time;
            this.expires_time = expires_time;
            this.body = body;
            this.attachments = new List<Attachment>();
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize<Message>(this);
        }
    }

    public class Envelope
    {
        private string senderID;
        private string receiverID;
        private string receiverServiceEndpointUrl;
        private string messageJWE;

        public string SenderID { get => senderID; set => senderID = value; }
        public string ReceiverID { get => receiverID; set => receiverID = value; }
        public string ReceiverServiceEndpointUrl { get => receiverServiceEndpointUrl; set => receiverServiceEndpointUrl = value; }
        public string MessageJWE { get => messageJWE; set => messageJWE = value; }

        public Envelope(string senderID, string receiverID, string receiverUrl, string messageJWE)
        {
            this.senderID = senderID;
            this.receiverID = receiverID;
            this.receiverServiceEndpointUrl = receiverUrl;
            this.messageJWE = messageJWE;
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize<Envelope>(this);
        }
    }
}
