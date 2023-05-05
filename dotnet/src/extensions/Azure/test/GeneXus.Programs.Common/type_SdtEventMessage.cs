/*
				   File: type_SdtEventMessage
			Description: EventMessage
				 Author: Nemo 🐠 for C# (.NET Core) version 17.0.4.150138
		   Program type: Callable routine
			  Main DBMS: 
*/
using System;
using System.Collections;
using GeneXus.Utils;
using GeneXus.Application;
using System.Xml.Serialization;
using System.Runtime.Serialization;
namespace GeneXus.Programs.genexusserverlessapi
{
	[XmlRoot(ElementName="EventMessage")]
	[XmlType(TypeName="EventMessage" , Namespace="ServerlessAPI" )]
	[Serializable]
	public class SdtEventMessage : GxUserType
	{
		public SdtEventMessage( )
		{
			/* Constructor for serialization */
			gxTv_SdtEventMessage_Eventmessageid = "";

			gxTv_SdtEventMessage_Eventmessagedate = (DateTime)(DateTime.MinValue);

			gxTv_SdtEventMessage_Eventmessagesourcetype = "";

			gxTv_SdtEventMessage_Eventmessagedata = "";

			gxTv_SdtEventMessage_Eventmessageversion = "";

		}

		public SdtEventMessage(IGxContext context)
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
			AddObjectProperty("EventMessageId", gxTpr_Eventmessageid, false);


			datetime_STZ = gxTpr_Eventmessagedate;
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
			AddObjectProperty("EventMessageDate", sDateCnv, false);


			AddObjectProperty("EventMessageSourceType", gxTpr_Eventmessagesourcetype, false);


			AddObjectProperty("EventMessageData", gxTpr_Eventmessagedata, false);


			if (gxTv_SdtEventMessage_Eventmessageproperties != null)
			{
				AddObjectProperty("EventMessageProperties", gxTv_SdtEventMessage_Eventmessageproperties, false);
			}
			
			
			
			AddObjectProperty("EventMessageVersion", gxTpr_Eventmessageversion, false);

			
			return;
		}
		#endregion

		#region Properties

		[SoapElement(ElementName="EventMessageId")]
		[XmlElement(ElementName="EventMessageId")]
		public string gxTpr_Eventmessageid
		{
			get { 
				return gxTv_SdtEventMessage_Eventmessageid; 
			}
			set { 
				gxTv_SdtEventMessage_Eventmessageid = value;
				SetDirty("Eventmessageid");
			}
		}



		[SoapElement(ElementName="EventMessageDate")]
		[XmlElement(ElementName="EventMessageDate" , IsNullable=true)]
		public string gxTpr_Eventmessagedate_Nullable
		{
			get {
				if ( gxTv_SdtEventMessage_Eventmessagedate == DateTime.MinValue)
					return null;
				return new GxDatetimeString(gxTv_SdtEventMessage_Eventmessagedate).value ;
			}
			set {
				gxTv_SdtEventMessage_Eventmessagedate = DateTimeUtil.CToD2(value);
			}
		}

		[XmlIgnore]
		public DateTime gxTpr_Eventmessagedate
		{
			get { 
				return gxTv_SdtEventMessage_Eventmessagedate; 
			}
			set { 
				gxTv_SdtEventMessage_Eventmessagedate = value;
				SetDirty("Eventmessagedate");
			}
		}



		[SoapElement(ElementName="EventMessageSourceType")]
		[XmlElement(ElementName="EventMessageSourceType")]
		public string gxTpr_Eventmessagesourcetype
		{
			get { 
				return gxTv_SdtEventMessage_Eventmessagesourcetype; 
			}
			set { 
				gxTv_SdtEventMessage_Eventmessagesourcetype = value;
				SetDirty("Eventmessagesourcetype");
			}
		}




