/*
				   File: type_SdtInvoicyretorno_MensagemItem
			Description: Mensagem
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
	[XmlRoot(ElementName="Invoicyretorno.MensagemItem")]
	[XmlType(TypeName="Invoicyretorno.MensagemItem" , Namespace="InvoiCy" )]
	public class SdtInvoicyretorno_MensagemItem : GxUserType
	{
		public SdtInvoicyretorno_MensagemItem( )
		{
			/* Constructor for serialization */
			gxTv_SdtInvoicyretorno_MensagemItem_Descricao = "";

		}

		public SdtInvoicyretorno_MensagemItem(IGxContext context)
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
			AddObjectProperty("Codigo", gxTpr_Codigo, false);


			AddObjectProperty("Descricao", gxTpr_Descricao, false);

			if (gxTv_SdtInvoicyretorno_MensagemItem_Documentos != null)
			{
				AddObjectProperty("Documentos", gxTv_SdtInvoicyretorno_MensagemItem_Documentos, false);
			}
			return;
		}
		#endregion

		#region Properties

		[SoapElement(ElementName="Codigo")]
		[XmlElement(ElementName="Codigo")]
		public int gxTpr_Codigo
		{
			get {
				return gxTv_SdtInvoicyretorno_MensagemItem_Codigo; 
			}
			set {
				gxTv_SdtInvoicyretorno_MensagemItem_Codigo = value;
				SetDirty("Codigo");
			}
		}




		[SoapElement(ElementName="Descricao")]
		[XmlElement(ElementName="Descricao")]
		public string gxTpr_Descricao
		{
			get {
				return gxTv_SdtInvoicyretorno_MensagemItem_Descricao; 
			}
			set {
				gxTv_SdtInvoicyretorno_MensagemItem_Descricao = value;
				SetDirty("Descricao");
			}
		}




		[SoapElement(ElementName="Documentos" )]
		[XmlArray(ElementName="Documentos"  )]
		[XmlArrayItemAttribute(ElementName="DocumentosItem" , IsNullable=false )]
		public GXBaseCollection<SdtInvoicyretorno_MensagemItem_DocumentosItem> gxTpr_Documentos
		{
			get {
				if ( gxTv_SdtInvoicyretorno_MensagemItem_Documentos == null )
				{
					gxTv_SdtInvoicyretorno_MensagemItem_Documentos = new GXBaseCollection<SdtInvoicyretorno_MensagemItem_DocumentosItem>( context, "Invoicyretorno.MensagemItem.DocumentosItem", "");
				}
				SetDirty("Documentos");
				return gxTv_SdtInvoicyretorno_MensagemItem_Documentos;
			}
			set {
				gxTv_SdtInvoicyretorno_MensagemItem_Documentos_N = false;
				gxTv_SdtInvoicyretorno_MensagemItem_Documentos = value;
				SetDirty("Documentos");
			}
		}

		public void gxTv_SdtInvoicyretorno_MensagemItem_Documentos_SetNull()
		{
			gxTv_SdtInvoicyretorno_MensagemItem_Documentos_N = true;
			gxTv_SdtInvoicyretorno_MensagemItem_Documentos = null;
		}

		public bool gxTv_SdtInvoicyretorno_MensagemItem_Documentos_IsNull()
		{
			return gxTv_SdtInvoicyretorno_MensagemItem_Documentos == null;
		}
		public bool ShouldSerializegxTpr_Documentos_GxSimpleCollection_Json()
		{
			return gxTv_SdtInvoicyretorno_MensagemItem_Documentos != null && gxTv_SdtInvoicyretorno_MensagemItem_Documentos.Count > 0;

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

		public void initialize( )
		{
			gxTv_SdtInvoicyretorno_MensagemItem_Descricao = "";

			gxTv_SdtInvoicyretorno_MensagemItem_Documentos_N = true;

			return  ;
		}



		#endregion

		#region Declaration

		protected int gxTv_SdtInvoicyretorno_MensagemItem_Codigo;
		 

		protected string gxTv_SdtInvoicyretorno_MensagemItem_Descricao;
		 
		protected bool gxTv_SdtInvoicyretorno_MensagemItem_Documentos_N;
		protected GXBaseCollection<SdtInvoicyretorno_MensagemItem_DocumentosItem> gxTv_SdtInvoicyretorno_MensagemItem_Documentos = null; 



		#endregion
	}
	
}