using GeneXusXmlSignature.GeneXusCommons;
using System;
using System.Security;
using System.Xml;
using GeneXusXmlSignature.GeneXusUtils;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Utils;
using SecurityAPICommons.Keys;

namespace GeneXusXmlSignature.GeneXusDSig
{
	[SecuritySafeCritical]
	public class XmlDSigSigner : SecurityAPIObject, IXmlDSigSignerObject
	{

		private AsymmetricAlgorithm privateKey;
		private AsymmetricAlgorithm publicKey;
		private string digest;
		private string asymAlgorithm;

		public XmlDSigSigner() : base()
		{

		}

		/******** EXTERNAL OBJECT PUBLIC METHODS - BEGIN ********/

		[SecuritySafeCritical]
		public bool DoSignFile(string xmlFilePath, PrivateKeyManager privateKey,
				CertificateX509 certificate, string outputPath, DSigOptions options)
		{
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput("xmlFilePath", xmlFilePath, this.error);
			SecurityUtils.validateObjectInput("privateKey", privateKey, this.error);
			SecurityUtils.validateObjectInput("certificate", certificate, this.error);
			SecurityUtils.validateStringInput("outputPath", outputPath, this.error);
			SecurityUtils.validateObjectInput("options", options, this.error);
			if (this.HasError())
			{
				return false;
			}

			/******* INPUT VERIFICATION - END *******/
			return Convert.ToBoolean(AxuiliarSign(xmlFilePath, privateKey, certificate, outputPath, options, true, "", null));
		}

		[SecuritySafeCritical]
		public string DoSign(string xmlInput, PrivateKeyManager privateKey,
				CertificateX509 certificate, DSigOptions options)
		{
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput("xmlInput", xmlInput, this.error);
			SecurityUtils.validateObjectInput("privateKey", privateKey, this.error);
			SecurityUtils.validateObjectInput("certificate", certificate, this.error);
			SecurityUtils.validateObjectInput("options", options, this.error);
			if (this.HasError())
			{
				return "";
			}

			/******* INPUT VERIFICATION - END *******/
			return AxuiliarSign(xmlInput, privateKey, certificate, "", options, false, "", null);
		}

		[SecuritySafeCritical]
		public bool DoSignFileElement(string xmlFilePath, string xPath,
				PrivateKeyManager privateKey, CertificateX509 certificate, string outputPath,
				DSigOptions options)
		{
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput("xmlFilePath", xmlFilePath, this.error);
			SecurityUtils.validateObjectInput("privateKey", privateKey, this.error);
			SecurityUtils.validateObjectInput("certificate", certificate, this.error);
			SecurityUtils.validateStringInput("outputPath", outputPath, this.error);
			SecurityUtils.validateObjectInput("options", options, this.error);
			if (this.HasError())
			{
				return false;
			}

			/******* INPUT VERIFICATION - END *******/
			return Convert.ToBoolean(AxuiliarSign(xmlFilePath, privateKey, certificate, outputPath, options, true, xPath, null));
		}

		[SecuritySafeCritical]
		public string DoSignElement(string xmlInput, string xPath, PrivateKeyManager privateKey,
				CertificateX509 certificate, DSigOptions options)
		{
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput("xmlInput", xmlInput, this.error);
			SecurityUtils.validateStringInput("xPath", xPath, this.error);
			SecurityUtils.validateObjectInput("privateKey", privateKey, this.error);
			SecurityUtils.validateObjectInput("certificate", certificate, this.error);
			SecurityUtils.validateObjectInput("options", options, this.error);
			if (this.HasError())
			{
				return "";
			}

			/******* INPUT VERIFICATION - END *******/
			return AxuiliarSign(xmlInput, privateKey, certificate, "", options, false, xPath, null);
		}

		[SecuritySafeCritical]
		public bool DoVerify(string xmlSigned, DSigOptions options)
		{
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput("xmlSigned", xmlSigned, this.error);
			SecurityUtils.validateObjectInput("options", options, this.error);
			if (this.HasError())
			{
				return false;
			}

			/******* INPUT VERIFICATION - END *******/
			return AuxiliarVerify(xmlSigned, options, null, false, false);
		}

