using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web7.TrustLibrary.Did
{
    // The JsonWebKeyDotnet6 partial class supports the tricky field-based JSON serialization/deserialization needed by users of the DIDDocumenter class.
    // The underlying JsonWebKeyDotnet6 partial class is generated from DIDDocument.tsl.
    // Keywords: DID DID-Document JsonWebKey Dotnet

    public partial struct JsonWebKeyDotnet6
    {
        public JsonWebKey ToJsonWebKey()
        {
            JsonWebKey jwk = new JsonWebKey();
            jwk.Alg = this.Alg;
            jwk.Crv = this.Crv;
            jwk.D = this.D;
            jwk.DP = this.DP;
            jwk.DQ = this.DQ;
            jwk.E = this.E;
            jwk.K = this.K;
            // jwk.KeyOps = this.KeyOps; // readonly
            jwk.Kid = this.Kid;
            jwk.Kty = this.Kty;
            jwk.N = this.N;
            jwk.Oth = this.Oth;
            jwk.P = this.P;
            jwk.Q = this.Q;
            jwk.QI = this.QI;
            jwk.Use = this.Use;
            jwk.X = this.X;
            // jwk.X5c = this.X5c; // readonly
            jwk.X5t = this.X5t;
            jwk.X5tS256 = this.X5tS256;
            jwk.X5u = this.X5u;
            jwk.Y = this.Y;
            return jwk;
        }
    }
}
