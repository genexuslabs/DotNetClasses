using System.Threading;

namespace GeneXus.Procedure
{
	using System;
    using GeneXus.Encryption;
    using GeneXus.Configuration;
	using GeneXus.Application;
	using GeneXus.Printer;
	using System.Reflection;
	using System.IO;
	using log4net;
	using GeneXus.Performance;
	using GeneXus.Utils;
	using System.Globalization;
	using System.Collections.Generic;
	using GeneXus.XML;
	using GeneXus.Metadata;

	public abstract class GXProcedure
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Procedure.GXProcedure));
		protected IGxContext _Context;					
		protected bool _isMain;
		public abstract void initialize();
		public abstract void cleanup();

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
		public bool DisconnectAtCleanup
		{
			get{ return disconnectUserAtCleanup;}
			set{ disconnectUserAtCleanup=value;}
		}
		public static WaitCallback PropagateCulture(WaitCallback action)
		{
			var currentCulture = Thread.CurrentThread.CurrentCulture;
			GXLogging.Debug(log, "Submit PropagateCulture " + currentCulture);
			var currentUiCulture = Thread.CurrentThread.CurrentUICulture;
			return (x) =>
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
				Thread.CurrentThread.CurrentUICulture = currentUiCulture;
				action(x);
			};
		}
		protected void exitApplication()
		{
			if(IsMain)
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

		protected virtual void printHeaders(){}
		protected virtual void printFooters(){}

        protected string GetEncryptedHash(string value, string key)
        {
            return Encrypt64(GXUtil.GetHash(GeneXus.Web.Security.WebSecurityHelper.StripInvalidChars(value), Cryptography.Constants.SECURITY_HASH_ALGORITHM), key);
        }

        protected String Encrypt64(String value, String key)
        {
            String sRet = "";
            try
            {
                sRet = Crypto.Encrypt64(value, key);
            }
            catch (InvalidKeyException)
            {
                GXLogging.Error(log, "440 Invalid encryption key");
            }
            return sRet;
        }

        protected String Decrypt64(String value, String key)
        {
            String sRet = "";
            try
            {
                sRet = Crypto.Decrypt64(value, key);
            }
            catch (InvalidKeyException)
            {
                GXLogging.Error(log, "440 Invalid encryption key");
            }
            return sRet;
        }

        public IGxContext context
		{
			set	{ _Context = value;	}
			get	{ return _Context;	}
		}
		public msglist GX_msglist 
		{
			get	{ return context.GX_msglist ; }
			set	{ context.GX_msglist = value;}
		}
		public bool IsMain
		{
			set	{ _isMain = value; }
			get	{ return _isMain;  }
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
			GXFileWatcher.Instance.AddTemporaryFile(new GxFile(Preferences.getBLOB_PATH(), new GxFileInfo(fileName, Preferences.getBLOB_PATH())));
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
		public void CallWebObject(string url)
		{
			context.wjLoc = url;
		}

		public virtual void handleException(String gxExceptionType, String gxExceptionDetails, String gxExceptionStack)
		{
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

	public class GxReportUtils
	{
		public static int OUTPUT_RVIEWER_NATIVE = 1;
		public static int OUTPUT_RVIEWER_DLL = 2;
		public static int OUTPUT_PDF     = 3;
		static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		static public IReportHandler GetPrinter( int outputType, string path, Stream reportOutputStream)
		{
			IReportHandler reportHandler;
#if !NETCORE
			if	(outputType == OUTPUT_RVIEWER_NATIVE)
				reportHandler = new GxReportBuilderNative(path, reportOutputStream);
			else if	(outputType == OUTPUT_RVIEWER_DLL)
				reportHandler = new GxReportBuilderDll(path);
			else
#endif
			if  (outputType == OUTPUT_PDF)
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
					reportHandler = (IReportHandler)(ClassLoader.FindInstance("GxPdfReportsCS", "GeneXus.Printer", "GxReportBuilderPdf", new Object[] { path, reportOutputStream }, null));
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

