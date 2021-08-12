
using SecurityAPICommons.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;


namespace GeneXusCryptography.SymmetricUtils
{
    /// <summary>
    /// Implements SymmetricBlockMode enumerated
    /// </summary>
    [SecuritySafeCritical]
    public enum SymmetricBlockMode
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1707 // Identifiers should not contain underscores
		NONE, ECB, CBC, CFB, CTR, CTS, GOFB, OFB, OPENPGPCFB, SIC, /* AEAD */ AEAD_EAX, AEAD_GCM, AEAD_KCCM, AEAD_CCM
#pragma warning restore CA1707 // Identifiers should not contain underscores
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}


    /// <summary>
    /// Implements SymmetricBlockMode associated functions
    /// </summary>
    [SecuritySafeCritical]
    public static class SymmetricBlockModeUtils
    {
        /// <summary>
        /// Mapping between string name and SymmetricBlockMode enum representation
        /// </summary>
        /// <param name="symmetricBlockMode">string symmetricBlockMode</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>SymmetricBlockMode enum representation</returns>
        public static SymmetricBlockMode getSymmetricBlockMode(string symmetricBlockMode, Error error)
        {
			if (error == null) return SymmetricBlockMode.NONE;
			if(symmetricBlockMode == null)
			{
				error.setError("SB005", "Unrecognized SymmetricBlockMode");
				return SymmetricBlockMode.NONE;
			}

			switch (symmetricBlockMode.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Trim())
            {
                case "ECB":
                    return SymmetricBlockMode.ECB;
                case "CBC":
                    return SymmetricBlockMode.CBC;
                case "CFB":
                    return SymmetricBlockMode.CFB;
                case "CTS":
                    return SymmetricBlockMode.CTS;
                case "GOFB":
                    return SymmetricBlockMode.GOFB;
                case "OFB":
                    return SymmetricBlockMode.OFB;
                case "OPENPGPCFB":
                    return SymmetricBlockMode.OPENPGPCFB;
                case "SIC":
                    return SymmetricBlockMode.SIC;
                case "CTR":
                    return SymmetricBlockMode.CTR;

                /* AEAD */
                case "AEAD_EAX":
                    return SymmetricBlockMode.AEAD_EAX;
                case "AEAD_GCM":
                    return SymmetricBlockMode.AEAD_GCM;
                case "AEAD_KCCM":
                    return SymmetricBlockMode.AEAD_KCCM;
                case "AEAD_CCM":
                    return SymmetricBlockMode.AEAD_CCM;
                default:
                    error.setError("SB005", "Unrecognized SymmetricBlockMode");
                    return SymmetricBlockMode.NONE;
            }
        }


        /// <summary>
        /// Mapping between SymmetricBlockMode enum representation and string name
        /// </summary>
        /// <param name="symmetricBlockMode">SymmetricBlockMode enum, mode name</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>SymmetricBlockMode name value in string</returns>
        public static string valueOf(SymmetricBlockMode symmetricBlockMode, Error error)
        {
			if (error == null) return "Unrecognized operation mode";

			switch (symmetricBlockMode)
            {
                case SymmetricBlockMode.ECB:
                    return "ECB";
                case SymmetricBlockMode.CBC:
                    return "CBC";
                case SymmetricBlockMode.CFB:
                    return "CFB";
                case SymmetricBlockMode.CTS:
                    return "CTS";
                case SymmetricBlockMode.GOFB:
                    return "GOFB";
                case SymmetricBlockMode.OFB:
                    return "OFB";
                case SymmetricBlockMode.OPENPGPCFB:
                    return "OPENPGPCFB";
                case SymmetricBlockMode.SIC:
                    return "SIC";
                case SymmetricBlockMode.CTR:
                    return "CTR";

                /* AEAD */


                case SymmetricBlockMode.AEAD_EAX:
                    return "AEAD_EAX";
                case SymmetricBlockMode.AEAD_GCM:
                    return "AEAD_GCM";
                case SymmetricBlockMode.AEAD_KCCM:
                    return "AEAD_KCCM";
                case SymmetricBlockMode.AEAD_CCM:
                    return "AEAD_CCM";
                default:
                    error.setError("SB006", "Unrecognized SymmetricBlockMode");
                    return "Unrecognized operation mode";
            }
        }

        /// <summary>
        /// Find if a given mode is AEAD type
        /// </summary>
        /// <param name="symmetricBlockMode">SymmetricBlockMode enum, mode name</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>boolean true if operation mode is AEAD type</returns>
        public static bool isAEAD(SymmetricBlockMode symmetricBlockMode, Error error)
        {
			if (error == null) return false;
            switch (symmetricBlockMode)
            {
                case SymmetricBlockMode.AEAD_EAX:
                case SymmetricBlockMode.AEAD_GCM:
                case SymmetricBlockMode.AEAD_KCCM:
                case SymmetricBlockMode.AEAD_CCM:
                    return true;
                default:
                    error.setError("SB007", "Unrecognized Symmetric AEAD BlockMode");
                    return false;
            }
        }

        /// <summary>
        /// Manage Enumerable enum 
        /// </summary>
        /// <typeparam name="SymmetricBlockMode">SymmetricBlockMode enum</typeparam>
        /// <returns>Enumerated values</returns>
        internal static IEnumerable<SymmetricBlockMode> GetValues<SymmetricBlockMode>()
        {
            return Enum.GetValues(typeof(SymmetricBlockMode)).Cast<SymmetricBlockMode>();
        }
    }
}
