using System.Threading;

namespace GeneXus.Procedure
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using GeneXus.Application;
	using GeneXus.Configuration;
	using GeneXus.Data;
	using GeneXus.Metadata;
	using GeneXus.Performance;
	using GeneXus.Printer;
	using GeneXus.Utils;
	using GeneXus.XML;

	public abstract class GXProcedure: GXBaseObject
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
		protected string Gx_out = "";
        protected string Gx_docfmt = "";
        protected string Gx_docname = "";
        
		public const int IN_NEW_UTL = -2;
		private bool disconnectUserAtCleanup;
#if !NETCORE
		private DateTime beginExecute;
		private ProcedureInfo pInfo;
#endif
		public GXProcedure()
		{
#if !NETCORE
			if (Preferences.Instrumented)
			{
				string name = this.GetType().ToString();
				beginExecute = DateTime.Now;
				pInfo = ProceduresInfo.addProcedureInfo(name);
				pInfo.incCount();
			}
#endif
		}
		protected int MainImplEx(string[] args)
		{
			try
			{
				Config.ParseArgs(ref args);
				return ExecuteCmdLine(args);
			}
			catch (Exception ex)
			{
				return GXUtil.HandleException(ex, Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location), args); ;
			}
		}

		protected int MainImpl(string[] args)
		{
			try
			{
				Config.ParseArgs(ref args);
				return ExecuteCmdLine(args);
			}
			catch (Exception e)
			{
				GXUtil.SaveToEventLog("Design", e);
				Console.WriteLine(e.ToString());
				return 1;
			}
		}
		protected virtual int ExecuteCmdLine(string[] args)
		{
			initialize();
			ExecutePrivate();
			return GX.GXRuntime.ExitCode;
		}
		public override void cleanup()
		{
			CloseCursors();
			if (IsMain)
			{
				context.CloseConnections();
			}
			ExitApp();
		}
		public bool DisconnectAtCleanup
		{
			get{ return disconnectUserAtCleanup;}
			set{ disconnectUserAtCleanup=value;}
		}
		protected void ExitApp()
		{
			exitApplication(BatchCursorHolder());
		}
		protected void exitApplication()
		{
			exitApplication(true);
		}
		private void exitApplication(bool flushBatchCursor)
		{
			if (!(GxContext.IsHttpContext || GxContext.IsRestService) && IsMain && GxApplication.MainContext==context)
				ThreadUtil.WaitForEnd();

			if (flushBatchCursor)
			{
				foreach (IGxDataStore ds in context.DataStores)
					ds.Connection.FlushBatchCursors(this);
			}

			if (IsMain)
				dbgInfo?.OnExit();
			if (disconnectUserAtCleanup)
			{
				try
				{
					context.Disconnect();
				}
				catch(Exception){ ; }
			}
#if !NETCORE
			if (Preferences.Instrumented)
			{
				pInfo.setTimeExecute(DateTime.Now.Subtract(beginExecute).Ticks / TimeSpan.TicksPerMillisecond);
			}
#endif
			
		}
		protected virtual bool BatchCursorHolder() { return false; }
		protected virtual void printHeaders(){}
		protected virtual void printFooters(){}

		public msglist GX_msglist 
		{
			get	{ return context.GX_msglist ; }
			set	{ context.GX_msglist = value;}
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
                reportHandler = GxReportUtils.GetPrinter(getOutputType(), context.GetPhysicalPath(), null);
                context.reportHandler = reportHandler;
            }
            return reportHandler;
        }
        
		public static short openGXReport(String document, IGxContext ctx)
		{
			IReportHandler reportHandler=ctx.reportHandler; 
			if (reportHandler==null)
			{
				reportHandler = GxReportUtils.GetPrinter( 
					GxReportUtils.OUTPUT_RVIEWER_DLL, 
					ctx.GetPhysicalPath(), 
					null);
				ctx.reportHandler = reportHandler;
			}
			reportHandler.GxClearAttris(); 
			bool opened = reportHandler.GxOpenDoc(document);
			if (!ctx.isRemoteGXDB())
			{
				reportHandler.GxRptSilentMode();
			}
			
			if (opened)
			{
				while (reportHandler.GxIsAlive())
				{
					try
					{
						Thread.Sleep(200);
					}
					catch(Exception) { ; }
				}
				reportHandler.GxShutdown();
			}
			return 0;
		}
        

		public virtual int getOutputType()
		{
			return GxReportUtils.GetOutputType();
		}

        protected void SetPrintAtClient()
        {
            SetPrintAtClient("");
        }

		protected void SetPrintAtClient(string printerRule)
		{
            string fileExtension = "txt";
            if (getOutputType() == GxReportUtils.OUTPUT_RVIEWER_DLL)
            {
                fileExtension = "gxr";
            }
            if (getOutputType() == GxReportUtils.OUTPUT_PDF)
            {
                fileExtension = "pdf";
            }
            string fileName = FileUtil.getTempFileName(Preferences.getBLOB_PATH(), "clientReport", fileExtension);
			getPrinter().GxSetDocName(fileName);
            getPrinter().GxSetDocFormat(fileExtension);
			context.PrintReportAtClient(fileName, printerRule);
			GXFileWatcher.Instance.AddTemporaryFile(new GxFile(Preferences.getBLOB_PATH(), new GxFileInfo(fileName, Preferences.getBLOB_PATH())), context);
		}

		protected bool initPrinter(String outputTo, int gxXPage, int gxYPage, string iniFile, string form, string printer, int mode, int orientation, int pageSize, int pageLength, int pageWidth, int scale, int copies, int defSrc, int quality, int color, int duplex)
		{
			string idiom;
			if (!Config.GetValueOf("LANGUAGE", out idiom))
				idiom = "eng";
			getPrinter().GxRVSetLanguage( idiom); 
			int xPage = gxXPage;
			int yPage = gxYPage;
			bool ret = getPrinter().GxPrintInit(outputTo, ref xPage, ref yPage, iniFile, form, printer, mode, orientation, pageSize, pageLength, pageWidth, scale, copies, defSrc, quality, color, duplex);
			this. gxXPage = xPage;
			this. gxYPage = yPage;
			return ret;
		}
		protected bool initTextPrinter(string output, int gxXPage, int gxYPage, string iniFile, string form, string printer, int mode, int paperLength, int paperWidth, int gridX, int gridY, int pageLines)
		{
            string idiom;
			if (!Config.GetValueOf("LANGUAGE", out idiom))
                idiom = "eng";
            getPrinter().GxRVSetLanguage(idiom);
            int xPage = gxXPage;
            int yPage = gxYPage;
            bool ret = getPrinter().GxPrTextInit(output, ref xPage, ref yPage, iniFile, form, printer, mode, paperLength, paperWidth, gridX, gridY, pageLines);
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

		private XMLPrefixes currentNamespacePrefixes = new XMLPrefixes();

		public void SetNamedPrefixesFromReader(GXXMLReader rdr)
		{
			currentNamespacePrefixes.SetNamedPrefixesFromReader(rdr);
		}
		public void SetPrefixesFromReader(GXXMLReader rdr)
		{
			currentNamespacePrefixes.SetPrefixesFromReader(rdr);
		}
		public Dictionary<string, string> GetPrefixesInContext()
		{
			return currentNamespacePrefixes.GetPrefixes();
		}
		public void SetPrefixes(Dictionary<string, string> pfxs)
		{
			currentNamespacePrefixes.SetPrefixes(new Dictionary<string, string>(pfxs));
		}
		public override void CallWebObject(string url)
		{
			context.wjLoc = url;
		}

		private Diagnostics.GXDebugInfo dbgInfo;
		protected void initialize(int objClass, int objId, int dbgLines, long hash)
		{
			dbgInfo = Diagnostics.GXDebugManager.Instance?.GetDbgInfo(context, objClass, objId, dbgLines, hash);
		}

		protected void trkCleanup() => dbgInfo?.OnCleanup();

		protected void trk(int lineNro) => dbgInfo?.Trk(lineNro);
		protected void trk(int lineNro, int colNro) => dbgInfo?.Trk(lineNro, colNro);
		protected void trkrng(int lineNro, int lineNro2) => dbgInfo?.TrkRng(lineNro, 0, lineNro2, 0);
		protected void trkrng(int lineNro, int colNro, int lineNro2, int colNro2) => dbgInfo?.TrkRng(lineNro, colNro, lineNro2, colNro2);
	}
	public class GXDataGridProcedure : GXProcedure
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<GXDataGridProcedure>();
		const string HAS_NEXT_PAGE = "HasNextPage";
		const string RECORD_COUNT = "RecordCount";
		const string RECORD_COUNT_SUPPORTED = "RecordCountSupported";
		long totalRecordCount = -1;
		protected virtual long RecordCount()
		{
			return -1;
		}
		protected virtual bool RecordCountSupported()
		{
			return true;
		}
		protected void SetPaginationHeaders(bool hasNextPage)
		{
			try
			{
				SetHasNextPageHeader(hasNextPage);
				SetRecordCountSupportedHeader();
			}
			catch (Exception ex)
			{
				GXLogging.Warn(log, $"A processing error occurred while setting pagination headers", ex);
			}
		}
		private void SetRecordCountSupportedHeader()
		{
			if (!RecordCountSupported())
			{
				GXLogging.Debug(log, $"Adding '{RECORD_COUNT_SUPPORTED}' header");
				context.SetHeader(RECORD_COUNT_SUPPORTED, false.ToString());
			}
		}

		private void SetHasNextPageHeader(bool hasNextPage)
		{
			context.SetHeader(HAS_NEXT_PAGE, StringUtil.BoolToStr(hasNextPage));
		}

		private void SetRecordCountHeader()
		{
			bool recordCountHeaderRequired = false;
			bool setHeader = false;
			if (context.HttpContext != null)
			{
				recordCountHeaderRequired = !string.IsNullOrEmpty(context.HttpContext.Request.Headers[RECORD_COUNT]);
			}
			if (totalRecordCount != -1)
			{
				setHeader = true;
			}
			else if (recordCountHeaderRequired)
			{
				totalRecordCount = RecordCount();
				setHeader = true;
			}
			if (setHeader)
			{
				GXLogging.Debug(log, $"Adding '{RECORD_COUNT}' header:", totalRecordCount.ToString());
				context.SetHeader(RECORD_COUNT, totalRecordCount.ToString());
			}
		}
		protected long GetPaginationStart(long start, long count)
		{
			if (start < 0) //last page
			{
				totalRecordCount = RecordCount();
				long lastPageRecords = totalRecordCount % count;
				if (lastPageRecords == 0)
					start = totalRecordCount - count;
				else
					start = totalRecordCount - lastPageRecords;
			}
			SetRecordCountHeader();
			return start;
		}
	}
	public class GxReportUtils
	{
		public static int OUTPUT_RVIEWER_NATIVE = 1;
		public static int OUTPUT_RVIEWER_DLL = 2;
		public static int OUTPUT_PDF     = 3;

		static readonly IGXLogger log = GXLoggerFactory.GetLogger<GxReportUtils>();

#if NETCORE
		const string PDF_LIBRARY_ITEXT8 = "ITEXT8";
		const string PDF_LIBRARY_PDFPIG = "PDFPIG";
#endif
		static public IReportHandler GetPrinter( int outputType, string path, Stream reportOutputStream)
		{
			IReportHandler reportHandler;
			if	(outputType == OUTPUT_RVIEWER_NATIVE)
				reportHandler = new GxReportBuilderNative(path, reportOutputStream);
#if !NETCORE
			else if	(outputType == OUTPUT_RVIEWER_DLL)
				reportHandler = new GxReportBuilderDll(path);
#endif
			else if (outputType == OUTPUT_PDF)
			{
				try
				{
#if !NETCORE
					Assembly assem=null;
					
					try
					{
						assem = Assembly.Load("GxPdfReportsCS");
					}
					catch(FileNotFoundException ex)
					{
						GXLogging.Debug(log, ex.Message, ex);
					}

					if (assem==null)
					{
						assem = Assembly.Load("GxPdfReportsI");
					}
					Type classType = assem.GetType( "GeneXus.Printer.GxReportBuilderPdf", false, true);
					reportHandler = (IReportHandler) Activator.CreateInstance(classType,new Object[]{path, reportOutputStream});
#else
					string reportBuidler;
					if (Preferences.PdfReportLibrary().Equals(PDF_LIBRARY_ITEXT8, StringComparison.OrdinalIgnoreCase))
						reportBuidler = "GxReportBuilderPdf8";
					else if (Preferences.PdfReportLibrary().Equals(PDF_LIBRARY_PDFPIG, StringComparison.OrdinalIgnoreCase))
						reportBuidler = "GxReportBuilderPDFPig";
					else
						reportBuidler = "GxReportBuilderPdf";
					reportHandler = (IReportHandler)(ClassLoader.FindInstance("GxPdfReportsCS", "GeneXus.Printer", reportBuidler, new Object[] { path, reportOutputStream }, null));
#endif
				}
				catch (TargetInvocationException ex)
				{
					if (ex.InnerException!=null)
					{
						GXLogging.Error(log, "Load GxPdfReportsI Error", ex.InnerException);
						throw ex.InnerException;
					}else
						throw ex;

				}
				catch(Exception ex)
				{
					GXLogging.Error(log, "Load GxPdfReportsI Error", ex);
					throw ex;

				}
			}
			else
			{
				throw new Exception("Unrecognized report type: " + outputType);
			}
			return reportHandler;
		}
		static public int GetOutputType()
		{
			string rptType;
			if (Config.GetValueOf("ReportManager", out rptType))
				if (rptType.ToUpper() == "DLL")
					return OUTPUT_RVIEWER_DLL;
			return OUTPUT_RVIEWER_NATIVE;
		}
	}
}

