
using SecurityAPICommons.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;


namespace GeneXusCryptography.AsymmetricUtils
{
    /// <summary>
    /// Implements enumerable AsymmetricEncryptionAlgorithm 
    /// </summary>
    [SecuritySafeCritical]
    public enum AsymmetricEncryptionAlgorithm
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        NONE, RSA,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Implements AsymmetricEncryptionAlgorithm assoaciated functions
    /// </summary>
    [SecuritySafeCritical]
    public static class AsymmetricEncryptionAlgorithmUtils
    {

        /// <summary>
        /// Mapping between string name and AsymmetricEncryptionAlgorithm enum representation
        /// </summary>
        /// <param name="asymmetricEncryptionAlgorithm">string asymmetricEncryptionAlgorithm</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>AsymmetricEncryptionAlgorithm enum representation</returns>
        public static AsymmetricEncryptionAlgorithm getAsymmetricEncryptionAlgorithm(string asymmetricEncryptionAlgorithm, Error error)
        {
			if (error == null) return AsymmetricEncryptionAlgorithm.NONE;
			if(asymmetricEncryptionAlgorithm == null)
			{
				error.setError("AE001", "Unrecognized AsymmetricEncryptionAlgorithm");
				return AsymmetricEncryptionAlgorithm.NONE;
			}
            switch (asymmetricEncryptionAlgorithm.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Trim())
            {
                case "RSA":
                    return AsymmetricEncryptionAlgorithm.RSA;
                default:
                    error.setError("AE001", "Unrecognized AsymmetricEncryptionAlgorithm");
                    return AsymmetricEncryptionAlgorithm.NONE;
            }
        }
        /// <summary>
        ///  Mapping between AsymmetricEncryptionAlgorithm enum representation and string name
        /// </summary>
        /// <param name="asymmetricEncryptionAlgorithm">AsymmetricEncryptionAlgorithm enum, algorithm name</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>string asymmetricEncryptionAlgorithm name</returns>
        public static string valueOf(AsymmetricEncryptionAlgorithm asymmetricEncryptionAlgorithm, Error error)
        {
			if (error == null) return "";
            switch (asymmetricEncryptionAlgorithm)
            {
                case AsymmetricEncryptionAlgorithm.RSA:
                    return "RSA";
                default:
                    error.setError("AE002", "Unrecognized AsymmetricEncryptionAlgorithm");
                    return "";
            }

        }

        /// <summary>
        /// Manage Enumerable enum 
        /// </summary>
        /// <typeparam name="AsymmetricEncryptionAlgorithm">AsymmetricEncryptionAlgorithm enum</typeparam>
        /// <returns>Enumerated values</returns>
        internal static IEnumerable<AsymmetricEncryptionAlgorithm> GetValues<AsymmetricEncryptionAlgorithm>()
        {
            return Enum.GetValues(typeof(AsymmetricEncryptionAlgorithm)).Cast<AsymmetricEncryptionAlgorithm>();
        }

    }
}
