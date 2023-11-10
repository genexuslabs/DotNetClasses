using System;
using System.Collections;
using System.Xml.Serialization;
using GeneXus.Application;
using GeneXus.Utils;


namespace GeneXus.Programs
{
	[XmlRoot(ElementName="SDTGeneric")]
	[XmlType(TypeName="SDTGeneric" , Namespace="TestGeographyDatatype" )]
	[Serializable]
	public class SdtSDTGeneric : GxUserType
	{
		public SdtSDTGeneric( )
		{
			/* Constructor for serialization */
			gxTv_SdtSDTGeneric_Itemchar = "";

			gxTv_SdtSDTGeneric_Itemgeopoint = new Geospatial("");

			gxTv_SdtSDTGeneric_Itemgeography = new Geospatial("");

			gxTv_SdtSDTGeneric_Itemgeolocation = "";

		}

		public SdtSDTGeneric(IGxContext context)
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
			AddObjectProperty("ItemNum", gxTpr_Itemnum, false);


			AddObjectProperty("ItemChar", gxTpr_Itemchar, false);


			AddObjectProperty("ItemGeoPoint", gxTpr_Itemgeopoint, false);


			AddObjectProperty("ItemGeography", gxTpr_Itemgeography, false);


			AddObjectProperty("ItemGEolocation", gxTpr_Itemgeolocation, false);

			return;
		}
		#endregion

		#region Properties

		[SoapElement(ElementName="ItemNum")]
		[XmlElement(ElementName="ItemNum")]
		public short gxTpr_Itemnum
		{
			get {
				return gxTv_SdtSDTGeneric_Itemnum; 
			}
			set {
				gxTv_SdtSDTGeneric_Itemnum = value;
				SetDirty("Itemnum");
			}
		}




		[SoapElement(ElementName="ItemChar")]
		[XmlElement(ElementName="ItemChar")]
		public string gxTpr_Itemchar
		{
			get {
				return gxTv_SdtSDTGeneric_Itemchar; 
			}
			set {
				gxTv_SdtSDTGeneric_Itemchar = value;
				SetDirty("Itemchar");
			}
		}




		[SoapElement(ElementName="ItemGeoPoint")]
		[XmlElement(ElementName="ItemGeoPoint")]
		public Geospatial gxTpr_Itemgeopoint
		{
			get {
				return gxTv_SdtSDTGeneric_Itemgeopoint; 
			}
			set {
				gxTv_SdtSDTGeneric_Itemgeopoint = value;
				SetDirty("Itemgeopoint");
			}
		}




		[SoapElement(ElementName="ItemGeography")]
		[XmlElement(ElementName="ItemGeography")]
		public Geospatial gxTpr_Itemgeography
		{
			get {
				return gxTv_SdtSDTGeneric_Itemgeography; 
			}
			set {
				gxTv_SdtSDTGeneric_Itemgeography = value;
				SetDirty("Itemgeography");
			}
		}




		[SoapElement(ElementName="ItemGEolocation")]
		[XmlElement(ElementName="ItemGEolocation")]
		public string gxTpr_Itemgeolocation
		{
			get {
				return gxTv_SdtSDTGeneric_Itemgeolocation; 
			}
			set {
				gxTv_SdtSDTGeneric_Itemgeolocation = value;
				SetDirty("Itemgeolocation");
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
			gxTv_SdtSDTGeneric_Itemchar = "";
			gxTv_SdtSDTGeneric_Itemgeopoint = new Geospatial("");
			gxTv_SdtSDTGeneric_Itemgeography = new Geospatial("");
			gxTv_SdtSDTGeneric_Itemgeolocation = "";
			return  ;
		}



		#endregion

		#region Declaration

		protected short gxTv_SdtSDTGeneric_Itemnum;
		 

		protected string gxTv_SdtSDTGeneric_Itemchar;
		 

		protected Geospatial gxTv_SdtSDTGeneric_Itemgeopoint;
		 

		protected Geospatial gxTv_SdtSDTGeneric_Itemgeography;
		 

		protected string gxTv_SdtSDTGeneric_Itemgeolocation;
		 


		#endregion
	}
	
}