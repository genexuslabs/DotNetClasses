/*
               File: PDFBasicTest
        Description: PDFBasic Test
             Author: GeneXus .NET Framework Generator version 17_0_8-156507
       Generated on: 12/21/2021 16:34:13.62
       Program type: Main program
          Main DBMS: SQL Server
*/
using System;
using System.IO;
using System.Threading;
using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Encryption;
using GeneXus.Procedure;
using GeneXus.Utils;
namespace GeneXus.Programs {
   public class apdfbasictest : GXProcedure
   {

      public apdfbasictest( )
      {
         context = new GxContext(  );
         DataStoreUtil.LoadDataStores( context);
         IsMain = true;
         context.SetDefaultTheme("Carmine");
      }

      public apdfbasictest( IGxContext context )
      {
         this.context = context;
         IsMain = false;
      }

      public void execute( )
      {
         initialize();
         executePrivate();
      }

      void executePrivate( )
      {
         /* GeneXus formulas */
         /* Output device settings */
         M_top = 0;
         M_bot = 6;
         P_lines = (int)(66-M_bot);
         getPrinter().GxClearAttris() ;
         AddMetrics( ) ;
         lineHeight = 15;
         gxXPage = 100;
         gxYPage = 100;
         getPrinter().GxSetDocName("Report") ;
         getPrinter().GxSetDocFormat("PDF") ;
			try
			{
				Gx_out = "FIL";
            if (!initPrinter (Gx_out, gxXPage, gxYPage, "GXPRN.INI", "", "", 2, 1, 256, 16834, 11909, 0, 1, 1, 0, 1, 1) )
				{
					cleanup();
					return;
				}
				getPrinter().setModal(false);
				P_lines = (int)(gxYPage - (lineHeight * 6));
				Gx_line = (int)(P_lines + 1);
				getPrinter().setPageLines(P_lines);
            getPrinter().setLineHeight(lineHeight);
            getPrinter().setM_top(M_top);
            getPrinter().setM_bot(M_bot);
            AV8charactervar = "Textline is the most secure business texting service for modern customer support, sales, and logistics teams.Hang up the phone, 52% of your customers want to text you.";
            AV9htmlvar = "<!DOCTYPE html><html><body><p>I am normal</p><p style=\"color:red;\">I am red</p>" + "<p style=\"color:blue;\">I am blue</p><p style=\"font-size:50px;\">I am big</p></body></html>";
            H1V0( false, 885) ;
            getPrinter().GxDrawLine(50, Gx_line+91, 367, Gx_line+91, 5, 255, 0, 0, 3) ;
            getPrinter().GxDrawRect(52, Gx_line+113, 356, Gx_line+142, 5, 255, 69, 0, 1, 255, 215, 0, 2, 4, 3, 5, 45, 45, 45, 45) ;
            getPrinter().GxAttris("Microsoft Sans Serif", 12, false, false, false, false, 0, 0, 0, 0, 0, 255, 255, 255) ;
            getPrinter().GxDrawText("The New York Times (the Times or NYT) is a daily newspaper based in New York City with a worldwide readership reported in 2022 to comprise 740,000 paid print subscribers, and 8.6 million paid digital subscribers. It also is a producer of popular podcasts such as The Daily.[4][5][6] Founded in 1851, it is published by The New York Times Company. The Times has won 132 Pulitzer Prizes, the most of any newspaper,[7] and has long been regarded as a national \"newspaper of record\".[8] For print, it is ranked 18th in the world by circulation and 3rd in the United States.[9] The newspaper is headquartered at The New York Times Building near Times Square, Manhattan.", 50, Gx_line+489, 717, Gx_line+689, 3+16, 0, 0, 1) ;
            getPrinter().GxDrawRect(389, Gx_line+111, 489, Gx_line+141, 1, 0, 0, 0, 0, 255, 255, 255, 4, 0, 0, 0, 0, 0, 0, 0) ;
            getPrinter().GxAttris("Courier New", 12, false, false, false, false, 0, 0, 0, 0, 0, 255, 255, 255) ;
            getPrinter().GxDrawText("Textblock courier new 12", 50, Gx_line+11, 228, Gx_line+30, 0, 0, 0, 0) ;
            getPrinter().GxAttris("Microsoft Sans Serif", 8, false, false, false, false, 0, 0, 0, 0, 0, 255, 255, 255) ;
            getPrinter().GxDrawText(StringUtil.RTrim( context.localUtil.Format( AV8charactervar, "")), 50, Gx_line+56, 666, Gx_line+71, 0, 0, 0, 0) ;
            getPrinter().GxDrawText(AV9htmlvar, 50, Gx_line+156, 522, Gx_line+278, 0, 1, 0, 0) ;
            getPrinter().GxAttris("Microsoft Sans Serif", 8, true, false, false, false, 0, 0, 0, 0, 1, 0, 255, 255) ;
            getPrinter().GxDrawText("BackColorRight", 50, Gx_line+311, 233, Gx_line+344, 2, 0, 0, 1) ;
            getPrinter().GxAttris("Microsoft Sans Serif", 8, false, false, false, false, 0, 0, 0, 0, 0, 255, 255, 255) ;
            getPrinter().GxDrawText(StringUtil.LTrim( context.localUtil.Format( (decimal)(Gx_page), "ZZZZZ9")), 717, Gx_line+11, 756, Gx_line+26, 2+256, 0, 0, 0) ;
            getPrinter().GxDrawText(StringUtil.RTrim( context.localUtil.Format( AV8charactervar, "")), 50, Gx_line+400, 656, Gx_line+456, 3, 0, 0, 1) ;
            getPrinter().GxDrawBitMap(Path.Combine(GxContext.StaticPhysicalPath(), @"resources\bird-thumbnail.jpg"), 483, Gx_line+322, 550, Gx_line+389) ;
            getPrinter().GxAttris("Microsoft Sans Serif", 12, true, true, true, false, 0, 0, 0, 0, 0, 255, 255, 255) ;
            getPrinter().GxDrawText("Underlined bold italic text", 50, Gx_line+711, 350, Gx_line+744, 0, 0, 0, 0) ;
            getPrinter().GxAttris("Microsoft Sans Serif", 10, true, false, false, true, 0, 0, 0, 0, 0, 255, 255, 255) ;
            getPrinter().GxDrawText("Strikethrough bold text ", 394, Gx_line+711, 694, Gx_line+744, 0, 0, 0, 0) ;
            getPrinter().GxDrawLine(556, Gx_line+400, 656, Gx_line+400, 1, 0, 0, 0, 0) ;
            getPrinter().GxDrawLine(556, Gx_line+456, 656, Gx_line+456, 1, 0, 0, 0, 0) ;
            Gx_OldLine = Gx_line;
            Gx_line = (int)(Gx_line+885);
            /* Print footer for last page */
            ToSkip = (int)(P_lines+1);
            H1V0( true, 0) ;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}

			finally
			{
				/* Close printer file */
				try
				{
					getPrinter().GxEndPage();
					getPrinter().GxEndDocument();
				}
				catch (GeneXus.Printer.ProcessInterruptedException)
				{
				}
				endPrinter();
			}
         this.cleanup();
      }

