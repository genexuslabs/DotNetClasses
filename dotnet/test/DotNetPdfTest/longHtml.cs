using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Procedure;
namespace GeneXus.Programs
{
	public class longHtml : GXProcedure
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

      public longHtml( )
      {
         context = new GxContext(  );
         DataStoreUtil.LoadDataStores( context);
         IsMain = true;
         context.SetDefaultTheme("Carmine", false);
      }

      public longHtml( IGxContext context )
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
         getPrinter().GxSetDocName("longHtmlReport.pdf") ;
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
            AV8var = "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">A su solicitud hemos realizado una <strong>ANGIOGRAFÍA FLUORESCEINICA Y TOMOGRAFÍA DE COHERENCIA ÓPTICA LÁSER SPECTRALIS (OCT) </strong>en forma simultánea.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\"><strong>ESPECIFICACIONES ANGIOGRAFÍA&nbsp;</strong><br />";
            AV8var += "Para el registro de la fluorescencia se utilizó un Láser de Argón 480nm. Las imágenes fueron registradas con una cadencia de 6 FPS, durante un período de aproximadamente 3 minutos, 20 minutos después se hizo una toma tardía final.&nbsp;<br />";
            AV8var += "El angiógrafo HRA2 permite además estudio no invasivo con presentación de imágenes con técnica de autofluorescencia, red free e infrarrojo. Las imágenes se pueden obtener en forma simultánea o por separado.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\"><strong>ESPECIFICACIONES OCT SPECTRALIS&nbsp;</strong><br />";
            AV8var += "Para registro de OCT Spectralis se utilizó un tomógrafo Heidelberg de última generación.<br />";
            AV8var += "La iluminación se hace con un láser diodo.&nbsp;<br />";
            AV8var += "La velocidad es de 40.000 scans/segundo.<br />";
            AV8var += "El examen se puede realizar en escalas de colores o en escalas de grises, este permite mayor resolución.&nbsp;<br />";
            AV8var += "Resolución axial de 3.9 micras digital.<br />";
            AV8var += "Los medios de presentación de las imágenes pueden ser: OCT &ndash; Angiofluoresceinografia &ndash; Indocianinografía &ndash; Autofluorescencia &ndash; Infrarrojo &ndash; red free. Las imágenes se hacen en forma simultánea.&nbsp;<br />";
            AV8var += "Permite estudio de evolución enfocando exactamente y en forma automática los exámenes sucesivos en el lugar del primer corte.</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Medios oculares:&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp;FOTO COLOR A.O.:&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">INICIO.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Signos de retinopatía diabética caracterizados por microaneurismas, micro hemorragias, hemorragias y exudados, algunos de ellos circinados.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">FIN.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp;&nbsp; &nbsp;ANGIOGRAFÍA A.O.:&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">A partir de microaneurismas se observa hiperfluorescencia por filtración que aumenta en toma tardía a nivel de polos posteriores con caracteres de edema macular diabético.&nbsp;<br />";
            AV8var += "Se observan áreas hipofluorescentes de isquemia y no perfusión capilar a predominio en&hellip;&hellip;&hellip;&hellip;&hellip;&hellip;&hellip;&hellip;&hellip;&hellip;&hellip;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">Se aconseja control si disminuye AV para evaluar inicio de fotocoagulación.&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">El paciente debe concurrir con la angiografía original (no la fotocopia, ni el informe), para dicho tratamiento. //sp//</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp;&nbsp; &nbsp;OJO DERECHO:</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp;&nbsp; &nbsp;-Cámara vítrea:</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp; -Interfase vítreo-retiniana:</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp; -Corte foveolar:&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp; -Estructura retiniana:</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp; -Complejo EP-CC:&nbsp;</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp; OJO IZQUIERDO:</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp; -Cámara vítrea:</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp; -Interfase vítreo-retiniana:</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp; -Corte foveolar:</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp; -Estructura retiniana:</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp; -Complejo EP-CC:&nbsp;</span></em></span><br />";
            AV8var += "&nbsp;</p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp; OJO LOS OJO:</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp; -Cámara vítrea:</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp; -Interfase vítreo-retiniana:</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp; -Corte foveolar:</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp; -Estructura retiniana:</span></em></span></p>";
            AV8var += "<p><span style=\"font-family:times new roman,times,serif\"><em><span style=\"font-size:12px\">&nbsp; &nbsp; -Complejo EP-CC:&nbsp;</span></em></span><br />";
            AV8var += "&nbsp;</p>";
            H1V0( false, 3524) ;
            getPrinter().GxAttris("Microsoft Sans Serif", 8, false, false, false, false, 0, 0, 0, 0, 0, 255, 255, 255) ;
            getPrinter().GxDrawText(AV8var, 42, Gx_line+17, 817, Gx_line+3524, 0, 1, 0, 0) ;
            Gx_line = (int)(Gx_line+3524);
            /* Print footer for last page */
            ToSkip = (int)(P_lines+1);
            H1V0( true, 0) ;
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
         AV8var = "";
         /* GeneXus formulas. */
         Gx_line = 0;
      }

      private int M_top ;
      private int M_bot ;
      private int ToSkip ;
      private string AV8var ;
   }

}
