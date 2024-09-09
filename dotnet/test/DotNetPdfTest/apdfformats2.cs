using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Procedure;
using GeneXus.Utils;
namespace GeneXus.Programs
{
	public class apdfformats2 : GXProcedure
   {
      public int executeCmdLine( string[] args )
      {
         return ExecuteCmdLine(args); ;
      }

      protected override int ExecuteCmdLine( string[] args )
      {
         execute();
         return GX.GXRuntime.ExitCode ;
      }

      public apdfformats2( )
      {
         context = new GxContext(  );
         DataStoreUtil.LoadDataStores( context);
         IsMain = true;
         context.SetDefaultTheme("Carmine", false);
      }

      public apdfformats2( IGxContext context )
      {
         this.context = context;
         IsMain = false;
      }

      public void execute( )
      {
         initialize();
         ExecutePrivate();
      }

      public void executeSubmit( )
      {
         SubmitImpl();
      }

      protected override void ExecutePrivate( )
      {
         /* GeneXus formulas */
         /* Output device settings */
         M_top = 0;
         M_bot = 6;
         P_lines = (int)(66-M_bot);
         getPrinter().GxClearAttris() ;
         add_metrics( ) ;
         lineHeight = 15;
         gxXPage = 100;
         gxYPage = 100;
         getPrinter().GxSetDocName("PDFFormat") ;
         getPrinter().GxSetDocFormat("PDF") ;
         try
         {
            Gx_out = "FIL" ;
            if (!initPrinter (Gx_out, gxXPage, gxYPage, "GXPRN.INI", "", "", 2, 1, 256, 16834, 11909, 0, 1, 1, 0, 1, 1) )
            {
               cleanup();
               return;
            }
            getPrinter().setModal(false) ;
            P_lines = (int)(gxYPage-(lineHeight*6));
            Gx_line = (int)(P_lines+1);
            getPrinter().setPageLines(P_lines);
            getPrinter().setLineHeight(lineHeight);
            getPrinter().setM_top(M_top);
            getPrinter().setM_bot(M_bot);
            AV16staticDir = string.Empty;
            GXt_objcol_SdtFCKTstCollection_FCKTst1 = AV17FCKTstCollection;
            new adatos(context ).execute(  AV16staticDir, out  GXt_objcol_SdtFCKTstCollection_FCKTst1) ;
            AV17FCKTstCollection = GXt_objcol_SdtFCKTstCollection_FCKTst1;
            AV24GXV1 = 1;
            while ( AV24GXV1 <= AV17FCKTstCollection.Count )
            {
               AV18FCKTstItem = ((SdtFCKTstCollection_FCKTst)AV17FCKTstCollection.Item(AV24GXV1));
               AV8FCKAux = AV18FCKTstItem.gxTpr_Fcktstfck;
               AV14FormatRawHTML = AV18FCKTstItem.gxTpr_Fcktstfck;
               AV20FCKTstFCK = AV18FCKTstItem.gxTpr_Fcktstfck;
               AV21FCKTstId = AV18FCKTstItem.gxTpr_Fcktstid;
               AV22FCKTstDsc = AV18FCKTstItem.gxTpr_Fcktstdsc;
               AV23FCKTstOtro = AV18FCKTstItem.gxTpr_Fcktstotro;
               H1U0( false, 548) ;
               getPrinter().GxAttris("Microsoft Sans Serif", 8, false, false, false, false, 0, 0, 0, 0, 0, 255, 255, 255) ;
               getPrinter().GxDrawText(StringUtil.LTrim( context.localUtil.Format( (decimal)(AV21FCKTstId), "ZZZ9")), 133, Gx_line+17, 159, Gx_line+32, 2+256, 0, 0, 0) ;
               getPrinter().GxDrawText(StringUtil.RTrim( context.localUtil.Format( AV22FCKTstDsc, "")), 183, Gx_line+17, 288, Gx_line+32, 0+256, 0, 0, 0) ;
               getPrinter().GxDrawText(StringUtil.LTrim( context.localUtil.Format( (decimal)(AV23FCKTstOtro), "ZZZ9")), 342, Gx_line+17, 368, Gx_line+32, 2+256, 0, 0, 0) ;
               getPrinter().GxDrawText(AV20FCKTstFCK, 83, Gx_line+67, 776, Gx_line+217, 3, 1, 0, 0) ;
               getPrinter().GxDrawText(AV8FCKAux, 83, Gx_line+267, 776, Gx_line+442, 3, 0, 0, 0) ;
               getPrinter().GxDrawText("Campo FCK en texto plano", 83, Gx_line+33, 366, Gx_line+47, 0, 0, 0, 0) ;
               getPrinter().GxDrawText("Campo FCK formateado con itext", 83, Gx_line+233, 383, Gx_line+247, 0, 0, 0, 0) ;
               getPrinter().GxDrawRect(75, Gx_line+50, 783, Gx_line+217, 1, 0, 0, 0, 0, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0) ;
               getPrinter().GxDrawRect(75, Gx_line+250, 783, Gx_line+450, 1, 0, 0, 0, 0, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0) ;
               getPrinter().GxDrawText("FCKTstId:", 75, Gx_line+17, 125, Gx_line+31, 0+256, 0, 0, 0) ;
               getPrinter().GxDrawText("Test Format Raw HTML", 92, Gx_line+450, 212, Gx_line+464, 0+256, 0, 0, 0) ;
               getPrinter().GxDrawText(AV14FormatRawHTML, 75, Gx_line+467, 783, Gx_line+548, 0, 1, 0, 0) ;
               Gx_OldLine = Gx_line;
               Gx_line = (int)(Gx_line+548);
               AV24GXV1 = (int)(AV24GXV1+1);
            }
            /* Print footer for last page */
            ToSkip = (int)(P_lines+1);
            H1U0( true, 0) ;
         }
         catch ( GeneXus.Printer.ProcessInterruptedException  )
         {
         }
         finally
         {
            /* Close printer file */
            try
            {
               getPrinter().GxEndPage() ;
               getPrinter().GxEndDocument() ;
            }
            catch ( GeneXus.Printer.ProcessInterruptedException  )
            {
            }
            endPrinter();
         }
         this.cleanup();
      }