		[SecuritySafeCritical]
		public bool DoVerifyFile(string xmlFilePath, DSigOptions options)
		{
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput("xmlFilePath", xmlFilePath, this.error);
			SecurityUtils.validateObjectInput("options", options, this.error);
			if (this.HasError())
			{
				return false;
			}
			/******* INPUT VERIFICATION - END *******/
			return AuxiliarVerify(xmlFilePath, options, null, true, false);
		}

		[SecuritySafeCritical]
		public bool DoVerifyWithCert(string xmlSigned, CertificateX509 certificate, DSigOptions options)
		{
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput("xmlSigned", xmlSigned, this.error);
			SecurityUtils.validateObjectInput("certificate", certificate, this.error);
			SecurityUtils.validateObjectInput("options", options, this.error);
			if (this.HasError())
			{
				return false;
			}
			/******* INPUT VERIFICATION - END *******/
			return AuxiliarVerify(xmlSigned, options, certificate, false, true);
		}

		[SecuritySafeCritical]
		public bool DoVerifyFileWithCert(string xmlFilePath, CertificateX509 certificate, DSigOptions options)
		{
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput("xmlFilePath", xmlFilePath, this.error);
			SecurityUtils.validateObjectInput("certificate", certificate, this.error);
			SecurityUtils.validateObjectInput("options", options, this.error);
			if (this.HasError())
			{
				return false;
			}
			/******* INPUT VERIFICATION - END *******/
			return AuxiliarVerify(xmlFilePath, options, certificate, true, true);
		}

		[SecuritySafeCritical]
		public bool DoSignFileWithPublicKey(string xmlFilePath, PrivateKey privateKey, SecurityAPICommons.Commons.PublicKey publicKey, string outputPath, DSigOptions options, string hash)
		{
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput("xmlFilePath", xmlFilePath, this.error);
			SecurityUtils.validateObjectInput("privateKey", privateKey, this.error);
			SecurityUtils.validateObjectInput("publicKey", publicKey, this.error);
			SecurityUtils.validateStringInput("outputPath", outputPath, this.error);
			SecurityUtils.validateObjectInput("options", options, this.error);
			SecurityUtils.validateStringInput("hash", hash, this.error);
			if (this.HasError())
			{
				return false;
			}

			/******* INPUT VERIFICATION - END *******/

			return Convert.ToBoolean(AxuiliarSign(xmlFilePath, privateKey, publicKey, outputPath, options, true, "", hash));
		}

		[SecuritySafeCritical]
		public string DoSignWithPublicKey(string xmlInput, PrivateKey privateKey, SecurityAPICommons.Commons.PublicKey publicKey, DSigOptions options, string hash)
		{
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput("xmlInput", xmlInput, this.error);
			SecurityUtils.validateObjectInput("privateKey", privateKey, this.error);
			SecurityUtils.validateObjectInput("publicKey", publicKey, this.error);
			SecurityUtils.validateObjectInput("options", options, this.error);
			SecurityUtils.validateStringInput("hash", hash, this.error);
			if (this.HasError())
			{
				return "";
			}

			/******* INPUT VERIFICATION - END *******/

			return AxuiliarSign(xmlInput, privateKey, publicKey, "", options, false, "", hash);
		}

		[SecuritySafeCritical]
		public bool DoSignFileElementWithPublicKey(string xmlFilePath, string xPath,
			PrivateKey privateKey, SecurityAPICommons.Commons.PublicKey publicKey, string outputPath,
			DSigOptions options, string hash)
		{
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput("xmlFilePath", xmlFilePath, this.error);
			SecurityUtils.validateStringInput("xPath", xPath, this.error);
			SecurityUtils.validateObjectInput("privateKey", privateKey, this.error);
			SecurityUtils.validateObjectInput("publicKey", publicKey, this.error);
			SecurityUtils.validateStringInput("outputPath", outputPath, this.error);
			SecurityUtils.validateObjectInput("options", options, this.error);
			SecurityUtils.validateStringInput("hash", hash, this.error);
			if (this.HasError())
			{
				return false;
			}

			/******* INPUT VERIFICATION - END *******/

			return Convert.ToBoolean(AxuiliarSign(xmlFilePath, privateKey, publicKey, outputPath, options, true, xPath, hash));
		}

