namespace GeneXus.Printer
{
	using System;
	using System.Runtime.InteropServices;
	using System.Runtime.CompilerServices;
	using System.Drawing;
	using System.Drawing.Printing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Text.RegularExpressions;
	using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Collections;
	using System.Threading;
    using System.Globalization;
	using System.Text;
	using GeneXus.Configuration;
    using GeneXus.XML;
	using log4net;
	using System.Runtime.Serialization;
	using System.Threading.Tasks;

	public interface IPrintHandler
	{
		string Name { get; }
		bool CanSlot { get; }
		StreamReader InputStream { get; set; }
		void Open();
		void Open(StreamWriter output);
		void Print(string configString);
		void Close();
	}
	public interface IReportHandler
	{
		bool GxPrintInit(string output, ref int gxXPage, ref int gxYPage, string iniFile, string form, string printer, int mode, int orientation, int pageSize, int pageLength, int pageWidth, int sacle, int copies, int defSrc, int quality, int color, int duplex) ;
		bool GxPrTextInit(string ouput, ref int nxPage, ref int nyPage, string psIniFile, string psForm, string sPrinter, int nMode, int nPaperLength, int nPaperWidth, int nGridX, int nGridY, int nPageLines) ;

		bool GxOpenDoc(string fileName);
		bool GxRptSilentMode();

		void GxStartDoc() ;
		void GxStartPage() ;
		void GxEndPage() ;
		void GxEndDocument() ;
		void GxEndPrinter() ;
		void GxShutdown();

        [Obsolete("GxDrawText with 6 arguments is deprecated", false)]
		void GxDrawText(string sTxt, int left, int top, int right, int bottom, int align) ;
        void GxDrawText(string sTxt, int left, int top, int right, int bottom, int align, int htmlformat);
        void GxDrawText(string sTxt, int left, int top, int right, int bottom, int align, int htmlformat, int border);
		void GxDrawText(string sTxt, int left, int top, int right, int bottom, int align, int htmlformat, int border, int valign);

		void GxDrawLine(int left, int top, int right, int bottom, int width, int foreRed, int foreGreen, int foreBlue, int style) ;

		void GxDrawRect(int left, int top, int right, int bottom, int pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue, int styleTop, int styleBottom, int styleRight, int styleLeft, int cornerRadioTL, int cornerRadioTR, int cornerRadioBL, int cornerRadioBR);
		void GxDrawBitMap(string bitmap, int left, int top, int right, int bottom) ;
		void GxDrawBitMap(string bitmap, int left, int top, int right, int bottom, int aspectRatio);
		void GxAttris(string fontName, int fontSize, bool fontBold, bool fontItalic, bool fontUnderline, bool fontStrikethru, int Pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue) ;
		void GxClearAttris();

		void GxRVSetLanguage(string lang);
		void GxSetDocName(string docName) ;
		void GxSetDocFormat(string format) ;

		void setModal(bool value);
		bool getModal();
		void setPage(int page);
		int getPage();
		void setPageLines(int plines) ;
		int getPageLines() ;
		void setLineHeight(int lineHeight) ;
		int getLineHeight() ;

		int getM_top();
		int getM_bot();
		void setM_top(int top);
		void setM_bot(int bot);

		void GxPrintMax();
		void GxPrintNormal();
		void GxPrintOnTop();
		void GxPrnCmd(string cmd) ;
		void GxPrnCmd() ;
		bool GxIsAlive();
		bool GxIsAliveDoc();
		bool GxPrnCfg( string ini );

		void setMetrics(string fontName, bool bold, bool italic, int ascent, int descent, int height, int maxAdvance, int[] sizes);
        void setOutputStream(object stream);
	}
	
#if !NETCORE
	public class GxReportBuilderNative : IReportHandler
	{
		public const string END_PAGE = "EPG";
		public const string END_DOCUMENT = "EDC";
		public const string PAGES_TEMPLATE = "{{Pages}}";
		GxCommandFileSender _commandFileSender;
		private bool _templateCreated = false;

		int pageLines;
		int lineHeight;
		int page;
		private bool modal;
		string _appPath;

		public GxReportBuilderNative(string appPath, Stream reportOutputStream)
		{
			_appPath = appPath;
			_commandFileSender = new GxCommandFileSender( reportOutputStream);
			_commandFileSender.Open();
			if (reportOutputStream != null)	// do not batch to a stream 
				_commandFileSender.Slotted = false;

		} 
		public GxReportBuilderNative(Stream reportOutputStream)
		{
			_appPath = "";
			_commandFileSender = new GxCommandFileSender( reportOutputStream);
			_commandFileSender.Open();
			if (reportOutputStream != null)
				_commandFileSender.Slotted = false;
		}
		public bool GxPrintInit(string output, ref int gxXPage, ref int gxYPage, string iniFile, string form, string printer, int mode, int orientation, int pageSize, int pageLength, int pageWidth, int scale, int copies, int defSrc, int quality, int color, int duplex) 
		{
			_commandFileSender.WriteCommand(	"SET",
				gxXPage.ToString(),
				gxYPage.ToString(),
				iniFile, 
				form, 
				printer, 
				mode.ToString(), 
				orientation.ToString(), 
				pageSize.ToString(), 
				pageLength.ToString(), 
				pageWidth.ToString(), 
				scale.ToString(), 
				copies.ToString(), 
				defSrc.ToString(),
				quality.ToString(),
				color.ToString(),
				duplex.ToString());
			gxXPage = pageLength / 1440 * gxXPage;
			gxYPage = pageLength / 1440 * gxYPage;
            
            SetSlottedFromOutput(output); 
            return true;
		}
		public virtual bool GxPrTextInit(string output, ref int nxPage, ref int nyPage, string iniFile, string form, string printer, int mode, int nPaperLength, int nPaperWidth, int nGridX, int nGridY, int nPageLines)
		{
			_commandFileSender.WriteCommand(	"SET",
				nxPage.ToString(),
				nyPage.ToString(),
				iniFile, 
				form, 
				printer, 
				mode.ToString(), 
				"", 
				"", 
				nPaperLength.ToString(), 
				nPaperWidth.ToString(), 
				"", 
				"", 
				"",
				"",
				"",
				"",
				nGridX.ToString(),
				nGridX.ToString(),
				nPageLines.ToString()
				);
			nxPage = nPaperLength / 1440 * nxPage;
			nyPage = nPaperLength / 1440 * nyPage;

            SetSlottedFromOutput(output);
			return true;
		}
        private void SetSlottedFromOutput(string output)
        {
            if (output.ToUpper() != "PRN") //Only reports sent directly to the printer are slotted.
                _commandFileSender.Slotted = false;
        }
		public virtual void GxStartDoc()
		{
			_commandFileSender.WriteCommand("SDC");
		}
		public void GxStartPage()
		{
			_commandFileSender.WriteCommand("SPG");
		} 
		public void GxEndPage()
		{
			_commandFileSender.WriteCommand(END_PAGE);
			_commandFileSender.EndSlot();
		}
		public void GxEndDocument()
		{
			_commandFileSender.WriteCommand(END_DOCUMENT);
			if (_templateCreated)
			{
				_commandFileSender.Template = PAGES_TEMPLATE;
				_commandFileSender.TemplateValue = page.ToString();
			}
			_commandFileSender.Close();
		}
		public virtual void GxEndPrinter()
		{
		}
        public void GxDrawText(string text, int left, int top, int right, int bottom, int align)
        {
            GxDrawText(text, left, top, right, bottom, align, 0);
        }
        public void GxDrawText(string text, int left, int top, int right, int bottom, int align, int htmlformat)
        {
            GxDrawText(text, left, top, right, bottom, align, htmlformat, 0);
        }
		public void GxDrawText(string text, int left, int top, int right, int bottom, int align, int htmlformat, int border)
		{
			GxDrawText(text, left, top, right, bottom, align, htmlformat, border, 0);
		}
		public void GxDrawText(string text, int left, int top, int right, int bottom, int align, int htmlformat, int border, int valign)
		{
			if (!_templateCreated && text.Trim().Equals(PAGES_TEMPLATE, StringComparison.OrdinalIgnoreCase))
			{
				_templateCreated = true;
				text = PAGES_TEMPLATE;
			}
			_commandFileSender.WriteCommand(	"DT",
				text, 
				left.ToString(), 
				top.ToString(), 
				right.ToString(), 
				bottom.ToString(),
				align.ToString());
		}
		public void GxDrawLine(int left, int top, int right, int bottom, int width, int foreRed, int foreGreen, int foreBlue, int style)
		{
			_commandFileSender.WriteCommand( "DL",
				left.ToString(), 
				top.ToString(), 
				right.ToString(), 
				bottom.ToString(), 
				width.ToString(), 
				foreRed.ToString(), 
				foreGreen.ToString(), 
				foreBlue.ToString());
		}
		public void GxDrawLine(int left, int top, int right, int bottom, int width, int foreRed, int foreGreen, int foreBlue)
		{
			GxDrawLine(left, top, right, bottom, width, foreRed, foreGreen, foreBlue, 0);
		}
		public void GxDrawRect(int left, int top, int right, int bottom, int pen,int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue)
		{
			GxDrawRect(left, top, right, bottom, pen, foreRed, foreGreen, foreBlue, backMode, backRed, backGreen, backBlue, 0, 0);
		}
		public void GxDrawRect(int left, int top, int right, int bottom, int pen,int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue, int style, int cornerRadius)
		{
			GxDrawRect(left, top, right, bottom, pen, foreRed, foreGreen, foreBlue, backMode, backRed, backGreen, backBlue, 0, 0, 0, 0, 0, 0, 0, 0);
		}

		public void GxDrawRect(int left, int top, int right, int bottom, int pen,int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue, 
			int styleTop, int styleBottom, int styleRight, int styleLeft, int cornerRadioTL, int cornerRadioTR, int cornerRadioBL, int cornerRadioBR)
		{
			_commandFileSender.WriteCommand( "DR",
				left.ToString(), 
				top.ToString(), 
				right.ToString(), 
				bottom.ToString(), 
				pen.ToString(), 
				foreRed.ToString(),
				foreGreen.ToString(),
				foreBlue.ToString(), 
				backMode.ToString(), 
				backRed.ToString(), 
				backGreen.ToString(), 
				backBlue.ToString());
		}
		public void GxDrawBitMap(string bitmap, int left, int top, int right, int bottom, int aspectRatio)
		{
			bitmap = ReportUtils.AddPath(bitmap, _appPath);
			_commandFileSender.WriteCommand("DB",
				bitmap,
				left.ToString(),
				top.ToString(),
				right.ToString(),
				bottom.ToString());
		}
		public void GxDrawBitMap(string bitmap, int left, int top, int right, int bottom)
		{
			GxDrawBitMap(bitmap, left, top, right, bottom, 0);
		}
		public void GxAttris(string fontName, int fontSize, bool fontBold, bool fontItalic, bool fontUnderline, bool fontStrikethru, int Pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue)
		{
			_commandFileSender.WriteCommand(	"ATT",
				fontName, 
				fontSize.ToString(), 
				fontBold ? "1":"0", 
				fontItalic ? "1":"0",
				fontUnderline ? "1":"0",
				fontStrikethru ? "1":"0", 
				Pen.ToString(), 
				foreRed.ToString(),
				foreGreen.ToString(),
				foreBlue.ToString(), 
				backMode.ToString(), 
				backRed.ToString(), 
				backGreen.ToString(), 
				backBlue.ToString());
		}
		public virtual void GxClearAttris()
		{
			GxAttris("Courier New", 9, false, false, false, false, 0, 0, 0, 0, 0, 255, 255, 255);
		}
		public virtual void GxRVSetLanguage(string lang)
		{
		}
		public virtual void GxSetDocName(string docName)
		{
			if (docName.Trim().Length == 0)
				return;
			docName = ReportUtils.AddPath( docName, _appPath );
			_commandFileSender.WriteCommand( "NME", docName);
		}
		public void GxSetDocFormat(string docFormat)
		{
			_commandFileSender.WriteCommand( "FMT", docFormat.ToUpper());
			switch (docFormat.ToUpper())
			{
				case "RTF":
					// RTF cannot print batch. It have to ead everything to extract the fonts
					_commandFileSender.Slotted = false;
					break;
			}
		}
		public virtual void setModal(bool modal)
		{
			this.modal = modal;
		}
		public virtual bool getModal()
		{
			return modal;
		}
		public virtual void setPageLines(int pageLines) 
		{
			this.pageLines = pageLines;
		}
		public virtual int getPageLines() 
		{
			return pageLines;
		}
		public virtual void setLineHeight(int lineHeight) 
		{
			this.lineHeight = lineHeight;
		}
		public virtual int getLineHeight() 
		{
			return lineHeight;
		}
		public virtual void setPage(int page) 
		{
			this.page = page;
		}
		public virtual int getPage() 
		{
			return page;
		}
		public virtual void GxPrintMax() 
		{
		}
		public virtual void GxPrintNormal() 
		{
		}
		public virtual void GxPrintOnTop() 
		{
		}
		public virtual void GxPrnCmd(string scmd)
		{
            _commandFileSender.WriteCommand("CMD", scmd);
		}
		public virtual void GxPrnCmd()
		{
		}
		public void GxShutdown()
		{
		}
		public virtual bool GxIsAlive() 
		{
			return false;
		}
		public virtual bool GxIsAliveDoc()
		{
			return false;
		}
		public virtual bool GxPrnCfg( string ini )
		{
			return true;
		}
		public virtual void setMetrics(string fontName, bool bold, bool italic, int ascent, int descent, int height, int maxAdvance, int[] sizes)
		{
		}

