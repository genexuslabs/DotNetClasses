/*
				   File: type_SdtEventCustomPayload_CustomPayloadItem
			Description: EventCustomPayload
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
	[XmlRoot(ElementName="CustomPayloadItem")]
	[XmlType(TypeName="CustomPayloadItem" , Namespace="ServerlessAPI" )]
	[Serializable]
	public class SdtEventCustomPayload_CustomPayloadItem : GxUserType
	{
		public SdtEventCustomPayload_CustomPayloadItem( )
		{
			/* Constructor for serialization */
			gxTv_SdtEventCustomPayload_CustomPayloadItem_Propertyid = "";

			gxTv_SdtEventCustomPayload_CustomPayloadItem_Propertyvalue = "";

		}

		public SdtEventCustomPayload_CustomPayloadItem(IGxContext context)
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
				return gxTv_SdtEventCustomPayload_CustomPayloadItem_Propertyid; 
			}
			set { 
				gxTv_SdtEventCustomPayload_CustomPayloadItem_Propertyid = value;
				SetDirty("Propertyid");
			}
		}




		[SoapElement(ElementName="PropertyValue")]
		[XmlElement(ElementName="PropertyValue")]
		public string gxTpr_Propertyvalue
		{
			get { 
				return gxTv_SdtEventCustomPayload_CustomPayloadItem_Propertyvalue; 
			}
			set { 
				gxTv_SdtEventCustomPayload_CustomPayloadItem_Propertyvalue = value;
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
			gxTv_SdtEventCustomPayload_CustomPayloadItem_Propertyid = "";
			gxTv_SdtEventCustomPayload_CustomPayloadItem_Propertyvalue = "";
			return  ;
		}



		#endregion

		#region Declaration

		protected string gxTv_SdtEventCustomPayload_CustomPayloadItem_Propertyid;
		 

		protected string gxTv_SdtEventCustomPayload_CustomPayloadItem_Propertyvalue;
		 


		#endregion
	}

}