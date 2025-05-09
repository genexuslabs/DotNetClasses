using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;
using GamSaml20.Utils.Xml;
using GeneXus;
using log4net;
namespace GamSaml20.Utils
{
	internal class SamlAssertionUtils
	{

		private static readonly ILog logger = LogManager.GetLogger(typeof(SamlAssertionUtils));

		internal static XElement CreateLogoutRequest(string id, string issuer, string nameID, string sessionIndex, string destination)
		{
			logger.Trace("CreateLogoutRequest");
			string issueInstant = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"); //UTC

			XNamespace saml2p = "urn:oasis:names:tc:SAML:2.0:protocol";
			XNamespace saml2 = "urn:oasis:names:tc:SAML:2.0:assertion";

			XElement request = new XElement(saml2p + "LogoutRequest",
				new XAttribute("ID", id),
				new XAttribute("Version", "2.0"),
				new XAttribute("IssueInstant", issueInstant),
				new XAttribute("Destination", destination),
				new XAttribute("Reason", "urn:oasis:names:tc:SAML:2.0:logout:user"),

				new XElement(saml2 + "Issuer", issuer),
				new XElement(saml2 + "NameID", nameID),
				new XElement(saml2p + "SessionIndex", sessionIndex)
				);
			logger.Debug($"CreateLogoutRequest - XML request: {request.ToString()}");
			return request;
		}

		internal static bool IsLogout(XmlDocument xmlDoc)
		{
			return xmlDoc.DocumentElement.LocalName.Equals("LogoutResponse");
		}

		internal static XmlDocument CanonicalizeXml(string xmlString)
		{
			//delete comments from the xml - security meassure
			logger.Trace("CanonicalizeXml");
			logger.Debug($"xmlString: {xmlString}");
			XmlDocument doc = new XmlDocument();
			doc.XmlResolver = null; //disable parser's DTD reading - security meassure
			doc.LoadXml(xmlString);
			XmlDsigExcC14NTransform c14nTransform = new XmlDsigExcC14NTransform();
			c14nTransform.LoadInput(doc);
			Stream outputStream = (Stream)c14nTransform.GetOutput();
			using (MemoryStream memoryStream = new MemoryStream())
			{
				outputStream.CopyTo(memoryStream);
				byte[] output = memoryStream.ToArray();
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.XmlResolver = null; //disable parser's DTD reading - security meassure
				xmlDoc.LoadXml(System.Text.Encoding.UTF8.GetString(output));
				logger.Debug($"CanonicalizeXml -- Cannonicalized xml: {xmlDoc.OuterXml}");
				return xmlDoc;
			}
		}

		internal static XElement CreateLoginRequest(string id, string destination, string acsUrl, string issuer, string policyFormat, string authContext, string spname, bool forceAuthn)
		{
			logger.Trace("CreateLoginRequest");
			string issueInstant = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"); //UTC

			XNamespace samlp = "urn:oasis:names:tc:SAML:2.0:protocol";
			XNamespace saml = "urn:oasis:names:tc:SAML:2.0:assertion";


			XElement authnRequest = new XElement(samlp + "AuthnRequest",
				new XAttribute("ID", id),
				new XAttribute("Version", "2.0"),
				new XAttribute("IssueInstant", issueInstant),
				new XAttribute("Destination", destination),
				new XAttribute("AssertionConsumerServiceURL", acsUrl),
				new XAttribute("ForceAuthn", forceAuthn),
				//new XAttribute("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"),

				new XElement(saml + "Issuer", issuer),


				new XElement(samlp + "NameIDPolicy",
					new XAttribute("Format", policyFormat.Trim()),
					new XAttribute("AllowCreate", "true"),
					new XAttribute("SPNameQualifier", spname)
					),

				new XElement(samlp + "RequestedAuthnContext",
					new XAttribute("Comparison", "exact"),
					new XElement(saml + "AuthnContextClassRef", authContext)
				)
			);
			logger.Debug($"CreateLoginRequest - XML request: {authnRequest.ToString()}");
			return authnRequest;
		}

