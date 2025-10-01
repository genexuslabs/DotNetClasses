using System;
using System.Collections.Generic;
#if NETCORE
using GeneXus.Drawing;
using GeneXus.Drawing.Imaging;
using GeneXus.Drawing.Text;
using GeneXus.Drawing.Drawing2D;
using Color = GeneXus.Drawing.Color;
using Font = GeneXus.Drawing.Font;
#else
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
#endif
using System.IO;
using System.Text;
using GeneXus;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using static GeneXus.Utils.StringUtil;
using static UglyToad.PdfPig.Writer.PdfDocumentBuilder;
using static UglyToad.PdfPig.Writer.PdfPageBuilder;
using PageSize = UglyToad.PdfPig.Content.PageSize;
using PdfRectangle = UglyToad.PdfPig.Core.PdfRectangle;
using System.Net;
using GeneXus.Http;

namespace GeneXus.Printer
{
	public class GxReportBuilderPDFPig : GxReportBuilderPdf
	{
		static IGXLogger log = GXLoggerFactory.GetLogger<GxReportBuilderPDFPig>();
		public GxReportBuilderPDFPig() { }
		public GxReportBuilderPDFPig(string appPath, Stream outputStream)
		{
			_appPath = appPath;
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
		private string baseFontPath;
		private string baseFontName;
		private Color backColor, foreColor;

		private string barcodeType = null;

		private Dictionary<string, AddedImage> documentImages;

		protected override void init(ref int gxYPage, ref int gxXPage, int pageWidth, int pageLength)
		{
			try
			{
				pageSize = ComputePageSize(leftMargin, topMargin, pageWidth, pageLength, props.getBooleanGeneralProperty(Const.MARGINS_INSIDE_BORDER, Const.DEFAULT_MARGINS_INSIDE_BORDER));
				documentBuilder = new PdfDocumentBuilder();

				pageBuilder = pageSize.IsCustomPageSize() ? documentBuilder.AddPage(pageSize.Width, pageSize.Height) : documentBuilder.AddPage(pageSize.PageSize);

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

		public PDFReportPDFPig(string appPath) : base(appPath)
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
					pageBuilder = pageSize.IsCustomPageSize() ? documentBuilder.AddPage(pageSize.Width, pageSize.Height) : documentBuilder.AddPage(pageSize.PageSize);
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

			GXLogging.Info(log, "Corner styling and radio are not taken into consideration because the PDFPig " +
				"API provides no way to render rounded rectangles or style them");

			if (pen > 0)
				pageBuilder.SetStrokeColor( (byte)foreRed, (byte)foreGreen, (byte)foreBlue);
			else
				pageBuilder.SetStrokeColor( (byte)backRed, (byte)backGreen, (byte)backBlue);

			GXLogging.Info(log, "The PDFPig API provides no way of setting the line cap style");

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
					GXLogging.Info(log, "The PDFPig API provides no way of creating a dashed line");
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

		public override void GxDrawBitMap(string bitmap, int left, int top, int right, int bottom, int aspectRatio)
		{
			try
			{
				string imageType = Path.GetExtension(bitmap).Substring(1);

				float rightAux = (float)convertScale(right);
				float bottomAux = (float)convertScale(bottom);
				float leftAux = (float)convertScale(left);
				float topAux = (float)convertScale(top);
				
				float llx = leftAux + leftMargin;
				float lly = (float) pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin;
				float width;
				float height;
				if (aspectRatio == 0)
				{
					width = rightAux - leftAux;
					height = bottomAux - topAux;
				}
				else
				{
					width = (rightAux - leftAux) * aspectRatio;
					height = (bottomAux - topAux) * aspectRatio;
				}

				PdfRectangle position = new PdfRectangle(llx, lly, llx + width, lly + height);

				AddedImage image = null;
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
						image = AddImageFromURL(bitmap, position);
					}
					if (image == null)
					{
						image = AddImageFromURL(bitmap, position);
					}

					if (documentImages == null)
					{
						documentImages = new Dictionary<string, AddedImage>();
					}
					documentImages[bitmap] = image;
				}
				GXLogging.Debug(log, "GxDrawBitMap ->  '" + bitmap + "' [" + left + "," + top + "] - Size: (" + (right - left) + "," + (bottom - top) + ")");
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "GxDrawBitMap error", e);
			}
		}

