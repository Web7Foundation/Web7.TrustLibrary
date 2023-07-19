using Web7.TrustLibrary.Base;
using Web7.TrustLibrary.Did.DIDComm;

namespace Web7.TrustLibrary.Transports
{
    //  Not a full DI design: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
    public interface IMessageSender
    {
        string SendMessage(string signerID, Signer signer, string encrypterID, Encrypter encrypter, string messageType, string body);
        string SendMessage(string signerID, Signer signer, string encrypterID, Encrypter encrypter, string messageType, string body, List<Attachment> attachments);
    }

    public interface IMessageProcessor
    {
        Message AuthenticateMessage(Envelope envelope);
        string ProcessMessage(Message message);
    }
}