      protected void H1U0( bool bFoot ,
                           int Inc )
      {
         /* Skip the required number of lines */
         while ( ( ToSkip > 0 ) || ( Gx_line + Inc > P_lines ) )
         {
            if ( Gx_line + Inc >= P_lines )
            {
               if ( Gx_page > 0 )
               {
                  /* Print footers */
                  Gx_line = P_lines;
                  getPrinter().GxEndPage() ;
                  if ( bFoot )
                  {
                     return  ;
                  }
               }
               ToSkip = 0;
               Gx_line = 0;
               Gx_page = (int)(Gx_page+1);
               /* Skip Margin Top Lines */
               Gx_line = (int)(Gx_line+(M_top*lineHeight));
               /* Print headers */
               getPrinter().GxStartPage() ;
               if (true) break;
            }
            else
            {
               Gx_line = (int)(Gx_line+1);
            }
            ToSkip = (int)(ToSkip-1);
         }
         getPrinter().setPage(Gx_page);
      }

      protected void add_metrics( )
      {
         add_metrics0( ) ;
      }

      protected void add_metrics0( )
      {
         getPrinter().setMetrics("Microsoft Sans Serif", false, false, 58, 14, 72, 171,  new int[] {48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 18, 20, 23, 36, 36, 57, 43, 12, 21, 21, 25, 37, 18, 21, 18, 18, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 18, 18, 37, 37, 37, 36, 65, 43, 43, 46, 46, 43, 39, 50, 46, 18, 32, 43, 36, 53, 46, 50, 43, 50, 46, 43, 40, 46, 43, 64, 41, 42, 39, 18, 18, 18, 27, 36, 21, 36, 36, 32, 36, 36, 18, 36, 36, 14, 15, 33, 14, 55, 36, 36, 36, 36, 21, 32, 18, 36, 33, 47, 31, 31, 31, 21, 17, 21, 37, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 18, 20, 36, 36, 36, 36, 17, 36, 21, 47, 24, 36, 37, 21, 47, 35, 26, 35, 21, 21, 21, 37, 34, 21, 21, 21, 23, 36, 53, 53, 53, 39, 43, 43, 43, 43, 43, 43, 64, 46, 43, 43, 43, 43, 18, 18, 18, 18, 46, 46, 50, 50, 50, 50, 50, 37, 50, 46, 46, 46, 46, 43, 43, 39, 36, 36, 36, 36, 36, 36, 57, 32, 36, 36, 36, 36, 18, 18, 18, 18, 36, 36, 36, 36, 36, 36, 36, 35, 39, 36, 36, 36, 36, 32, 36, 32}) ;
      }

      public override int getOutputType( )
      {
         return GxReportUtils.OUTPUT_PDF ;
      }

      public override void cleanup( )
      {
         CloseCursors();
         if (IsMain)	waitPrinterEnd();
         if ( IsMain )
         {
            context.CloseConnections();
         }
         ExitApp();
      }

      public override void initialize( )
      {
         AV16staticDir = "";
         AV17FCKTstCollection = new GXBaseCollection<SdtFCKTstCollection_FCKTst>( context, "FCKTst", "TestReportes");
         GXt_objcol_SdtFCKTstCollection_FCKTst1 = new GXBaseCollection<SdtFCKTstCollection_FCKTst>( context, "FCKTst", "TestReportes");
         AV18FCKTstItem = new SdtFCKTstCollection_FCKTst(context);
         AV8FCKAux = "";
         AV14FormatRawHTML = "";
         AV20FCKTstFCK = "";
         AV22FCKTstDsc = "";
         /* GeneXus formulas. */
         Gx_line = 0;
      }

      private short AV21FCKTstId ;
      private short AV23FCKTstOtro ;
      private int M_top ;
      private int M_bot ;
      private int ToSkip ;
      private int AV24GXV1 ;
      private int Gx_OldLine ;
      private string AV16staticDir ;
      private string AV22FCKTstDsc ;
      private string AV8FCKAux ;
      private string AV14FormatRawHTML ;
      private string AV20FCKTstFCK ;
      private GXBaseCollection<SdtFCKTstCollection_FCKTst> AV17FCKTstCollection ;
      private GXBaseCollection<SdtFCKTstCollection_FCKTst> GXt_objcol_SdtFCKTstCollection_FCKTst1 ;
      private SdtFCKTstCollection_FCKTst AV18FCKTstItem ;
   }

}