      protected void H1V0( bool bFoot ,
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

      protected void AddMetrics( )
      {
         
         Add_metrics0( ) ;
         Add_metrics1( ) ;
         Add_metrics2( ) ;
         Add_metrics3( ) ;
	  }

      protected void Add_metrics0( )
      {
         getPrinter().setMetrics("Microsoft Sans Serif", false, false, 58, 14, 72, 171,  new int[] {48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 18, 20, 23, 36, 36, 57, 43, 12, 21, 21, 25, 37, 18, 21, 18, 18, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 18, 18, 37, 37, 37, 36, 65, 43, 43, 46, 46, 43, 39, 50, 46, 18, 32, 43, 36, 53, 46, 50, 43, 50, 46, 43, 40, 46, 43, 64, 41, 42, 39, 18, 18, 18, 27, 36, 21, 36, 36, 32, 36, 36, 18, 36, 36, 14, 15, 33, 14, 55, 36, 36, 36, 36, 21, 32, 18, 36, 33, 47, 31, 31, 31, 21, 17, 21, 37, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 18, 20, 36, 36, 36, 36, 17, 36, 21, 47, 24, 36, 37, 21, 47, 35, 26, 35, 21, 21, 21, 37, 34, 21, 21, 21, 23, 36, 53, 53, 53, 39, 43, 43, 43, 43, 43, 43, 64, 46, 43, 43, 43, 43, 18, 18, 18, 18, 46, 46, 50, 50, 50, 50, 50, 37, 50, 46, 46, 46, 46, 43, 43, 39, 36, 36, 36, 36, 36, 36, 57, 32, 36, 36, 36, 36, 18, 18, 18, 18, 36, 36, 36, 36, 36, 36, 36, 35, 39, 36, 36, 36, 36, 32, 36, 32}) ;
      }

      protected void Add_metrics1( )
      {
         getPrinter().setMetrics("Courier New", false, false, 58, 14, 72, 171,  new int[] {48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 18, 20, 23, 36, 36, 57, 43, 12, 21, 21, 25, 37, 18, 21, 18, 18, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 18, 18, 37, 37, 37, 36, 65, 43, 43, 46, 46, 43, 39, 50, 46, 18, 32, 43, 36, 53, 46, 50, 43, 50, 46, 43, 40, 46, 43, 64, 41, 42, 39, 18, 18, 18, 27, 36, 21, 36, 36, 32, 36, 36, 18, 36, 36, 14, 15, 33, 14, 55, 36, 36, 36, 36, 21, 32, 18, 36, 33, 47, 31, 31, 31, 21, 17, 21, 37, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 18, 20, 36, 36, 36, 36, 17, 36, 21, 47, 24, 36, 37, 21, 47, 35, 26, 35, 21, 21, 21, 37, 34, 21, 21, 21, 23, 36, 53, 53, 53, 39, 43, 43, 43, 43, 43, 43, 64, 46, 43, 43, 43, 43, 18, 18, 18, 18, 46, 46, 50, 50, 50, 50, 50, 37, 50, 46, 46, 46, 46, 43, 43, 39, 36, 36, 36, 36, 36, 36, 57, 32, 36, 36, 36, 36, 18, 18, 18, 18, 36, 36, 36, 36, 36, 36, 36, 35, 39, 36, 36, 36, 36, 32, 36, 32}) ;
      }

      protected void Add_metrics2( )
      {
         getPrinter().setMetrics("Microsoft Sans Serif", true, true, 58, 14, 72, 123,  new int[] {47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 18, 21, 30, 35, 35, 55, 45, 14, 21, 21, 25, 37, 18, 21, 18, 18, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 21, 21, 37, 37, 37, 38, 61, 45, 45, 45, 45, 42, 38, 49, 45, 17, 35, 45, 38, 52, 45, 49, 42, 49, 45, 42, 38, 45, 42, 59, 42, 42, 38, 21, 18, 23, 37, 35, 21, 35, 38, 35, 38, 35, 21, 38, 38, 18, 18, 35, 18, 56, 38, 38, 38, 38, 25, 35, 21, 38, 35, 49, 35, 35, 32, 25, 17, 25, 37, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 18, 21, 36, 35, 35, 35, 17, 35, 21, 46, 23, 35, 37, 21, 46, 35, 25, 35, 21, 21, 21, 36, 35, 21, 21, 21, 23, 35, 53, 53, 53, 38, 45, 45, 45, 45, 45, 45, 63, 45, 42, 42, 42, 42, 18, 18, 18, 18, 45, 45, 49, 49, 49, 49, 49, 37, 49, 45, 45, 45, 45, 42, 42, 38, 35, 35, 35, 35, 35, 35, 56, 35, 35, 35, 35, 35, 18, 18, 18, 18, 38, 38, 38, 38, 38, 38, 38, 35, 38, 38, 38, 38, 38, 35, 38, 35}) ;
      }

      protected void Add_metrics3( )
      {
         getPrinter().setMetrics("Microsoft Sans Serif", true, false, 57, 15, 72, 163,  new int[] {47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 17, 19, 29, 34, 34, 55, 45, 15, 21, 21, 24, 36, 17, 21, 17, 17, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 21, 21, 36, 36, 36, 38, 60, 43, 45, 45, 45, 41, 38, 48, 45, 17, 34, 45, 38, 53, 45, 48, 41, 48, 45, 41, 38, 45, 41, 57, 41, 41, 38, 21, 17, 21, 36, 34, 21, 34, 38, 34, 38, 34, 21, 38, 38, 17, 17, 34, 17, 55, 38, 38, 38, 38, 24, 34, 21, 38, 33, 49, 34, 34, 31, 24, 17, 24, 36, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 17, 21, 34, 34, 34, 34, 17, 34, 21, 46, 23, 34, 36, 21, 46, 34, 25, 34, 21, 21, 21, 36, 34, 21, 20, 21, 23, 34, 52, 52, 52, 38, 45, 45, 45, 45, 45, 45, 62, 45, 41, 41, 41, 41, 17, 17, 17, 17, 45, 45, 48, 48, 48, 48, 48, 36, 48, 45, 45, 45, 45, 41, 41, 38, 34, 34, 34, 34, 34, 34, 55, 34, 34, 34, 34, 34, 17, 17, 17, 17, 38, 38, 38, 38, 38, 38, 38, 34, 38, 38, 38, 38, 38, 34, 38, 34}) ;
      }

      public override int getOutputType( )
      {
         return GxReportUtils.OUTPUT_PDF ;
      }

      public override void cleanup( )
      {
         if (IsMain)	waitPrinterEnd();
         if ( IsMain )
         {
            context.CloseConnections();
         }
         ExitApp();
      }

      public override void initialize( )
      {
         AV8charactervar = "";
         AV9htmlvar = "";
         /* GeneXus formulas. */
         Gx_line = 0;
      }

      private int M_top ;
      private int M_bot ;
      private int ToSkip ;
      private int Gx_OldLine ;
      private string AV8charactervar ;
      private string AV9htmlvar ;
   }

}
