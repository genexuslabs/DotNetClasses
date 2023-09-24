
using System;

using System.IO;
using System.Collections;
using System.Threading;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Globalization;

using System.util;
using System.Diagnostics;
using log4net;

using GeneXus.Printer;
using System.Collections.Generic;
using System.Security;
using GeneXus;
using GeneXus.Utils;
using System.Reflection;
using GeneXus.Metadata;

namespace com.genexus.reports
{

	internal enum VerticalAlign
	{
		TOP = 0,
		MIDDLE = 1,
		BOTTOM = 2,
	}

	public abstract class PDFReportItextBase : IReportHandler
	{
		protected int lineHeight, pageLines;

		static IGXLogger log = GXLoggerFactory.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName);

		protected bool fontUnderline;
		protected bool fontStrikethru;
		protected int fontSize;
		protected string language;
   
		protected Stream outputStream = null;

		protected static object syncRoot = new Object();
		protected static ParseINI props;
		protected ParseINI printerSettings;
		protected String form;
		protected ArrayList stringTotalPages;
		protected int outputType = -1;
		protected int printerOutputMode = -1;
		protected bool modal = false;
		protected String docName = "PDFReport.pdf";
		protected static NativeSharpFunctionsMS nativeCode = new NativeSharpFunctionsMS();
		protected static Hashtable fontSubstitutes = new Hashtable();
		protected static String configurationFile = null;
		protected static String configurationTemplateFile = null;
		protected static String defaultRelativePrepend = null;
		protected static String webAppDir = null;
		
		public static bool DEBUG = false;
		protected static String predefinedSearchPath = "";
		protected float leftMargin;
		protected float topMargin;
		protected float bottomMargin;
		protected int templateFontSize;
		protected bool backFill = true;
		protected int templateAlignment;
		protected int pages = 0;
		protected bool templateCreated = false;
		protected static char[] TRIM_CHARS = { ' ' };
		protected int M_top, M_bot;
		public static float DASHES_UNITS_ON = 10;
		public static float DASHES_UNITS_OFF = 10;
		public static float DOTS_UNITS_OFF = 3;
		public static float DOTS_UNITS_ON = 1;
		public bool lineCapProjectingSquare = true;
		public bool barcode128AsImage = true;
		protected PdfConformanceLevel complianceLevel = PdfConformanceLevel.None;
		protected float[] STYLE_SOLID = new float[] { 1, 0 };//0
		protected float[] STYLE_NONE = null;//1
		protected float[] STYLE_DOTTED, //2
			STYLE_DASHED, //3
			STYLE_LONG_DASHED, //4
			STYLE_LONG_DOT_DASHED; //5
		protected int STYLE_NONE_CONST = 1;
		protected int runDirection;
		protected int MULTIPLIED_LEADING = 1;

		public bool GxOpenDoc(string fileName)
		{
			return false;
		}
		public bool GxRptSilentMode()
		{
			return false;
		}

		public void setOutputStream(Object outputStream)
		{
			GXLogging.Debug(log,"setOutputStream " + outputStream.GetType().ToString());

			this.outputStream = (Stream)outputStream;
		}

		private static String getAcrobatLocation()
		{
			ParseINI props;
			try
			{
				props = new ParseINI(configurationFile, configurationTemplateFile);
				if (new FileInfo(configurationFile).Length == 0) File.Delete(configurationFile);
			}
			catch (IOException) { props = new ParseINI(); }
			
			String acrobatLocation = props.getGeneralProperty(Const.ACROBAT_LOCATION); 
			if (acrobatLocation == null && GXUtil.IsWindowsPlatform)
			{
				try
				{
					acrobatLocation = (String)ParseINI.parseLine(nativeCode.getRegistryValue(Registry.ClassesRoot.Name, Const.DEFAULT_ACROBAT_LOCATION, ""), " ")[0];
				}
				catch (Exception)//AcrobatNotFound
				{
					try
					{
						acrobatLocation = (String)ParseINI.parseLine(nativeCode.getRegistryValue(Registry.ClassesRoot.Name, Const.DEFAULT_ACROREAD_LOCATION, ""), " ")[0];
					}
					catch (Exception)//lookAgain
					{
						try
						{
							acrobatLocation = (String)ParseINI.parseLine(nativeCode.getRegistryValue(Registry.LocalMachine.Name, Const.DEFAULT_ACROBAT_EX_LOCATION, ""), " ")[0];
						}
						catch (Exception)//lookAgain2
						{
							try
							{
								acrobatLocation = (String)ParseINI.parseLine(nativeCode.getRegistryValue(Registry.ClassesRoot.Name, Const.DEFAULT_ACROBAT_EX_LOCATION2, ""), " ")[0];
							}
							catch
							{
								acrobatLocation = (String)ParseINI.parseLine(nativeCode.getRegistryValue(Registry.ClassesRoot.Name, Const.DEFAULT_ACROREAD_LOCATION2, ""), " ")[0];

							}
						}
					}
				}
			}
			return acrobatLocation;
		}

		public static void printReport(String pdfFilename, bool silent, string printerName)
		{
			pdfFilename = Path.GetFullPath(pdfFilename);
			GXLogging.Debug(log,"printReport pdfFilename:" + pdfFilename);
			try
			{
				silent = true;
				
				String acrobatLocation = string.Format("\"{0}\"", getAcrobatLocation());
				string args = "";
				GXLogging.Debug(log,"printReport silent:" + silent + " acrobatLocation:" + acrobatLocation);

				if (silent)
				{ 

					if (!string.IsNullOrEmpty(printerName))
					{
						args = string.Format("/t \"{0}\" \"{1}\"", pdfFilename, printerName);
					}
					else
					{
						PrinterInfo defaultInfo = nativeCode.getDefaultPrinter();
						args = string.Format("/t \"{0}\" \"{1}\" \"{2}\" \"{3}\"", pdfFilename, defaultInfo.getDeviceName(), defaultInfo.getDriverName(), defaultInfo.getPortName());
					}
					GXLogging.Debug(log,"printReport silent args:" + args);
					nativeCode.shellExecute(acrobatLocation, args); // There is NO executeModal here, because the execution is in 'parallel'
				}
				else
				{
					args = string.Format("/p \"{0}\" ", pdfFilename);
					GXLogging.Debug(log,"printReport args:" + args);
					nativeCode.shellExecute(acrobatLocation, args); 
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log,"printReport error", ex);
				throw new Exception("Acrobat cannot be found in this machine");
			}
		}

		public static void showReport(String filename1, bool modal)
		{
			String filename = "\"" + Path.GetFullPath(filename1) + "\"";
			String acrobatLocation;
			try
			{
				
				acrobatLocation = getAcrobatLocation();
			}
			catch (Exception)
			{
				throw new Exception("Acrobat cannot be found in this machine");
			}

			GXLogging.Debug(log,"Opening document, modal:" + modal);
			if (modal)
			{
				nativeCode.executeModal(acrobatLocation, filename, true);
			}
			else
			{
				System.DateTime lastAccess = System.IO.File.GetLastAccessTime(filename1);
				if (nativeCode.shellExecute(acrobatLocation, filename) == 0)
				{
					long start = System.DateTime.Now.Ticks;
					while (System.IO.File.GetLastAccessTime(filename1) == lastAccess && ((System.DateTime.Now.Ticks - start) / System.TimeSpan.TicksPerSecond) <= 8)
					{
						GXLogging.Debug(log,"Waiting until acrobat opens file... ");
						Thread.Sleep(100);
					}
				}
				else
				{
					GXLogging.Error(log,"Error opening document");
				}
			}
		}

		private static char alternateSeparator = Path.DirectorySeparatorChar == '/' ? '\\' : '/';
		public PDFReportItextBase(String appPath)
		{
			try
			{
				stringTotalPages = new ArrayList();
				defaultRelativePrepend = appPath;
				webAppDir = defaultRelativePrepend;
				if (appPath.Length > 0)
				{
					configurationFile = Path.Combine(defaultRelativePrepend, Const.INI_FILE); 
					configurationTemplateFile = Path.Combine(defaultRelativePrepend, Const.INI_TEMPLATE_FILE);
				}
				else
				{
					configurationFile = Const.INI_FILE;
					configurationTemplateFile = Const.INI_TEMPLATE_FILE;
				}

				loadProps();
#if NETCORE
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
			}
			catch (Exception ex)
			{
				GXLogging.Error(log,"PDFReportItextSharp Ctr error", ex);
				throw ex;
			}
		}

