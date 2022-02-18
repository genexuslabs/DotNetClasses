namespace GeneXus.Procedure
{
	using System;
	using GeneXus.Configuration;
	using GeneXus.Printer;
	using System.Globalization;
	using GeneXus.Http;
	using System.Threading;
	using GeneXus.Mime;

	public class GXWebProcedure : GXHttpHandler
	{
		protected int handle;

		protected GXReportMetadata reportMetadata;
		protected IReportHandler reportHandler;
		protected IReportHandler oldReportHandler;

		protected int lineHeight;
		protected int Gx_line;
		protected int P_lines;
		protected int gxXPage;
		protected int gxYPage;
		protected int Gx_page;
		protected string Gx_dev = "";
		protected String Gx_out = "";

		protected virtual void printHeaders() { }
		protected virtual void printFooters() { }

		public override void webExecute() { }
		public override void initialize() { }
		protected override void createObjects() { }
		public override void skipLines(long nToSkip) { }

		public override void cleanup()
		{
			if (!context.WillRedirect() && context.IsLocalStorageSupported())
			{
				context.DeleteReferer();
			}
		}

		public void setContextReportHandler()
		{

			oldReportHandler = null;
			reportHandler = context.reportHandler;
		}
		public IReportHandler getPrinter()
		{
			if (reportHandler == null)
			{
				oldReportHandler = reportHandler;
				setContentType();
#if NETCORE
				reportHandler = GxReportUtils.GetPrinter(getOutputType(), context.GetPhysicalPath(), context.HttpContext.Response.Body);
#else
				reportHandler = GxReportUtils.GetPrinter(getOutputType(), context.GetPhysicalPath(), context.HttpContext.Response.OutputStream);
#endif
				context.reportHandler = reportHandler;
			}
			return reportHandler;
		}
		void setContentType()
		{
			if (getOutputType() == GxReportUtils.OUTPUT_PDF) { 
				context.HttpContext.Response.ContentType = MediaTypesNames.ApplicationPdf;
			}
			else
				context.HttpContext.Response.ContentType = "text/richtext";
		}
		protected override void sendCacheHeaders()
		{
			string utcNow = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH':'mm':'ss 'GMT'", CultureInfo.GetCultureInfo("en-US"));
			if (string.IsNullOrEmpty(context.GetHeader(HttpHeader.CONTENT_DISPOSITION)))
			{
				context.HttpContext.Response.AddHeader(HttpHeader.CONTENT_DISPOSITION, "inline; filename=" + GetType().Name + ".pdf");
			}
			if (string.IsNullOrEmpty(context.GetHeader(HttpHeader.EXPIRES)))
			{
				localHttpContext.Response.AddHeader(HttpHeader.EXPIRES, utcNow);
			}
			if (string.IsNullOrEmpty(context.GetHeader(HttpHeader.LAST_MODIFIED)))
			{
				localHttpContext.Response.AddHeader(HttpHeader.LAST_MODIFIED, utcNow);
			}
			if (string.IsNullOrEmpty(context.GetHeader(HttpHeader.CACHE_CONTROL)))
			{
				if (getOutputType() == GxReportUtils.OUTPUT_PDF)
				{
					localHttpContext.Response.AddHeader(HttpHeader.CACHE_CONTROL, "must-revalidate,post-check=0, pre-check=0");
					//These headers are set by a Reader X bug that causes the report to not be seen in IE when embedded,
					// only seen after doing F5.
				}
				else
				{
					localHttpContext.Response.AddHeader(HttpHeader.CACHE_CONTROL, HttpHelper.CACHE_CONTROL_HEADER_NO_CACHE_REVALIDATE);
				}
			}
		}
		public virtual int getOutputType()
		{
			return GxReportUtils.GetOutputType();
		}
		protected bool initPrinter(String output, int gxXPage, int gxYPage, String iniFile, String form, String printer, int mode, int orientation, int pageSize, int pageLength, int pageWidth, int scale, int copies, int defSrc, int quality, int color, int duplex)
		{
			string idiom;
			if (!Config.GetValueOf("LANGUAGE", out idiom))
				idiom = "eng";
			getPrinter().GxRVSetLanguage(idiom);
			int xPage = gxXPage;
			int yPage = gxYPage;
			bool ret = getPrinter().GxPrintInit(output, ref xPage, ref yPage, iniFile, form, printer, mode, orientation, pageSize, pageLength, pageWidth, scale, copies, defSrc, quality, color, duplex);
			this.gxXPage = xPage;
			this.gxYPage = yPage;
			return ret;
		}
		protected void endPrinter()
		{
			try
			{
				getPrinter().GxEndPrinter();
				waitPrinterEnd();
			}
			catch
			{
			}
			context.reportHandler = oldReportHandler;
		}
		protected virtual void waitPrinterEnd()
		{
			
		}
		public bool doAsk()
		{
			return true;
		}
		public virtual bool isRemoteProcedure()
		{
			return true;
		}

		protected override bool IsSpaSupported()
		{
			return false;
		}

		protected void loadReportMetadata(String name)
		{
			reportMetadata = new GXReportMetadata(name, getPrinter());
			reportMetadata.load();
		}

		protected int GxDrawDynamicGetPrintBlockHeight(int printBlock)
		{
			return reportMetadata.GxDrawGetPrintBlockHeight(printBlock);
		}

		protected void GxDrawDynamicText(int printBlock, int controlId, int line)
		{
			reportMetadata.GxDrawText(printBlock, controlId, line);
		}

		protected void GxDrawDynamicText(int printBlock, int controlId, string value, int line)
		{
			reportMetadata.GxDrawText(printBlock, controlId, line, value);
		}

		protected void GxDrawDynamicLine(int printBlock, int controlId, int line)
		{
			reportMetadata.GxDrawLine(printBlock, controlId, line);
		}

		protected void GxDrawDynamicRect(int printBlock, int controlId, int line)
		{
			reportMetadata.GxDrawRect(printBlock, controlId, line);
		}

		protected void GxDrawDynamicBitMap(int printBlock, int controlId, string value, int line)
		{
			reportMetadata.GxDrawBitMap(printBlock, controlId, line, value, 0);
		}

		protected void GxDrawDynamicBitMap(int printBlock, int controlId, string value, int aspectRatio, int line)
		{
			reportMetadata.GxDrawBitMap(printBlock, controlId, line, value, aspectRatio);
		}

		protected static WaitCallback PropagateCulture(WaitCallback action)
		{
			return GXProcedure.PropagateCulture(action);
		}
	}
}