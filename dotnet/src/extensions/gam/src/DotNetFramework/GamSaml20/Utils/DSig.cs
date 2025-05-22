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

		internal static string NFE_ID_ATT_NAME = "ID";
		internal static string REFERENCE_URI = "URI";

		internal static bool ValidateSignatures(XmlDocument doc, string certPath)
		{
			logger.Trace("ValidateSignatures");
			X509Certificate2 certificate = Keys.GetPublicX509Certificate2(certPath);
			if (certificate == null)
			{
				logger.Error("ValidateSignatures - Problems loading the certificate");
				return false;
			}

			XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
			nsManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
			XmlNodeList signatureNodeList = doc.SelectNodes("//ds:Signature", nsManager);

			foreach (XmlNode node in signatureNodeList)
			{
				try
				{
					XmlElement signature = node as XmlElement;
					SignedXml signedXml = new SignedXml(signature.OwnerDocument);
					signedXml.LoadXml(signature);
					if (!signedXml.CheckSignature(certificate, true))
					{
						return false;
					}
				}
				catch (Exception ex)
				{
					logger.Error("ValidateSignatures - Exception", ex);
					return false;
				}
			}
			return true;

		}

		private static XmlElement FindNodeById(XmlDocument doc, string name, string value)
		{
			logger.Trace("FindNodeById");
			XmlNodeList nodeList = doc.SelectNodes($"//*[@{name}]");
			if (nodeList == null)
			{
				logger.Error("FindNodeById -could not find node by id");
				return null;
			}

			foreach (XmlNode node in nodeList)
			{
				if (node.Attributes[name].Value.Equals(value))
				{
					return node as XmlElement;
				}
			}
			logger.Error("FindNodeById - could not find node");
			return null;
		}
	}

}