		private void loadPrinterSettingsProps(String iniFile, String form, String printer, int mode, int orientation, int pageSize, int pageLength, int pageWidth, int scale, int copies, int defSrc, int quality, int color, int duplex)
		{
			try
			{
				String configurationFile = iniFile;
				if (defaultRelativePrepend.Length > 0)
				{
					configurationFile = defaultRelativePrepend + Path.DirectorySeparatorChar + iniFile;
				}
				this.form = form;
				if (!File.Exists(configurationFile) && File.Exists("bin/" + iniFile))
				{
					configurationFile = "bin/" + iniFile;
				}
				GXLogging.Debug(log,"LoadingProps from " + configurationFile);
				printerSettings = new ParseINI(configurationFile);
			}
			catch (IOException e)
			{
				GXLogging.Warn(log,"Error loadingProps from " + iniFile, e);
				printerSettings = new ParseINI();
			}
			printerSettings.setupProperty(form, Const.PRINTER, printer);
			printerSettings.setupProperty(form, Const.MODE, mode + "");
			printerSettings.setupProperty(form, Const.ORIENTATION, orientation + "");
			printerSettings.setupProperty(form, Const.PAPERSIZE, pageSize + "");
			printerSettings.setupProperty(form, Const.PAPERLENGTH, pageLength + "");
			printerSettings.setupProperty(form, Const.PAPERWIDTH, pageWidth + "");
			printerSettings.setupProperty(form, Const.SCALE, scale + "");
			printerSettings.setupProperty(form, Const.COPIES, copies + "");
			printerSettings.setupProperty(form, Const.DEFAULTSOURCE, defSrc + "");
			printerSettings.setupProperty(form, Const.PRINTQUALITY, quality + "");
			printerSettings.setupProperty(form, Const.COLOR, color + "");
			printerSettings.setupProperty(form, Const.DUPLEX, duplex + "");
		}
		internal static void SetDefaultComplianceLevel(PdfConformanceLevel level)
		{
			if (props!=null)
				props.setGeneralProperty(Const.COMPLIANCE_LEVEL, level.ToString());
		}
		private void loadProps()
		{
			if (props == null)
			{
				lock (syncRoot)
				{
					if (props == null)
					{
						try
						{
							props = new ParseINI(configurationFile, configurationTemplateFile);
						}
						catch (IOException e)
						{
							GXLogging.Warn(log, "Error loadingProps from " + configurationFile, e);
							props = new ParseINI();
						}
						props.setupGeneralProperty(Const.PDF_REPORT_INI_VERSION_ENTRY, Const.PDF_REPORT_INI_VERSION);

						string fontsDir = nativeCode.getRegistryValue("HKEY_USERS", @".DEFAULT\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "Fonts");
						props.setupGeneralProperty(Const.FONTS_LOCATION, fontsDir);
						props.setupGeneralProperty(Const.MS_FONT_LOCATION, fontsDir);
						props.setupGeneralProperty(Const.EMBEED_SECTION, Const.EMBEED_DEFAULT);
						props.setupGeneralProperty(Const.EMBEED_NOT_SPECIFIED_SECTION, Const.EMBEED_DEFAULT);
						props.setupGeneralProperty(Const.SEARCH_FONTS_ALWAYS, "false");
						props.setupGeneralProperty(Const.SEARCH_FONTS_ONCE, "true");
						props.setupGeneralProperty(Const.SERVER_PRINTING, "false");
						props.setupGeneralProperty(Const.ADJUST_TO_PAPER, "true");
						props.setupGeneralProperty(Const.LINE_CAP_PROJECTING_SQUARE, Const.DEFAULT_LINE_CAP_PROJECTING_SQUARE);
						props.setupGeneralProperty(Const.BARCODE128_AS_IMAGE, Const.DEFAULT_BARCODE128_AS_IMAGE);
						props.setupGeneralProperty("DEBUG", "false");
						props.setupGeneralProperty(Const.LEFT_MARGIN, Const.DEFAULT_LEFT_MARGIN);
						props.setupGeneralProperty(Const.TOP_MARGIN, Const.DEFAULT_TOP_MARGIN);
						props.setupGeneralProperty(Const.BOTTOM_MARGIN, Const.DEFAULT_BOTTOM_MARGIN);
						props.setupGeneralProperty(Const.MARGINS_INSIDE_BORDER, Const.DEFAULT_MARGINS_INSIDE_BORDER.ToString().ToLower());
						props.setupGeneralProperty(Const.OUTPUT_FILE_DIRECTORY, ".");
						props.setupGeneralProperty(Const.LEADING, "2");
						props.setupGeneralProperty(Const.COMPLIANCE_LEVEL, PdfConformanceLevel.None.ToString()); 
						props.setupGeneralProperty(Const.RUN_DIRECTION, Const.RUN_DIRECTION_LTR);
						props.setupGeneralProperty(Const.JUSTIFIED_TYPE_ALL, "false");

						props.setupGeneralProperty(Const.STYLE_DOTTED, Const.DEFAULT_STYLE_DOTTED);
						props.setupGeneralProperty(Const.STYLE_DASHED, Const.DEFAULT_STYLE_DASHED);
						props.setupGeneralProperty(Const.STYLE_LONG_DASHED, Const.DEFAULT_STYLE_LONG_DASHED);
						props.setupGeneralProperty(Const.STYLE_LONG_DOT_DASHED, Const.DEFAULT_STYLE_LONG_DOT_DASHED);

						props.setupGeneralProperty(Const.LEFT_MARGIN, Const.DEFAULT_LEFT_MARGIN);
						props.setupGeneralProperty(Const.TOP_MARGIN, Const.DEFAULT_TOP_MARGIN);
						props.setupGeneralProperty(Const.BOTTOM_MARGIN, Const.DEFAULT_BOTTOM_MARGIN);

						loadSubstituteTable();

						if (props.getBooleanGeneralProperty("DEBUG", false))
						{
							DEBUG = true;
						}
						else
						{
							DEBUG = false;
						}
						try
						{
							string systemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
							if (!string.IsNullOrEmpty(systemFolder))
							{
								Utilities.addPredefinedSearchPaths(new String[]{props.getGeneralProperty(Const.FONTS_LOCATION, "."),
															   Path.Combine(
															   Directory.GetParent(systemFolder).FullName, "fonts")});
							}
							else
							{
								Utilities.addPredefinedSearchPaths(new String[] { props.getGeneralProperty(Const.FONTS_LOCATION, ".") });
							}
						}
						catch (SecurityException ex)
						{
							GXLogging.Warn(log, "loadProps error", ex);
							Utilities.addPredefinedSearchPaths(new String[] { props.getGeneralProperty(Const.FONTS_LOCATION, ".") });
						}
					}
				}
			}
		}

		public static void addPredefinedSearchPaths(String[] predefinedPaths)
		{
			String predefinedPath = "";
			for (int i = 0; i < predefinedPaths.Length; i++)
				predefinedPath += predefinedPaths[i] + ";";
			predefinedSearchPath = predefinedPath + predefinedSearchPath; 
		}

		public static String getPredefinedSearchPaths()
		{
			return predefinedSearchPath;
		}

		protected virtual void init(ref int gxYPage, ref int gxXPage, int pageWidth, int pageLength)
		{
			
		}
		private float[] parsePattern(String patternStr)
		{
			if (patternStr!=null)
			{
				patternStr = patternStr.Trim();
				String[] values = patternStr.Split(new char[]{';'});
				if (values.Length>0)
				{
					float[] pattern = new float[values.Length];
					for (int i=0; i<values.Length; i++)
					{
						pattern[i] = float.Parse(values[i], CultureInfo.InvariantCulture.NumberFormat);
					}
					return pattern;
				}
			}
			return null;
		}

		

		public void GxRVSetLanguage(String lang)
		{
			language = lang;
		}

		public void GxSetTextMode(int nHandle, int nGridX, int nGridY, int nPageLength)
		{
		}

		protected float[] getDashedPattern(int style)
		{
			switch (style)
			{
				case 0: return STYLE_SOLID;
				case 1: return STYLE_NONE;
				case 2: return STYLE_DOTTED;
				case 3: return STYLE_DASHED;
				case 4: return STYLE_LONG_DASHED;
				case 5: return STYLE_LONG_DOT_DASHED;
				default:
					return STYLE_SOLID;
			}
		}

		public void GxDrawRect(int left, int top, int right, int bottom, int pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue)
		{
			GxDrawRect(left, top, right, bottom, pen, foreRed, foreGreen, foreBlue, backMode, backRed, backGreen, backBlue, 0, 0);
		}
		public void GxDrawRect(int left, int top, int right, int bottom, int pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue, int style, int cornerRadius)
		{
			GxDrawRect(left, top, right, bottom, pen, foreRed, foreGreen, foreBlue, backMode, backRed, backGreen, backBlue, style, style, style, style, cornerRadius, cornerRadius, cornerRadius, cornerRadius);
		}
		public virtual void GxDrawRect(int left, int top, int right, int bottom, int pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue,
			int styleTop, int styleBottom, int styleRight, int styleLeft, int cornerRadioTL, int cornerRadioTR, int cornerRadioBL, int cornerRadioBR)
		{
		}
		public void GxDrawLine(int left, int top, int right, int bottom, int width, int foreRed, int foreGreen, int foreBlue)
		{
			GxDrawLine(left, top, right, bottom, width, foreRed, foreGreen, foreBlue, 0);
		}
		public virtual void GxDrawLine(int left, int top, int right, int bottom, int width, int foreRed, int foreGreen, int foreBlue, int style)
		{
			
		}
		public void GxDrawBitMap(String bitmap, int left, int top, int right, int bottom)
		{
			GxDrawBitMap(bitmap, left, top, right, bottom, 0);
		}
		public virtual void GxDrawBitMap(String bitmap, int left, int top, int right, int bottom, int aspectRatio)
		{

		}

		public String getSubstitute(String fontName)
		{
			ArrayList fontSubstitutesProcessed = new ArrayList();
			String newFontName = fontName;
			while (fontSubstitutes.ContainsKey(newFontName))
			{
				if (!fontSubstitutesProcessed.Contains(newFontName))
				{
					fontSubstitutesProcessed.Add(newFontName);
					newFontName = (String)fontSubstitutes[newFontName];
				}
				else
				{
					return (String)fontSubstitutes[newFontName];
				}
			}
			return newFontName;
		}


		public virtual void GxAttris(String fontName, int fontSize, bool fontBold, bool fontItalic, bool fontUnderline, bool fontStrikethru, int Pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue)
		{
			
		}
		protected string GetFontLocation(string fontName)
		{
			string fontPath = props.getProperty(Const.MS_FONT_LOCATION, fontName, "");
			if (string.IsNullOrEmpty(fontPath))
			{
				fontPath = props.getProperty(Const.SUN_FONT_LOCATION, fontName, "");
			}
			return fontPath;
		}
		protected Hashtable GetFontLocations()
		{
			Hashtable msLocations = props.getSection(Const.MS_FONT_LOCATION);
			Hashtable sunLocations = props.getSection(Const.SUN_FONT_LOCATION);
			Hashtable locations = new Hashtable();
			if (msLocations != null)
			{
				foreach (object key in msLocations.Keys)
				{
					locations[key]=msLocations[key];
				}
			}
			if (sunLocations != null)
			{
				foreach (object key in sunLocations.Keys)
				{
					locations[key] = sunLocations[key];
				}
			}
			return locations;
		}

		protected bool IsEmbeddedFont(string fontName)
		{
			if (IsPdfA())
				return true;
			else
			{
				bool generalEmbeedFont = props.getBooleanGeneralProperty(Const.EMBEED_SECTION, false);
				bool generalEmbeedNotSpecified = props.getBooleanGeneralProperty(Const.EMBEED_NOT_SPECIFIED_SECTION, false);
				return generalEmbeedFont && props.getBooleanProperty(Const.EMBEED_SECTION, fontName, generalEmbeedNotSpecified);
			}
		}
		
		public virtual void setAsianFont(String fontName, String style)
		{
			
		}
		[Obsolete("GxDrawText with 6 arguments is deprecated", false)]
		public void GxDrawText(String sTxt, int left, int top, int right, int bottom, int align)
		{
			GxDrawText(sTxt, left, top, right, bottom, align, 0);
		}
		public void GxDrawText(String sTxt, int left, int top, int right, int bottom, int align, int htmlformat)
		{
			GxDrawText(sTxt, left, top, right, bottom, align, htmlformat, 0);
		}
		public void GxDrawText(String sTxt, int left, int top, int right, int bottom, int align, int htmlformat, int border)
		{
			GxDrawText(sTxt, left, top, right, bottom, align, htmlformat, border, 0);
		}
#pragma warning disable CS0612 // Type or member is obsolete
		public virtual void GxDrawText(String sTxt, int left, int top, int right, int bottom, int align, int htmlformat, int border, int valign)
		{
		}

		public void GxClearAttris()
		{
		}

		public static double PAGE_SCALE_Y = 20; 
		public static double PAGE_SCALE_X = 20; 
        public static double GX_PAGE_SCALE_Y_OLD = 15.45;
		public static double GX_PAGE_SCALE_Y = 14.4;
		
		private static double TO_CM_SCALE =28.6;  
		public bool GxPrintInit(String output, ref int gxXPage, ref int gxYPage, String iniFile, String form, String printer, int mode, int orientation, int pageSize, int pageLength, int pageWidth, int scale, int copies, int defSrc, int quality, int color, int duplex)
		{
			GXLogging.Debug(log,"GxPrintInit start");
			PPP = gxYPage;
			loadPrinterSettingsProps(iniFile, form, printer, mode, orientation, pageSize, pageLength, pageWidth, scale, copies, defSrc, quality, color, duplex); 

			if(outputStream != null)
			{
				if (output.ToUpper().Equals("PRN"))
					outputType = Const.OUTPUT_STREAM_PRINTER;
				else
					outputType = Const.OUTPUT_STREAM;
				GXLogging.Debug(log,"GxPrintInit outputStream != null");
			}
			else
			{
				if(output.ToUpper().Equals("SCR"))
					outputType = Const.OUTPUT_SCREEN;
				else if(output.ToUpper().Equals("PRN"))
					outputType = Const.OUTPUT_PRINTER;
				else outputType = Const.OUTPUT_FILE;

				if(outputType == Const.OUTPUT_FILE)
					TemporaryFiles.getInstance().removeFileFromList(docName);
				else
				{
					String tempPrefix = docName;
					String tempExtension = "pdf";
					int tempIndex = docName.LastIndexOf('.');
					if(tempIndex != -1)
					{
						tempPrefix = docName.Substring(0, tempIndex);
						tempExtension = ((docName + " ").Substring(tempIndex + 1)).Trim();
					}
					docName = TemporaryFiles.getInstance().getTemporaryFile(tempPrefix, tempExtension);
				}
				try
				{
					setOutputStream(File.OpenWrite(docName));
					GXLogging.Debug(log,"GxPrintInit outputStream: FileOutputStream("  + docName + ")");
				}
				catch(IOException accessError)
				{ 
					
					outputStream = new MemoryStream();
					outputType = Const.OUTPUT_FILE; 
					GXLogging.Error(log,"GxPrintInit outputStream: IOException ", accessError);
				}
			}
			printerOutputMode = mode;

			leftMargin = (float) (TO_CM_SCALE * Convert.ToDouble(props.getGeneralProperty(Const.LEFT_MARGIN), CultureInfo.InvariantCulture.NumberFormat));
			topMargin = (float) (TO_CM_SCALE * Convert.ToDouble(props.getGeneralProperty(Const.TOP_MARGIN), CultureInfo.InvariantCulture.NumberFormat));
			bottomMargin = (float) (Convert.ToDouble(props.getGeneralProperty(Const.BOTTOM_MARGIN), CultureInfo.InvariantCulture.NumberFormat));

			lineCapProjectingSquare = props.getGeneralProperty(Const.LINE_CAP_PROJECTING_SQUARE).Equals("true");
			barcode128AsImage = props.getGeneralProperty(Const.BARCODE128_AS_IMAGE).Equals("true");

			STYLE_DOTTED = parsePattern(props.getGeneralProperty(Const.STYLE_DOTTED));
			STYLE_DASHED = parsePattern(props.getGeneralProperty(Const.STYLE_DASHED));
			STYLE_LONG_DASHED = parsePattern(props.getGeneralProperty(Const.STYLE_LONG_DASHED));
			STYLE_LONG_DOT_DASHED = parsePattern(props.getGeneralProperty(Const.STYLE_LONG_DOT_DASHED));

			int result;
			if (Int32.TryParse(props.getGeneralProperty(Const.RUN_DIRECTION), out result))
				runDirection = result;
	
			init(ref gxYPage, ref gxXPage, pageWidth, pageLength);
			return true;
		}

		public int getPageLines()
		{
			GXLogging.Debug(log,"getPageLines: --> " + pageLines);
			return pageLines;
		}
		public int getLineHeight()
		{
			GXLogging.Debug(log,"getLineHeight: --> " + lineHeight);
			return lineHeight;
		}
		public void setPageLines(int P_lines)
		{
			GXLogging.Debug(log,"setPageLines: " + P_lines);
			pageLines = P_lines;
		}
		public void setLineHeight(int lineHeight)
		{
			this.lineHeight = lineHeight;
			GXLogging.Debug(log,"setLineHeight: " + lineHeight);
		}

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
		public void GxEndPage()
		{
			GXLogging.Debug(log,"GxEndPage pages:" + pages);
		
		}

		public virtual void GxEndDocument()
		{
			

		}
		protected Stream ReadResource(string fileName)
		{
			Assembly assembly = GetType().Assembly;
			string resourcePath = $"{assembly.GetName().Name}.{fileName}";
			return assembly.GetManifestResourceStream(resourcePath);
		}

		protected bool IsPdfA()
		{
			return complianceLevel != 0;
		}

		public void GxEndPrinter()
		{
		}
		public virtual void GxStartPage()
		{
		}
	
		public void GxStartDoc()
		{
		}
		public void GxSetDocFormat(String format)
		{
		}

		public void GxSetDocName(String docName)
		{
			this.docName = docName.Trim();
			if(!Path.IsPathRooted(docName))
			{ 
				string outputDir = props.getGeneralProperty(Const.OUTPUT_FILE_DIRECTORY, "").Replace(alternateSeparator, Path.DirectorySeparatorChar).Trim();
				if(!string.IsNullOrEmpty(outputDir) && outputDir!=".")
				{
					try
					{
						if (!outputDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
						{
							outputDir += Path.DirectorySeparatorChar;
						}
						string[] dirs = Directory.GetDirectories(outputDir);
						foreach (string dir in dirs)
						{
							Directory.CreateDirectory(dir);
						}
					}catch (Exception ex)
					{
						Exception exDetailed = new Exception($"Error creating {Const.OUTPUT_FILE_DIRECTORY} of {Const.INI_FILE} ({outputDir})", ex);
						GXLogging.Error(log, "GxSetDocName error", exDetailed);
						throw exDetailed;
					}
					this.docName = outputDir + this.docName;
				}
			}
			if(this.docName.IndexOf('.') < 0)
				this.docName += ".pdf";
			GXLogging.Debug(log,"GxSetDocName: '" + this.docName + "'");
		}

		public bool GxPrTextInit(String ouput, ref int nxPage, ref int nyPage, String psIniFile, String psForm, String sPrinter, int nMode, int nPaperLength, int nPaperWidth, int nGridX, int nGridY, int nPageLines)
		{
			return true;
		}

		public bool GxPrnCfg( String ini )
		{
			return true;
		}

		public void GxShutdown()
		{
		}

		public bool GxIsAlive()
		{
			return false;
		}

		public bool GxIsAliveDoc()
		{
			return false;
		}

		private int page;
		public int getPage()
		{
			return page;
		}

		public void setPage(int page)
		{
			this.page = page;
			GXLogging.Debug(log,"setPage: old:" + this.page + ",new:"  + page );
		}

		public bool getModal()
		{
			return modal;
		}
		public void setModal(bool modal)
		{
			this.modal = modal;
		}

		public void setMetrics(String fontName, bool bold, bool italic, int ascent, int descent, int height, int maxAdvance, int[] sizes)
		{
		}

		private void loadSubstituteTable()
		{
			Hashtable tempInverseMappings = new Hashtable();
			if (GXUtil.IsWindowsPlatform)
			{
				// Registry substitutes
				ArrayList table = new ArrayList();
				String registryEntry = Const.REGISTRY_FONT_SUBSTITUTES_ENTRY;
				try
				{ // Win95/98/Me
					table = nativeCode.getRegistrySubValues(Registry.LocalMachine.Name, Const.REGISTRY_FONT_SUBSTITUTES_ENTRY);
				}
				catch (Exception)
				{
					registryEntry = Const.REGISTRY_FONT_SUBSTITUTES_ENTRY_NT;
					try
					{ // Win NT/2000
						table = nativeCode.getRegistrySubValues(Registry.LocalMachine.Name, Const.REGISTRY_FONT_SUBSTITUTES_ENTRY_NT);
					}
					catch (Exception) {; }
				}
				for (IEnumerator enu = table.GetEnumerator(); enu.MoveNext();)
				{
					String entryName = (String)enu.Current;
					String fontName = entryName;
					int index = fontName.IndexOf(',');
					if (index != -1)
						fontName = fontName.Substring(0, index);
					try
					{
						String substitute = nativeCode.getRegistryValue(Registry.LocalMachine.Name, registryEntry, entryName);
						int index2 = substitute.IndexOf(',');
						if (index2 != -1)
							substitute = substitute.Substring(0, index2);
						fontSubstitutes[fontName] = substitute;
						ArrayList tempVector = (ArrayList)tempInverseMappings[substitute];
						if (tempVector == null)
						{
							tempVector = new ArrayList();
							tempInverseMappings[substitute] = tempVector;
						}
						tempVector.Add(fontName);
					}
					catch (Exception) {; }
				}
			}
			for(int i = 0; i < Const.FONT_SUBSTITUTES_TTF_TYPE1.Length; i++)
				fontSubstitutes[Const.FONT_SUBSTITUTES_TTF_TYPE1[i][0]]= Const.FONT_SUBSTITUTES_TTF_TYPE1[i][1];
        
			// substitutes from PDFReport.INI
			Hashtable otherMappings = props.getSection(Const.FONT_SUBSTITUTES_SECTION);
			if(otherMappings != null)
				for(IEnumerator enu = otherMappings.Keys.GetEnumerator(); enu.MoveNext();)
				{
					String fontName = (String)enu.Current;
					fontSubstitutes[fontName]= otherMappings[fontName];
					if(tempInverseMappings.ContainsKey(fontName)) // solves fonts recursion p.e Font1 -> Font2 and Font2->Font3. Set Font1->Font3.
					{                                             
						String fontSubstitute = (String)otherMappings[fontName];
						for(IEnumerator enum2 = ((ArrayList)tempInverseMappings[fontName]).GetEnumerator(); enum2.MoveNext();)
							fontSubstitutes[enum2.Current]= fontSubstitute; 
					}
				}		
		}

		public void GxPrintMax() { ; }
		public void GxPrintNormal() { ; }
		public void GxPrintOnTop() { ; }
		public void GxPrnCmd(string cmd)  { ; }
		public void GxPrnCmd()  { ; }

		public void showInformation()
		{
		}
	
		public static double SCALE_FACTOR = 72;
		private double PPP = 96;
		protected double convertScale(int value)
		{
			double result = value * SCALE_FACTOR / PPP;
			return result;
		}	
		protected double convertScale(double value)
		{
			double result = value * SCALE_FACTOR / PPP;
			return result;
		}
		protected float reconvertScale(float value)
		{
			float result = value / (float)(SCALE_FACTOR / PPP);
			return result;
		}

		internal abstract bool SetComplainceLevel(PdfConformanceLevel level);
	}

	public class ParseINI
	{
		static IGXLogger log = GXLoggerFactory.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName);

		private static int MAX_LINE_LENGTH=255; 
		private static String GENERAL="&General&"; 

		private String entryName;
        private StreamReader inputStream;
		private Hashtable sections=new Hashtable();
		private Hashtable sectionEntries;
		private Hashtable general=new Hashtable();
		private Hashtable aliased=new Hashtable(); // (original -> alias)
		private Hashtable alias=new Hashtable();   // (alias -> original)
		private bool _need2Save = false, autoSave = true;
		private String filename = null;

		char []tempBytes=new char[16384];

		public ParseINI()
		{
			filename = null;
		}

		public ParseINI(String filename, String template)
		{
			this.filename = Path.GetFullPath(filename);
			try
			{
				string inputFileNameOrTemplate = filename;
				if (!String.IsNullOrEmpty(template) && !File.Exists(filename) && File.Exists(template))
				{
					inputFileNameOrTemplate = Path.GetFullPath(template);
				}
				if (File.Exists(inputFileNameOrTemplate))
				{
					using (StreamReader sr = new StreamReader(inputFileNameOrTemplate, Encoding.UTF8))
					{
						load(sr);
					}
				}
			}
			catch (FileNotFoundException)
			{ 
				File.Create(filename).Close();
			}
		}
		public ParseINI(String filename) 
		{
			this.filename = Path.GetFullPath(filename);
			try
			{
				if (File.Exists(filename))
				{
					using (StreamReader sr = new StreamReader(filename, Encoding.UTF8))
					{
						load(sr);
					}
				}
			}
			catch(FileNotFoundException)
			{ 
                File.Create(filename).Close();
			}
		}

		public ParseINI(StreamReader inputStream) 
		{
			load(inputStream);
		}

		public bool need2Save()
		{
			return _need2Save;
		}

		public void finalize()
		{
			if(autoSave && _need2Save)
				try
				{
					save();
				}
				catch(IOException e)
				{
					Console.WriteLine("ParseINI (" + filename + "):" +e);
				}
		}

		public IEnumerator sectionElements()
		{
			return sections.GetEnumerator();
		}

		public IEnumerator sectionNames()
		{
			return sections.Keys.GetEnumerator();
		}

		public Hashtable getSection(String section)
		{
			if(sections.ContainsKey(section))
				return (Hashtable) ((Hashtable)sections[section]).Clone();
			else return null;

		}

		public bool getBooleanProperty(String section, String property, bool byDefault)
		{
			if(byDefault)
				return !getProperty(section, property, "true").ToLower().Equals("false");
			else
				return getProperty(section, property, "false").ToLower().Equals("true");
		}

		public bool getBooleanGeneralProperty(String property, bool byDefault)
		{
			if(byDefault)
				return !getGeneralProperty(property, "true").ToLower().Equals("false");
			else
				return getGeneralProperty(property, "false").ToLower().Equals("true");
		}

		public bool sectionExists(String section)
		{
			return sections.ContainsKey(section);
		}

		public String getGeneralProperty(String prop)
		{
			return getProperty(general,prop,null);
		}

		public String getGeneralProperty(String prop, String defecto)
		{
			return getProperty(general,prop,defecto);
		}

		public void setGeneralProperty(String prop, String value)
		{
			string oldvalue = (string)general[prop];
			general[prop]= value;
			if (!(isEmptyValueProperty(value) && oldvalue == null))//Empty values ​​are not considered in the save
			{ 
				_need2Save |= !value.Equals(oldvalue);
			}
		}

		public bool setupGeneralProperty(String prop, String value)
		{
			if(general.ContainsKey(prop))
				return false;
			else setGeneralProperty(prop, value);
			return true;
		}

		public bool setupProperty(String section, String prop, String value)
		{
			if(getProperty(section, prop) != null)
				return false;
			else setProperty(section, prop, value);
			return true;
		}

		private bool isEmptyValueProperty(string value)
		{
			return value != null && value.Length==0;
		}

		public void setProperty(String section, String prop, String value)
		{
			if(!sections.ContainsKey(section))
				sections[section]= new Hashtable();

			string oldvalue = (string)((Hashtable)sections[section])[prop];

			((Hashtable)sections[section])[prop] = value;

			if (!(isEmptyValueProperty(value) && oldvalue == null))//Empty values ​​are not considered in the save
				_need2Save = _need2Save |= !value.Equals(oldvalue);
		}

		public String getProperty(String section, String prop)
		{
			return getProperty((Hashtable)sections[section], prop, null);
		}

		public String getProperty(String section, String prop, String defecto)
		{
			return getProperty((Hashtable)sections[section], prop, defecto);
		}

		private String getProperty(Hashtable section, String prop, String defecto)
		{
			if(section != null && section.ContainsKey(prop))
				return (String)section[prop];
			if(section != null && alias.ContainsKey(prop)) 
				return getProperty(section, (String)alias[prop], defecto);
			return defecto;
		}

		public bool isAliased(String original)
		{
			return aliased.ContainsKey(original);
		}

        public void load(StreamReader iStream) 
		{
			inputStream=iStream;

			sectionEntries=new Hashtable();
			sections[GENERAL]= sectionEntries;
			try
			{
				while ((entryName = readEntryName()) != null)
				{
					String entry=readEntry();
					sectionEntries[entryName]=entry;
					GXLogging.Debug(log,entryName + "=" + entry);
				}	
			}
			catch(Exception){}
			general = (Hashtable) sections[GENERAL];
			sections.Remove(GENERAL);
		}

		public void save() 
		{
			try
			{
				if (autoSave && _need2Save && filename != null)
				{
					using (FileStream fs = File.OpenWrite(filename))
					{
						save(fs);
					}
				}
			}
			catch (Exception ex)//p.e. UnauthorizedAccessException 
			{
				GXLogging.Warn(log,"Unable to save " + filename, ex);
				autoSave = false;
			}
		}

		public void setAutoSave(bool autoSave)
		{
			this.autoSave = autoSave;
		}

		public void save(Stream oStream)
		{
			using (StreamWriter outputStream = new StreamWriter(oStream, Encoding.UTF8))
			{
				IEnumerator props, secs;
				String prop, value;
				int it;
				if (general != null)
				{
					props = general.Keys.GetEnumerator();
					while (props.MoveNext())
					{
						prop = (String)props.Current;
						value = (String)getGeneralProperty(prop);
						if (isEmptyValueProperty(value))
							continue;
						if ((value.Length + prop.Length + 4) > MAX_LINE_LENGTH) it = (MAX_LINE_LENGTH - prop.Length - 4);
						else it = value.Length;
						outputStream.Write(prop + "= " + value.Substring(0, it) + "\r\n");
						for (; (value.Length - it) > (MAX_LINE_LENGTH - 4); it += MAX_LINE_LENGTH)
							outputStream.Write(" " + value.Substring(it, MAX_LINE_LENGTH) + "\r\n");
						if (it < value.Length) outputStream.Write(" " + value.Substring(it) + "\r\n");
					}
				}
				secs = sectionNames();
				while (secs.MoveNext())
				{
					String section = (String)secs.Current;
					outputStream.Write("\r\n[" + section + "]\r\n");
					props = ((Hashtable)sections[section]).Keys.GetEnumerator();
					while (props.MoveNext())
					{
						prop = (String)props.Current;
						value = (String)getProperty(section, prop);
						if (isEmptyValueProperty(value))
							continue;
						if ((value.Length + prop.Length + 4) > MAX_LINE_LENGTH) it = (MAX_LINE_LENGTH - prop.Length - 4);
						else it = value.Length;
						outputStream.Write(prop + "= " + value.Substring(0, it) + "\r\n");
						for (; (value.Length - it) > (MAX_LINE_LENGTH - 4); it += MAX_LINE_LENGTH - 4)
							outputStream.Write(" " + value.Substring(it, MAX_LINE_LENGTH - 4) + "\r\n");
						if (it < value.Length) outputStream.Write(" " + value.Substring(it) + "\r\n");
					}
				}
			}
			_need2Save = false;
		}

		private String readEntryName()
		{
			int offset=0;
			int car;

			while(true)
			{
				car= inputStream.Read();
				if (car==-1)
				{
					return null;
				}
				switch((char)(car))
				{
					case '%':  // Comments
					case '*':
						while(true)
						{
							car=inputStream.Read();
							if(car=='\n' || car=='\r' || car==-1)break;
						}
						break;
					case ' ':  // Continuation of the previous entry -> is concatenated with the entry that was already being processed
						if(entryName==null)throw new IOException("Invalid entry");
						sectionEntries[entryName]=(String) sectionEntries[entryName] + readEntry();
						return readEntryName();
					case '[':  // New section begins
						sectionEntries = new Hashtable();
						String sectionName = readEntry();
						sectionName=sectionName.Substring(0,sectionName.Length-1);
						GXLogging.Debug(log,"sectionName=" + sectionName);
						sections[sectionName]=sectionEntries;
						entryName=null;
						return readEntryName();
					case '\n':
					case '\r': break;
					default:   
						tempBytes[offset++]=(char)car;
						while(true)
						{
							car=inputStream.Read();
							if(
								car==':' || car=='=' || car==-1)break;
							else tempBytes[offset++]=(char)car;
						}
						if(offset==4 &&         // Check if it is new Section
							tempBytes[0]=='N' &&
							tempBytes[1]=='a' &&
							tempBytes[2]=='m' &&
							tempBytes[3]=='e')
						{
							sectionEntries=new Hashtable();
							String section=readEntry();
							if(section.StartsWith("./"))section=section.Substring(2);
							sections[section]=sectionEntries;
							entryName=null;
							return readEntryName();
						}
						else return new String(tempBytes,0,offset);
				}
			}
		}

		private String readEntry()
		{		
			int offset=0;
			int car=inputStream.Read();
			while(Char.IsWhiteSpace((char)car))
			{
				if (car=='\0' || car=='\n' || car=='\r')
				{
					return "";
				}
				car=inputStream.Read();
			}
			tempBytes[offset++]=(char)car;
			try
			{
				while((car=inputStream.Read())!='\0' && car!='\n' && car!='\r' && car!=-1)
				{
					tempBytes[offset++]=(char)car;
				}
			}
			catch(Exception){}
			return new String(tempBytes,0,offset);
		}

		public static ArrayList parseLine(String line, String separator)
		{
			ArrayList partes = new ArrayList();
			if(line == null) return partes;
			StringTokenizer tokens = new StringTokenizer(line, separator, false);
			if(!tokens.HasMoreTokens())
				return partes;
			String thisToken;
			String lastToken = tokens.NextToken();
			while(tokens.HasMoreTokens())
			{
				thisToken = tokens.NextToken();
				if(lastToken.StartsWith("\"") &&
					(!lastToken.EndsWith("\"") || lastToken.Length == 1))
				{
					lastToken = lastToken + separator + thisToken;
				}
				else
				{
					partes.Add(lastToken);
					lastToken = thisToken;
				}
			}
			if(lastToken.StartsWith("\"") && !lastToken.EndsWith("\"")) // it starts with " and does not end with "
				lastToken = lastToken + "\"";
			partes.Add(lastToken);
			return partes;
		}
	}
	public class Const
	{
    
