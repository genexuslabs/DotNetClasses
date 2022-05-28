/*
				   File: type_SdtEventMessages
			Description: EventMessages
				 Author: Nemo 🐠 for C# (.NET Core) version 17.0.4.150138
		   Program type: Callable routine
			  Main DBMS: 
*/
using System;
using System.Collections;
using GeneXus.Utils;
using GeneXus.Application;
using System.Xml.Serialization;
namespace GeneXus.Programs.genexusserverlessapi
{
	[XmlRoot(ElementName="EventMessages")]
	[XmlType(TypeName="EventMessages" , Namespace="ServerlessAPI" )]
	[Serializable]
	public class SdtEventMessages : GxUserType
	{
		public SdtEventMessages( )
		{
			/* Constructor for serialization */
		}

		public SdtEventMessages(IGxContext context)
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
			if (gxTv_SdtEventMessages_Eventmessage != null)
			{
				AddObjectProperty("EventMessage", gxTv_SdtEventMessages_Eventmessage, false);  
			}
			return;
		}
		#endregion

		#region Properties

		[SoapElement(ElementName="EventMessage" )]
		[XmlArray(ElementName="EventMessage"  )]
		[XmlArrayItemAttribute(ElementName="EventMessage" , IsNullable=false )]
		public GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventMessage> gxTpr_Eventmessage_GXBaseCollection
		{
			get {
				if ( gxTv_SdtEventMessages_Eventmessage == null )
				{
					gxTv_SdtEventMessages_Eventmessage = new GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventMessage>( context, "EventMessage", "");
				}
				return gxTv_SdtEventMessages_Eventmessage;
			}
			set {
				if ( gxTv_SdtEventMessages_Eventmessage == null )
				{
					gxTv_SdtEventMessages_Eventmessage = new GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventMessage>( context, "EventMessage", "");
				}
				gxTv_SdtEventMessages_Eventmessage_N = 0;

				gxTv_SdtEventMessages_Eventmessage = value;
			}
		}

		[XmlIgnore]
		public GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventMessage> gxTpr_Eventmessage
		{
			get {
				if ( gxTv_SdtEventMessages_Eventmessage == null )
				{
					gxTv_SdtEventMessages_Eventmessage = new GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventMessage>( context, "EventMessage", "");
				}
				gxTv_SdtEventMessages_Eventmessage_N = 0;

				return gxTv_SdtEventMessages_Eventmessage ;
			}
			set {
				gxTv_SdtEventMessages_Eventmessage_N = 0;

				gxTv_SdtEventMessages_Eventmessage = value;
				SetDirty("Eventmessage");
			}
		}

		public void gxTv_SdtEventMessages_Eventmessage_SetNull()
		{
			gxTv_SdtEventMessages_Eventmessage_N = 1;

			gxTv_SdtEventMessages_Eventmessage = null;
			return  ;
		}

		public bool gxTv_SdtEventMessages_Eventmessage_IsNull()
		{
			if (gxTv_SdtEventMessages_Eventmessage == null)
			{
				return true ;
			}
			return false ;
		}

		public bool ShouldSerializegxTpr_Eventmessage_GXBaseCollection_Json()
		{
				return gxTv_SdtEventMessages_Eventmessage != null && gxTv_SdtEventMessages_Eventmessage.Count > 0;

		}


		public override bool ShouldSerializeSdtJson()
		{
		  return ( 
		   ShouldSerializegxTpr_Eventmessage_GXBaseCollection_Json()||  
		  false  );
		}

		#endregion

		#region Initialization

		public void initialize( )
		{
			gxTv_SdtEventMessages_Eventmessage_N = 1;

			return  ;
		}



		#endregion

		#region Declaration

		protected short gxTv_SdtEventMessages_Eventmessage_N;
		protected GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventMessage> gxTv_SdtEventMessages_Eventmessage = null;  


		#endregion
	}

}