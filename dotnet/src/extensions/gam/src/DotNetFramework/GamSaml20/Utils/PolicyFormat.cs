using GeneXus;
using log4net;

namespace GamSaml20.Utils
{
	internal enum PolicyFormat
	{
		NONE, UNSPECIFIED, EMAIL, ENCRYPTED, TRANSIENT, ENTITY, KERBEROS, WIN_DOMAIN_QUALIFIED, X509_SUBJECT
	}

	internal class PolicyFormatUtil
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(PolicyFormatUtil));

		internal static PolicyFormat GetPolicyFormat(string format)
		{
			logger.Trace("GetPolicyFormat");
			switch (format.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Trim())
			{
				case "UNSPECIFIED":
					return PolicyFormat.UNSPECIFIED;
				case "EMAIL":
					return PolicyFormat.EMAIL;
				case "ENCRYPTED":
					return PolicyFormat.ENCRYPTED;
				case "TRANSIENT":
					return PolicyFormat.TRANSIENT;
				case "ENTITY":
					return PolicyFormat.ENTITY;
				case "KERBEROS":
					return PolicyFormat.KERBEROS;
				case "WIN_DOMAIN_QUALIFIED":
					return PolicyFormat.WIN_DOMAIN_QUALIFIED;
				case "X509_SUBJECT":
					return PolicyFormat.X509_SUBJECT;
				default:
					logger.Error("Unknown policy format");
					return PolicyFormat.NONE;
			}
		}

		internal static string ValueOf(PolicyFormat format)
		{
			logger.Trace("ValueOf");
			switch (format)
			{
				case PolicyFormat.UNSPECIFIED:
					return "UNSPECIFIED";
				case PolicyFormat.EMAIL:
					return "EMAIL";
				case PolicyFormat.ENCRYPTED:
					return "ENCRYPTED";
				case PolicyFormat.TRANSIENT:
					return "TRANSIENT";
				case PolicyFormat.ENTITY:
					return "ENTITY";
				case PolicyFormat.KERBEROS:
					return "KERBEROS";
				case PolicyFormat.WIN_DOMAIN_QUALIFIED:
					return "WIN_DOMAIN_QUALIFIED";
				case PolicyFormat.X509_SUBJECT:
					return "X509_SUBJECT";
				default:
					logger.Error("Unknown policy format");
					return string.Empty;
			}
		}

		internal static string GetPolicyFormatXmlValue(PolicyFormat format)
		{
			logger.Trace("GetPolicyFormatXmlValue");
			switch (format)
			{
				case PolicyFormat.UNSPECIFIED:
					return @"urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified";
				case PolicyFormat.EMAIL:
					return @"urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress";
				case PolicyFormat.ENCRYPTED:
					return @"urn:oasis:names:tc:SAML:2.0:nameid-format:encrypted";
				case PolicyFormat.TRANSIENT:
					return @"urn:oasis:names:tc:SAML:2.0:nameid-format:transient";
				case PolicyFormat.ENTITY:
					return @"urn:oasis:names:tc:SAML:2.0:nameid-format:entity";
				case PolicyFormat.KERBEROS:
					return @"urn:oasis:names:tc:SAML:2.0:nameid-format:kerberos";
				case PolicyFormat.WIN_DOMAIN_QUALIFIED:
					return @"urn:oasis:names:tc:SAML:2.0:nameid-format:windowsDomainQualifiedName";
				case PolicyFormat.X509_SUBJECT:
					return @"urn:oasis:names:tc:SAML:2.0:nameid-format:x509Subject";
				default:
					logger.Error("Unknown policy format");
					return string.Empty;
			}
		}



	}
}
