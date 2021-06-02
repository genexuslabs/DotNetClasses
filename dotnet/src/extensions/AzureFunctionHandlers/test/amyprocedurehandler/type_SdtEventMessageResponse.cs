/*
				   File: type_SdtEventMessageResponse
			Description: EventMessageResponse
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
	[XmlRoot(ElementName="EventMessageResponse")]
	[XmlType(TypeName="EventMessageResponse" , Namespace="ServerlessAPI" )]
	[Serializable]
	public class SdtEventMessageResponse : GxUserType
	{
		public SdtEventMessageResponse( )
		{
			/* Constructor for serialization */
			gxTv_SdtEventMessageResponse_Errormessage = "";

		}

		public SdtEventMessageResponse(IGxContext context)
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
			AddObjectProperty("Handled", gxTpr_Handled, false);


			AddObjectProperty("ErrorMessage", gxTpr_Errormessage, false);

			return;
		}
		#endregion

		#region Properties

		[SoapElement(ElementName="Handled")]
		[XmlElement(ElementName="Handled")]
		public bool gxTpr_Handled
		{
			get { 
				return gxTv_SdtEventMessageResponse_Handled; 
			}
			set { 
				gxTv_SdtEventMessageResponse_Handled = value;
				SetDirty("Handled");
			}
		}




		[SoapElement(ElementName="ErrorMessage")]
		[XmlElement(ElementName="ErrorMessage")]
		public string gxTpr_Errormessage
		{
			get { 
				return gxTv_SdtEventMessageResponse_Errormessage; 
			}
			set { 
				gxTv_SdtEventMessageResponse_Errormessage = value;
				SetDirty("Errormessage");
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
			gxTv_SdtEventMessageResponse_Errormessage = "";
			return  ;
		}



		#endregion

		#region Declaration

		protected bool gxTv_SdtEventMessageResponse_Handled;
		 

		protected string gxTv_SdtEventMessageResponse_Errormessage;
		 


		#endregion
	}

}