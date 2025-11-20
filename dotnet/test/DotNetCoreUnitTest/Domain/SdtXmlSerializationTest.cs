using System.IO;
using GeneXus.Application;
using GeneXus.Programs;
using Xunit;

namespace DotNetCoreUnitTest.Domain
{
	public class SdtXmlSerializationTest
	{
		[Fact]
		public void EmptyValuesDeserializationTest()
		{
			GxContext context = new GxContext();
			SdtEmisor emisor = new SdtEmisor(context);
			string xml = "<Emisor xmlns=\"\">";
			xml += "  <RUCEmisor>212934610017</RUCEmisor>";
			xml += "  <RznSoc>ISL</RznSoc>";
			xml += "  <Departamento>MKT</Departamento>";
			xml += "</Emisor>";

			emisor.FromXml(xml, null, "", "");
			bool shouldSerializeField = emisor.ShouldSerializegxTpr_Giroemis();
			Assert.False(shouldSerializeField, "GiroEmis should not be serialized since it was not assigned during XML deserialization");

			shouldSerializeField = emisor.ShouldSerializegxTpr_Departamento();
			Assert.True(shouldSerializeField, "Departamento should be serialized since it was assigned during XML deserialization");

		}

		[Fact]
		public void GXCDataDeserializationTest()
		{
			GxContext context = new GxContext();
			SdtInvoicyretorno invoice = new SdtInvoicyretorno(context);
			string xml = File.ReadAllText("invoicy.xml");

			invoice.FromXml(xml, null, "Invoicyretorno", "InvoiCy");
			Assert.Contains("40047", invoice.ToJSonString(), System.StringComparison.OrdinalIgnoreCase);

		}
	}
}
