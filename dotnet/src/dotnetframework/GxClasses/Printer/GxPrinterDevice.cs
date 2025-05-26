using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Drawing;
using System.Drawing.Printing;
#if NETCORE
using System.Runtime.Versioning;
#endif

namespace GeneXus.Printer
{
#if NETCORE
	[SupportedOSPlatform("windows")]
#endif

	public class GxPrinterDevice : IPrintHandler
	{
		Font currentFont;
		Color currentForeColor;
		Color currentBackColor;
		String lastLine;

		bool pageStarted;
		StreamReader streamToRead;
		Graphics _gr;

		int originalScaleX = 96;        // Dots x inch
		int originalScaleY = 96;        // Dots x inch

		public GxPrinterDevice(StreamReader sr)
		{
			streamToRead = sr;
			lastLine = null;
		}
		public string Name
		{
			get { return "GxPrinterDevice"; }
		}
		public bool CanSlot
		{
			get { return true; }
		}
		public void Open(StreamWriter output)
		{
		}
		public StreamReader InputStream
		{
			get { return streamToRead; }
			set { streamToRead = value; lastLine = null; }
		}
		public void Open()
		{
		}
		public void Print(string configString)
		{
#if !NETCORE
			PrintingPermission pp = new PrintingPermission(PrintingPermissionLevel.AllPrinting);
			pp.Demand();
#endif
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
			pd.Dispose();
		}
		public void Close()
		{
			try
			{
				streamToRead.Close();
			}
			catch (Exception)
			{
				//NOOP
			}
		}
		void initPrnReport(string configString, PrinterSettings printerSettings, PageSettings pageSettings)
		{
			NameValueCollection configSettings;

			configSettings = GxPrinterConfig.ConfigPrinterSettings(configString);

			if (configSettings == null)
				return;

			originalScaleX = Convert.ToInt32(configSettings["XPAGE"]);
			originalScaleY = Convert.ToInt32(configSettings["YPAGE"]);

			if (printerSettings != null)
			{

				if (configSettings["PRINTER"].Length > 0)
					foreach (string prnName in PrinterSettings.InstalledPrinters)
						if (prnName == configSettings["PRINTER"])
						{
							printerSettings.PrinterName = configSettings["PRINTER"];
							break;
						}
				// Number of copies
				printerSettings.Copies = Convert.ToInt16(configSettings["COPIES"]);
				// Duplex
				if (printerSettings.CanDuplex)
					printerSettings.Duplex = Convert.ToInt32(configSettings["DUPLEX"]) == 1 ? Duplex.Simplex : Duplex.Vertical;
			}
			if (pageSettings != null)
			{
				// Landscape
				pageSettings.Landscape = Convert.ToInt32(configSettings["ORIENTATION"]) != 1;
				// Color
				if (printerSettings.SupportsColor)
					pageSettings.Color = Convert.ToInt32(configSettings["COLOR"]) == 1;
				// paper try

				foreach (PaperSource pSrc in printerSettings.PaperSources)
					if ((int)(pSrc.Kind) == Convert.ToInt32(configSettings["DEFAULTSOURCE"]))
					{
						pageSettings.PaperSource = pSrc;
						break;
					}
				// paper size
				if (configSettings["PAPERSIZE"] == "0")
					pageSettings.PaperSize = new PaperSize("Custom",
						Convert.ToInt32(configSettings["PAPERLENGTH"]) / 1440 * 100,
						Convert.ToInt32(configSettings["PAPERWIDTH"]) / 1440 * 100);
				else
					foreach (PaperSize pSz in printerSettings.PaperSizes)
						if ((int)(pSz.Kind) == Convert.ToInt32(configSettings["PAPERSIZE"]))
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

			while (lastLine != null && printPage)
			{
				printPage = processPrinterCommand(ev, lastLine, ref morePages);
				lastLine = streamToRead.ReadLine();
			}
			if (lastLine == null || lastLine == GxReportBuilderNative.END_DOCUMENT || !morePages)
				ev.HasMorePages = false;
			else
				ev.HasMorePages = true;
		}
		private bool processPrinterCommand(PrintPageEventArgs ev, string line, ref bool morePages)
		{
			GroupCollection grCol;
			string cmd = line.Substring(0, 3);
			switch (cmd.ToUpper().Trim())
			{
				case "DR":
					if ((grCol = GxPrintCommandParser.ParseRect(line)) != null)
						DrawRect(new Point(Convert.ToInt32(grCol["left"].Value), Convert.ToInt32(grCol["top"].Value)),
									new Point(Convert.ToInt32(grCol["right"].Value), Convert.ToInt32(grCol["bottom"].Value)),
									Convert.ToInt32(grCol["pen"].Value),
									Color.FromArgb(Convert.ToInt32(grCol["fr"].Value),
													Convert.ToInt32(grCol["fg"].Value),
													Convert.ToInt32(grCol["fb"].Value)),
									Convert.ToInt32(grCol["bm"].Value) == 1 ?
										Color.FromArgb(Convert.ToInt32(grCol["br"].Value),
														Convert.ToInt32(grCol["bg"].Value),
														Convert.ToInt32(grCol["bb"].Value)) :
										Color.Empty);
					break;
				case "DL":
					if ((grCol = GxPrintCommandParser.ParseLine(line)) != null)
						DrawLine(
									new Point(Convert.ToInt32(grCol["left"].Value), Convert.ToInt32(grCol["top"].Value)),
									new Point(Convert.ToInt32(grCol["right"].Value), Convert.ToInt32(grCol["bottom"].Value)),
									Convert.ToInt32(grCol["width"].Value),
									Color.FromArgb(Convert.ToInt32(grCol["fr"].Value),
													Convert.ToInt32(grCol["fg"].Value),
													Convert.ToInt32(grCol["fb"].Value)));
					break;
				case "DB":
					if ((grCol = GxPrintCommandParser.ParseBitmap(line)) != null)
						DrawBitmap(
									grCol["bitmap"].Value,
									new Point(Convert.ToInt32(grCol["left"].Value), Convert.ToInt32(grCol["top"].Value)),
									new Point(Convert.ToInt32(grCol["right"].Value), Convert.ToInt32(grCol["bottom"].Value)));
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
					if ((grCol = GxPrintCommandParser.ParseTextAttributes(line)) != null)
						setTextAttributes(grCol["name"].Value,
									Convert.ToInt32(grCol["size"].Value),
									Convert.ToInt32(grCol["bold"].Value) == 1,
									Convert.ToInt32(grCol["italic"].Value) == 1,
									Convert.ToInt32(grCol["underline"].Value) == 1,
									Convert.ToInt32(grCol["strike"].Value) == 1,
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
			if (fontBold) fntSt |= FontStyle.Bold;
			if (fontItalic) fntSt |= FontStyle.Italic;
			if (fontUnderline) fntSt |= FontStyle.Underline;
			if (fontStrikethru) fntSt |= FontStyle.Strikeout;
			this.currentFont = new Font(fontName, fontSize, fntSt);
			this.currentForeColor = Color.FromArgb(foreRed, foreGreen, foreBlue);
			if (backMode != 1)
				this.currentBackColor = Color.Empty;
			else
				this.currentBackColor = Color.FromArgb(backRed, backGreen, backBlue);
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
			_gr.DrawImage(img, l, t, r - l, b - t);
		}
		void DrawText(string text, Point p1, Point p2, Font fnt, int align, Color foreColor, Color backColor)
		{
			float l = convertX(p1.X);
			float t = convertY(p1.Y);
			float r = convertX(p2.X);
			float b = convertY(p2.Y);
			DrawRect(p1, p2, 0, foreColor, backColor);
			RectangleF rect = new RectangleF(correctTextCoor(l, align), t, r - l, b - t);
			using (Brush br = new SolidBrush(foreColor))
			{
				_gr.DrawString(text, fnt, br, rect, stringFormatFromAlign(align));
			}
		}
		StringFormat stringFormatFromAlign(int align)
		{

			StringFormat sfmt = new StringFormat();
			sfmt.Trimming = StringTrimming.None;
			if ((align & 256) > 0)
				sfmt.FormatFlags |= StringFormatFlags.NoClip;
			if ((align & 16) == 0)
				sfmt.FormatFlags |= StringFormatFlags.NoWrap;
			if ((align & 2) > 0)
				sfmt.Alignment |= StringAlignment.Far;
			else if ((align & 1) > 0)
				sfmt.Alignment |= StringAlignment.Center;
			else
				sfmt.Alignment |= StringAlignment.Near;
			return sfmt;
		}
		float correctTextCoor(float l, int align)
		{
			int mn = 4;
			float ret;
			if ((align & 2) > 0)
				ret = l + mn;
			else if ((align & 1) > 0)
				ret = l;
			else
				ret = l - mn;
			return ret;
		}
		float convertX(float x)
		{
			return x / this.originalScaleX * 100;
		}
		float convertY(float y)
		{
			return y / this.originalScaleY * 100;
		}
	}
}