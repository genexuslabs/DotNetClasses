using System;
using System.Collections;
using GeneXus.Utils;
using GeneXus.Resources;
using GeneXus.Application;
using GeneXus.Metadata;
using GeneXus.Cryptography;
using GeneXus.Encryption;
using GeneXus.Http.Client;
using System.Reflection;
using System.Xml.Serialization;
using System.Runtime.Serialization;
namespace GeneXus.Programs.apps
{
   [XmlRoot(ElementName = "Invoice.Level" )]
   [XmlType(TypeName =  "Invoice.Level" , Namespace = "TestRestProcs" )]
   [Serializable]
   public class SdtInvoice_Level : GxSilentTrnSdt, IGxSilentTrnGridItem
   {
      public SdtInvoice_Level( )
      {
      }

      public SdtInvoice_Level( IGxContext context )
      {
         this.context = context;
         constructorCallingAssembly = Assembly.GetEntryAssembly();
         initialize();
      }

      private static Hashtable mapper;
      public override string JsonMap( string value )
      {
         if ( mapper == null )
         {
            mapper = new Hashtable();
         }
         return (string)mapper[value]; ;
      }

      public override Object[][] GetBCKey( )
      {
         return (Object[][])(new Object[][]{new Object[]{"InvoiceLevelId", typeof(short)}}) ;
      }

      public override GXProperties GetMetadata( )
      {
         GXProperties metadata = new GXProperties();
         metadata.Set("Name", "Level");
         metadata.Set("BT", "InvoiceLevel");
         metadata.Set("PK", "[ \"InvoiceLevelId\" ]");
         metadata.Set("PKAssigned", "[ \"InvoiceLevelId\" ]");
         metadata.Set("FKList", "[ { \"FK\":[ \"InvoiceId\" ],\"FKMap\":[  ] },{ \"FK\":[ \"ProductId\" ],\"FKMap\":[  ] } ]");
         metadata.Set("AllowInsert", "True");
         metadata.Set("AllowUpdate", "True");
         metadata.Set("AllowDelete", "True");
         return metadata ;
      }

      public override GeneXus.Utils.GxStringCollection StateAttributes( )
      {
         GeneXus.Utils.GxStringCollection state = new GeneXus.Utils.GxStringCollection();
         state.Add("gxTpr_Mode");
         state.Add("gxTpr_Modified");
         state.Add("gxTpr_Initialized");
         state.Add("gxTpr_Invoicelevelid_Z");
         state.Add("gxTpr_Productid_Z");
         state.Add("gxTpr_Invoicelevelqty_Z");
         state.Add("gxTpr_Productprice_Z");
         state.Add("gxTpr_Invoicelevelsubtot_Z");
         return state ;
      }

      public override void Copy( GxUserType source )
      {
         SdtInvoice_Level sdt;
         sdt = (SdtInvoice_Level)(source);
         gxTv_SdtInvoice_Level_Invoicelevelid = sdt.gxTv_SdtInvoice_Level_Invoicelevelid ;
         gxTv_SdtInvoice_Level_Productid = sdt.gxTv_SdtInvoice_Level_Productid ;
         gxTv_SdtInvoice_Level_Invoicelevelqty = sdt.gxTv_SdtInvoice_Level_Invoicelevelqty ;
         gxTv_SdtInvoice_Level_Productprice = sdt.gxTv_SdtInvoice_Level_Productprice ;
         gxTv_SdtInvoice_Level_Invoicelevelsubtot = sdt.gxTv_SdtInvoice_Level_Invoicelevelsubtot ;
         gxTv_SdtInvoice_Level_Mode = sdt.gxTv_SdtInvoice_Level_Mode ;
         gxTv_SdtInvoice_Level_Modified = sdt.gxTv_SdtInvoice_Level_Modified ;
         gxTv_SdtInvoice_Level_Initialized = sdt.gxTv_SdtInvoice_Level_Initialized ;
         gxTv_SdtInvoice_Level_Invoicelevelid_Z = sdt.gxTv_SdtInvoice_Level_Invoicelevelid_Z ;
         gxTv_SdtInvoice_Level_Productid_Z = sdt.gxTv_SdtInvoice_Level_Productid_Z ;
         gxTv_SdtInvoice_Level_Invoicelevelqty_Z = sdt.gxTv_SdtInvoice_Level_Invoicelevelqty_Z ;
         gxTv_SdtInvoice_Level_Productprice_Z = sdt.gxTv_SdtInvoice_Level_Productprice_Z ;
         gxTv_SdtInvoice_Level_Invoicelevelsubtot_Z = sdt.gxTv_SdtInvoice_Level_Invoicelevelsubtot_Z ;
         return  ;
      }