		private AddedImage AddImageFromURL(string url, PdfRectangle position)
		{
			AddedImage image = null;
			byte[] imageBytes = HttpHelper.DownloadFile(url, out HttpStatusCode statusCode);
			try
			{
				image = pageBuilder.AddJpeg(imageBytes, position);
			}
			catch (Exception)
			{
				pageBuilder.AddPng(imageBytes, position);
			}
			if (image == null)
			{
				GXLogging.Error(log, "GxDrawBitMap : PDFPig only supports adding jpeg or png images to documents");
			}
			return image;
		}

		public override void GxAttris(string fontName, int fontSize, bool fontBold, bool fontItalic, bool fontUnderline, bool fontStrikethru, int pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue)
		{
			bool isCJK = false;
			bool embedFont = IsEmbeddedFont(fontName);
			string originalFontName = fontName;
			if (!embedFont)
			{
				fontName = getSubstitute(fontName);
			}

			string fontSubstitute = originalFontName != fontName ? $"Original Font: {originalFontName} Substitute" : "";

			GXLogging.Debug(log, $"GxAttris: ");
			GXLogging.Debug(log, $"\\-> Font: {fontName} ({fontSize})" +
								 (fontBold ? " BOLD" : "") +
								 (fontItalic ? " ITALIC" : "") +
								 (fontStrikethru ? " Strike" : ""));
			GXLogging.Debug(log, $"\\-> Fore ({foreRed}, {foreGreen}, {foreBlue})");
			GXLogging.Debug(log, $"\\-> Back ({backRed}, {backGreen}, {backBlue})");

			this.fontUnderline = fontUnderline;
			this.fontStrikethru = fontStrikethru;
			this.fontSize = fontSize;
			foreColor = Color.FromArgb(foreRed, foreGreen, foreBlue);
			backColor = Color.FromArgb(backRed, backGreen, backBlue);

			backFill = backMode != 0;

			try
			{
				string fontNameLower = fontName.ToLower();
				if (PDFFont.isType1(fontName))
				{
					foreach (string[] cjkName in Type1FontMetrics.CJKNames)
					{
						if (cjkName[0].ToLower().Equals(fontNameLower) || cjkName[1].ToLower().Equals(fontNameLower))
						{
							string style = fontBold && fontItalic ? "BoldItalic" : fontItalic ? "Italic" : fontBold ? "Bold" : "";
							setAsianFont(fontName, style);
							isCJK = true;
							break;
						}
					}
					if (!isCJK)
					{
						int style = (fontBold && fontItalic ? 3 : 0) + (fontItalic && !fontBold ? 2 : 0) + (fontBold && !fontItalic ? 1 : 0);
						foreach (string[] base14Font in PDFFont.base14)
						{
							if (base14Font[0].ToLower().Equals(fontNameLower))
							{
								fontName = base14Font[1 + style].Substring(1);
								break;
							}
						}
						ProcessBaseFont(fontName);
					}
				}
				else
				{
					string style = (fontBold && fontItalic ? ",BoldItalic" : "") + (fontItalic && !fontBold ? ",Italic" : "") + (fontBold && !fontItalic ? ",Bold" : "");
					fontName += style;
					ProcessBaseFont(fontName);
				}

				if (barcode128AsImage && (
						fontName.ToLower().Contains("barcode 128") || fontName.ToLower().Contains("barcode128")
						||
						(!string.IsNullOrEmpty(baseFontPath) && (baseFontPath.ToLower().Contains("3of9") || baseFontPath.ToLower().Contains("3 of 9")))
					)
				)
				{
					barcodeType = "barcode128";
				}
			}
			catch (Exception e)
			{
				GXLogging.Debug(log, "GxAttris DocumentException", e);
				throw;
			}
		}

