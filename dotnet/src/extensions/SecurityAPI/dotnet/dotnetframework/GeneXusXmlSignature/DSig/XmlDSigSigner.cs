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

        public bool DoSignFile(string xmlFilePath, PrivateKeyManager key, CertificateX509 certificate, string outputPath, DSigOptions options)
        {
            this.error.cleanError();
            return doSignFilePKCS12(xmlFilePath, key, certificate, options.DSigSignatureType, options.Canonicalization, outputPath, options.KeyInfoType, options.XmlSchemaPath);
        }

        public bool DoSignFileElement(string xmlFilePath, string xPath, PrivateKeyManager key, CertificateX509 certificate, string outputPath, DSigOptions options)
        {
            this.error.cleanError();
            return doSignFileElementPKCS12(xmlFilePath, xPath, key, certificate, options.DSigSignatureType, options.Canonicalization, outputPath, options.KeyInfoType, options.XmlSchemaPath, options.IdentifierAttribute);
        }

        public string DoSign(string xmlInput, PrivateKeyManager key, CertificateX509 certificate, DSigOptions options)
        {
            this.error.cleanError();
            return doSignPKCS12(xmlInput, key, certificate, options.DSigSignatureType, options.Canonicalization, options.KeyInfoType, options.XmlSchemaPath);
        }

        public string DoSignElement(string xmlInput, string xPath, PrivateKeyManager key, CertificateX509 certificate, DSigOptions options)
        {
            this.error.cleanError();
            return doSignElementPKCS12(xmlInput, xPath, key, certificate, options.DSigSignatureType, options.Canonicalization, options.KeyInfoType, options.XmlSchemaPath, options.IdentifierAttribute);
        }

        public bool DoVerify(string xmlSigned, DSigOptions options)
        {
            this.error.cleanError();
            XmlDocument xmlDoc = SignatureUtils.documentFromString(xmlSigned, options.XmlSchemaPath, this.error);
            if (this.HasError())
            {
                return false;
            }
            return verify(xmlDoc, options.IdentifierAttribute);

        }


        public bool DoVerifyFile(string xmlFilePath, DSigOptions options)
        {
            this.error.cleanError();
            if (!SignatureUtils.validateExtensionXML(xmlFilePath))
            {
                this.error.setError("DS001", "The file is not an xml file");
                return false;
            }
            XmlDocument xmlDoc = SignatureUtils.documentFromFile(xmlFilePath, options.XmlSchemaPath, this.error);
            if (this.HasError())
            {
                return false;
            }
            return verify(xmlDoc, options.IdentifierAttribute);
        }

        public bool DoVerifyWithCert(string xmlSigned, CertificateX509 certificate, DSigOptions options)
        {
            this.error.cleanError();
            if (!certificate.Inicialized)
            {
                this.error.setError("DS003", "Certificate not loaded");
                return false;
            }
            if (SecurityUtils.compareStrings(certificate.getPublicKeyAlgorithm(), "ECDSA"))
            {
                this.error.setError("DS004", "XML signature with ECDSA keys is not implemented on Net Framework");
                return false;
            }
            XmlDocument xmlDoc = SignatureUtils.documentFromString(xmlSigned, options.XmlSchemaPath, this.error);
            if (this.HasError())
            {
                return false;
            }
            return verify(xmlDoc, certificate, options.IdentifierAttribute);
        }

        public bool DoVerifyFileWithCert(string xmlFilePath, CertificateX509 certificate, DSigOptions options)
        {
            this.error.cleanError();
            if (!certificate.Inicialized)
            {
                this.error.setError("DS005", "Certificate not loaded");
            }
            if (SecurityUtils.compareStrings(certificate.getPublicKeyAlgorithm(), "ECDSA"))
            {
                this.error.setError("DS006", "XML signature with ECDSA keys is not implemented on Net Framework");
                return false;
            }
            if (!SignatureUtils.validateExtensionXML(xmlFilePath))
            {
                this.error.setError("DS007", "The file is not an xml file");
                return false;
            }
            XmlDocument xmlDoc = SignatureUtils.documentFromFile(xmlFilePath, options.XmlSchemaPath, this.error);
            return verify(xmlDoc, certificate, options.IdentifierAttribute);




        }


        /******** EXTERNAL OBJECT PUBLIC METHODS - END ********/

        private bool doSignFilePKCS12(string xmlFilePath, PrivateKeyManager key, CertificateX509 certificate, string dSigType, string canonicalizationType, string outputPath, string keyInfoType, string xmlSchemaPath)
        {
            if (TransformsWrapperUtils.getTransformsWrapper(dSigType, this.error) != TransformsWrapper.ENVELOPED)
            {
                error.setError("DS009", "Not implemented DSigType");
                return false;
            }
            if (!SignatureUtils.validateExtensionXML(xmlFilePath))
            {
                this.error.setError("DS010", "Not XML file");
                return false;
            }
            if (!certificate.Inicialized)
            {
                this.error.setError("DS011", "Certificate not loaded");
                return false;
            }
            if (SecurityUtils.compareStrings(certificate.getPublicKeyAlgorithm(), "ECDSA"))
            {
                this.error.setError("DS004", "XML signature with ECDSA keys is not implemented on Net Framework");
                return false;
            }

            XmlDocument xmlDoc = SignatureUtils.documentFromFile(xmlFilePath, xmlSchemaPath, this.error);
            if (this.HasError())
            {
                return false;
            }
            string result = Sign(xmlDoc, key, certificate, dSigType, canonicalizationType, keyInfoType, "", "");
            if (result == null || SecurityUtils.compareStrings("", result))
            {
                this.error.setError("DS012", "Error generating signature");
                return false;
            }
            else
            {
                // string prefix = "<?xml version=”1.0″ encoding=”UTF-8″ ?>".Trim();
                string prefix = "";
                return SignatureUtils.writeToFile(result, outputPath, prefix, this.error);
            }
        }

        private bool doSignFileElementPKCS12(string xmlFilePath, string xPath, PrivateKeyManager key, CertificateX509 certificate, string dSigType, string canonicalizationType, string outputPath, string keyInfoType, string xmlSchemaPath, string id)
        {
            if (TransformsWrapperUtils.getTransformsWrapper(dSigType, this.error) != TransformsWrapper.ENVELOPED)
            {
                error.setError("DS013", "Not implemented DSigType");
                return false;
            }
            if (!SignatureUtils.validateExtensionXML(xmlFilePath))
            {
                this.error.setError("DS014", "Not XML file");
                return false;
            }
            if (!certificate.Inicialized)
            {
                this.error.setError("DS015", "Certificate not loaded");
            }
            if (SecurityUtils.compareStrings(certificate.getPublicKeyAlgorithm(), "ECDSA"))
            {
                this.error.setError("DS004", "XML signature with ECDSA keys is not implemented on Net Framework");
                return false;
            }
            XmlDocument xmlDoc = SignatureUtils.documentFromFile(xmlFilePath, xmlSchemaPath, this.error);
            if (this.HasError())
            {
                return false;
            }
            string result = Sign(xmlDoc, key, certificate, dSigType, canonicalizationType, keyInfoType, xPath, id);
            if (result == null || SecurityUtils.compareStrings("", result))
            {
                this.error.setError("DS016", "Error generating signature");
                return false;
            }
            else
            {
                // string prefix = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
                string prefix = "";
                return SignatureUtils.writeToFile(result, outputPath, prefix, this.error);
            }
        }

        private string doSignElementPKCS12(string xmlInput, string xPath, PrivateKeyManager key, CertificateX509 certificate, string dSigType, string canonicalizationType, string keyInfoType, string xmlSchemaPath, string id)
        {
            if (TransformsWrapperUtils.getTransformsWrapper(dSigType, this.error) != TransformsWrapper.ENVELOPED)
            {
                error.setError("DS017", "Not implemented DSigType");
                return "";
            }
            if (!certificate.Inicialized)
            {
                this.error.setError("DS018", "Certificate not loaded");
                return "";
            }
            if (SecurityUtils.compareStrings(certificate.getPublicKeyAlgorithm(), "ECDSA"))
            {
                this.error.setError("DS004", "XML signature with ECDSA keys is not implemented on Net Framework");
                return "";
            }
            XmlDocument xmlDoc = SignatureUtils.documentFromString(xmlInput, xmlSchemaPath, this.error);
            if (this.HasError())
            {
                return "";
            }
            return Sign(xmlDoc, key, certificate, dSigType, canonicalizationType, keyInfoType, xPath, id);
        }

        private string doSignPKCS12(string xmlInput, PrivateKeyManager key, CertificateX509 certificate, string dSigType, string canonicalizationType, string keyInfoType, string xmlSchemaPath)
        {
            if (TransformsWrapperUtils.getTransformsWrapper(dSigType, this.error) != TransformsWrapper.ENVELOPED)
            {
                error.setError("DS019", "Not implemented DSigType");
                return "";
            }
            if (!certificate.Inicialized)
            {
                this.error.setError("DS0220", "Certificate not loaded");
                return "";
            }
            if (SecurityUtils.compareStrings(certificate.getPublicKeyAlgorithm(), "ECDSA"))
            {
                this.error.setError("DS004", "XML signature with ECDSA keys is not implemented on Net Framework");
                return "";
            }
            XmlDocument xmlDoc = SignatureUtils.documentFromString(xmlInput, xmlSchemaPath, this.error);
            if (this.HasError())
            {
                return "";
            }
            return Sign(xmlDoc, key, certificate, dSigType, canonicalizationType, keyInfoType, "", "");
        }

        private string Sign(XmlDocument xmlInput, PrivateKeyManager key, CertificateX509 certificate,
                string dSigType, string canonicalizationType, string keyInfoType, string xpath, string id)
        {
            bool flag = inicializeInstanceVariables(key, certificate);
            if (!flag)
            {
                return "";
            }

            SignatureElementType signatureElementType;
            if (!SecurityUtils.compareStrings(xpath, ""))
            {
                if (xpath[0] == '#')
                {
                    signatureElementType = SignatureElementType.id;
                    if (id == null || SecurityUtils.compareStrings(id, ""))
                    {
                        this.error.setError("DS021", "identifier attribute name missing");
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
                    XmlDsigXPathTransform XPathTransform = CreateXPathTransform(xpath);
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

            KeyInfo keyInfo = createKeyInfo(certificate, keyInfoType);

            if (keyInfo != null)
            {
                signedXml.KeyInfo = keyInfo;
            }
            try
            {
                signedXml.ComputeSignature();
            }
            catch (Exception)
            {
                this.error.setError("DS023", "Error on signing");
                return "";
            }
            XmlElement xmlDigitalSignature = null;
            try
            {
                xmlDigitalSignature = signedXml.GetXml();
            }
            catch (Exception)
            {
                this.error.setError("DS028", "Error at signing");
                return "";
            }




            parentNode.AppendChild(xmlDigitalSignature);
            // xmlInput.DocumentElement.AppendChild(xmlInput.ImportNode(xmlDigitalSignature, true));


            return SignatureUtils.XMLDocumentToString(xmlInput, this.error);

        }

        private bool verify(XmlDocument doc, string id)
        {
            doc.PreserveWhitespace = true;
            XmlNodeList nodeList = null;
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

                return verifyDocument(doc, id);

            }
            else
            {
                return verifyPath(doc);
            }
        }

        private bool verifyPath(XmlDocument doc)
        {

            doc.PreserveWhitespace = true;

            XmlNodeList nodeList = doc.GetElementsByTagName("XPath");
            //java xmlsec hack
            if (nodeList == null || nodeList.Count == 0)
            {
                nodeList = doc.GetElementsByTagName("ds:XPath");
            }

            XmlNode node = nodeList[0];
            string path = node.InnerText;
            XmlNode signedNode = SignatureUtils.getNodeFromPath(doc, path, this.error);
            if (this.HasError())
            {
                return false;
            }
            XmlElement pathElement = signedNode as XmlElement;
            SignedXml signedXML = new SignedXml(pathElement);
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
                res = signedXML.CheckSignature();
            }
            catch (Exception)
            {
                this.error.setError("DS036", "Error on signature verification");
                return false;
            }

            return res;



        }

        private bool verifyID(XmlDocument doc, string identifier, string idValue)
        {
            XmlNode node = SignatureUtils.getNodeFromID(doc, identifier, idValue, this.error);
            XmlElement element = node as XmlElement;
            SignedXml signedXML = new SignedXml(element);
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
                res = signedXML.CheckSignature();
            }
            catch (Exception)
            {
                this.error.setError("DS037", "Error on signature verification");
                return false;
            }

            return res;

        }


        private bool verifyDocument(XmlDocument doc, string id)
        {
            /***WHITESPACES***/
            doc.PreserveWhitespace = true;
            string idValue = SignatureUtils.getIDNodeValue(doc);
            if (!SecurityUtils.compareStrings("", idValue) && idValue[0] == '#')
            {
                if (id == null || SecurityUtils.compareStrings("", id))
                {
                    this.error.setError("DS038", "The signature has a Reference URI by ID and ID attribute name is not defined");
                    return false;
                }
                return verifyID(doc, id, idValue);
            }
            SignedXml signedXML = new SignedXml(doc);
            XmlNodeList nodeList = doc.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl);
            //java xmlsec hack
            if (nodeList == null || nodeList.Count == 0)
            {
                nodeList = doc.GetElementsByTagName("ds:Signature");
            }
            bool res = false;
            try
            {
                signedXML.LoadXml((XmlElement)nodeList[0]);
                res = signedXML.CheckSignature();
            }
            catch (Exception)
            {
                this.error.setError("DS039", "Error on signature verification");

                return false;
            }
            return res;




        }

        private bool verify(XmlDocument doc, CertificateX509 certificate, string id)
        {
            /***WHITESPACES***/
            doc.PreserveWhitespace = true;

            SignedXml signedXml = new SignedXml(doc);
            XmlNodeList nodeList = doc.GetElementsByTagName("Signature");
            signedXml.LoadXml((XmlElement)nodeList[0]);
            return signedXml.CheckSignature(certificate.Cert, true);
        }

        private KeyInfo createKeyInfo(CertificateX509 certificate, string keyInfoType)
        {
            KeyInfo keyInfo = new KeyInfo();
            KeyInfoType kinfo = KeyInfoTypeUtils.getKeyInfoType(keyInfoType, this.error);
            switch (kinfo)
            {
                case KeyInfoType.KeyValue:

                    if (SecurityUtils.compareStrings(certificate.getPublicKeyAlgorithm(), "RSA"))
                    {
                        keyInfo.AddClause(new RSAKeyValue((RSA)certificate.getPublicKeyXML()));
                    }
                    else
                    {
                        keyInfo.AddClause(new DSAKeyValue((DSA)certificate.getPublicKeyXML()));
                    }
                    break;
                case KeyInfoType.X509Certificate:

                    KeyInfoX509Data keyInfoX509Data = new KeyInfoX509Data();
                    keyInfoX509Data.AddCertificate((X509Certificate)certificate.Cert);
                    keyInfoX509Data.AddSubjectName(certificate.Cert.SubjectName.Name);
                    keyInfoX509Data.AddIssuerSerial(certificate.Cert.IssuerName.Name, certificate.Cert.SerialNumber);
                    keyInfo.AddClause((KeyInfoClause)keyInfoX509Data);

                    break;
                case KeyInfoType.NONE:
                    keyInfo = null;
                    break;
            }
            return keyInfo;
        }


        private void addCanonTransform(Reference reference, CanonicalizerWrapper canonW)
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


        private bool inicializeInstanceVariables(PrivateKeyManager key, CertificateX509 certificate)
        {

            this.privateKey = key.getPrivateKeyForXML();
            if (this.privateKey == null)
            {
                this.error = key.GetError();
                return false;

            }
            this.publicKey = certificate.getPublicKeyXML();
            this.digest = certificate.getPublicKeyHash();
            this.asymAlgorithm = certificate.getPublicKeyAlgorithm();
            return true;
        }

        // Create the XML that represents the transform.
        private static XmlDsigXPathTransform CreateXPathTransform(string XPathString)
        {
            // Create a new XMLDocument object.
            XmlDocument doc = new XmlDocument();

            // Create a new XmlElement.
            XmlElement xPathElem = doc.CreateElement("XPath");

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


    }
}
