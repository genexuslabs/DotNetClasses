using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Security.Cryptography.Xml;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using GeneXus.Cryptography.CryptoException;
using GeneXus.Utils;


namespace GeneXus.Cryptography
{
    //http://msdn.microsoft.com/en-us/library/system.security.cryptography.xml.signedxml.addreference.aspx
    public class GXXMLDsig
    {
        /*
         * Signatures Types:
            Enveloped signature — the signature is added to the document that was signed.
            Enveloping signature — the signature contains the document that was signed.
            Detached signature — the signature is distributed separate from the document that was signed.
         * 
         * Samples: http://msdn.microsoft.com/en-us/library/windows/desktop/ms759193(v=vs.85).aspx
         * */

        private X509Certificate2 _cert;
        private GXCertificate _gxCert;

        private List<string> _references;
        private string _canonicalizationMethod;
        private bool _detached;
        private GxStringCollection _keyInfoClauses;
        private int _lastError;
        private string _lastErrorDescription;
        private bool _validateCertificates;

        public GxStringCollection KeyInfoClauses
        {
            get { return _keyInfoClauses; }
            set { _keyInfoClauses = value; }
        }

        public bool Detached
        {
            get { return _detached; }
            set { _detached = value; }
        }


        public GXCertificate Certificate
        {
            get { return _gxCert; }
            set
            {
                _gxCert = value;
                _cert = _gxCert.Certificate;
            }
        }



        public GXXMLDsig()
        {
            _references = new List<string>();
            _keyInfoClauses = new GxStringCollection() { "X509IssuerSerial", "X509SubjectName", "X509Certificate" };
            _canonicalizationMethod = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";

        }

        public void AddReference(string reference)
        {
            _references.Add(reference);
        }

        public string SignElements(string originalXml, string xPath)
        {          
            if (!String.IsNullOrEmpty(originalXml) && !String.IsNullOrEmpty(xPath) && !AnyError)
            {
                XmlDocument doc = null;
                try
                {
                    doc = GetXmlDoc(originalXml);
                }
                catch (XmlException)
                {
                    SetError(2);
                    return string.Empty;
                }
#pragma warning disable SCS0003 // XPath injection possible in {1} argument passed to '{0}'
                XmlNodeList nodeList = doc.DocumentElement.SelectNodes(xPath);
#pragma warning restore SCS0003 // XPath injection possible in {1} argument passed to '{0}'
                Dictionary<XmlNode, XmlNode> nodeToReplace = new Dictionary<XmlNode, XmlNode>();
                foreach (XmlNode tag in nodeList)
                {
                    XmlDocument partialDoc = GetXmlDoc(ReplaceFirst(tag.OuterXml, "xmlns=\"" + tag.NamespaceURI + "\"", "")); //.NET Adds default namespace to XML representation.
                    Sign(partialDoc.DocumentElement);
                    nodeToReplace.Add(tag, doc.ImportNode(partialDoc.DocumentElement, true));
                }

                foreach (var item in nodeToReplace)
                {
                    XmlNode old = item.Key;
                    XmlNode newNode = item.Value;
                    old.ParentNode.ReplaceChild(newNode, old);
                }
                return doc.OuterXml;
            }
            else
            {
                SetError(1);
                return string.Empty;
            }
        }

        public string Sign(string xml)
        {
            SetError(0);
            string signedXml = string.Empty;
            if (!String.IsNullOrEmpty(xml) && !AnyError)
            {
                if (_gxCert.HasPrivateKey())
                {
                    XmlDocument doc = null;
                    try
                    {
                        doc = GetXmlDoc(xml);
                        Sign(doc.DocumentElement);
                        signedXml = doc.OuterXml;
                    }
                    catch (XmlException)
                    {
                        SetError(2);
                    }

                }
                else
                {
                    SetError(5);
                }
            }
            else
            {
                SetError(1);
            }
            return signedXml;
        }