		private void ProcessBaseFont(string fontName)
		{
			string fontLocation = GetFontLocation(fontName);
			if (string.IsNullOrEmpty(fontLocation))
			{
				fontLocation = new MSPDFFontDescriptor().getTrueTypeFontLocation(fontName);
			}

			if (!string.IsNullOrEmpty(fontLocation))
			{
				byte[] fontBytes = File.ReadAllBytes(fontLocation);
				baseFont = documentBuilder.AddTrueTypeFont(fontBytes);
			}
			else
			{
				baseFont = createType1FontFromName(fontName);
				if (baseFont == null)
					baseFont = documentBuilder.AddStandard14Font(Standard14Font.TimesRoman);
			}

			baseFontName = fontName;
			baseFontPath = fontLocation;
		}

		private AddedFont createType1FontFromName(string fontName)
		{
			switch (fontName.ToLower())
			{
				case "times-roman":
					return documentBuilder.AddStandard14Font(Standard14Font.TimesRoman);
				case "times-bold":
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
					return null;
			}
		}

		public override void setAsianFont(string fontName, string style)
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

		public override void GxDrawText(string sTxt, int left, int top, int right, int bottom, int align, int htmlformat, int border, int valign)
		{
			bool printRectangle = false;
			if (props.getBooleanGeneralProperty(Const.BACK_FILL_IN_CONTROLS, true))
				printRectangle = true;

			if (printRectangle && (border == 1 || backFill))
			{
				GxDrawRect(left, top, right, bottom, border, foreColor.R, foreColor.G, foreColor.B, backFill ? 1 : 0, backColor.R, backColor.G, backColor.B, 0, 0);
			}

			sTxt = RTrim(sTxt);

			AddedFont font = createType1FontFromName(baseFontName);
			if (font == null)
			{
				try
				{
					byte[] fontBytes = File.ReadAllBytes(baseFontPath);
					font = documentBuilder.AddTrueTypeFont(fontBytes);
				}
				catch
				{
					font = baseFont;
				}
			}

			float captionHeight = CalculateFontCaptionHeight(baseFontPath, fontSize);
			float rectangleWidth = MeasureTextWidth(sTxt, baseFontPath, fontSize);
			float lineHeight = MeasureTextHeight(sTxt, baseFontPath, fontSize);
			float textBlockHeight = (float)convertScale(bottom - top);
			int linesCount = (int)(textBlockHeight / lineHeight);
			int bottomOri = bottom;
			int topOri = top;

			if (linesCount >= 2 && !((align & 16) == 16) && htmlformat != 1)
			{
				if (valign == (int)VerticalAlign.TOP)
					bottom = top + (int)reconvertScale(lineHeight);
				else if (valign == (int)VerticalAlign.BOTTOM)
					top = bottom - (int)reconvertScale(lineHeight);
			}

			float bottomAux = (float)convertScale(bottom) - ((float)convertScale(bottom - top)) / 2;
			float topAux = (float)convertScale(top) + ((float)convertScale(bottom - top)) / 2;

			float startHeight = bottomAux - topAux - captionHeight;

			float leftAux = (float)convertScale(left);
			float rightAux = (float)convertScale(right);
			int alignment = align & 3;
			bool autoResize = (align & 256) == 256;

			if (htmlformat == 1)
			{
				GXLogging.Error(log, "GxDrawText: PDFPig report implementation does not support rendering HTML content into PDF reports");
			}

			if (barcodeType != null)
			{
				PdfRectangle barcodeRectangle;
				switch (alignment)
				{
					case 1: // Center Alignment
						barcodeRectangle = new PdfRectangle(
							(leftAux + rightAux) / 2 + leftMargin - rectangleWidth / 2,
							pageBuilder.PageSize.TopRight.Y - (float)convertScale(bottom) - topMargin - bottomMargin,
							(leftAux + rightAux) / 2 + leftMargin + rectangleWidth / 2,
							pageBuilder.PageSize.TopRight.Y - (float)convertScale(top) - topMargin - bottomMargin
							);
						break;
					case 2: // Right Alignment
						barcodeRectangle = new PdfRectangle(
							rightAux + leftMargin - rectangleWidth,
							pageBuilder.PageSize.TopRight.Y - (float)convertScale(bottom) - topMargin - bottomMargin,
							rightAux + leftMargin,
							pageBuilder.PageSize.TopRight.Y - (float)convertScale(top) - topMargin - bottomMargin
							);
						break;
					default: // Left Alignment (Corresponds to alignment = 0 but used as default)
						barcodeRectangle = new PdfRectangle(
							leftAux + leftMargin,
							pageBuilder.PageSize.TopRight.Y - (float)convertScale(bottom) - topMargin - bottomMargin,
							leftAux + leftMargin + rectangleWidth,
							pageBuilder.PageSize.TopRight.Y - (float)convertScale(top) - topMargin - bottomMargin
							);
						break;

				}
				Image barcodeImage = CreateBarcodeImage((float)barcodeRectangle.Width, (float)barcodeRectangle.Height, baseFontPath, sTxt);
				using (MemoryStream ms = new MemoryStream())
				{
					barcodeImage.Save(ms, ImageFormat.Png);
					pageBuilder.AddPng(ms.ToArray(), barcodeRectangle);
				}
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

				decimal width = (decimal)(((leftAux + rightAux) / 2 + leftMargin + rectangleWidth / 2) - ((leftAux + rightAux) / 2 + leftMargin - rectangleWidth / 2));
				decimal height = (decimal)((pageBuilder.PageSize.TopRight.Y - topAux - topMargin - bottomMargin) - (pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin));

				pageBuilder.SetTextAndFillColor(backColor.R, backColor.G, backColor.B);
				pageBuilder.DrawRectangle(rectangleStartingPoint, width, height, 1, true);
			}

			pageBuilder.SetTextAndFillColor(foreColor.R, foreColor.G, foreColor.B);

			if (fontUnderline)
			{
				float underlineSeparation = lineHeight / 5;
				int underlineHeight = (int)underlineSeparation + (int)(underlineSeparation / 4);

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

				pageBuilder.DrawLine(underlineStartingPoint, underlineEndingPoint, underlineHeight);
			}

			if (fontStrikethru)
			{
				float strikethruSeparation = (float)(lineHeight / 1.5);
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

			float textBlockWidth = rightAux - leftAux;
			float TxtWidth = MeasureTextWidth(sTxt, baseFontPath, fontSize);
			bool justified = (alignment == 3) && textBlockWidth < TxtWidth;
			bool wrap = ((align & 16) == 16);

			if (wrap || justified)
			{
				bottomAux = (float)convertScale(bottomOri);
				topAux = (float)convertScale(topOri);

				float llx = leftAux + leftMargin;
				float lly = (float)(pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin);
				float urx = rightAux + leftMargin;
				float ury = (float)(pageBuilder.PageSize.TopRight.Y - topAux - topMargin - bottomMargin);

				ShowWrappedTextAligned(font, alignment, sTxt, llx, lly, urx, ury);
			}
			else
			{
				if (!autoResize)
				{
					string newsTxt = sTxt;
					while (TxtWidth > textBlockWidth && (newsTxt.Length - 1 >= 0))
					{
						sTxt = newsTxt;
						newsTxt = newsTxt.Substring(0, newsTxt.Length - 1);
						TxtWidth = MeasureTextWidth(sTxt, baseFontPath, fontSize);
					}
				}
				switch (alignment)
				{
					case 1: // Center Alignment
						ShowTextAligned(font, alignment, sTxt, ((leftAux + rightAux) / 2) + leftMargin, (float)(pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin));
						break;
					case 2: // Right Alignment
						ShowTextAligned(font, alignment, sTxt, rightAux + leftMargin, (float)(pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin));
						break;
					case 0: // Left Alignment
					case 3: // Justified, only one text line
						ShowTextAligned(font, alignment, sTxt, leftAux + leftMargin, (float)(pageBuilder.PageSize.TopRight.Y - bottomAux - topMargin - bottomMargin));
						break;
				}
			}
		}

		public Image CreateBarcodeImage(float width, float height, string fontPath, string text)
		{
			PrivateFontCollection fontCollection = new PrivateFontCollection();
			fontCollection.AddFontFile(fontPath);
			FontFamily fontFamily = fontCollection.Families[0];

			int bitmapWidth = (int)Math.Ceiling(width);
			int bitmapHeight = (int)Math.Ceiling(height);

			float fontSize = Math.Min(width, height);
			Font font;
			SizeF textSize;

			using (Bitmap tempBitmap = new Bitmap(1, 1))
			{
				using (Graphics tempGraphics = Graphics.FromImage(tempBitmap))
				{
					do
					{
						font = new Font(fontFamily, fontSize, GraphicsUnit.Pixel);
						textSize = tempGraphics.MeasureString(text, font);
						fontSize--;
					} while (textSize.Width > width || textSize.Height > height);
				}
			}

			Bitmap bitmap = new Bitmap(bitmapWidth, bitmapHeight);
			bitmap.SetResolution(600, 600);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.Clear(Color.White);
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
				StringFormat format = new StringFormat
				{
					Alignment = StringAlignment.Center,
					LineAlignment = StringAlignment.Center
				};
				graphics.DrawString(text, font, Brushes.Black, new RectangleF(0, 0, width, height), format);
			}
			font.Dispose();
			fontCollection.Dispose();

			return bitmap;
		}

