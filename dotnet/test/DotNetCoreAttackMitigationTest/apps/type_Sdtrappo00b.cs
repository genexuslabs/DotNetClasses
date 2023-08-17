using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using GeneXus.Application;
using GeneXus.Utils;
namespace GeneXus.Programs
{
	[XmlRoot(ElementName = "rappo00b" )]
   [XmlType(TypeName =  "rappo00b" , Namespace = "NETCoreTest" )]
   [Serializable]
   public class Sdtrappo00b : GxSilentTrnSdt
   {
      public Sdtrappo00b( )
      {
      }

      public Sdtrappo00b( IGxContext context )
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
      public override GXProperties GetMetadata( )
      {
         GXProperties metadata = new GXProperties();
         metadata.Set("Name", "rappo00b");
         metadata.Set("BT", "RAPPO00F");
         metadata.Set("PK", "[ \"UteCod\",\"RapPrg\" ]");
         metadata.Set("PKAssigned", "[ \"UteCod\" ]");
         metadata.Set("AllowInsert", "True");
         metadata.Set("AllowUpdate", "True");
         metadata.Set("AllowDelete", "True");
         return metadata ;
      }

      public override void Copy( GxUserType source )
      {
         Sdtrappo00b sdt;
         sdt = (Sdtrappo00b)(source);
         gxTv_Sdtrappo00b_Utecod = sdt.gxTv_Sdtrappo00b_Utecod ;
         gxTv_Sdtrappo00b_Rapprg = sdt.gxTv_Sdtrappo00b_Rapprg ;
         gxTv_Sdtrappo00b_Raptitolo = sdt.gxTv_Sdtrappo00b_Raptitolo ;
         gxTv_Sdtrappo00b_Rapdata = sdt.gxTv_Sdtrappo00b_Rapdata ;
         gxTv_Sdtrappo00b_Raporafin = sdt.gxTv_Sdtrappo00b_Raporafin ;
         gxTv_Sdtrappo00b_Raporaini = sdt.gxTv_Sdtrappo00b_Raporaini ;
         gxTv_Sdtrappo00b_Rapore = sdt.gxTv_Sdtrappo00b_Rapore ;
         gxTv_Sdtrappo00b_Rapdaa = sdt.gxTv_Sdtrappo00b_Rapdaa ;
         gxTv_Sdtrappo00b_Raporafinall = sdt.gxTv_Sdtrappo00b_Raporafinall ;
         gxTv_Sdtrappo00b_Raporainiall = sdt.gxTv_Sdtrappo00b_Raporainiall ;
         gxTv_Sdtrappo00b_Mode = sdt.gxTv_Sdtrappo00b_Mode ;
         gxTv_Sdtrappo00b_Initialized = sdt.gxTv_Sdtrappo00b_Initialized ;
         gxTv_Sdtrappo00b_Utecod_Z = sdt.gxTv_Sdtrappo00b_Utecod_Z ;
         gxTv_Sdtrappo00b_Rapprg_Z = sdt.gxTv_Sdtrappo00b_Rapprg_Z ;
         gxTv_Sdtrappo00b_Raptitolo_Z = sdt.gxTv_Sdtrappo00b_Raptitolo_Z ;
         gxTv_Sdtrappo00b_Rapdata_Z = sdt.gxTv_Sdtrappo00b_Rapdata_Z ;
         gxTv_Sdtrappo00b_Raporafin_Z = sdt.gxTv_Sdtrappo00b_Raporafin_Z ;
         gxTv_Sdtrappo00b_Raporaini_Z = sdt.gxTv_Sdtrappo00b_Raporaini_Z ;
         gxTv_Sdtrappo00b_Rapore_Z = sdt.gxTv_Sdtrappo00b_Rapore_Z ;
         gxTv_Sdtrappo00b_Rapdaa_Z = sdt.gxTv_Sdtrappo00b_Rapdaa_Z ;
         gxTv_Sdtrappo00b_Raporafinall_Z = sdt.gxTv_Sdtrappo00b_Raporafinall_Z ;
         gxTv_Sdtrappo00b_Raporainiall_Z = sdt.gxTv_Sdtrappo00b_Raporainiall_Z ;
         gxTv_Sdtrappo00b_Raptitolo_N = sdt.gxTv_Sdtrappo00b_Raptitolo_N ;
         gxTv_Sdtrappo00b_Rapdata_N = sdt.gxTv_Sdtrappo00b_Rapdata_N ;
         gxTv_Sdtrappo00b_Raporafin_N = sdt.gxTv_Sdtrappo00b_Raporafin_N ;
         gxTv_Sdtrappo00b_Raporaini_N = sdt.gxTv_Sdtrappo00b_Raporaini_N ;
         gxTv_Sdtrappo00b_Rapore_N = sdt.gxTv_Sdtrappo00b_Rapore_N ;
         gxTv_Sdtrappo00b_Rapdaa_N = sdt.gxTv_Sdtrappo00b_Rapdaa_N ;
         gxTv_Sdtrappo00b_Raporafinall_N = sdt.gxTv_Sdtrappo00b_Raporafinall_N ;
         gxTv_Sdtrappo00b_Raporainiall_N = sdt.gxTv_Sdtrappo00b_Raporainiall_N ;
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
         AddObjectProperty("UteCod", StringUtil.LTrim( StringUtil.Str( (decimal)(gxTv_Sdtrappo00b_Utecod), 18, 0)), false, includeNonInitialized);
         AddObjectProperty("RapPrg", StringUtil.LTrim( StringUtil.Str( (decimal)(gxTv_Sdtrappo00b_Rapprg), 18, 0)), false, includeNonInitialized);
         AddObjectProperty("RapTitolo", gxTv_Sdtrappo00b_Raptitolo, false, includeNonInitialized);
         AddObjectProperty("RapTitolo_N", gxTv_Sdtrappo00b_Raptitolo_N, false, includeNonInitialized);
         sDateCnv = "";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Year( gxTv_Sdtrappo00b_Rapdata)), 10, 0));
         sDateCnv += StringUtil.Substring( "0000", 1, 4-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "-";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Month( gxTv_Sdtrappo00b_Rapdata)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "-";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Day( gxTv_Sdtrappo00b_Rapdata)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         AddObjectProperty("RapData", sDateCnv, false, includeNonInitialized);
         AddObjectProperty("RapData_N", gxTv_Sdtrappo00b_Rapdata_N, false, includeNonInitialized);
         datetime_STZ = gxTv_Sdtrappo00b_Raporafin;
         sDateCnv = "";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Year( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "0000", 1, 4-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "-";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Month( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "-";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Day( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "T";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Hour( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += ":";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Minute( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += ":";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Second( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         AddObjectProperty("RapOraFin", sDateCnv, false, includeNonInitialized);
         AddObjectProperty("RapOraFin_N", gxTv_Sdtrappo00b_Raporafin_N, false, includeNonInitialized);
         datetime_STZ = gxTv_Sdtrappo00b_Raporaini;
         sDateCnv = "";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Year( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "0000", 1, 4-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "-";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Month( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "-";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Day( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "T";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Hour( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += ":";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Minute( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += ":";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Second( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         AddObjectProperty("RapOraIni", sDateCnv, false, includeNonInitialized);
         AddObjectProperty("RapOraIni_N", gxTv_Sdtrappo00b_Raporaini_N, false, includeNonInitialized);
         AddObjectProperty("RapOre", gxTv_Sdtrappo00b_Rapore, false, includeNonInitialized);
         AddObjectProperty("RapOre_N", gxTv_Sdtrappo00b_Rapore_N, false, includeNonInitialized);
         datetime_STZ = gxTv_Sdtrappo00b_Rapdaa;
         sDateCnv = "";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Year( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "0000", 1, 4-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "-";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Month( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "-";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Day( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "T";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Hour( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += ":";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Minute( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += ":";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Second( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         AddObjectProperty("RapDaa", sDateCnv, false, includeNonInitialized);
         AddObjectProperty("RapDaa_N", gxTv_Sdtrappo00b_Rapdaa_N, false, includeNonInitialized);
         datetime_STZ = gxTv_Sdtrappo00b_Raporafinall;
         sDateCnv = "";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Year( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "0000", 1, 4-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "-";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Month( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "-";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Day( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "T";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Hour( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += ":";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Minute( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += ":";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Second( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         AddObjectProperty("RapOraFinAll", sDateCnv, false, includeNonInitialized);
         AddObjectProperty("RapOraFinAll_N", gxTv_Sdtrappo00b_Raporafinall_N, false, includeNonInitialized);
         datetime_STZ = gxTv_Sdtrappo00b_Raporainiall;
         sDateCnv = "";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Year( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "0000", 1, 4-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "-";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Month( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "-";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Day( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += "T";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Hour( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += ":";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Minute( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         sDateCnv += ":";
         sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Second( datetime_STZ)), 10, 0));
         sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
         AddObjectProperty("RapOraIniAll", sDateCnv, false, includeNonInitialized);
         AddObjectProperty("RapOraIniAll_N", gxTv_Sdtrappo00b_Raporainiall_N, false, includeNonInitialized);
         if ( includeState )
         {
            AddObjectProperty("Mode", gxTv_Sdtrappo00b_Mode, false, includeNonInitialized);
            AddObjectProperty("Initialized", gxTv_Sdtrappo00b_Initialized, false, includeNonInitialized);
            AddObjectProperty("UteCod_Z", StringUtil.LTrim( StringUtil.Str( (decimal)(gxTv_Sdtrappo00b_Utecod_Z), 18, 0)), false, includeNonInitialized);
            AddObjectProperty("RapPrg_Z", StringUtil.LTrim( StringUtil.Str( (decimal)(gxTv_Sdtrappo00b_Rapprg_Z), 18, 0)), false, includeNonInitialized);
            AddObjectProperty("RapTitolo_Z", gxTv_Sdtrappo00b_Raptitolo_Z, false, includeNonInitialized);
            sDateCnv = "";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Year( gxTv_Sdtrappo00b_Rapdata_Z)), 10, 0));
            sDateCnv += StringUtil.Substring( "0000", 1, 4-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "-";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Month( gxTv_Sdtrappo00b_Rapdata_Z)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "-";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Day( gxTv_Sdtrappo00b_Rapdata_Z)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            AddObjectProperty("RapData_Z", sDateCnv, false, includeNonInitialized);
            datetime_STZ = gxTv_Sdtrappo00b_Raporafin_Z;
            sDateCnv = "";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Year( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "0000", 1, 4-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "-";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Month( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "-";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Day( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "T";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Hour( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += ":";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Minute( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += ":";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Second( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            AddObjectProperty("RapOraFin_Z", sDateCnv, false, includeNonInitialized);
            datetime_STZ = gxTv_Sdtrappo00b_Raporaini_Z;
            sDateCnv = "";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Year( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "0000", 1, 4-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "-";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Month( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "-";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Day( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "T";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Hour( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += ":";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Minute( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += ":";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Second( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            AddObjectProperty("RapOraIni_Z", sDateCnv, false, includeNonInitialized);
            AddObjectProperty("RapOre_Z", gxTv_Sdtrappo00b_Rapore_Z, false, includeNonInitialized);
            datetime_STZ = gxTv_Sdtrappo00b_Rapdaa_Z;
            sDateCnv = "";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Year( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "0000", 1, 4-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "-";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Month( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "-";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Day( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "T";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Hour( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += ":";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Minute( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += ":";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Second( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            AddObjectProperty("RapDaa_Z", sDateCnv, false, includeNonInitialized);
            datetime_STZ = gxTv_Sdtrappo00b_Raporafinall_Z;
            sDateCnv = "";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Year( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "0000", 1, 4-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "-";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Month( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "-";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Day( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "T";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Hour( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += ":";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Minute( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += ":";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Second( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            AddObjectProperty("RapOraFinAll_Z", sDateCnv, false, includeNonInitialized);
            datetime_STZ = gxTv_Sdtrappo00b_Raporainiall_Z;
            sDateCnv = "";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Year( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "0000", 1, 4-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "-";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Month( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "-";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Day( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += "T";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Hour( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += ":";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Minute( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            sDateCnv += ":";
            sNumToPad = StringUtil.Trim( StringUtil.Str( (decimal)(DateTimeUtil.Second( datetime_STZ)), 10, 0));
            sDateCnv += StringUtil.Substring( "00", 1, 2-StringUtil.Len( sNumToPad)) + sNumToPad;
            AddObjectProperty("RapOraIniAll_Z", sDateCnv, false, includeNonInitialized);
            AddObjectProperty("RapTitolo_N", gxTv_Sdtrappo00b_Raptitolo_N, false, includeNonInitialized);
            AddObjectProperty("RapData_N", gxTv_Sdtrappo00b_Rapdata_N, false, includeNonInitialized);
            AddObjectProperty("RapOraFin_N", gxTv_Sdtrappo00b_Raporafin_N, false, includeNonInitialized);
            AddObjectProperty("RapOraIni_N", gxTv_Sdtrappo00b_Raporaini_N, false, includeNonInitialized);
            AddObjectProperty("RapOre_N", gxTv_Sdtrappo00b_Rapore_N, false, includeNonInitialized);
            AddObjectProperty("RapDaa_N", gxTv_Sdtrappo00b_Rapdaa_N, false, includeNonInitialized);
            AddObjectProperty("RapOraFinAll_N", gxTv_Sdtrappo00b_Raporafinall_N, false, includeNonInitialized);
            AddObjectProperty("RapOraIniAll_N", gxTv_Sdtrappo00b_Raporainiall_N, false, includeNonInitialized);
         }
         return  ;
      }

      public void UpdateDirties( Sdtrappo00b sdt )
      {
         if ( sdt.IsDirty("UteCod") )
         {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Utecod = sdt.gxTv_Sdtrappo00b_Utecod ;
         }
         if ( sdt.IsDirty("RapPrg") )
         {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Rapprg = sdt.gxTv_Sdtrappo00b_Rapprg ;
         }
         if ( sdt.IsDirty("RapTitolo") )
         {
            gxTv_Sdtrappo00b_Raptitolo_N = (short)(sdt.gxTv_Sdtrappo00b_Raptitolo_N);
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raptitolo = sdt.gxTv_Sdtrappo00b_Raptitolo ;
         }
         if ( sdt.IsDirty("RapData") )
         {
            gxTv_Sdtrappo00b_Rapdata_N = (short)(sdt.gxTv_Sdtrappo00b_Rapdata_N);
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Rapdata = sdt.gxTv_Sdtrappo00b_Rapdata ;
         }
         if ( sdt.IsDirty("RapOraFin") )
         {
            gxTv_Sdtrappo00b_Raporafin_N = (short)(sdt.gxTv_Sdtrappo00b_Raporafin_N);
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raporafin = sdt.gxTv_Sdtrappo00b_Raporafin ;
         }
         if ( sdt.IsDirty("RapOraIni") )
         {
            gxTv_Sdtrappo00b_Raporaini_N = (short)(sdt.gxTv_Sdtrappo00b_Raporaini_N);
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raporaini = sdt.gxTv_Sdtrappo00b_Raporaini ;
         }
         if ( sdt.IsDirty("RapOre") )
         {
            gxTv_Sdtrappo00b_Rapore_N = (short)(sdt.gxTv_Sdtrappo00b_Rapore_N);
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Rapore = sdt.gxTv_Sdtrappo00b_Rapore ;
         }
         if ( sdt.IsDirty("RapDaa") )
         {
            gxTv_Sdtrappo00b_Rapdaa_N = (short)(sdt.gxTv_Sdtrappo00b_Rapdaa_N);
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Rapdaa = sdt.gxTv_Sdtrappo00b_Rapdaa ;
         }
         if ( sdt.IsDirty("RapOraFinAll") )
         {
            gxTv_Sdtrappo00b_Raporafinall_N = (short)(sdt.gxTv_Sdtrappo00b_Raporafinall_N);
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raporafinall = sdt.gxTv_Sdtrappo00b_Raporafinall ;
         }
         if ( sdt.IsDirty("RapOraIniAll") )
         {
            gxTv_Sdtrappo00b_Raporainiall_N = (short)(sdt.gxTv_Sdtrappo00b_Raporainiall_N);
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raporainiall = sdt.gxTv_Sdtrappo00b_Raporainiall ;
         }
         return  ;
      }

      [  SoapElement( ElementName = "UteCod" )]
      [  XmlElement( ElementName = "UteCod"   )]
      public long gxTpr_Utecod
      {
         get {
            return gxTv_Sdtrappo00b_Utecod ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            if ( gxTv_Sdtrappo00b_Utecod != value )
            {
               gxTv_Sdtrappo00b_Mode = "INS";
               this.gxTv_Sdtrappo00b_Utecod_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Rapprg_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Raptitolo_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Rapdata_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Raporafin_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Raporaini_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Rapore_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Rapdaa_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Raporafinall_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Raporainiall_Z_SetNull( );
            }
            gxTv_Sdtrappo00b_Utecod = value;
            SetDirty("Utecod");
         }

      }

      [  SoapElement( ElementName = "RapPrg" )]
      [  XmlElement( ElementName = "RapPrg"   )]
      public long gxTpr_Rapprg
      {
         get {
            return gxTv_Sdtrappo00b_Rapprg ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            if ( gxTv_Sdtrappo00b_Rapprg != value )
            {
               gxTv_Sdtrappo00b_Mode = "INS";
               this.gxTv_Sdtrappo00b_Utecod_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Rapprg_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Raptitolo_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Rapdata_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Raporafin_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Raporaini_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Rapore_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Rapdaa_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Raporafinall_Z_SetNull( );
               this.gxTv_Sdtrappo00b_Raporainiall_Z_SetNull( );
            }
            gxTv_Sdtrappo00b_Rapprg = value;
            SetDirty("Rapprg");
         }

      }

      [  SoapElement( ElementName = "RapTitolo" )]
      [  XmlElement( ElementName = "RapTitolo"   )]
      public string gxTpr_Raptitolo
      {
         get {
            return gxTv_Sdtrappo00b_Raptitolo ;
         }

         set {
            gxTv_Sdtrappo00b_Raptitolo_N = 0;
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raptitolo = value;
            SetDirty("Raptitolo");
         }

      }

      public void gxTv_Sdtrappo00b_Raptitolo_SetNull( )
      {
         gxTv_Sdtrappo00b_Raptitolo_N = 1;
         gxTv_Sdtrappo00b_Raptitolo = "";
         SetDirty("Raptitolo");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Raptitolo_IsNull( )
      {
         return (gxTv_Sdtrappo00b_Raptitolo_N==1) ;
      }

      [  SoapElement( ElementName = "RapData" )]
      [  XmlElement( ElementName = "RapData"  , IsNullable=true )]
      public string gxTpr_Rapdata_Nullable
      {
         get {
            if ( gxTv_Sdtrappo00b_Rapdata == DateTime.MinValue)
               return null;
            return new GxDateString(gxTv_Sdtrappo00b_Rapdata).value ;
         }

         set {
            gxTv_Sdtrappo00b_Rapdata_N = 0;
            gxTv_Sdtrappo00b_N = 0;
            if (String.IsNullOrEmpty(value) || value == GxDateString.NullValue )
               gxTv_Sdtrappo00b_Rapdata = DateTime.MinValue;
            else
               gxTv_Sdtrappo00b_Rapdata = DateTime.Parse( value);
         }

      }

      [XmlIgnore]
      public DateTime gxTpr_Rapdata
      {
         get {
            return gxTv_Sdtrappo00b_Rapdata ;
         }

         set {
            gxTv_Sdtrappo00b_Rapdata_N = 0;
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Rapdata = value;
            SetDirty("Rapdata");
         }

      }

      public void gxTv_Sdtrappo00b_Rapdata_SetNull( )
      {
         gxTv_Sdtrappo00b_Rapdata_N = 1;
         gxTv_Sdtrappo00b_Rapdata = (DateTime)(DateTime.MinValue);
         SetDirty("Rapdata");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Rapdata_IsNull( )
      {
         return (gxTv_Sdtrappo00b_Rapdata_N==1) ;
      }

      [  SoapElement( ElementName = "RapOraFin" )]
      [  XmlElement( ElementName = "RapOraFin"  , IsNullable=true )]
      public string gxTpr_Raporafin_Nullable
      {
         get {
            if ( gxTv_Sdtrappo00b_Raporafin == DateTime.MinValue)
               return null;
            return new GxDatetimeString(gxTv_Sdtrappo00b_Raporafin).value ;
         }

         set {
            gxTv_Sdtrappo00b_Raporafin_N = 0;
            gxTv_Sdtrappo00b_N = 0;
            if (String.IsNullOrEmpty(value) || value == GxDatetimeString.NullValue )
               gxTv_Sdtrappo00b_Raporafin = DateTime.MinValue;
            else
               gxTv_Sdtrappo00b_Raporafin = DateTime.Parse( value);
         }

      }

      [XmlIgnore]
      public DateTime gxTpr_Raporafin
      {
         get {
            return gxTv_Sdtrappo00b_Raporafin ;
         }

         set {
            gxTv_Sdtrappo00b_Raporafin_N = 0;
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raporafin = value;
            SetDirty("Raporafin");
         }

      }

      public void gxTv_Sdtrappo00b_Raporafin_SetNull( )
      {
         gxTv_Sdtrappo00b_Raporafin_N = 1;
         gxTv_Sdtrappo00b_Raporafin = (DateTime)(DateTime.MinValue);
         SetDirty("Raporafin");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Raporafin_IsNull( )
      {
         return (gxTv_Sdtrappo00b_Raporafin_N==1) ;
      }

      [  SoapElement( ElementName = "RapOraIni" )]
      [  XmlElement( ElementName = "RapOraIni"  , IsNullable=true )]
      public string gxTpr_Raporaini_Nullable
      {
         get {
            if ( gxTv_Sdtrappo00b_Raporaini == DateTime.MinValue)
               return null;
            return new GxDatetimeString(gxTv_Sdtrappo00b_Raporaini).value ;
         }

         set {
            gxTv_Sdtrappo00b_Raporaini_N = 0;
            gxTv_Sdtrappo00b_N = 0;
            if (String.IsNullOrEmpty(value) || value == GxDatetimeString.NullValue )
               gxTv_Sdtrappo00b_Raporaini = DateTime.MinValue;
            else
               gxTv_Sdtrappo00b_Raporaini = DateTime.Parse( value);
         }

      }

      [XmlIgnore]
      public DateTime gxTpr_Raporaini
      {
         get {
            return gxTv_Sdtrappo00b_Raporaini ;
         }

         set {
            gxTv_Sdtrappo00b_Raporaini_N = 0;
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raporaini = value;
            SetDirty("Raporaini");
         }

      }

      public void gxTv_Sdtrappo00b_Raporaini_SetNull( )
      {
         gxTv_Sdtrappo00b_Raporaini_N = 1;
         gxTv_Sdtrappo00b_Raporaini = (DateTime)(DateTime.MinValue);
         SetDirty("Raporaini");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Raporaini_IsNull( )
      {
         return (gxTv_Sdtrappo00b_Raporaini_N==1) ;
      }

      [  SoapElement( ElementName = "RapOre" )]
      [  XmlElement( ElementName = "RapOre"   )]
      public decimal gxTpr_Rapore
      {
         get {
            return gxTv_Sdtrappo00b_Rapore ;
         }

         set {
            gxTv_Sdtrappo00b_Rapore_N = 0;
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Rapore = value;
            SetDirty("Rapore");
         }

      }

      public void gxTv_Sdtrappo00b_Rapore_SetNull( )
      {
         gxTv_Sdtrappo00b_Rapore_N = 1;
         gxTv_Sdtrappo00b_Rapore = 0;
         SetDirty("Rapore");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Rapore_IsNull( )
      {
         return (gxTv_Sdtrappo00b_Rapore_N==1) ;
      }

      [  SoapElement( ElementName = "RapDaa" )]
      [  XmlElement( ElementName = "RapDaa"  , IsNullable=true )]
      public string gxTpr_Rapdaa_Nullable
      {
         get {
            if ( gxTv_Sdtrappo00b_Rapdaa == DateTime.MinValue)
               return null;
            return new GxDatetimeString(gxTv_Sdtrappo00b_Rapdaa).value ;
         }

         set {
            gxTv_Sdtrappo00b_Rapdaa_N = 0;
            gxTv_Sdtrappo00b_N = 0;
            if (String.IsNullOrEmpty(value) || value == GxDatetimeString.NullValue )
               gxTv_Sdtrappo00b_Rapdaa = DateTime.MinValue;
            else
               gxTv_Sdtrappo00b_Rapdaa = DateTime.Parse( value);
         }

      }

      [XmlIgnore]
      public DateTime gxTpr_Rapdaa
      {
         get {
            return gxTv_Sdtrappo00b_Rapdaa ;
         }

         set {
            gxTv_Sdtrappo00b_Rapdaa_N = 0;
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Rapdaa = value;
            SetDirty("Rapdaa");
         }

      }

      public void gxTv_Sdtrappo00b_Rapdaa_SetNull( )
      {
         gxTv_Sdtrappo00b_Rapdaa_N = 1;
         gxTv_Sdtrappo00b_Rapdaa = (DateTime)(DateTime.MinValue);
         SetDirty("Rapdaa");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Rapdaa_IsNull( )
      {
         return (gxTv_Sdtrappo00b_Rapdaa_N==1) ;
      }

      [  SoapElement( ElementName = "RapOraFinAll" )]
      [  XmlElement( ElementName = "RapOraFinAll"  , IsNullable=true )]
      public string gxTpr_Raporafinall_Nullable
      {
         get {
            if ( gxTv_Sdtrappo00b_Raporafinall == DateTime.MinValue)
               return null;
            return new GxDatetimeString(gxTv_Sdtrappo00b_Raporafinall).value ;
         }

         set {
            gxTv_Sdtrappo00b_Raporafinall_N = 0;
            gxTv_Sdtrappo00b_N = 0;
            if (String.IsNullOrEmpty(value) || value == GxDatetimeString.NullValue )
               gxTv_Sdtrappo00b_Raporafinall = DateTime.MinValue;
            else
               gxTv_Sdtrappo00b_Raporafinall = DateTime.Parse( value);
         }

      }

      [XmlIgnore]
      public DateTime gxTpr_Raporafinall
      {
         get {
            return gxTv_Sdtrappo00b_Raporafinall ;
         }

         set {
            gxTv_Sdtrappo00b_Raporafinall_N = 0;
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raporafinall = value;
            SetDirty("Raporafinall");
         }

      }

      public void gxTv_Sdtrappo00b_Raporafinall_SetNull( )
      {
         gxTv_Sdtrappo00b_Raporafinall_N = 1;
         gxTv_Sdtrappo00b_Raporafinall = (DateTime)(DateTime.MinValue);
         SetDirty("Raporafinall");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Raporafinall_IsNull( )
      {
         return (gxTv_Sdtrappo00b_Raporafinall_N==1) ;
      }

      [  SoapElement( ElementName = "RapOraIniAll" )]
      [  XmlElement( ElementName = "RapOraIniAll"  , IsNullable=true )]
      public string gxTpr_Raporainiall_Nullable
      {
         get {
            if ( gxTv_Sdtrappo00b_Raporainiall == DateTime.MinValue)
               return null;
            return new GxDatetimeString(gxTv_Sdtrappo00b_Raporainiall).value ;
         }

         set {
            gxTv_Sdtrappo00b_Raporainiall_N = 0;
            gxTv_Sdtrappo00b_N = 0;
            if (String.IsNullOrEmpty(value) || value == GxDatetimeString.NullValue )
               gxTv_Sdtrappo00b_Raporainiall = DateTime.MinValue;
            else
               gxTv_Sdtrappo00b_Raporainiall = DateTime.Parse( value);
         }

      }

      [XmlIgnore]
      public DateTime gxTpr_Raporainiall
      {
         get {
            return gxTv_Sdtrappo00b_Raporainiall ;
         }

         set {
            gxTv_Sdtrappo00b_Raporainiall_N = 0;
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raporainiall = value;
            SetDirty("Raporainiall");
         }

      }

      public void gxTv_Sdtrappo00b_Raporainiall_SetNull( )
      {
         gxTv_Sdtrappo00b_Raporainiall_N = 1;
         gxTv_Sdtrappo00b_Raporainiall = (DateTime)(DateTime.MinValue);
         SetDirty("Raporainiall");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Raporainiall_IsNull( )
      {
         return (gxTv_Sdtrappo00b_Raporainiall_N==1) ;
      }

      [  SoapElement( ElementName = "Mode" )]
      [  XmlElement( ElementName = "Mode"   )]
      public string gxTpr_Mode
      {
         get {
            return gxTv_Sdtrappo00b_Mode ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Mode = value;
            SetDirty("Mode");
         }

      }

      public void gxTv_Sdtrappo00b_Mode_SetNull( )
      {
         gxTv_Sdtrappo00b_Mode = "";
         SetDirty("Mode");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Mode_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "Initialized" )]
      [  XmlElement( ElementName = "Initialized"   )]
      public short gxTpr_Initialized
      {
         get {
            return gxTv_Sdtrappo00b_Initialized ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Initialized = value;
            SetDirty("Initialized");
         }

      }

      public void gxTv_Sdtrappo00b_Initialized_SetNull( )
      {
         gxTv_Sdtrappo00b_Initialized = 0;
         SetDirty("Initialized");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Initialized_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "UteCod_Z" )]
      [  XmlElement( ElementName = "UteCod_Z"   )]
      public long gxTpr_Utecod_Z
      {
         get {
            return gxTv_Sdtrappo00b_Utecod_Z ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Utecod_Z = value;
            SetDirty("Utecod_Z");
         }

      }

      public void gxTv_Sdtrappo00b_Utecod_Z_SetNull( )
      {
         gxTv_Sdtrappo00b_Utecod_Z = 0;
         SetDirty("Utecod_Z");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Utecod_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapPrg_Z" )]
      [  XmlElement( ElementName = "RapPrg_Z"   )]
      public long gxTpr_Rapprg_Z
      {
         get {
            return gxTv_Sdtrappo00b_Rapprg_Z ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Rapprg_Z = value;
            SetDirty("Rapprg_Z");
         }

      }

      public void gxTv_Sdtrappo00b_Rapprg_Z_SetNull( )
      {
         gxTv_Sdtrappo00b_Rapprg_Z = 0;
         SetDirty("Rapprg_Z");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Rapprg_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapTitolo_Z" )]
      [  XmlElement( ElementName = "RapTitolo_Z"   )]
      public string gxTpr_Raptitolo_Z
      {
         get {
            return gxTv_Sdtrappo00b_Raptitolo_Z ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raptitolo_Z = value;
            SetDirty("Raptitolo_Z");
         }

      }

      public void gxTv_Sdtrappo00b_Raptitolo_Z_SetNull( )
      {
         gxTv_Sdtrappo00b_Raptitolo_Z = "";
         SetDirty("Raptitolo_Z");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Raptitolo_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapData_Z" )]
      [  XmlElement( ElementName = "RapData_Z"  , IsNullable=true )]
      public string gxTpr_Rapdata_Z_Nullable
      {
         get {
            if ( gxTv_Sdtrappo00b_Rapdata_Z == DateTime.MinValue)
               return null;
            return new GxDateString(gxTv_Sdtrappo00b_Rapdata_Z).value ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            if (String.IsNullOrEmpty(value) || value == GxDateString.NullValue )
               gxTv_Sdtrappo00b_Rapdata_Z = DateTime.MinValue;
            else
               gxTv_Sdtrappo00b_Rapdata_Z = DateTime.Parse( value);
         }

      }

      [XmlIgnore]
      public DateTime gxTpr_Rapdata_Z
      {
         get {
            return gxTv_Sdtrappo00b_Rapdata_Z ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Rapdata_Z = value;
            SetDirty("Rapdata_Z");
         }

      }

      public void gxTv_Sdtrappo00b_Rapdata_Z_SetNull( )
      {
         gxTv_Sdtrappo00b_Rapdata_Z = (DateTime)(DateTime.MinValue);
         SetDirty("Rapdata_Z");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Rapdata_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapOraFin_Z" )]
      [  XmlElement( ElementName = "RapOraFin_Z"  , IsNullable=true )]
      public string gxTpr_Raporafin_Z_Nullable
      {
         get {
            if ( gxTv_Sdtrappo00b_Raporafin_Z == DateTime.MinValue)
               return null;
            return new GxDatetimeString(gxTv_Sdtrappo00b_Raporafin_Z).value ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            if (String.IsNullOrEmpty(value) || value == GxDatetimeString.NullValue )
               gxTv_Sdtrappo00b_Raporafin_Z = DateTime.MinValue;
            else
               gxTv_Sdtrappo00b_Raporafin_Z = DateTime.Parse( value);
         }

      }

      [XmlIgnore]
      public DateTime gxTpr_Raporafin_Z
      {
         get {
            return gxTv_Sdtrappo00b_Raporafin_Z ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raporafin_Z = value;
            SetDirty("Raporafin_Z");
         }

      }

      public void gxTv_Sdtrappo00b_Raporafin_Z_SetNull( )
      {
         gxTv_Sdtrappo00b_Raporafin_Z = (DateTime)(DateTime.MinValue);
         SetDirty("Raporafin_Z");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Raporafin_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapOraIni_Z" )]
      [  XmlElement( ElementName = "RapOraIni_Z"  , IsNullable=true )]
      public string gxTpr_Raporaini_Z_Nullable
      {
         get {
            if ( gxTv_Sdtrappo00b_Raporaini_Z == DateTime.MinValue)
               return null;
            return new GxDatetimeString(gxTv_Sdtrappo00b_Raporaini_Z).value ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            if (String.IsNullOrEmpty(value) || value == GxDatetimeString.NullValue )
               gxTv_Sdtrappo00b_Raporaini_Z = DateTime.MinValue;
            else
               gxTv_Sdtrappo00b_Raporaini_Z = DateTime.Parse( value);
         }

      }

      [XmlIgnore]
      public DateTime gxTpr_Raporaini_Z
      {
         get {
            return gxTv_Sdtrappo00b_Raporaini_Z ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raporaini_Z = value;
            SetDirty("Raporaini_Z");
         }

      }

      public void gxTv_Sdtrappo00b_Raporaini_Z_SetNull( )
      {
         gxTv_Sdtrappo00b_Raporaini_Z = (DateTime)(DateTime.MinValue);
         SetDirty("Raporaini_Z");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Raporaini_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapOre_Z" )]
      [  XmlElement( ElementName = "RapOre_Z"   )]
      public decimal gxTpr_Rapore_Z
      {
         get {
            return gxTv_Sdtrappo00b_Rapore_Z ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Rapore_Z = value;
            SetDirty("Rapore_Z");
         }

      }

      public void gxTv_Sdtrappo00b_Rapore_Z_SetNull( )
      {
         gxTv_Sdtrappo00b_Rapore_Z = 0;
         SetDirty("Rapore_Z");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Rapore_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapDaa_Z" )]
      [  XmlElement( ElementName = "RapDaa_Z"  , IsNullable=true )]
      public string gxTpr_Rapdaa_Z_Nullable
      {
         get {
            if ( gxTv_Sdtrappo00b_Rapdaa_Z == DateTime.MinValue)
               return null;
            return new GxDatetimeString(gxTv_Sdtrappo00b_Rapdaa_Z).value ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            if (String.IsNullOrEmpty(value) || value == GxDatetimeString.NullValue )
               gxTv_Sdtrappo00b_Rapdaa_Z = DateTime.MinValue;
            else
               gxTv_Sdtrappo00b_Rapdaa_Z = DateTime.Parse( value);
         }

      }

      [XmlIgnore]
      public DateTime gxTpr_Rapdaa_Z
      {
         get {
            return gxTv_Sdtrappo00b_Rapdaa_Z ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Rapdaa_Z = value;
            SetDirty("Rapdaa_Z");
         }

      }

      public void gxTv_Sdtrappo00b_Rapdaa_Z_SetNull( )
      {
         gxTv_Sdtrappo00b_Rapdaa_Z = (DateTime)(DateTime.MinValue);
         SetDirty("Rapdaa_Z");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Rapdaa_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapOraFinAll_Z" )]
      [  XmlElement( ElementName = "RapOraFinAll_Z"  , IsNullable=true )]
      public string gxTpr_Raporafinall_Z_Nullable
      {
         get {
            if ( gxTv_Sdtrappo00b_Raporafinall_Z == DateTime.MinValue)
               return null;
            return new GxDatetimeString(gxTv_Sdtrappo00b_Raporafinall_Z).value ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            if (String.IsNullOrEmpty(value) || value == GxDatetimeString.NullValue )
               gxTv_Sdtrappo00b_Raporafinall_Z = DateTime.MinValue;
            else
               gxTv_Sdtrappo00b_Raporafinall_Z = DateTime.Parse( value);
         }

      }

      [XmlIgnore]
      public DateTime gxTpr_Raporafinall_Z
      {
         get {
            return gxTv_Sdtrappo00b_Raporafinall_Z ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raporafinall_Z = value;
            SetDirty("Raporafinall_Z");
         }

      }

      public void gxTv_Sdtrappo00b_Raporafinall_Z_SetNull( )
      {
         gxTv_Sdtrappo00b_Raporafinall_Z = (DateTime)(DateTime.MinValue);
         SetDirty("Raporafinall_Z");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Raporafinall_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapOraIniAll_Z" )]
      [  XmlElement( ElementName = "RapOraIniAll_Z"  , IsNullable=true )]
      public string gxTpr_Raporainiall_Z_Nullable
      {
         get {
            if ( gxTv_Sdtrappo00b_Raporainiall_Z == DateTime.MinValue)
               return null;
            return new GxDatetimeString(gxTv_Sdtrappo00b_Raporainiall_Z).value ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            if (String.IsNullOrEmpty(value) || value == GxDatetimeString.NullValue )
               gxTv_Sdtrappo00b_Raporainiall_Z = DateTime.MinValue;
            else
               gxTv_Sdtrappo00b_Raporainiall_Z = DateTime.Parse( value);
         }

      }

      [XmlIgnore]
      public DateTime gxTpr_Raporainiall_Z
      {
         get {
            return gxTv_Sdtrappo00b_Raporainiall_Z ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raporainiall_Z = value;
            SetDirty("Raporainiall_Z");
         }

      }

      public void gxTv_Sdtrappo00b_Raporainiall_Z_SetNull( )
      {
         gxTv_Sdtrappo00b_Raporainiall_Z = (DateTime)(DateTime.MinValue);
         SetDirty("Raporainiall_Z");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Raporainiall_Z_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapTitolo_N" )]
      [  XmlElement( ElementName = "RapTitolo_N"   )]
      public short gxTpr_Raptitolo_N
      {
         get {
            return gxTv_Sdtrappo00b_Raptitolo_N ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raptitolo_N = value;
            SetDirty("Raptitolo_N");
         }

      }

      public void gxTv_Sdtrappo00b_Raptitolo_N_SetNull( )
      {
         gxTv_Sdtrappo00b_Raptitolo_N = 0;
         SetDirty("Raptitolo_N");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Raptitolo_N_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapData_N" )]
      [  XmlElement( ElementName = "RapData_N"   )]
      public short gxTpr_Rapdata_N
      {
         get {
            return gxTv_Sdtrappo00b_Rapdata_N ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Rapdata_N = value;
            SetDirty("Rapdata_N");
         }

      }

      public void gxTv_Sdtrappo00b_Rapdata_N_SetNull( )
      {
         gxTv_Sdtrappo00b_Rapdata_N = 0;
         SetDirty("Rapdata_N");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Rapdata_N_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapOraFin_N" )]
      [  XmlElement( ElementName = "RapOraFin_N"   )]
      public short gxTpr_Raporafin_N
      {
         get {
            return gxTv_Sdtrappo00b_Raporafin_N ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raporafin_N = value;
            SetDirty("Raporafin_N");
         }

      }

      public void gxTv_Sdtrappo00b_Raporafin_N_SetNull( )
      {
         gxTv_Sdtrappo00b_Raporafin_N = 0;
         SetDirty("Raporafin_N");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Raporafin_N_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapOraIni_N" )]
      [  XmlElement( ElementName = "RapOraIni_N"   )]
      public short gxTpr_Raporaini_N
      {
         get {
            return gxTv_Sdtrappo00b_Raporaini_N ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raporaini_N = value;
            SetDirty("Raporaini_N");
         }

      }

      public void gxTv_Sdtrappo00b_Raporaini_N_SetNull( )
      {
         gxTv_Sdtrappo00b_Raporaini_N = 0;
         SetDirty("Raporaini_N");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Raporaini_N_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapOre_N" )]
      [  XmlElement( ElementName = "RapOre_N"   )]
      public short gxTpr_Rapore_N
      {
         get {
            return gxTv_Sdtrappo00b_Rapore_N ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Rapore_N = value;
            SetDirty("Rapore_N");
         }

      }

      public void gxTv_Sdtrappo00b_Rapore_N_SetNull( )
      {
         gxTv_Sdtrappo00b_Rapore_N = 0;
         SetDirty("Rapore_N");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Rapore_N_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapDaa_N" )]
      [  XmlElement( ElementName = "RapDaa_N"   )]
      public short gxTpr_Rapdaa_N
      {
         get {
            return gxTv_Sdtrappo00b_Rapdaa_N ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Rapdaa_N = value;
            SetDirty("Rapdaa_N");
         }

      }

      public void gxTv_Sdtrappo00b_Rapdaa_N_SetNull( )
      {
         gxTv_Sdtrappo00b_Rapdaa_N = 0;
         SetDirty("Rapdaa_N");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Rapdaa_N_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapOraFinAll_N" )]
      [  XmlElement( ElementName = "RapOraFinAll_N"   )]
      public short gxTpr_Raporafinall_N
      {
         get {
            return gxTv_Sdtrappo00b_Raporafinall_N ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raporafinall_N = value;
            SetDirty("Raporafinall_N");
         }

      }

      public void gxTv_Sdtrappo00b_Raporafinall_N_SetNull( )
      {
         gxTv_Sdtrappo00b_Raporafinall_N = 0;
         SetDirty("Raporafinall_N");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Raporafinall_N_IsNull( )
      {
         return false ;
      }

      [  SoapElement( ElementName = "RapOraIniAll_N" )]
      [  XmlElement( ElementName = "RapOraIniAll_N"   )]
      public short gxTpr_Raporainiall_N
      {
         get {
            return gxTv_Sdtrappo00b_Raporainiall_N ;
         }

         set {
            gxTv_Sdtrappo00b_N = 0;
            gxTv_Sdtrappo00b_Raporainiall_N = value;
            SetDirty("Raporainiall_N");
         }

      }

      public void gxTv_Sdtrappo00b_Raporainiall_N_SetNull( )
      {
         gxTv_Sdtrappo00b_Raporainiall_N = 0;
         SetDirty("Raporainiall_N");
         return  ;
      }

      public bool gxTv_Sdtrappo00b_Raporainiall_N_IsNull( )
      {
         return false ;
      }

      public void initialize( )
      {
         gxTv_Sdtrappo00b_Utecod = 1;
         gxTv_Sdtrappo00b_N = 1;
         gxTv_Sdtrappo00b_Raptitolo = "";
         gxTv_Sdtrappo00b_Rapdata = DateTime.MinValue;
         gxTv_Sdtrappo00b_Raporafin = (DateTime)(DateTime.MinValue);
         gxTv_Sdtrappo00b_Raporaini = (DateTime)(DateTime.MinValue);
         gxTv_Sdtrappo00b_Rapdaa = (DateTime)(DateTime.MinValue);
         gxTv_Sdtrappo00b_Raporafinall = (DateTime)(DateTime.MinValue);
         gxTv_Sdtrappo00b_Raporainiall = (DateTime)(DateTime.MinValue);
         gxTv_Sdtrappo00b_Mode = "";
         gxTv_Sdtrappo00b_Raptitolo_Z = "";
         gxTv_Sdtrappo00b_Rapdata_Z = DateTime.MinValue;
         gxTv_Sdtrappo00b_Raporafin_Z = (DateTime)(DateTime.MinValue);
         gxTv_Sdtrappo00b_Raporaini_Z = (DateTime)(DateTime.MinValue);
         gxTv_Sdtrappo00b_Rapdaa_Z = (DateTime)(DateTime.MinValue);
         gxTv_Sdtrappo00b_Raporafinall_Z = (DateTime)(DateTime.MinValue);
         gxTv_Sdtrappo00b_Raporainiall_Z = (DateTime)(DateTime.MinValue);
         sDateCnv = "";
         sNumToPad = "";
         datetime_STZ = (DateTime)(DateTime.MinValue);
         /*IGxSilentTrn obj;
         obj = (IGxSilentTrn)ClassLoader.FindInstance( "rappo00b", "GeneXus.Programs.rappo00b_bc", new Object[] {context}, constructorCallingAssembly);;
         obj.initialize();
         obj.SetSDT(this, 1);
         setTransaction( obj) ;
         obj.SetMode("INS");*/
         return  ;
      }

      public short isNull( )
      {
         return gxTv_Sdtrappo00b_N ;
      }

      private short gxTv_Sdtrappo00b_N ;
      private short gxTv_Sdtrappo00b_Initialized ;
      private short gxTv_Sdtrappo00b_Raptitolo_N ;
      private short gxTv_Sdtrappo00b_Rapdata_N ;
      private short gxTv_Sdtrappo00b_Raporafin_N ;
      private short gxTv_Sdtrappo00b_Raporaini_N ;
      private short gxTv_Sdtrappo00b_Rapore_N ;
      private short gxTv_Sdtrappo00b_Rapdaa_N ;
      private short gxTv_Sdtrappo00b_Raporafinall_N ;
      private short gxTv_Sdtrappo00b_Raporainiall_N ;
      private long gxTv_Sdtrappo00b_Utecod ;
      private long gxTv_Sdtrappo00b_Rapprg ;
      private long gxTv_Sdtrappo00b_Utecod_Z ;
      private long gxTv_Sdtrappo00b_Rapprg_Z ;
      private decimal gxTv_Sdtrappo00b_Rapore ;
      private decimal gxTv_Sdtrappo00b_Rapore_Z ;
      private string gxTv_Sdtrappo00b_Mode ;
      private string sDateCnv ;
      private string sNumToPad ;
      private DateTime gxTv_Sdtrappo00b_Raporafin ;
      private DateTime gxTv_Sdtrappo00b_Raporaini ;
      private DateTime gxTv_Sdtrappo00b_Rapdaa ;
      private DateTime gxTv_Sdtrappo00b_Raporafinall ;
      private DateTime gxTv_Sdtrappo00b_Raporainiall ;
      private DateTime gxTv_Sdtrappo00b_Raporafin_Z ;
      private DateTime gxTv_Sdtrappo00b_Raporaini_Z ;
      private DateTime gxTv_Sdtrappo00b_Rapdaa_Z ;
      private DateTime gxTv_Sdtrappo00b_Raporafinall_Z ;
      private DateTime gxTv_Sdtrappo00b_Raporainiall_Z ;
      private DateTime datetime_STZ ;
      private DateTime gxTv_Sdtrappo00b_Rapdata ;
      private DateTime gxTv_Sdtrappo00b_Rapdata_Z ;
      private string gxTv_Sdtrappo00b_Raptitolo ;
      private string gxTv_Sdtrappo00b_Raptitolo_Z ;
   }

   [DataContract(Name = @"rappo00b", Namespace = "NETCoreTest")]
   public class Sdtrappo00b_RESTInterface : GxGenericCollectionItem<Sdtrappo00b>
   {
      public Sdtrappo00b_RESTInterface( ) : base()
      {
      }

      public Sdtrappo00b_RESTInterface( Sdtrappo00b psdt ) : base(psdt)
      {
      }

      [DataMember( Name = "UteCod" , Order = 0 )]
      [GxSeudo()]
      public string gxTpr_Utecod
      {
         get {
            return StringUtil.LTrim( StringUtil.Str( (decimal)(sdt.gxTpr_Utecod), 18, 0)) ;
         }

         set {
            sdt.gxTpr_Utecod = (long)(NumberUtil.Val( value, "."));
         }

      }

      [DataMember( Name = "RapPrg" , Order = 1 )]
      [GxSeudo()]
      public string gxTpr_Rapprg
      {
         get {
            return StringUtil.LTrim( StringUtil.Str( (decimal)(sdt.gxTpr_Rapprg), 18, 0)) ;
         }

         set {
            sdt.gxTpr_Rapprg = (long)(NumberUtil.Val( value, "."));
         }

      }

      [DataMember( Name = "RapTitolo" , Order = 2 )]
      [GxSeudo()]
      public string gxTpr_Raptitolo
      {
         get {
            return sdt.gxTpr_Raptitolo ;
         }

         set {
            sdt.gxTpr_Raptitolo = value;
         }

      }

      [DataMember( Name = "RapData" , Order = 3 )]
      [GxSeudo()]
      public string gxTpr_Rapdata
      {
         get {
            return DateTimeUtil.DToC2( sdt.gxTpr_Rapdata) ;
         }

         set {
            sdt.gxTpr_Rapdata = DateTimeUtil.CToD2( value);
         }

      }

      [DataMember( Name = "RapOraFin" , Order = 4 )]
      [GxSeudo()]
      public string gxTpr_Raporafin
      {
         get {
            return DateTimeUtil.TToC2( sdt.gxTpr_Raporafin) ;
         }

         set {
            GXt_dtime1 = DateTimeUtil.ResetDate(DateTimeUtil.CToT2( value));
            sdt.gxTpr_Raporafin = GXt_dtime1;
         }

      }

      [DataMember( Name = "RapOraIni" , Order = 5 )]
      [GxSeudo()]
      public string gxTpr_Raporaini
      {
         get {
            return DateTimeUtil.TToC2( sdt.gxTpr_Raporaini) ;
         }

         set {
            GXt_dtime1 = DateTimeUtil.ResetDate(DateTimeUtil.CToT2( value));
            sdt.gxTpr_Raporaini = GXt_dtime1;
         }

      }

      [DataMember( Name = "RapOre" , Order = 6 )]
      [GxSeudo()]
      public string gxTpr_Rapore
      {
         get {
            return StringUtil.LTrim( StringUtil.Str( sdt.gxTpr_Rapore, 9, 2)) ;
         }

         set {
            sdt.gxTpr_Rapore = NumberUtil.Val( value, ".");
         }

      }

      [DataMember( Name = "RapDaa" , Order = 7 )]
      [GxSeudo()]
      public string gxTpr_Rapdaa
      {
         get {
            return DateTimeUtil.TToC2( sdt.gxTpr_Rapdaa) ;
         }

         set {
            sdt.gxTpr_Rapdaa = DateTimeUtil.CToT2( value);
         }

      }

      [DataMember( Name = "RapOraFinAll" , Order = 8 )]
      [GxSeudo()]
      public string gxTpr_Raporafinall
      {
         get {
            return DateTimeUtil.TToC2( sdt.gxTpr_Raporafinall) ;
         }

         set {
            sdt.gxTpr_Raporafinall = DateTimeUtil.CToT2( value);
         }

      }

      [DataMember( Name = "RapOraIniAll" , Order = 9 )]
      [GxSeudo()]
      public string gxTpr_Raporainiall
      {
         get {
            return DateTimeUtil.TToC2( sdt.gxTpr_Raporainiall) ;
         }

         set {
            sdt.gxTpr_Raporainiall = DateTimeUtil.CToT2( value);
         }

      }

      public Sdtrappo00b sdt
      {
         get {
            return (Sdtrappo00b)Sdt ;
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
            sdt = new Sdtrappo00b() ;
         }
      }

      [DataMember( Name = "gx_md5_hash", Order = 10 )]
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
      private DateTime GXt_dtime1 ;
   }

   [DataContract(Name = @"rappo00b", Namespace = "NETCoreTest")]
   public class Sdtrappo00b_RESTLInterface : GxGenericCollectionItem<Sdtrappo00b>
   {
      public Sdtrappo00b_RESTLInterface( ) : base()
      {
      }

      public Sdtrappo00b_RESTLInterface( Sdtrappo00b psdt ) : base(psdt)
      {
      }

      [DataMember( Name = "RapTitolo" , Order = 0 )]
      [GxSeudo()]
      public string gxTpr_Raptitolo
      {
         get {
            return sdt.gxTpr_Raptitolo ;
         }

         set {
            sdt.gxTpr_Raptitolo = value;
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

      public Sdtrappo00b sdt
      {
         get {
            return (Sdtrappo00b)Sdt ;
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
            sdt = new Sdtrappo00b() ;
         }
      }

   }

}
