
using System.Security;
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
		public static string ValueOf(SignatureElementType signatureElementType, Error error)
		{
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
					return "";
			}
		}
	}
}
