/*
				   File: type_SdtInvoicyretorno_MensagemItem_DocumentosItem
			Description: Documentos
				 Author: Nemo 🐠 for C# (.NET) version 18.0.13.186668
		   Program type: Callable routine
			  Main DBMS: 
*/
using GeneXus.Application;
using GeneXus.Utils;
using System.Collections;
using System.Xml.Serialization;


namespace GeneXus.Programs
{
	[XmlRoot(ElementName = "Invoicyretorno.MensagemItem.DocumentosItem")]
	[XmlType(TypeName = "Invoicyretorno.MensagemItem.DocumentosItem", Namespace = "InvoiCy")]
	public class SdtInvoicyretorno_MensagemItem_DocumentosItem : GxUserType
	{
		public SdtInvoicyretorno_MensagemItem_DocumentosItem()
		{
			/* Constructor for serialization */
			gxTv_SdtInvoicyretorno_MensagemItem_DocumentosItem_Documento = "";

		}

		public SdtInvoicyretorno_MensagemItem_DocumentosItem(IGxContext context)
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
			AddObjectProperty("Documento", gxTpr_Documento, false);

			return;
		}
		#endregion

		#region Properties

		[XmlElement(ElementName = "Documento")]
		public GXCData gxTpr_Documento_GXCData
		{
			get
			{
				return new GXCData(gxTv_SdtInvoicyretorno_MensagemItem_DocumentosItem_Documento);
			}
			set
			{
				gxTv_SdtInvoicyretorno_MensagemItem_DocumentosItem_Documento = (string)(value.content);
			}
		}
		[XmlIgnore]
		public string gxTpr_Documento
		{
			get
			{
				return gxTv_SdtInvoicyretorno_MensagemItem_DocumentosItem_Documento;
			}
			set
			{
				gxTv_SdtInvoicyretorno_MensagemItem_DocumentosItem_Documento = value;
				SetDirty("Documento");
			}
		}



		public override bool ShouldSerializeSdtJson()
		{
			return true;
		}



		#endregion

		#region Static Type Properties

		[XmlIgnore]
		private static GXTypeInfo _typeProps;
		protected override GXTypeInfo TypeInfo { get { return _typeProps; } set { _typeProps = value; } }

		#endregion

		#region Initialization

		public void initialize()
		{
			gxTv_SdtInvoicyretorno_MensagemItem_DocumentosItem_Documento = "";
			return;
		}



		#endregion

		#region Declaration

		protected string gxTv_SdtInvoicyretorno_MensagemItem_DocumentosItem_Documento;



		#endregion
	}


}