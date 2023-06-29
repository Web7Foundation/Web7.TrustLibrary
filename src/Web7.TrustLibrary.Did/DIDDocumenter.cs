using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
            JsonWebKey signerKeyPublicPWK = signer.KeyPublicSecurityKeyToJsonWebKey();
            JsonWebKeyDotnet6 signerKeyPublicJsonWebKeyDotnet6 = new JsonWebKeyDotnet6(signerKeyPublicPWK.Alg, signerKeyPublicPWK.Crv, signerKeyPublicPWK.D, signerKeyPublicPWK.DP, signerKeyPublicPWK.DQ, signerKeyPublicPWK.E, signerKeyPublicPWK.K, 
                signerKeyPublicPWK.KeyOps == null ? null : new List<string>(signerKeyPublicPWK.KeyOps), 
                signerKeyPublicPWK.Kid, signerKeyPublicPWK.Kty, signerKeyPublicPWK.N,
                signerKeyPublicPWK.Oth == null ? null : new List<string>(signerKeyPublicPWK.Oth), 
                signerKeyPublicPWK.P, signerKeyPublicPWK.Q, signerKeyPublicPWK.QI, signerKeyPublicPWK.Use, signerKeyPublicPWK.X,
                signerKeyPublicPWK.X5c == null ? null : new List<string>(signerKeyPublicPWK.X5c), 
                signerKeyPublicPWK.X5t, signerKeyPublicPWK.X5tS256, signerKeyPublicPWK.X5u, signerKeyPublicPWK.Y);

            JsonWebKey encrypterKeyPublicPWK = encrypter.KeyPublicSecurityKeyToJsonWebKey();
            JsonWebKeyDotnet6 encrypterKeyPublicJsonWebKeyDotnet6 = new JsonWebKeyDotnet6(encrypterKeyPublicPWK.Alg, encrypterKeyPublicPWK.Crv, encrypterKeyPublicPWK.D, encrypterKeyPublicPWK.DP, encrypterKeyPublicPWK.DQ, encrypterKeyPublicPWK.E, encrypterKeyPublicPWK.K,
                encrypterKeyPublicPWK.KeyOps == null ? null : new List<string>(encrypterKeyPublicPWK.KeyOps), 
                encrypterKeyPublicPWK.Kid, encrypterKeyPublicPWK.Kty, encrypterKeyPublicPWK.N,
                encrypterKeyPublicPWK.Oth == null ? null : new List<string>(encrypterKeyPublicPWK.Oth), 
                encrypterKeyPublicPWK.P, encrypterKeyPublicPWK.Q, encrypterKeyPublicPWK.QI, encrypterKeyPublicPWK.Use, encrypterKeyPublicPWK.X,
                encrypterKeyPublicPWK.X5c == null ? null : new List<string>(encrypterKeyPublicPWK.X5c), 
                encrypterKeyPublicPWK.X5t, encrypterKeyPublicPWK.X5tS256, encrypterKeyPublicPWK.X5u, encrypterKeyPublicPWK.Y);

            VerificationMethodMap verificationMethod0 = new VerificationMethodMap("Web 7.0 PTL Signer Public Key", signer.KeyID, signer.KeyID, "JsonWebKeyWeb7PTL", signerKeyPublicJsonWebKeyDotnet6);
            VerificationMethodMap keyAgreement0 = new VerificationMethodMap("Web 7.0 PTL Encrypter Public Key", encrypter.KeyID, encrypter.KeyID, "JsonWebKeyWeb7PTL", encrypterKeyPublicJsonWebKeyDotnet6);

            List<VerificationMethodMap> verificationMethod = new List<VerificationMethodMap>() { verificationMethod0 };
            List<VerificationMethodMap> keyAgreement = new List<VerificationMethodMap>() { keyAgreement0 };

            Initialize(did, controller, verificationMethod, keyAgreement, service, relationship);
        }

        public DIDDocumenter(string didDocJson)
        {
            didDocument = didDocument.FromJson(didDocJson);
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
    }
}