		public virtual bool GxOpenDoc(String fileName)
		{
			
			_commandFileSender.EndSlot();
			return true;
		}						 

		public virtual bool GxRptSilentMode()
		{
			return true;
		}
	#region IReportHandler Members

		public int getM_top()
		{
			
			return 0;
		}

		public int getM_bot()
		{
			
			return 0;
		}

		public void setM_top(int top)
		{
			
		}

		public void setM_bot(int bot)
		{
			
		}
        public void setOutputStream(object o) { }

	#endregion
	}
	
	public class GxCommandFileSender
	{
		Queue slotQueue;
		Thread execThread;
		Stream _reportOutputStream;

		StreamWriter _sw;
		string _tempFileName;
		bool _printerInitialized;
		
		int _pageSlotSize = 20;
		int _pagesSlotted;
		DateTime _timeStamp;
		bool toSlot = true;
		bool _lastCommandEPG = false;

		public GxCommandFileSender( Stream outputStream)
		{
			slotQueue = new Queue();
			_reportOutputStream = outputStream;
		}
		public bool Slotted
		{
			get
			{
				return toSlot;
			}
			set
			{
				toSlot = value;
			}
		}

		public string Template { get; set; }
		public string TemplateValue { get; set; }

		public void Open()
		{
			_timeStamp = DateTime.Now;
			_pagesSlotted = 0;
			_tempFileName = GxCommandFileSender.GetTempFileName();
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			_sw = new StreamWriter( new FileStream( _tempFileName, FileMode.Create), Encoding.UTF8);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
		}
		public void WriteCommand( string cmd, params string[] parms)
		{
			if (cmd == "SET")
				_printerInitialized = true;

			if (_sw == null || ! _printerInitialized)
				return;

			if (_lastCommandEPG && cmd == GxReportBuilderNative.END_PAGE) //Ignore EPG when two or more EPG consecutive.
				return;
			else
				_lastCommandEPG = (cmd == GxReportBuilderNative.END_PAGE);

			_sw.Write(cmd.PadRight(3,' '));
			for ( int i = 0 ; i < parms.Length ; i++ )
				_sw.Write(","+NormalizeToRegExp(parms[i]));
			_sw.WriteLine("");

		}
		public static string NormalizeToRegExp(string parm)
		{
			return parm.Replace(',', '\0');
		}
		public static string NormalizeFromRegExp(string parm)
		{
			return parm.Replace('\0',',' );
		}

