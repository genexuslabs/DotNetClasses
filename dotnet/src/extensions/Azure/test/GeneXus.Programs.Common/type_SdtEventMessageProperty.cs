/*
				   File: type_SdtEventMessageProperty
			Description: EventMessageProperty
				 Author: Nemo 🐠 for C# (.NET) version 18.0.3.169293
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

using GeneXus.Programs;
namespace GeneXus.Programs.genexusserverlessapi
{
	[XmlRoot(ElementName="EventMessageProperty")]
	[XmlType(TypeName="EventMessageProperty" , Namespace="ServerlessAPI" )]
	[Serializable]
	public class SdtEventMessageProperty : GxUserType
	{
		public SdtEventMessageProperty( )
		{
			/* Constructor for serialization */
			gxTv_SdtEventMessageProperty_Propertyid = "";

			gxTv_SdtEventMessageProperty_Propertyvalue = "";

		}

		public SdtEventMessageProperty(IGxContext context)
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
			AddObjectProperty("PropertyId", gxTpr_Propertyid, false);


			AddObjectProperty("PropertyValue", gxTpr_Propertyvalue, false);

			return;
		}
		#endregion

		#region Properties

		[SoapElement(ElementName="PropertyId")]
		[XmlElement(ElementName="PropertyId")]
		public string gxTpr_Propertyid
		{
			get {
				return gxTv_SdtEventMessageProperty_Propertyid; 
			}
			set {
				gxTv_SdtEventMessageProperty_Propertyid = value;
				SetDirty("Propertyid");
			}
		}




		[SoapElement(ElementName="PropertyValue")]
		[XmlElement(ElementName="PropertyValue")]
		public string gxTpr_Propertyvalue
		{
			get {
				return gxTv_SdtEventMessageProperty_Propertyvalue; 
			}
			set {
				gxTv_SdtEventMessageProperty_Propertyvalue = value;
				SetDirty("Propertyvalue");
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
			gxTv_SdtEventMessageProperty_Propertyid = "";
			gxTv_SdtEventMessageProperty_Propertyvalue = "";
			return  ;
		}



		#endregion

		#region Declaration

		protected string gxTv_SdtEventMessageProperty_Propertyid;
		 

		protected string gxTv_SdtEventMessageProperty_Propertyvalue;
		 


		#endregion
	}
	#region Rest interface
	[GxUnWrappedJson()]
	[DataContract(Name=@"EventMessageProperty", Namespace="ServerlessAPI")]
	public class SdtEventMessageProperty_RESTInterface : GxGenericCollectionItem<SdtEventMessageProperty>, System.Web.SessionState.IRequiresSessionState
	{
		public SdtEventMessageProperty_RESTInterface( ) : base()
		{	
		}

		public SdtEventMessageProperty_RESTInterface( SdtEventMessageProperty psdt ) : base(psdt)
		{	
		}

		#region Rest Properties
		[DataMember(Name="PropertyId", Order=0)]
		public  string gxTpr_Propertyid
		{
			get { 
				return StringUtil.RTrim( sdt.gxTpr_Propertyid);

			}
			set { 
				 sdt.gxTpr_Propertyid = value;
			}
		}

		[DataMember(Name="PropertyValue", Order=1)]
		public  string gxTpr_Propertyvalue
		{
			get { 
				return StringUtil.RTrim( sdt.gxTpr_Propertyvalue);

			}
			set { 
				 sdt.gxTpr_Propertyvalue = value;
			}
		}


		#endregion

		public SdtEventMessageProperty sdt
		{
			get { 
				return (SdtEventMessageProperty)Sdt;
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
				sdt = new SdtEventMessageProperty() ;
			}
		}
	}
	#endregion
}