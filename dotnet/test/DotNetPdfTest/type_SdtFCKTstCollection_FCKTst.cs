/*
				   File: type_SdtFCKTstCollection_FCKTst
			Description: FCKTstCollection
				 Author: Nemo 🐠 for C# (.NET) version 18.0.6.176584
		   Program type: Callable routine
			  Main DBMS: 
*/
using System;
using System.Collections;
using GeneXus.Utils;
using GeneXus.Resources;
using GeneXus.Application;
using GeneXus.Metadata;
using GeneXus.Cryptography;
using GeneXus.Encryption;
using GeneXus.Http.Client;
using GeneXus.Http.Server;
using System.Reflection;
using System.Xml.Serialization;
using System.Runtime.Serialization;


namespace GeneXus.Programs
{
	[XmlRoot(ElementName="FCKTst")]
	[XmlType(TypeName="FCKTst" , Namespace="TestReportes" )]
	[Serializable]
	public class SdtFCKTstCollection_FCKTst : GxUserType
	{
		public SdtFCKTstCollection_FCKTst( )
		{
			/* Constructor for serialization */
			gxTv_SdtFCKTstCollection_FCKTst_Fcktstdsc = "";

			gxTv_SdtFCKTstCollection_FCKTst_Fcktstfck = "";

		}

		public SdtFCKTstCollection_FCKTst(IGxContext context)
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
			AddObjectProperty("FCKTstId", gxTpr_Fcktstid, false);


			AddObjectProperty("FCKTstDsc", gxTpr_Fcktstdsc, false);


			AddObjectProperty("FCKTstFCK", gxTpr_Fcktstfck, false);


			AddObjectProperty("FCKTstOtro", gxTpr_Fcktstotro, false);

			return;
		}
		#endregion

		#region Properties

		[SoapElement(ElementName="FCKTstId")]
		[XmlElement(ElementName="FCKTstId")]
		public short gxTpr_Fcktstid
		{
			get {
				return gxTv_SdtFCKTstCollection_FCKTst_Fcktstid; 
			}
			set {
				gxTv_SdtFCKTstCollection_FCKTst_Fcktstid = value;
				SetDirty("Fcktstid");
			}
		}




		[SoapElement(ElementName="FCKTstDsc")]
		[XmlElement(ElementName="FCKTstDsc")]
		public string gxTpr_Fcktstdsc
		{
			get {
				return gxTv_SdtFCKTstCollection_FCKTst_Fcktstdsc; 
			}
			set {
				gxTv_SdtFCKTstCollection_FCKTst_Fcktstdsc = value;
				SetDirty("Fcktstdsc");
			}
		}




		[SoapElement(ElementName="FCKTstFCK")]
		[XmlElement(ElementName="FCKTstFCK")]
		public string gxTpr_Fcktstfck
		{
			get {
				return gxTv_SdtFCKTstCollection_FCKTst_Fcktstfck; 
			}
			set {
				gxTv_SdtFCKTstCollection_FCKTst_Fcktstfck = value;
				SetDirty("Fcktstfck");
			}
		}




		[SoapElement(ElementName="FCKTstOtro")]
		[XmlElement(ElementName="FCKTstOtro")]
		public short gxTpr_Fcktstotro
		{
			get {
				return gxTv_SdtFCKTstCollection_FCKTst_Fcktstotro; 
			}
			set {
				gxTv_SdtFCKTstCollection_FCKTst_Fcktstotro = value;
				SetDirty("Fcktstotro");
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
			gxTv_SdtFCKTstCollection_FCKTst_Fcktstdsc = "";
			gxTv_SdtFCKTstCollection_FCKTst_Fcktstfck = "";

			return  ;
		}



		#endregion

		#region Declaration

		protected short gxTv_SdtFCKTstCollection_FCKTst_Fcktstid;
		 

		protected string gxTv_SdtFCKTstCollection_FCKTst_Fcktstdsc;
		 

		protected string gxTv_SdtFCKTstCollection_FCKTst_Fcktstfck;
		 

		protected short gxTv_SdtFCKTstCollection_FCKTst_Fcktstotro;
		 


		#endregion
	}
	#region Rest interface
	[DataContract(Name=@"FCKTst", Namespace="TestReportes")]
	public class SdtFCKTstCollection_FCKTst_RESTInterface : GxGenericCollectionItem<SdtFCKTstCollection_FCKTst>, System.Web.SessionState.IRequiresSessionState
	{
		public SdtFCKTstCollection_FCKTst_RESTInterface( ) : base()
		{	
		}

		public SdtFCKTstCollection_FCKTst_RESTInterface( SdtFCKTstCollection_FCKTst psdt ) : base(psdt)
		{	
		}

		#region Rest Properties
		[DataMember(Name="FCKTstId", Order=0)]
		public short gxTpr_Fcktstid
		{
			get { 
				return sdt.gxTpr_Fcktstid;

			}
			set { 
				sdt.gxTpr_Fcktstid = value;
			}
		}

		[DataMember(Name="FCKTstDsc", Order=1)]
		public  string gxTpr_Fcktstdsc
		{
			get { 
				return StringUtil.RTrim( sdt.gxTpr_Fcktstdsc);

			}
			set { 
				 sdt.gxTpr_Fcktstdsc = value;
			}
		}

		[DataMember(Name="FCKTstFCK", Order=2)]
		public  string gxTpr_Fcktstfck
		{
			get { 
				return StringUtil.RTrim( sdt.gxTpr_Fcktstfck);

			}
			set { 
				 sdt.gxTpr_Fcktstfck = value;
			}
		}

		[DataMember(Name="FCKTstOtro", Order=3)]
		public short gxTpr_Fcktstotro
		{
			get { 
				return sdt.gxTpr_Fcktstotro;

			}
			set { 
				sdt.gxTpr_Fcktstotro = value;
			}
		}


		#endregion

		public SdtFCKTstCollection_FCKTst sdt
		{
			get { 
				return (SdtFCKTstCollection_FCKTst)Sdt;
			}
			set { 
				Sdt = value;
			}
		}

		[OnDeserializing]
		void checkSdt( StreamingContext ctx )
		{
			if ( sdt == null )
			{
				sdt = new SdtFCKTstCollection_FCKTst() ;
			}
		}
	}
	#endregion
}