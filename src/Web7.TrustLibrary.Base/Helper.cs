using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Web7.TrustLibrary.Base
{
    // The Helper class contains a number of public static helper or utility methods.
    public static class Helper
    {
        public const int ROOT_CELLID = 0;

        public const string CLAIM_MESSAGE = "message";

        public const string DID_SUBJECT = "did:web7:subject:";
        public const string DID_PERSON = DID_SUBJECT + "person:";
        public const string DID_ALICE = DID_PERSON + "1234";
        public const string DID_BOB = DID_PERSON + "4567";

        public const string DID_AGENT = "did:web7:agent:";
        public const string DID_DIDREGISTRYAGENT = DID_AGENT + "didregstry:";
        public const string DID_DIDREGISTRYAGENT2222 = DID_DIDREGISTRYAGENT + "2222";

        public const string DID_DIDCOMM = "did:web7:didcomm:";
        public const string DID_MESSAGEID = DID_DIDCOMM + "messageid:";
        public const string DID_ATTACHMENTID = DID_DIDCOMM + "attachmentid:";
        public const string DID_THID = DID_DIDCOMM + "thid:";

        // https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-body64-string
        public static string Base64Encode(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            return Base64Encode(bytes);
        }

        public static string Base64Encode(byte[] bytes)
        {
            return System.Convert.ToBase64String(bytes);
        }

        public static string Base64Decode64ToString(string string64)
        {
            var base64EncodedBytes = Base64Decode64(string64);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static byte[] Base64Decode64(string string64)
        {
            return System.Convert.FromBase64String(string64);
        }

        public static long UNIX_time(DateTime t)
        {
            return (long)(t.Subtract(DateTime.UnixEpoch)).TotalSeconds;
        }

        public static string JsonWebKeyToString(JsonWebKey jwk)
        {
            return JsonSerializer.Serialize<JsonWebKey>(jwk);
        }

        // https://stackoverflow.com/questions/65620631/how-to-pretty-print-using-system-text-json-for-unknown-object
        public static string JsonPrettyPrint(this string json)
        {
            using var jsonDoc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true }).Replace("\\u0022", "\"");
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        public static string GetTemplate(System.Reflection.Assembly assembly, string resname)
        {
            string[] streams = assembly.GetManifestResourceNames();
            var resStream = assembly.GetManifestResourceStream(resname);
            if (resStream == null) return "";
            byte[] res = new byte[resStream.Length];
            int nBytes = resStream.Read(res);
            string template = Encoding.UTF8.GetString(res);

            if (String.IsNullOrEmpty(template)) throw new NullReferenceException("GetTemplate");

            return template;
        }
    }
}
