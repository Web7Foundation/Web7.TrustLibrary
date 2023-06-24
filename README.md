# Web7 Trust Libraries

## DIDDocumenter

The DIDDocumenter class takes as inputs a Signer public key, an Encrypter public key, a list of service endpoints, and a list of relationships and creates an in-memory DIDDocument. In addition, a DIDDocumenter can be used to update as well as serialize an existing, in-memory DIDDocument.

The Registry class is used to persist an in-memory DIDDocument to as well as retrieve a persisted DIDDocument from the DID Registry.

## DIDRegistry

The DIDRegistry class is used to persist and retrieve a DIDDocument to and from the DID Registry.

The DIDocumenter class is used to create, update and serialzied in-memory DIDDocuments

## Encrypter

The Encrypter class is used to support a set of public and private keys for encryption and decryption; including key generation and serialization.

The Signer and Encrypter classes are used in the JWETokenier class to create JWETokens.

## Hasher

The Hasher class is used to create Hashes of arbitrary Strings and Byte arrays.

This class is used primarily by the Signer class.

## JWETokenizer

The JWETokenizer class is used to support the creation, verification, and serialization of JWE tokens. 

This class uses keys created or deserialized from the Signer and Encrypter classes. 

## Notary

THe Notary class is used to create, serialized, persist, and retrieve Signatures from a Verifiable Data Registry.

This class uses Signatures created or deserialized from the Signer class.

## Signer

The Signer class can be used to to support the creation digital Signatures for arbitrary Strings and Byte arrays; including key generation and serialization.

The Signer and Encrypter classes are used in the JWETokenier class to create JWETokens.
