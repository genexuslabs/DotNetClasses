using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;

namespace GeneXusXmlSignature.GeneXusUtils
{
    [SecuritySafeCritical]
    internal class Constants
    {
        /****CONSTANTS****/
        internal static string SignatureSpecNS = "http://www.w3.org/2000/09/xmldsig#";

        internal static string MoreAlgorithmsSpecNS = "http://www.w3.org/2001/04/xmldsig-more#";

        internal static string XML_DSIG_NS_MORE_07_05 = "http://www.w3.org/2007/05/xmldsig-more#";



        /****CANONICALIZATION****/
        internal static string ALGO_ID_C14N_OMIT_COMMENTS = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";

        internal static string ALGO_ID_C14N_WITH_COMMENTS = ALGO_ID_C14N_OMIT_COMMENTS + "#WithComments";

        internal static string ALGO_ID_C14N_EXCL_OMIT_COMMENTS = "http://www.w3.org/2001/10/xml-exc-c14n#";

        internal static string ALGO_ID_C14N_EXCL_WITH_COMMENTS = ALGO_ID_C14N_EXCL_OMIT_COMMENTS + "WithComments";

        internal static string ALGO_ID_C14N11_OMIT_COMMENTS = "http://www.w3.org/2006/12/xml-c14n11";

        internal static string ALGO_ID_C14N11_WITH_COMMENTS = ALGO_ID_C14N11_OMIT_COMMENTS + "#WithComments";


        /****MESSAGE DIGEST ALGORITHM****/

        internal static string ALGO_ID_DIGEST_NOT_RECOMMENDED_MD5 = Constants.MoreAlgorithmsSpecNS + "md5";

        internal static string ALGO_ID_DIGEST_SHA1 = Constants.SignatureSpecNS + "sha1";

        internal static string ALGO_ID_DIGEST_SHA224 = Constants.MoreAlgorithmsSpecNS + "sha224";

        internal static string ALGO_ID_DIGEST_SHA256 = Constants.EncryptionSpecNS + "sha256";

        internal static string ALGO_ID_DIGEST_SHA384 = Constants.MoreAlgorithmsSpecNS + "sha384";

        internal static string ALGO_ID_DIGEST_SHA512 = Constants.EncryptionSpecNS + "sha512";

        internal static string ALGO_ID_DIGEST_RIPEMD160 = Constants.EncryptionSpecNS + "ripemd160";

        internal static string ALGO_ID_DIGEST_WHIRLPOOL = Constants.XML_DSIG_NS_MORE_07_05 + "whirlpool";

        internal static string ALGO_ID_DIGEST_SHA3_224 = Constants.XML_DSIG_NS_MORE_07_05 + "sha3-224";

        internal static string ALGO_ID_DIGEST_SHA3_256 = Constants.XML_DSIG_NS_MORE_07_05 + "sha3-256";

        internal static string ALGO_ID_DIGEST_SHA3_384 = Constants.XML_DSIG_NS_MORE_07_05 + "sha3-384";

        internal static string ALGO_ID_DIGEST_SHA3_512 = Constants.XML_DSIG_NS_MORE_07_05 + "sha3-512";

        /****ENCRYPTION CONSTANTS****/

        internal static string EncryptionSpecNS = "http://www.w3.org/2001/04/xmlenc#";

        /****TRANSFORMS****/

        internal static string TRANSFORM_ENVELOPED_SIGNATURE = Constants.SignatureSpecNS + "enveloped-signature";

        /****XMLSIGNATURE****/

        internal static string ALGO_ID_SIGNATURE_RSA = Constants.SignatureSpecNS + "rsa-sha1";


        internal static string ALGO_ID_SIGNATURE_RSA_SHA1 = Constants.SignatureSpecNS + "rsa-sha1";


        internal static string ALGO_ID_SIGNATURE_NOT_RECOMMENDED_RSA_MD5 = Constants.MoreAlgorithmsSpecNS + "rsa-md5";


        internal static string ALGO_ID_SIGNATURE_RSA_RIPEMD160 = Constants.MoreAlgorithmsSpecNS + "rsa-ripemd160";


        internal static string ALGO_ID_SIGNATURE_RSA_SHA224 = Constants.MoreAlgorithmsSpecNS + "rsa-sha224";


        internal static string ALGO_ID_SIGNATURE_RSA_SHA256 = Constants.MoreAlgorithmsSpecNS + "rsa-sha256";


        internal static string ALGO_ID_SIGNATURE_RSA_SHA384 = Constants.MoreAlgorithmsSpecNS + "rsa-sha384";


        internal static string ALGO_ID_SIGNATURE_RSA_SHA512 = Constants.MoreAlgorithmsSpecNS + "rsa-sha512";

        internal static string ALGO_ID_SIGNATURE_ECDSA_SHA1 = "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha1";


        internal static string ALGO_ID_SIGNATURE_ECDSA_SHA224 = "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha224";


        internal static string ALGO_ID_SIGNATURE_ECDSA_SHA256 = "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha256";


        internal static string ALGO_ID_SIGNATURE_ECDSA_SHA384 = "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha384";


        internal static string ALGO_ID_SIGNATURE_ECDSA_SHA512 = "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha512";


        internal static string ALGO_ID_SIGNATURE_ECDSA_RIPEMD160 = "http://www.w3.org/2007/05/xmldsig-more#ecdsa-ripemd160";
    }

}
