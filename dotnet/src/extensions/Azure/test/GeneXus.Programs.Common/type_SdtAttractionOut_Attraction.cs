/*
				   File: type_SdtAttractionOut_Attraction
			Description: AttractionOut
				 Author: Nemo 🐠 for C# (.NET) version 17.0.10.160814
		   Program type: Callable routine
			  Main DBMS: 
*/
using System;
using System.Collections;
using GeneXus.Utils;
using GeneXus.Resources;
using GeneXus.Application;
using GeneXus.Metadata;
using GeneXus.Cryptography;
using GeneXus.Encryption;
using GeneXus.Http.Client;
using GeneXus.Http.Server;
using System.Reflection;
using System.Xml.Serialization;
using System.Runtime.Serialization;


namespace GeneXus.Programs
{
	[XmlRoot(ElementName="Attraction")]
	[XmlType(TypeName="Attraction" , Namespace="TestServerlessGAM" )]
	[Serializable]
	public class SdtAttractionOut_Attraction : GxUserType
	{
		public SdtAttractionOut_Attraction( )
		{
			/* Constructor for serialization */
			gxTv_SdtAttractionOut_Attraction_Attractionname = "";

		}

		public SdtAttractionOut_Attraction(IGxContext context)
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
			AddObjectProperty("AttractionId", gxTpr_Attractionid, false);


			AddObjectProperty("AttractionName", gxTpr_Attractionname, false);

			return;
		}
		#endregion

		#region Properties

		[SoapElement(ElementName="AttractionId")]
		[XmlElement(ElementName="AttractionId")]
		public short gxTpr_Attractionid
		{
			get {
				return gxTv_SdtAttractionOut_Attraction_Attractionid; 
			}
			set {
				gxTv_SdtAttractionOut_Attraction_Attractionid = value;
				SetDirty("Attractionid");
			}
		}




		[SoapElement(ElementName="AttractionName")]
		[XmlElement(ElementName="AttractionName")]
		public string gxTpr_Attractionname
		{
			get {
				return gxTv_SdtAttractionOut_Attraction_Attractionname; 
			}
			set {
				gxTv_SdtAttractionOut_Attraction_Attractionname = value;
				SetDirty("Attractionname");
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
			gxTv_SdtAttractionOut_Attraction_Attractionname = "";
			return  ;
		}



		#endregion

		#region Declaration

		protected short gxTv_SdtAttractionOut_Attraction_Attractionid;
		 

		protected string gxTv_SdtAttractionOut_Attraction_Attractionname;
		 


		#endregion
	}
	#region Rest interface
	[DataContract(Name=@"Attraction", Namespace="TestServerlessGAM")]
	public class SdtAttractionOut_Attraction_RESTInterface : GxGenericCollectionItem<SdtAttractionOut_Attraction>, System.Web.SessionState.IRequiresSessionState
	{
		public SdtAttractionOut_Attraction_RESTInterface( ) : base()
		{	
		}

		public SdtAttractionOut_Attraction_RESTInterface( SdtAttractionOut_Attraction psdt ) : base(psdt)
		{	
		}

		#region Rest Properties
		[DataMember(Name="AttractionId", Order=0)]
		public short gxTpr_Attractionid
		{
			get { 
				return sdt.gxTpr_Attractionid;

			}
			set { 
				sdt.gxTpr_Attractionid = value;
			}
		}

		[DataMember(Name="AttractionName", Order=1)]
		public  string gxTpr_Attractionname
		{
			get { 
				return StringUtil.RTrim( sdt.gxTpr_Attractionname);

			}
			set { 
				 sdt.gxTpr_Attractionname = value;
			}
		}


		#endregion

		public SdtAttractionOut_Attraction sdt
		{
			get { 
				return (SdtAttractionOut_Attraction)Sdt;
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
				sdt = new SdtAttractionOut_Attraction() ;
			}
		}
	}
	#endregion
}