		public static String ACROBAT_LOCATION = "Acrobat Location"; 
		public static String DEFAULT_ACROBAT_LOCATION = "Applications\\Acrobat.exe\\shell\\open\\command"; 
		public static String DEFAULT_ACROREAD_LOCATION = "Applications\\AcroRd32.exe\\shell\\open\\command"; 
		public static String DEFAULT_ACROREAD_LOCATION2 = "Applications\\AcroRd32.exe\\shell\\Read\\command"; 
		public static String DEFAULT_ACROBAT_EX_LOCATION = "SOFTWARE\\Classes\\AcroExch.Document\\shell\\open\\command"; 
		public static String DEFAULT_ACROBAT_EX_LOCATION2 = "AcroExch.Document\\shell\\open\\command"; 
    	
		public static String INI_FILE = "PDFReport.ini";
		public static String INI_TEMPLATE_FILE = "PDFReport.template";
		public static String WEB_INF = "WEB-INF";
		public static String EMBEED_DEFAULT = "false"; 
		public static String SEARCH_FONTS_ALWAYS = "SearchNewFonts";   
		public static String SEARCH_FONTS_ONCE = "SearchNewFontsOnce"; 
		public static String LEFT_MARGIN = "LeftMargin"; 
		public static String TOP_MARGIN = "TopMargin";
		public static String BOTTOM_MARGIN = "BottomMargin";
		public static String DEFAULT_LEFT_MARGIN = "0.75"; 
		public static String DEFAULT_TOP_MARGIN = "0.75";  
		public static bool DEFAULT_MARGINS_INSIDE_BORDER = false; //By default the margins do not add to the pageSize specified in the report.

