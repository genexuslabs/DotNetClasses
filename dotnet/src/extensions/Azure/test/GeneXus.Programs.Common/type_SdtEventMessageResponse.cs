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
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Reflection.Metadata;

namespace GeneXus.Programs.genexusserverlessapi
{
	[XmlRoot(ElementName = "EventMessageResponse")]
	[XmlType(TypeName = "EventMessageResponse", Namespace = "ServerlessAPI")]
	[Serializable]
	public class SdtEventMessageResponse : GxUserType
	{
		public SdtEventMessageResponse()
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
			ToJSON(true);
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

		[SoapElement(ElementName = "Handled")]
		[XmlElement(ElementName = "Handled")]
		public bool gxTpr_Handled
		{
			get
			{
				return gxTv_SdtEventMessageResponse_Handled;
			}
			set
			{
				gxTv_SdtEventMessageResponse_Handled = value;
				SetDirty("Handled");
			}
		}




		[SoapElement(ElementName = "ErrorMessage")]
		[XmlElement(ElementName = "ErrorMessage")]
		public string gxTpr_Errormessage
		{
			get
			{
				return gxTv_SdtEventMessageResponse_Errormessage;
			}
			set
			{
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

		public void initialize()
		{
			gxTv_SdtEventMessageResponse_Errormessage = "";
			return;
		}



		#endregion

		#region Declaration

		protected bool gxTv_SdtEventMessageResponse_Handled;


		protected string gxTv_SdtEventMessageResponse_Errormessage;



		#endregion
	}
	#region Rest interface
	[GxUnWrappedJson()]
	[DataContract(Name = @"EventMessageResponse", Namespace = "GeneXus")]
	public class SdtEventMessageResponse_RESTInterface : GxGenericCollectionItem<SdtEventMessageResponse>, System.Web.SessionState.IRequiresSessionState
	{
		public SdtEventMessageResponse_RESTInterface() : base()
		{
		}

		public SdtEventMessageResponse_RESTInterface(SdtEventMessageResponse psdt) : base(psdt)
		{
		}

		#region Rest Properties
		[DataMember(Name = "Handled", Order = 0)]
		public bool gxTpr_Handled
		{
			get
			{
				return sdt.gxTpr_Handled;

			}
			set
			{
				sdt.gxTpr_Handled = value;
			}
		}
		[DataMember(Name = "ErrorMessage", Order = 0)]
		public string gxTpr_ErrorMessage
		{
			get
			{
				return sdt.gxTpr_Errormessage;

			}
			set
			{
				sdt.gxTpr_Errormessage = value;
			}
		}
		#endregion

		public SdtEventMessageResponse sdt
		{
			get
			{
				return (SdtEventMessageResponse)Sdt;
			}
			set
			{
				Sdt = value;
			}
		}

		[OnDeserializing]
		void checkSdt(StreamingContext ctx)
		{
			if (sdt == null)
			{
				sdt = new SdtEventMessageResponse();
			}
		}
	}
	#endregion
}