﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web7.TrustLibrary
{
    // The Helper class contains a number of public static helper or utility methods.
    public static class Helper
    {
        public const string DID_SUBJECT = "did:web7:subject:";
        public const string DID_PERSON = DID_SUBJECT + "person:";
        public const string DID_ALICE = DID_PERSON + "1234";
        public const string DID_BOB = DID_PERSON + "4567";

        public const string DID_DIDCOMM = "did:web7:didcomm:";
        public const string DID_MESSAGEID = DID_DIDCOMM + "messageid:";
        public const string DID_ATTACHMENTID = DID_DIDCOMM + "attachmentid:";
        public const string DID_THID = DID_DIDCOMM + "thid:";

        // https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        // https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string Base64Encode(byte[] bytes)
        {
            return System.Convert.ToBase64String(bytes);
        }

        public static byte[] Base64DecodeBytes(string byteString)
        {
            return System.Convert.FromBase64String(byteString);
        }

        public static long UNIX_time(DateTime t)
        {
            return (long)(t.Subtract(DateTime.UnixEpoch)).TotalSeconds;
        }
    }
}