		public static String DEFAULT_BOTTOM_MARGIN = "6";
		public static String DEFAULT_LINE_CAP_PROJECTING_SQUARE = "true";
		public static String DEFAULT_BARCODE128_AS_IMAGE = "true";
		public static String PDF_REPORT_INI_VERSION_ENTRY = "Version";
		public static String PDF_REPORT_INI_VERSION = "1.0.0.0";
		public static String MARGINS_INSIDE_BORDER = "MarginsInsideBorder"; // Indicates whether the value of TopMargin and LeftMargin are added to PageSize
		public static String FONTS_LOCATION = "FontsLocation"; // location of the fonts (; separated)
		public static String OUTPUT_FILE_DIRECTORY = "OutputFileDirectory"; // directory for the PDFs when the docName is relative
		public static String SERVER_PRINTING = "ServerPrinting"; 
		public static String ADJUST_TO_PAPER = "AdjustToPaper"; //fit to page
		public static String LINE_CAP_PROJECTING_SQUARE = "LineCapProjectingSquare";
		public static String BARCODE128_AS_IMAGE = "Barcode128AsImage";
        public static String LEADING = "Leading";
		internal static String COMPLIANCE_LEVEL = "ComplianceLevel";

		//Printer settings
		public static String PRINTER = "Printer"; 
		public static String MODE = "Mode"; 
		public static String ORIENTATION = "Orientation"; 
		public static String PAPERSIZE = "PaperSize"; 
		public static String PAPERLENGTH = "PaperLength"; 
		public static String PAPERWIDTH = "PaperWidth"; 
		public static String SCALE = "Scale"; 
		public static String COPIES = "Copies"; 
		public static String DEFAULTSOURCE = "DefaultSource"; 
		public static String PRINTQUALITY = "PrintQuality"; 
		public static String COLOR = "Color"; 
		public static String DUPLEX = "Duplex"; 