		private float MeasureTextWidth(string text, string fontPath, float fontSize)
		{
			Font font;
			if (string.IsNullOrEmpty(fontPath))
			{
				font = new Font("Times New Roman", fontSize, GraphicsUnit.Point);
			}
			else
			{
				PrivateFontCollection pfc = new PrivateFontCollection();
				pfc.AddFontFile(fontPath);
				font = new Font(pfc.Families[0], fontSize, GraphicsUnit.Point);
			}

			using (font)
			{
				using (var fakeImage = new Bitmap(1, 1))
				{
					using (var graphics = Graphics.FromImage(fakeImage))
					{
						graphics.PageUnit = GraphicsUnit.Point;
						graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
						var sizeF = graphics.MeasureString(text, font);
						return sizeF.Width;
					}
				}
			}
		}

		private float MeasureTextHeight(string text, string fontPath, float fontSize)
		{
			Font font;
			if (string.IsNullOrEmpty(fontPath))
			{
				font = new Font("Times New Roman", fontSize, GraphicsUnit.Point);
			}
			else
			{
				PrivateFontCollection pfc = new PrivateFontCollection();
				pfc.AddFontFile(fontPath);
				font = new Font(pfc.Families[0], fontSize, GraphicsUnit.Point);
			}

			using (font)
			{
				using (var fakeImage = new Bitmap(1, 1))
				{
					using (var graphics = Graphics.FromImage(fakeImage))
					{
						graphics.PageUnit = GraphicsUnit.Point;
						graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
						var sizeF = graphics.MeasureString(text, font);
						return sizeF.Height;
					}
				}
			}
		}

