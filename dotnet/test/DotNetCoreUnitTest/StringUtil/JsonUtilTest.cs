using System;
using System.Collections;
using System.Xml.Serialization;
using GeneXus.Application;
using GeneXus.Utils;
using System.ServiceModel;
using GeneXus.Programs;
#if !NETCORE
using Jayrock.Json;
#endif
using Xunit;

namespace xUnitTesting
{
	public class JsonUtilTest
	{
		[Fact]
		public void PropertiesSerialization()
		{
			GXProperties AV9properties = new GXProperties();
			string url = "http://localhost/OuvidoriaOuvidoriaAndroidHomologa/login.aspx?pmsa";
			string user = "mpsa";
			string AV8json = "{\"Url\" : \""+ url + "\",\"User\" : \"" + user + "\"}";
			AV9properties.FromJSonString(AV8json, null);
			GxKeyValuePair AV10property = AV9properties.GetFirst();
			Assert.Equal("Url", AV10property.Key);
			Assert.Equal(url, AV10property.Value);
			AV10property = AV9properties.GetNext();
			Assert.Equal("User", AV10property.Key);
			Assert.Equal(user, AV10property.Value);

		}
		[Fact]
		public void StringSerialization()
		{
			string result = JSONHelper.WriteJSON<dynamic>("myvalue");
			Assert.Equal("myvalue", result);

		}
		[Fact]
		public void SerializationFloatingNumbers()
		{
			SdtSDTGeneric genericSdt = new SdtSDTGeneric();
			genericSdt.gxTpr_Itemnum = 1;
			genericSdt.gxTpr_Itemchar = "Caso Base";
			genericSdt.gxTpr_Itemgeopoint = new Geospatial("POINT(-56.248367 -34.873821)");
			genericSdt.gxTpr_Itemgeography = new Geospatial("-34.873821, -56.248367");
			string json = genericSdt.ToJSonString();
			string expectedJson = "{\"ItemNum\":1,\"ItemChar\":\"Caso Base\",\"ItemGeoPoint\":\"POINT (-56.248367 -34.873821)\",\"ItemGeography\":\"\",\"ItemGEolocation\":\"\"}";
			Assert.Equal(expectedJson, json);
		}
		[Fact]
		public void DeserializationInvalidJsonNoDuplicateError()
		{
			string invalidJson1 = "{\"id\":1,\"name\":\"uno\",\"date\":\"2016-02-24\"'";
			GXBaseCollection<SdtMessages_Message> messages = new GXBaseCollection<SdtMessages_Message>();
			JSONHelper.ReadJSON<JObject>(invalidJson1, messages);
			string errMessage = messages.ToJSonString();
#if NETCORE
			string expectedError = "[{\"Id\":\"FromJson Error\",\"Type\":1,\"Description\":\"''' is invalid after a value. Expected either ',', '}', or ']'. Path: $.date | LineNumber: 0 | BytePositionInLine: 40.\"}]";
#else
			string expectedError = "[{\"Id\":\"FromJson Error\",\"Type\":1,\"Description\":\"Expected a ',' or '}'.\"}]";
#endif
			Assert.Equal(expectedError, errMessage);
		}

		[Fact]
		public void SerializationWithDateTimes()
		{
			object[] parms = new object[]
			{
				new DateTime(2016, 6, 29, 0, 0, 0),
				new DateTime(2016, 6, 29, 12, 12, 0),
				true
			};
			JArray jarray = new JArray(parms);
			string json = jarray.ToString();
			Assert.Equal("[\"06/29/2016 00:00:00\",\"06/29/2016 12:12:00\",true]", json);

			JObject jObject = new JObject();
			jObject.Put("datetime", new DateTime(2016, 6, 29, 12, 12, 0));
			json = jObject.ToString();
			Assert.Equal("{\"datetime\":\"06/29/2016 12:12:00\"}", json);
		}
		[Fact]
		public void DeserializationWithDateTimes()
		{
			string json = "{\"parms\": [\"2016/06/29 00:00:00\",    \"2016/06/29 12:12:00\", true]}";
			JObject jObj = JSONHelper.ReadJSON<JObject>(json);
			string parm = (string)((JArray)jObj["parms"])[1];
			GxContext gxContext = new GxContext();
			DateTime dParm = gxContext.localUtil.CToT(parm, 0, 0);
			Assert.Equal(new DateTime(2016, 6, 29, 12, 12, 0), dParm);
		}
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
		public void DeserializationWithBigDecimalsTest_Issue70446()
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
		public void SerializationWithBigDecimalsTest_Issue70446()
		{
			decimal expectedBigdecimal = 53903859.09090909M;
			GxContext context = new GxContext();
			SdtSDTteste sdt = new SdtSDTteste(context);
			sdt.gxTpr_Testeid = 1;
			sdt.gxTpr_Testename = "um";
			sdt.gxTpr_Testenumero = expectedBigdecimal;

			string expectedJson = "{\"testeID\":1,\"testeName\":\"um\",\"testeNumero\":\"53903859.09090909\"}";
			string json = sdt.ToJSonString();
			Assert.Equal(expectedJson, json);
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
		[Fact]
		public void DeserializationTrailingCommasCompatibility()
		{
			string json = "[[\"Client\",\"5a4ff115ab7e9d0f6c290b4ef33e34ce\"],[\"Invoice\",\"6c656e4034128018024144afdcc756aa\"],[\"InvoiceLine\",\"cd07f8e2bc014e27340005d9f26ec626\"],[\"Product\",\"84da0573bd448e9761ce20a4c4f9fce1\"],[\"ClientAutonumber\",\"479484926ef142cc026b362e2579a9f3\"],[\"Orders\",\"d857b7ff7dbf9329047b52918f7c5cac\"],[\"TiposDeDatos\",\"0c9a48d30dc26bd787ce466f235a5b3f\"],]";
			JArray array = JSONHelper.ReadJSON<JArray>(json);
			Assert.Equal(7, array.Count);
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
