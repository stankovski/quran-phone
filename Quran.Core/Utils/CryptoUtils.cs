using System.Text;

namespace Quran.Core.Utils
{
    public class CryptoUtils
    {
        public static string GetHash(string value)
        {
            byte[] unencryptedByteArray = Encoding.Unicode.GetBytes(value);
            byte[] encryptedByteArray = MD5Core.GetHash(unencryptedByteArray);
            var encryptedStringBuilder = new StringBuilder();
            foreach (byte b in encryptedByteArray)
            {
                encryptedStringBuilder.AppendFormat("{0:x2}", b);
            }
            return encryptedStringBuilder.ToString();
        }
    }
}
