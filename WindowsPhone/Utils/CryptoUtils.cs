using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Utils
{
    public class CryptoUtils
    {
        public static string GetHash(string value)
        {
            var sha1 = new SHA1Managed();
            byte[] unencryptedByteArray = Encoding.Unicode.GetBytes(value);
            using (var stream = new MemoryStream(unencryptedByteArray))
            {
                byte[] encryptedByteArray = sha1.ComputeHash(stream);
                var encryptedStringBuilder = new StringBuilder();
                foreach (byte b in encryptedByteArray)
                {
                    encryptedStringBuilder.AppendFormat("{0:x2}", b);
                }
                return encryptedStringBuilder.ToString();
            }
        }
    }
}
