using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Utils;
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
			File.WriteAllText(@"C:\temp\json.txt", jsonEncoded);
			Assert.Equal(jsonEncoded, expectedJsonEncoded);

		}
	}
}
