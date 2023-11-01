using System;
using System.Collections;
using System.Xml.Serialization;
using GeneXus.Application;
using GeneXus.Utils;
using System.ServiceModel;
#if !NETCORE
using Jayrock.Json;
#endif
using Xunit;

namespace xUnitTesting
{
	public class JsonUtilTest
	{
		[Fact]
		public void SerializationWithNumbers()
		{
			JObject jObject = new JObject();
			jObject.Put("gridId", 2);
			string json = JSONHelper.WriteJSON<JObject>(jObject);
			Assert.Contains(":2", json, StringComparison.OrdinalIgnoreCase);
		}
		[Fact]
		public void DeserializationWithNumbers()
		{
			string json = "{\"MPage\":false,\"cmpCtx\":\"\",\"parms\":[0,\"\",\"\",\"\",[{\"pRow\":\"\",\"c\":[],\"v\":null}],\"\",\"0\",{\"gridRC\":53903859.090,\"hsh\":true,\"grid\":2},\"0\",null],\"hsh\":[],\"objClass\":\"testsac42351\",\"pkgName\":\"GeneXus.Programs\",\"events\":[\"'BTN_SEARCH'\"],\"grids\":{\"Grid1\":{\"id\":2,\"lastRow\":0,\"pRow\":\"\"}}}";
			JObject jObj = JSONHelper.ReadJSON<JObject>(json);
			JObject parm = (JObject)((JArray)jObj["parms"])[7];
			Assert.NotNull(parm);
			int gridId = (int)(parm["grid"]);
			Assert.Equal(2, gridId);
			decimal gridRC = Convert.ToDecimal(parm["gridRC"]);
			Assert.Equal(53903859.090M, gridRC);
			bool hash = (bool)parm["hsh"];
			Assert.True(hash);
		}

		[Fact]
		public void SerializationWithBigDecimalsTest_Issue70446()
		{
			decimal expectedBigdecimal = 53903859.09090909M;
			GxContext context = new GxContext();
			SdtSDTteste sdt = new SdtSDTteste(context);
			string json = "{\"testeID\":1,\"testeName\":\"um\",\"testeNumero\":53903859.09090909}";
			sdt.FromJSonString(json, null);
			Assert.Equal(expectedBigdecimal, sdt.gxTpr_Testenumero);
			Assert.Equal(1, sdt.gxTpr_Testeid);
			Assert.Equal("um", sdt.gxTpr_Testename);
		}
		[Fact]
		public void SerializationWithSpecialCharacters_Issue69271()
		{
			string specialCharacters =  $"1:{StringUtil.Chr(30)}:1-2:{StringUtil.Chr(29)}:2";
			GxContext context = new GxContext();
			SdtSDTteste sdt = new SdtSDTteste(context);
			string json = "{\"testeID\":1,\"testeName\":\"" + StringUtil.JSONEncode(specialCharacters) + "\"}";
			sdt.FromJSonString(json, null);
			Assert.Equal(specialCharacters, sdt.gxTpr_Testename);
		}

		[Fact]
		public void SerializationWithSpecialCharacters_js()
		{
			JObject jObject = new JObject();
			jObject.Put("Grid1ContainerData", "{\"GridName\":\"Links\",\"CmpContext\":\"MPW0020\",\"Caption\":\"One+Two<>&\"}");
			string json = JSONHelper.WriteJSON<JObject>(jObject);
			Assert.Contains("\\\"GridName\\\"", json, StringComparison.OrdinalIgnoreCase);
		}
		[Fact]
		public void DeserializationWithSpecialCharacters_js()
		{
			string json="{\"GridName\":\"Links\",\"CmpContext\":\"MPW0020\",\"Caption\":\"One+Two<>&\"}";
			JObject jObject = JSONHelper.ReadJSON<JObject>(json);
			string caption = (string)jObject["Caption"];
			Assert.Equal("One+Two<>&", caption);
		}

		[Fact]
		public void SerializationPropertiesKeepOrder()
		{
			string json = JSONHelper.WriteJSON<dynamic>(GetJSONObject());
			Assert.Equal("{\"ClientId\":0,\"ClientName\":\"John\",\"ClientActive\":false}", json);
		}
		object GetJSONObject()
		{
			JObject jObject = new JObject();
			jObject.Put("ClientId", 0);
			jObject.Put("ClientName", "John");
			jObject.Put("ClientActive", false);
			return jObject;
		}
	}

	[XmlSerializerFormat]
	[XmlRoot(ElementName = "SDTteste")]
	[XmlType(TypeName = "SDTteste", Namespace = "JsonSerialization")]
	[Serializable]
	public class SdtSDTteste : GxUserType
	{
		public SdtSDTteste()
		{
			/* Constructor for serialization */
			gxTv_SdtSDTteste_Testename = "";

		}
		public SdtSDTteste(IGxContext context)
		{
			this.context = context;
			initialize();
		}
		#region Json
		private static Hashtable mapper;
		public override string JsonMap(string value)
		{
			if (mapper == null)
			{
				mapper = new Hashtable();
			}
			return (string)mapper[value]; ;
		}
		public override void ToJSON()
		{
			ToJSON(true);
			return;
		}
		public override void ToJSON(bool includeState)
		{
			AddObjectProperty("testeID", gxTpr_Testeid, false);
			AddObjectProperty("testeName", gxTpr_Testename, false);
			AddObjectProperty("testeNumero", StringUtil.LTrim(StringUtil.Str(gxTpr_Testenumero, 21, 8)), false);
			return;
		}
		#endregion

		#region Properties

		[SoapElement(ElementName = "testeID")]
		[XmlElement(ElementName = "testeID")]
		public short gxTpr_Testeid
		{
			get
			{
				return gxTv_SdtSDTteste_Testeid;
			}
			set
			{
				gxTv_SdtSDTteste_Testeid = value;
				SetDirty("Testeid");
			}
		}

		[SoapElement(ElementName = "testeName")]
		[XmlElement(ElementName = "testeName")]
		public string gxTpr_Testename
		{
			get
			{
				return gxTv_SdtSDTteste_Testename;
			}
			set
			{
				gxTv_SdtSDTteste_Testename = value;
				SetDirty("Testename");
			}
		}
		[SoapElement(ElementName = "testeNumero")]
		[XmlElement(ElementName = "testeNumero")]
		public string gxTpr_Testenumero_double
		{
			get
			{
				return Convert.ToString(gxTv_SdtSDTteste_Testenumero, System.Globalization.CultureInfo.InvariantCulture);
			}
			set
			{
				gxTv_SdtSDTteste_Testenumero = Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture);
			}
		}
		[SoapIgnore]
		[XmlIgnore]
		public decimal gxTpr_Testenumero
		{
			get
			{
				return gxTv_SdtSDTteste_Testenumero;
			}
			set
			{
				gxTv_SdtSDTteste_Testenumero = value;
				SetDirty("Testenumero");
			}
		}
		public override bool ShouldSerializeSdtJson()
		{
			return true;
		}

		#endregion

		#region Initialization

		public void initialize()
		{
			gxTv_SdtSDTteste_Testename = "";

			return;
		}

		#endregion

		#region Declaration

		protected short gxTv_SdtSDTteste_Testeid;
		protected string gxTv_SdtSDTteste_Testename;
		protected decimal gxTv_SdtSDTteste_Testenumero;
		#endregion
	}
}
