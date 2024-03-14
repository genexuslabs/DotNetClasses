/*
				   File: type_SdtSDTTwoLevels_LevelItem
			Description: Level
				 Author: Nemo 🐠 for C# (.NET) version 18.0.9.180423
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
using System.Collections.Concurrent;


namespace GeneXus.Programs
{
	[XmlRoot(ElementName="SDTTwoLevels.LevelItem")]
	[XmlType(TypeName="SDTTwoLevels.LevelItem" , Namespace="MassiveSearch" )]
	[Serializable]
	public class SdtSDTTwoLevels_LevelItem : GxUserType
	{
		public SdtSDTTwoLevels_LevelItem( )
		{
			/* Constructor for serialization */
			gxTv_SdtSDTTwoLevels_LevelItem_Levelname = "";

		}

		public SdtSDTTwoLevels_LevelItem(IGxContext context)
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
			AddObjectProperty("LevelName", gxTpr_Levelname, false);

			return;
		}
		#endregion

		#region Properties

		[SoapElement(ElementName="LevelName")]
		[XmlElement(ElementName="LevelName")]
		public string gxTpr_Levelname
		{
			get {
				return gxTv_SdtSDTTwoLevels_LevelItem_Levelname; 
			}
			set {
				gxTv_SdtSDTTwoLevels_LevelItem_Levelname = value;
				SetDirty("Levelname");
			}
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
			gxTv_SdtSDTTwoLevels_LevelItem_Levelname = "";
			return  ;
		}



		#endregion

		#region Declaration

		protected string gxTv_SdtSDTTwoLevels_LevelItem_Levelname;
		 


		#endregion
	}
	#region Rest interface
	[DataContract(Name=@"SDTTwoLevels.LevelItem", Namespace="MassiveSearch")]
	public class SdtSDTTwoLevels_LevelItem_RESTInterface : GxGenericCollectionItem<SdtSDTTwoLevels_LevelItem>, System.Web.SessionState.IRequiresSessionState
	{
		public SdtSDTTwoLevels_LevelItem_RESTInterface( ) : base()
		{	
		}

		public SdtSDTTwoLevels_LevelItem_RESTInterface( SdtSDTTwoLevels_LevelItem psdt ) : base(psdt)
		{	
		}

		#region Rest Properties
		[DataMember(Name="LevelName", Order=0)]
		public  string gxTpr_Levelname
		{
			get { 
				return StringUtil.RTrim( sdt.gxTpr_Levelname);

			}
			set { 
				 sdt.gxTpr_Levelname = value;
			}
		}


		#endregion

		public SdtSDTTwoLevels_LevelItem sdt
		{
			get { 
				return (SdtSDTTwoLevels_LevelItem)Sdt;
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
				sdt = new SdtSDTTwoLevels_LevelItem() ;
			}
		}
	}
	#endregion
}