/*
				   File: type_SdtGridState
			Description: GridState
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
	[XmlRoot(ElementName = "GridState")]
	[XmlType(TypeName = "GridState", Namespace = "GeneXus")]
	[Serializable]
	public class SdtGridState : GxUserType
	{
		public SdtGridState()
		{
			/* Constructor for serialization */
		}

		public SdtGridState(IGxContext context)
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
			AddObjectProperty("CurrentPage", gxTpr_Currentpage, false);


			AddObjectProperty("OrderedBy", gxTpr_Orderedby, false);

			if (gxTv_SdtGridState_Inputvalues != null)
			{
				AddObjectProperty("InputValues", gxTv_SdtGridState_Inputvalues, false);
			}
			return;
		}
		#endregion

		#region Properties

		[SoapElement(ElementName = "CurrentPage")]
		[XmlElement(ElementName = "CurrentPage")]
		public int gxTpr_Currentpage
		{
			get
			{
				return gxTv_SdtGridState_Currentpage;
			}
			set
			{
				gxTv_SdtGridState_Currentpage = value;
				SetDirty("Currentpage");
			}
		}




		[SoapElement(ElementName = "OrderedBy")]
		[XmlElement(ElementName = "OrderedBy")]
		public short gxTpr_Orderedby
		{
			get
			{
				return gxTv_SdtGridState_Orderedby;
			}
			set
			{
				gxTv_SdtGridState_Orderedby = value;
				SetDirty("Orderedby");
			}
		}




		[SoapElement(ElementName = "InputValues")]
		[XmlArray(ElementName = "InputValues")]
		[XmlArrayItemAttribute(ElementName = "InputValuesItem", IsNullable = false)]
		public GXBaseCollection<SdtGridState_InputValuesItem> gxTpr_Inputvalues
		{
			get
			{
				if (gxTv_SdtGridState_Inputvalues == null)
				{
					gxTv_SdtGridState_Inputvalues = new GXBaseCollection<SdtGridState_InputValuesItem>(context, "GridState.InputValuesItem", "");
				}
				return gxTv_SdtGridState_Inputvalues;
			}
			set
			{
				if (gxTv_SdtGridState_Inputvalues == null)
				{
					gxTv_SdtGridState_Inputvalues = new GXBaseCollection<SdtGridState_InputValuesItem>(context, "GridState.InputValuesItem", "");
				}
				gxTv_SdtGridState_Inputvalues_N = 0;

				gxTv_SdtGridState_Inputvalues = value;
				SetDirty("Inputvalues");
			}
		}

		public void gxTv_SdtGridState_Inputvalues_SetNull()
		{
			gxTv_SdtGridState_Inputvalues_N = 1;

			gxTv_SdtGridState_Inputvalues = null;
			return;
		}

		public bool gxTv_SdtGridState_Inputvalues_IsNull()
		{
			if (gxTv_SdtGridState_Inputvalues == null)
			{
				return true;
			}
			return false;
		}

		public bool ShouldSerializegxTpr_Inputvalues_GxSimpleCollection_Json()
		{
			return gxTv_SdtGridState_Inputvalues != null && gxTv_SdtGridState_Inputvalues.Count > 0;

		}



		#endregion

		#region Initialization

		public void initialize()
		{
			gxTv_SdtGridState_Inputvalues_N = 1;

			return;
		}



		#endregion

		#region Declaration

		protected int gxTv_SdtGridState_Currentpage;


		protected short gxTv_SdtGridState_Orderedby;

		protected short gxTv_SdtGridState_Inputvalues_N;
		protected GXBaseCollection<SdtGridState_InputValuesItem> gxTv_SdtGridState_Inputvalues = null;



		#endregion
	}
	#region Rest interface
	[DataContract(Name = @"GridState", Namespace = "GeneXus")]
	public class SdtGridState_RESTInterface : GxGenericCollectionItem<SdtGridState>, System.Web.SessionState.IRequiresSessionState
	{
		public SdtGridState_RESTInterface() : base()
		{
		}

		public SdtGridState_RESTInterface(SdtGridState psdt) : base(psdt)
		{
		}

		#region Rest Properties
		[DataMember(Name = "CurrentPage", Order = 0)]
		public int gxTpr_Currentpage
		{
			get
			{
				return sdt.gxTpr_Currentpage;

			}
			set
			{
				sdt.gxTpr_Currentpage = value;
			}
		}

		[DataMember(Name = "OrderedBy", Order = 1)]
		public short gxTpr_Orderedby
		{
			get
			{
				return sdt.gxTpr_Orderedby;

			}
			set
			{
				sdt.gxTpr_Orderedby = value;
			}
		}

		[DataMember(Name = "InputValues", Order = 2, EmitDefaultValue = false)]
		public GxGenericCollection<SdtGridState_InputValuesItem_RESTInterface> gxTpr_Inputvalues
		{
			get
			{
				if (sdt.ShouldSerializegxTpr_Inputvalues_GxSimpleCollection_Json())
					return new GxGenericCollection<SdtGridState_InputValuesItem_RESTInterface>(sdt.gxTpr_Inputvalues);
				else
					return null;

			}
			set
			{
				value.LoadCollection(sdt.gxTpr_Inputvalues);
			}
		}


		#endregion

		public SdtGridState sdt
		{
			get
			{
				return (SdtGridState)Sdt;
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
				sdt = new SdtGridState();
			}
		}
	}
	#endregion
}