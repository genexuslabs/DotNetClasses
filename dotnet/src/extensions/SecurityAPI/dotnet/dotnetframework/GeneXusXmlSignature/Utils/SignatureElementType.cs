
using System.Security;
using log4net;
using SecurityAPICommons.Commons;

namespace GeneXusXmlSignature.GeneXusUtils
{
	[SecuritySafeCritical]
	public enum SignatureElementType
	{
		id, path, document
	}

	[SecuritySafeCritical]
	public static class SignatureElementTypeUtils
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(SignatureElementTypeUtils));
		public static string ValueOf(SignatureElementType signatureElementType, Error error)
		{
			logger.Debug("ValueOf");
			if (error == null) return "";
			switch (signatureElementType)
			{
				case SignatureElementType.id:
					return "id";
				case SignatureElementType.path:
					return "path";
				case SignatureElementType.document:
					return "document";
				default:
					error.setError("SET01", "Unrecognized SignatureElementType");
					logger.Error("Unrecognized SignatureElementType");
					return "";
			}
		}
	}
}
