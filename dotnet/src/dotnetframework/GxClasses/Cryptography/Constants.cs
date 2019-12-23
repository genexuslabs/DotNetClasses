using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneXus.Cryptography
{
    public static class Constants
    {
        public const string DEFAULT_HASH_ALGORITHM = "SHA256";
		public const string SECURITY_HASH_ALGORITHM = "SHA256";

		public static Encoding DEFAULT_ENCODING = Encoding.UTF8;
        public const PKCSStandard DEFAULT_SIGN_FORMAT = PKCSStandard.PKCS1;
        public const PKCSSignAlgorithm DEFAULT_SIGN_ALGORITHM = PKCSSignAlgorithm.RSA;        

        public enum PKCSStandard { PKCS1, PKCS7 };
        public enum PKCSSignAlgorithm { RSA, DSA };
    }
     
}
