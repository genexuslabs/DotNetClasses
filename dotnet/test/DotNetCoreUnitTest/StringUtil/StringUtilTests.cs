using GeneXus.Programs;
using GeneXus.Utils;
using Newtonsoft.Json.Linq;
using Xunit;

namespace xUnitTesting
{
	public class StringUtilTests
	{
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
