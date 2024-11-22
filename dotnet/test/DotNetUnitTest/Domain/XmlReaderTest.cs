using System;
using System.IO;
using System.Xml;
using GeneXus.XML;
using Xunit;

namespace xUnitTesting
{
	public class XmlReaderTest
	{
		[Fact]
		public void TestExternalEntitiesEnabled()
		{
			TestExternalEntities(1);
		}
		[Fact]
		public void TestExternalEntitiesDisabled()
		{
			TestExternalEntities(0);
		}
		void TestExternalEntities(int externalEntities)
		{
			string xml;
			string value;
			GXXMLReader xmlReader;

			using (xmlReader = new GXXMLReader(Directory.GetCurrentDirectory()))
			{
				xmlReader.ReadExternalEntities = externalEntities;
				xml = "";
				xml += "<!DOCTYPE Envelope [";
				xml += "<!ELEMENT Envelope ANY >";
				xml += "<!ENTITY xxe \"Hello\">";
				xml += "<!ENTITY xxe2 \"&xxe;&xxe;&xxe;&xxe;\">";
				xml += "] >";
				xml += "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xxe=\"issue63212\">";
				xml += "<soapenv:Header/>";
				xml += "<soapenv:Body>";
				xml += "<xxe:helloworld.Execute>";
				xml += "<xxe:Name>&xxe2;</xxe:Name>";
				xml += "</xxe:helloworld.Execute>";
				xml += "</soapenv:Body>";
				xml += "</soapenv:Envelope>";
				xmlReader.OpenFromString(xml);
				Assert.Equal(0, xmlReader.ErrCode);
				Assert.Equal(string.Empty, xmlReader.ErrDescription);
				if (!xmlReader.EOF)
				{
					xmlReader.Read();
					Assert.Equal(0, xmlReader.ErrCode);
					Assert.Equal(string.Empty, xmlReader.ErrDescription);
					value = xmlReader.Value;
					if (externalEntities==0)
						Assert.Equal(string.Empty, value);
					else
						Assert.Equal("Envelope", value);
				}
				xmlReader.Close();
			}

		}
		[Fact]
		public void TestValidationType()
		{
			string value;
			GXXMLReader xmlReader;

			using (xmlReader = new GXXMLReader(Directory.GetCurrentDirectory()))
			{
				xmlReader.ValidationType = GXXMLReader.ValidationSchema;
				xmlReader.AddSchema("./resources/QueryViewerObjects.xsd", "qv");
				xmlReader.Open("./resources/QueryViewerObjects.xml");
				Assert.Equal(string.Empty, xmlReader.ErrDescription);
				Assert.Equal(0, xmlReader.ErrCode);
				if (!xmlReader.EOF)
				{
					xmlReader.Read();
					Assert.Equal(0, xmlReader.ErrCode);
					Assert.Equal(string.Empty, xmlReader.ErrDescription);
					value = xmlReader.Name;
					Assert.Equal("Objects", value);
				}
				xmlReader.Close();
			}

		}
	}
}
