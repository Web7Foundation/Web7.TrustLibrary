using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Web7.TrustLibrary;
using Web7.TrustLibrary.DIDComm;


namespace Web7.TrustLibrary
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string MESSAGE_TYPE = "https://example.org/example/1.0/hello";

            string plaintext = "{ \"message\": \"Hello world!\" }";
            byte[] plaintextbytes = Encoding.UTF8.GetBytes(plaintext);

            DateTime now = DateTime.Now;
            Message msg = new Message(
                Helper.DID_MESSAGEID + Guid.NewGuid().ToString(),
                MESSAGE_TYPE,
                Helper.DID_ALICE,
                new List<string>() { Helper.DID_BOB },
                Helper.DID_THID + Guid.NewGuid().ToString(),
                "",
                Helper.UNIX_time(now),
                Helper.UNIX_time(now.AddDays(30)),
                Helper.Base64Encode(plaintext)
            );
            string text64 = Helper.Base64Encode("Foo bar!");
            AttachmentData d = new AttachmentData("", "", "", text64, "");
            Attachment a = new Attachment(
                Helper.DID_ATTACHMENTID + Guid.NewGuid().ToString(),
                "Attachment abc",
                "abc.txt",
                "text/plain",
                "",
                Helper.UNIX_time(now),
                d,
                0
            );
            msg.attachments.Add(a);

            string msgJson = JsonSerializer.Serialize<Message>(msg);
            Console.WriteLine( msgJson );

            Console.WriteLine(msgJson);
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
    }
}
