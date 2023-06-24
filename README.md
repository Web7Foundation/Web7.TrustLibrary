# Web 7.0 Trust Library

The Web 7.0 Trust Library is used to support all of the trust operations required to develop a Web 7.0 Trusted Personal Agent and supporting components using the .NET 6.0 development environment. The list of trust operations includes:
- Key generation, serialization and persistence (Signer)
- Digital signature creation and validation (Signer)
- Encryption and decryption (Encrypter)
- JWE-based authenticated encryption (JWETokenizer)
- DID Document creation and management (DIDDocumenter)
- DID Registry-based DID Document registration and retrieval (DIDRegistrar)
- DIDComm message creation (DIDComm)
- DIDComm message transmission (HTTPTransporter)
- VDR-based signature authentication (Notary)

## DIDComm

The DIDComm class is used to create and serialize an in-memory Web 7.0 DIDComm Message with (or without) a DIDComm Attachment. In addition the DIDComm class can create authenticated encrypted messages by internally using the JWETokenizer class.

The HTTPTransporter class is used to transport a DIDCommMessage from a Sender agent's outbound service endpoint to a Receiver agent's inbound service endpoint using the HTTP protocol.

Keywords: Secure-Messaging Authenticity Confidentiality DID DID-Communications DIDComm-Messaging

## DIDDocumenter

The DIDDocumenter class takes as inputs a Signer public key, an Encrypter public key, a list of service endpoints, and a list of relationships and creates an in-memory DIDDocument. In addition, a DIDDocumenter can be used to update as well as serialize an existing, in-memory DIDDocument.

The Registry class is used to persist an in-memory DIDDocument to as well as retrieve a persisted DIDDocument from the DID Registry.

Keywords: DID DID-Document

## DIDRegistrar

The DIDRegistrar class is used to register and retrieve a DIDDocument to and from the DID Registry.

The DIDocumenter class is used to create, update and serialized in-memory DIDDocuments

Keywords: DID-Registry DID Decentralized-Identifier

## Encrypter

The Encrypter class is used to support a set of public and private keys for encryption and decryption; including key generation and serialization.

The Signer and Encrypter classes are used in the JWETokenizer class to create JWETokens.

Keywords: Confidentiality RSA

## Hasher

The Hasher class is used to create Hashes of arbitrary Strings and Byte arrays.

This class is used primarily by the Signer class.

Keywords: Authenticity SHA SHA256

## HTTPTransporter

The HTTPTransporter class is used to transport a DIDCommMessage from a Sender agent's outbound service endpoint to a Receiver agent's inbound service endpoint using the HTTP protocol.

The DIDComm class is used to create and serialize an in-memory Web 7.0 DIDComm Message with (or without) a DIDComm Attachment. In addition the DIDComm class can create authenticated encrypted messages by internally using the JWETokenizer class.

Keywords: DIDComm HTTP

## JWETokenizer

The JWETokenizer class is used to support the creation, verification, and serialization of JWE tokens. 

This class uses keys created or deserialized from the Signer and Encrypter classes. 

Keywords: Authenticated-Encryption JWE JWE-Token

## Notary

THe Notary class is used to create, serialized, persist, and retrieve Signatures using a Verifiable Data Registry.

This class uses Signatures created or deserialized from the Signer class.

Keywords: Authenticity

## Signer

The Signer class can be used to to support the creation digital Signatures for arbitrary Strings and Byte arrays; including key generation and serialization.

The Signer and Encrypter classes are used in the JWETokenizer class to create JWETokens.

Keywords: Authenticity ECDsa
