using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Application;
using GeneXus.XML;
using Xunit;

namespace UnitTesting
{
	public class XMLWriterTest : FileSystemTest
	{
		[Fact]
		public void dfwpnumNullElementTest()
		{
			string content = dfwpnumTest(null, false);
			Assert.Contains("<varchar xmlns=\"StorageExpiration\" />", content, StringComparison.OrdinalIgnoreCase);
		}
		[Fact]
		public void dfwpnumEmptyElementWithoutEndTest()
		{
			string content = dfwpnumTest("validValue", false);
			Assert.Contains("<varchar xmlns=\"StorageExpiration\">validValue</varchar>", content, StringComparison.OrdinalIgnoreCase);
		}
		internal string dfwpnumTest(string varcharValue, bool closeElements)
		{
			GxContext context = new GxContext();
			string fileName = Path.Combine(BaseDir, "dfwpnumTest.txt");
			GXXMLWriter GXSoapXMLWriter = new GXXMLWriter(context.GetPhysicalPath());
			GXSoapXMLWriter.Open(fileName);
			GXSoapXMLWriter.WriteStartDocument("utf-8", 0);
			GXSoapXMLWriter.WriteStartElement("SOAP-ENV:Envelope");
			GXSoapXMLWriter.WriteElement("varchar", varcharValue);
			GXSoapXMLWriter.WriteAttribute("xmlns", "StorageExpiration");
			string sDateCnv = "0000-00-00";
			GXSoapXMLWriter.WriteElement("date", sDateCnv);
			GXSoapXMLWriter.WriteAttribute("xmlns", "StorageExpiration");
			if (closeElements)
			{
				GXSoapXMLWriter.WriteEndElement();
			}
			GXSoapXMLWriter.Close();
			return File.ReadAllText(fileName);
		}

	}
}
