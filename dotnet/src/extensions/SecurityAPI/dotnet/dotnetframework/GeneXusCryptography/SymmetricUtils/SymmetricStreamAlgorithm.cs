
using SecurityAPICommons.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using log4net;


namespace GeneXusCryptography.SymmetricUtils
{
	/// <summary>
	/// Implements SymmetricStreamAlgorithm enumerated
	/// </summary>
	[SecuritySafeCritical]
	public enum SymmetricStreamAlgorithm
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		NONE, RC4, HC256, CHACHA20, SALSA20, XSALSA20, ISAAC
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}

	/// <summary>
	/// Implements SymmetricStreamAlgorithm associated functions
	/// </summary>
	[SecuritySafeCritical]
	public static class SymmetricStreamAlgorithmUtils
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(SymmetricStreamAlgorithmUtils));
		/// <summary>
		/// Mapping between String name and SymmetricStreamAlgorithm enum representation
		/// </summary>
		/// <param name="symmetricStreamAlgorithm">String symmetricStreamAlgorithm</param>
		/// <param name="error">Error type for error management</param>
		/// <returns>SymmetricStreamAlgorithm enum representation</returns>
		public static SymmetricStreamAlgorithm getSymmetricStreamAlgorithm(String symmetricStreamAlgorithm, Error error)
		{
			logger.Debug("getSymmetricStreamAlgorithm");
			if (error == null) return SymmetricStreamAlgorithm.NONE;
			if (symmetricStreamAlgorithm == null)
			{
				error.setError("SSA05", "Unrecognized SymmetricStreamAlgorithm");
				logger.Error("Unrecognized SymmetricStreamAlgorithm");
				return SymmetricStreamAlgorithm.NONE;
			}

			switch (symmetricStreamAlgorithm.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Trim())
			{
				case "RC4":
					return SymmetricStreamAlgorithm.RC4;
				case "HC256":
					return SymmetricStreamAlgorithm.HC256;
				case "CHACHA20":
					return SymmetricStreamAlgorithm.CHACHA20;
				case "SALSA20":
					return SymmetricStreamAlgorithm.SALSA20;
				case "XSALSA20":
					return SymmetricStreamAlgorithm.XSALSA20;
				case "ISAAC":
					return SymmetricStreamAlgorithm.ISAAC;
				default:
					error.setError("SSA01", "Unrecognized SymmetricStreamAlgorithm");
					logger.Error("Unrecognized SymmetricStreamAlgorithm");
					return SymmetricStreamAlgorithm.NONE;
			}
		}
		/// <summary>
		/// Mapping between SymmetricStreamAlgorithm enum representationa and String name
		/// </summary>
		/// <param name="symmetrcStreamAlgorithm">SymmetrcStreamAlgorithm enum, algorithm name</param>
		/// <param name="error">Error type for error management</param>
		/// <returns>String SymmetrcStreamAlgorithm name value</returns>
		public static String valueOf(SymmetricStreamAlgorithm symmetrcStreamAlgorithm, Error error)
		{
			logger.Debug("valueOf");
			if (error == null) return "Unrecognized algorithm";
			switch (symmetrcStreamAlgorithm)
			{
				case SymmetricStreamAlgorithm.RC4:
					return "RC4";
				case SymmetricStreamAlgorithm.HC256:
					return "HC256";
				case SymmetricStreamAlgorithm.CHACHA20:
					return "CHACHA20";
				case SymmetricStreamAlgorithm.SALSA20:
					return "SALSA20";
				case SymmetricStreamAlgorithm.XSALSA20:
					return "XSALSA20";
				case SymmetricStreamAlgorithm.ISAAC:
					return "ISAAC";
				default:
					error.setError("SSA02", "Unrecognized SymmetricStreamAlgorithm");
					logger.Error("Unrecognized SymmetricStreamAlgorithm");
					return "Unrecognized algorithm";
			}
		}

		/// <summary>
		/// Returns key size for the algorithm in bits
		/// </summary>
		/// <param name="algorithm">SymmetrcStreamAlgorithm enum, algorithm name</param>
		/// <param name="error">Error type for error management</param>
		/// <returns>array int with fixed length 3 with key, if array[0]=0 is range, else fixed values</returns>
		public static int[] getKeySize(SymmetricStreamAlgorithm algorithm, Error error)
		{
			logger.Debug("getKeySize");
			if (error == null) return null;
			int[] keySize = new int[3];
			switch (algorithm)
			{
				case SymmetricStreamAlgorithm.RC4:
					keySize[0] = 0;
					keySize[1] = 40;
					keySize[2] = 2048;
					break;
				case SymmetricStreamAlgorithm.HC256:
				case SymmetricStreamAlgorithm.XSALSA20:
					keySize[0] = 1;
					keySize[1] = 256;
					break;
				case SymmetricStreamAlgorithm.CHACHA20:
				case SymmetricStreamAlgorithm.SALSA20:
					keySize[0] = 1;
					keySize[1] = 128;
					keySize[2] = 256;
					break;
				case SymmetricStreamAlgorithm.ISAAC:
					keySize[0] = 0;
					keySize[1] = 32;
					keySize[2] = 8192;
					break;
				default:
					error.setError("SSA03", "Unrecognized SymmetricStreamAlgorithm");
					logger.Error("Unrecognized SymmetricStreamAlgorithm");
					break;
			}
			return keySize;
		}


		/// <summary>
		/// Containsinformation about algorithm's IV
		/// </summary>
		/// <param name="algorithm">SymmetrcStreamAlgorithm enum, algorithm name</param>
		/// <param name="error">Error type for error management</param>
		/// <returns>true if the algorithm uses an IV or nonce, false if it do not</returns>
		internal static bool usesIV(SymmetricStreamAlgorithm algorithm, Error error)
		{
			logger.Debug("usesIV");
			switch (algorithm)
			{
				case SymmetricStreamAlgorithm.RC4:
				case SymmetricStreamAlgorithm.ISAAC:
					return false;
				case SymmetricStreamAlgorithm.HC256:
				case SymmetricStreamAlgorithm.SALSA20:
				case SymmetricStreamAlgorithm.CHACHA20:
				case SymmetricStreamAlgorithm.XSALSA20:
					return true;
				default:
					error.setError("SSA04", "Unrecognized SymmetricStreamAlgorithm");
					logger.Error("Unrecognized SymmetricStreamAlgorithm");
					return true;
			}

		}

		/// <summary>
		/// Manage Enumerable enum 
		/// </summary>
		/// <typeparam name="SymmetricStreamAlgorithm">SymmetricStreamAlgorithm enum</typeparam>
		/// <returns>Enum values</returns>
		internal static IEnumerable<SymmetricStreamAlgorithm> GetValues<SymmetricStreamAlgorithm>()
		{
			return Enum.GetValues(typeof(SymmetricStreamAlgorithm)).Cast<SymmetricStreamAlgorithm>();
		}
	}
}