      public override void ToJSON( )
      {
         ToJSON( true) ;
         return  ;
      }

      public override void ToJSON( bool includeState )
      {
         ToJSON( includeState, true) ;
         return  ;
      }

      public override void ToJSON( bool includeState ,
                                   bool includeNonInitialized )
      {
         AddObjectProperty("InvoiceLevelId", gxTv_SdtInvoice_Level_Invoicelevelid, false, includeNonInitialized);
         AddObjectProperty("ProductId", gxTv_SdtInvoice_Level_Productid, false, includeNonInitialized);
         AddObjectProperty("InvoiceLevelQty", gxTv_SdtInvoice_Level_Invoicelevelqty, false, includeNonInitialized);
         AddObjectProperty("ProductPrice", gxTv_SdtInvoice_Level_Productprice, false, includeNonInitialized);
         AddObjectProperty("InvoiceLevelSubTot", gxTv_SdtInvoice_Level_Invoicelevelsubtot, false, includeNonInitialized);
         if ( includeState )
         {
            AddObjectProperty("Mode", gxTv_SdtInvoice_Level_Mode, false, includeNonInitialized);
            AddObjectProperty("Modified", gxTv_SdtInvoice_Level_Modified, false, includeNonInitialized);
            AddObjectProperty("Initialized", gxTv_SdtInvoice_Level_Initialized, false, includeNonInitialized);
            AddObjectProperty("InvoiceLevelId_Z", gxTv_SdtInvoice_Level_Invoicelevelid_Z, false, includeNonInitialized);
            AddObjectProperty("ProductId_Z", gxTv_SdtInvoice_Level_Productid_Z, false, includeNonInitialized);
            AddObjectProperty("InvoiceLevelQty_Z", gxTv_SdtInvoice_Level_Invoicelevelqty_Z, false, includeNonInitialized);
            AddObjectProperty("ProductPrice_Z", gxTv_SdtInvoice_Level_Productprice_Z, false, includeNonInitialized);
            AddObjectProperty("InvoiceLevelSubTot_Z", gxTv_SdtInvoice_Level_Invoicelevelsubtot_Z, false, includeNonInitialized);
         }
         return  ;
      }

      public void UpdateDirties( SdtInvoice_Level sdt )
      {
         if ( sdt.IsDirty("InvoiceLevelId") )
         {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Invoicelevelid = sdt.gxTv_SdtInvoice_Level_Invoicelevelid ;
         }
         if ( sdt.IsDirty("ProductId") )
         {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Productid = sdt.gxTv_SdtInvoice_Level_Productid ;
         }
         if ( sdt.IsDirty("InvoiceLevelQty") )
         {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Invoicelevelqty = sdt.gxTv_SdtInvoice_Level_Invoicelevelqty ;
         }
         if ( sdt.IsDirty("ProductPrice") )
         {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Productprice = sdt.gxTv_SdtInvoice_Level_Productprice ;
         }
         if ( sdt.IsDirty("InvoiceLevelSubTot") )
         {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Invoicelevelsubtot = sdt.gxTv_SdtInvoice_Level_Invoicelevelsubtot ;
         }
         return  ;
      }

      [  SoapElement( ElementName = "InvoiceLevelId" )]
      [  XmlElement( ElementName = "InvoiceLevelId"   )]
      public short gxTpr_Invoicelevelid
      {
         get {
            return gxTv_SdtInvoice_Level_Invoicelevelid ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Invoicelevelid = value;
            gxTv_SdtInvoice_Level_Modified = 1;
            SetDirty("Invoicelevelid");
         }

      }

