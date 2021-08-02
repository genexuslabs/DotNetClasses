using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using SecurityAPICommons.Commons;

namespace GeneXusXmlSignature.GeneXusUtils
{
    [SecuritySafeCritical]
    public enum XmlSignatureWrapper
    {
#pragma warning disable CA1707 // Identifiers should not contain underscores
		NONE, RSA_SHA1, RSA_SHA256, RSA_SHA512, ECDSA_SHA1, ECDSA_SHA256,
#pragma warning restore CA1707 // Identifiers should not contain underscores
	}

    [SecuritySafeCritical]
    public static class XMLSignatureWrapperUtils
    {
        public static XmlSignatureWrapper getXMLSignatureWrapper(string xMLSignatureWrapper, Error error)
        {
			if(error == null) return XmlSignatureWrapper.NONE;
			if (xMLSignatureWrapper == null)
			{
				error.setError("XS001", "Unrecognized algorithm: " + xMLSignatureWrapper);
				return XmlSignatureWrapper.NONE;
			}
            switch (xMLSignatureWrapper.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Trim())
            {
                case "RSA_SHA1":
                    return XmlSignatureWrapper.RSA_SHA1;
                case "RSA_SHA256":
                    return XmlSignatureWrapper.RSA_SHA256;
                case "RSA_SHA512":
                    return XmlSignatureWrapper.RSA_SHA512;
                case "ECDSA_SHA1":
                    return XmlSignatureWrapper.ECDSA_SHA1;
                case "ECDSA_SHA256":
                    return XmlSignatureWrapper.ECDSA_SHA256;
                default:
                    error.setError("XS001", "Unrecognized algorithm: " + xMLSignatureWrapper);
                    return XmlSignatureWrapper.NONE;
            }
        }

        public static string valueOf(XmlSignatureWrapper xMLSignatureWrapper, Error error)
        {
			if (error == null) return null;
            switch (xMLSignatureWrapper)
            {
                case XmlSignatureWrapper.RSA_SHA1:
                    return "RSA_SHA1";
                case XmlSignatureWrapper.RSA_SHA256:
                    return "RSA_SHA256";
                case XmlSignatureWrapper.RSA_SHA512:
                    return "RSA_SHA512";
                case XmlSignatureWrapper.ECDSA_SHA1:
                    return "ECDSA_SHA1";
                case XmlSignatureWrapper.ECDSA_SHA256:
                    return "ECDSA_SHA256";
                default:
                    error.setError("XS002", "Unrecognized algorithm");
                    return null;
            }
        }

        public static string getSignatureMethodAlgorithm(XmlSignatureWrapper xMLSignatureWrapper, Error error)
        {
			if (error == null) return null;
            switch (xMLSignatureWrapper)
            {
                case XmlSignatureWrapper.RSA_SHA1:
                    return Constants.ALGO_ID_SIGNATURE_RSA_SHA1;
                case XmlSignatureWrapper.RSA_SHA256:
                    return Constants.ALGO_ID_SIGNATURE_RSA_SHA256;
                case XmlSignatureWrapper.RSA_SHA512:
                    return Constants.ALGO_ID_SIGNATURE_RSA_SHA512;
                case XmlSignatureWrapper.ECDSA_SHA1:
                    return Constants.ALGO_ID_SIGNATURE_ECDSA_SHA1;
                case XmlSignatureWrapper.ECDSA_SHA256:
                    return Constants.ALGO_ID_SIGNATURE_ECDSA_SHA256;
                default:
                    error.setError("XS003", "Unrecognized algorithm");
                    return null;
            }
        }

        public static string getCanonicalizationTransformation(XmlSignatureWrapper xMLSignatureWrapper, Error error)
        {
            return XMLSignatureWrapperUtils.valueOf(xMLSignatureWrapper, error);
        }

        /// <summary>
        /// Manage Enumerable enum 
        /// </summary>
        /// <typeparam name="XMLSignatureWrapper">XMLSignatureWrapper enum</typeparam>
        /// <returns>Enumerated values</returns>
        internal static IEnumerable<XMLSignatureWrapper> GetValues<XMLSignatureWrapper>()
        {
            return Enum.GetValues(typeof(XMLSignatureWrapper)).Cast<XMLSignatureWrapper>();
        }
    }
}
