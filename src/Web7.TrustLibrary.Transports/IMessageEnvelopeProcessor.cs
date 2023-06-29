using Web7.TrustLibrary.Did.DIDComm;

namespace Web7.TrustLibrary.Transports
{
    //  Not a full DI design: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
    public interface IMessageEnvelopeProcessor
    {
        void ProcessEnvelope(Envelope envelope);
    }
}
