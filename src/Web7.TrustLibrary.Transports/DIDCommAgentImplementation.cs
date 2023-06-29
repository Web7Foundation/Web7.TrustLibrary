using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity;
using Web7.TrustLibrary.Did.DIDComm;

namespace Web7.TrustLibrary.Transports
{
    public class DIDCommAgentImplementation : DIDCommAgentBase
    {
        ConcurrentDictionary<string, ConcurrentQueue<long>> queues = new ConcurrentDictionary<string, ConcurrentQueue<long>>();

        public override void DIDCommEndpointHandler(DIDCommMessageRequest request, out DIDCommResponse response)
        {
            DIDCommMessageEnvelope env = request.envelope;

            // Persist DIDCommMessageEnvelope and queue CellId based on ReceiverID
            DIDCommMessageEnvelope_Cell envCell = new DIDCommMessageEnvelope_Cell(env);
            Global.LocalStorage.SaveDIDCommMessageEnvelope_Cell(envCell);
            Global.LocalStorage.SaveStorage();
            var celltype = Global.LocalStorage.GetCellType(envCell.CellId);
            ulong cellcount = Global.LocalStorage.CellCount;
            Console.WriteLine(">>>> cellid: " + envCell.CellId.ToString() + " celltype: " + celltype.ToString() + " cellcount: " + cellcount);

            if (!queues.ContainsKey(env.ReceiverID))
            {
                queues.TryAdd(env.ReceiverID, new ConcurrentQueue<long>());
            }
            queues[env.ReceiverID].Enqueue(envCell.CellId);

            response.rc = (int)Trinity.TrinityErrorCode.E_SUCCESS;

            // Create a Web 7.0 Message Envelope
            // Envelope envelope = new Envelope(env.SenderID, env.ReceiverID, env.ReceiverID, env.Token);
        }

        public void ProcessMessageQueues(IMessageEnvelopeProcessor messageEnvelopeProcessor)
        {
            bool Processing = true;
            while (Processing)
            {
                foreach (var queue in queues)
                {
                    ConcurrentQueue<long> cellids = queue.Value;
                    while (cellids.Count > 0)
                    {
                        long cellid;
                        bool dequeued = cellids.TryDequeue(out cellid);
                        if (dequeued)
                        {
                            DIDCommMessageEnvelope_Cell envCell = Global.LocalStorage.LoadDIDCommMessageEnvelope_Cell(cellid);
                            DIDCommMessageEnvelope env = envCell.env;
                            Envelope envelope = new Envelope(env.SenderID, env.ReceiverID, env.ReceiverServiceEndpointUrl, env.Token);
                            Console.WriteLine("17. Process envelope addressed to: " + envelope.ReceiverID);
                            messageEnvelopeProcessor.ProcessEnvelope(envelope);
                            Global.LocalStorage.RemoveCell(cellid);
                        }
                    }
                }

                Thread.Sleep(100);
            }
        }
    }
}