		internal static string GetLoginInfo(XmlDocument xmlDoc)
		{
			logger.Trace("GetLoginInfo");
			List<Utils.Xml.Attribute> atributesList = new List<Utils.Xml.Attribute>();
			atributesList.Add(new Utils.Xml.Attribute(new List<string> { "SubjectConfirmationData", "saml2:SubjectConfirmationData" }, "InResponseTo"));
			atributesList.Add(new Utils.Xml.Attribute(new List<string> { "Conditions", "saml2:Conditions" }, "NotOnOrAfter"));
			atributesList.Add(new Utils.Xml.Attribute(new List<string> { "Conditions", "saml2:Conditions" }, "NotBefore"));
			atributesList.Add(new Utils.Xml.Attribute(new List<string> { "SubjectConfirmationData", "saml2:SubjectConfirmationData" }, "Recipient"));
			atributesList.Add(new Utils.Xml.Attribute(new List<string> { "AuthnStatement", "saml2:AuthnStatement" }, "SessionIndex"));
			atributesList.Add(new Utils.Xml.Attribute(new List<string> { "samlp:Response", "saml2p:Response" }, "Destination"));
			atributesList.Add(new Utils.Xml.Attribute(new List<string> { "StatusCode", "saml2p:StatusCode", "samlp:StatusCode" }, "Value"));



			List<Utils.Xml.Element> elementsList = new List<Utils.Xml.Element>();
			elementsList.Add(new Utils.Xml.Element(new List<string> { "Issuer", "saml2:Issuer" }));
			elementsList.Add(new Utils.Xml.Element(new List<string> { "Audience", "saml2:Audience" }));
			elementsList.Add(new Utils.Xml.Element(new List<string> { "NameID", "saml2:NameID" }));

			return PrintJson(xmlDoc, atributesList, elementsList);
		}

		internal static string GetLoginAttribute(XmlDocument xmlDoc, string name)
		{
			logger.Trace($"GetLoginAttribue -- attribute name: {name}");
			XmlNodeList nodeList = xmlDoc.GetElementsByTagName("Attribute");
			if (nodeList.Count == 0)
			{
				nodeList = xmlDoc.GetElementsByTagName("saml2:Attribute");
			}
			foreach (XmlNode node in nodeList)
			{
				if (node.Attributes.GetNamedItem("Name").Value.Equals(name))
				{
					logger.Debug($"GetLoginAttribue -- attribute name: {name}, value: {node.InnerText}");
					return node.InnerText;
				}
			}
			logger.Error($"GetLoginAttribue -- Could not find attribute with name {name}");
			return string.Empty;
		}

		internal static string GetRoles(XmlDocument xmlDoc, string name)
		{
			logger.Trace($"GetRoles -- name: {name}");
			XmlNodeList nodeList = xmlDoc.GetElementsByTagName("Attribute");
			List<string> roles = new List<string>();
			if (nodeList.Count == 0)
			{
				nodeList = xmlDoc.GetElementsByTagName("saml2:Attribute");
			}
			foreach (XmlNode node in nodeList)
			{
				if (node.Attributes.GetNamedItem("Name").Value.Equals(name))
				{
					XmlNodeList nlist = node.ChildNodes;
					foreach (XmlNode n in nlist)
					{
						roles.Add(n.InnerText);
					}
					return string.Join(",", roles);
				}
			}
			logger.Debug($"GetRoles -- Could not find attribute with name {name}");
			return string.Empty;


		}

		internal static string GetLogoutInfo(XmlDocument xmlDoc)
		{
			logger.Trace("GetLogoutInfo");
			List<Utils.Xml.Attribute> atributesList = new List<Utils.Xml.Attribute>();
			atributesList.Add(new Utils.Xml.Attribute(new List<string> { "LogoutResponse", "saml2p:LogoutResponse", "samlp:LogoutResponse" }, "Destination"));
			atributesList.Add(new Utils.Xml.Attribute(new List<string> { "LogoutResponse", "saml2p:LogoutResponse", "samlp:LogoutResponse" }, "InResponseTo"));
			atributesList.Add(new Utils.Xml.Attribute(new List<string> { "StatusCode", "samlp:StatusCode", "saml2p:StatusCode" }, "Value"));


			List<Element> elementsList = new List<Element>();
			elementsList.Add(new Utils.Xml.Element(new List<string> { "Issuer", "saml2:Issuer" }));


			return PrintJson(xmlDoc, atributesList, elementsList);
		}

		private static string PrintJson(XmlDocument xmlDoc, List<Utils.Xml.Attribute> atributes, List<Utils.Xml.Element> elements)
		{
			logger.Trace("PrintJson");
			string json = "{";
			foreach (Utils.Xml.Attribute at in atributes)
			{
				string value = at.PrintJson(xmlDoc);
				if (value != null)
				{
					json += $"{value},";
				}

			}

			int counter = 0;
			foreach (Utils.Xml.Element el in elements)
			{
				string value = el.PrintJson(xmlDoc);
				if (value != null)
				{
					if (counter != elements.Count - 1)
					{
						json += $"{value},";
					}
					else
					{
						json += $"{value} }}";
					}
				}
				counter++;
			}
			logger.Debug($"PrintJson -- json: {json}");
			return json;
		}
	}
}
