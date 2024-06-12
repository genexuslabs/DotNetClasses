/*
				   File: type_SdtCustomerSdtSimple
			Description: CustomerSdtSimple
				 Author: Nemo 🐠 for C# (.NET) version 18.0.10.183411
		   Program type: Callable routine
			  Main DBMS: 
*/
using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using GeneXus.Application;
using GeneXus.Utils;


namespace GeneXus.Programs.apps
{
	[XmlRoot(ElementName="CustomerSdtSimple")]
	[XmlType(TypeName="CustomerSdtSimple" , Namespace="TestRestProcs" )]
	[Serializable]
	public class SdtCustomerSdtSimple : GxUserType
	{
		public SdtCustomerSdtSimple( )
		{
			/* Constructor for serialization */
			gxTv_SdtCustomerSdtSimple_Customername = "";

			gxTv_SdtCustomerSdtSimple_Customerpaydate = (DateTime)(DateTime.MinValue);

		}

		public SdtCustomerSdtSimple(IGxContext context)
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
			AddObjectProperty("CustomerId", gxTpr_Customerid, false);


			AddObjectProperty("CustomerName", gxTpr_Customername, false);


			sDateCnv = "";
			sNumToPad = StringUtil.Trim(StringUtil.Str((decimal)(DateTimeUtil.Year(gxTpr_Customerbirthdate)), 10, 0));
			sDateCnv = sDateCnv + StringUtil.Substring("0000", 1, 4-StringUtil.Len(sNumToPad)) + sNumToPad;
			sDateCnv = sDateCnv + "-";
			sNumToPad = StringUtil.Trim(StringUtil.Str((decimal)(DateTimeUtil.Month(gxTpr_Customerbirthdate)), 10, 0));
			sDateCnv = sDateCnv + StringUtil.Substring("00", 1, 2-StringUtil.Len(sNumToPad)) + sNumToPad;
			sDateCnv = sDateCnv + "-";
			sNumToPad = StringUtil.Trim(StringUtil.Str((decimal)(DateTimeUtil.Day(gxTpr_Customerbirthdate)), 10, 0));
			sDateCnv = sDateCnv + StringUtil.Substring("00", 1, 2-StringUtil.Len(sNumToPad)) + sNumToPad;
			AddObjectProperty("CustomerBirthDate", sDateCnv, false);



			datetime_STZ = gxTpr_Customerpaydate;
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
			AddObjectProperty("CustomerPayDate", sDateCnv, false);


			return;
		}
		#endregion

		#region Properties

		[SoapElement(ElementName="CustomerId")]
		[XmlElement(ElementName="CustomerId")]
		public short gxTpr_Customerid
		{
			get {
				return gxTv_SdtCustomerSdtSimple_Customerid; 
			}
			set {
				gxTv_SdtCustomerSdtSimple_Customerid = value;
				SetDirty("Customerid");
			}
		}




		[SoapElement(ElementName="CustomerName")]
		[XmlElement(ElementName="CustomerName")]
		public string gxTpr_Customername
		{
			get {
				return gxTv_SdtCustomerSdtSimple_Customername; 
			}
			set {
				gxTv_SdtCustomerSdtSimple_Customername = value;
				SetDirty("Customername");
			}
		}



		[SoapElement(ElementName="CustomerBirthDate")]
		[XmlElement(ElementName="CustomerBirthDate" , IsNullable=true)]
		public string gxTpr_Customerbirthdate_Nullable
		{
			get {
				if ( gxTv_SdtCustomerSdtSimple_Customerbirthdate == DateTime.MinValue)
					return null;
				return new GxDateString(gxTv_SdtCustomerSdtSimple_Customerbirthdate).value ;
			}
			set {
				gxTv_SdtCustomerSdtSimple_Customerbirthdate = DateTimeUtil.CToD2(value);
			}
		}

		[XmlIgnore]
		public DateTime gxTpr_Customerbirthdate
		{
			get {
				return gxTv_SdtCustomerSdtSimple_Customerbirthdate; 
			}
			set {
				gxTv_SdtCustomerSdtSimple_Customerbirthdate = value;
				SetDirty("Customerbirthdate");
			}
		}


