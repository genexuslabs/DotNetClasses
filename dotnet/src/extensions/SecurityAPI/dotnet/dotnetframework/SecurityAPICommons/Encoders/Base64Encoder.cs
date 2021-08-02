using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using Org.BouncyCastle.Utilities.Encoders;

namespace SecurityAPICommons.Encoders
{
    /// <summary>
    /// Base64Encoder class
    /// </summary>
    [SecuritySafeCritical]
    public class Base64Encoder : SecurityAPIObject
    {


        /// <summary>
        /// Base64Encoder constructor
        /// </summary>
        [SecuritySafeCritical]
        public Base64Encoder() : base()
        {

        }

        /// <summary>
        /// string to Base64 encoded string
        /// </summary>
        /// <param name="text">string UTF-8 plain text to encode</param>
        /// <returns>Base64 string text encoded</returns>
        [SecuritySafeCritical]
        public string toBase64(string text)
        {
            EncodingUtil eu = new EncodingUtil();
            byte[] textBytes = eu.getBytes(text);
            if (eu.HasError())
            {
                this.error = eu.error;
                return "";

            }

            string result = System.Text.Encoding.UTF8.GetString(Base64.Encode(textBytes));
            if (result == null || result.Length == 0)
            {
                this.error.setError("B64001", "Error encoding base64");
                return "";
            }
            this.error.cleanError();
            return result;
        }
        /// <summary>
        /// string Base64 encoded to string plain text
        /// </summary>
        /// <param name="base64Text">string Base64 encoded</param>
        /// <returns>string UTF-8 plain text from Base64</returns>
        [SecuritySafeCritical]
        public string toPlainText(string base64Text)
        {
            EncodingUtil eu = new EncodingUtil();
            byte[] bytes = Base64.Decode(base64Text);
            string result = eu.getString(bytes);
            if (eu.HasError())
            {
                this.error = eu.error;
                return "";
            }
            if (result == null || result.Length == 0)
            {
                this.error.setError("B64002", "Error decoding base64");
                return "";
            }
            this.error.cleanError();
            return result;
        }
        /// <summary>
        /// string Base64 encoded text to string hexadecimal encoded text
        /// </summary>
        /// <param name="base64Text">string Base64 encoded</param>
        /// <returns>string Hexa representation of base64Text</returns>
        [SecuritySafeCritical]
        public string toStringHexa(string base64Text)
        {

            byte[] bytes = Base64.Decode(base64Text);
            string result = BitConverter.ToString(bytes).Replace("-", string.Empty);
            if (result == null || result.Length == 0)
            {
                this.error.setError("B64003", "Error decoding base64 to hexa");
                return "";
            }
            this.error.cleanError();
            return result;

        }
        /// <summary>
        /// string hexadecimal encoded text to string Base64 encoded text
        /// </summary>
        /// <param name="stringHexa">string Hexa</param>
        /// <returns>string Base64 encoded of stringHexa</returns>
        [SecuritySafeCritical]
        public string fromStringHexaToBase64(string stringHexa)
        {
            byte[] stringBytes = Hex.Decode(stringHexa);
            string result = System.Text.Encoding.UTF8.GetString((Base64.Encode(stringBytes)));
            if (result == null || result.Length == 0)
            {
                this.error.setError("B64004", "Error encoding base64 from hexa");
                return "";
            }
            this.error.cleanError();
            return result;
        }
    }
}