		// Secciones
		public static String EMBEED_SECTION = "Embeed Fonts";
		public static String EMBEED_NOT_SPECIFIED_SECTION= "EmbeedNotSpecifiedFonts";
		public static String MS_FONT_LOCATION = "Fonts Location (MS)"; 
		public static String SUN_FONT_LOCATION = "Fonts Location (Sun)"; 
		public static String FONT_SUBSTITUTES_SECTION = "Fonts Substitutions"; 
		public static String FONT_METRICS_SECTION = "Font Metrics";
    
		public static String DEBUG_SECTION = "Debug";
		public static String DRAW_IMAGE = "DrawImage"; 
		public static String DRAW_LINE = "DrawLine"; 
		public static String DRAW_TEXT = "DrawText"; 
		public static String DRAW_BOX = "DrawBox"; 

		public static String STYLE_DOTTED = "DottedStyle"; 
		public static String STYLE_DASHED = "DashedStyle"; 
		public static String STYLE_LONG_DASHED = "LongDashedStyle"; 
		public static String STYLE_LONG_DOT_DASHED = "LongDotDashedStyle"; 

		public static String DEFAULT_STYLE_DOTTED = "1;2";
		public static String DEFAULT_STYLE_DASHED = "4;2";
		public static String DEFAULT_STYLE_LONG_DASHED = "6;2";
		public static String DEFAULT_STYLE_LONG_DOT_DASHED = "6;2;1;2";

