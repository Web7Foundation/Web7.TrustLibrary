# Web 7.0 Portable Trust Library (PTL)

The Web 7.0 Portable Trust Library (PTL) is used to support all of the trust operations required to develop Web 7.0 Trusted Personal Agents (and supporting components) across multiple operating system environments. 

The list of PTL trust operations includes:
- Key generation, serialization and persistence (Signer and Encrypter)
- Digital signature creation and validation (Signer and Hasher)
- Encryption and decryption (Encrypter)
- JWE-based authenticated encryption (JWETokenizer)
- DID Document creation and management (DIDDocumenter)
- DID Registry-based DID Document registration and retrieval (DIDRegistrar)
- DIDComm message creation (DIDComm)
- DIDComm message transmission (HTTPTransporter)
- VDR-based signature authentication (Notary)
- Trust Registry TrustReg document management

The TPL trust operations are factored into 3 namespaces:
1. Web7.TrustLibrary.Base
2. Web7.TrustLibrary.Did
3. Web7.TrustLibrary.Registries

The PTL supports the Web 7.0 Foundation's goal of "making decentralized systems development easy-to-understand and easier for you to explain to other developers.

GitHub Repository: https://github.com/Web7Foundation/Web7.TrustLibrary 

## Web7.TrustLibrary.Base

### Encrypter

The Encrypter class is used to support a set of public and private keys for encryption and decryption; 
including key generation and serialization.

The Signer and Encrypter classes are used in the JWETokenizer class to create JWETokens.

Keywords: Confidentiality RSA

### Hasher

The Hasher class is used to create Hashes of arbitrary Strings and Byte arrays.

Keywords: Authenticity SHA SHA256 Hash

### Helper

The Helper class contains a number of public static helper or utility methods.

### JWETokenizer

The JWETokenizer class is used to support the creation, verification, and serialization of JWE tokens. 

This class uses keys created or deserialized from the Signer and Encrypter classes. 

Keywords: Authenticated-Encryption JWE JWE-Token

### Signer

The Signer class can be used to to support the creation of digital Signatures for arbitrary Strings and Byte arrays; including key generation and serialization.

The Signer and Encrypter classes are used in the JWETokenizer class to create JWETokens.

Keywords: Authenticity ECDsa Digital-Signature

## Web7.TrustLibrary.Did

### DIDComm Message

The DIDComm Message classes are used to create and serialize an in-memory Web 7.0 DIDComm Message with (or without) a DIDComm Attachment. 
In addition, the DIDComm class can create authenticated encrypted messages by internally using the JWETokenizer class.

The HTTPTransporter class is used to transport a DIDCommMessage from a Sender agent's outbound service endpoint to a Receiver 
agent's inbound service endpoint using the HTTP protocol.

Keywords: Secure-Messaging Authenticity Confidentiality DID DID-Communications DIDComm-Messaging

### DIDDocumenter

The DIDDocumenter class takes as inputs a Signer public key, an Encrypter public key, a list of service endpoints, 
and a list of relationships and creates an in-memory DIDDocument. 
In addition, a DIDDocumenter can be used to update as well as serialize an existing, in-memory DIDDocument.

The Registry class is used to persist an in-memory DIDDocument to as well as retrieve a persisted DIDDocument from the DID Registry.

Keywords: DID DID-Document

### DIDDocument (Extension Methods)

The DIDDocument partial class supports the tricky field-based JSON serialization/deserialization needed by users of the DIDDocumenter class.

The underlying DIDDocument partial class is generated from DIDDocument.tsl.

Keywords: DID DID-Document

### JsonWebKeyDotnet6 (Extension Methods)

The JsonWebKeyDotnet6 partial class supports the tricky field-based JSON serialization/deserialization needed by users of the DIDDocumenter class.

The underlying JsonWebKeyDotnet6 partial class is generated from DIDDocument.tsl.

Keywords: DID DID-Document JsonWebKey Dotnet

## Web7.TrustLibrary.Registries

### DIDRegistrar

The DIDRegistrar class is used to register and retrieve a DIDDocument to and from the DID Registry.

The DIDocumenter class is used to create, update and serialized in-memory DIDDocuments

Keywords: DID-Registry DID Decentralized-Identifier

### Notary

The Notary class is used to create, serialized, persist, and retrieve Signatures using a Verifiable Data Registry.

This class uses Signatures created or deserialized from the Signer class.

Keywords: Authenticity Verifiable-Data-Registry

### TrustRegistrar

The TrustRegistrar class is used to maintain the collection of relationships in a Trust Registration (TrustReg) document, 
a specialization of a DID Document.
TrustReg documents are stored in a DID Registry managed by the DIDRegistrar class.

The DIDDocumenter and DIDRegistrar classes are used to support the capabilities of the TrustRegistrar class.

Keywords: Trust-Registry DID-Document DID-Registry DID Decentralized-Identifier

## Web7.TrustLibrary.Transports

### HTTPTransporter

The HTTPTransporter class is used to transport a DIDCommMessage from a Sender agent's outbound service endpoint to a Receiver 
agent's inbound service endpoint using the HTTP protocol.

The DIDComm class is used to create and serialize an in-memory Web 7.0 DIDComm Message with (or without) a DIDComm Attachment. 
In addition the DIDComm class can create authenticated encrypted messages by internally using the JWETokenizer class.

Keywords: DIDComm HTTP Transport-Protocol