using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
public class GxXsltImpl
{
	public static string ApplyToString(string xml, string xslFileName)
	{
		XslTransform xsltE = new XslTransform();
		StringReader srXml = new StringReader(xml);
		XPathDocument xpdXml = new XPathDocument(srXml);
		XPathDocument xpdXslt = new XPathDocument(xslFileName);
		xsltE.Load(xpdXslt, null, System.Reflection.Assembly.GetCallingAssembly().Evidence);
		StringWriter result = new StringWriter();
		xsltE.Transform(xpdXml, null, result, null);
		return result.ToString();
	}
	public static string ApplyOld(string xslFileName, string fileFullName)
    {
        XslTransform xsltE = new XslTransform();
		XPathDocument xpdXml = new XPathDocument(fileFullName);
		XPathDocument xpdXslt = new XPathDocument(xslFileName);
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
                    transform.Load(xslFileName, new XsltSettings(true, true), new XmlUrlResolver());
                    transform.Transform(xmlReader, new XsltArgumentList(), textWriter);
                    s = textWriter.ToString();
                }
            }
        }
        return s;
    }
}