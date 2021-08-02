using SecurityAPICommons.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;


namespace GeneXusCryptography.AsymmetricUtils
{
    /// <summary>
    /// Implements AsymmetricEncryptionPadding enumerable
    /// </summary>
    [SecuritySafeCritical]
    public enum AsymmetricEncryptionPadding
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        NOPADDING, OAEPPADDING, PCKS1PADDING, ISO97961PADDING
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Implements AsymmetricEncryptionPadding associated functions
    /// </summary>
    [SecuritySafeCritical]
    public class AsymmetricEncryptionPaddingUtils
    {
        /// <summary>
        /// Mapping between string name and AsymmetricEncryptionPadding enum representation
        /// </summary>
        /// <param name="asymmetricEncryptionPadding">string asymmetricEncryptionPadding</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>AsymmetricEncryptionPadding enum representation</returns>
        public static AsymmetricEncryptionPadding getAsymmetricEncryptionPadding(string asymmetricEncryptionPadding, Error error)
        {
            switch (asymmetricEncryptionPadding.ToUpper().Trim())
            {
                case "NOPADDING":
                    return AsymmetricEncryptionPadding.NOPADDING;
                case "OAEPPADDING":
                    return AsymmetricEncryptionPadding.OAEPPADDING;
                case "PCKS1PADDING":
                    return AsymmetricEncryptionPadding.PCKS1PADDING;
                case "ISO97961PADDING":
                    return AsymmetricEncryptionPadding.ISO97961PADDING;
                default:
                    error.setError("AE003", "Unrecognized AsymmetricEncryptionPadding");
                    return AsymmetricEncryptionPadding.NOPADDING;
            }
        }
        /// <summary>
        /// Mapping between AsymmetricEncryptionPadding enum representation and string name 
        /// </summary>
        /// <param name="asymmetricEncryptionPadding">AsymmetricEncryptionPadding enum, padding name</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>string name of asymmetricEncryptionPadding</returns>
        public static string valueOf(AsymmetricEncryptionPadding asymmetricEncryptionPadding, Error error)
        {
            switch (asymmetricEncryptionPadding)
            {
                case AsymmetricEncryptionPadding.NOPADDING:
                    return "NOPADDING";
                case AsymmetricEncryptionPadding.OAEPPADDING:
                    return "OAEPPADDING";
                case AsymmetricEncryptionPadding.PCKS1PADDING:
                    return "PCKS1PADDING";
                case AsymmetricEncryptionPadding.ISO97961PADDING:
                    return "ISO97961PADDING";
                default:
                    error.setError("AE004", "Unrecognized AsymmetricEncryptionPadding");
                    return "";
            }
        }

        /// <summary>
        /// Manage Enumerable enum 
        /// </summary>
        /// <typeparam name="AsymmetricEncryptionPadding">AsymmetricEncryptionPaddingenum</typeparam>
        /// <returns>Enumerated values</returns>
        internal static IEnumerable<AsymmetricEncryptionPadding> GetValues<AsymmetricEncryptionPadding>()
        {
            return Enum.GetValues(typeof(AsymmetricEncryptionPadding)).Cast<AsymmetricEncryptionPadding>();
        }
    }

}
