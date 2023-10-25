using System;
using System.Collections;
using System.ServiceModel;
using System.Xml.Serialization;
using GeneXus.Application;
using GeneXus.Programs;
using GeneXus.Utils;
using Xunit;

namespace xUnitTesting
{
	public class JsonUtilTest
	{

		[Fact]
		public void SerializationWithBigDecimalsTest_Issue70446()
		{
			decimal expectedBigdecimal = 53903859.09090909M;
			if (GXJsonSerializer.DefaultJSonSerializer== GXJsonSerializerType.Jayrock)
			{
				expectedBigdecimal = 53903859.0909091M;
			}
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
