using GeneXus.Programs;
using GeneXus.Utils;
using Newtonsoft.Json.Linq;
using Xunit;

namespace xUnitTesting
{
	public class StringUtilTests
	{
		[Fact]
		public void TestJSONEncodeDoNotEncodeGreaterCharacter()
		{
			string json = "<kml>";
			json += "<Document>";
			json +="	<Style id=\"MyLine\">";
			json +="		<LineStyle>";
			json +="			<color>802080ff</color>";
			json +="			<width>6</width>";
			json +="		</LineStyle>";
			json +="	</Style>";
			json +="	<Placemark>";
			json +="		<LineString>";
			json +="			<coordinates>-88.076680,43.945580 -88.077480,43.945930 -88.078530,43.946390 -88.078960</coordinates>";
			json +="		</LineString>";
			json +="		<styleUrl>#MyLine</styleUrl>";
			json +="	</Placemark>";
			json +="</Document>";
			json +="</kml>";

			string expectedJsonEncoded = "<kml><Document>\\t<Style id=\\\"MyLine\\\">\\t\\t<LineStyle>\\t\\t\\t<color>802080ff</color>\\t\\t\\t<width>6</width>\\t\\t</LineStyle>\\t</Style>\\t<Placemark>\\t\\t<LineString>\\t\\t\\t<coordinates>-88.076680,43.945580 -88.077480,43.945930 -88.078530,43.946390 -88.078960</coordinates>\\t\\t</LineString>\\t\\t<styleUrl>#MyLine</styleUrl>\\t</Placemark></Document></kml>";
			string jsonEncoded = StringUtil.JSONEncode(json);
			Assert.Equal(jsonEncoded, expectedJsonEncoded);

		}
		[Fact]
		public void TestFromJsonSDTWithBlankDateTime()
		{
			SdtSDT1_SDT1Item sdt = new SdtSDT1_SDT1Item();
			Jayrock.Json.JObject json = new Jayrock.Json.JObject();
			json["SDT1_DateTime"] = "           00:00:00";
			json["SDT1_Name"]=string.Empty;
			json["SDT1_No"] = 0;

			sdt.FromJSONObject(json);
			Assert.Equal(sdt.gxTpr_Sdt1_datetime, DateTimeUtil.NullDate());
		}

		[Fact]
		public void TestFromJsonSDTWithTimeMustNotApplyTimezone()
		{
			SdtSDT1_SDT1Item sdt = new SdtSDT1_SDT1Item();
			Jayrock.Json.JObject json = new Jayrock.Json.JObject();
			json["SDT1_DateTime"] = "2014-04-29T14:29:40";
			json["SDT1_Name"] = string.Empty;
			json["SDT1_No"] = 0;

			sdt.FromJSONObject(json);
			Assert.Equal(14, sdt.gxTpr_Sdt1_datetime.Hour);
			Assert.Equal(29, sdt.gxTpr_Sdt1_datetime.Minute);
		}

		
	}
}
