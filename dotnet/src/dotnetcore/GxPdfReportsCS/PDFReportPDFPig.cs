using System;
using System.Collections.Generic;
using System.IO;
using GeneXus;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using PdfRectangle = UglyToad.PdfPig.Core.PdfRectangle;
using static UglyToad.PdfPig.Writer.PdfDocumentBuilder;
using PageSize = UglyToad.PdfPig.Content.PageSize;
using Color = System.Drawing.Color;
using static UglyToad.PdfPig.Writer.PdfPageBuilder;
using System.Net;

namespace GeneXus.Printer
{
	public class GxReportBuilderPDFPig : GxReportBuilderPdf
	{
		static IGXLogger log = GXLoggerFactory.GetLogger<GxReportBuilderPDFPig>();
		public GxReportBuilderPDFPig() { }
		public GxReportBuilderPDFPig(string appPath, Stream outputStream)
		{

			_pdfReport = new com.genexus.reports.PDFReportPDFPig(appPath);
			if (outputStream != null)
			{
				_pdfReport.setOutputStream(outputStream);
				GXLogging.Debug(log, "GxReportBuilderPdf outputStream: binaryWriter");
			}
		}
	}

}

namespace com.genexus.reports
{
	public class PDFReportPDFPig : PDFReportBase
	{
		static IGXLogger log = GXLoggerFactory.GetLogger<PDFReportPDFPig>();

		private PdfDocumentBuilder documentBuilder;
		private PdfPageBuilder pageBuilder;
		private ExtendedPageSize pageSize;

		private AddedFont baseFont;
		private string baseFontName;
		private bool fontBold;
		private bool fontItalic;
		private Color backColor, foreColor;

		private Dictionary<string, AddedImage> documentImages;

		private string barcodeType = null;

		private static HashSet<string> supportedHTMLTags = new HashSet<string>
		{
			"div",
			"span",
			"p",
			"h1",
			"h2",
			"h3",
			"h4",
			"h5",
			"h6",
			"a",
		};

		protected override void init(ref int gxYPage, ref int gxXPage, int pageWidth, int pageLength)
		{
			try
			{
				pageSize = ComputePageSize(leftMargin, topMargin, pageWidth, pageLength, props.getBooleanGeneralProperty(Const.MARGINS_INSIDE_BORDER, Const.DEFAULT_MARGINS_INSIDE_BORDER));
				documentBuilder = new PdfDocumentBuilder(outputStream);

				pageBuilder = documentBuilder.AddPage(pageSize.Width, pageSize.Height);
				gxXPage = (int) pageBuilder.PageSize.TopRight.X;
				if (props.getBooleanGeneralProperty(Const.FIX_SAC24437, true))
					gxYPage = (int)(pageLength / GX_PAGE_SCALE_Y);
				else
					gxYPage = (int)(pageLength / GX_PAGE_SCALE_Y_OLD);

			}
			catch (Exception e)
			{
				GXLogging.Error(log, "GxPrintInit failed", e);
			}
		}

		public PDFReportPDFPig(String appPath) : base(appPath)
		{
			documentBuilder = null;
			documentImages = new Dictionary<string, AddedImage>();
		}

		public override void GxStartPage()
		{
			try
			{
				if (pages > 0)
				{
					pageBuilder = documentBuilder.AddPage(pageSize.Width, pageSize.Height);
				}
				pages = pages + 1;
			}
			catch (Exception de)
			{
				GXLogging.Error(log, "GxStartPage error", de);
			}
		}

		internal override bool SetComplainceLevel(PdfConformanceLevel level)
		{
			return false;
		}

		public override void GxDrawRect(int left, int top, int right, int bottom, int pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue,
											int styleTop, int styleBottom, int styleRight, int styleLeft, int cornerRadioTL, int cornerRadioTR, int cornerRadioBL, int cornerRadioBR)
		{
			float penAux = (float)convertScale(pen);
			float rightAux = (float)convertScale(right);
			float bottomAux = (float)convertScale(bottom);
			float leftAux = (float)convertScale(left);
			float topAux = (float)convertScale(top);

			float x1, y1, x2, y2;
			x1 = leftAux + leftMargin;
			y1 = (float)pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin;
			x2 = rightAux + leftMargin;
			y2 = (float)pageBuilder.PageSize.TopRight.Y - topAux - topMargin - bottomMargin;

			// Corner styling and radio are not taken into consideration because there is no way to render rounded rectangles or style them by corner using PDF Pig

			if (pen > 0)
				pageBuilder.SetStrokeColor( (byte)foreRed, (byte)foreGreen, (byte)foreBlue);
			else
				pageBuilder.SetStrokeColor( (byte)backRed, (byte)backGreen, (byte)backBlue);

			// There seems to be no way of setting the line cap style
			if (backMode != 0)
			{
				pageBuilder.SetTextAndFillColor((byte)backRed, (byte)backGreen, (byte)backBlue);
				pageBuilder.DrawRectangle(new PdfPoint(x1, y1), (decimal)(x2 - x1), (decimal)(y2 - y1), (decimal)penAux, true);
			}
			else
			{
				pageBuilder.DrawRectangle(new PdfPoint(x1, y1), (decimal)(x2 - x1), (decimal)(y2 - y1), (decimal) penAux, false);
			}
			pageBuilder.ResetColor();
		}

