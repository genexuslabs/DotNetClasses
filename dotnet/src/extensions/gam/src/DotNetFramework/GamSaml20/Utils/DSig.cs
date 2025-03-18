using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using log4net;
using GeneXus;

namespace GamSaml20.Utils
{
	internal class DSig
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(DSig));


		internal static bool ValidateSignatures(XmlDocument xmlDoc, string certPath)
		{
			//validates all assertion's signatures and check that there is not assertions without signature
			logger.Trace("ValidateSignatures");
			try
			{
				XmlNodeList signatureNodeList = xmlDoc.GetElementsByTagName("Signature");
				if (signatureNodeList.Count == 0)
				{
					signatureNodeList = xmlDoc.GetElementsByTagName("ds:Signature");
				}
				X509Certificate2 certificate = Keys.GetPublicX509Certificate2(certPath);
				if (certificate == null)
				{
					logger.Error("SAML --- problems loading the certificate");
				}

				SignedXml signedXml = new SignedXml(xmlDoc);

				return true;
			}

			catch (Exception ex)
			{
				logger.Error("ValidateSignatures", ex);
				return false;
			}
		}
	}
}
