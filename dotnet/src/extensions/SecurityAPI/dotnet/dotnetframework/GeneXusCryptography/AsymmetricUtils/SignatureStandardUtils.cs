using System.Security;
using log4net;
using SecurityAPICommons.Commons;

namespace GeneXusCryptography.AsymmetricUtils
{
	[SecuritySafeCritical]
	public enum SignatureStandard
	{
		NONE, CMS,
	}

	[SecuritySafeCritical]
	public static class SignatureStandardUtils
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(SignatureStandardUtils));
		public static SignatureStandard getSignatureStandard(string signatureStandard,
																				 Error error)
		{
			logger.Debug("getSignatureStandard");
			if (error == null) return SignatureStandard.NONE;
			if (signatureStandard == null)
			{
				error.setError("SS001", "Unrecognized SignatureStandard");
				logger.Error("Unrecognized SignatureStandard");
				return SignatureStandard.NONE;
			}
			switch (signatureStandard.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Trim())
			{
				case "CMS":
					return SignatureStandard.CMS;
				default:
					error.setError("SS001", "Unrecognized SignatureStandard");
					logger.Error("Unrecognized SignatureStandard");
					return SignatureStandard.NONE;
			}
		}

		public static string valueOf(SignatureStandard signatureStandard, Error error)
		{
			logger.Debug("valueOf");
			if (error == null) return "";
			switch (signatureStandard)
			{
				case SignatureStandard.CMS:
					return "CMS";
				default:
					error.setError("SS002", "Unrecognized SignatureStandard");
					logger.Error("Unrecognized SignatureStandard");
					return "";
			}
		}
	}
}