        // Enveloped or detached 'internally' supported ony. 
        internal void Sign(XmlElement rootElement)
        {
            XmlDocument doc = rootElement.OwnerDocument;

            SignedXml signedXml = new SignedXml(rootElement);
            signedXml.SigningKey = _cert.PrivateKey;
            KeyInfo keyInfo = GetKeyInfo();            
            signedXml.SignedInfo.CanonicalizationMethod = _canonicalizationMethod;
            if (keyInfo != null)
            {
                signedXml.KeyInfo = keyInfo;
            }

            if (_references.Count > 0)
            {
                foreach (var reference in _references)
                {
                    signedXml.AddReference(NewReference(reference));
                }
            }
            else
            {
                signedXml.AddReference(NewReference(string.Empty)); //Sign entire xml document.
            }

            try
            {
                signedXml.ComputeSignature();
            }
            catch (CryptographicException e)
            {
                throw new DigitalSignException(e);
            }

            XmlElement xmlDSig = signedXml.GetXml();
            if (_detached)
            {
                doc.LoadXml(xmlDSig.OuterXml);
            }
            else
            {
                //Enveloped
                if (_references.Count == 1 && _references[0].StartsWith("#"))
                {
                    XmlElement idElement = signedXml.GetIdElement(doc, _references[0].Substring(1));
                    idElement.AppendChild(doc.ImportNode(xmlDSig, true));
                }
                else
                {
                    rootElement.AppendChild(doc.ImportNode(xmlDSig, true));
                }
            }

        }

        private static XmlElement GetSignatureNode(XmlDocument document)
        {
            if (document.DocumentElement.LocalName.Equals("Signature"))
            {
                return document.DocumentElement;
            }
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(document.NameTable);
            nsMgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            var signatureElement = document.DocumentElement.SelectSingleNode("//ds:Signature", nsMgr) as XmlElement;
            if (signatureElement == null)
                throw new InvalidSignatureException("Verification failed: No Signature was found in the document.");
            return signatureElement;
        }

        public bool Verify(string text)
        {
            XmlDocument doc = null;
            try
            {
                doc = GetXmlDoc(text, false);
            }
            catch (XmlException)
            {
                SetError(2);
                return false;
            }
            XmlElement signatureNode = null;
            try
            {               
                signatureNode = GetSignatureNode(doc);
            }
            catch (InvalidSignatureException)
            {
                SetError(8);
                return false;
            }
            X509Certificate2 cert = GetVerificationCertificate(signatureNode);

            // Create a new SignedXml object and pass it the XML document class.
            SignedXml signedXml = new SignedXml(doc);

            // Load the first <signature> node.  
            signedXml.LoadXml(signatureNode);

            // Check the signature and return the result.
            bool result = signedXml.CheckSignature();
            if (!result || ValidateCertificate)
            {
                result = signedXml.CheckSignature(cert, !ValidateCertificate);
            }
            if (!result)
            {
                SetError(9);
            }
            return result;
        }

        private X509Certificate2 GetVerificationCertificate(XmlNode signatureNode)
        {
            X509Certificate2 cert = null;
            try
            {
                var SignatureNode = signatureNode.SelectSingleNode("//*[local-name()='X509Certificate']");
                string x509certificate = SignatureNode.InnerText;
                cert = new X509Certificate2(Encoding.Unicode.GetBytes(x509certificate));
            }
            catch (Exception)
            {
            }

            if (cert == null)
            {
                cert = _cert;
            }
            return cert;
        }

        private static XmlDocument GetXmlDoc(string originalXml)
        {
            return GetXmlDoc(originalXml, false);
        }

        private static XmlDocument GetXmlDoc(string originalXml, bool preserveWS)
        {
            XmlDocument doc = new XmlDocument { PreserveWhitespace = preserveWS };
            doc.LoadXml(originalXml);
            doc = Canonize(doc);
            return doc;
        }

