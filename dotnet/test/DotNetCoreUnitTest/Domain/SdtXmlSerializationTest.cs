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
	}
}
