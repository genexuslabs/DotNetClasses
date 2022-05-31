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


			AddObjectProperty("EventMessageVersion", gxTpr_Eventmessageversion, false);

			if (gxTv_SdtEventMessage_Eventmessagecustompayload != null)
			{
				AddObjectProperty("EventMessageCustomPayload", gxTv_SdtEventMessage_Eventmessagecustompayload, false);  
			}
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




		[SoapElement(ElementName="EventMessageCustomPayload" )]
		[XmlArray(ElementName="EventMessageCustomPayload"  )]
		[XmlArrayItemAttribute(ElementName="CustomPayloadItem" , IsNullable=false )]
		public GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem> gxTpr_Eventmessagecustompayload_GXBaseCollection
		{
			get {
				if ( gxTv_SdtEventMessage_Eventmessagecustompayload == null )
				{
					gxTv_SdtEventMessage_Eventmessagecustompayload = new GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem>( context, "EventCustomPayload", "");
				}
				return gxTv_SdtEventMessage_Eventmessagecustompayload;
			}
			set {
				if ( gxTv_SdtEventMessage_Eventmessagecustompayload == null )
				{
					gxTv_SdtEventMessage_Eventmessagecustompayload = new GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem>( context, "EventCustomPayload", "");
				}
				gxTv_SdtEventMessage_Eventmessagecustompayload_N = 0;

				gxTv_SdtEventMessage_Eventmessagecustompayload = value;
			}
		}

		[XmlIgnore]
		public GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem> gxTpr_Eventmessagecustompayload
		{
			get {
				if ( gxTv_SdtEventMessage_Eventmessagecustompayload == null )
				{
					gxTv_SdtEventMessage_Eventmessagecustompayload = new GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem>( context, "EventCustomPayload", "");
				}
				gxTv_SdtEventMessage_Eventmessagecustompayload_N = 0;

				return gxTv_SdtEventMessage_Eventmessagecustompayload ;
			}
			set {
				gxTv_SdtEventMessage_Eventmessagecustompayload_N = 0;

				gxTv_SdtEventMessage_Eventmessagecustompayload = value;
				SetDirty("Eventmessagecustompayload");
			}
		}

		public void gxTv_SdtEventMessage_Eventmessagecustompayload_SetNull()
		{
			gxTv_SdtEventMessage_Eventmessagecustompayload_N = 1;

			gxTv_SdtEventMessage_Eventmessagecustompayload = null;
			return  ;
		}

		public bool gxTv_SdtEventMessage_Eventmessagecustompayload_IsNull()
		{
			if (gxTv_SdtEventMessage_Eventmessagecustompayload == null)
			{
				return true ;
			}
			return false ;
		}

		public bool ShouldSerializegxTpr_Eventmessagecustompayload_GXBaseCollection_Json()
		{
				return gxTv_SdtEventMessage_Eventmessagecustompayload != null && gxTv_SdtEventMessage_Eventmessagecustompayload.Count > 0;

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
			gxTv_SdtEventMessage_Eventmessageversion = "";

			gxTv_SdtEventMessage_Eventmessagecustompayload_N = 1;

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
		 

		protected string gxTv_SdtEventMessage_Eventmessageversion;
		 
		protected short gxTv_SdtEventMessage_Eventmessagecustompayload_N;
		protected GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem> gxTv_SdtEventMessage_Eventmessagecustompayload = null;  


		#endregion
	}
}