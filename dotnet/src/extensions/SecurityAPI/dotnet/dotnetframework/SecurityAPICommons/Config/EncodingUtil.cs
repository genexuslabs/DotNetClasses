﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using SecurityAPICommons.Commons;

namespace SecurityAPICommons.Config
{
    [SecuritySafeCritical]
    public class EncodingUtil : SecurityAPIObject
    {



        /// <summary>
        /// EncodingUtil class constructor
        /// </summary>
        [SecuritySafeCritical]
        public EncodingUtil() : base()
        {

        }

        [SecuritySafeCritical]
        public string getEncoding()
        {
            return Global.GLOBAL_ENCODING;
        }

        [SecuritySafeCritical]
        public void setEncoding(string enc)
        {
            if (AvailableEncodingUtils.existsEncoding(enc))
            {
                Global.GLOBAL_ENCODING = enc;
            }
            else
            {
                this.error.setError("EU003", "set encoding error");
            }
        }
        /// <summary>
        /// byte array representation of the input UTF-8 text
        /// </summary>
        /// <param name="inputText">string UTF-8 text</param>
        /// <returns>byte array representation of the string UTF-8 input text</returns>
        [SecuritySafeCritical]
        public byte[] getBytes(string inputText)
        {
            byte[] output = null;
            String encoding = Global.GLOBAL_ENCODING;
            AvailableEncoding aEncoding = AvailableEncodingUtils.getAvailableEncoding(encoding, this.error);
            if (this.HasError())
            {
                return null;
            }
            String encodingString = AvailableEncodingUtils.valueOf(aEncoding);
            try
            {
                output = AvailableEncodingUtils.encapsulateeGetBytes(inputText, aEncoding, this.error);
                if (this.HasError())
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                this.error.setError("EU001", e.Message);
                return null;
            }

            this.error.cleanError();
            return output;
        }

        /// <summary>
        /// byte array representation of the input UTF-8 text
        /// </summary>
        /// <param name="inputBytes">byte array</param>
        /// <returns>byte array representation of the string UTF-8 input text</returns>
        [SecuritySafeCritical]
        public string getString(byte[] inputBytes)
        {
            String res = null;
            String encoding = Global.GLOBAL_ENCODING;

            AvailableEncoding aEncoding = AvailableEncodingUtils.getAvailableEncoding(encoding, this.error);
            if (this.HasError())
            {
                return "";
            }
            String encodingString = AvailableEncodingUtils.valueOf(aEncoding);
            try
            {
                res = AvailableEncodingUtils.encapsulateGetString(inputBytes, aEncoding, this.error).Replace("[\ufffd]", "");
                res = res.Replace("\x00", string.Empty);

                if (this.HasError())
                {
                    return "";
                }
            }
            catch (Exception e)
            {
                this.error.setError("EU002", e.Message);
                return "";
            }
            this.error.cleanError();
            return res.Trim();
        }
    }
}
