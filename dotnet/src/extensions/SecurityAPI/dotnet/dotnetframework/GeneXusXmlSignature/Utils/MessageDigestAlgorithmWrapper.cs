using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using SecurityAPICommons.Commons;

namespace GeneXusXmlSignature.GeneXusUtils
{
    [SecuritySafeCritical]
    public enum MessageDigestAlgorithmWrapper
    {
        NONE, SHA1, SHA256, SHA512,
    }

    [SecuritySafeCritical]
    public class MessageDigestAlgorithmWrapperUtils
    {
        public static MessageDigestAlgorithmWrapper getMessageDigestAlgorithmWrapper(string messageDigestAlgorithmWrapper,
        Error error)
        {
            switch (messageDigestAlgorithmWrapper.ToUpper().Trim())
            {
                case "SHA1":
                    return MessageDigestAlgorithmWrapper.SHA1;
                case "SHA256":
                    return MessageDigestAlgorithmWrapper.SHA256;
                case "SHA512":
                    return MessageDigestAlgorithmWrapper.SHA512;
                default:
                    error.setError("MD001", "Algorithm not found: " + messageDigestAlgorithmWrapper);
                    return MessageDigestAlgorithmWrapper.NONE;
            }
        }

        public static string valueOf(MessageDigestAlgorithmWrapper messageDigestAlgorithmWrapper, Error error)
        {
            switch (messageDigestAlgorithmWrapper)
            {
                case MessageDigestAlgorithmWrapper.SHA1:
                    return "SHA1";
                case MessageDigestAlgorithmWrapper.SHA256:
                    return "SHA256";
                case MessageDigestAlgorithmWrapper.SHA512:
                    return "SHA512";
                default:
                    error.setError("MD002", "Not recognized digest algorithm");
                    return null;
            }
        }

        public static string getDigestMethod(MessageDigestAlgorithmWrapper messageDigestAlgorithmWrapper, Error error)
        {
            switch (messageDigestAlgorithmWrapper)
            {
                case MessageDigestAlgorithmWrapper.SHA1:
                    return Constants.ALGO_ID_DIGEST_SHA1;
                case MessageDigestAlgorithmWrapper.SHA256:
                    return Constants.ALGO_ID_DIGEST_SHA256;
                case MessageDigestAlgorithmWrapper.SHA512:
                    return Constants.ALGO_ID_DIGEST_SHA512;
                default:
                    error.setError("MD003", "Not recognized digest algorithm");
                    return null;
            }
        }

        /// <summary>
        /// Manage Enumerable enum 
        /// </summary>
        /// <typeparam name="MessageDigestAlgorithmWrapper">AsymmetricSigningAlgorithm enum</typeparam>
        /// <returns>Enumerated values</returns>
        internal static IEnumerable<MessageDigestAlgorithmWrapper> GetValues<MessageDigestAlgorithmWrapper>()
        {
            return Enum.GetValues(typeof(MessageDigestAlgorithmWrapper)).Cast<MessageDigestAlgorithmWrapper>();
        }
    }
}