		public static String RUN_DIRECTION = "RunDirection";
		public static String RUN_DIRECTION_LTR = "2";

		public static String JUSTIFIED_TYPE_ALL = "JustifiedTypeAll";

		public const int OUTPUT_SCREEN = 0; 
		public const int OUTPUT_PRINTER = 1; 
		public const int OUTPUT_FILE = 2; 
		public const int OUTPUT_STREAM = 3; 
		public const int OUTPUT_STREAM_PRINTER = 4; // OUTPUT_STREAM and Only to printer

		public static float OPTIMAL_MINIMU_BAR_WIDTH_SMALL_FONT = 0.6f;
		public static float OPTIMAL_MINIMU_BAR_WIDTH_LARGE_FONT = 0.68f;
		public static int LARGE_FONT_SIZE = 10;

		public static string[][] FONT_SUBSTITUTES_TTF_TYPE1 = {new string[]  { "Arial", "Helvetica" },
			new string[]{ "Courier New", "Courier" },
			new string[]{"Fixedsys", "Helvetica"},
			new string[]{"Modern", "Helvetica"},
			new string[]{"MS Sans Serif", "Helvetica"},
			new string[]{"MS Serif", "Helvetica"},
			new string[]{"Roman", "Helvetica"},
			new string[]{"Script", "Helvetica"},
			new string[]{"System", "Helvetica"},
			new string[]{"Times New Roman", "Times"},
			new string[]{"\uff2d\uff33 \u660e\u671d", "Japanese"},  //(MS Mincho)
			new string[]{"\uff2d\uff33 \u30b4\u30b7\u30c3\u30af", "Japanese2"}};//(MS Gothic)


