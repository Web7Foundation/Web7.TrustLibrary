using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web7.TrustLibrary.Base
{
    public class SubjectCryptoActors
    {
        Signer signer;
        Encrypter encrypter;

        public SubjectCryptoActors( Signer signer, Encrypter encrypter )
        {
            this.signer = signer;
            this.encrypter = encrypter; 
        }

        public Signer Signer { get => signer; set => signer = value; }
        public Encrypter Encrypter { get => encrypter; set => encrypter = value; }
    }
}