		[SecuritySafeCritical]
		public string DoSignElementWithPublicKey(string xmlInput, string xPath, PrivateKey privateKey, SecurityAPICommons.Commons.PublicKey publicKey
			, DSigOptions options, string hash)
		{
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput("xmlInput", xmlInput, this.error);
			SecurityUtils.validateStringInput("xPath", xPath, this.error);
			SecurityUtils.validateObjectInput("privateKey", privateKey, this.error);
			SecurityUtils.validateObjectInput("publicKey", publicKey, this.error);
			SecurityUtils.validateObjectInput("options", options, this.error);
			SecurityUtils.validateStringInput("hash", hash, this.error);
			if (this.HasError())
			{
				return "";
			}

			/******* INPUT VERIFICATION - END *******/

			return AxuiliarSign(xmlInput, privateKey, publicKey, "", options, false, xPath, hash);
		}

		[SecuritySafeCritical]
		public bool DoVerifyWithPublicKey(string xmlSigned, SecurityAPICommons.Commons.PublicKey publicKey, DSigOptions options)
		{
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput("xmlSigned", xmlSigned, this.error);
			SecurityUtils.validateObjectInput("publicKey", publicKey, this.error);
			SecurityUtils.validateObjectInput("options", options, this.error);
			if (this.HasError())
			{
				return false;
			}
			/******* INPUT VERIFICATION - END *******/
			return AuxiliarVerify(xmlSigned, options, publicKey, false, true);
			
		}

		[SecuritySafeCritical]
		public bool DoVerifyFileWithPublicKey(string xmlFilePath, SecurityAPICommons.Commons.PublicKey publicKey, DSigOptions options)
		{
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput("xmlFilePath", xmlFilePath, this.error);
			SecurityUtils.validateObjectInput("publicKey", publicKey, this.error);
			SecurityUtils.validateObjectInput("options", options, this.error);
			if (this.HasError())
			{
				return false;
			}
			/******* INPUT VERIFICATION - END *******/

			return AuxiliarVerify(xmlFilePath, options, publicKey, true, true);
		}

		/******** EXTERNAL OBJECT PUBLIC METHODS - END ********/

		private string AxuiliarSign(string xmlInput, PrivateKey key,
		Key publicKey, string outputPath, DSigOptions options, bool isFile, string xPath, string hash)
		{
			if (TransformsWrapperUtils.getTransformsWrapper(options.DSigSignatureType,
					this.error) != TransformsWrapper.ENVELOPED)
			{
				error.setError("XD001", "Not implemented DSigType");
			}
			SecurityAPICommons.Commons.PublicKey cert = null;
			cert = (hash != null) ? (SecurityAPICommons.Commons.PublicKey)publicKey : (CertificateX509)publicKey;
			if (cert.HasError())
			{
				this.error = cert.GetError();
				return "";
			}
			else if (SecurityUtils.compareStrings(cert.getAlgorithm(), "ECDSA"))
			{
				this.error.setError("XD014", "XML signature with ECDSA keys is not implemented on Net Framework");
				return "";
			}

			XmlDocument xmlDoc = LoadDocument(isFile, xmlInput, options);
			if (this.HasError())
			{
				return "";
			}
			string result = Sign(xmlDoc, (PrivateKeyManager)key, cert, options.DSigSignatureType,
					options.Canonicalization, options.KeyInfoType, xPath, options.IdentifierAttribute, hash);
			if (isFile)
			{
				if ((result == null) || SecurityUtils.compareStrings(result, ""))
				{
					result = "false";
				}
				else
				{
					// string prefix = "<?xml version=”1.0″ encoding=”UTF-8″ ?>".Trim();
					string prefix = "";
				result = SignatureUtils.writeToFile(result, outputPath, prefix, this.error).ToString();
				}

			}

			return result;
		}