        private static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        private KeyInfo GetKeyInfo()
        {
            KeyInfo info = null;
            if (_keyInfoClauses.Count > 0)
            {
                info = new KeyInfo();
                KeyInfoX509Data clause = new KeyInfoX509Data();
                foreach (var item in _keyInfoClauses)
                {
                    switch (item)
                    {
                        case "RSAKeyValue":
                            RSACryptoServiceProvider pKey = (RSACryptoServiceProvider)_cert.PublicKey.Key;
                            info.AddClause(new RSAKeyValue((RSA)pKey));
                            break;
                        case "X509IssuerSerial":
                            clause.AddIssuerSerial(_cert.IssuerName.Name, _cert.SerialNumber);
                            break;
                        case "X509SubjectName":
                            clause.AddSubjectName(_cert.SubjectName.Name);
                            break;
                        case "X509Certificate":
                            clause.AddCertificate(_cert);
                            break;
                    }
                }
                info.AddClause(clause);
            }
            return info;
        }

        private Reference NewReference(string uri)
        {
            Reference reference = new Reference();
            reference.Uri = uri;
            if (!_detached)
            {
                XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
                reference.AddTransform(env);
            }
            Transform canonicalTransform = GetCanonicalTransform();
            if (canonicalTransform != null)
            {
                reference.AddTransform(canonicalTransform);
            }
            return reference;
        }

        private Transform GetCanonicalTransform()
        {
            Transform t = null;
            switch (_canonicalizationMethod)
            {
                case "http://www.w3.org/TR/2001/REC-xml-c14n-20010315":
                    t = new XmlDsigC14NTransform();
                    break;
                case "http://www.w3.org/2006/12/xml-c14n11#WithComments":
                    t = new XmlDsigC14NTransform(true);
                    break;
                case "http://www.w3.org/2000/09/xmldsig#base64":
                    t = new XmlDsigBase64Transform();
                    break;
                case "http://www.w3.org/TR/2002/REC-xml-exc-c14n-20020718/":
                    t = new XmlDsigExcC14NTransform();
                    break;
            }
            return t;
        }

        internal static XmlDocument Canonize(XmlDocument doc)
        {
            XmlDsigC14NTransform t = new XmlDsigC14NTransform();
            t.LoadInput(doc);
            Stream s = (Stream)t.GetOutput(typeof(Stream));
            StreamReader reader = new StreamReader(s);
            doc.LoadXml(reader.ReadToEnd());
            return doc;
        }


        public bool ValidateCertificate
        {
            get { return _validateCertificates; }
            set { _validateCertificates = value; }

        }
        private void SetError(int errorCode)
        {
            SetError(errorCode, string.Empty);
        }

        private void SetError(int errorCode, string errDsc)
        {
            _lastError = errorCode;
            switch (errorCode)
            {
                case 0:
                    _lastErrorDescription = string.Empty;
                    break;
                case 1:
                    _lastErrorDescription = "Cannot sign an empty xml.";
                    break;
                case 2:
                    _lastErrorDescription = "Input XML is not valid";
                    break;
                case 3:
                    break;
                case 4:
                    _lastErrorDescription = "Certificate not initialized";
                    break;
                case 5:
                    _lastErrorDescription = "Certificate does not contain private key.";
                    break;
                case 6:
                    _lastErrorDescription = "Signature Exception";
                    break;
                case 9:
                    _lastErrorDescription = "Signature is not valid";
                    break;
                case 8:
                    _lastErrorDescription = "Signature element was not found";
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(errDsc))
            {
                if (!string.IsNullOrEmpty(_lastErrorDescription))
                {
                    _lastErrorDescription = String.Format("{0} - {1}", _lastErrorDescription, errDsc);
                }
                else
                {
                    _lastErrorDescription = errDsc;
                }
            }
        }

        private bool AnyError
        {
            get
            {
                if (_gxCert == null || (_gxCert != null && !_gxCert.CertLoaded()))
                {
                    SetError(4); //Certificate not initialized
                    return true;
                }
                return false;
            }
        }

        public int ErrCode
        {
            get
            {
                return _lastError;
            }
        }

        public string ErrDescription
        {
            get
            {
                return _lastErrorDescription;
            }
        }

    }
}
