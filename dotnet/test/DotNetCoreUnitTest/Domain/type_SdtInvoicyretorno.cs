/*
				   File: type_SdtInvoicyretorno
			Description: Invoicyretorno
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
	[XmlRoot(ElementName="Invoicyretorno")]
	[XmlType(TypeName="Invoicyretorno" , Namespace="InvoiCy" )]
	public class SdtInvoicyretorno : GxUserType
	{
		public SdtInvoicyretorno( )
		{
			/* Constructor for serialization */
		}

		public SdtInvoicyretorno(IGxContext context)
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
			if (gxTv_SdtInvoicyretorno_Mensagem != null)
			{
				AddObjectProperty("Mensagem", gxTv_SdtInvoicyretorno_Mensagem, false);
			}
			return;
		}
		#endregion

		#region Properties

		[SoapElement(ElementName="Mensagem" )]
		[XmlArray(ElementName="Mensagem"  )]
		[XmlArrayItemAttribute(ElementName="MensagemItem" , IsNullable=false )]
		public GXBaseCollection<SdtInvoicyretorno_MensagemItem> gxTpr_Mensagem
		{
			get {
				if ( gxTv_SdtInvoicyretorno_Mensagem == null )
				{
					gxTv_SdtInvoicyretorno_Mensagem = new GXBaseCollection<SdtInvoicyretorno_MensagemItem>( context, "Invoicyretorno.MensagemItem", "");
				}
				SetDirty("Mensagem");
				return gxTv_SdtInvoicyretorno_Mensagem;
			}
			set {
				gxTv_SdtInvoicyretorno_Mensagem_N = false;
				gxTv_SdtInvoicyretorno_Mensagem = value;
				SetDirty("Mensagem");
			}
		}

		public void gxTv_SdtInvoicyretorno_Mensagem_SetNull()
		{
			gxTv_SdtInvoicyretorno_Mensagem_N = true;
			gxTv_SdtInvoicyretorno_Mensagem = null;
		}

		public bool gxTv_SdtInvoicyretorno_Mensagem_IsNull()
		{
			return gxTv_SdtInvoicyretorno_Mensagem == null;
		}
		public bool ShouldSerializegxTpr_Mensagem_GxSimpleCollection_Json()
		{
			return gxTv_SdtInvoicyretorno_Mensagem != null && gxTv_SdtInvoicyretorno_Mensagem.Count > 0;

		}


		public override bool ShouldSerializeSdtJson()
		{
			return (
				ShouldSerializegxTpr_Mensagem_GxSimpleCollection_Json() || 
				false);
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
			gxTv_SdtInvoicyretorno_Mensagem_N = true;

			return  ;
		}



		#endregion

		#region Declaration

		protected bool gxTv_SdtInvoicyretorno_Mensagem_N;
		protected GXBaseCollection<SdtInvoicyretorno_MensagemItem> gxTv_SdtInvoicyretorno_Mensagem = null; 



		#endregion
	}

}