		private bool AuxiliarVerify(string xmlInput, DSigOptions options, Key key, bool isFile, bool withCert)
		{
			if (TransformsWrapperUtils.getTransformsWrapper(options.DSigSignatureType,
		this.error) != TransformsWrapper.ENVELOPED)
			{
				error.setError("XD001", "Not implemented DSigType");
			}
			XmlDocument xmlDoc = LoadDocument(isFile, xmlInput, options);
			if (this.HasError())
			{
				return false;
			}
			if (withCert)
			{
				 if (SecurityUtils.compareStrings(key.getAlgorithm(), "ECDSA"))
				{
					this.error.setError("XD014", "XML signature with ECDSA keys is not implemented on Net Framework");
					return false;
				}
				return Verify(xmlDoc, withCert, key, options);
			}
			else
			{
				return Verify(xmlDoc, withCert, null, options);
			}
		}

		private string Sign(XmlDocument xmlInput, PrivateKeyManager key, SecurityAPICommons.Commons.PublicKey certificate,
				string dSigType, string canonicalizationType, string keyInfoType, string xpath, string id, string hash)
		{
			inicializeInstanceVariables(key, certificate, hash);

			SignatureElementType signatureElementType;
			if (!SecurityUtils.compareStrings(xpath, ""))
			{
				if (xpath[0] == '#')
				{
					signatureElementType = SignatureElementType.id;
					if (id == null || SecurityUtils.compareStrings(id, ""))
					{
						this.error.setError("XD003", "Identifier attribute name missing");
						return "";
					}
				}
				else
				{
					signatureElementType = SignatureElementType.path;
				}
			}
			else
			{
				signatureElementType = SignatureElementType.document;
			}

			/***WHITESPACES***/
			xmlInput.PreserveWhitespace = true;
			CanonicalizerWrapper canon = CanonicalizerWrapperUtils.getCanonicalizerWrapper(canonicalizationType, this.error);


			CanonicalizerWrapper canonW = CanonicalizerWrapperUtils.getCanonicalizerWrapper(canonicalizationType, this.error);
			if (this.HasError())
			{
				return "";
			}

			Reference reference = new Reference();

			XmlNode parentNode;
			SignedXml signedXml;
			switch (signatureElementType)
			{
				case SignatureElementType.path:
					XmlNode pathNode = SignatureUtils.getNodeFromPath(xmlInput, xpath, this.error);
					XmlElement pathElement = pathNode as XmlElement;
					if (this.HasError() || pathElement == null)
					{
						return "";
					}
					parentNode = pathNode.ParentNode;



					signedXml = new SignedXml(pathElement);
					XmlDsigXPathTransform XPathTransform = CreateXPathTransform(xpath, xmlInput);
					//reference.Uri = "";
					reference.Uri = pathNode.NamespaceURI;
					reference.AddTransform(XPathTransform);
					break;
				case SignatureElementType.id:
					XmlNode idNode = SignatureUtils.getNodeFromID(xmlInput, id, xpath, this.error);
					XmlElement idElement = idNode as XmlElement;

					if (this.HasError() || idElement == null)
					{
						return "";
					}

					reference.Uri = xpath;
					signedXml = new SignedXml(idElement);
					parentNode = idNode.ParentNode;
					
					break;
				default:
					signedXml = new SignedXml(xmlInput);
					parentNode = xmlInput.DocumentElement;
					reference.Uri = "";
					break;
			}

			signedXml.SigningKey = this.privateKey;
			signedXml.SignedInfo.CanonicalizationMethod = CanonicalizerWrapperUtils.getCanonicalizationMethodAlorithm(canonW, this.error);
			if (this.HasError())
			{
				return "";
			}

			XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
			reference.AddTransform(env);

			addCanonTransform(reference, canonW);

			signedXml.AddReference(reference);

			KeyInfo keyInfo = createKeyInfo(certificate, keyInfoType, hash);
			if(this.HasError()) { return ""; }
			if (keyInfo != null)
			{
				signedXml.KeyInfo = keyInfo;
			}
			try
			{
				signedXml.ComputeSignature();
			}
			catch (Exception e)
			{
				this.error.setError("XD004", e.Message);
				return "";
			}
			XmlElement xmlDigitalSignature = null;
			try
			{
				xmlDigitalSignature = signedXml.GetXml();
			}
			catch (Exception ex)
			{
				this.error.setError("XD005", ex.Message);
				return "";
			}




			parentNode.AppendChild(xmlDigitalSignature);
			// xmlInput.DocumentElement.AppendChild(xmlInput.ImportNode(xmlDigitalSignature, true));


			return SignatureUtils.XMLDocumentToString(xmlInput);

		}