		public override void GxDrawLine(int left, int top, int right, int bottom, int width, int foreRed, int foreGreen, int foreBlue, int style)
		{
			try
			{
				float widthAux = (float)convertScale(width);
				float rightAux = (float)convertScale(right);
				float bottomAux = (float)convertScale(bottom);
				float leftAux = (float)convertScale(left);
				float topAux = (float)convertScale(top);

				GXLogging.Debug(log, "GxDrawLine -> (" + left + "," + top + ") - (" + right + "," + bottom + ") Width: " + width);

				float x1, y1, x2, y2;

				x1 = leftAux + leftMargin;
				y1 = (float)pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin;
				x2 = rightAux + leftMargin;
				y2 = (float)pageBuilder.PageSize.TopRight.Y - topAux - topMargin - bottomMargin;

				pageBuilder.SetStrokeColor((byte)foreRed, (byte)foreGreen, (byte)foreBlue);

				if (style != 0)
				{
					// There seems to be no way of creating a dashed line in PDFPig
					float[] dashPattern = getDashedPattern(style);
				}

				pageBuilder.DrawLine(new PdfPoint(x1, y1), new PdfPoint(x2, y2), (decimal)widthAux);
				pageBuilder.ResetColor();
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "GxDrawLine  error", e);
			}
		}