		private float CalculateFontCaptionHeight(string fontPath, float fontSize, FontStyle fontStyle = FontStyle.Regular)
		{
			Font font;
			if (string.IsNullOrEmpty(fontPath))
			{
				font = new Font("Times New Roman", fontSize, GraphicsUnit.Point);
			}
			else
			{
				PrivateFontCollection pfc = new PrivateFontCollection();
				pfc.AddFontFile(fontPath);
				font = new Font(pfc.Families[0], fontSize, GraphicsUnit.Point);
			}
			using (font)
			{
				FontFamily ff = font.FontFamily;

				float ascent = ff.GetCellAscent(fontStyle);
				float descent = ff.GetCellDescent(fontStyle);
				float lineSpacing = ff.GetLineSpacing(fontStyle);

				float height = fontSize * (ascent + descent) / ff.GetEmHeight(fontStyle);

				return height;
			}
		}

		private void ShowTextAligned(AddedFont font, int alignment, string text, float x, float y)
		{
			try
			{
				float textWidth = MeasureTextWidth(text, baseFontPath, fontSize);
				switch (alignment)
				{
					case 0: // Left-aligned
					case 3: // Justified, only one text line
						break;
					case 1: // Center-aligned
						x = x - textWidth / 2;
						break;
					case 2: // Right-aligned
						x = x - textWidth;
						break;
				}
				y = (float)(y - fontSize * 0.5);
				pageBuilder.AddText(text, fontSize, new PdfPoint(x, y), font);
			}
			catch (IOException ioe)
			{
				GXLogging.Error(log, "failed to draw aligned text: ", ioe);
			}
		}

