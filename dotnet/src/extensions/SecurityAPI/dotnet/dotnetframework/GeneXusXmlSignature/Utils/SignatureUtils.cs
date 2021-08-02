using System;
using System.Security;
using SecurityAPICommons.Commons;
using System.Xml;
using System.IO;
using System.Xml.Schema;
using SecurityAPICommons.Utils;

namespace GeneXusXmlSignature.GeneXusUtils
{
    [SecuritySafeCritical]
    internal class SignatureUtils

    {
        internal static XmlDocument documentFromFile(string path, string schemapath, Error error)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            try
            {
                xmlDoc.Load(path);
            }
            catch (Exception)
            {
                error.setError("SU001", "Unable to load file");
                return null;
            }
            if (schemapath != null && !SecurityUtils.compareStrings(schemapath, ""))
            {
                if (!validateExtensionSchema(schemapath))
                {
                    error.setError("SU002", "The schema file should be an xsd, dtd or xml file");
                    return null;
                }
                XmlSchemaSet schema = new XmlSchemaSet();
                XmlElement rootNode = (XmlElement)xmlDoc.DocumentElement;
                schema.Add(rootNode.NamespaceURI, schemapath);
                schema.ValidationEventHandler += new ValidationEventHandler(validationEventHandler);
                xmlDoc.Schemas = schema;

                try
                {
                    xmlDoc.Validate(validationEventHandler);
                }
                catch (Exception e)
                {
                    error.setError("SU003", e.Message);
                    return null;
                }
            }
            return xmlDoc;
        }

        internal static XmlDocument documentFromString(string xmlString, string schemapath, Error error)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            try
            {
                xmlDoc.LoadXml(xmlString);
            }
            catch (Exception)
            {
                error.setError("SU005", "Error reading XML");
                return null;
            }
            if (schemapath != null && !SecurityUtils.compareStrings(schemapath, ""))
            {
                if (!validateExtensionSchema(schemapath))
                {
                    error.setError("SU004", "The schema file should be an xsd, dtd or xml file");
                    return null;
                }
                XmlSchemaSet schema = new XmlSchemaSet();
                XmlElement rootNode = (XmlElement)xmlDoc.DocumentElement;
                schema.Add(rootNode.NamespaceURI, schemapath);
                schema.ValidationEventHandler += new ValidationEventHandler(validationEventHandler);
                xmlDoc.Schemas = schema;

                try
                {
                    xmlDoc.Validate(validationEventHandler);
                }
                catch (Exception e)
                {
                    error.setError("SU006", e.Message);
                    return null;
                }
            }

            return xmlDoc;

        }

        internal static string XMLDocumentToString(XmlDocument doc, Error error)
        {
            doc.PreserveWhitespace = true;
            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                doc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }

        public static bool writeToFile(string text, string path, string prefix, Error error)
        {
            try
            {

                using (StreamWriter writetext = new StreamWriter(path))
                {
                    writetext.WriteLine(prefix + text);
                }
            }
            catch (Exception)
            {
                error.setError("SU007", "Error writing file");
                return false;
            }
            return true;


        }


        internal static bool validateExtensionXML(string path)
        {
            if (SecurityUtils.extensionIs(path, ".xml"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static bool validateExtensionSchema(string path)
        {
            if (SecurityUtils.extensionIs(path, ".xsd") || SecurityUtils.extensionIs(path, ".xml") || SecurityUtils.extensionIs(path, ".dtd"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /* internal static XmlNode getNodeFromID(XmlDocument doc, string id, string xPath, Error error)
         {
             if (id == null || SecurityUtils.compareStrings(id, ""))
             {
                 error.setError("SU010", "Error, id data is empty");
                 return null;
             }
             string idToFind = xPath.Substring(1);
             XmlNode rootNode = doc.DocumentElement;
             XmlNodeList allNodes = rootNode.ChildNodes;
             foreach (XmlNode node in allNodes)
             {

                 XmlAttributeCollection attributes = node.Attributes;
                 if (attributes != null)
                 {
                     foreach (XmlAttribute attribute in node.Attributes)
                     {
                         if (SecurityUtils.compareStrings(attribute.Name, id) && SecurityUtils.compareStrings(attribute.Value, idToFind))
                         {
                             return node;
                         }
                     }
                 }


             }
             error.setError("SU009", "Could not found element attribute " + id + " with id: " + idToFind);
             return null;
         }*/

        internal static XmlNode getNodeFromID(XmlDocument doc, String id, String xPath, Error error)
        {
            if (id == null || SecurityUtils.compareStrings(id, ""))
            {
                error.setError("SU010", "Error, id data is empty");
                return null;
            }
            string idToFind = xPath.Substring(1);
            XmlNode root = doc.DocumentElement;
            XmlNodeList allNodes = root.ChildNodes;

            XmlNode n = RecursivegetNodeFromID(allNodes, id, idToFind);
            if (n == null)
            {
                error.setError("SU009", "Could not find element with id " + idToFind);
            }
            return n;

        }

        private static XmlNode FindAttribute(XmlNode node, String id, String idToFind)
        {
            XmlAttributeCollection attributes = node.Attributes;
            if (attributes != null)
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    if (SecurityUtils.compareStrings(attribute.Name, id) && SecurityUtils.compareStrings(attribute.Value, idToFind))
                    {
                        return node;
                    }
                }
            }
            return null;
        }

        private static XmlNode RecursivegetNodeFromID(XmlNodeList list, String id, String idToFind)
        {
            if (list.Count == 0)
            {
                return null;
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    XmlNode node = FindAttribute(list.Item(i), id, idToFind);
                    if (node == null)
                    {
                        XmlNode n1 = RecursivegetNodeFromID(list.Item(i).ChildNodes, id, idToFind);
                        if (n1 != null)
                        {
                            return n1;
                        }
                    }
                    else
                    {
                        return node;
                    }
                }
                return null;
            }
        }



        internal static string getIDNodeValue(XmlDocument doc)
        {
            doc.PreserveWhitespace = true;
            XmlNode rootNode = doc.DocumentElement;
            XmlNodeList allNodes = rootNode.ChildNodes;
            if (allNodes == null)
            {
                return "";
            }
            foreach (XmlNode node in allNodes)
            {
                //java xmlsec hack
                if (SecurityUtils.compareStrings("Reference", node.Name) || SecurityUtils.compareStrings("ds:Reference", node.Name))
                {
                    XmlAttributeCollection attributes = node.Attributes;
                    if (attributes == null)
                    {
                        return "";
                    }
                    foreach (XmlAttribute attribute in attributes)
                    {
                        if (SecurityUtils.compareStrings("URI", attribute.Name))
                        {
                            return attribute.Value;
                        }
                    }

                }
            }
            return "";
        }

        internal static XmlNode getNodeFromPath(XmlDocument doc, string expression, Error error)
        {
            doc.PreserveWhitespace = true;
            try
            {
                return doc.SelectSingleNode(expression);
            }
            catch (Exception)
            {
                error.setError("SU008", "Could not found any node that matches de xPath predicate");
                return null;
            }
        }



        private static void validationEventHandler(object sender, ValidationEventArgs e)
        {
            XmlSeverityType type = XmlSeverityType.Warning;
            if (Enum.TryParse<XmlSeverityType>("Error", out type))
            {
                if (type == XmlSeverityType.Error) throw new Exception(e.Message);
            }
        }


    }
}
