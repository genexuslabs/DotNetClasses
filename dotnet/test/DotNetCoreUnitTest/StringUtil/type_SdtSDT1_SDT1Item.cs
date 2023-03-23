using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using GeneXus.Application;
using GeneXus.Utils;


namespace GeneXus.Programs
{
	[XmlRoot(ElementName="SDT1Item")]
	[XmlType(TypeName="SDT1Item" , Namespace="DateBlank" )]
	[Serializable]
	public class SdtSDT1_SDT1Item : GxUserType
	{
		public SdtSDT1_SDT1Item( )
		{
			/* Constructor for serialization */
			gxTv_SdtSDT1_SDT1Item_Sdt1_name = "";

			gxTv_SdtSDT1_SDT1Item_Sdt1_datetime = (DateTime)(DateTime.MinValue);

		}

		public SdtSDT1_SDT1Item(IGxContext context)
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
			ToJSON(true) ;
			return;
		}

		public override void ToJSON(bool includeState)
		{
			AddObjectProperty("SDT1_No", gxTpr_Sdt1_no, false);


			AddObjectProperty("SDT1_Name", gxTpr_Sdt1_name, false);


			datetime_STZ = gxTpr_Sdt1_datetime;
			sDateCnv = "";
			sNumToPad = StringUtil.Trim(StringUtil.Str((decimal)(DateTimeUtil.Year(datetime_STZ)), 10, 0));
			sDateCnv = sDateCnv + StringUtil.Substring("0000", 1, 4-StringUtil.Len( sNumToPad)) + sNumToPad;
			sDateCnv = sDateCnv + "-";
			sNumToPad = StringUtil.Trim( StringUtil.Str((decimal)(DateTimeUtil.Month(datetime_STZ)), 10, 0));
			sDateCnv = sDateCnv + StringUtil.Substring("00", 1, 2-StringUtil.Len(sNumToPad)) + sNumToPad;
			sDateCnv = sDateCnv + "-";
			sNumToPad = StringUtil.Trim(StringUtil.Str((decimal)(DateTimeUtil.Day(datetime_STZ)), 10, 0));
			sDateCnv = sDateCnv + StringUtil.Substring("00", 1, 2-StringUtil.Len(sNumToPad)) + sNumToPad;
			sDateCnv = sDateCnv + "T";
			sNumToPad = StringUtil.Trim(StringUtil.Str((decimal)(DateTimeUtil.Hour(datetime_STZ)), 10, 0));
			sDateCnv = sDateCnv + StringUtil.Substring("00", 1, 2-StringUtil.Len(sNumToPad)) + sNumToPad;
			sDateCnv = sDateCnv + ":";
			sNumToPad = StringUtil.Trim(StringUtil.Str((decimal)(DateTimeUtil.Minute(datetime_STZ)), 10, 0));
			sDateCnv = sDateCnv + StringUtil.Substring("00", 1, 2-StringUtil.Len(sNumToPad)) + sNumToPad;
			sDateCnv = sDateCnv + ":";
			sNumToPad = StringUtil.Trim(StringUtil.Str((decimal)(DateTimeUtil.Second(datetime_STZ)), 10, 0));
			sDateCnv = sDateCnv + StringUtil.Substring("00", 1, 2-StringUtil.Len(sNumToPad)) + sNumToPad;
			AddObjectProperty("SDT1_DateTime", sDateCnv, false);


			return;
		}
		#endregion

		#region Properties

		[SoapElement(ElementName="SDT1_No")]
		[XmlElement(ElementName="SDT1_No")]
		public short gxTpr_Sdt1_no
		{
			get {
				return gxTv_SdtSDT1_SDT1Item_Sdt1_no; 
			}
			set {
				gxTv_SdtSDT1_SDT1Item_Sdt1_no = value;
				SetDirty("Sdt1_no");
			}
		}




		[SoapElement(ElementName="SDT1_Name")]
		[XmlElement(ElementName="SDT1_Name")]
		public string gxTpr_Sdt1_name
		{
			get {
				return gxTv_SdtSDT1_SDT1Item_Sdt1_name; 
			}
			set {
				gxTv_SdtSDT1_SDT1Item_Sdt1_name = value;
				SetDirty("Sdt1_name");
			}
		}



		[SoapElement(ElementName="SDT1_DateTime")]
		[XmlElement(ElementName="SDT1_DateTime" , IsNullable=true)]
		public string gxTpr_Sdt1_datetime_Nullable
		{
			get {
				if ( gxTv_SdtSDT1_SDT1Item_Sdt1_datetime == DateTime.MinValue)
					return null;
				return new GxDatetimeString(gxTv_SdtSDT1_SDT1Item_Sdt1_datetime).value ;
			}
			set {
				gxTv_SdtSDT1_SDT1Item_Sdt1_datetime = DateTimeUtil.CToD2(value);
			}
		}

		[XmlIgnore]
		public DateTime gxTpr_Sdt1_datetime
		{
			get {
				return gxTv_SdtSDT1_SDT1Item_Sdt1_datetime; 
			}
			set {
				gxTv_SdtSDT1_SDT1Item_Sdt1_datetime = value;
				SetDirty("Sdt1_datetime");
			}
		}


		public override bool ShouldSerializeSdtJson()
		{
			return true;
		}



		#endregion

		#region Initialization

		public void initialize( )
		{
			gxTv_SdtSDT1_SDT1Item_Sdt1_name = "";
			gxTv_SdtSDT1_SDT1Item_Sdt1_datetime = (DateTime)(DateTime.MinValue);
			datetime_STZ = (DateTime)(DateTime.MinValue);
			sDateCnv = "";
			sNumToPad = "";
			return  ;
		}



		#endregion

		#region Declaration

		protected string sDateCnv ;
		protected string sNumToPad ;
		protected DateTime datetime_STZ ;

		protected short gxTv_SdtSDT1_SDT1Item_Sdt1_no;
		 

		protected string gxTv_SdtSDT1_SDT1Item_Sdt1_name;
		 

		protected DateTime gxTv_SdtSDT1_SDT1Item_Sdt1_datetime;
		 


		#endregion
	}

}