      [  SoapElement( ElementName = "ProductId" )]
      [  XmlElement( ElementName = "ProductId"   )]
      public short gxTpr_Productid
      {
         get {
            return gxTv_SdtInvoice_Level_Productid ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Productid = value;
            gxTv_SdtInvoice_Level_Modified = 1;
            SetDirty("Productid");
         }

      }

      [  SoapElement( ElementName = "InvoiceLevelQty" )]
      [  XmlElement( ElementName = "InvoiceLevelQty"   )]
      public short gxTpr_Invoicelevelqty
      {
         get {
            return gxTv_SdtInvoice_Level_Invoicelevelqty ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Invoicelevelqty = value;
            gxTv_SdtInvoice_Level_Modified = 1;
            SetDirty("Invoicelevelqty");
         }

      }

      [  SoapElement( ElementName = "ProductPrice" )]
      [  XmlElement( ElementName = "ProductPrice"   )]
      public decimal gxTpr_Productprice
      {
         get {
            return gxTv_SdtInvoice_Level_Productprice ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Productprice = value;
            gxTv_SdtInvoice_Level_Modified = 1;
            SetDirty("Productprice");
         }

      }

      [  SoapElement( ElementName = "InvoiceLevelSubTot" )]
      [  XmlElement( ElementName = "InvoiceLevelSubTot"   )]
      public decimal gxTpr_Invoicelevelsubtot
      {
         get {
            return gxTv_SdtInvoice_Level_Invoicelevelsubtot ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Invoicelevelsubtot = value;
            gxTv_SdtInvoice_Level_Modified = 1;
            SetDirty("Invoicelevelsubtot");
         }

      }

      public void gxTv_SdtInvoice_Level_Invoicelevelsubtot_SetNull( )
      {
         gxTv_SdtInvoice_Level_Invoicelevelsubtot = 0;
         SetDirty("Invoicelevelsubtot");
         return  ;
      }

