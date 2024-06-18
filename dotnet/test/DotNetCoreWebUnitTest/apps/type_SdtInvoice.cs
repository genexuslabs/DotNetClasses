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
   [XmlRoot(ElementName = "Invoice" )]
   [XmlType(TypeName =  "Invoice" , Namespace = "TestRestProcs" )]
   [Serializable]
   public class SdtInvoice : GxSilentTrnSdt
   {
      public SdtInvoice( )
      {
      }

      public SdtInvoice( IGxContext context )
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

      public void Load( short AV5InvoiceId )
      {
         IGxSilentTrn obj;
         obj = getTransaction();
         obj.LoadKey(new Object[] {(short)AV5InvoiceId});
         return  ;
      }

      public void LoadStrParms( string sAV5InvoiceId )
      {
         short AV5InvoiceId;
         AV5InvoiceId = (short)(Math.Round(NumberUtil.Val( sAV5InvoiceId, "."), 18, MidpointRounding.ToEven));
         Load( AV5InvoiceId) ;
         return  ;
      }

      public override Object[][] GetBCKey( )
      {
         return (Object[][])(new Object[][]{new Object[]{"InvoiceId", typeof(short)}}) ;
      }

      public override GXProperties GetMetadata( )
      {
         GXProperties metadata = new GXProperties();
         metadata.Set("Name", "Invoice");
         metadata.Set("BT", "Invoice");
         metadata.Set("PK", "[ \"InvoiceId\" ]");
         metadata.Set("Levels", "[ \"Level\" ]");
         metadata.Set("Serial", "[ [ \"Same\",\"Invoice\",\"InvoiceLast\",\"InvoiceLevelId\",\"InvoiceId\",\"InvoiceId\" ] ]");
         metadata.Set("FKList", "[ { \"FK\":[ \"CustomerId\" ],\"FKMap\":[  ] } ]");
         metadata.Set("AllowInsert", "True");
         metadata.Set("AllowUpdate", "True");
         metadata.Set("AllowDelete", "True");
         return metadata ;
      }

      public override GeneXus.Utils.GxStringCollection StateAttributes( )
      {
         GeneXus.Utils.GxStringCollection state = new GeneXus.Utils.GxStringCollection();
         state.Add("gxTpr_Mode");
         state.Add("gxTpr_Initialized");
         state.Add("gxTpr_Invoiceid_Z");
         state.Add("gxTpr_Invoicedate_Z_Nullable");
         state.Add("gxTpr_Customerid_Z");
         state.Add("gxTpr_Customername_Z");
         state.Add("gxTpr_Invoicetotal_Z");
         state.Add("gxTpr_Invoicelast_Z");
         state.Add("gxTpr_Invoicetotal_N");
         return state ;
      }

      public override void Copy( GxUserType source )
      {
         SdtInvoice sdt;
         sdt = (SdtInvoice)(source);
         gxTv_SdtInvoice_Invoiceid = sdt.gxTv_SdtInvoice_Invoiceid ;
         gxTv_SdtInvoice_Invoicedate = sdt.gxTv_SdtInvoice_Invoicedate ;
         gxTv_SdtInvoice_Customerid = sdt.gxTv_SdtInvoice_Customerid ;
         gxTv_SdtInvoice_Customername = sdt.gxTv_SdtInvoice_Customername ;
         gxTv_SdtInvoice_Invoicetotal = sdt.gxTv_SdtInvoice_Invoicetotal ;
         gxTv_SdtInvoice_Invoicelast = sdt.gxTv_SdtInvoice_Invoicelast ;
         gxTv_SdtInvoice_Level = sdt.gxTv_SdtInvoice_Level ;
         gxTv_SdtInvoice_Mode = sdt.gxTv_SdtInvoice_Mode ;
         gxTv_SdtInvoice_Initialized = sdt.gxTv_SdtInvoice_Initialized ;
         gxTv_SdtInvoice_Invoiceid_Z = sdt.gxTv_SdtInvoice_Invoiceid_Z ;
         gxTv_SdtInvoice_Invoicedate_Z = sdt.gxTv_SdtInvoice_Invoicedate_Z ;
         gxTv_SdtInvoice_Customerid_Z = sdt.gxTv_SdtInvoice_Customerid_Z ;
         gxTv_SdtInvoice_Customername_Z = sdt.gxTv_SdtInvoice_Customername_Z ;
         gxTv_SdtInvoice_Invoicetotal_Z = sdt.gxTv_SdtInvoice_Invoicetotal_Z ;
         gxTv_SdtInvoice_Invoicelast_Z = sdt.gxTv_SdtInvoice_Invoicelast_Z ;
         gxTv_SdtInvoice_Invoicetotal_N = sdt.gxTv_SdtInvoice_Invoicetotal_N ;
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
         AddObjectProperty("InvoiceId", gxTv_SdtInvoice_Invoiceid, false, includeNonInitialized);
         sDateCnv = "";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Year( gxTv_SdtInvoice_Invoicedate)), 10, 0));
         sDateCnv += StringUtil.Substring( "0000", 1, 4-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "-";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Month( gxTv_SdtInvoice_Invoicedate)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "-";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Day( gxTv_SdtInvoice_Invoicedate)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         AddObjectProperty("InvoiceDate", sDateCnv, false, includeNonInitialized);
         AddObjectProperty("CustomerId", gxTv_SdtInvoice_Customerid, false, includeNonInitialized);
         AddObjectProperty("CustomerName", gxTv_SdtInvoice_Customername, false, includeNonInitialized);
         AddObjectProperty("InvoiceTotal", gxTv_SdtInvoice_Invoicetotal, false, includeNonInitialized);
         AddObjectProperty("InvoiceTotal_N", gxTv_SdtInvoice_Invoicetotal_N, false, includeNonInitialized);
         AddObjectProperty("InvoiceLast", gxTv_SdtInvoice_Invoicelast, false, includeNonInitialized);
         if ( gxTv_SdtInvoice_Level != null )
         {
            AddObjectProperty("Level", gxTv_SdtInvoice_Level, includeState, includeNonInitialized);
         }
         if ( includeState )
         {
            AddObjectProperty("Mode", gxTv_SdtInvoice_Mode, false, includeNonInitialized);
            AddObjectProperty("Initialized", gxTv_SdtInvoice_Initialized, false, includeNonInitialized);
            AddObjectProperty("InvoiceId_Z", gxTv_SdtInvoice_Invoiceid_Z, false, includeNonInitialized);
            sDateCnv = "";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Year( gxTv_SdtInvoice_Invoicedate_Z)), 10, 0));
            sDateCnv += StringUtil.Substring( "0000", 1, 4-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "-";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Month( gxTv_SdtInvoice_Invoicedate_Z)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "-";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Day( gxTv_SdtInvoice_Invoicedate_Z)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            AddObjectProperty("InvoiceDate_Z", sDateCnv, false, includeNonInitialized);
            AddObjectProperty("CustomerId_Z", gxTv_SdtInvoice_Customerid_Z, false, includeNonInitialized);
            AddObjectProperty("CustomerName_Z", gxTv_SdtInvoice_Customername_Z, false, includeNonInitialized);
            AddObjectProperty("InvoiceTotal_Z", gxTv_SdtInvoice_Invoicetotal_Z, false, includeNonInitialized);
            AddObjectProperty("InvoiceLast_Z", gxTv_SdtInvoice_Invoicelast_Z, false, includeNonInitialized);
            AddObjectProperty("InvoiceTotal_N", gxTv_SdtInvoice_Invoicetotal_N, false, includeNonInitialized);
         }
         return  ;
      }

      public void UpdateDirties( SdtInvoice sdt )
      {
         if ( sdt.IsDirty("InvoiceId") )
         {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Invoiceid = sdt.gxTv_SdtInvoice_Invoiceid ;
         }
         if ( sdt.IsDirty("InvoiceDate") )
         {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Invoicedate = sdt.gxTv_SdtInvoice_Invoicedate ;
         }
         if ( sdt.IsDirty("CustomerId") )
         {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Customerid = sdt.gxTv_SdtInvoice_Customerid ;
         }
         if ( sdt.IsDirty("CustomerName") )
         {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Customername = sdt.gxTv_SdtInvoice_Customername ;
         }
         if ( sdt.IsDirty("InvoiceTotal") )
         {
            gxTv_SdtInvoice_Invoicetotal_N = (short)(sdt.gxTv_SdtInvoice_Invoicetotal_N);
            sdtIsNull = 0;
            gxTv_SdtInvoice_Invoicetotal = sdt.gxTv_SdtInvoice_Invoicetotal ;
         }
         if ( sdt.IsDirty("InvoiceLast") )
         {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Invoicelast = sdt.gxTv_SdtInvoice_Invoicelast ;
         }
         if ( gxTv_SdtInvoice_Level != null )
         {
            GXBCLevelCollection<SdtInvoice_Level> newCollectionLevel = sdt.gxTpr_Level;
            SdtInvoice_Level currItemLevel;
            SdtInvoice_Level newItemLevel;
            short idx = 1;
            while ( idx <= newCollectionLevel.Count )
            {
               newItemLevel = ((SdtInvoice_Level)newCollectionLevel.Item(idx));
               currItemLevel = gxTv_SdtInvoice_Level.GetByKey(newItemLevel.gxTpr_Invoicelevelid);
               if ( StringUtil.StrCmp(currItemLevel.gxTpr_Mode, "UPD") == 0 )
               {
                  currItemLevel.UpdateDirties(newItemLevel);
                  if ( StringUtil.StrCmp(newItemLevel.gxTpr_Mode, "DLT") == 0 )
                  {
                     currItemLevel.gxTpr_Mode = "DLT";
                  }
                  currItemLevel.gxTpr_Modified = 1;
               }
               else
               {
                  gxTv_SdtInvoice_Level.Add(newItemLevel, 0);
               }
               idx = (short)(idx+1);
            }
         }
         return  ;
      }

      [  SoapElement( ElementName = "InvoiceId" )]
      [  XmlElement( ElementName = "InvoiceId"   )]
      public short gxTpr_Invoiceid
      {
         get {
            return gxTv_SdtInvoice_Invoiceid ;
         }

         set {
            sdtIsNull = 0;
            if ( gxTv_SdtInvoice_Invoiceid != value )
            {
               gxTv_SdtInvoice_Mode = "INS";
               this.gxTv_SdtInvoice_Invoiceid_Z_SetNull( );
               this.gxTv_SdtInvoice_Invoicedate_Z_SetNull( );
               this.gxTv_SdtInvoice_Customerid_Z_SetNull( );
               this.gxTv_SdtInvoice_Customername_Z_SetNull( );
               this.gxTv_SdtInvoice_Invoicetotal_Z_SetNull( );
               this.gxTv_SdtInvoice_Invoicelast_Z_SetNull( );
               if ( gxTv_SdtInvoice_Level != null )
               {
                  GXBCLevelCollection<SdtInvoice_Level> collectionLevel = gxTv_SdtInvoice_Level;
                  SdtInvoice_Level currItemLevel;
                  short idx = 1;
                  while ( idx <= collectionLevel.Count )
                  {
                     currItemLevel = ((SdtInvoice_Level)collectionLevel.Item(idx));
                     currItemLevel.gxTpr_Mode = "INS";
                     currItemLevel.gxTpr_Modified = 1;
                     idx = (short)(idx+1);
                  }
               }
            }
            gxTv_SdtInvoice_Invoiceid = value;
            SetDirty("Invoiceid");
         }

      }

      [  SoapElement( ElementName = "InvoiceDate" )]
      [  XmlElement( ElementName = "InvoiceDate"  , IsNullable=true )]
      public string gxTpr_Invoicedate_Nullable
      {
         get {
            if ( gxTv_SdtInvoice_Invoicedate == DateTime.MinValue)
               return null;
            return new GxDateString(gxTv_SdtInvoice_Invoicedate).value ;
         }

         set {
            sdtIsNull = 0;
            if (String.IsNullOrEmpty(value) || value == GxDateString.NullValue )
               gxTv_SdtInvoice_Invoicedate = DateTime.MinValue;
            else
               gxTv_SdtInvoice_Invoicedate = DateTime.Parse( value);
         }

      }

      [XmlIgnore]
      public DateTime gxTpr_Invoicedate
      {
         get {
            return gxTv_SdtInvoice_Invoicedate ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Invoicedate = value;
            SetDirty("Invoicedate");
         }

      }

      [  SoapElement( ElementName = "CustomerId" )]
      [  XmlElement( ElementName = "CustomerId"   )]
      public short gxTpr_Customerid
      {
         get {
            return gxTv_SdtInvoice_Customerid ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Customerid = value;
            SetDirty("Customerid");
         }

      }

      [  SoapElement( ElementName = "CustomerName" )]
      [  XmlElement( ElementName = "CustomerName"   )]
      public string gxTpr_Customername
      {
         get {
            return gxTv_SdtInvoice_Customername ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Customername = value;
            SetDirty("Customername");
         }

      }

      [  SoapElement( ElementName = "InvoiceTotal" )]
      [  XmlElement( ElementName = "InvoiceTotal"   )]
      public short gxTpr_Invoicetotal
      {
         get {
            return gxTv_SdtInvoice_Invoicetotal ;
         }

         set {
            gxTv_SdtInvoice_Invoicetotal_N = 0;
            sdtIsNull = 0;
            gxTv_SdtInvoice_Invoicetotal = value;
            SetDirty("Invoicetotal");
         }

      }

      public void gxTv_SdtInvoice_Invoicetotal_SetNull( )
      {
         gxTv_SdtInvoice_Invoicetotal_N = 1;
         gxTv_SdtInvoice_Invoicetotal = 0;
         SetDirty("Invoicetotal");
         return  ;
      }

      public bool gxTv_SdtInvoice_Invoicetotal_IsNull( )
      {
         return (gxTv_SdtInvoice_Invoicetotal_N==1) ;
      }

      [  SoapElement( ElementName = "InvoiceLast" )]
      [  XmlElement( ElementName = "InvoiceLast"   )]
      public short gxTpr_Invoicelast
      {
         get {
            return gxTv_SdtInvoice_Invoicelast ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Invoicelast = value;
            SetDirty("Invoicelast");
         }

      }

      [  SoapElement( ElementName = "Level" )]
      [  XmlArray( ElementName = "Level"  )]
      [  XmlArrayItemAttribute( ElementName= "Invoice.Level"  , IsNullable=false)]
      public GXBCLevelCollection<SdtInvoice_Level> gxTpr_Level_GXBCLevelCollection
      {
         get {
            if ( gxTv_SdtInvoice_Level == null )
            {
               gxTv_SdtInvoice_Level = new GXBCLevelCollection<SdtInvoice_Level>( context, "Invoice.Level", "TestRestProcs");
            }
            return gxTv_SdtInvoice_Level ;
         }

         set {
            if ( gxTv_SdtInvoice_Level == null )
            {
               gxTv_SdtInvoice_Level = new GXBCLevelCollection<SdtInvoice_Level>( context, "Invoice.Level", "TestRestProcs");
            }
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level = value;
         }

      }

      [XmlIgnore]
      public GXBCLevelCollection<SdtInvoice_Level> gxTpr_Level
      {
         get {
            if ( gxTv_SdtInvoice_Level == null )
            {
               gxTv_SdtInvoice_Level = new GXBCLevelCollection<SdtInvoice_Level>( context, "Invoice.Level", "TestRestProcs");
            }
            sdtIsNull = 0;
            return gxTv_SdtInvoice_Level ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Level = value;
            SetDirty("Level");
         }

      }

      public void gxTv_SdtInvoice_Level_SetNull( )
      {
         gxTv_SdtInvoice_Level = null;
         SetDirty("Level");
         return  ;
      }

      public bool gxTv_SdtInvoice_Level_IsNull( )
      {
         if ( gxTv_SdtInvoice_Level == null )
         {
            return true ;
         }
         return false ;
      }

      [  SoapElement( ElementName = "Mode" )]
      [  XmlElement( ElementName = "Mode"   )]
      public string gxTpr_Mode
      {
         get {
            return gxTv_SdtInvoice_Mode ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Mode = value;
            SetDirty("Mode");
         }

      }

      public void gxTv_SdtInvoice_Mode_SetNull( )
      {
         gxTv_SdtInvoice_Mode = "";
         SetDirty("Mode");
         return  ;
      }

      public bool gxTv_SdtInvoice_Mode_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "Initialized" )]
      [  XmlElement( ElementName = "Initialized"   )]
      public short gxTpr_Initialized
      {
         get {
            return gxTv_SdtInvoice_Initialized ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Initialized = value;
            SetDirty("Initialized");
         }

      }

      public void gxTv_SdtInvoice_Initialized_SetNull( )
      {
         gxTv_SdtInvoice_Initialized = 0;
         SetDirty("Initialized");
         return  ;
      }

      public bool gxTv_SdtInvoice_Initialized_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "InvoiceId_Z" )]
      [  XmlElement( ElementName = "InvoiceId_Z"   )]
      public short gxTpr_Invoiceid_Z
      {
         get {
            return gxTv_SdtInvoice_Invoiceid_Z ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Invoiceid_Z = value;
            SetDirty("Invoiceid_Z");
         }

      }

      public void gxTv_SdtInvoice_Invoiceid_Z_SetNull( )
      {
         gxTv_SdtInvoice_Invoiceid_Z = 0;
         SetDirty("Invoiceid_Z");
         return  ;
      }

      public bool gxTv_SdtInvoice_Invoiceid_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "InvoiceDate_Z" )]
      [  XmlElement( ElementName = "InvoiceDate_Z"  , IsNullable=true )]
      public string gxTpr_Invoicedate_Z_Nullable
      {
         get {
            if ( gxTv_SdtInvoice_Invoicedate_Z == DateTime.MinValue)
               return null;
            return new GxDateString(gxTv_SdtInvoice_Invoicedate_Z).value ;
         }

         set {
            sdtIsNull = 0;
            if (String.IsNullOrEmpty(value) || value == GxDateString.NullValue )
               gxTv_SdtInvoice_Invoicedate_Z = DateTime.MinValue;
            else
               gxTv_SdtInvoice_Invoicedate_Z = DateTime.Parse( value);
         }

      }

      [XmlIgnore]
      public DateTime gxTpr_Invoicedate_Z
      {
         get {
            return gxTv_SdtInvoice_Invoicedate_Z ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Invoicedate_Z = value;
            SetDirty("Invoicedate_Z");
         }

      }

      public void gxTv_SdtInvoice_Invoicedate_Z_SetNull( )
      {
         gxTv_SdtInvoice_Invoicedate_Z = (DateTime)(DateTime.MinValue);
         SetDirty("Invoicedate_Z");
         return  ;
      }

      public bool gxTv_SdtInvoice_Invoicedate_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "CustomerId_Z" )]
      [  XmlElement( ElementName = "CustomerId_Z"   )]
      public short gxTpr_Customerid_Z
      {
         get {
            return gxTv_SdtInvoice_Customerid_Z ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Customerid_Z = value;
            SetDirty("Customerid_Z");
         }

      }

      public void gxTv_SdtInvoice_Customerid_Z_SetNull( )
      {
         gxTv_SdtInvoice_Customerid_Z = 0;
         SetDirty("Customerid_Z");
         return  ;
      }

      public bool gxTv_SdtInvoice_Customerid_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "CustomerName_Z" )]
      [  XmlElement( ElementName = "CustomerName_Z"   )]
      public string gxTpr_Customername_Z
      {
         get {
            return gxTv_SdtInvoice_Customername_Z ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Customername_Z = value;
            SetDirty("Customername_Z");
         }

      }

      public void gxTv_SdtInvoice_Customername_Z_SetNull( )
      {
         gxTv_SdtInvoice_Customername_Z = "";
         SetDirty("Customername_Z");
         return  ;
      }

      public bool gxTv_SdtInvoice_Customername_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "InvoiceTotal_Z" )]
      [  XmlElement( ElementName = "InvoiceTotal_Z"   )]
      public short gxTpr_Invoicetotal_Z
      {
         get {
            return gxTv_SdtInvoice_Invoicetotal_Z ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Invoicetotal_Z = value;
            SetDirty("Invoicetotal_Z");
         }

      }

      public void gxTv_SdtInvoice_Invoicetotal_Z_SetNull( )
      {
         gxTv_SdtInvoice_Invoicetotal_Z = 0;
         SetDirty("Invoicetotal_Z");
         return  ;
      }

      public bool gxTv_SdtInvoice_Invoicetotal_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "InvoiceLast_Z" )]
      [  XmlElement( ElementName = "InvoiceLast_Z"   )]
      public short gxTpr_Invoicelast_Z
      {
         get {
            return gxTv_SdtInvoice_Invoicelast_Z ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Invoicelast_Z = value;
            SetDirty("Invoicelast_Z");
         }

      }

      public void gxTv_SdtInvoice_Invoicelast_Z_SetNull( )
      {
         gxTv_SdtInvoice_Invoicelast_Z = 0;
         SetDirty("Invoicelast_Z");
         return  ;
      }

      public bool gxTv_SdtInvoice_Invoicelast_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "InvoiceTotal_N" )]
      [  XmlElement( ElementName = "InvoiceTotal_N"   )]
      public short gxTpr_Invoicetotal_N
      {
         get {
            return gxTv_SdtInvoice_Invoicetotal_N ;
         }

         set {
            sdtIsNull = 0;
            gxTv_SdtInvoice_Invoicetotal_N = value;
            SetDirty("Invoicetotal_N");
         }

      }

      public void gxTv_SdtInvoice_Invoicetotal_N_SetNull( )
      {
         gxTv_SdtInvoice_Invoicetotal_N = 0;
         SetDirty("Invoicetotal_N");
         return  ;
      }

      public bool gxTv_SdtInvoice_Invoicetotal_N_IsNull( )
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
         gxTv_SdtInvoice_Invoicedate = DateTime.MinValue;
         gxTv_SdtInvoice_Customername = "";
         gxTv_SdtInvoice_Mode = "";
         gxTv_SdtInvoice_Invoicedate_Z = DateTime.MinValue;
         gxTv_SdtInvoice_Customername_Z = "";
         sDateCnv = "";
         sNumToPad = "";
         return  ;
      }

      public short isNull( )
      {
         return sdtIsNull ;
      }

      private short gxTv_SdtInvoice_Invoiceid ;
      private short sdtIsNull ;
      private short gxTv_SdtInvoice_Customerid ;
      private short gxTv_SdtInvoice_Invoicetotal ;
      private short gxTv_SdtInvoice_Invoicelast ;
      private short gxTv_SdtInvoice_Initialized ;
      private short gxTv_SdtInvoice_Invoiceid_Z ;
      private short gxTv_SdtInvoice_Customerid_Z ;
      private short gxTv_SdtInvoice_Invoicetotal_Z ;
      private short gxTv_SdtInvoice_Invoicelast_Z ;
      private short gxTv_SdtInvoice_Invoicetotal_N ;
      private string gxTv_SdtInvoice_Customername ;
      private string gxTv_SdtInvoice_Mode ;
      private string gxTv_SdtInvoice_Customername_Z ;
      private string sDateCnv ;
      private string sNumToPad ;
      private DateTime gxTv_SdtInvoice_Invoicedate ;
      private DateTime gxTv_SdtInvoice_Invoicedate_Z ;
      private GXBCLevelCollection<SdtInvoice_Level> gxTv_SdtInvoice_Level=null ;
   }

   [DataContract(Name = @"Invoice", Namespace = "TestRestProcs")]
   [GxJsonSerialization("default")]
   public class SdtInvoice_RESTInterface : GxGenericCollectionItem<SdtInvoice>
   {
      public SdtInvoice_RESTInterface( ) : base()
      {
      }

      public SdtInvoice_RESTInterface( SdtInvoice psdt ) : base(psdt)
      {
      }

      [DataMember( Name = "InvoiceId" , Order = 0 )]
      [GxSeudo()]
      public Nullable<short> gxTpr_Invoiceid
      {
         get {
            return sdt.gxTpr_Invoiceid ;
         }

         set {
            sdt.gxTpr_Invoiceid = (short)(value.HasValue ? value.Value : 0);
         }

      }

      [DataMember( Name = "InvoiceDate" , Order = 1 )]
      [GxSeudo()]
      public string gxTpr_Invoicedate
      {
         get {
            return DateTimeUtil.DToC2( sdt.gxTpr_Invoicedate) ;
         }

         set {
            sdt.gxTpr_Invoicedate = DateTimeUtil.CToD2( value);
         }

      }

      [DataMember( Name = "CustomerId" , Order = 2 )]
      [GxSeudo()]
      public Nullable<short> gxTpr_Customerid
      {
         get {
            return sdt.gxTpr_Customerid ;
         }

         set {
            sdt.gxTpr_Customerid = (short)(value.HasValue ? value.Value : 0);
         }

      }

      [DataMember( Name = "CustomerName" , Order = 3 )]
      [GxSeudo()]
      public string gxTpr_Customername
      {
         get {
            return StringUtil.RTrim( sdt.gxTpr_Customername) ;
         }

         set {
            sdt.gxTpr_Customername = value;
         }

      }

      [DataMember( Name = "InvoiceTotal" , Order = 4 )]
      [GxSeudo()]
      public Nullable<short> gxTpr_Invoicetotal
      {
         get {
            return sdt.gxTpr_Invoicetotal ;
         }

         set {
            sdt.gxTpr_Invoicetotal = (short)(value.HasValue ? value.Value : 0);
         }

      }

      [DataMember( Name = "InvoiceLast" , Order = 5 )]
      [GxSeudo()]
      public Nullable<short> gxTpr_Invoicelast
      {
         get {
            return sdt.gxTpr_Invoicelast ;
         }

         set {
            sdt.gxTpr_Invoicelast = (short)(value.HasValue ? value.Value : 0);
         }

      }

      [DataMember( Name = "Level" , Order = 6 )]
      public GxGenericCollection<SdtInvoice_Level_RESTInterface> gxTpr_Level
      {
         get {
            return new GxGenericCollection<SdtInvoice_Level_RESTInterface>(sdt.gxTpr_Level) ;
         }

         set {
            value.LoadCollection(sdt.gxTpr_Level);
         }

      }

      public SdtInvoice sdt
      {
         get {
            return (SdtInvoice)Sdt ;
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
            sdt = new SdtInvoice() ;
         }
      }

      [DataMember( Name = "gx_md5_hash", Order = 7 )]
      public string Hash
      {
         get {
            if ( StringUtil.StrCmp(md5Hash, null) == 0 )
            {
               md5Hash = (string)(getHash());
            }
            return md5Hash ;
         }

         set {
            md5Hash = value ;
         }

      }

      private string md5Hash ;
   }

   [DataContract(Name = @"Invoice", Namespace = "TestRestProcs")]
   [GxJsonSerialization("default")]
   public class SdtInvoice_RESTLInterface : GxGenericCollectionItem<SdtInvoice>
   {
      public SdtInvoice_RESTLInterface( ) : base()
      {
      }

      public SdtInvoice_RESTLInterface( SdtInvoice psdt ) : base(psdt)
      {
      }

      [DataMember( Name = "InvoiceDate" , Order = 0 )]
      [GxSeudo()]
      public string gxTpr_Invoicedate
      {
         get {
            return DateTimeUtil.DToC2( sdt.gxTpr_Invoicedate) ;
         }

         set {
            sdt.gxTpr_Invoicedate = DateTimeUtil.CToD2( value);
         }

      }

      [DataMember( Name = "uri", Order = 1 )]
      public string Uri
      {
         get {
            return "" ;
         }

         set {
         }

      }

      public SdtInvoice sdt
      {
         get {
            return (SdtInvoice)Sdt ;
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
            sdt = new SdtInvoice() ;
         }
      }

   }

}
