/*
				   File: type_SdtSDTTwoLevels
			Description: SDTTwoLevels
				 Author: Nemo 🐠 for C# (.NET) version 18.0.9.180423
		   Program type: Callable routine
			  Main DBMS: 
*/
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using GeneXus.Application;
using GeneXus.Utils;


namespace GeneXus.Programs
{
	[XmlRoot(ElementName="SDTTwoLevels")]
	[XmlType(TypeName="SDTTwoLevels" , Namespace="MassiveSearch" )]
	[Serializable]
	public class SdtSDTTwoLevels : GxUserType
	{
		public SdtSDTTwoLevels( )
		{
			/* Constructor for serialization */
			gxTv_SdtSDTTwoLevels_Name = "";

		}

		public SdtSDTTwoLevels(IGxContext context)
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
			AddObjectProperty("Name", gxTpr_Name, false);

			if (gxTv_SdtSDTTwoLevels_Level != null)
			{
				AddObjectProperty("Level", gxTv_SdtSDTTwoLevels_Level, false);
			}
			return;
		}
		#endregion

		#region Properties

		[SoapElement(ElementName="Name")]
		[XmlElement(ElementName="Name")]
		public string gxTpr_Name
		{
			get {
				return gxTv_SdtSDTTwoLevels_Name; 
			}
			set {
				gxTv_SdtSDTTwoLevels_Name = value;
				SetDirty("Name");
			}
		}




		[SoapElement(ElementName="Level" )]
		[XmlArray(ElementName="Level"  )]
		[XmlArrayItemAttribute(ElementName="LevelItem" , IsNullable=false )]
		public GXBaseCollection<SdtSDTTwoLevels_LevelItem> gxTpr_Level
		{
			get {
				if ( gxTv_SdtSDTTwoLevels_Level == null )
				{
					gxTv_SdtSDTTwoLevels_Level = new GXBaseCollection<SdtSDTTwoLevels_LevelItem>( context, "SDTTwoLevels.LevelItem", "");
				}
				return gxTv_SdtSDTTwoLevels_Level;
			}
			set {
				gxTv_SdtSDTTwoLevels_Level_N = false;
				gxTv_SdtSDTTwoLevels_Level = value;
				SetDirty("Level");
			}
		}

		public void gxTv_SdtSDTTwoLevels_Level_SetNull()
		{
			gxTv_SdtSDTTwoLevels_Level_N = true;
			gxTv_SdtSDTTwoLevels_Level = null;
		}

		public bool gxTv_SdtSDTTwoLevels_Level_IsNull()
		{
			return gxTv_SdtSDTTwoLevels_Level == null;
		}
		public bool ShouldSerializegxTpr_Level_GxSimpleCollection_Json()
		{
			return gxTv_SdtSDTTwoLevels_Level != null && gxTv_SdtSDTTwoLevels_Level.Count > 0;

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
			gxTv_SdtSDTTwoLevels_Name = "";

			gxTv_SdtSDTTwoLevels_Level_N = true;

			return  ;
		}



		#endregion

		#region Declaration

		protected string gxTv_SdtSDTTwoLevels_Name;
		 
		protected bool gxTv_SdtSDTTwoLevels_Level_N;
		protected GXBaseCollection<SdtSDTTwoLevels_LevelItem> gxTv_SdtSDTTwoLevels_Level = null; 



		#endregion
	}
	#region Rest interface
	[GxUnWrappedJson()]
	[DataContract(Name=@"SDTTwoLevels", Namespace="MassiveSearch")]
	public class SdtSDTTwoLevels_RESTInterface : GxGenericCollectionItem<SdtSDTTwoLevels>, System.Web.SessionState.IRequiresSessionState
	{
		public SdtSDTTwoLevels_RESTInterface( ) : base()
		{	
		}

		public SdtSDTTwoLevels_RESTInterface( SdtSDTTwoLevels psdt ) : base(psdt)
		{	
		}

		#region Rest Properties
		[DataMember(Name="Name", Order=0)]
		public  string gxTpr_Name
		{
			get { 
				return StringUtil.RTrim( sdt.gxTpr_Name);

			}
			set { 
				 sdt.gxTpr_Name = value;
			}
		}

		[DataMember(Name="Level", Order=1, EmitDefaultValue=false)]
		public GxGenericCollection<SdtSDTTwoLevels_LevelItem_RESTInterface> gxTpr_Level
		{
			get {
				if (sdt.ShouldSerializegxTpr_Level_GxSimpleCollection_Json())
					return new GxGenericCollection<SdtSDTTwoLevels_LevelItem_RESTInterface>(sdt.gxTpr_Level);
				else
					return null;

			}
			set {
				value.LoadCollection(sdt.gxTpr_Level);
			}
		}


		#endregion

		public SdtSDTTwoLevels sdt
		{
			get { 
				return (SdtSDTTwoLevels)Sdt;
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
				sdt = new SdtSDTTwoLevels() ;
			}
		}
	}
	#endregion
}