		public static String REGISTRY_FONT_SUBSTITUTES_ENTRY = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\FontSubstitutes"; 
		public static String REGISTRY_FONT_SUBSTITUTES_ENTRY_NT = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\FontSubstitutes"; 

        public static String FIX_SAC24437 = "FixSac24437";
        public static String BACK_FILL_IN_CONTROLS = "BackFillInControls";	
    
	}

	public class NativeSharpFunctionsMS 
	{
		static IGXLogger log = GXLoggerFactory.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName);
		public int shellExecute(String cmd, String fileName)
		{
			Process p = new Process();
			ProcessStartInfo si  = new ProcessStartInfo(cmd, fileName);
			si.CreateNoWindow=true;
			si.UseShellExecute=true;
			p.StartInfo=si;
			return p.Start() ? 0 : 1;
		}

		[DllImport("kernel32", EntryPoint="GetProfileString", CharSet=CharSet.Auto)] 
		private static extern int GetProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize);

		public PrinterInfo getDefaultPrinter() 
		{
			StringBuilder buffer = new StringBuilder(255);
			GetProfileString("windows", "device", ",,,", buffer, 255);
			String result = buffer.ToString();
			if (!string.IsNullOrEmpty(result) && result.IndexOf(',') >= 0)
			{
				if (result.Equals(",,,")) throw new IOException("DefaultPrinter not defined");

				return new PrinterInfo(result.Substring(0, result.IndexOf(',')),
					result.Substring(result.IndexOf(',') + 1, result.LastIndexOf(',') - result.IndexOf(',') - 1),
					result.Substring(result.LastIndexOf(',') + 1));
			}
			else
			{
				throw new IOException("DefaultPrinter not found");
			}
		}
    
		public String getRegistryValue(string key, String entry, String subValue) 
		{
			try
			{
				return ReadRegKey(key + "\\" + entry + "\\" + subValue);
			}
			catch (SecurityException ex)
			{
				GXLogging.Error(log,"getRegistryValue error" , ex);
				return string.Empty;
			}
		}

		public ArrayList getRegistrySubValues(string key, String entry) 
		{
			RegistryKey regkey = findRegKey(key + "\\" + entry);
			if (regkey != null && GXUtil.IsWindowsPlatform)
			{
				string[] values = regkey.GetValueNames();
				return new ArrayList( values);
			}else return new ArrayList();
		}
    
		public bool executeModal(String command, string fileName, bool newConsole)
		{
			Process p = new Process();
			ProcessStartInfo si  = new ProcessStartInfo(command, fileName);
			si.CreateNoWindow=true;
			si.UseShellExecute=true;
			p.StartInfo=si;
			bool result = p.Start();
			p.WaitForExit();
			return result;
		}        

		public static string ReadRegKey(string path)
		{
			string[] splitPath = path.Split('\\');
			int pathItems = splitPath.Length;
			if (pathItems < 2)
				return "";
			string partialPath = splitPath[0];
			for(int i=1; i < pathItems-1; i++)
				partialPath += "\\"+splitPath[i];

			RegistryKey rKey = findRegKey(partialPath);
			if ( rKey != null && GXUtil.IsWindowsPlatform)
			{
				object oRet = rKey.GetValue(splitPath[pathItems-1]);
				if (oRet != null)
					return oRet.ToString();
			}
			return "";
		}
		static RegistryKey findRegKey(string path)
		{
			string[] splitPath = path.Split('\\');
			int pathItems = splitPath.Length;
			if (pathItems < 2 || !GXUtil.IsWindowsPlatform)
				return null;
			RegistryKey baseKey;
			switch(splitPath[0].ToUpper())
			{
				case "HKEY_CLASSES_ROOT":
					baseKey = Registry.ClassesRoot;
					break;
				case "HKEY_CURRENT_CONFIG":
					baseKey = Registry.CurrentConfig;
					break;
				case "HKEY_PERFORMANCE_DATA":
					baseKey = Registry.PerformanceData;
					break;
				case "HKEY_CURRENT_USER":
					baseKey = Registry.CurrentUser;
					break;
				case "HKEY_LOCAL_MACHINE":
					baseKey = Registry.LocalMachine;
					break;
				case "HKEY_USERS":
					baseKey = Registry.Users;
					break;
				default:
					return null;
			}
			RegistryKey subKey;
			int i = 2;
			for( subKey = baseKey.OpenSubKey(splitPath[1]);
				(i < pathItems && subKey != null);
				subKey = subKey.OpenSubKey(splitPath[i++]));
			return subKey;
		}
	
	}

	public class TemporaryFiles 
	{
		private static ArrayList files = new ArrayList();
		private static Random random = new Random();
		private static TemporaryFiles temporaryFiles = new TemporaryFiles();
    
		public static TemporaryFiles getInstance()
		{
			return temporaryFiles;
		}
    
		public void cleanup()
		{
			for(IEnumerator enu = files.GetEnumerator(); enu.MoveNext();)
				File.Delete((String)enu.Current);
			files.Clear();
		}
    
		public String getTemporaryFile(String extension)
		{
			String tempFile;
			extension = "." + extension;
			do tempFile = "" + ((int)(random.Next() * 1e8)) + extension;
			while(File.Exists(tempFile));
			addFile(tempFile);
			return tempFile;
		}
    
		public String getTemporaryFile(String filename, String extension)
		{
			extension = "." + extension;
			String tempFile = filename + extension; 
			double expCounter = 1;
			while(File.Exists(tempFile)) 
			{
				tempFile = filename + ((int)(random.Next() * expCounter)) + extension;
				expCounter *= 2;
				expCounter %= 1e8;
				File.Delete(tempFile); 
			}
			addFile(tempFile);
			return tempFile;
		}
    
		public void addFile(String file)
		{
			files.Add(file);
		}
    
		public bool removeFileFromList(String file)
		{
			if (files.Contains(file))
			{
				files.Remove(file);
				return true;
			}
			else
			{
				return false;
			}
		}

		public ArrayList getFiles()
		{
			return (ArrayList)files.Clone();
		}
    
	}
	public class PrinterInfo
	{
		private String deviceName;
		private String driverName;
		private String portName;

		public PrinterInfo(String deviceName, String driverName, String portName)
		{
			this.deviceName = deviceName;
			this.driverName = driverName;
			this.portName = portName;
		}

		public String getDeviceName()
		{
			return deviceName;
		}

		public String getDriverName()
		{
			return driverName;
		}

		public String getPortName()
		{
			return portName;
		}
	}

	public class PDFFont
	{
		public static string[][] base14 = {new string[]{"sansserif",    "/Helvetica",   "/Helvetica-Bold","/Helvetica-Oblique", "/Helvetica-BoldOblique"},
			new string[]{"monospaced",  "/Courier", "/Courier-Bold","/Courier-Oblique", "/Courier-BoldOblique"},
				new string[]{"timesroman",  "/Times-Roman", "/Times-Bold",  "/Times-Italic",    "/Times-BoldItalic"},
				new string[]{"courier",     "/Courier", "/Courier-Bold","/Courier-Oblique", "/Courier-BoldOblique"},
				new string[]{"helvetica",   "/Helvetica",   "/Helvetica-Bold","/Helvetica-Oblique", "/Helvetica-BoldOblique"},
				new string[]{"dialog",      "/Courier", "/Courier-Bold","/Courier-Oblique", "/Courier-BoldOblique"},
				new string[]{"dialoginput", "/Courier", "/Courier-Bold","/Courier-Oblique", "/Courier-BoldOblique"},
				new string[]{"symbol",      "/Symbol",     "/Symbol", "/Symbol",          "/Symbol"},
				new string[]{"times",       "/Times-Roman", "/Times-Bold", "/Times-Italic", "/Times-BoldItalic"},
				new string[]{"zapfdingbats", "/ZapfDingBats", "/ZapfDingBats", "/ZapfDingBats", "/ZapfDingBats"} };

		public static bool isType1(String fontName)
		{
			String f = fontName.ToLower();
			for(int i=0;i<base14.Length;i++)
			{
				if(base14[i][0].Equals(f))
				{
					return true;
				}
			}
			for(int i = 0; i < Type1FontMetrics.CJKNames.Length; i++)
			{
				if(Type1FontMetrics.CJKNames[i][0].ToLower().Equals(f) ||
					Type1FontMetrics.CJKNames[i][1].ToLower().Equals(f))
				{
					return true;
				}			
			}
			return false;
		}


	}
	public class Type1FontMetrics
	{
		public static string[][] CJKNames = {new string[] { "SimplifiedChinese", "AdobeSongStd-Light-Acro" },
			new string[] {"TraditionalChinese", "AdobeMingStd-Light-Acro"},
			new string[] {"Japanese", "KozMinPro-Regular-Acro"},
			new string[] {"Japanese2", "KozGoPro-Medium-Acro"},
			new string[] {"Korean", "AdobeMyungjoStd-Medium-Acro"},
			new string[] {"SimplifiedChineseAcro5", "STSongStd-Light-Acro"},
			new string[] {"TraditionalChineseAcro5", "MSungStd-Light-Acro"},
			new string[] {"KoreanAcro5", "HYSMyeongJoStd-Medium-Acro"}
		};
	}

	public class MSPDFFontDescriptor 
	{
		static IGXLogger log = GXLoggerFactory.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName);

		private static String TRUE_TYPE_REGISTRY_SIGNATURE = "(TrueType)"; 
		private static String REGISTRY_FONTS_ENTRY = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Fonts"; // Fonts NT/2000
		private static String REGISTRY_FONTS_ENTRY_NT = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Fonts"; // Fonts 95/98/Me

		private static ParseINI staticProps = new ParseINI();

		private static bool staticMappingsSearched=false;
		public String getTrueTypeFontLocation(String fontName)
		{        
			
			String fontFileLocation = staticProps.getProperty(Const.MS_FONT_LOCATION, fontName);
			if(fontFileLocation != null || staticMappingsSearched)
				return fontFileLocation;
			staticMappingsSearched = true;

			if (GXUtil.IsWindowsPlatform)
			{
				try
				{
					RegistryKey FONTS_ENTRY = isWinNT() ? Registry.LocalMachine.OpenSubKey(REGISTRY_FONTS_ENTRY_NT, false) : Registry.LocalMachine.OpenSubKey(REGISTRY_FONTS_ENTRY, false);
					String[] fontNames = FONTS_ENTRY.GetValueNames();
					for (int i = 0; i < fontNames.Length; i++)
					{
						String fontNameMs = fontNames[i];

						if (fontNameMs.EndsWith(TRUE_TYPE_REGISTRY_SIGNATURE))
						{
							try
							{

								String strippedFontName = fontNameMs.Substring(0, fontNameMs.Length - TRUE_TYPE_REGISTRY_SIGNATURE.Length).Trim();
								String fontFile = (String)FONTS_ENTRY.GetValue(fontNameMs);
								fontFileLocation = Utilities.findFileInPath(fontFile);

								if (fontFile.ToLower().EndsWith("ttc"))
								{
									String[] strippedFontNames = strippedFontName.Split(new char[] { '&' });
									for (int j = 0; j < strippedFontNames.Length; j++)
									{
										String strippedFontName1 = strippedFontNames[j].Trim();
										staticProps.setProperty(Const.MS_FONT_LOCATION, strippedFontName1, fontFileLocation + "," + j);
									}
								}
								else
								{
									staticProps.setProperty(Const.MS_FONT_LOCATION, strippedFontName, fontFileLocation);
								}
							}
							catch (IOException) {; }
						}
					}
					FONTS_ENTRY.Close();
				}
				catch (Exception ex)
				{
					GXLogging.Warn(log, "getTrueTypeFontLocation error", ex);
				}
			}
			return staticProps.getProperty(Const.MS_FONT_LOCATION, fontName);
		}  
  
		private static bool isWinNT()
		{
			return Environment.OSVersion.ToString().IndexOf("NT") > 0;
		}

	}

	public class Utilities
	{
		private static List<String> predefinedSearchPath = new List<string>(); 
		
		public static void addPredefinedSearchPaths(String [] predefinedPaths)
		{
			lock (predefinedSearchPath)
				predefinedSearchPath.InsertRange(0, predefinedPaths);
		}

		public static String getPredefinedSearchPaths()
		{
			return string.Join(";", predefinedSearchPath);
		}

		public static String findFileInPath(String filename) 
		{
			if (File.Exists(filename)) return Path.GetFullPath(filename);
			else filename = Path.GetFileName(filename);
		
			foreach (string path in predefinedSearchPath) {
				string filePath = Path.Combine(path, filename);
				if (File.Exists(filePath))
				{
					return filePath;
				}
			}
			throw new FileNotFoundException(filename);
		}

		public static ArrayList parseLine(String line, String separator)
		{
			ArrayList partes=new ArrayList();
			int index = 0,offset = 0;
			int indexComillas;
			bool startingComillas = true;
			if(line==null)return partes;
			if(!line.EndsWith(separator))line+=separator;
			if((indexComillas = line.IndexOf('\"')) == -1)indexComillas = Int32.MaxValue;
			while((index=line.IndexOf(separator,startingComillas ? offset : indexComillas))!=-1)
			{
				if(index > indexComillas)
				{
					if((indexComillas = line.IndexOf('\"', index)) == -1)indexComillas = Int32.MaxValue;
					if(startingComillas)
					{
						startingComillas = false;
						offset++;
						if(indexComillas == Int32.MaxValue)break;
						else continue;
					}
					else startingComillas = true;
					index--;
				}
				partes.Add(line.Substring(offset,index-offset));
				offset=index;
				while(line.Substring(++offset).StartsWith(separator)&&offset<line.Length); // remove separators in a row
			}
			if(!startingComillas)  
				partes.Add(line.Substring(offset, line.Length - separator.Length-offset));
			return partes;
		}

	}
	public enum PdfConformanceLevel
	{
		None,
		Pdf_A1B,
		Pdf_X1A2001,
		Pdf_A1A,
		Pdf_A2A,
		Pdf_A2B,
		Pdf_A3A,
		Pdf_A3B
	}
}

