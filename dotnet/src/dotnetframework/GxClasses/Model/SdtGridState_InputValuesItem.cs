/*
				   File: type_SdtGridState_InputValuesItem
			Description: InputValues
				 Author: Nemo 🐠 for C# version 17.0.0.144776
		   Program type: Callable routine
			  Main DBMS: 
*/
using System;
using System.Collections;
using GeneXus.Utils;
using GeneXus.Application;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.ServiceModel;
namespace GeneXus.Core.genexus.common
{
	[XmlSerializerFormat]
	[XmlRoot(ElementName = "GridState.InputValuesItem")]
	[XmlType(TypeName = "GridState.InputValuesItem", Namespace = "GeneXus")]
	[Serializable]
	public class SdtGridState_InputValuesItem : GxUserType
	{
		public SdtGridState_InputValuesItem()
		{
			/* Constructor for serialization */
			gxTv_SdtGridState_InputValuesItem_Name = "";

			gxTv_SdtGridState_InputValuesItem_Value = "";

		}

		public SdtGridState_InputValuesItem(IGxContext context)
		{
			this.context = context;
			initialize();
		}

		#region Json
		private static Hashtable mapper;
		public override String JsonMap(String value)
		{
			if (mapper == null)
			{
				mapper = new Hashtable();
			}
			return (String)mapper[value]; ;
		}

		public override void ToJSON()
		{
			ToJSON(true);
			return;
		}

		public override void ToJSON(bool includeState)
		{
			AddObjectProperty("Name", gxTpr_Name, false);


			AddObjectProperty("Value", gxTpr_Value, false);

			return;
		}
		#endregion

		#region Properties

		[SoapElement(ElementName = "Name")]
		[XmlElement(ElementName = "Name")]
		public String gxTpr_Name
		{
			get
			{
				return gxTv_SdtGridState_InputValuesItem_Name;
			}
			set
			{
				gxTv_SdtGridState_InputValuesItem_Name = value;
				SetDirty("Name");
			}
		}




		[SoapElement(ElementName = "Value")]
		[XmlElement(ElementName = "Value")]
		public String gxTpr_Value
		{
			get
			{
				return gxTv_SdtGridState_InputValuesItem_Value;
			}
			set
			{
				gxTv_SdtGridState_InputValuesItem_Value = value;
				SetDirty("Value");
			}
		}




		#endregion

		#region Initialization

		public void initialize()
		{
			gxTv_SdtGridState_InputValuesItem_Name = "";
			gxTv_SdtGridState_InputValuesItem_Value = "";
			return;
		}



		#endregion

		#region Declaration

		protected String gxTv_SdtGridState_InputValuesItem_Name;


		protected String gxTv_SdtGridState_InputValuesItem_Value;



		#endregion
	}
	#region Rest interface
	[DataContract(Name = @"GridState.InputValuesItem", Namespace = "GeneXus")]
	public class SdtGridState_InputValuesItem_RESTInterface : GxGenericCollectionItem<SdtGridState_InputValuesItem>, System.Web.SessionState.IRequiresSessionState
	{
		public SdtGridState_InputValuesItem_RESTInterface() : base()
		{
		}

		public SdtGridState_InputValuesItem_RESTInterface(SdtGridState_InputValuesItem psdt) : base(psdt)
		{
		}

		#region Rest Properties
		[DataMember(Name = "Name", Order = 0)]
		public String gxTpr_Name
		{
			get
			{
				return sdt.gxTpr_Name;

			}
			set
			{
				sdt.gxTpr_Name = value;
			}
		}

		[DataMember(Name = "Value", Order = 1)]
		public String gxTpr_Value
		{
			get
			{
				return sdt.gxTpr_Value;

			}
			set
			{
				sdt.gxTpr_Value = value;
			}
		}


		#endregion

		public SdtGridState_InputValuesItem sdt
		{
			get
			{
				return (SdtGridState_InputValuesItem)Sdt;
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
				sdt = new SdtGridState_InputValuesItem();
			}
		}
	}
	#endregion
}