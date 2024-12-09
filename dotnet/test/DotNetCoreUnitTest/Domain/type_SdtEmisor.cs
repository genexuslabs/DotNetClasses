/*
				   File: type_SdtEmisor
			Description: Emisor
				 Author: Nemo 🐠 for C# version 18.0.11.184517
		   Program type: Callable routine
			  Main DBMS: 
*/
using System;
using System.Collections;
using System.ServiceModel;
using System.Xml.Serialization;
using GeneXus.Application;
using GeneXus.Utils;


namespace GeneXus.Programs
{
	[XmlSerializerFormat]
	[XmlRoot(ElementName="Emisor")]
	[XmlType(TypeName="Emisor")]
	[Serializable]
	public class SdtEmisor : GxUserType
	{
		public SdtEmisor( )
		{
			/* Constructor for serialization */
			gxTv_SdtEmisor_Rucemisor = "";
			gxTv_SdtEmisor_Rucemisor_N = true;

			gxTv_SdtEmisor_Rznsoc = "";
			gxTv_SdtEmisor_Rznsoc_N = true;

			gxTv_SdtEmisor_Nomcomercial = "";
			gxTv_SdtEmisor_Nomcomercial_N = true;

			gxTv_SdtEmisor_Giroemis = "";
			gxTv_SdtEmisor_Giroemis_N = true;

			gxTv_SdtEmisor_Departamento = "";
			gxTv_SdtEmisor_Departamento_N = true;

		}

		public SdtEmisor(IGxContext context)
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
			AddObjectProperty("RUCEmisor", gxTpr_Rucemisor, false);


			AddObjectProperty("RznSoc", gxTpr_Rznsoc, false);


			AddObjectProperty("NomComercial", gxTpr_Nomcomercial, false);


			AddObjectProperty("GiroEmis", gxTpr_Giroemis, false);


			AddObjectProperty("Departamento", gxTpr_Departamento, false);

			return;
		}
		#endregion

		#region Properties
		[SoapElement(ElementName="RUCEmisor")]
		[XmlElement(ElementName="RUCEmisor")]
		public string gxTpr_Rucemisor
		{
			get {
				return gxTv_SdtEmisor_Rucemisor; 
			}
			set {
				gxTv_SdtEmisor_Rucemisor_N = false;
				gxTv_SdtEmisor_Rucemisor = value;
				SetDirty("Rucemisor");
			}
		}

		public bool ShouldSerializegxTpr_Rucemisor()

		{
			return !gxTv_SdtEmisor_Rucemisor_N;

		}



		[SoapElement(ElementName="RznSoc")]
		[XmlElement(ElementName="RznSoc")]
		public string gxTpr_Rznsoc
		{
			get {
				return gxTv_SdtEmisor_Rznsoc; 
			}
			set {
				gxTv_SdtEmisor_Rznsoc_N = false;
				gxTv_SdtEmisor_Rznsoc = value;
				SetDirty("Rznsoc");
			}
		}

		public bool ShouldSerializegxTpr_Rznsoc()

		{
			return !gxTv_SdtEmisor_Rznsoc_N;

		}



		[SoapElement(ElementName="NomComercial")]
		[XmlElement(ElementName="NomComercial")]
		public string gxTpr_Nomcomercial
		{
			get {
				return gxTv_SdtEmisor_Nomcomercial; 
			}
			set {
				gxTv_SdtEmisor_Nomcomercial_N = false;
				gxTv_SdtEmisor_Nomcomercial = value;
				SetDirty("Nomcomercial");
			}
		}

		public bool ShouldSerializegxTpr_Nomcomercial()

		{
			return !gxTv_SdtEmisor_Nomcomercial_N;

		}



		[SoapElement(ElementName="GiroEmis")]
		[XmlElement(ElementName="GiroEmis")]
		public string gxTpr_Giroemis
		{
			get {
				return gxTv_SdtEmisor_Giroemis; 
			}
			set {
				gxTv_SdtEmisor_Giroemis_N = false;
				gxTv_SdtEmisor_Giroemis = value;
				SetDirty("Giroemis");
			}
		}

		public bool ShouldSerializegxTpr_Giroemis()

		{
			return !gxTv_SdtEmisor_Giroemis_N;

		}



		[SoapElement(ElementName="Departamento")]
		[XmlElement(ElementName="Departamento")]
		public string gxTpr_Departamento
		{
			get {
				return gxTv_SdtEmisor_Departamento; 
			}
			set {
				gxTv_SdtEmisor_Departamento_N = false;
				gxTv_SdtEmisor_Departamento = value;
				SetDirty("Departamento");
			}
		}

		public bool ShouldSerializegxTpr_Departamento()

		{
			return !gxTv_SdtEmisor_Departamento_N;

		}


		public override bool ShouldSerializeSdtJson()
		{
			return true;
		}



		#endregion

		#region Static Type Properties

		[SoapIgnore]
		[XmlIgnore]
		private static GXTypeInfo _typeProps;
		protected override GXTypeInfo TypeInfo { get { return _typeProps; } set { _typeProps = value; } }

		#endregion

		#region Initialization

		public void initialize( )
		{
			gxTv_SdtEmisor_Rucemisor = "";
			gxTv_SdtEmisor_Rucemisor_N = true;

			gxTv_SdtEmisor_Rznsoc = "";
			gxTv_SdtEmisor_Rznsoc_N = true;

			gxTv_SdtEmisor_Nomcomercial = "";
			gxTv_SdtEmisor_Nomcomercial_N = true;

			gxTv_SdtEmisor_Giroemis = "";
			gxTv_SdtEmisor_Giroemis_N = true;

			gxTv_SdtEmisor_Departamento = "";
			gxTv_SdtEmisor_Departamento_N = true;

			return  ;
		}



		#endregion

		#region Declaration

		protected string gxTv_SdtEmisor_Rucemisor;
		protected bool gxTv_SdtEmisor_Rucemisor_N;
		 

		protected string gxTv_SdtEmisor_Rznsoc;
		protected bool gxTv_SdtEmisor_Rznsoc_N;
		 

		protected string gxTv_SdtEmisor_Nomcomercial;
		protected bool gxTv_SdtEmisor_Nomcomercial_N;
		 

		protected string gxTv_SdtEmisor_Giroemis;
		protected bool gxTv_SdtEmisor_Giroemis_N;
		 

		protected string gxTv_SdtEmisor_Departamento;
		protected bool gxTv_SdtEmisor_Departamento_N;
		 


		#endregion
	}

}