		private void ShowWrappedTextAligned(AddedFont font, int alignment, string text, float llx, float lly, float urx, float ury)
		{
			try
			{
				List<string> lines = new List<string>();
				string[] words = text.Split(' ');
				StringBuilder currentLine = new StringBuilder();
				foreach (string word in words)
				{
					float currentLineWidth = MeasureTextWidth(currentLine + " " + word, baseFontPath, fontSize);
					if (currentLineWidth < urx - llx)
					{
						if (currentLine.Length > 0)
						{
							currentLine.Append(" ");
						}
						currentLine.Append(word);
					}
					else
					{
						lines.Add(currentLine.ToString());
						currentLine.Clear();
						currentLine.Append(word);
					}
				}
				lines.Add(currentLine.ToString());

				float leading = (float)(lines.Count == 1 ? fontSize : 1.2 * fontSize);
				float totalTextHeight = fontSize * lines.Count + leading * (lines.Count - 1);
				float startY = lines.Count == 1 ? lly + (ury - lly - totalTextHeight) / 2 : lly + (ury - lly - totalTextHeight) / 2 + (lines.Count - 1) * (fontSize + leading);

				foreach (string line in lines)
				{
					float lineWidth = MeasureTextWidth(line, baseFontPath, fontSize);
					float startX;

					switch (alignment)
					{
						case 1: // Center-aligned
							startX = llx + (urx - llx - lineWidth) / 2;
							break;
						case 2: // Right-aligned
							startX = urx - lineWidth;
							break;
						default: // Left-aligned & Justified, only one text line
							startX = llx;
							break;
					}

					pageBuilder.AddText(line, fontSize, new PdfPoint(startX, startY), font);
					startY -= leading;
				}
			}
			catch (IOException ioe)
			{
				GXLogging.Error(log, "Failed to draw wrapped text", ioe);
			}
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

			GXLogging.Info(log, "The PDFPig API provides no way of setting the number of copies and the duplex value for the viewer preferences");

			GXLogging.Info(log, "The PDFPig API provides no way of embedding javascript into the document");

			try
			{
				byte[] documentBytes = documentBuilder.Build();
				outputStream.Write(documentBytes, 0, documentBytes.Length);
			}
			catch (IOException e)
			{
				GXLogging.Debug(log,"GxEndDocument: failed to write document to the output stream", e);
			}

			string serverPrinting = props.getGeneralProperty(Const.SERVER_PRINTING);
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
					catch (IOException) { }
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
			PageSize = PageSize.Custom;
		}
		internal ExtendedPageSize(PageSize pageSize)
		{
			PageSize = pageSize;
		}
		internal bool IsCustomPageSize()
		{
			return PageSize == PageSize.Custom;
		}
	}
}
