using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Web7.TrustLibrary
{
    // The DIDDocumenter class takes as inputs a Signer public key, an Encrypter public key, a list of service endpoints,
    // and a list of relationships and creates an in-memory DIDDocument.
    // In addition, a DIDDocumenter can be used to update as well as serialize an existing, in-memory DIDDocument.
    // The Registry class is used to persist an in-memory DIDDocument to as well as retrieve a persisted DIDDocument from the DID Registry.
    // Keywords: DID DID-Document
    public class DIDDocumenter
    {
    }
}
