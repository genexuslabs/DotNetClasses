
using SecurityAPICommons.Commons;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Security;

namespace GeneXusCryptography.SymmetricUtils
{
    /// <summary>
    /// Implements SymmetricBlockPadding enumerated
    /// </summary>
    [SecuritySafeCritical]
    public enum SymmetricBlockPadding
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        NOPADDING, PKCS7PADDING, ISO10126D2PADDING, X923PADDING, ISO7816D4PADDING, ZEROBYTEPADDING, WITHCTS
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Implements SymmetricBlockPadding associated functions
    /// </summary>
    [SecuritySafeCritical]
    public static class SymmetricBlockPaddingUtils
    {
        /// <summary>
        /// Mapping between string name and SymmetricBlockPadding enum representation
        /// </summary>
        /// <param name="symmetricBlockPadding">string symmetricBlockPadding</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>SymmetricBlockPadding enum representation</returns>
        public static SymmetricBlockPadding getSymmetricBlockPadding(string symmetricBlockPadding, Error error)
        {
			if (error == null) return SymmetricBlockPadding.NOPADDING;
			if(symmetricBlockPadding == null)
			{
				error.setError("SB008", "Unrecognized SymmetricBlockPadding");
				return SymmetricBlockPadding.NOPADDING;
			}
            switch (symmetricBlockPadding.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Trim())
            {
                case "NOPADDING":
                    return SymmetricBlockPadding.NOPADDING;
                case "PKCS7PADDING":
                    return SymmetricBlockPadding.PKCS7PADDING;
                case "ISO10126D2PADDING":
                    return SymmetricBlockPadding.ISO10126D2PADDING;
                case "X923PADDING":
                    return SymmetricBlockPadding.X923PADDING;
                case "ISO7816D4PADDING":
                    return SymmetricBlockPadding.ISO7816D4PADDING;
                case "ZEROBYTEPADDING":
                    return SymmetricBlockPadding.ZEROBYTEPADDING;
                case "WITHCTS":
                    return SymmetricBlockPadding.WITHCTS;
                default:
                    error.setError("SB008", "Unrecognized SymmetricBlockPadding");
                    return SymmetricBlockPadding.NOPADDING;
            }
        }
        /// <summary>
        /// Mapping between SymmetricBlockPadding enum representation and string name
        /// </summary>
        /// <param name="symmetricBlockPadding">SymmetricBlockPadding enum, padding name</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>string name value of SymmetricBlockPadding</returns>
        public static string valueOf(SymmetricBlockPadding symmetricBlockPadding, Error error)
        {
			if(error == null) return "Unrecognized block padding";
			switch (symmetricBlockPadding)
            {
                case SymmetricBlockPadding.NOPADDING:
                    return "NOPADDING";
                case SymmetricBlockPadding.PKCS7PADDING:
                    return "PKCS7PADDING";
                case SymmetricBlockPadding.ISO10126D2PADDING:
                    return "ISO10126D2PADDING";
                case SymmetricBlockPadding.X923PADDING:
                    return "X923PADDING";
                case SymmetricBlockPadding.ISO7816D4PADDING:
                    return "ISO7816D4PADDING";
                case SymmetricBlockPadding.ZEROBYTEPADDING:
                    return "ZEROBYTEPADDING";
                case SymmetricBlockPadding.WITHCTS:
                    return "WITHCTS";
                default:
                    error.setError("SB009", "Unrecognized SymmetricBlockPadding");
                    return "Unrecognized block padding";
            }
        }
        /// <summary>
        /// Manage Enumerable enum 
        /// </summary>
        /// <typeparam name="SymmetricBlockPadding">SymmetricBlockPadding enum</typeparam>
        /// <returns>Enumerated values</returns>
        internal static IEnumerable<SymmetricBlockPadding> GetValues<SymmetricBlockPadding>()
        {
            return Enum.GetValues(typeof(SymmetricBlockPadding)).Cast<SymmetricBlockPadding>();
        }
    }
}
