using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity;
using Web7.TrustLibrary.Base;
using Web7.TrustLibrary.Did.DIDComm;

namespace Web7.TrustLibrary.Transports
{
    public class DIDCommAgentImplementation : DIDCommAgentBase
    {
        ConcurrentDictionary<string, ConcurrentQueue<long>> queues = new ConcurrentDictionary<string, ConcurrentQueue<long>>();
        IMessageSender messageSender = null;
        IMessageProcessor messageProcessor = null;
        bool queueMessages = true;

        public DIDCommAgentImplementation()
        {
            Initialize(true);
        }

        public DIDCommAgentImplementation(bool queueMessages)
        {
            Initialize(queueMessages);
        }

        private void Initialize(bool queueMessages)
        {
            this.queueMessages = queueMessages;
        }

        public IMessageProcessor MessageProcessor { get => messageProcessor; set => messageProcessor = value; }
        public IMessageSender MessageSender { get => messageSender; set => messageSender = value; }
        public bool QueueMessages { get => queueMessages; }

        public void Start(IMessageSender messageSender, IMessageProcessor messageProcessor)
        {
            this.messageSender = messageSender;
            this.messageProcessor = messageProcessor;
            Start(); // Also call the default (inherited) Start() - else connections are refused
        }

        public string SendMessage(string signerID, Signer signer, string encrypterID, Encrypter encrypter, string messageType, string body)
        {
            return messageSender.SendMessage(signerID, signer, encrypterID, encrypter, messageType, body, new List<Attachment>());
        }

        public string SendMessage(string signerID, Signer signer, string encrypterID, Encrypter encrypter, string messageType, string body, List<Attachment>attachments)
        {
            return messageSender.SendMessage(signerID, signer, encrypterID, encrypter, messageType, body, attachments); 
        }

        public override void DIDCommEndpointHandler(DIDCommMessageRequest requestDIDComm, out DIDCommResponse responseDIDComm)
        {
            DIDCommMessageEnvelope envDIDComm = requestDIDComm.envelope;

            if (queueMessages)
            {
                // Persist DIDCommMessageEnvelope and queue CellId based on ReceiverID
                DIDCommMessageEnvelope_Cell envDIDCommCell = new DIDCommMessageEnvelope_Cell(envDIDComm);
                Global.LocalStorage.SaveDIDCommMessageEnvelope_Cell(envDIDCommCell);
                Global.LocalStorage.SaveStorage();

                if (!queues.ContainsKey(envDIDComm.ReceiverID))
                {
                    queues.TryAdd(envDIDComm.ReceiverID, new ConcurrentQueue<long>());
                }
                queues[envDIDComm.ReceiverID].Enqueue(envDIDCommCell.CellId);

                responseDIDComm.resp = "OK";
            }
            else
            {
                string response = "";
                Envelope envelope = new Envelope(envDIDComm.SenderID, envDIDComm.ReceiverID, envDIDComm.ReceiverServiceEndpointUrl, envDIDComm.MessageJWE);
                Console.WriteLine("17. Authenticating envelope addressed to: " + envelope.ReceiverID);
                Message message = messageProcessor.AuthenticateMessage(envelope);
                if (message != null)
                {
                    Console.WriteLine("17. Processing messsage from: " + message.from);
                    response = messageProcessor.ProcessMessage(message);
                }
                responseDIDComm.resp = response;
            }   

        }

        public void ProcessMessageQueues()
        {
            if (!queueMessages)
            {
                throw new InvalidOperationException("Queuing disabled");
            }

            bool Processing = true;
            while (Processing)
            {
                // Doubly-nested queues: queues of CellId's indexed by receiverID
                foreach (var queue in queues)
                {
                    ConcurrentQueue<long> cellids = queue.Value;
                    while (cellids.Count > 0)
                    {
                        long cellid;
                        bool dequeued = cellids.TryDequeue(out cellid);
                        if (dequeued)
                        {
                            DIDCommMessageEnvelope_Cell envDIDCommCell = Global.LocalStorage.LoadDIDCommMessageEnvelope_Cell(cellid);
                            DIDCommMessageEnvelope envDIDComm = envDIDCommCell.env;
                            Envelope envelope = new Envelope(envDIDComm.SenderID, envDIDComm.ReceiverID, envDIDComm.ReceiverServiceEndpointUrl, envDIDComm.MessageJWE);
                            Console.WriteLine("17b. Authenticating envelope addressed to: " + envelope.ReceiverID);
                            Message message = messageProcessor.AuthenticateMessage(envelope);
                            if (message != null)
                            {
                                Console.WriteLine("17b. Processing messsage from: " + message.from);
                                messageProcessor.ProcessMessage(message);
                            }
                            Global.LocalStorage.RemoveCell(cellid);
                        }
                    }
                }

                Thread.Sleep(100);
            }
        }
    }
}