		private bool Verify(XmlDocument doc, bool withCert, Key certificate, DSigOptions options)
		{
			doc.PreserveWhitespace = true;
			XmlNodeList nodeList = null;
			SignedXml signedXML = null;
			XmlNode node = null;
			//searching for an element
			try
			{
				nodeList = doc.GetElementsByTagName("XPath");
			}
			catch (Exception)
			{
				//NOOP
			}
			if (nodeList == null || nodeList.Count == 0)
			{
				//search for id

				string idValue = SignatureUtils.getIDNodeValue(doc);
				if (idValue != null)
				{
					node = SignatureUtils.getNodeFromID(doc, options.IdentifierAttribute, idValue, this.error);
				}
				else
				{
					//all document
					node = doc.DocumentElement;

				}

			}
			else
			{
				//search for xpath
				//java xmlsec hack
				if (nodeList == null || nodeList.Count == 0)
				{
					nodeList = doc.GetElementsByTagName("ds:XPath");
				}

				XmlNode nodee = nodeList[0];
				string path = nodee.InnerText;
				node = SignatureUtils.getNodeFromPath(doc, path, this.error);
				if (this.HasError())
				{
					return false;
				}
			}


			XmlElement element = node as XmlElement;
			signedXML = new SignedXml(element);




			XmlNodeList signatureNodes = doc.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl);
			//java xmlsec hack
			if (signatureNodes == null || signatureNodes.Count == 0)
			{
				signatureNodes = doc.GetElementsByTagName("ds:Signature");
			}
			bool res = false;
			try
			{
				signedXML.LoadXml((XmlElement)signatureNodes[0]);
				if (withCert)
				{
					//res = signedXML.CheckSignature(((CertificateX509)certificate).Cert, true);
					res = signedXML.CheckSignature(((SecurityAPICommons.Commons.PublicKey)certificate).getAsymmetricAlgorithm());
				}
				else
				{
					res = signedXML.CheckSignature();
				}
			}
			catch (Exception e)
			{
				this.error.setError("XD006", e.Message);
				return false;
			}

			return res;
		}

		private KeyInfo createKeyInfo(SecurityAPICommons.Commons.PublicKey certificate, string keyInfoType, string hash)
		{
			KeyInfo keyInfo = new KeyInfo();
			KeyInfoType kinfo = KeyInfoTypeUtils.getKeyInfoType(keyInfoType, this.error);
			switch (kinfo)
			{
				case KeyInfoType.KeyValue:
					if (SecurityUtils.compareStrings(certificate.getAlgorithm(), "RSA"))
					{
						keyInfo.AddClause(new RSAKeyValue((RSA)certificate.getAsymmetricAlgorithm()));
					}
					else
					{
						keyInfo.AddClause(new DSAKeyValue((DSA)certificate.getAsymmetricAlgorithm()));
					}
					break;
				case KeyInfoType.X509Certificate:
					if (hash != null)
					{
						this.error.setError("XD002", "The file included is a Public Key, cannot include a certificate on the signature");
						return null;
					}
					X509Certificate2 x509Certificate = ((CertificateX509)certificate).Cert;
					KeyInfoX509Data keyInfoX509Data = new KeyInfoX509Data();
					keyInfoX509Data.AddCertificate(x509Certificate);
					keyInfoX509Data.AddSubjectName(x509Certificate.SubjectName.Name);
					keyInfoX509Data.AddIssuerSerial(x509Certificate.IssuerName.Name, x509Certificate.SerialNumber);
					keyInfo.AddClause((KeyInfoClause)keyInfoX509Data);

					break;
				case KeyInfoType.NONE:
					keyInfo = null;
					break;
			}
			return keyInfo;
		}