		public override void GxDrawBitMap(String bitmap, int left, int top, int right, int bottom, int aspectRatio)
		{
			try
			{
				string imageType = Path.GetExtension(bitmap).Substring(1);
				if (imageType.ToLower() != "jpeg" || imageType.ToLower() != "png")
				{
					GXLogging.Error(log, "GxDrawBitMap : PDFPig only supports adding jpeg or png images to documents");
					return;
				}

				float rightAux = (float)convertScale(right);
				float bottomAux = (float)convertScale(bottom);
				float leftAux = (float)convertScale(left);
				float topAux = (float)convertScale(top);
				float x = leftAux + leftMargin;
				float y = (float) pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin;

				PdfRectangle position = new PdfRectangle(new PdfPoint(rightAux, bottomAux), new PdfPoint(leftAux, topAux));

				AddedImage image;
				AddedImage imageRef;
				if (documentImages != null && documentImages.TryGetValue(bitmap, out imageRef))
				{
					image = imageRef;
				}
				else
				{
					try
					{
						if (!Path.IsPathRooted(bitmap))
						{

							image = imageType == "jpeg" ? pageBuilder.AddJpeg(File.ReadAllBytes(defaultRelativePrepend + bitmap), position) : pageBuilder.AddPng(File.ReadAllBytes(defaultRelativePrepend + bitmap), position);
							if (image == null)
							{
								bitmap = webAppDir + bitmap;
								image = imageType == "jpeg" ? pageBuilder.AddJpeg(File.ReadAllBytes(bitmap), position) : pageBuilder.AddPng(File.ReadAllBytes(bitmap), position);
							}
							else
							{
								bitmap = defaultRelativePrepend + bitmap;
							}
						}
						else
						{
							image = imageType == "jpeg" ? pageBuilder.AddJpeg(File.ReadAllBytes(bitmap), position) : pageBuilder.AddPng(File.ReadAllBytes(bitmap), position);
						}
					}
					catch (Exception)
					{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
						using (WebClient webClient = new WebClient())
						{
							byte[] imageBytes = webClient.DownloadData(bitmap);
							image = imageType == "jpeg" ? pageBuilder.AddJpeg(imageBytes, position) : pageBuilder.AddPng(imageBytes, position);
						}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
					}
					if (documentImages == null)
					{
						documentImages = new Dictionary<string, AddedImage>();
					}
					documentImages[bitmap] = image;
				}
				GXLogging.Debug(log, "GxDrawBitMap ->  '" + bitmap + "' [" + left + "," + top + "] - Size: (" + (right - left) + "," + (bottom - top) + ")");
			}
			catch (IOException ioe)
			{
				GXLogging.Error(log, "GxDrawBitMap io error", ioe);
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "GxDrawBitMap error", e);
			}
		}

		public override void GxAttris(String fontName, int fontSize, bool fontBold, bool fontItalic, bool fontUnderline, bool fontStrikethru, int Pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue)
		{
			bool isCJK = false;
			bool embeedFont = IsEmbeddedFont(fontName);
			string originalFontName = fontName;
			if (!embeedFont)
			{
				fontName = getSubstitute(fontName);
			}

			String fontSubstitute = "";
			if (originalFontName != fontName)
			{
				fontSubstitute = "Original Font: " + originalFontName + " Substitute";
			}

			GXLogging.Debug(log, "GxAttris: ");
			GXLogging.Debug(log, "\\-> Font: " + fontName + " (" + fontSize + ")" + (fontBold ? " BOLD" : "") + (fontItalic ? " ITALIC" : "") + (fontStrikethru ? " Strike" : ""));
			GXLogging.Debug(log, "\\-> Fore (" + foreRed + ", " + foreGreen + ", " + foreBlue + ")");
			GXLogging.Debug(log, "\\-> Back (" + backRed + ", " + backGreen + ", " + backBlue + ")");

			/*
			There seems to be no way of natively working with barcodes in PDFPig.
			The alternative is to just write text with a provided barcode font.
			*/
			if (barcode128AsImage && fontName.ToLower().IndexOf("barcode 128") >= 0 || fontName.ToLower().IndexOf("barcode128") >= 0)
				barcodeType = "barcode128";

			this.fontUnderline = fontUnderline;
			this.fontStrikethru = fontStrikethru;
			this.fontSize = fontSize;
			this.fontBold = fontBold;
			this.fontItalic = fontItalic;
			foreColor = Color.FromArgb(foreRed, foreGreen, foreBlue);
			backColor = Color.FromArgb(backRed, backGreen, backBlue);

			backFill = (backMode != 0);
			try
			{
				string f = fontName.ToLower();
				if (PDFFont.isType1(fontName))
				{
					for (int i = 0; i < Type1FontMetrics.CJKNames.Length; i++)
					{
						if (Type1FontMetrics.CJKNames[i][0].ToLower().Equals(f) ||
							Type1FontMetrics.CJKNames[i][1].ToLower().Equals(f))
						{
							string style = "";
							if (fontBold && fontItalic)
								style = "BoldItalic";
							else
							{
								if (fontItalic)
									style = "Italic";
								if (fontBold)
									style = "Bold";
							}
							setAsianFont(fontName, style);
							isCJK = true;
							break;
						}
					}
					if (!isCJK)
					{
						int style = 0;
						if (fontBold && fontItalic)
							style = style + 3;
						else
						{
							if (fontItalic)
								style = style + 2;
							if (fontBold)
								style = style + 1;
						}
						for (int i = 0; i < PDFFont.base14.Length; i++)
						{
							if (PDFFont.base14[i][0].ToLower().Equals(f))
							{
								fontName = PDFFont.base14[i][1 + style].Substring(1);
								break;
							}
						}
						baseFont = createType1FontFromName(fontName);
						if (baseFont != null)
							baseFontName = fontName;
						else
						{
							byte[] fontBytes = File.ReadAllBytes(GetFontLocation(fontName));
							baseFont = documentBuilder.AddTrueTypeFont(fontBytes);
							baseFontName = fontName;
						}
					}
				}
				else
				{
					String style = "";
					if (fontBold && fontItalic)
						style = ",BoldItalic";
					else
					{
						if (fontItalic)
							style = ",Italic";
						if (fontBold)
							style = ",Bold";
					}

					fontName = fontName + style;
					string fontPath = GetFontLocation(fontName);
					bool foundFont = true;
					if (string.IsNullOrEmpty(fontPath))
					{
						fontPath = new MSPDFFontDescriptor().getTrueTypeFontLocation(fontName);
						if (string.IsNullOrEmpty(fontPath))
						{
							baseFont = documentBuilder.AddStandard14Font(Standard14Font.Helvetica);
							baseFontName = "helvetica";
							foundFont = false;
						}
					}
					if (foundFont)
					{
						baseFont = createType1FontFromName(fontName);
						if (baseFont != null)
							baseFontName = fontName;
						else
						{
							byte[] fontBytes = File.ReadAllBytes(GetFontLocation(fontName));
							baseFont = documentBuilder.AddTrueTypeFont(fontBytes);
							baseFontName = fontName;
						}
					}
				}
			}
			catch (Exception e)
			{
				GXLogging.Debug(log, "GxAttris DocumentException", e);
				throw e;
			}
		}

		private AddedFont createType1FontFromName(String fontName)
		{
			switch (fontName.ToLower())
			{
				case "times-roman":
					return documentBuilder.AddStandard14Font(Standard14Font.TimesRoman);
				case "Ttimes-bold":
					return documentBuilder.AddStandard14Font(Standard14Font.TimesBold);
				case "times-italic":
					return documentBuilder.AddStandard14Font(Standard14Font.TimesItalic);
				case "times-bolditalic":
					return documentBuilder.AddStandard14Font(Standard14Font.TimesBoldItalic);
				case "helvetica":
					return documentBuilder.AddStandard14Font(Standard14Font.Helvetica);
				case "helvetica-bold":
					return documentBuilder.AddStandard14Font(Standard14Font.HelveticaBold);
				case "helvetica-oblique":
					return documentBuilder.AddStandard14Font(Standard14Font.HelveticaOblique);
				case "helvetica-boldoblique":
					return documentBuilder.AddStandard14Font(Standard14Font.HelveticaBoldOblique);
				case "courier":
					return documentBuilder.AddStandard14Font(Standard14Font.Courier);
				case "courier-bold":
					return documentBuilder.AddStandard14Font(Standard14Font.CourierBold);
				case "courier-oblique":
					return documentBuilder.AddStandard14Font(Standard14Font.CourierOblique);
				case "courier-boldoblique":
					return documentBuilder.AddStandard14Font(Standard14Font.CourierBoldOblique);
				case "symbol":
					return documentBuilder.AddStandard14Font(Standard14Font.Symbol);
				case "zapfdingbats":
					return documentBuilder.AddStandard14Font(Standard14Font.ZapfDingbats);
				default:
					// Use Helvetica as default font is fontName does not match any Type 1 font
					return documentBuilder.AddStandard14Font(Standard14Font.Helvetica);
			}
		}

		public override void setAsianFont(String fontName, String style)
		{
			try
			{
				string fontPath = GetFontLocation(fontName);
				if (string.IsNullOrEmpty(fontPath))
				{
					MSPDFFontDescriptor fontDescriptor = new MSPDFFontDescriptor();
					fontPath = fontDescriptor.getTrueTypeFontLocation(fontName);
				}
				byte[] fontBytes = File.ReadAllBytes(fontPath);
				baseFont = documentBuilder.AddTrueTypeFont(fontBytes);
			}
			catch (IOException de)
			{
				GXLogging.Debug(log, "setAsianFont  error", de);
			}
		}

		public override void GxDrawText(String sTxt, int left, int top, int right, int bottom, int align, int htmlformat, int border, int valign)
		{
			bool printRectangle = false;
			if (props.getBooleanGeneralProperty(Const.BACK_FILL_IN_CONTROLS, true))
				printRectangle = true;

			if (printRectangle && (border == 1 || backFill))
			{
				GxDrawRect(left, top, right, bottom, border, foreColor.R, foreColor.G, foreColor.B, backFill ? 1 : 0, backColor.R, backColor.G, backColor.B, 0, 0);
			}

			sTxt = sTxt.TrimEnd(TRIM_CHARS);

			float rectangleWidth = (float)convertScale(right - left);
			float rectangleHeight = (float)convertScale(top - bottom);

			int bottomOri = bottom;
			int topOri = top;

			if (rectangleHeight / convertScale(fontSize) >= 2 && !((align & 16) == 16) && htmlformat != 1)
			{
				if (valign == (int)VerticalAlign.TOP)
					bottom = top + (int)reconvertScale(lineHeight);
				else if (valign == (int)VerticalAlign.BOTTOM)
					top = bottom - (int)reconvertScale(lineHeight);
			}

			float bottomAux = (float)convertScale(bottom) - ((float)convertScale(bottom - top)) / 2;
			float topAux = (float)convertScale(top) + ((float)convertScale(bottom - top)) / 2;

			float startHeight = bottomAux - topAux;

			float leftAux = (float)convertScale(left);
			float rightAux = (float)convertScale(right);
			int alignment = align & 3;
			bool autoResize = (align & 256) == 256;

			if (htmlformat == 1)
			{
				if (supportedHTMLTags == null) { return; }
				return;
			}

			if (barcodeType != null)
			{
				PdfPoint barcodeStartingPoint;
				switch (alignment)
				{
					case 1: // Center Alignment
						barcodeStartingPoint = new PdfPoint(
								((leftAux + rightAux) / 2) + leftMargin,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight
							);
						break;
					case 2: // Right Alignment
						barcodeStartingPoint = new PdfPoint(
								rightAux + leftMargin,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight
							);
						break;
					default: // Left Alignment (Corresponds to alignment = 0 but used as default)
						barcodeStartingPoint = new PdfPoint(
								leftAux + leftMargin,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight
							);
						break;

				}
				pageBuilder.SetTextAndFillColor(0, 0, 0);
				pageBuilder.AddText(sTxt, fontSize, barcodeStartingPoint, baseFont);
				return;
			}

			if (backFill)
			{
				PdfPoint rectangleStartingPoint;
				switch (alignment)
				{
					case 1: // Center Alignment
						rectangleStartingPoint = new PdfPoint(
								(leftAux + rightAux) / 2 + leftMargin - rectangleWidth / 2,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin
							);
						break;
					case 2: // Right Alignment
						rectangleStartingPoint = new PdfPoint(
								rightAux + leftMargin - rectangleWidth,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin
							);
						break;
					default: // Left Alignment (Corresponds to alignment = 0 but used as default)
						rectangleStartingPoint = new PdfPoint(
								leftAux + leftMargin,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin
							);
						break;

				}
				pageBuilder.SetTextAndFillColor(backColor.R, backColor.G, backColor.B);
				pageBuilder.DrawRectangle(rectangleStartingPoint, (decimal)rectangleWidth, (decimal)rectangleHeight, 1, true);
			}

			pageBuilder.SetTextAndFillColor(foreColor.R, foreColor.G, foreColor.B);

			if (fontUnderline)
			{
				PdfPoint underlineStartingPoint;
				PdfPoint underlineEndingPoint;
				switch (alignment)
				{
					case 1: // Center Alignment
						underlineStartingPoint = new PdfPoint(
								(leftAux + rightAux) / 2 + leftMargin - rectangleWidth / 2,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight
							);
						underlineEndingPoint = new PdfPoint(
								(leftAux + rightAux) / 2 + leftMargin + rectangleWidth / 2,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight
							);
						break;
					case 2: // Right Alignment
						underlineStartingPoint = new PdfPoint(
								rightAux + leftMargin - rectangleWidth,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight
							);
						underlineEndingPoint = new PdfPoint(
								rightAux + leftMargin,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight
							);
						break;
					default: // Left Alignment (Corresponds to alignment = 0 but used as default)
						underlineStartingPoint = new PdfPoint(
								leftAux + leftMargin,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight
							);
						underlineEndingPoint = new PdfPoint(
								leftAux + leftMargin + rectangleWidth,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight
							);
						break;

				}
				pageBuilder.DrawLine(underlineStartingPoint, underlineEndingPoint, (decimal)convertScale(fontSize) / 10);
			}

			if (fontStrikethru)
			{
				float strikethruSeparation = lineHeight / 2;
				PdfPoint strikethruStartingPoint;
				PdfPoint strikethruEndingPoint;
				switch (alignment)
				{
					case 1: // Center Alignment
						strikethruStartingPoint = new PdfPoint(
								(leftAux + rightAux) / 2 + leftMargin - rectangleWidth / 2,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight + strikethruSeparation
							);
						strikethruEndingPoint = new PdfPoint(
								(leftAux + rightAux) / 2 + leftMargin + rectangleWidth / 2,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight + strikethruSeparation
							);
						break;
					case 2: // Right Alignment
						strikethruStartingPoint = new PdfPoint(
								rightAux + leftMargin - rectangleWidth,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight + strikethruSeparation
							);
						strikethruEndingPoint = new PdfPoint(
								rightAux + leftMargin,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight + strikethruSeparation
							);
						break;
					default: // Left Alignment (Corresponds to alignment = 0 but used as default)
						strikethruStartingPoint = new PdfPoint(
								leftAux + leftMargin,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight + strikethruSeparation
							);
						strikethruEndingPoint = new PdfPoint(
								leftAux + leftMargin + rectangleWidth,
								pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight + strikethruSeparation
							);
						break;

				}
				pageBuilder.DrawLine(strikethruStartingPoint, strikethruEndingPoint, (decimal)convertScale(fontSize) / 10);
			}

			/*
			 There seems to be no way of wrapping text in PDF Pig as the library does not provide a way to
			 measure the width of a given string
			*/

			PdfPoint textStartingPoint;
			switch (alignment)
			{
				case 1: // Center Alignment
					textStartingPoint = new PdfPoint(
							((leftAux + rightAux) / 2) + leftMargin,
							pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight
						);
					break;
				case 2: // Right Alignment
					textStartingPoint = new PdfPoint(
							rightAux + leftMargin,
							pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight
						);
					break;
				default: // Left Alignment (Corresponds to alignment = 0 but used as default)
					textStartingPoint = new PdfPoint(
							leftAux + leftMargin,
							pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin + startHeight
						);
					break;

			}
			pageBuilder.AddText(sTxt, fontSize, textStartingPoint, baseFont);
		}

		private ExtendedPageSize ComputePageSize(float leftMargin, float topMargin, int width, int length, bool marginsInsideBorder)
		{
			if ((leftMargin == 0 && topMargin == 0) || marginsInsideBorder)
			{
				if (length == 23818 && width == 16834)
					return new ExtendedPageSize(PageSize.A3);
				else if (length == 16834 && width == 11909)
					return new ExtendedPageSize(PageSize.A4);
				else if (length == 11909 && width == 8395)
					return new ExtendedPageSize(PageSize.A5);
				else if (length == 15120 && width == 10440)
					return new ExtendedPageSize(PageSize.Executive);
				else if (length == 20160 && width == 12240)
					return new ExtendedPageSize(PageSize.Legal);
				else if (length == 15840 && width == 12240)
					return new ExtendedPageSize(PageSize.Letter);
				else
					return new ExtendedPageSize((width / PAGE_SCALE_X), (length / PAGE_SCALE_Y));
			}
			return new ExtendedPageSize((width / PAGE_SCALE_X) + leftMargin, (length / PAGE_SCALE_Y) + topMargin);

		}

		public override void GxEndDocument()
		{
			if (pages == 0)
			{
				GxStartPage();
			}

			/*
			 There seems to be no way of setting the number of copies and the duplex value for the viewer preferences
			*/

			/*
			 There seems to be no way of embedding javascript into the document
			*/

			try
			{
				byte[] documentBytes = documentBuilder.Build();
				File.WriteAllBytes(docName, documentBytes);
			}
			catch (IOException e)
			{
				GXLogging.Debug(log,"GxEndDocument: failed to write document to the output stream", e);
			}

			String serverPrinting = props.getGeneralProperty(Const.SERVER_PRINTING);
			bool printingScript = (outputType == Const.OUTPUT_PRINTER || outputType == Const.OUTPUT_STREAM_PRINTER) && serverPrinting.Equals("false");
			switch (outputType)
			{
				case Const.OUTPUT_SCREEN:
					try
					{
						outputStream.Close();
						GXLogging.Debug(log, "GxEndDocument OUTPUT_SCREEN outputstream length" + outputStream.ToString().Length);
					}
					catch (IOException e)
					{
						GXLogging.Error(log, "GxEndDocument OUTPUT_SCREEN error", e);
					}
					try { showReport(docName, modal); }
					catch (Exception) { }
					break;

				case Const.OUTPUT_PRINTER:
					try { outputStream.Close(); }
					catch (IOException) {; } // Cierro el archivo
					try
					{
						if (!serverPrinting.Equals("false") && !printingScript)
						{
							printReport(docName, this.printerOutputMode == 0, printerSettings.getProperty(form, Const.PRINTER));
						}
					}
					catch (Exception) { }
					break;

				case Const.OUTPUT_FILE:
					try
					{
						outputStream.Close();
						GXLogging.Debug(log, "GxEndDocument OUTPUT_FILE outputstream length" + outputStream.ToString().Length);
					}
					catch (IOException e)
					{
						GXLogging.Error(log, "GxEndDocument OUTPUT_FILE error", e);
					}
					break;

				case Const.OUTPUT_STREAM:
				case Const.OUTPUT_STREAM_PRINTER:
				default: break;
			}
			outputStream = null;
		}

	}
	internal class ExtendedPageSize
	{
		internal double Width;
		internal double Height;
		internal PageSize PageSize;
		internal ExtendedPageSize(double w, double h)
		{
			Width = w;
			Height = h;
		}
		internal ExtendedPageSize(PageSize pageSize)
		{
			PageSize = pageSize;
		}

	}
}
