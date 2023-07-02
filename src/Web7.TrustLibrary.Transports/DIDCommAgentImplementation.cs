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

        public IMessageProcessor MessageProcessor { get => messageProcessor; set => messageProcessor = value; }
        public IMessageSender MessageSender { get => messageSender; set => messageSender = value; }

        public void Start(IMessageSender messageSender, IMessageProcessor messageProcessor)
        {
            this.messageSender = messageSender;
            this.messageProcessor = messageProcessor;
            Start(); // Also call the default (inherited) Start() - else connections are refused
        }

        public void SendMessage(string signerID, Signer signer, string encrypterID, Encrypter encrypter, string messageType, string body)
        {
            messageSender.SendMessage(signerID, signer, encrypterID, encrypter, messageType, body, new List<Attachment>());
        }

        public void SendMessage(string signerID, Signer signer, string encrypterID, Encrypter encrypter, string messageType, string body, List<Attachment>attachments)
        {
            messageSender.SendMessage(signerID, signer, encrypterID, encrypter, messageType, body, attachments); 
        }

        public override void DIDCommEndpointHandler(DIDCommMessageRequest requestDIDComm, out DIDCommResponse responseDIDComm)
        {
            DIDCommMessageEnvelope envDIDComm = requestDIDComm.envelope;

            // Persist DIDCommMessageEnvelope and queue CellId based on ReceiverID
            DIDCommMessageEnvelope_Cell envDIDCommCell = new DIDCommMessageEnvelope_Cell(envDIDComm);
            Global.LocalStorage.SaveDIDCommMessageEnvelope_Cell(envDIDCommCell);
            Global.LocalStorage.SaveStorage();
            var celltype = Global.LocalStorage.GetCellType(envDIDCommCell.CellId);
            ulong cellcount = Global.LocalStorage.CellCount;
            Console.WriteLine(">>>> cellid: " + envDIDCommCell.CellId.ToString() + " celltype: " + celltype.ToString() + " cellcount: " + cellcount);

            if (!queues.ContainsKey(envDIDComm.ReceiverID))
            {
                queues.TryAdd(envDIDComm.ReceiverID, new ConcurrentQueue<long>());
            }
            queues[envDIDComm.ReceiverID].Enqueue(envDIDCommCell.CellId);

            responseDIDComm.rc = (int)Trinity.TrinityErrorCode.E_SUCCESS;
        }

        public void ProcessMessageQueues()
        {
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
                            Console.WriteLine("17. Authenticating envelope addressed to: " + envelope.ReceiverID);
                            Message message = messageProcessor.AuthenticateMessage(envelope);
                            if (message != null)
                            {
                                Console.WriteLine("17. Processing messsage from: " + message.from);
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
