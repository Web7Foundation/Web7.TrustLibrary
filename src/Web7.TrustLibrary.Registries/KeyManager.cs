using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web7.TrustLibrary.Base;

namespace Web7.TrustLibrary.Registries
{
    // The KeyManager class is used to manage a collection of Signer and Encrypter key pairs
    // stored locally on a device in a personal KeyVault.
    // Keywords: KeyManager KeyVault Key-Storage
    public class KeyManager
    {
        // TODO
    }

    public class AnonSubjectSecretKeys
    {
        JsonWebKey signerKeyPrivateJWK;
        JsonWebKey signerKeyPublicJWK;
        JsonWebKey encrypterKeyPrivateJWK;
        JsonWebKey encrypterKeyPublicJWK;

        public AnonSubjectSecretKeys(Signer signer, Encrypter encrypter)
        {
            signerKeyPrivateJWK = signer.KeyPrivateSecurityKeyToJsonWebKey();
            signerKeyPublicJWK = signer.KeyPublicSecurityKeyToJsonWebKey();
            encrypterKeyPrivateJWK = encrypter.KeyPrivateSecurityKeyToJsonWebKey();
            encrypterKeyPublicJWK = encrypter.KeyPublicSecurityKeyToJsonWebKey();
        }

        public JsonWebKey SignerKeyPrivateJWK { get => signerKeyPrivateJWK; set => signerKeyPrivateJWK = value; }
        public JsonWebKey SignerKeyPublicJWK { get => signerKeyPublicJWK; set => signerKeyPublicJWK = value; }
        public JsonWebKey EncrypterKeyPrivateJWK { get => encrypterKeyPrivateJWK; set => encrypterKeyPrivateJWK = value; }
        public JsonWebKey EncrypterKeyPublicJWK { get => encrypterKeyPublicJWK; set => encrypterKeyPublicJWK = value; }
    }
}
