using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using log4net;
using GeneXus;
using System.Collections.Generic;

namespace GamSaml20.Utils
{
	internal class DSig
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(DSig));

		internal static string ValidateSignatures(XmlDocument doc, string certPath)
		{
			List<XmlElement> assertions = new List<XmlElement>();
			logger.Trace("ValidateSignatures");
			X509Certificate2 certificate = Keys.GetPublicX509Certificate2(certPath);
			if (certificate == null)
			{
				logger.Error("ValidateSignatures - Problems loading the certificate");
				return "";
			}

			XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
			nsManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
			XmlNodeList signatureNodeList = doc.SelectNodes("//ds:Signature", nsManager);

			foreach (XmlNode node in signatureNodeList)
			{
				try
				{
					XmlElement signature = node as XmlElement;
					if (!IsValidAlgorithm(signature)) //securty meassure
					{
						logger.Error($"ValidateSignatures - Unsupported algorithm: {GetAlgorithm(signature)}");
						return "";
					}

#if NETCORE
					SignedXml signedXml = new SignedXml(signature.OwnerDocument);
					signedXml.LoadXml(signature);
					if (!signedXml.CheckSignature(certificate, true))
					{
						return "";
					}else
					{
						string uri = GetReference(signature).Attributes.GetNamedItem("URI").Value.Replace("#", "").Trim();
						XmlElement signedElement = SamlAssertionUtils.FindNodeById(doc, "ID", uri);
						assertions.Add(signedElement);
					}
#else
					string uri = GetReference(signature).Attributes.GetNamedItem("URI").Value.Replace("#", "").Trim();
					XmlElement signedElement = SamlAssertionUtils.FindNodeById(doc, "ID", uri);
					logger.Debug($"ValidateSignatures - signedElement: {signedElement.LocalName} uri: {uri}");
					signedElement.RemoveChild(signature);
					if (!VerifySignature(signature, certificate, signedElement))
					{
						logger.Debug("ValidateSignatures - false");
						return "";
					}else
					{
						assertions.Add(signedElement);
					}	
#endif
				}
				catch (Exception ex)
				{
					logger.Error("ValidateSignatures - Exception", ex);
					return "";
				}
			}
			return SamlAssertionUtils.IsLogout(doc) ? SamlAssertionUtils.BuildXmlLogout(assertions, doc) : SamlAssertionUtils.BuildXmlLogin(assertions, doc);

		}

		private static string GetAlgorithm(XmlElement signature)
		{
			logger.Trace("GetAlgorithm");
			XmlNodeList nodeList = signature.GetElementsByTagName("SignatureMethod");
			if (nodeList.Count == 0)
			{
				nodeList = signature.GetElementsByTagName("ds:SignatureMethod");
			}
			return nodeList[0].Attributes["Algorithm"].Value;
		}

		private static bool IsValidAlgorithm(XmlElement signature)
		{
			logger.Trace("IsValidAlgorithm");
			switch (GetAlgorithm(signature).Trim())
			{
				case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha1":
				case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256":
				case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512":
					return true;
				default:
					return false;
			}
		}

#if !NETCORE
		private static bool VerifySignature(XmlElement signature, X509Certificate2 certificate, XmlElement signedElement)
		{
			XmlDocument doc = new XmlDocument();
			doc.PreserveWhitespace = true;
			doc.XmlResolver = null; //disable parser's DTD reading - security meassure

			doc.LoadXml(signedElement.OuterXml);

			SamlSignedXml signedXml = new SamlSignedXml(doc);
			signedXml.LoadXml(signature);
			return signedXml.CheckSignature(certificate, true);
		}

#endif
		private static XmlElement GetReference(XmlElement signature)
		{
			 XmlNodeList nodeList = signature.GetElementsByTagName("Reference");
             if(nodeList.Count == 0)
             {
                 nodeList = signature.GetElementsByTagName("ds:Reference");
             }
             return nodeList[0] as XmlElement;
		}

	}

#if !NETCORE
	internal class SamlSignedXml : SignedXml
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(SamlSignedXml));
		public SamlSignedXml(XmlDocument doc) : base(doc) { }

		public override XmlElement GetIdElement(XmlDocument doc, string id)
		{
			logger.Trace("GetIdElement");
			XmlElement element = SamlAssertionUtils.FindNodeById(doc, "ID", id);
			logger.Debug($"GetIdElement - Node name: {element.LocalName} id: {id}");
			logger.Debug($"GetIdElement - Node value: {element.OuterXml}");
			return element;
		}
		
	}
#endif

}