		[SoapElement(ElementName="EventMessageData")]
		[XmlElement(ElementName="EventMessageData")]
		public string gxTpr_Eventmessagedata
		{
			get { 
				return gxTv_SdtEventMessage_Eventmessagedata; 
			}
			set { 
				gxTv_SdtEventMessage_Eventmessagedata = value;
				SetDirty("Eventmessagedata");
			}
		}


	[SoapElement(ElementName="EventMessageProperties" )]
		[XmlArray(ElementName="EventMessageProperties"  )]
		[XmlArrayItemAttribute(ElementName="EventMessageProperty" , IsNullable=false )]
		public GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventMessageProperty> gxTpr_Eventmessageproperties_GXBaseCollection
		{
			get {
				if ( gxTv_SdtEventMessage_Eventmessageproperties == null )
				{
					gxTv_SdtEventMessage_Eventmessageproperties = new GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventMessageProperty>( context, "EventMessageProperty", "");
				}
				return gxTv_SdtEventMessage_Eventmessageproperties;
			}
			set {
				gxTv_SdtEventMessage_Eventmessageproperties_N = false;
				gxTv_SdtEventMessage_Eventmessageproperties = value;
			}
		}

		[XmlIgnore]
		public GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventMessageProperty> gxTpr_Eventmessageproperties
		{
			get {
				if ( gxTv_SdtEventMessage_Eventmessageproperties == null )
				{
					gxTv_SdtEventMessage_Eventmessageproperties = new GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventMessageProperty>( context, "EventMessageProperty", "");
				}
				gxTv_SdtEventMessage_Eventmessageproperties_N = false;
				return gxTv_SdtEventMessage_Eventmessageproperties ;
			}
			set {
				gxTv_SdtEventMessage_Eventmessageproperties_N = false;
				gxTv_SdtEventMessage_Eventmessageproperties = value;
				SetDirty("Eventmessageproperties");
			}
		}

		public void gxTv_SdtEventMessage_Eventmessageproperties_SetNull()
		{
			gxTv_SdtEventMessage_Eventmessageproperties_N = true;
			gxTv_SdtEventMessage_Eventmessageproperties = null;
		}

		public bool gxTv_SdtEventMessage_Eventmessageproperties_IsNull()
		{
			return gxTv_SdtEventMessage_Eventmessageproperties == null;
		}
		public bool ShouldSerializegxTpr_Eventmessageproperties_GXBaseCollection_Json()
		{
			return gxTv_SdtEventMessage_Eventmessageproperties != null && gxTv_SdtEventMessage_Eventmessageproperties.Count > 0;

		}


		[SoapElement(ElementName="EventMessageVersion")]
		[XmlElement(ElementName="EventMessageVersion")]
		public string gxTpr_Eventmessageversion
		{
			get { 
				return gxTv_SdtEventMessage_Eventmessageversion; 
			}
			set { 
				gxTv_SdtEventMessage_Eventmessageversion = value;
				SetDirty("Eventmessageversion");
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
			gxTv_SdtEventMessage_Eventmessageid = "";
			gxTv_SdtEventMessage_Eventmessagedate = (DateTime)(DateTime.MinValue);
			gxTv_SdtEventMessage_Eventmessagesourcetype = "";
			gxTv_SdtEventMessage_Eventmessagedata = "";
			gxTv_SdtEventMessage_Eventmessageproperties_N = true;
			gxTv_SdtEventMessage_Eventmessageversion = "";

		

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

		protected string gxTv_SdtEventMessage_Eventmessageid;
		 

		protected DateTime gxTv_SdtEventMessage_Eventmessagedate;
		 

		protected string gxTv_SdtEventMessage_Eventmessagesourcetype;
		 

		protected string gxTv_SdtEventMessage_Eventmessagedata;
		 
		protected bool gxTv_SdtEventMessage_Eventmessageproperties_N;
		protected GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventMessageProperty> gxTv_SdtEventMessage_Eventmessageproperties = null;  

		protected string gxTv_SdtEventMessage_Eventmessageversion;
		 
	
	


		#endregion
	}
}