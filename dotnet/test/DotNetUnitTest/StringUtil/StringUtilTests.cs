using GeneXus.Application;
using GeneXus.Programs;
using GeneXus.Utils;
#if !NETCORE
using Jayrock.Json;
#endif
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
			JObject json = new JObject();
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
			JObject json = new JObject();
			json["SDT1_DateTime"] = "2014-04-29T14:29:40";
			json["SDT1_Name"] = string.Empty;
			json["SDT1_No"] = 0;

			sdt.FromJSONObject(json);
			Assert.Equal(14, sdt.gxTpr_Sdt1_datetime.Hour);
			Assert.Equal(29, sdt.gxTpr_Sdt1_datetime.Minute);
		}
		[Fact]
		public void TestZPictureCompatibiliy()
		{
			string picture = "$ 9.99";
			GxContext context = new GxContext();
			decimal decNumber = 5;
			string decStr = context.localUtil.Format(decNumber, picture);
			Assert.Equal("$ 5.00", decStr);
		}
		[Fact]
		public void TestZPictureWithEscapeChar()
		{
			string picture = "\\\\\" ZZ,ZZZ,ZZ9";
			GxContext context = new GxContext();
			decimal decNumber = 87654321;
			string decStr = context.localUtil.Format(decNumber, picture);
			Assert.Equal("\\\" 87,654,321", decStr);
		}
		[Fact]
		public void TestZPicture()
		{
			GxContext context = new GxContext();
			decimal decNumber = 123456.12M;
			string decStr = context.localUtil.Format(decNumber, "ZZZZZZZZZZ9.ZZZZZZ");
			Assert.Equal("     123456.120000", decStr);

			decStr = context.localUtil.Format(decNumber, "ZZZZZZZZZZ9.999999");
			Assert.Equal("     123456.120000", decStr);

			decStr = context.localUtil.Format(decNumber, "99999999999.999999");
			Assert.Equal("00000123456.120000", decStr);

			decStr = context.localUtil.Format(decNumber, "##########9.######");
			Assert.Equal("         123456.12", decStr);

			decStr = context.localUtil.Format(decNumber, "??????????9.??????");
			Assert.Equal("     123456.12    ", decStr);

			decStr = context.localUtil.Format(decNumber, "\\# ??????????9.??????");
			Assert.Equal("#      123456.12    ", decStr);


			decStr = context.localUtil.Format(decNumber, "##,###,###,##9.######");
			Assert.Equal("           123,456.12", decStr);

			decStr = context.localUtil.Format(decNumber, "??,???,???,??9.??????");
			Assert.Equal("        123456.12    ", decStr);

			//=====================Zero========================================

			decNumber = 0;

			decStr = context.localUtil.Format(decNumber, "ZZZZZZZZZZ9.ZZZZZZ");
			Assert.Equal("          0.000000", decStr);

			decStr = context.localUtil.Format(decNumber, "###########.######");
			Assert.Equal("                  ", decStr);

			decStr = context.localUtil.Format(decNumber, "???????????.??????");
			Assert.Equal("                  ", decStr);

			decStr = context.localUtil.Format(decNumber, "(??????????9.??????)");
			Assert.Equal("           0        ", decStr);

			decStr = context.localUtil.Format(decNumber, "\\# ??????????9.??????");
			Assert.Equal("#           0       ", decStr);

			decStr = context.localUtil.Format(decNumber, "(##########9.######)");
			Assert.Equal("                  0 ", decStr);

			//=====================One========================================

			decNumber = 1;

			decStr = context.localUtil.Format(decNumber, "ZZZZZZZZZZ9.ZZZZZZ");
			Assert.Equal("          1.000000", decStr);

			decStr = context.localUtil.Format(decNumber, "###########.######");
			Assert.Equal("                 1", decStr);

			decStr = context.localUtil.Format(decNumber, "???????????.??????");
			Assert.Equal("          1       ", decStr);

			decStr = context.localUtil.Format(decNumber, "(??????????9.??????)");
			Assert.Equal("           1        ", decStr);

			decStr = context.localUtil.Format(decNumber, "\\# ??????????9.??????");
			Assert.Equal("#           1       ", decStr);

			decStr = context.localUtil.Format(decNumber, "(##########9.######)");
			Assert.Equal("                  1 ", decStr);

			//=====================0.1========================================
			decNumber = 0.1M;
			decStr = context.localUtil.Format(decNumber, "???????????.??????");
			Assert.Equal("           .1     ", decStr);


			//=====================Negatives========================================
			decNumber = -123456.12M;
			decStr = context.localUtil.Format(decNumber, "(??????????9.??????)");
			Assert.Equal("(     123456.12    )", decStr);

			decStr = context.localUtil.Format(decNumber, "(##########9.######)");
			Assert.Equal("         (123456.12)", decStr);

			//=====================Positives========================================
			decNumber = 123456.12M;
			decStr = context.localUtil.Format(decNumber, "+ ??????????9.??????");
			Assert.Equal("+      123456.12    ", decStr);

			decStr = context.localUtil.Format(decNumber, "+ ##########9.######");
			Assert.Equal("         + 123456.12", decStr);

		}
	}
}
