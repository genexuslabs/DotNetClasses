using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

public class Xslt
{

    public static string ApplyOld(string xslFileName, string fileFullName)
    {
        XslTransform xsltE = new XslTransform();
#pragma warning disable CA5372 // Use XmlReader For XPathDocument
#pragma warning disable CA3075 // Insecure DTD processing in XML
		XPathDocument xpdXml = new XPathDocument(fileFullName);
#pragma warning restore CA3075 // Insecure DTD processing in XML
#pragma warning restore CA5372 // Use XmlReader For XPathDocument
#pragma warning disable CA3075 // Insecure DTD processing in XML
#pragma warning disable CA5372 // Use XmlReader For XPathDocument
		XPathDocument xpdXslt = new XPathDocument(xslFileName);
#pragma warning restore CA5372 // Use XmlReader For XPathDocument
#pragma warning restore CA3075 // Insecure DTD processing in XML
        xsltE.Load(xpdXslt, new XmlUrlResolver(), System.Reflection.Assembly.GetCallingAssembly().Evidence);
        StringWriter result = new StringWriter();
        xsltE.Transform(xpdXml, null, result, null);
        return result.ToString();
    }
    public static string Apply(string xslFileName, string fileFullName)
    {

        string s;
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.CheckCharacters = false;
        using (StreamReader streamReader = new StreamReader(fileFullName))
        {
            using (XmlReader xmlReader = XmlReader.Create(streamReader, readerSettings))
            {
                using (StringWriter textWriter = new StringWriter())
                {
                    var transform = new XslCompiledTransform();
#pragma warning disable CA3076 // Insecure XSLT script processing.
                    transform.Load(xslFileName, new XsltSettings(true, true), new XmlUrlResolver());
#pragma warning restore CA3076 // Insecure XSLT script processing.
                    transform.Transform(xmlReader, new XsltArgumentList(), textWriter);
                    s = textWriter.ToString();
                }
            }
        }
        return s;
    }
}