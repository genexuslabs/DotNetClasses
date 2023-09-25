using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Procedure;
namespace GeneXus.Programs
{
	public class apdfformat : GXProcedure
	{
		public int executeCmdLine(string[] args)
		{
			return ExecuteCmdLine(args); ;
		}

		protected override int ExecuteCmdLine(string[] args)
		{
			execute();
			return GX.GXRuntime.ExitCode;
		}

		public apdfformat()
		{
			context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
			IsMain = true;
			context.SetDefaultTheme("Carmine", false);
		}

		public apdfformat(IGxContext context)
		{
			this.context = context;
			IsMain = false;
		}

		public void execute()
		{
			initialize();
			ExecutePrivate();
		}

		public void executeSubmit()
		{
			SubmitImpl();
		}

		protected override void ExecutePrivate()
		{
			/* GeneXus formulas */
			/* Output device settings */
			M_top = 0;
			M_bot = 6;
			P_lines = (int)(66 - M_bot);
			getPrinter().GxClearAttris();
			add_metrics();
			lineHeight = 15;
			gxXPage = 100;
			gxYPage = 100;
			getPrinter().GxSetDocName("PDFFormat");
			getPrinter().GxSetDocFormat("PDF");
			try
			{
				Gx_out = "FIL";
				if (!initPrinter(Gx_out, gxXPage, gxYPage, "GXPRN.INI", "", "", 2, 1, 256, 16834, 11909, 0, 1, 1, 0, 1, 1))
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
            AV8FCKAux = "tabla basica";
            AV9FormatRawHTML = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"1\" style=\"width: 500px;\">	<caption>Tabla basica</caption><thead><tr>" + "<th scope=\"col\"><span style=\"color: red;\">row 1 cell 1 bold rojo</span></th><th scope=\"col\"><span style=\"color: blue;\">row1 cell 2 bold azul</span></th>" + "</tr></thead><tbody><tr><td>row 2 cell 1</td><td>row2 cell 2</td></tr><tr>	<td>row 3 cell 1</td><td>row 3 cell 2</td></tr></tbody>" + "</table><p>&nbsp;</p>";
            H1T0( false, 410) ;
            getPrinter().GxAttris("Microsoft Sans Serif", 8, false, false, false, false, 0, 0, 0, 0, 0, 255, 255, 255) ;
            getPrinter().GxDrawText(AV9FormatRawHTML, 61, Gx_line+89, 769, Gx_line+356, 0, 1, 0, 0) ;
            getPrinter().GxDrawRect(61, Gx_line+11, 769, Gx_line+78, 1, 0, 0, 0, 0, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0) ;
            getPrinter().GxDrawText(AV8FCKAux, 67, Gx_line+22, 760, Gx_line+66, 3, 0, 0, 0) ;
            Gx_line = (int)(Gx_line+410);
            AV8FCKAux = "lista de elementos";
            AV9FormatRawHTML = "<ol><li>Coffee</li><li>Milk</li></ol>";
            H1T0( false, 410) ;
            getPrinter().GxAttris("Microsoft Sans Serif", 8, false, false, false, false, 0, 0, 0, 0, 0, 255, 255, 255) ;
            getPrinter().GxDrawText(AV9FormatRawHTML, 61, Gx_line+89, 769, Gx_line+356, 0, 1, 0, 0) ;
            getPrinter().GxDrawRect(61, Gx_line+11, 769, Gx_line+78, 1, 0, 0, 0, 0, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0) ;
            getPrinter().GxDrawText(AV8FCKAux, 67, Gx_line+22, 760, Gx_line+66, 3, 0, 0, 0) ;
            Gx_line = (int)(Gx_line+410);
            AV8FCKAux = "parrafos";
            AV9FormatRawHTML = "<p>This is a paragraph 1<p>This is a paragraph 2";
            H1T0( false, 410) ;
            getPrinter().GxAttris("Microsoft Sans Serif", 8, false, false, false, false, 0, 0, 0, 0, 0, 255, 255, 255) ;
            getPrinter().GxDrawText(AV9FormatRawHTML, 61, Gx_line+89, 769, Gx_line+356, 0, 1, 0, 0) ;
            getPrinter().GxDrawRect(61, Gx_line+11, 769, Gx_line+78, 1, 0, 0, 0, 0, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0) ;
            getPrinter().GxDrawText(AV8FCKAux, 67, Gx_line+22, 760, Gx_line+66, 3, 0, 0, 0) ;
            Gx_line = (int)(Gx_line+410);
            AV8FCKAux = "horizontal rule";
            AV9FormatRawHTML = "<html><body><p>The hr tag defines a horizontal rule:</p><hr /><p>This is a paragraph</p><hr /><p>This is a paragraph</p>" + "<hr /><p>This is a paragraph</p></body></html>";
            H1T0( false, 410) ;
            getPrinter().GxAttris("Microsoft Sans Serif", 8, false, false, false, false, 0, 0, 0, 0, 0, 255, 255, 255) ;
            getPrinter().GxDrawText(AV9FormatRawHTML, 61, Gx_line+89, 769, Gx_line+356, 0, 1, 0, 0) ;
            getPrinter().GxDrawRect(61, Gx_line+11, 769, Gx_line+78, 1, 0, 0, 0, 0, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0) ;
            getPrinter().GxDrawText(AV8FCKAux, 67, Gx_line+22, 760, Gx_line+66, 3, 0, 0, 0) ;
            Gx_line = (int)(Gx_line+410);
            AV8FCKAux = "fonts";
            AV9FormatRawHTML = "<html><body><h1 style=\"font-family:verdana;\">A heading</h1><p style=\"font-family:arial;color:red;font-size:20px;\">A paragraph.</p>" + "</body></html>";
            H1T0( false, 410) ;
            getPrinter().GxAttris("Microsoft Sans Serif", 8, false, false, false, false, 0, 0, 0, 0, 0, 255, 255, 255) ;
            getPrinter().GxDrawText(AV9FormatRawHTML, 61, Gx_line+89, 769, Gx_line+356, 0, 1, 0, 0) ;
            getPrinter().GxDrawRect(61, Gx_line+11, 769, Gx_line+78, 1, 0, 0, 0, 0, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0) ;
            getPrinter().GxDrawText(AV8FCKAux, 67, Gx_line+22, 760, Gx_line+66, 3, 0, 0, 0) ;
            Gx_line = (int)(Gx_line+410);
            AV8FCKAux = "nested lists";
            AV9FormatRawHTML = "<html><body><h4>A nested List:</h4><ul>  <li>Coffee</li>  <li>Tea    <ul>    <li>Black tea</li>    <li>Green tea" + "  <ul>      <li>China</li>      <li>Africa</li>      </ul>    </li>    </ul>  </li>  <li>Milk</li></ul></body></html>";
            H1T0( false, 410) ;
            getPrinter().GxAttris("Microsoft Sans Serif", 8, false, false, false, false, 0, 0, 0, 0, 0, 255, 255, 255) ;
            getPrinter().GxDrawText(AV9FormatRawHTML, 61, Gx_line+89, 769, Gx_line+356, 0, 1, 0, 0) ;
            getPrinter().GxDrawRect(61, Gx_line+11, 769, Gx_line+78, 1, 0, 0, 0, 0, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0) ;
            getPrinter().GxDrawText(AV8FCKAux, 67, Gx_line+22, 760, Gx_line+66, 3, 0, 0, 0) ;
            Gx_line = (int)(Gx_line+410);
            AV8FCKAux = "radio buttons";
            AV9FormatRawHTML = "<html><body><form action=\"\"><input type=\"radio\" name=\"sex\" value=\"male\" /> Male<br /><input type=\"radio\" name=\"sex\" value=\"female\" /> Female" + "<input type=\"checkbox\" name=\"vehicle\" value=\"Bike\" /> I have a bike<br /><input type=\"checkbox\" name=\"vehicle\" value=\"Car\" /> I have a car </form>" + "</body></html>";
            H1T0( false, 410) ;
            getPrinter().GxAttris("Microsoft Sans Serif", 8, false, false, false, false, 0, 0, 0, 0, 0, 255, 255, 255) ;
            getPrinter().GxDrawText(AV9FormatRawHTML, 61, Gx_line+89, 769, Gx_line+356, 0, 1, 0, 0) ;
            getPrinter().GxDrawRect(61, Gx_line+11, 769, Gx_line+78, 1, 0, 0, 0, 0, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0) ;
            getPrinter().GxDrawText(AV8FCKAux, 67, Gx_line+22, 760, Gx_line+66, 3, 0, 0, 0) ;
            Gx_line = (int)(Gx_line+410);
            /* Print footer for last page */
				ToSkip = (int)(P_lines + 1);
				H1T0(true, 0);
			}
			catch (GeneXus.Printer.ProcessInterruptedException)
			{
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

		protected void H1T0(bool bFoot,
							 int Inc)
		{
			/* Skip the required number of lines */
			while ((ToSkip > 0) || (Gx_line + Inc > P_lines))
			{
				if (Gx_line + Inc >= P_lines)
				{
					if (Gx_page > 0)
					{
						/* Print footers */
						Gx_line = P_lines;
						getPrinter().GxEndPage();
						if (bFoot)
						{
							return;
						}
					}
					ToSkip = 0;
					Gx_line = 0;
					Gx_page = (int)(Gx_page + 1);
					/* Skip Margin Top Lines */
					Gx_line = (int)(Gx_line + (M_top * lineHeight));
					/* Print headers */
					getPrinter().GxStartPage();
					if (true) break;
				}
				else
				{
					Gx_line = (int)(Gx_line + 1);
				}
				ToSkip = (int)(ToSkip - 1);
			}
			getPrinter().setPage(Gx_page);
		}

		protected void add_metrics()
		{
			add_metrics0();
		}

		protected void add_metrics0()
		{
			getPrinter().setMetrics("Microsoft Sans Serif", false, false, 58, 14, 72, 171, new int[] { 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 18, 20, 23, 36, 36, 57, 43, 12, 21, 21, 25, 37, 18, 21, 18, 18, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 18, 18, 37, 37, 37, 36, 65, 43, 43, 46, 46, 43, 39, 50, 46, 18, 32, 43, 36, 53, 46, 50, 43, 50, 46, 43, 40, 46, 43, 64, 41, 42, 39, 18, 18, 18, 27, 36, 21, 36, 36, 32, 36, 36, 18, 36, 36, 14, 15, 33, 14, 55, 36, 36, 36, 36, 21, 32, 18, 36, 33, 47, 31, 31, 31, 21, 17, 21, 37, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 18, 20, 36, 36, 36, 36, 17, 36, 21, 47, 24, 36, 37, 21, 47, 35, 26, 35, 21, 21, 21, 37, 34, 21, 21, 21, 23, 36, 53, 53, 53, 39, 43, 43, 43, 43, 43, 43, 64, 46, 43, 43, 43, 43, 18, 18, 18, 18, 46, 46, 50, 50, 50, 50, 50, 37, 50, 46, 46, 46, 46, 43, 43, 39, 36, 36, 36, 36, 36, 36, 57, 32, 36, 36, 36, 36, 18, 18, 18, 18, 36, 36, 36, 36, 36, 36, 36, 35, 39, 36, 36, 36, 36, 32, 36, 32 });
		}

		public override int getOutputType()
		{
			return GxReportUtils.OUTPUT_PDF;
		}

		public override void cleanup()
		{
			CloseCursors();
			if (IsMain) waitPrinterEnd();
			if (IsMain)
			{
				context.CloseConnections();
			}
			ExitApp();
		}

		public override void initialize()
		{
			AV8FCKAux = "";
			AV9FormatRawHTML = "";
			/* GeneXus formulas. */
			Gx_line = 0;
		}

		private int M_top;
		private int M_bot;

		private int ToSkip;


		private string AV8FCKAux;
		private string AV9FormatRawHTML;
	}

}