		[SoapElement(ElementName="CustomerPayDate")]
		[XmlElement(ElementName="CustomerPayDate" , IsNullable=true)]
		public string gxTpr_Customerpaydate_Nullable
		{
			get {
				if ( gxTv_SdtCustomerSdtSimple_Customerpaydate == DateTime.MinValue)
					return null;
				return new GxDatetimeString(gxTv_SdtCustomerSdtSimple_Customerpaydate).value ;
			}
			set {
				gxTv_SdtCustomerSdtSimple_Customerpaydate = DateTimeUtil.CToD2(value);
			}
		}

		[XmlIgnore]
		public DateTime gxTpr_Customerpaydate
		{
			get {
				return gxTv_SdtCustomerSdtSimple_Customerpaydate; 
			}
			set {
				gxTv_SdtCustomerSdtSimple_Customerpaydate = value;
				SetDirty("Customerpaydate");
			}
		}


		public override bool ShouldSerializeSdtJson()
		{
			return true;
		}



		#endregion

		#region Static Type Properties

		[XmlIgnore]
		private static GXTypeInfo _typeProps;
		protected override GXTypeInfo TypeInfo { get { return _typeProps; } set { _typeProps = value; } }

		#endregion

		#region Initialization

		public void initialize( )
		{
			gxTv_SdtCustomerSdtSimple_Customername = "";

			gxTv_SdtCustomerSdtSimple_Customerpaydate = (DateTime)(DateTime.MinValue);
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

		protected short gxTv_SdtCustomerSdtSimple_Customerid;
		 

		protected string gxTv_SdtCustomerSdtSimple_Customername;
		 

		protected DateTime gxTv_SdtCustomerSdtSimple_Customerbirthdate;
		 

		protected DateTime gxTv_SdtCustomerSdtSimple_Customerpaydate;
		 


		#endregion
	}
	#region Rest interface
	[GxJsonSerialization("default")]
	[DataContract(Name=@"CustomerSdtSimple", Namespace="TestRestProcs")]
	public class SdtCustomerSdtSimple_RESTInterface : GxGenericCollectionItem<SdtCustomerSdtSimple>, System.Web.SessionState.IRequiresSessionState
	{
		public SdtCustomerSdtSimple_RESTInterface( ) : base()
		{	
		}

		public SdtCustomerSdtSimple_RESTInterface( SdtCustomerSdtSimple psdt ) : base(psdt)
		{	
		}

		#region Rest Properties
		[DataMember(Name="CustomerId", Order=0)]
		public short gxTpr_Customerid
		{
			get { 
				return sdt.gxTpr_Customerid;

			}
			set { 
				sdt.gxTpr_Customerid = value;
			}
		}

		[DataMember(Name="CustomerName", Order=1)]
		public  string gxTpr_Customername
		{
			get { 
				return StringUtil.RTrim( sdt.gxTpr_Customername);

			}
			set { 
				 sdt.gxTpr_Customername = value;
			}
		}

		[DataMember(Name="CustomerBirthDate", Order=2)]
		public  string gxTpr_Customerbirthdate
		{
			get { 
				return DateTimeUtil.DToC2( sdt.gxTpr_Customerbirthdate);

			}
			set { 
				sdt.gxTpr_Customerbirthdate = DateTimeUtil.CToD2(value);
			}
		}

		[DataMember(Name="CustomerPayDate", Order=3)]
		public  string gxTpr_Customerpaydate
		{
			get { 
				return DateTimeUtil.TToC2( sdt.gxTpr_Customerpaydate,context);

			}
			set { 
				sdt.gxTpr_Customerpaydate = DateTimeUtil.CToT2(value,context);
			}
		}


		#endregion

		public SdtCustomerSdtSimple sdt
		{
			get { 
				return (SdtCustomerSdtSimple)Sdt;
			}
			set { 
				Sdt = value;
			}
		}

		[OnDeserializing]
		void checkSdt( StreamingContext ctx )
		{
			if ( sdt == null )
			{
				sdt = new SdtCustomerSdtSimple() ;
			}
		}
	}
	#endregion
}