		public void EndSlot()
		{
			if (startNewSlot())
			{
				closeAndSend();
				Open();
			}
		}
		public void Close()
		{
			closeAndSend();
			slotQueue.Enqueue("_GXENDFILE");
		}
		void closeAndSend()
		{
			GxPrintManager p;
			_sw.Close();
			ProcessTemplate();
			slotQueue.Enqueue(_tempFileName);
			if (toSlot)
			{
				if (execThread == null)
				{
					p = new GxPrintManager( slotQueue);
					p.OutputStream = _reportOutputStream;
					execThread = new Thread(new ThreadStart( p.Start ));
					execThread.Start();
				}
			}
			else
			{
				p = new GxPrintManager( (string) slotQueue.Dequeue());
				p.OutputStream = _reportOutputStream;
				p.Start();
			}
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		static string GetTempFileName()
		{	
			string tmpName = Path.Combine(Preferences.getBLOB_PATH(), "GXRpt" + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
			string tmpExt = ".tmp";
			string tmpName1 = tmpName+tmpExt;
			int i = 1;
			while (File.Exists( tmpName1))
			{
				tmpName1 = tmpName+"-"+i.ToString()+tmpExt;
				i++;
			}
			File.Create( tmpName1).Close();
			return tmpName1;
		}
		bool startNewSlot()
		{
			_pagesSlotted++;
			if (toSlot && _pagesSlotted >= _pageSlotSize)
			{
				int pagesPerSec = ( DateTime.Now.Subtract(_timeStamp)).Seconds / _pagesSlotted;
				_pageSlotSize = pagesPerSec * 2;
				_timeStamp = DateTime.Now;
				if (_pageSlotSize < 3)
					_pageSlotSize = 3;
				_pagesSlotted = 0;
				return true;
			}
			return false;
		}

		internal void ProcessTemplate()
		{
			if (Template != null && TemplateValue != null)
			{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				string [] allLines = File.ReadAllLines(_tempFileName, Encoding.UTF8);
				Parallel.For(0, allLines.Length, x =>
				{
					allLines[x] = allLines[x].Replace(Template, TemplateValue);
				});
				File.WriteAllLines(_tempFileName, allLines, Encoding.UTF8);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			}
		}
	}
	
	public class GxPrintManager
	{
		string docName = "";
		Stream docStream;
		string docFormat = "";
		string docOutput = "NOFILE";	// FILE o NOFILE
		string configString = "";

		Queue tempFiles;				// Slotted printing
		string tempFileName;			// One file printing
		IPrintHandler printHandler;

		public GxPrintManager(Queue q)
		{
			tempFiles = q;
			tempFileName = String.Empty;
		}
		
		public GxPrintManager(string fileName)
		{
			tempFileName = fileName;
		}
		public Stream OutputStream
		{
			get { return docStream;}
			set { docStream = value;}
		}
		
		public void Start()
		{
			string fName;
			long waitTime = 0;
			bool firstTimeInit;
			StreamReader streamToRead;
			if (tempFiles == null)		// no file queue: print directly
			{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				streamToRead = new StreamReader( tempFileName);
				initPrinting(streamToRead, false);	
				startPrinting();		
				streamToRead.Close();	
				File.Delete( tempFileName );
			}
			else	// file queue so get the file and print slotted
			{
				waitTime = 0;		
				
				firstTimeInit = true;
				while (true && (waitTime / 1000 / 60) < 10)
				{
					if (tempFiles.Count > 0)
					{
						waitTime = 0;
						fName = (string) tempFiles.Dequeue();
						if (fName == "_GXENDFILE")		
							break;
						streamToRead = new StreamReader( fName);	
						if ( firstTimeInit)		
						{
							initPrinting(streamToRead, true);			
							firstTimeInit = false;
						}
						else 
							reInitPrinting( streamToRead);
						startPrinting();				
						streamToRead.Close();			
						File.Delete( fName );			
						if (this.OutputStream != null)
							this.OutputStream.Flush();
					}
					else
					{
						Thread.Sleep(2000);
						waitTime += 2000;
					}
				}
			}
			endPrinting();      
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
		}
		void initPrinting(StreamReader streamToRead, bool mustSlot)
		{
			
			string line;
			bool settingUp = true;
			line = streamToRead.ReadLine();
			int setupLines = 0;
			while (  settingUp && line != null)
			{ 
				settingUp = processSetUp( line);
				if (settingUp)
					setupLines++;
				line = streamToRead.ReadLine();
			}
			
			streamToRead.DiscardBufferedData();
			streamToRead.BaseStream.Seek(0, SeekOrigin.Begin);
			streamToRead.BaseStream.Position = 0;
			for (int i = 0; i < setupLines; i++) {	streamToRead.ReadLine(); }

			printHandler = GetPrintHandler( streamToRead);
			if (mustSlot && ! printHandler.CanSlot)		
			{
				streamToRead.Close();
				throw new Exception(printHandler.Name+" can't print slotted");
			}
			if (docStream != null && docOutput != "FILE")	// Output to a stream
				printHandler.Open( new StreamWriter( docStream, Encoding.UTF8));
			else if (docName.Trim().Length > 0)		// Output to a file
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				printHandler.Open( new StreamWriter( addFormatExtension(docName), false, Encoding.UTF8));
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			else 
				printHandler.Open();
		}
		void reInitPrinting(StreamReader streamToRead)
		{
			// Se the new stream
			if ( printHandler != null)
				printHandler.InputStream = streamToRead;
			else 
			{
				streamToRead.Close();
				throw new Exception("Could not reinitialize printHandler (slotted printing).");
			}
		}
		string addFormatExtension( string docName)
		{
			if( docName.Trim().Length == 0)
				return "";
			if (docName.IndexOf(".") == -1)
				return docName.Trim()+"."+docFormat;
			return docName;
		}
		void startPrinting()
		{
			printHandler.Print( configString);
		}
		void endPrinting()
		{
			if (printHandler != null)
				printHandler.Close();
		}
		bool processSetUp( string line)
		{
			string cmd = line.Substring(0,3);
			switch (cmd.ToUpper().Trim())
			{
				case "///":		//	Comment
					break;
				case "SET":		//	Configuration
					configString = line;
					break;
				case "FMT":		// Name
					docFormat = line.Substring(4,line.Length-4).Trim().ToUpper();
					break;
				case "NME":		// Format
					docName = line.Substring(4,line.Length-4).Trim();
					if (docName.Trim().Length != 0)
						docOutput = "FILE";
					else
						docOutput = "NOFILE";
					break;
				default:		
					return false;
			}
			return true;
		}
		IPrintHandler GetPrintHandler( StreamReader s)
		{
			IPrintHandler ph = null;
			switch (docFormat)
			{
				case "RTF":
					ph = new GxRtfPrinter( s );
					break;
				case "TXT":
					ph = new GxTxtPrinter( s );
					break;
				default :
					ph = new GxPrinterDevice( s );
					break;
			}
			return ph;
		}
	}
	public class GxPrinterDevice : IPrintHandler
	{
		Font currentFont;
		Color currentForeColor;
		Color currentBackColor;
		String lastLine;

		bool pageStarted;
		StreamReader streamToRead;
		Graphics _gr;

		int originalScaleX = 96;		// Dots x inch
		int originalScaleY = 96;		// Dots x inch

		public GxPrinterDevice(StreamReader sr)
		{
			streamToRead = sr;
			lastLine = null;
		}
		public string Name
		{
			get { return "GxPrinterDevice";}
		}
		public bool CanSlot
		{
			get {return true;}
		}
		public void Open(StreamWriter output)
		{
		}
		public StreamReader InputStream
		{
			get { return streamToRead;}
			set {streamToRead = value; lastLine = null; }
		}
		public void Open()
		{
		}
		public void Print(string configString) 
		{
			PrintingPermission pp = new PrintingPermission(PrintingPermissionLevel.AllPrinting);
			pp.Demand();
			PrintDocument pd = new PrintDocument(); 
			pd.PrintController = new StandardPrintController(); 
			pd.PrintPage += new PrintPageEventHandler(evt_PrintPage);
			if (configString.Length > 0)
				initPrnReport(configString, pd.PrinterSettings, pd.DefaultPageSettings);
			if (pd.PrinterSettings.IsValid)
				pd.Print();
			else
			{
				streamToRead.Close();
				throw new Exception("Printer settins not valid");
			}
		}
		public void Close()
		{
		}
		void initPrnReport( string configString, PrinterSettings printerSettings, PageSettings pageSettings)
		{
			NameValueCollection configSettings;
			
			configSettings = GxPrinterConfig.ConfigPrinterSettings( configString);
			
			if (configSettings == null)
				return;

			originalScaleX = Convert.ToInt32(configSettings["XPAGE"]);
			originalScaleY = Convert.ToInt32(configSettings["YPAGE"]);

			if ( printerSettings != null)
			{
				
				if (configSettings["PRINTER"].Length > 0)
					foreach( string prnName in PrinterSettings.InstalledPrinters)
						if ( prnName == configSettings["PRINTER"])
						{
							printerSettings.PrinterName = configSettings["PRINTER"];
							break;
						}
				// Number of copies
				printerSettings.Copies =  Convert.ToInt16( configSettings["COPIES"]);
				// Duplex
				if ( printerSettings.CanDuplex)
					printerSettings.Duplex = Convert.ToInt32( configSettings["DUPLEX"]) == 1 ? Duplex.Simplex : Duplex.Vertical ;
			}
			if ( pageSettings != null)
			{
				// Landscape
				pageSettings.Landscape = Convert.ToInt32( configSettings["ORIENTATION"]) != 1;
				// Color
				if ( printerSettings.SupportsColor)
					pageSettings.Color = Convert.ToInt32(configSettings["COLOR"]) == 1;
				// paper try

				foreach( PaperSource pSrc in printerSettings.PaperSources)
					if ( (int)(pSrc.Kind) == Convert.ToInt32(configSettings["DEFAULTSOURCE"]))
					{
						pageSettings.PaperSource = pSrc;
						break;
					}
				// paper size
				if ( configSettings["PAPERSIZE"] == "0")
					pageSettings.PaperSize = new PaperSize(	"Custom", 
						Convert.ToInt32( configSettings["PAPERLENGTH"])/1440 *100,
						Convert.ToInt32( configSettings["PAPERWIDTH"])/1440 *100); 
				else
					foreach( PaperSize pSz in printerSettings.PaperSizes)
						if ( (int)(pSz.Kind) == Convert.ToInt32(configSettings["PAPERSIZE"]))
						{
							pageSettings.PaperSize = pSz;
							break;
						}
			}
			
		}

		private void evt_PrintPage(object sender, PrintPageEventArgs ev)
		{	
			bool morePages = false;
			bool printPage = true;
			_gr = ev.Graphics;
			if (lastLine == null)
				lastLine = streamToRead.ReadLine();

			while (lastLine != null && printPage) { 
				printPage = processPrinterCommand( ev, lastLine, ref morePages );
				lastLine = streamToRead.ReadLine();
			}
			if (lastLine == null || lastLine == GxReportBuilderNative.END_DOCUMENT || ! morePages)		
				ev.HasMorePages = false;
			else
				ev.HasMorePages = true;
		}
		private bool processPrinterCommand(PrintPageEventArgs ev, string line, ref bool morePages )
		{
			GroupCollection grCol;
			string cmd = line.Substring(0,3);
			switch (cmd.ToUpper().Trim())
			{
				case "DR":
					if( (grCol = GxPrintCommandParser.ParseRect( line)) != null)
						DrawRect(	new Point( Convert.ToInt32(grCol["left"].Value), Convert.ToInt32(grCol["top"].Value)),
									new Point( Convert.ToInt32(grCol["right"].Value), Convert.ToInt32(grCol["bottom"].Value)),
									Convert.ToInt32(grCol["pen"].Value),
									Color.FromArgb(	Convert.ToInt32(grCol["fr"].Value), 
													Convert.ToInt32(grCol["fg"].Value), 
													Convert.ToInt32(grCol["fb"].Value)),
									Convert.ToInt32(grCol["bm"].Value) == 1 ?
										Color.FromArgb(	Convert.ToInt32(grCol["br"].Value), 
														Convert.ToInt32(grCol["bg"].Value), 
														Convert.ToInt32(grCol["bb"].Value)) :
										Color.Empty);
					break;
				case "DL":
					if( (grCol = GxPrintCommandParser.ParseLine( line)) != null)
						DrawLine(	
									new Point( Convert.ToInt32(grCol["left"].Value), Convert.ToInt32(grCol["top"].Value)),
									new Point( Convert.ToInt32(grCol["right"].Value), Convert.ToInt32(grCol["bottom"].Value)),
									Convert.ToInt32(grCol["width"].Value), 
									Color.FromArgb(	Convert.ToInt32(grCol["fr"].Value), 
													Convert.ToInt32(grCol["fg"].Value), 
													Convert.ToInt32(grCol["fb"].Value)));
					break;
				case "DB":
					if( (grCol = GxPrintCommandParser.ParseBitmap( line)) != null)
						DrawBitmap(	
									grCol["bitmap"].Value, 
									new Point( Convert.ToInt32(grCol["left"].Value), Convert.ToInt32(grCol["top"].Value)),
									new Point( Convert.ToInt32(grCol["right"].Value), Convert.ToInt32(grCol["bottom"].Value)));
					break;
				case "DT":
                    if ((grCol = GxPrintCommandParser.ParseText(line)) != null)
                    {
                        string text = grCol["text"].Value;
                        text = GxCommandFileSender.NormalizeFromRegExp(text);

                        DrawText(text,
                                    new Point(Convert.ToInt32(grCol["left"].Value), Convert.ToInt32(grCol["top"].Value)),
                                    new Point(Convert.ToInt32(grCol["right"].Value), Convert.ToInt32(grCol["bottom"].Value)),
                                    this.currentFont,
                                    Convert.ToInt32(grCol["align"].Value),
                                    this.currentForeColor,
                                    this.currentBackColor);
                    }
					break;
				case "ATT":
					if( (grCol = GxPrintCommandParser.ParseTextAttributes( line)) != null)
						setTextAttributes(	grCol["name"].Value, 
									Convert.ToInt32(grCol["size"].Value), 
									Convert.ToInt32(grCol["bold"].Value)== 1, 
									Convert.ToInt32(grCol["italic"].Value)== 1, 
									Convert.ToInt32(grCol["underline"].Value)== 1, 
									Convert.ToInt32(grCol["strike"].Value)== 1, 
									Convert.ToInt32(grCol["pen"].Value), 
									Convert.ToInt32(grCol["fr"].Value), 
									Convert.ToInt32(grCol["fg"].Value), 
									Convert.ToInt32(grCol["fb"].Value), 
									Convert.ToInt32(grCol["bm"].Value), 
									Convert.ToInt32(grCol["br"].Value), 
									Convert.ToInt32(grCol["bg"].Value), 
									Convert.ToInt32(grCol["bb"].Value));
					break;
				case "SPG":
					pageStarted = true;
					break;
				case GxReportBuilderNative.END_PAGE:
					if (pageStarted)
					{
						morePages = true;
						pageStarted = false;
						return false;
					}
					break;
				case GxReportBuilderNative.END_DOCUMENT:
					morePages = false;
					return false;
			}
			return true;
		}
		void setTextAttributes(String fontName, int fontSize, bool fontBold, bool fontItalic, bool fontUnderline, bool fontStrikethru, int Pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue)
		{
			FontStyle fntSt = 0;
			if ( fontBold ) fntSt |= FontStyle.Bold ;
			if ( fontItalic )  fntSt |= FontStyle.Italic ;
			if ( fontUnderline )  fntSt |= FontStyle.Underline ;
			if ( fontStrikethru )  fntSt |= FontStyle.Strikeout ;
			this.currentFont = new Font( fontName, fontSize, fntSt);
			this.currentForeColor = Color.FromArgb( foreRed, foreGreen, foreBlue);
			if (backMode != 1)
				this.currentBackColor = Color.Empty;
			else
				this.currentBackColor = Color.FromArgb( backRed, backGreen, backBlue);
		}
		void DrawRect(Point p1, Point p2, int penSize, Color foreColor, Color backColor)
		{
			float l = convertX(p1.X);
			float t = convertY(p1.Y);
			float r = convertX(p2.X);
			float b = convertY(p2.Y);
			if (penSize > 0)
			{
				using (Pen pen = new Pen(foreColor, penSize))
				{
					_gr.DrawRectangle(pen, l, t, r - l, b - t);
				}
			}
			if (backColor != Color.Empty)
			{
				using (Brush br = new SolidBrush(backColor))
				{
					_gr.FillRectangle(br, l, t, r - l, b - t);
				}
			}
		}
		void DrawLine(Point p1, Point p2, int penSize, Color foreColor)
		{
			float l = convertX(p1.X);
			float t = convertY(p1.Y);
			float r = convertX(p2.X);
			float b = convertY(p2.Y);
			using (Pen pen = new Pen(foreColor, penSize))
			{
				_gr.DrawLine(pen, l, t, r, b);
			}
		}
		void DrawBitmap(string bitmap, Point p1, Point p2)
		{
			float l = convertX(p1.X);
			float t = convertY(p1.Y);
			float r = convertX(p2.X);
			float b = convertY(p2.Y);
			Image img = Image.FromFile(bitmap);
			_gr.DrawImage( img, l, t, r-l, b-t);
		}
		void DrawText(string text, Point p1, Point p2, Font fnt, int align, Color foreColor, Color backColor)
		{
			float l = convertX(p1.X);
			float t = convertY(p1.Y);
			float r = convertX(p2.X);
			float b = convertY(p2.Y);
			DrawRect( p1, p2, 0, foreColor, backColor);
			RectangleF rect = new RectangleF(correctTextCoor(l, align), t, r-l, b-t);
			using (Brush br = new SolidBrush(foreColor))
			{
				_gr.DrawString(text, fnt, br, rect, stringFormatFromAlign(align));
			}
		}
		StringFormat stringFormatFromAlign( int align)
		{
			
			StringFormat sfmt = new StringFormat();
			sfmt.Trimming = StringTrimming.None;
			if ( (align & 256) > 0)
				sfmt.FormatFlags |= StringFormatFlags.NoClip;
			if ( (align & 16) == 0)
				sfmt.FormatFlags |= StringFormatFlags.NoWrap;
			if ( (align & 2) > 0)
				sfmt.Alignment |= StringAlignment.Far;
			else if ( (align & 1) > 0)
				sfmt.Alignment |= StringAlignment.Center;
			else 
				sfmt.Alignment |= StringAlignment.Near;
			return sfmt;
		}
		float correctTextCoor(float l, int align)
		{
			int mn = 4;
			float ret;
			if ( (align & 2) > 0)
				ret = l + mn;
			else if ( (align & 1) > 0)
				ret = l;
			else 
				ret = l - mn;
			return ret;
		}
		float convertX( float x)
		{
			return x/this.originalScaleX * 100;
		}
		float convertY( float y)
		{
			return y/this.originalScaleY * 100;
		}
	}
	public class GxTxtPrinter : IPrintHandler
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GxTxtPrinter));
		StreamReader streamToRead;
		StreamWriter streamToWrite;
		List<string> pageContents;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("GxFxCopRules", "CR1000:EnforceThreadSafeType")]
		Dictionary<int, List<string>> commands;
		
        int NPAGELINES;

		public GxTxtPrinter(StreamReader sr)
		{
			streamToRead = sr;
            pageContents = new List<string>();
            commands = new Dictionary<int, List<string>>();
		}
		public string Name
		{
			get { return "GxTxtPrinter";}
		}
		public bool CanSlot
		{
			get { return true;}
		}
		public StreamReader InputStream
		{
			get { return streamToRead;}
			set {streamToRead = value;}
		}
		public void Open( StreamWriter output)
		{
			streamToWrite = output;
		}
		public void Open()
		{
			throw new Exception("GxTxtPrinter open error: output file name not specified");
		}
		public void Print(string configString) 
		{
			string line;
			if (configString.Length > 0)
				initTxtReport( configString);
			line = streamToRead.ReadLine();
			while (!string.IsNullOrEmpty(line))
			{
				processPrinterCommand(line);
				line = streamToRead.ReadLine();
			}
		}
		void initTxtReport(string configString)
		{
			NameValueCollection configSettings = GxPrinterConfig.ConfigPrinterSettings( configString);
			
			if (configSettings == null)
				return;

			NPAGELINES = Convert.ToInt32(configSettings["NPAGELINES"]);
		}
		public void Close()
		{
			streamToWrite.Close();
		}
		void processPrinterCommand( string line)
		{
			GXLogging.Debug(log, "processPrinterCommand:" + line);
			GroupCollection grCol;
			string cmd = line.Substring(0,3);
			switch (cmd.ToUpper().Trim())
			{
                case "CMD":
                    string commandCode = line.Substring(4);
					if (!string.IsNullOrEmpty(commandCode))
					{
						int idx = pageContents.Count;
						if (!commands.ContainsKey(idx))
						{
							commands.Add(idx, new List<string>());
						}
						commands[idx].Add(commandCode);
					}
                    break;
				case "DT":
					if( (grCol = GxPrintCommandParser.ParseText( line)) != null)
					{
						int row= Convert.ToInt32(grCol["top"].Value) / 17;
						int col = Convert.ToInt32(grCol["left"].Value) / 8;

						string text = grCol["text"].Value;
						text=GxCommandFileSender.NormalizeFromRegExp(text);

						while(row > pageContents.Count-1)
						{
							pageContents.Add(string.Empty);
						}
						

						string oldText = pageContents[row];

						if (oldText.Length > col)
						{
							pageContents[row] = oldText.Substring(0, col) + text + (( col + text.Length < oldText.Length) ? oldText.Substring(col + text.Length) : string.Empty);
						}
						else
						{
							pageContents[row] = oldText.PadRight(col, ' ') + text;
						}
					}
					break;
				case GxReportBuilderNative.END_PAGE:

                    for (int i = 0; i < pageContents.Count; i++)
                    {
						if (commands.ContainsKey(i))
						{
							List<string> cmds = commands[i];
							foreach (string s in cmds)
							{
								streamToWrite.WriteLine(s);
							}
						}
						string text = pageContents[i];
                        streamToWrite.WriteLine(text);
                    }
					if (NPAGELINES>0 && pageContents.Count<NPAGELINES)
					{
						for(int i=pageContents.Count; i<NPAGELINES;i++)
						{
							if (commands.ContainsKey(i))
							{
								List<string> cmds = commands[i];
								foreach (string s in cmds)
								{
									streamToWrite.WriteLine(s);
								}
							}
							streamToWrite.WriteLine(string.Empty);
                        }
					}
					pageContents.Clear();
                    commands.Clear();
					break;
			}
		}

		
	}
	public class TxtLine
	{
		public int Top;
		public int Left;
		public string Text;
		public TxtLine( int top, int left, string text)
		{
			Top = top;
			Left = left;
			Text = text;
		}
	}


	public class txtLineComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			return  ((TxtLine)x).Top.CompareTo( ((TxtLine)y).Top);
		}
	}
 
	public class GxRtfPrinter : IPrintHandler
	{
		Font currentFont;
		Color currentForeColor;
		Color currentBackColor;

		bool pageStarted;
		StreamReader streamToRead;
		StreamWriter streamToWrite;

		int originalScaleX = 96;		// Dots x inch
		int originalScaleY = 96;		// Dots x inch

		int LOGICAL2TWIP=15;	//1440 TPI /96 PPI

		Hashtable reportFonts;

		public GxRtfPrinter(StreamReader s)
		{
			streamToRead = s;
		}
		public string Name
		{
			get { return "GxRtfPrinter";}
		}
		public bool CanSlot
		{
			
			get { return false;}
		}
		public StreamReader InputStream
		{
			get { return streamToRead;}
			set {streamToRead = value;}
		}
		public void Open( StreamWriter output)
		{
			streamToWrite = output;
		}
		public void Open()
		{
			throw new Exception("GxRtfPrinter open error: output file name not specified");
		}
		public void Print(string configString) 
		{
			string line;
			rtfPreread();
			if (configString.Length > 0)
				initRtfReport( configString);
			while ((line = streamToRead.ReadLine()) != null) 
				processPrinterCommand( line );
		}
		public void Close()
		{
			streamToWrite.Close();
		}
		void processPrinterCommand(string line )
		{
			GroupCollection grCol;
			string cmd = line.Substring(0,3);
			switch (cmd.ToUpper().Trim())
			{
				case "DR":
					if( (grCol = GxPrintCommandParser.ParseRect( line)) != null)
						DrawRect(	new Point( Convert.ToInt32(grCol["left"].Value), Convert.ToInt32(grCol["top"].Value)),
							new Point( Convert.ToInt32(grCol["right"].Value), Convert.ToInt32(grCol["bottom"].Value)),
							Convert.ToInt32(grCol["pen"].Value),
							Color.FromArgb(	Convert.ToInt32(grCol["fr"].Value), 
							Convert.ToInt32(grCol["fg"].Value), 
							Convert.ToInt32(grCol["fb"].Value)),
							Convert.ToInt32(grCol["bm"].Value) == 1 ?
							Color.FromArgb(	Convert.ToInt32(grCol["br"].Value), 
							Convert.ToInt32(grCol["bg"].Value), 
							Convert.ToInt32(grCol["bb"].Value)) :
							Color.Empty);
					break;
				case "DL":
					if( (grCol = GxPrintCommandParser.ParseLine( line)) != null)
						DrawLine(	
							new Point( Convert.ToInt32(grCol["left"].Value), Convert.ToInt32(grCol["top"].Value)),
							new Point( Convert.ToInt32(grCol["right"].Value), Convert.ToInt32(grCol["bottom"].Value)),
							Convert.ToInt32(grCol["width"].Value), 
							Color.FromArgb(	Convert.ToInt32(grCol["fr"].Value), 
							Convert.ToInt32(grCol["fg"].Value), 
							Convert.ToInt32(grCol["fb"].Value)));
					break;
				case "DB":
					if( (grCol = GxPrintCommandParser.ParseBitmap( line)) != null)
						DrawBitmap(	
							grCol["bitmap"].Value, 
							new Point( Convert.ToInt32(grCol["left"].Value), Convert.ToInt32(grCol["top"].Value)),
							new Point( Convert.ToInt32(grCol["right"].Value), Convert.ToInt32(grCol["bottom"].Value)));
					break;
				case "DT":
                    if ((grCol = GxPrintCommandParser.ParseText(line)) != null)
                    {
                        string text = grCol["text"].Value;
                        text = GxCommandFileSender.NormalizeFromRegExp(text);
                        DrawText(text,
                            new Point(Convert.ToInt32(grCol["left"].Value), Convert.ToInt32(grCol["top"].Value)),
                            new Point(Convert.ToInt32(grCol["right"].Value), Convert.ToInt32(grCol["bottom"].Value)),
                            this.currentFont,
                            Convert.ToInt32(grCol["align"].Value),
                            this.currentForeColor,
                            this.currentBackColor);
                    }
					break;
				case "ATT":
					if( (grCol = GxPrintCommandParser.ParseTextAttributes( line)) != null)
						setTextAttributes(	grCol["name"].Value, 
							Convert.ToInt32(grCol["size"].Value), 
							Convert.ToInt32(grCol["bold"].Value)== 1, 
							Convert.ToInt32(grCol["italic"].Value)== 1, 
							Convert.ToInt32(grCol["underline"].Value)== 1, 
							Convert.ToInt32(grCol["strike"].Value)== 1, 
							Convert.ToInt32(grCol["pen"].Value), 
							Convert.ToInt32(grCol["fr"].Value), 
							Convert.ToInt32(grCol["fg"].Value), 
							Convert.ToInt32(grCol["fb"].Value), 
							Convert.ToInt32(grCol["bm"].Value), 
							Convert.ToInt32(grCol["br"].Value), 
							Convert.ToInt32(grCol["bg"].Value), 
							Convert.ToInt32(grCol["bb"].Value));
					break;
				case "SPG":
					pageStarted = true;
					break;
				case GxReportBuilderNative.END_PAGE:
					if (pageStarted)
					{
						EndPage();
						pageStarted = false;
					}
					break;
				case GxReportBuilderNative.END_DOCUMENT:
					EndDoc();
					break;
			}
		}
		void setTextAttributes(String fontName, int fontSize, bool fontBold, bool fontItalic, bool fontUnderline, bool fontStrikethru, int Pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue)
		{
			FontStyle fntSt = 0;
			if ( fontBold ) fntSt |= FontStyle.Bold ;
			if ( fontItalic )  fntSt |= FontStyle.Italic ;
			if ( fontUnderline )  fntSt |= FontStyle.Underline ;
			if ( fontStrikethru )  fntSt |= FontStyle.Strikeout ;
			this.currentFont = new Font( fontName, fontSize, fntSt);
			this.currentForeColor = Color.FromArgb( foreRed, foreGreen, foreBlue);
			if (backMode != 1)
				this.currentBackColor = Color.Empty;
			else
				this.currentBackColor = Color.FromArgb( backRed, backGreen, backBlue);
		}
		void rtfPreread()
		{
			
			Font fnt;
			string fntName, line;
			GroupCollection grCol;
			Stream s = streamToRead.BaseStream;
			reportFonts = new Hashtable();
			if ( ! s.CanRead)	
				return;			
			while ((line = streamToRead.ReadLine()) != null) 
			{
				if (line.Substring(0,3) == "ATT")
					if( (grCol = GxPrintCommandParser.ParseTextAttributes( line)) != null)
					{
						fntName = grCol["name"].Value;
						fnt = new Font( fntName, Convert.ToInt32(grCol["size"].Value));
						if ( ! reportFonts.Contains(fntName))
							reportFonts.Add( fntName, fnt);
					}
			}
			s.Seek( 0,SeekOrigin.Begin);
		}
		void initRtfReport(string configString)
		{
			NameValueCollection configSettings;
			Font fnt;
			
			configSettings = GxPrinterConfig.ConfigPrinterSettings( configString);
			
			if (configSettings == null)
				return;

			originalScaleX = Convert.ToInt32(configSettings["XPAGE"]);
			originalScaleY = Convert.ToInt32(configSettings["YPAGE"]);

			streamToWrite.Write( "{\\rtf1\\ansi {\\fonttbl");
			int i = 0;
			foreach( DictionaryEntry fntObj in reportFonts)
			{
				fnt = (Font) fntObj.Value;
				
				streamToWrite.Write("{\\f"+i.ToString()+"\\fswiss "+fnt.Name+";}");
				i++;
			}
			streamToWrite.Write("}");
			streamToWrite.Write("\\psz"+configSettings["PAPERSIZE"]);
			if ( Convert.ToInt32( configSettings["ORIENTATION"]) != 1)
				streamToWrite.Write("\\lndscpsxn");
		}
		int findFont(string name)
		{
			Font fnt;
			int i = 0;
			foreach( DictionaryEntry fntObj in reportFonts)
			{
				fnt = (Font) fntObj.Value;
				if (fnt.Name.ToUpper() == name.ToUpper())
					return i;
				i++;
			}
			return -1;
		}
		void DrawRect(Point p1, Point p2, int penSize, Color foreColor, Color backColor)
		{
			string sBuffer;
			sBuffer =	"{\\*\\do\\dobxpage\\dobypage\\dprect\\dpx"+(p1.X * LOGICAL2TWIP).ToString()+
						"\\dpy"+(p1.Y * LOGICAL2TWIP).ToString()+
						"\\dpxsize"+((p2.X - p1.X) * LOGICAL2TWIP).ToString()+
						"\\dpysize"+((p2.Y - p1.Y) * LOGICAL2TWIP).ToString()+
						"\\dplinew"+(penSize * LOGICAL2TWIP).ToString()+"}";
			streamToWrite.Write(sBuffer.ToString());
		}
		void DrawLine(Point p1, Point p2, int penSize, Color foreColor)
		{
			string sBuffer;
			sBuffer =	"{\\*\\do\\dobxpage\\dobypage\\dpline\\dpx"+(p1.X * LOGICAL2TWIP).ToString()+
						"\\dpy"+(p1.Y * LOGICAL2TWIP).ToString()+
						"\\dpxsize"+((p2.X - p1.X) * LOGICAL2TWIP).ToString()+
						"\\dpysize"+((p2.Y - p1.Y) * LOGICAL2TWIP).ToString()+
						"\\dplinew"+(penSize * LOGICAL2TWIP).ToString()+"}";
			streamToWrite.Write(sBuffer);
		}
		void DrawBitmap(string bitmap, Point p1, Point p2)
		{
			int width = p2.X - p1.X + 1;
			int height = p2.Y - p1.Y + 1;
			int widthMM = (int) Math.Round( (double) width * 2540 / originalScaleX); 
			int heightMM = (int) Math.Round( (double) height * 2540 / originalScaleY); 
			string sBuffer;
			sBuffer =	"\\par \\pard\\phpg\\posx"+(p1.X * LOGICAL2TWIP).ToString()+
						"\\pvpg\\posy"+(p1.Y * LOGICAL2TWIP).ToString()+
						"{\\result {\\pict\\wmetafile8\\picw"+widthMM.ToString()+
						"\\pich"+heightMM.ToString()+
						"\\picwgoal"+(width * LOGICAL2TWIP).ToString()+
						"\\pichgoal"+(height * LOGICAL2TWIP).ToString()+
						"\n";
			streamToWrite.Write( sBuffer);
			Bitmap bm = new Bitmap(bitmap);
			bm.Save( streamToWrite.BaseStream, ImageFormat.Emf);
			streamToWrite.Write( "}}\n");
		}
		void DrawText(string text, Point p1, Point p2, Font fnt, int align, Color foreColor, Color backColor)
		{

			string sBuffer;
			string sAttributes;
			string sAttributes2;

			if (text.Length == 0)
				return;
			int fontNum = findFont( fnt.Name);
			sAttributes =	"\\par \\pard \\plain \\nowrap\\f"+fontNum.ToString()+
							"\\fs"+(fnt.Size * 2).ToString()+
							"\\phpg\\posx"+(p1.X * LOGICAL2TWIP).ToString()+
							"\\pvpg\\posy"+(p1.Y * LOGICAL2TWIP).ToString()+
							(fnt.Bold ? "\\b" : "")+
							(fnt.Italic ? "\\i" : "")+
							(fnt.Strikeout ? "\\strike" : ""); 
			if ( (align & 3) == 0)
				sAttributes2 = "";
			else if ( (align & 2) > 0)
				sAttributes2 = "\\qr\\absw"+((p2.X - p1.X + 5) * LOGICAL2TWIP).ToString();
			else
				sAttributes2 = "\\ql";

			sBuffer = sAttributes + sAttributes2 + " " + text;
			streamToWrite.Write( sBuffer);
		}
		void EndPage()
		{
			streamToWrite.Write( "\\par \\pagebb ");
        }
		void EndDoc()
		{
			streamToWrite.Write( "}");
		}

	}
	public class GxPrintCommandParser
	{
        static Regex regExpRect;
        static Regex regExpLine;
        static Regex regExpText;
        static Regex regExpAtt;
        static Regex regExpBitmap;
		public static GroupCollection ParseRect(string line)
		{
            if (regExpRect==null)
                regExpRect = new Regex("...,(?<left>[0-9]*),(?<top>[0-9]*),(?<right>[0-9]*),(?<bottom>[0-9]*),(?<pen>[0-9]*),(?<fr>[0-9]*),(?<fg>[0-9]*),(?<fb>[0-9]*),(?<bm>[0-9]*),(?<br>[0-9]*),(?<bg>[0-9]*),(?<bb>[0-9]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Match m = regExpRect.Match(line);
			if (m.Success)
				return m.Groups;
			else
				return null;
		}
		public static GroupCollection ParseLine(string line)
		{
            if (regExpLine==null)
                regExpLine = new Regex("...,(?<left>[0-9]*),(?<top>[0-9]*),(?<right>[0-9]*),(?<bottom>[0-9]*),(?<width>[0-9]*),(?<fr>[0-9]*),(?<fg>[0-9]*),(?<fb>[0-9]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Match m = regExpLine.Match(line);
			if (m.Success)
				return m.Groups;
			else
				return null;
		}
		public static GroupCollection ParseBitmap(string line)
		{
            if (regExpBitmap==null)
                regExpBitmap = new Regex("...,(?<bitmap>\\S*),(?<left>[0-9]*),(?<top>[0-9]*),(?<right>[0-9]*),(?<bottom>[0-9]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Match m = regExpBitmap.Match(line);
			if (m.Success)
				return m.Groups;
			else
				return null;
		}
		public static GroupCollection ParseText(string line)
		{
            if (regExpText==null)
                regExpText = new Regex("...,(?<text>[^,]*),(?<left>[0-9]*),(?<top>[0-9]*),(?<right>[0-9]*),(?<bottom>[0-9]*),(?<align>[0-9]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Match m = regExpText.Match(line);
			if (m.Success)
				return m.Groups;
			else
				return null;
		}
		public static GroupCollection ParseTextAttributes(string line)
		{
            if (regExpAtt==null)
                regExpAtt = new Regex("...,(?<name>[^,]*),(?<size>[0-9]*),(?<bold>[01]),(?<italic>[01]),(?<underline>[01]),(?<strike>[01]),(?<pen>[0-9]*),(?<fr>[0-9]*),(?<fg>[0-9]*),(?<fb>[0-9]*),(?<bm>[01]),(?<br>[0-9]*),(?<bg>[0-9]*),(?<bb>[0-9]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Match m = regExpAtt.Match(line);
			if (m.Success)
				return m.Groups;
			else
				return null;
		}
	}
	
	public class GxPrinterConfig
	{
		static public NameValueCollection ConfigPrinterSettings( string line)
		{
			int parmsCountGraphic=16;
			char []c = line.ToCharArray();
			int count=0;
			string regex = "SET,"+
				"(?<xpage>[0-9]*),"+
				"(?<ypage>[0-9]*),"+
				"(?<inifile>[^,]*),"+
				"(?<form>[^,]*),"+
				"(?<printer>[^,]*),"+
				"(?<mode>[^,]*),"+
				"(?<orientation>[0-9]*),"+
				"(?<pagesize>[0-9]*),"+
				"(?<pagelength>[0-9]*),"+
				"(?<pagewidth>[0-9]*),"+
				"(?<scale>[0-9]*),"+
				"(?<copies>[0-9]*),"+
				"(?<defsrc>[0-9]*),"+
				"(?<quality>[0-9]*),"+
				"(?<color>[0-9]*),";


				for (int i=0; i<c.Length; i++)
				{
					if (c[i]==',') count ++;
				}
			if (count > parmsCountGraphic)
			{
				regex += "(?<duplex>[0-9]*)," +
					"(?<ngridx>[0-9]*)," + 
					"(?<ngridy>[0-9]*)," + 
					"(?<npagelines>[0-9]*)";
			}
			else
			{
				regex += "(?<duplex>[0-9]*)";
			}
			Regex r = new Regex( regex, RegexOptions.IgnoreCase|RegexOptions.Compiled);

			Match m = r.Match(line);
			if (m.Success)
			{
				string formName = m.Groups["form"].Value;
				string iniFileName = m.Groups["inifile"].Value;
				
				NameValueCollection configSettings = parsePrinterSettingsIni( formName, iniFileName);
				
				if (configSettings == null)
				{
					configSettings = new NameValueCollection();
					configSettings.Add("PRINTER", m.Groups["printer"].Value);
					configSettings.Add("MODE", m.Groups["mode"].Value);
					configSettings.Add("ORIENTATION", m.Groups["orientation"].Value);
					configSettings.Add("PAPERSIZE", m.Groups["pagesize"].Value);
					configSettings.Add("PAPERLENGTH", m.Groups["pagelength"].Value);
					configSettings.Add("PAPERWIDTH", m.Groups["pagewidth"].Value);
					configSettings.Add("SCALE", m.Groups["scale"].Value);
					configSettings.Add("COPIES", m.Groups["copies"].Value);
					configSettings.Add("DEFAULTSOURCE", m.Groups["defsrc"].Value);
					configSettings.Add("PRINTQUALITY", m.Groups["quality"].Value);
					configSettings.Add("COLOR", m.Groups["color"].Value);
					configSettings.Add("DUPLEX", m.Groups["duplex"].Value);
				}
				if (count > parmsCountGraphic)
				{
					configSettings.Add("NGRIDX", m.Groups["ngridx"].Value);
					configSettings.Add("NGRIDY", m.Groups["ngridy"].Value);
					configSettings.Add("NPAGELINES", m.Groups["npagelines"].Value);

				}
				configSettings.Add("XPAGE", m.Groups["xpage"].Value);
				configSettings.Add("YPAGE", m.Groups["ypage"].Value);
				return configSettings;
			}
			return null;
		}
		static NameValueCollection parsePrinterSettingsIni( string formName, string iniFileName)
		{
			NameValueCollection configSettings = null;
			StreamReader iniFile;
			try
			{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				iniFile = new StreamReader( iniFileName);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				string currLine;
				while (( currLine = iniFile.ReadLine()) != null)
				{
					
					if ( currLine.Trim().ToUpper() == "["+formName.Trim().ToUpper()+"]")
					{
						
						configSettings = new NameValueCollection();
						while (( currLine = iniFile.ReadLine()) != null)
						{
							int startValue = currLine.IndexOf('=')+1;
							if (startValue == 0 )
								break;
							int lineLength = currLine.Length;
							string printPropValue = currLine.Substring(startValue, lineLength-startValue);
							string printPropName = currLine.Substring(0,startValue-1).ToUpper();
							switch (printPropName)
							{
								case "PRINTER":
								case "MODE":
								case "ORIENTATION":
								case "PAPERSIZE":
								case "PAPERLENGTH":
								case "PAPERWIDTH":
								case "SCALE":
								case "COPIES":
								case "DEFAULTSOURCE":
								case "PRINTQUALITY":
								case "COLOR":
								case "DUPLEX":
									configSettings.Add( printPropName, printPropValue);
									break;
							}
						}
						break;
					}
				}
				iniFile.Close();
			}
			catch
			{
				return null;
			}
			return configSettings;
		}
	}
	
	public class GxReportBuilderDll : IReportHandler
	{
		private int M_top, M_bot;

		[DllImport("rbuilder.dll", EntryPoint="GxPrInit")]
		static extern int gxPrInit(ref int hdc, int gxsoutput, ref int gxXPage, ref int gxYPage, int gxiniFile, string gxd, string gxsform, string gxsprinter, int gxMode, int gxorientation, int gxpageSize, int gxpageLength, int gxpageWidth, int gxScale, int gxCopies, int DefSrc, int gxquality, int gxcolor, int gxDuplex);
		[DllImport("rbuilder.dll", EntryPoint="GxPrnCfg")]
		static extern int gxPrnCfg(string gxs);
		[DllImport("rbuilder.dll", EntryPoint="GxStartPg")]
		static extern int gxStartPg( int hDC, int gxFlag );
		[DllImport("rbuilder.dll", EntryPoint="GxEndPg")]
		static extern int gxEndPg( int hDC, int gxFlag );
		[DllImport("rbuilder.dll",EntryPoint="GxEndDoc")]
		static extern int gxEndDoc( int hDC, int gxFlag );
		[DllImport("rbuilder.dll",EntryPoint="GxEndPrn")]
		static extern int gxEndPrn( int hDC, int gxFlag );
		[DllImport("rbuilder.dll",EntryPoint="GxDwLine")]
		static extern int gxDwLine( int hDC, int nleft, int ntop, int nright, int nbottom, int nPen, int nFrontColR, int nFrontColG, int nFrontColB);
		
		[DllImport("rbuilder.dll", EntryPoint="GxDwRect")]
		static extern int gxDwRect( int hDC, int nleft, int ntop, int nright, int nbottom, int nPen, int nFrontColR, int nFrontColG, int nFrontColB, int nBackMode, int nBackColR, int nBackColG, int nBackColB );
		
		[DllImport("rbuilder.dll", EntryPoint="GxDwText")]
		static extern int gxDwText( int hDC, string psText, int nL, int nT, int nR, int nB, string sFName, int nFSize, int nAlign, int nFBold, int nFItalic, int nFUnder, int nFStrike, int nPen, int nFR, int nFG, int nFB, int nBm, int nBR, int nBG, int nBB);
		[DllImport("rbuilder.dll", EntryPoint="GxDwBMap")]
		static extern int gxDwBMap( int hDC, string psName, int nleft, int ntop, int nright, int nbottom );
		
		[DllImport("rbuilder.dll", EntryPoint="GxRptWndMaximize")]
		static extern int gxRptWndMaximize();
		[DllImport("rbuilder.dll", EntryPoint="GxRptWndNormal")]
		static extern int gxRptWndNormal();
		[DllImport("rbuilder.dll", EntryPoint="GxRptWndOnTop")]
		static extern int gxRptWndOnTop();
		[DllImport("rbuilder.dll", EntryPoint="GxIsAlive")]
		static extern int gxIsAlive();
		[DllImport("rbuilder.dll", EntryPoint="GxIsAliveDoc")]
		static extern int gxIsAliveDoc( int hDC);
		[DllImport("rbuilder.dll", EntryPoint="GxPrTextInit")]
		static extern int gxPrTextInit( ref int hDC, int soutput, ref int gxXPage, ref int gxYPage, string iniFile, string sform, string sprinter, int nMode, int pageLength, int gxpageWidth, int nGridX, int nGridY, int nPageLines );
		
		[DllImport("rbuilder.dll", EntryPoint="GxOpenDoc")]
		static extern int gxOpenDoc(string fileName);

		[DllImport("rbuilder.dll", EntryPoint="GxRptSilentMode")]
		static extern int gxRptSilentMode();

		[DllImport("rbuilder.dll", EntryPoint="GxPrnCmd")]
		static extern int gxPrnCmd( int hDC, string sChars);
		
		[DllImport("rbuilder.dll", EntryPoint="GxSetDocName")]
		static extern int gxSetDocName( int hDC, string psDocName);
		[DllImport("rbuilder.dll", EntryPoint="GxSetDocFormat")]
		static extern int gxSetDocFormat( int hDC, int nFmt );
		[DllImport("rbuilder.dll", EntryPoint="GxRVSetLanguage")]
		static extern int gxRVSetLanguage( string sLang);

		[DllImport("rbuilder.dll", EntryPoint = "GxShutdown")]
		static extern int gxShutdown();

		int handle;

		private string fontName;
		private int fontSize;
		private int fontBold;
		private int fontItalic;
		private int fontUnderline;
		private int fontStrikethru;
		private int Pen;
		private int foreRed;
		private int foreGreen;
		private int foreBlue;
		private int backMode; 
		private int backRed;
		private int backGreen;
		private int backBlue;

		int pageLines;
		int lineHeight;
		int page;
		private bool modal;

		string _appPath;

		public GxReportBuilderDll(string appPath)
		{
			_appPath = appPath;
		}
		public bool GxPrintInit(string output, ref int gxXPage, ref int gxYPage, string iniFile, string form, string printer, int mode, int orientation, int pageSize, int pageLength, int pageWidth, int scale, int copies, int defSrc, int quality, int color, int duplex) 
		{
			int ret = gxPrInit( ref handle, getOutputCode(output), ref gxXPage, ref gxYPage, 0, iniFile, form, printer, mode, orientation, pageSize, pageLength, pageWidth, scale, copies, defSrc, quality, color, duplex);
			return ret == 0 ? false : true; 
		}
		public bool GxPrTextInit(string output, ref int nxPage, ref int nyPage, string psIniFile, string psForm, string sPrinter, int nMode, int nPaperLength, int nPaperWidth, int nGridX, int nGridY, int nPageLines)
		{
			int ret = gxPrTextInit(ref handle, getOutputCode(output), ref nxPage, ref nyPage, psIniFile, psForm, sPrinter, nMode, nPaperLength, nPaperWidth, nGridX, nGridY, nPageLines);
			return ret == 0 ? false : true; 
		}

		public bool GxOpenDoc(string fileName)
		{
			int ret = gxOpenDoc(fileName);
			return ret == 0 ? false : true; 
		}
		public bool GxRptSilentMode()
		{
			int ret = gxRptSilentMode();
			return ret == 0 ? false : true; 
		}
	
		public virtual void GxStartDoc()
		{
		}
		public void GxStartPage()
		{
			int ret = gxStartPg(handle, 0);
			if (ret == 0) 
				throw new ProcessInterruptedException();
		} 
		public void GxEndPage()
		{
			int ret = gxEndPg(handle, 0);
			if (ret == 0) 
				throw new ProcessInterruptedException();
		}
		public void GxEndDocument()
		{
			int ret = gxEndDoc(handle, 0);
			if (ret == 0) 
				throw new ProcessInterruptedException();
		}
		public void GxEndPrinter()
		{
			int ret = gxEndPrn(handle, 0);
			if (ret == 0) 
				throw new ProcessInterruptedException();
		}
        public void GxDrawText(string text, int left, int top, int right, int bottom, int align)
        {
            GxDrawText(text, left, top, right, bottom, align, 0);
        }
        public void GxDrawText(string text, int left, int top, int right, int bottom, int align, int htmlformat)
        {
            GxDrawText(text, left, top, right, bottom, align, htmlformat, 0);
        }
		public void GxDrawText(string text, int left, int top, int right, int bottom, int align, int htmlformat, int border)
		{
			GxDrawText(text, left, top, right, bottom, align, htmlformat, border, 0);
		}
		public void GxDrawText(string text, int left, int top, int right, int bottom, int align, int htmlformat, int border, int valign)
		{
			int ret = gxDwText(handle, text, left, top, right, bottom, fontName, fontSize, align, fontBold, fontItalic, fontUnderline, fontStrikethru, Pen, foreRed, foreGreen, foreBlue, backMode, backRed, backGreen, backBlue);
			if (ret == 0)
				throw new ProcessInterruptedException();
		}
		public void GxDrawLine(int left, int top, int right, int bottom, int width, int foreRed, int foreGreen, int foreBlue)
		{
			GxDrawLine(left, top, right, bottom, width, foreRed, foreGreen, foreBlue, 0);
		}
		public void GxDrawLine(int left, int top, int right, int bottom, int width, int foreRed, int foreGreen, int foreBlue, int style)
		{
			int ret = gxDwLine(handle, left, top, right, bottom, width, foreRed, foreGreen, foreBlue);
			if (ret == 0)
				throw new ProcessInterruptedException();
		}
		public void GxDrawRect(int left, int top, int right, int bottom, int pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue)
		{
			GxDrawRect(left, top, right, bottom, pen, foreRed, foreGreen, foreBlue, backMode, backRed, backGreen, backBlue, 0, 0);
		}
		public void GxDrawRect(int left, int top, int right, int bottom, int pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue, int style, int cornerRadius)
		{
			GxDrawRect(left, top, right, bottom, pen, foreRed, foreGreen, foreBlue, backMode, backRed, backGreen, backBlue, 0, 0, 0, 0, 0, 0, 0, 0);
		}
		public void GxDrawRect(int left, int top, int right, int bottom, int pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue, 
			int styleTop, int styleBottom, int styleRight, int styleLeft, int cornerRadioTL, int cornerRadioTR, int cornerRadioBL, int cornerRadioBR)
		{
			int ret = gxDwRect(handle, left, top, right, bottom, pen, foreRed, foreGreen, foreBlue, backMode, backRed, backGreen, backBlue);
			if (ret == 0)
				throw new ProcessInterruptedException();
		}
		public void GxDrawBitMap(string bitmap, int left, int top, int right, int bottom)
		{
			GxDrawBitMap(bitmap, left, top, right, bottom, 0);
		}
		public void GxDrawBitMap(string bitmap, int left, int top, int right, int bottom, int aspectRatio)
		{
			bitmap = ReportUtils.AddPath(bitmap, _appPath);
			int ret = gxDwBMap(handle, bitmap, left, top, right, bottom);
			if (ret == 0)
				throw new ProcessInterruptedException();
		}
		public virtual void GxAttris(string fontName, int fontSize, bool fontBold, bool fontItalic, bool fontUnderline, bool fontStrikethru, int Pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue)
		{
			this.fontName 		  = fontName;
			this.fontSize 		  = fontSize;
			this.fontBold		  = fontBold ? 1:0;
			this.fontItalic		  = fontItalic ? 1:0;
			this.fontUnderline	  = fontUnderline ? 1:0;
			this.fontStrikethru	  = fontStrikethru ? 1:0;
			this.Pen			  = Pen;
			this.foreRed		  = foreRed;
			this.foreGreen		  = foreGreen;
			this.foreBlue 		  = foreBlue;
			this.backMode 		  = backMode;
			this.backRed 		  = backRed;
			this.backGreen 		  = backGreen;
			this.backBlue		  = backBlue;
		}
		public virtual void GxClearAttris()
		{
			GxAttris("Courier New", 9, false, false, false, false, 0, 0, 0, 0, 0, 255, 255, 255);
		}
		public void GxRVSetLanguage(string lang)
		{
			gxRVSetLanguage(lang);
		}
		public void GxSetDocName(string docName)
		{
			docName = ReportUtils.AddPath( docName, _appPath );
			int ret = gxSetDocName(handle, docName);
			if (ret == 0) 
				throw new ProcessInterruptedException();
		}
		public void GxSetDocFormat(string docFormat)
		{
			int format = 0;
			switch ( docFormat.ToUpper())
			{
				case "GXR":
					format = 0;
					break;
				case "RTF":
					format = 1;
					break;
				case "HTML":
					format = 2;
					break;
				case "TXT":
					format = 3;
					break;
			}
			int ret = gxSetDocFormat(handle, format);
			if (ret == 0) 
				throw new ProcessInterruptedException();
		}
		public virtual void setModal(bool modal)
		{
			this.modal = modal;
		}
		public virtual bool getModal()
		{
			return modal;
		}
		public virtual void setPageLines(int pageLines) 
		{
			this.pageLines = pageLines;
		}
		public virtual int getPageLines() 
		{
			return pageLines;
		}
		public virtual void setLineHeight(int lineHeight) 
		{
			this.lineHeight = lineHeight;
		}
		public virtual int getLineHeight() 
		{
			return lineHeight;
		}
		public virtual void setPage(int page) 
		{
			this.page = page;
		}
		public virtual int getPage() 
		{
			return page;
		}
		public void GxPrintMax() 
		{
			gxRptWndMaximize();
		}
		public void GxPrintNormal () 
		{
			gxRptWndNormal();
		}
		public void GxPrintOnTop() 
		{
			gxRptWndOnTop();
		}
		public void GxPrnCmd()
		{
		}
		public void GxPrnCmd(string scmd)
		{
			int ret = gxPrnCmd(handle, scmd);
			if (ret == 0) 
				throw new ProcessInterruptedException();
		}
		public void GxShutdown()
		{
			gxShutdown();
		}
		public bool GxIsAlive() 
		{
			int ret = gxIsAlive();
			return ret == 0 ? false : true;
		}
		public bool GxIsAliveDoc()
		{
			int ret = gxIsAliveDoc(handle);
			return ret == 0 ? false : true;
		}
		public bool GxPrnCfg( string ini )
		{
			gxPrnCfg(ini);
			return true;
 		}
		public virtual void setMetrics(string fontName, bool bold, bool italic, int ascent, int descent, int height, int maxAdvance, int[] sizes)
		{
		}
		protected static int getOutputCode(string output)
		{
			if (output.ToUpper() == "PRN")
				return 0;
			if (output.ToUpper() == "FIL")
				return 2;
			return 1;
		}
	#region IReportHandler Members

		public int getM_top()
		{
			return M_top;
		}

		public int getM_bot()
		{
			return M_bot;
		}

		public void setM_top(int top)
		{
			M_top = top;
		}

		public void setM_bot(int bot)
		{
			M_bot = bot;
		}
        public void setOutputStream(object o) { }

	#endregion
	}
#endif
	[Serializable()]
	public class ProcessInterruptedException: Exception
	{
		public ProcessInterruptedException(SerializationInfo info, StreamingContext ctx)
			: base(info, ctx)
		{
			
		}
		public ProcessInterruptedException()
		{

		}
		public override string Message
		{
			get { return "Process Interrupted";	}
		}
		public override string ToString()
		{
			return Message;
		}
	}
	public class ReportUtils
	{
		static public string AddPath(string name, string path)
		{
			if (Path.IsPathRooted(name) || name.IndexOf(":") != -1 ||
				(name.Length >=2 && (name.Substring( 0,2) == "//" || name.Substring( 0,2) == @"\\")) ||
				(name.StartsWith("http:" ) || name.StartsWith("https:" )))
				return name;
			return Path.Combine(path, name);
		}
	}

    public class GXReportMetadata
    {
        private Hashtable hash = new Hashtable();
        private Hashtable attriHash = new Hashtable();
        private string fileName;
        private IReportHandler reportHandler;

        public GXReportMetadata(string fileName, IReportHandler reportHandler) 
        {
            this.fileName = Preferences.getPRINT_LAYOUT_METADATA_DIR() + fileName + ".rpt";
            this.reportHandler = reportHandler;
        }

        public void load()
        {                        
            string physicalPath = Application.GxContext.StaticPhysicalPath();
            string fileNamePhysical = Path.Combine(physicalPath, fileName);
            if (!File.Exists(fileNamePhysical) && physicalPath.EndsWith("bin"))
            {
                fileNamePhysical = Path.Combine(Directory.GetParent(physicalPath).FullName, fileName);
            }
            GXXMLReader reader = new GXXMLReader();
            reader.Open(fileNamePhysical);
		    if(reader.ErrCode != 0)
		    {
                Console.WriteLine("ERROR1");
                Console.WriteLine("Error opening metadata file: " + fileName);
			    return;
		    }		
		
		    while(reader.ReadType(1, "PrintBlock") >0)
		    {
			    processPrintBlock(reader);
		    }
		    reader.Close();
        }

        private void processPrintBlock(GXXMLReader reader)
        {
            short result;
            result = reader.Read(); 
            result = reader.Read();
            while (!(reader.Name == "PrintBlock" && (reader.NodeType == 2)) && result != 0)
            {
                if (reader.Name == "ReportLabel" && (reader.NodeType == 1) && (reader.IsSimple == 0))
                {
                    processReportLabel(reader, 0);
                }
                if (reader.Name == "ReportAttribute" && (reader.NodeType == 1) && (reader.IsSimple == 0))
                {
                    processReportLabel(reader, 1);
                }
                if (reader.Name == "ReportLine" && (reader.NodeType == 1) && (reader.IsSimple == 0))
                {
                    processReportLine(reader);
                }
                if (reader.Name == "ReportRectangle" && (reader.NodeType == 1) && (reader.IsSimple == 0))
                {
                    processReportRectangle(reader);
                }
                if (reader.Name == "ReportImage" && (reader.NodeType == 1) && (reader.IsSimple == 0))
                {
                    processReportImage(reader);
                }
                if (reader.Name == "Properties" && (reader.NodeType == 1) && (reader.IsSimple == 0))
                {
                    processProperties(reader);
                }
                result = reader.Read();
            }
        }

        private void processReportLabel(GXXMLReader reader, int type)
        {
            short result;
            int key;
            string sTxt;
            int left;
            int top;
            int right;
            int width;
            int defaultWidth;
            int bottom;
            int align;
            int valign;
            int htmlformat = 0;
            int border = 0;
            string aligment;
            RGB foreColor;
            int backMode = 1;
            RGB backColor;
            string fontInfo;
            bool visible;
            bool wordwrap;

            result = reader.ReadType(1, "RPT_ID");
            key = Convert.ToInt32(reader.Value);

            result = reader.ReadType(1, "RPT_VISIBLE");
            visible = reader.Value == "True";

            result = reader.ReadType(1, "RPT_TEXT");
            sTxt = reader.Value;

            result = reader.ReadType(1, "RPT_X");
            left = Convert.ToInt32(reader.Value);

            result = reader.ReadType(1, "RPT_Y");
            top = Convert.ToInt32(reader.Value);

            result = reader.ReadType(1, "RPT_WIDTH");
            width = Convert.ToInt32(reader.Value);
            right = left + width;

            if (type == 0)
            {
                defaultWidth = width;
            }
            else
            {
                result = reader.ReadType(1, "RPT_WIDTH_Default");
                defaultWidth = Convert.ToInt32(reader.Value);
            }

            result = reader.ReadType(1, "RPT_HEIGHT");
            bottom = top + Convert.ToInt32(reader.Value);

            result = reader.ReadType(1, "RPT_FORECOLOR");
            foreColor = parseRGB(reader.Value);

            result = reader.ReadType(1, "RPT_BACKCOLOR");
            backColor = parseRGB(reader.Value);

            if (reader.Value == "Transparent, ARGB(0,255,255,255)")
            {
                backMode = 0;
            }

            result = reader.ReadType(1, "RPT_BORDERS");
            border = reader.Value.Equals("None") ? 0 : 1;

            result = reader.ReadType(1, "RPT_ALIGNMENT");
            aligment = reader.Value;

            result = reader.ReadType(1, "RPT_FONT");
            fontInfo = reader.Value;

            if (type == 1) //Attribute
            {
                result = reader.ReadType(1, "GxFormat");
                if (result > 0 && reader.Value.IndexOf("HTML") >= 0)
                    htmlformat = 1;
            }

            result = reader.ReadType(1, "RPT_WORDWRAP");
            wordwrap = reader.Value == "True";
            align = parseAlignment(aligment, wordwrap, width, defaultWidth);
            valign = parseVerticalAlignment(aligment);

            parseAttris(key, fontInfo, foreColor, backMode, backColor);

            if (visible)
            {
                DrawText dt;
                if (type == 0)
                {
                    result = reader.ReadType(2, "ReportLabel");
                    dt = new DrawText(sTxt, left, top, right, bottom, align, valign, htmlformat, border);
                }
                else
                {
                    result = reader.ReadType(2, "ReportAttribute");
                    dt = new DrawText(null, left, top, right, bottom, align, valign, htmlformat, border);
                }
                hash.Add(key, dt);
            }
        }

        private void processReportLine(GXXMLReader reader)
        {
            int key;
            int left;
            int top;
            int right;
            int bottom;
            int widht;
            RGB rgb;
            bool visible;

            reader.ReadType(1, "RPT_ID");
            key = Convert.ToInt32(reader.Value);

            reader.ReadType(1, "RPT_VISIBLE");
            visible = reader.Value == "True";

            reader.ReadType(1, "RPT_X");
            left = Convert.ToInt32(reader.Value); ;

            reader.ReadType(1, "RPT_Y");
            top = Convert.ToInt32(reader.Value);

            reader.ReadType(1, "RPT_WIDTH");
            right = Convert.ToInt32(reader.Value);

            reader.ReadType(1, "RPT_HEIGHT");
            bottom = Convert.ToInt32(reader.Value);

            reader.ReadType(1, "RPT_FORECOLOR");
            rgb = parseRGB(reader.Value);

            reader.ReadType(1, "RPT_LINEDIRECTION");
            if (reader.Value == "Horizontal")
            {
                right = left + right;
                bottom = top;
            }
            else
            {
                right = left;
                bottom = top + bottom;
            }

            reader.ReadType(1, "RPT_LINEWIDTH");
            widht = Convert.ToInt32(reader.Value);

            reader.ReadType(2, "ReportLine");

            if (visible)
            {
                DrawLine dl = new DrawLine(left, top, right, bottom, widht, rgb.getRed(), rgb.getGreen(), rgb.getBlue(), 0);
                hash.Add(key, dl);
            }
        }

        private void processReportRectangle(GXXMLReader reader)
        {
            int key;
            int left;
            int top;
            int right;
            int bottom;
            int pen;
            int backMode = 1;
            RGB ForeRgb;
            RGB BackRgb;
            bool visible;

            reader.ReadType(1, "RPT_ID");
            key = Convert.ToInt32(reader.Value);

            reader.ReadType(1, "RPT_VISIBLE");
            visible = reader.Value == "True";

            reader.ReadType(1, "RPT_X");
            left = Convert.ToInt32(reader.Value);

            reader.ReadType(1, "RPT_Y");
            top = Convert.ToInt32(reader.Value);

            reader.ReadType(1, "RPT_WIDTH");
            right = left + Convert.ToInt32(reader.Value); ;

            reader.ReadType(1, "RPT_HEIGHT");
            bottom = top + Convert.ToInt32(reader.Value);

            reader.ReadType(1, "RPT_BACKCOLOR");
            if (reader.Value == "Transparent, ARGB(0,255,255,255)")
            {
                backMode = 0;
            }
            BackRgb = parseRGB(reader.Value);

            reader.ReadType(1, "RPT_BORDERWIDTH");
            pen = Convert.ToInt32(reader.Value);

            reader.ReadType(1, "RPT_BORDERCOLOR");
            ForeRgb = parseRGB(reader.Value);

            reader.ReadType(2, "ReportRectangle");

            if (visible)
            {
                DrawRect dr = new DrawRect(left, top, right, bottom, pen, ForeRgb.getRed(), ForeRgb.getGreen(), ForeRgb.getBlue(), backMode, BackRgb.getRed(), BackRgb.getGreen(), BackRgb.getBlue(), 0, 0, 0, 0, 0, 0, 0, 0);
                hash.Add(key, dr);
            }
        }

        private void processReportImage(GXXMLReader reader)
        {
            int key;
            int left;
            int top;
            int right;
            int bottom;
            bool visible;

            reader.ReadType(1, "RPT_ID");
            key = Convert.ToInt32(reader.Value);

            reader.ReadType(1, "RPT_VISIBLE");
            visible = reader.Value == "True";

            reader.ReadType(1, "RPT_X");
            left = Convert.ToInt32(reader.Value);

            reader.ReadType(1, "RPT_Y");
            top = Convert.ToInt32(reader.Value);

            reader.ReadType(1, "RPT_WIDTH");
            right = left + Convert.ToInt32(reader.Value);

            reader.ReadType(1, "RPT_HEIGHT");
            bottom = top + Convert.ToInt32(reader.Value);

            reader.ReadType(2, "ReportImage");

            if (visible)
            {
                DrawText dt = new DrawText(null, left, top, right, bottom, 0);
                hash.Add(key, dt);
            }
        }

        private void processProperties(GXXMLReader reader)
        {
            int key;
            int height;
            bool visible;

            reader.ReadType(1, "RPT_ID");
            key = Convert.ToInt32(reader.Value);

            reader.ReadType(1, "RPT_VISIBLE");
            visible = reader.Value == "True";

            reader.ReadType(1, "RPT_HEIGHT");
            height = Convert.ToInt32(reader.Value);

            if (visible)
            {
                PrintblockProperties dt = new PrintblockProperties(height);
                hash.Add(key, dt);
            }
        }

        private int parseAlignment(string txt, bool wordwrap, int width, int defaultWidth)
        {
            if (txt.EndsWith("Right"))
            {
                if (wordwrap)
                {
                    return 2 + 16;
                }
                if (width != defaultWidth)
                {
                    return 2;
                }
                return 2 + 256;
            }
            if (txt.EndsWith("Center"))
            {
                if (wordwrap)
                {
                    return 1 + 16;
                }
                if (width != defaultWidth)
                {
                    return 1;
                }
                return 1 + 256;
            }
            if (txt.EndsWith("Justify"))
            {
                if (wordwrap)
                {
                    return 3 + 16;
                }
                if (width != defaultWidth)
                {
                    return 3;
                }
                return 3 + 256;
            }
            if (wordwrap)
            {
                return 0 + 16;
            }
            if (width != defaultWidth)
            {
                return 0;
            }
            return 0 + 256;
        }

        private int parseVerticalAlignment(string txt)
        {
            if (txt.StartsWith("Top"))
            {
                return 0;
            }
            if (txt.StartsWith("Middle"))
            {
                return 1;
            }
            return 2;
        }

        private RGB parseRGB(String txt)
        {
            string[] txts = txt.Substring(0, txt.Length - 1).Split(',');
            RGB rgb = new RGB(Convert.ToInt32(txts[2]), Convert.ToInt32(txts[3]), Convert.ToInt32(txts[4]));
            return rgb;
        }

        private void parseAttris(int key, string fontInfo, RGB foreColor, int backMode, RGB backColor)
        {
            String fontName;
            int fontSize;
            bool fontBold = false;
            bool fontItalic = false;
            bool fontUnderline = false;
            bool fontStrikethru = false;

            string[] fontInfos = fontInfo.Split(',');
            fontName = fontInfos[0];

            float aux = float.Parse(fontInfos[1].Trim().Replace("pt", ""), CultureInfo.InvariantCulture);
            fontSize = Convert.ToInt32(Math.Round(aux));

            if (fontInfos.Length > 2)
            {
                if (fontInfos[2].IndexOf("Bold") != -1)
                {
                    fontBold = true;
                }
                if (fontInfos[2].IndexOf("Italic") != -1)
                {
                    fontItalic = true;
                }
                if (fontInfos[2].IndexOf("Underline") != -1)
                {
                    fontUnderline = true;
                }
                if (fontInfos[2].IndexOf("Strikeout") != -1)
                {
                    fontStrikethru = true;
                }
            }

            Attris atts = new Attris(fontName, fontSize, fontBold, fontItalic, fontUnderline, fontStrikethru, foreColor, backMode, backColor);
            attriHash.Add(key, atts);
        }

        public void GxDrawText(int printBlock, int controlId, int line)
        {
            int key = controlId;
            Attris att = (Attris)attriHash[key];
            if (att != null)
            {
                reportHandler.GxAttris(att.fontName, att.fontSize, att.fontBold, att.fontItalic, att.fontUnderline, att.fontStrikethru, 0, att.foreColor.getRed(), att.foreColor.getGreen(), att.foreColor.getBlue(), att.backMode, att.backColor.getRed(), att.backColor.getGreen(), att.backColor.getBlue());
            }
            DrawText dt = (DrawText)hash[key];
            if (dt != null)
            {
                reportHandler.GxDrawText(dt.sTxt, dt.left, line + dt.top, dt.right, line + dt.bottom, dt.align, dt.htmlformat, dt.border, dt.valign);
            }
        }

        public void GxDrawText(int printBlock, int controlId, int line, String value)
        {
            int key = controlId;
            Attris att = (Attris)attriHash[key];
            if (att != null)
            {
                reportHandler.GxAttris(att.fontName, att.fontSize, att.fontBold, att.fontItalic, att.fontUnderline, att.fontStrikethru, 0, att.foreColor.getRed(), att.foreColor.getGreen(), att.foreColor.getBlue(), att.backMode, att.backColor.getRed(), att.backColor.getGreen(), att.backColor.getBlue());
            }
            DrawText dt = (DrawText)hash[key];
            if (dt != null)
            {
                reportHandler.GxDrawText(value, dt.left, line + dt.top, dt.right, line + dt.bottom, dt.align, dt.htmlformat, dt.border, dt.valign);
            }
        }

        public void GxDrawLine(int printBlock, int controlId, int line)
        {
            int key = controlId;
            DrawLine dl = (DrawLine)hash[key];
            if (dl != null)
            {
                reportHandler.GxDrawLine(dl.left, line + dl.top, dl.right, line + dl.bottom, dl.widht, dl.foreRed, dl.foreGreen, dl.foreBlue, dl.style);
            }
        }

        public void GxDrawRect(int printBlock, int controlId, int line)
        {
            int key = controlId;
            DrawRect dr = (DrawRect)hash[key];
            if (dr != null)
            {
                reportHandler.GxDrawRect(dr.left, line + dr.top, dr.right, line + dr.bottom, dr.pen, dr.foreRed, dr.foreGreen, dr.foreBlue, dr.backMode, dr.backRed, dr.backGreen, dr.backBlue, dr.styleTop, dr.styleBottom, dr.styleRight, dr.styleLeft, dr.cornerRadioTL, dr.cornerRadioTR, dr.cornerRadioBL, dr.cornerRadioBR);
            }
        }

        public void GxDrawBitMap(int printBlock, int controlId, int line, String value, int aspectRatio)
        {
            int key = controlId;
            DrawText db = (DrawText)hash[key];
            if (db != null)
            {
                reportHandler.GxDrawBitMap(value, db.left, line + db.top, db.right, line + db.bottom, aspectRatio);
            }
        }

        public int GxDrawGetPrintBlockHeight(int printBlock)
        {
            int key = printBlock;
            PrintblockProperties db = (PrintblockProperties)hash[key];
            return db.height;
        }

        class DrawText
        {
            public string sTxt;
            public int left;
            public int top;
            public int right;
            public int bottom;
            public int align;
            public int valign;
            public int htmlformat;
            public int border;
            public DrawText(string sTxt, int left, int top, int right, int bottom, int align, int valign, int htmlformat, int border)
            {
                this.border = border;
                this.htmlformat = htmlformat;
                this.sTxt = sTxt;
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
                this.align = align;
                this.valign = valign;
            }
            public DrawText(string sTxt, int left, int top, int right, int bottom, int align)
            {
                this.sTxt = sTxt;
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
                this.align = align;
            }
        }

        class DrawLine
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public int widht;
            public int foreRed;
            public int foreGreen;
            public int foreBlue;
            public int style;

            public DrawLine(int left, int top, int right, int bottom, int widht, int foreRed, int foreGreen, int foreBlue, int style)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
                this.widht = widht;
                this.foreRed = foreRed;
                this.foreGreen = foreGreen;
                this.foreBlue = foreBlue;
                this.style = style;
            }
        }

        class DrawRect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public int pen;
            public int foreRed;
            public int foreGreen;
            public int foreBlue;
            public int backMode;
            public int backRed;
            public int backGreen;
            public int backBlue;
            public int styleTop;
            public int styleBottom;
            public int styleRight;
            public int styleLeft;
            public int cornerRadioTL;
            public int cornerRadioTR;
            public int cornerRadioBL;
            public int cornerRadioBR;

            public DrawRect(int left, int top, int right, int bottom, int pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue, int styleTop, int styleBottom, int styleRight, int styleLeft, int cornerRadioTL, int cornerRadioTR, int cornerRadioBL, int cornerRadioBR)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
                this.pen = pen;
                this.foreRed = foreRed;
                this.foreGreen = foreGreen;
                this.foreBlue = foreBlue;
                this.backMode = backMode;
                this.backRed = backRed;
                this.backGreen = backGreen;
                this.backBlue = backBlue;
                this.styleTop = styleTop;
                this.styleBottom = styleBottom;
                this.styleRight = styleRight;
                this.styleLeft = styleLeft;
                this.cornerRadioTL = cornerRadioTL;
                this.cornerRadioTR = cornerRadioTR;
                this.cornerRadioBL = cornerRadioBL;
                this.cornerRadioBR = cornerRadioBR;
            }
        }

        class RGB
        {
            int foreRed;
            int foreGreen;
            int foreBlue;

            public RGB(int foreRed, int foreGreen, int foreBlue)
            {
                this.foreRed = foreRed;
                this.foreGreen = foreGreen;
                this.foreBlue = foreBlue;
            }

            public int getRed()
            {
                return this.foreRed;
            }

            public int getGreen()
            {
                return this.foreGreen;
            }

            public int getBlue()
            {
                return this.foreBlue;
            }
        }

        class Attris
        {
            public string fontName;
            public int fontSize;
            public bool fontBold;
            public bool fontItalic;
            public bool fontUnderline;
            public bool fontStrikethru;
            public RGB foreColor;
            public int backMode;
            public RGB backColor;

            public Attris(string fontName, int fontSize, bool fontBold, bool fontItalic, bool fontUnderline, bool fontStrikethru, RGB foreColor, int backMode, RGB backColor)
            {
                this.fontName = fontName;
                this.fontSize = fontSize;
                this.fontBold = fontBold;
                this.fontItalic = fontItalic;
                this.fontUnderline = fontUnderline;
                this.fontStrikethru = fontStrikethru;
                this.foreColor = foreColor;
                this.backMode = backMode;
                this.backColor = backColor;
            }
        }

        class PrintblockProperties
        {
            public int height;

            public PrintblockProperties(int height)
            {
                this.height = height;
            }
        }
    }
}
