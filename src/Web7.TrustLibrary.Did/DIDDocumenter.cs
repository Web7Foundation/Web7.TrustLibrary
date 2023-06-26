using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Web7.TrustLibrary.Base;

namespace Web7.TrustLibrary.Did.DIDDocumemt
{
    // The DIDDocumenter class takes as inputs a Signer public key, an Encrypter public key, a list of service endpoints,
    // and a list of relationships and creates an in-memory DIDDocument.
    // In addition, a DIDDocumenter can be used to update as well as serialize an existing, in-memory DIDDocument.
    // The Registry class is used to persist an in-memory DIDDocument to as well as retrieve a persisted DIDDocument from the DID Registry.
    // Keywords: DID DID-Document
    public class DIDDocumenter
    {
        DIDDocument didDocument = new DIDDocument();

        public DIDDocumenter(string did, 
            List<string> controller, 
            List<VerificationMethodMap> verificationMethod, // Signing
            List<VerificationMethodMap> keyAgreement,       // Encryption
            List<ServiceMap> service, 
            List<ServiceMap> relationship)
        {
            Initialize(did, controller, verificationMethod, keyAgreement, service, relationship);
        }

        public DIDDocumenter(string did,
            List<string> controller,
            Signer signer, // verificationMethod
            Encrypter encrypter, // keyAgreement
            List<ServiceMap> service,
            List<ServiceMap> relationship)
        {
            VerificationMethodMap verificationMethod0 = new VerificationMethodMap("Web 7.0 PTL Signer Public Key", signer.KeyID, signer.KeyID, "JsonWebKeyWeb7PTL", signer.KeyPublicJsonWebKeyAsString());
            VerificationMethodMap keyAgreement0 = new VerificationMethodMap("Web 7.0 PTL Encrypter Public Key", encrypter.KeyID, encrypter.KeyID, "JsonWebKeyWeb7PTL", encrypter.KeyPublicJsonWebKeyAsString());

            List<VerificationMethodMap> verificationMethod = new List<VerificationMethodMap>() { verificationMethod0 };
            List<VerificationMethodMap> keyAgreement = new List<VerificationMethodMap>() { keyAgreement0 };

            Initialize(did, controller, verificationMethod, keyAgreement, service, relationship);
        }

        internal void Initialize(string did,
            List<string> controller,
            List<VerificationMethodMap> verificationMethod, // Signing
            List<VerificationMethodMap> keyAgreement,       // Encryption
            List<ServiceMap> service,
            List<ServiceMap> relationship)
        {
            didDocument.id = did;
            didDocument.context = new List<string> { "https://www.w3.org/ns/did/v1" };
            didDocument.controller = controller;
            didDocument.comment = "DID Document " + did + " created by " + this.GetType().Name + " on " + DateTime.Now.ToString();
            didDocument.verificationMethod = verificationMethod;
            didDocument.keyAgreement = keyAgreement;
            didDocument.service = service;
            didDocument.relationship = relationship;
        }

        public DIDDocument DidDocument { get => didDocument; set => didDocument = value; }

        public override string ToString()
        {
            return didDocument.ToString();
        }
    }
}
