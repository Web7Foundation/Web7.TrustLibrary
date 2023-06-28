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
        private static ConcurrentDictionary<string, ConcurrentQueue<long>> queues = new ConcurrentDictionary<string, ConcurrentQueue<long>>();

        public static ConcurrentDictionary<string, ConcurrentQueue<long>> Queues { get => queues; }

        public override void DIDCommEndpointHandler(DIDCommMessageRequest request, out DIDCommResponse response)
        {
            DIDCommMessageEnvelope env = request.envelope;

            // Persist DIDCommMessageEnvelope
            DIDCommMessageEnvelope_Cell envCell = new DIDCommMessageEnvelope_Cell(env);
            Global.LocalStorage.SaveDIDCommMessageEnvelope_Cell(envCell);
            Global.LocalStorage.SaveStorage();
            var celltype = Global.LocalStorage.GetCellType(envCell.CellId);
            ulong cellcount = Global.LocalStorage.CellCount;
            Console.WriteLine(">>>> cellid: " + envCell.CellId.ToString() + " celltype: " + celltype.ToString() + " cellcount: " + cellcount);

            if (!Queues.ContainsKey(env.ReceiverID))
            {
                Queues.TryAdd(env.ReceiverID, new ConcurrentQueue<long>());
            }
            Queues[env.ReceiverID].Enqueue(envCell.CellId);

            response.rc = (int)Trinity.TrinityErrorCode.E_SUCCESS;

            // Create a Web 7.0 Message Envelope
            // Envelope envelope = new Envelope(env.SenderID, env.ReceiverID, env.ReceiverID, env.Token);
        }
    }
}
