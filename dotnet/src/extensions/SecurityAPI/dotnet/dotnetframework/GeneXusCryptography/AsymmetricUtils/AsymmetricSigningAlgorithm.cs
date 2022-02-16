
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Signers;
using SecurityAPICommons.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;


namespace GeneXusCryptography.AsymmetricUtils
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
        public static AsymmetricSigningAlgorithm GetAsymmetricSigningAlgorithm(string asymmetricSigningAlgorithm, Error error)
        {
			if (error == null) return AsymmetricSigningAlgorithm.NONE;
			if (asymmetricSigningAlgorithm == null)
			{
				error.setError("AE005", "Unrecognized AsymmetricSigningAlgorithm");
				return AsymmetricSigningAlgorithm.NONE;
			}

			switch (asymmetricSigningAlgorithm.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Trim())
            {
                case "RSA":
                    return AsymmetricSigningAlgorithm.RSA;
                case "ECDSA":
                    return AsymmetricSigningAlgorithm.ECDSA;
                default:
                    error.setError("AE005", "Unrecognized AsymmetricSigningAlgorithm");
                    return AsymmetricSigningAlgorithm.NONE;
            }
        }
        /// <summary>
        /// Mapping between AsymmetricSigningAlgorithm enum representation and string name 
        /// </summary>
        /// <param name="asymmetricSigningAlgorithm">AsymmetricSigningAlgorithm enum, algorithm name</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>string value of the algorithm</returns>
        public static string ValueOf(AsymmetricSigningAlgorithm asymmetricSigningAlgorithm, Error error)
        {
			if (error == null) return "";
            switch (asymmetricSigningAlgorithm)
            {
                case AsymmetricSigningAlgorithm.RSA:
                    return "RSA";
                case AsymmetricSigningAlgorithm.ECDSA:
                    return "ECDSA";
                default:
                    error.setError("AE005", "Unrecognized AsymmetricSigningAlgorithm");
                    return "";
            }
        }

		public static ISigner GetSigner(AsymmetricSigningAlgorithm asymmetricSigningAlgorithm, IDigest hash, Error error)
		{
			if(error == null) return null;
			if (hash == null)
			{
				error.setError("AE008", "Hash digest is null");
				return null;
			}
			ISigner sig = null;
			switch (asymmetricSigningAlgorithm)
			{
				case AsymmetricSigningAlgorithm.RSA:
					sig = new RsaDigestSigner(hash);
					break;
				case AsymmetricSigningAlgorithm.ECDSA:
					ECDsaSigner dsaSigner = new ECDsaSigner();
					sig = new DsaDigestSigner(dsaSigner, hash);
					break;
			}
			return sig;
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