		private static void addCanonTransform(Reference reference, CanonicalizerWrapper canonW)
		{

			switch (canonW)
			{
				case CanonicalizerWrapper.ALGO_ID_C14N_OMIT_COMMENTS:
					reference.AddTransform(new XmlDsigC14NTransform());
					break;
				case CanonicalizerWrapper.ALGO_ID_C14N_WITH_COMMENTS:
					reference.AddTransform(new XmlDsigC14NWithCommentsTransform());
					break;
				case CanonicalizerWrapper.ALGO_ID_C14N_EXCL_OMIT_COMMENTS:
					reference.AddTransform(new XmlDsigExcC14NTransform());
					break;
				case CanonicalizerWrapper.ALGO_ID_C14N_EXCL_WITH_COMMENTS:
					reference.AddTransform(new XmlDsigExcC14NWithCommentsTransform());
					break;
			}
		}


		private void inicializeInstanceVariables(PrivateKeyManager key, SecurityAPICommons.Commons.PublicKey certificate, string hash)
		{

			this.privateKey = key.getPrivateKeyForXML();
			this.publicKey = certificate.getAsymmetricAlgorithm();			
			this.asymAlgorithm = certificate.getAlgorithm();
			if (hash == null)
			{
				this.digest = ((CertificateX509)certificate).getPublicKeyHash();
			}
			else
			{
				this.digest = hash;
			}
		}

		// Create the XML that represents the transform.
		private static XmlDsigXPathTransform CreateXPathTransform(string XPathString, XmlDocument xdoc)
		{
			// Create a new XMLDocument object.
			XmlDocument doc = new XmlDocument() { XmlResolver = null };

			// Create a new XmlElement.
			XmlElement xPathElem = doc.CreateElement("XPath");

			XmlNamespaceManager nsMgr = new XmlNamespaceManager(xdoc.NameTable);
			XmlNodeList _xmlNameSpaceList = doc.SelectNodes(@"//namespace::*[not(. = ../../namespace::*)]");

			//nsMgr = new XmlNamespaceManager(xDoc.NameTable);

			foreach (XmlNode nsNode in _xmlNameSpaceList)
			{
				xPathElem.SetAttribute(nsNode.LocalName, nsNode.Value);
			}
			
			// Set the element text to the value
			// of the XPath string.
			xPathElem.InnerText = XPathString;

			// Create a new XmlDsigXPathTransform object.
			XmlDsigXPathTransform xForm = new XmlDsigXPathTransform();

			// Load the XPath XML from the element. 
			xForm.LoadInnerXml(xPathElem.SelectNodes("."));

			// Return the XML that represents the transform.
			return xForm;
		}



		private XmlDocument LoadDocument(bool isFile, String path, DSigOptions options)
		{
			XmlDocument xmlDoc = null;
			if (isFile)
			{
				if (!SignatureUtils.validateExtensionXML(path))
				{
					this.error.setError("XD013", "Not XML file");
					return null;
				}
				xmlDoc = SignatureUtils.documentFromFile(path, options.XmlSchemaPath, this.error);

			}
			else
			{
				xmlDoc = SignatureUtils.documentFromString(path, options.XmlSchemaPath, this.error);
			}
			return xmlDoc;
		}
	}
}
