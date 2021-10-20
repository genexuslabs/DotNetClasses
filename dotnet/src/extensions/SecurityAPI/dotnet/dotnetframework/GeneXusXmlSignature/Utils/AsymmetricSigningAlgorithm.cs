using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using SecurityAPICommons.Commons;

namespace GeneXusXmlSignature.GeneXusUtils
{
    /// <summary>
    /// Implements AsymmetricSigningAlgorithm enumerated
    /// </summary>
    [SecuritySafeCritical]
    public enum AsymmetricSigningAlgorithm
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        NONE, RSA, ECDSA
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Implements AsymmetricSigningAlgorithm assiciated functions
    /// </summary>
    [SecuritySafeCritical]
    public static class AsymmetricSigningAlgorithmUtils
    {

        /// <summary>
        /// Mapping between string name and AsymmetricSigningAlgorithm enum representation
        /// </summary>
        /// <param name="asymmetricSigningAlgorithm">string asymmetricSigningAlgorithm</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>AsymmetricSigningAlgorithm enum representation</returns>
        public static AsymmetricSigningAlgorithm getAsymmetricSigningAlgorithm(string asymmetricSigningAlgorithm, Error error)
        {
			if(error == null) return AsymmetricSigningAlgorithm.NONE;
			if ( asymmetricSigningAlgorithm == null)
			{
				error.setError("AS001", "Unrecognized AsymmetricSigningAlgorithm");
				return AsymmetricSigningAlgorithm.NONE;
			}
            switch (asymmetricSigningAlgorithm.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Trim())
            {
                case "RSA":
                    return AsymmetricSigningAlgorithm.RSA;
                case "ECDSA":
                    return AsymmetricSigningAlgorithm.ECDSA;
                default:
                    error.setError("AS001", "Unrecognized AsymmetricSigningAlgorithm");
                    return AsymmetricSigningAlgorithm.NONE;
            }
        }
        /// <summary>
        /// Mapping between AsymmetricSigningAlgorithm enum representation and string name 
        /// </summary>
        /// <param name="asymmetricSigningAlgorithm">AsymmetricSigningAlgorithm enum, algorithm name</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>string value of the algorithm</returns>
        public static string valueOf(AsymmetricSigningAlgorithm asymmetricSigningAlgorithm, Error error)
        {
			if (error == null) return "";
            switch (asymmetricSigningAlgorithm)
            {
                case AsymmetricSigningAlgorithm.RSA:
                    return "RSA";
                case AsymmetricSigningAlgorithm.ECDSA:
                    return "ECDSA";
                default:
                    error.setError("AS002", "Unrecognized AsymmetricSigningAlgorithm");
                    return "";
            }
        }

        /// <summary>
        /// Manage Enumerable enum 
        /// </summary>
        /// <typeparam name="AsymmetricSigningAlgorithm">AsymmetricSigningAlgorithm enum</typeparam>
        /// <returns>Enumerated values</returns>
        internal static IEnumerable<AsymmetricSigningAlgorithm> GetValues<AsymmetricSigningAlgorithm>()
        {
            return Enum.GetValues(typeof(AsymmetricSigningAlgorithm)).Cast<AsymmetricSigningAlgorithm>();
        }
    }
}
