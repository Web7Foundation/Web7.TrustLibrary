using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Web7.TrustLibrary.Did.DIDDocumemt;

namespace Web7.TrustLibrary.Did
{
    // The DIDDocument partial class supports the tricky field-based JSON serialization/deserialization needed by users of the DIDDocumenter class.
    // The underlying DIDDocument partial class is generated from DIDDocument.tsl.
    // Keywords: DID DID-Document

    public partial struct DIDDocument
    {
        public DIDDocument FromJson(string didDocJson)
        {
            // https://stackoverflow.com/questions/58139759/how-to-use-class-fields-with-system-text-json-jsonserializer
            DIDDocument didDocument = JsonSerializer.Deserialize<DIDDocument>(didDocJson,
                new JsonSerializerOptions { IncludeFields = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
            return didDocument;
        }

        public string ToJson()
        {
            // https://stackoverflow.com/questions/58139759/how-to-use-class-fields-with-system-text-json-jsonserializer
            string diddocJson = JsonSerializer.Serialize<DIDDocument>(this,
                new JsonSerializerOptions { IncludeFields = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
            return diddocJson;
        }
    }
}