      public bool gxTv_SdtInvoice_Level_Invoicelevelsubtot_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "Mode" )]
      [  XmlElement( ElementName = "Mode"   )]
      public string gxTpr_Mode
      {
         get {
            return gxTv_SdtInvoice_Level_Mode ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Mode = value;
            SetDirty("Mode");
         }

      }

      public void gxTv_SdtInvoice_Level_Mode_SetNull( )
      {
         gxTv_SdtInvoice_Level_Mode = "";
         SetDirty("Mode");
         return  ;
      }

      public bool gxTv_SdtInvoice_Level_Mode_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "Modified" )]
      [  XmlElement( ElementName = "Modified"   )]
      public short gxTpr_Modified
      {
         get {
            return gxTv_SdtInvoice_Level_Modified ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Modified = value;
            SetDirty("Modified");
         }

      }

      public void gxTv_SdtInvoice_Level_Modified_SetNull( )
      {
         gxTv_SdtInvoice_Level_Modified = 0;
         SetDirty("Modified");
         return  ;
      }

      public bool gxTv_SdtInvoice_Level_Modified_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "Initialized" )]
      [  XmlElement( ElementName = "Initialized"   )]
      public short gxTpr_Initialized
      {
         get {
            return gxTv_SdtInvoice_Level_Initialized ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Initialized = value;
            gxTv_SdtInvoice_Level_Modified = 1;
            SetDirty("Initialized");
         }

      }

      public void gxTv_SdtInvoice_Level_Initialized_SetNull( )
      {
         gxTv_SdtInvoice_Level_Initialized = 0;
         SetDirty("Initialized");
         return  ;
      }

      public bool gxTv_SdtInvoice_Level_Initialized_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "InvoiceLevelId_Z" )]
      [  XmlElement( ElementName = "InvoiceLevelId_Z"   )]
      public short gxTpr_Invoicelevelid_Z
      {
         get {
            return gxTv_SdtInvoice_Level_Invoicelevelid_Z ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Invoicelevelid_Z = value;
            gxTv_SdtInvoice_Level_Modified = 1;
            SetDirty("Invoicelevelid_Z");
         }

      }

      public void gxTv_SdtInvoice_Level_Invoicelevelid_Z_SetNull( )
      {
         gxTv_SdtInvoice_Level_Invoicelevelid_Z = 0;
         SetDirty("Invoicelevelid_Z");
         return  ;
      }

      public bool gxTv_SdtInvoice_Level_Invoicelevelid_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "ProductId_Z" )]
      [  XmlElement( ElementName = "ProductId_Z"   )]
      public short gxTpr_Productid_Z
      {
         get {
            return gxTv_SdtInvoice_Level_Productid_Z ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Productid_Z = value;
            gxTv_SdtInvoice_Level_Modified = 1;
            SetDirty("Productid_Z");
         }

      }

      public void gxTv_SdtInvoice_Level_Productid_Z_SetNull( )
      {
         gxTv_SdtInvoice_Level_Productid_Z = 0;
         SetDirty("Productid_Z");
         return  ;
      }

      public bool gxTv_SdtInvoice_Level_Productid_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "InvoiceLevelQty_Z" )]
      [  XmlElement( ElementName = "InvoiceLevelQty_Z"   )]
      public short gxTpr_Invoicelevelqty_Z
      {
         get {
            return gxTv_SdtInvoice_Level_Invoicelevelqty_Z ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Invoicelevelqty_Z = value;
            gxTv_SdtInvoice_Level_Modified = 1;
            SetDirty("Invoicelevelqty_Z");
         }

      }

      public void gxTv_SdtInvoice_Level_Invoicelevelqty_Z_SetNull( )
      {
         gxTv_SdtInvoice_Level_Invoicelevelqty_Z = 0;
         SetDirty("Invoicelevelqty_Z");
         return  ;
      }

      public bool gxTv_SdtInvoice_Level_Invoicelevelqty_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "ProductPrice_Z" )]
      [  XmlElement( ElementName = "ProductPrice_Z"   )]
      public decimal gxTpr_Productprice_Z
      {
         get {
            return gxTv_SdtInvoice_Level_Productprice_Z ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Productprice_Z = value;
            gxTv_SdtInvoice_Level_Modified = 1;
            SetDirty("Productprice_Z");
         }

      }

      public void gxTv_SdtInvoice_Level_Productprice_Z_SetNull( )
      {
         gxTv_SdtInvoice_Level_Productprice_Z = 0;
         SetDirty("Productprice_Z");
         return  ;
      }

      public bool gxTv_SdtInvoice_Level_Productprice_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "InvoiceLevelSubTot_Z" )]
      [  XmlElement( ElementName = "InvoiceLevelSubTot_Z"   )]
      public decimal gxTpr_Invoicelevelsubtot_Z
      {
         get {
            return gxTv_SdtInvoice_Level_Invoicelevelsubtot_Z ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level_Invoicelevelsubtot_Z = value;
            gxTv_SdtInvoice_Level_Modified = 1;
            SetDirty("Invoicelevelsubtot_Z");
         }

      }

      public void gxTv_SdtInvoice_Level_Invoicelevelsubtot_Z_SetNull( )
      {
         gxTv_SdtInvoice_Level_Invoicelevelsubtot_Z = 0;
         SetDirty("Invoicelevelsubtot_Z");
         return  ;
      }

      public bool gxTv_SdtInvoice_Level_Invoicelevelsubtot_Z_IsNull( )
      {
         return false ;
      }

      [XmlIgnore]
      private static GXTypeInfo _typeProps;
      protected override GXTypeInfo TypeInfo
      {
         get {
            return _typeProps ;
         }

         set {
            _typeProps = value ;
         }

      }

      public void initialize( )
      {
         sdtIsNull = 1;
         gxTv_SdtInvoice_Level_Mode = "";
         return  ;
      }

      public short isNull( )
      {
         return sdtIsNull ;
      }

      private short gxTv_SdtInvoice_Level_Invoicelevelid ;
      private short sdtIsNull ;
      private short gxTv_SdtInvoice_Level_Productid ;
      private short gxTv_SdtInvoice_Level_Invoicelevelqty ;
      private short gxTv_SdtInvoice_Level_Modified ;
      private short gxTv_SdtInvoice_Level_Initialized ;
      private short gxTv_SdtInvoice_Level_Invoicelevelid_Z ;
      private short gxTv_SdtInvoice_Level_Productid_Z ;
      private short gxTv_SdtInvoice_Level_Invoicelevelqty_Z ;
      private decimal gxTv_SdtInvoice_Level_Productprice ;
      private decimal gxTv_SdtInvoice_Level_Invoicelevelsubtot ;
      private decimal gxTv_SdtInvoice_Level_Productprice_Z ;
      private decimal gxTv_SdtInvoice_Level_Invoicelevelsubtot_Z ;
      private string gxTv_SdtInvoice_Level_Mode ;
   }

   [DataContract(Name = @"Invoice.Level", Namespace = "TestRestProcs")]
   [GxJsonSerialization("default")]
   public class SdtInvoice_Level_RESTInterface : GxGenericCollectionItem<SdtInvoice_Level>
   {
      public SdtInvoice_Level_RESTInterface( ) : base()
      {
      }

      public SdtInvoice_Level_RESTInterface( SdtInvoice_Level psdt ) : base(psdt)
      {
      }

      [DataMember( Name = "InvoiceLevelId" , Order = 0 )]
      [GxSeudo()]
      public Nullable<short> gxTpr_Invoicelevelid
      {
         get {
            return sdt.gxTpr_Invoicelevelid ;
         }

         set {
            sdt.gxTpr_Invoicelevelid = (short)(value.HasValue ? value.Value : 0);
         }

      }

      [DataMember( Name = "ProductId" , Order = 1 )]
      [GxSeudo()]
      public Nullable<short> gxTpr_Productid
      {
         get {
            return sdt.gxTpr_Productid ;
         }

         set {
            sdt.gxTpr_Productid = (short)(value.HasValue ? value.Value : 0);
         }

      }

      [DataMember( Name = "InvoiceLevelQty" , Order = 2 )]
      [GxSeudo()]
      public Nullable<short> gxTpr_Invoicelevelqty
      {
         get {
            return sdt.gxTpr_Invoicelevelqty ;
         }

         set {
            sdt.gxTpr_Invoicelevelqty = (short)(value.HasValue ? value.Value : 0);
         }

      }

      [DataMember( Name = "ProductPrice" , Order = 3 )]
      [GxSeudo()]
      public Nullable<decimal> gxTpr_Productprice
      {
         get {
            return sdt.gxTpr_Productprice ;
         }

         set {
            sdt.gxTpr_Productprice = (decimal)(value.HasValue ? value.Value : 0);
         }

      }

      [DataMember( Name = "InvoiceLevelSubTot" , Order = 4 )]
      [GxSeudo()]
      public Nullable<decimal> gxTpr_Invoicelevelsubtot
      {
         get {
            return sdt.gxTpr_Invoicelevelsubtot ;
         }

         set {
            sdt.gxTpr_Invoicelevelsubtot = (decimal)(value.HasValue ? value.Value : 0);
         }

      }

      public SdtInvoice_Level sdt
      {
         get {
            return (SdtInvoice_Level)Sdt ;
         }

         set {
            Sdt = value ;
         }

      }

      [OnDeserializing]
      void checkSdt( StreamingContext ctx )
      {
         if ( sdt == null )
         {
            sdt = new SdtInvoice_Level() ;
         }
      }

   }

   [DataContract(Name = @"Invoice.Level", Namespace = "TestRestProcs")]
   [GxJsonSerialization("default")]
   public class SdtInvoice_Level_RESTLInterface : GxGenericCollectionItem<SdtInvoice_Level>
   {
      public SdtInvoice_Level_RESTLInterface( ) : base()
      {
      }

      public SdtInvoice_Level_RESTLInterface( SdtInvoice_Level psdt ) : base(psdt)
      {
      }

      public SdtInvoice_Level sdt
      {
         get {
            return (SdtInvoice_Level)Sdt ;
         }

         set {
            Sdt = value ;
         }

      }

      [OnDeserializing]
      void checkSdt( StreamingContext ctx )
      {
         if ( sdt == null )
         {
            sdt = new SdtInvoice_Level() ;
         }
      }

   }

}
