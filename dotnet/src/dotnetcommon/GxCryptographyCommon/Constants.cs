using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneXus.Cryptography
{
    public static class Constants
    {
        public const string DefaultHashAlgorithm = "SHA256";
		public const string SecurityHashAlgorithm= "SHA256";

		public static Encoding DEFAULT_ENCODING = Encoding.UTF8;
        public const PKCSStandard DefaultSignFormat= PKCSStandard.PKCS1;
        public const PKCSSignAlgorithm DefaultSignAlgorithm = PKCSSignAlgorithm.RSA;        

        public enum PKCSStandard { PKCS1, PKCS7 };
        public enum PKCSSignAlgorithm { RSA, DSA };
    }
     
}
