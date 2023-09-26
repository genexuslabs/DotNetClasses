using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using GeneXus;
using iText.Barcodes;
using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using iText.IO.Font;
using iText.IO.Font.Otf;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Exceptions;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Font;
using iText.Layout.Layout;
using iText.Layout.Properties;
using iText.Layout.Splitting;
using log4net;
using Path = System.IO.Path;

namespace GeneXus.Printer
{

	public class GxReportBuilderPdf8 : GxReportBuilderPdf
	{
		static ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public GxReportBuilderPdf8() { }
		public GxReportBuilderPdf8(string appPath, Stream outputStream)
		{

			_pdfReport = new com.genexus.reports.PDFReportItext8(appPath);
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

	public class PDFReportItext8 : PDFReportItextBase
	{

		static ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		//const int ASCENT_NORMALIZED_UNITS = 1000;

		private PageSize pageSize;
		private PdfFont baseFont;
		private Style fontStyle;
		//Color for, BaseColor for => Itext5
		private Color backColor, foreColor, templateColorFill;
		private Document document;
		PdfDocument pdfDocument;
		private PdfPage pdfPage;
		private PdfWriter writer;
		private Rectangle templateRectangle;
		private float templatex, templatey;
		private PdfFont templateFont;
		private PdfFont defaultFont;
		internal Dictionary<string, Image> documentImages;
		Barcode128 barcode;
		private Boolean fontBold;
		private Boolean fontItalic;

		public PDFReportItext8(String appPath) : base(appPath)
		{
			documentImages = new Dictionary<string, Image>();
		}

		protected override void init(ref int gxYPage, ref int gxXPage, int pageWidth, int pageLength)
		{
			this.pageSize = ComputePageSize(leftMargin, topMargin, pageWidth, pageLength, props.getBooleanGeneralProperty(Const.MARGINS_INSIDE_BORDER, Const.DEFAULT_MARGINS_INSIDE_BORDER));
			gxXPage = (int)this.pageSize.GetRight();
			if (props.getBooleanGeneralProperty(Const.FIX_SAC24437, true))
				gxYPage = (int)(pageLength / GX_PAGE_SCALE_Y);
			else
				gxYPage = (int)(pageLength / GX_PAGE_SCALE_Y_OLD);

			writer = new PdfWriter(outputStream);
			writer.SetCompressionLevel(CompressionConstants.BEST_COMPRESSION);
			try
			{
				string level = props.getGeneralProperty(Const.COMPLIANCE_LEVEL);
				if (Enum.TryParse(level, true, out complianceLevel))
				{
					//if (SetComplainceLevel(complianceLevel))
						//writer.SetTagged();
				}
				pdfDocument = new PdfDocument(writer);
				pdfDocument.SetDefaultPageSize(this.pageSize);
				document = new Document(pdfDocument);
				document.SetFontProvider(new DefaultFontProvider());
			}
			catch (PdfException de)
			{
				GXLogging.Debug(log, "GxDrawRect error", de);
			}
		}

		internal override bool SetComplainceLevel(PdfConformanceLevel level)
		{
			/*switch (level)
			{
				case PdfConformanceLevel.Pdf_A1A:
					writer.PDFXConformance = PdfWriter.PDFA1A;
					return true;
				case PdfConformanceLevel.Pdf_A1B:
					writer.PDFXConformance = PdfWriter.PDFA1B;
					return true;
				default:
					return false;
			}*/
			return false;
	}

	/**
	* @param hideCorners indicates whether corner triangles should be hidden when the side that joins them is hidden.
	*/

		private void drawRectangle(PdfCanvas cb, float x, float y, float w, float h, int styleTop, int styleBottom, int styleRight, int styleLeft,
								float radioTL, float radioTR, float radioBL, float radioBR, float penAux, bool hideCorners)
		{

			float[] dashPatternTop = getDashedPattern(styleTop);
			float[] dashPatternBottom = getDashedPattern(styleBottom);
			float[] dashPatternLeft = getDashedPattern(styleLeft);
			float[] dashPatternRight = getDashedPattern(styleRight);
			
			//-------------------bottom line---------------------
			if (styleBottom != STYLE_NONE_CONST)
			{
				cb.SetLineDash(dashPatternBottom, 0);
			}

			float b = 0.4477f;
			if (radioBL > 0)
			{
				cb.MoveTo(x + radioBL, y);
			}
			else
			{
				if (hideCorners && styleLeft == STYLE_NONE_CONST && radioBL == 0)
				{
					cb.MoveTo(x + penAux, y);
				}
				else
				{
					cb.MoveTo(x, y);
				}
			}

			//-------------------bottom right corner---------------------

			if (styleBottom != STYLE_NONE_CONST)
			{
				if (hideCorners && styleRight == STYLE_NONE_CONST && radioBR == 0)
				{
					cb.LineTo(x + w - penAux, y);
				}
				else
				{
					cb.LineTo(x + w - radioBR, y);
				}
				if (radioBR > 0 && styleRight != STYLE_NONE_CONST)
				{
					cb.CurveTo(x + w - radioBR * b, y, x + w, y + radioBR * b, x + w, y + radioBR);
				}
			}

			//-------------------right line---------------------

			if (styleRight != STYLE_NONE_CONST && dashPatternRight != dashPatternBottom)
			{
				cb.Stroke();
				cb.SetLineDash(dashPatternRight, 0);
				if (hideCorners && styleBottom == STYLE_NONE_CONST && radioBR == 0)
				{
					cb.MoveTo(x + w, y + penAux);
				}
				else
				{
					cb.MoveTo(x + w, y + radioBR);
				}
			}

			//-------------------top right corner---------------------
			if (styleRight != STYLE_NONE_CONST)
			{
				if (hideCorners && styleTop == STYLE_NONE_CONST && radioTR == 0)
				{
					cb.LineTo(x + w, y + h - penAux);
				}
				else
				{
					cb.LineTo(x + w, y + h - radioTR);
				}
				if (radioTR > 0 && styleTop != STYLE_NONE_CONST)
				{
					cb.CurveTo(x + w, y + h - radioTR * b, x + w - radioTR * b, y + h, x + w - radioTR, y + h);
				}
			}

			//-------------------top line---------------------

			if (styleTop != STYLE_NONE_CONST && dashPatternTop != dashPatternRight)
			{
				cb.Stroke();
				cb.SetLineDash(dashPatternTop, 0);
				if (hideCorners && styleRight == STYLE_NONE_CONST && radioTR == 0)
				{
					cb.MoveTo(x + w - penAux, y + h);
				}
				else
				{
					cb.MoveTo(x + w - radioTR, y + h);
				}
			}

			//-------------------top left corner---------------------
			if (styleTop != STYLE_NONE_CONST)
			{
				if (hideCorners && styleLeft == STYLE_NONE_CONST && radioTL == 0)
				{
					cb.LineTo(x + penAux, y + h);
				}
				else
				{
					cb.LineTo(x + radioTL, y + h);
				}
				if (radioTL > 0 && styleLeft != STYLE_NONE_CONST)
				{
					cb.CurveTo(x + radioTL * b, y + h, x, y + h - radioTL * b, x, y + h - radioTL);
				}
			}

			//-------------------left line---------------------

			if (styleLeft != STYLE_NONE_CONST && dashPatternLeft != dashPatternTop)
			{
				cb.Stroke();
				cb.SetLineDash(dashPatternLeft, 0);
				if (hideCorners && styleTop == STYLE_NONE_CONST && radioTL == 0)
				{
					cb.MoveTo(x, y + h - penAux);
				}
				else
				{
					cb.MoveTo(x, y + h - radioTL);
				}
			}

			//-------------------bottom left corner---------------------
			if (styleLeft != STYLE_NONE_CONST)
			{
				if (hideCorners && styleBottom == STYLE_NONE_CONST && radioBL == 0)
				{
					cb.LineTo(x, y + penAux);
				}
				else
				{
					cb.LineTo(x, y + radioBL);
				}
				if (radioBL > 0 && styleBottom != STYLE_NONE_CONST)
				{
					cb.CurveTo(x, y + radioBL * b, x + radioBL * b, y, x + radioBL, y);
				}
			}
			cb.Stroke();

		}

		private void roundRectangle(PdfCanvas cb, float x, float y, float w, float h, float radioTL, float radioTR, float radioBL, float radioBR)
		{
			//-------------------bottom line---------------------


			float b = 0.4477f;
			if (radioBL > 0)
			{
				cb.MoveTo(x + radioBL, y);
			}
			else
			{
				cb.MoveTo(x, y);
			}

			//-------------------bottom right corner---------------------

			cb.LineTo(x + w - radioBR, y);
			if (radioBR > 0)
			{
				cb.CurveTo(x + w - radioBR * b, y, x + w, y + radioBR * b, x + w, y + radioBR);
			}


			cb.LineTo(x + w, y + h - radioTR);
			if (radioTR > 0)
			{
				cb.CurveTo(x + w, y + h - radioTR * b, x + w - radioTR * b, y + h, x + w - radioTR, y + h);
			}

			cb.LineTo(x + radioTL, y + h);
			if (radioTL > 0)
			{
				cb.CurveTo(x + radioTL * b, y + h, x, y + h - radioTL * b, x, y + h - radioTL);
			}
			cb.LineTo(x, y + radioBL);
			if (radioBL > 0)
			{
				cb.CurveTo(x, y + radioBL * b, x + radioBL * b, y, x + radioBL, y);
			}
		}

		public override void GxDrawRect(int left, int top, int right, int bottom, int pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue,
											int styleTop, int styleBottom, int styleRight, int styleLeft, int cornerRadioTL, int cornerRadioTR, int cornerRadioBL, int cornerRadioBR)
		{
			Color rectBackColor = new DeviceRgb(backRed, backGreen, backBlue);
			Color rectForeColor = new DeviceRgb(foreRed, foreGreen, foreBlue);
			GxDrawRect(left, top, right, bottom, pen, rectForeColor, backMode, rectBackColor, styleTop, styleBottom, styleRight, styleLeft, cornerRadioTL, cornerRadioTR, cornerRadioBL, cornerRadioBR);
		}

		void GxDrawRect(int left, int top, int right, int bottom, int pen, Color foreColor, int backMode, Color backColor,
			int styleTop, int styleBottom, int styleRight, int styleLeft, int cornerRadioTL, int cornerRadioTR, int cornerRadioBL, int cornerRadioBR)
		{
			PdfCanvas cb = new PdfCanvas(pdfPage);

			float penAux = (float)convertScale(pen);
			float rightAux = (float)convertScale(right);
			float bottomAux = (float)convertScale(bottom);
			float leftAux = (float)convertScale(left);
			float topAux = (float)convertScale(top);

			GXLogging.Debug(log, "GxDrawRect -> (" + left + "," + top + ") - (" + right + "," + bottom + ")  BackMode: " + backMode + " Pen:" + pen + ",leftMargin:" + leftMargin);
			cb.SaveState();

			float x1, y1, x2, y2;
			x1 = leftAux + leftMargin;
			y1 = pageSize.GetTop() - bottomAux - topMargin - bottomMargin;
			x2 = rightAux + leftMargin;
			y2 = pageSize.GetTop() - topAux - topMargin - bottomMargin;

			cb.SetLineWidth(penAux);
			cb.SetLineCapStyle(PdfCanvasConstants.LineCapStyle.PROJECTING_SQUARE);

			if (cornerRadioBL == 0 && cornerRadioBR == 0 && cornerRadioTL == 0 && cornerRadioTR == 0 && styleBottom == 0 && styleLeft == 0 && styleRight == 0 && styleTop == 0)
			{
				//border color must be the same as the fill if border=0 since setLineWidth does not work.
				if (pen > 0)
					cb.SetStrokeColor(foreColor);
				else
					cb.SetStrokeColor(backColor);
				cb.Rectangle(x1, y1, x2 - x1, y2 - y1);

				if (backMode != 0)
				{
					cb.SetFillColor(backColor);
					cb.FillStroke();
				}

				cb.ClosePathStroke();
			}
			else
			{
				float w = x2 - x1;
				float h = y2 - y1;
				if (w < 0)
				{
					x1 += w;
					w = -w;
				}
				if (h < 0)
				{
					y1 += h;
					h = -h;
				}

				float cRadioTL = (float)convertScale(cornerRadioTL);
				float cRadioTR = (float)convertScale(cornerRadioTR);
				float cRadioBL = (float)convertScale(cornerRadioBL);
				float cRadioBR = (float)convertScale(cornerRadioBR);

				// Scale the radius if it's too large or to small to fit.
				int max = (int)Math.Min(w, h);
				cRadioTL = Math.Max(0, Math.Min(cRadioTL, max / 2));
				cRadioTR = Math.Max(0, Math.Min(cRadioTR, max / 2));
				cRadioBL = Math.Max(0, Math.Min(cRadioBL, max / 2));
				cRadioBR = Math.Max(0, Math.Min(cRadioBR, max / 2));

				if (backMode != 0)
				{
					cb.SetStrokeColor(backColor);
					cb.SetLineWidth(0);
					roundRectangle(cb, x1, y1, w, h, cRadioTL, cRadioTR, cRadioBL, cRadioBR);
					cb.SetFillColor(backColor);
					cb.FillStroke();
					cb.SetLineWidth(penAux);
				}
				if (pen > 0)
				{
					//Rectangle edge
					cb.SetFillColor(backColor);
					cb.SetStrokeColor(foreColor);
					drawRectangle(cb, x1, y1, w, h,
						styleTop, styleBottom, styleRight, styleLeft,
						cRadioTL, cRadioTR,
						cRadioBL, cRadioBR, penAux, false);
				}
			}
			cb.RestoreState();
		}
		public override void GxDrawLine(int left, int top, int right, int bottom, int width, int foreRed, int foreGreen, int foreBlue, int style)
		{
			PdfCanvas cb = new PdfCanvas(pdfPage);

			Color foreColor = new DeviceRgb(foreRed, foreGreen, foreBlue);

			float widthAux = (float)convertScale(width);
			float rightAux = (float)convertScale(right);
			float bottomAux = (float)convertScale(bottom);
			float leftAux = (float)convertScale(left);
			float topAux = (float)convertScale(top);

			GXLogging.Debug(log, "GxDrawLine leftAux: " + leftAux + ",leftMargin:" + leftMargin + ",pageSize.Top:" + pageSize.GetTop() + ",bottomAux:" + bottomAux + ",topMargin:" + topMargin + ",bottomMargin:" + bottomMargin);

			GXLogging.Debug(log, "GxDrawLine -> (" + left + "," + top + ") - (" + right + "," + bottom + ")  Width: " + width);
			float x1, y1, x2, y2;
			x1 = leftAux + leftMargin;
			y1 = pageSize.GetTop() - bottomAux - topMargin - bottomMargin;
			x2 = rightAux + leftMargin;
			y2 = pageSize.GetTop() - topAux - topMargin - bottomMargin;

			GXLogging.Debug(log, "Line-> (" + (x1) + "," + y1 + ") - (" + x2 + "," + y2 + ") ");
			cb.SaveState();
			cb.SetStrokeColor(foreColor);
			cb.SetLineWidth(widthAux);

			if (lineCapProjectingSquare)
			{
				cb.SetLineCapStyle(PdfCanvasConstants.LineCapStyle.PROJECTING_SQUARE);
			}
			if (style != 0)
			{
				float[] dashPattern = getDashedPattern(style);
				cb.SetLineDash(dashPattern, 0);
			}

			cb.MoveTo(x1, y1);
			cb.LineTo(x2, y2);
			cb.Stroke();

			cb.RestoreState();
		}

		public override void GxDrawBitMap(String bitmap, int left, int top, int right, int bottom, int aspectRatio)
		{
			try
			{
				Image image;
				Image imageRef;
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

							image = new Image(ImageDataFactory.Create(defaultRelativePrepend + bitmap));
							if (image == null)
							{
								bitmap = webAppDir + bitmap;
								image = new Image(ImageDataFactory.Create(bitmap));
							}
							else
							{
								bitmap = defaultRelativePrepend + bitmap;
							}
						}
						else
						{
							image = new Image(ImageDataFactory.Create(bitmap));
						}
					}
					catch (Exception)//absolute url
					{
						Uri uri = new Uri(bitmap);
						image = new Image(ImageDataFactory.Create(uri));
					}
					if (documentImages == null)
					{
						documentImages = new Dictionary<string, Image>();
					}
					documentImages[bitmap] = image;
				}
				GXLogging.Debug(log, "GxDrawBitMap ->  '" + bitmap + "' [" + left + "," + top + "] - Size: (" + (right - left) + "," + (bottom - top) + ")");

				if (image != null)
				{
					float rightAux = (float)convertScale(right);
					float bottomAux = (float)convertScale(bottom);
					float leftAux = (float)convertScale(left);
					float topAux = (float)convertScale(top);
					image.SetFixedPosition(this.getPage(),leftAux + leftMargin, this.pageSize.GetTop() - bottomAux - topMargin - bottomMargin);
					if (aspectRatio == 0)
						image.ScaleAbsolute(rightAux - leftAux, bottomAux - topAux);
					else
						image.ScaleToFit(rightAux - leftAux, bottomAux - topAux);
					image.GetAccessibilityProperties().SetAlternateDescription(Path.GetFileName(bitmap));
					document.Add(image);
				}
			}
			catch (PdfException de)
			{
				GXLogging.Error(log, "GxDrawBitMap document error", de);
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
			fontStyle = null;
			bool _isCJK = false;
			bool embeedFont = IsEmbeddedFont(fontName);
			if (!embeedFont)
			{
				fontName = getSubstitute(fontName);
			}

			GXLogging.Debug(log, "GxAttris: ");
			GXLogging.Debug(log, "\\-> Font: " + fontName + " (" + fontSize + ")" + (fontBold ? " BOLD" : "") + (fontItalic ? " ITALIC" : "") + (fontStrikethru ? " Strike" : ""));
			GXLogging.Debug(log, "\\-> Fore (" + foreRed + ", " + foreGreen + ", " + foreBlue + ")");
			GXLogging.Debug(log, "\\-> Back (" + backRed + ", " + backGreen + ", " + backBlue + ")");

			if (barcode128AsImage && fontName.ToLower().IndexOf("barcode 128") >= 0 || fontName.ToLower().IndexOf("barcode128") >= 0)
			{
				barcode = new Barcode128(pdfDocument);
				barcode.SetCodeType(Barcode128.CODE128);
			}
			else
			{
				barcode = null;
			}
			this.fontUnderline = fontUnderline;
			this.fontStrikethru = fontStrikethru;
			this.fontSize = fontSize;
			this.fontBold = fontBold;
			this.fontItalic = fontItalic;
			foreColor = new DeviceRgb(foreRed, foreGreen, foreBlue);
			backColor = new DeviceRgb(backRed, backGreen, backBlue);

			backFill = (backMode != 0);
			try
			{
				//LoadAsianFontsDll();
				string f = fontName.ToLower();
				if (PDFFont.isType1(fontName))
				{
					//Asian font
					for (int i = 0; i < Type1FontMetrics.CJKNames.Length; i++)
					{
						if (Type1FontMetrics.CJKNames[i][0].ToLower().Equals(f) ||
							Type1FontMetrics.CJKNames[i][1].ToLower().Equals(f))
						{
							fontStyle = new Style();
							if (fontItalic) fontStyle.SetItalic();
							if (fontBold) fontStyle.SetBold();

							setAsianFont(fontName, string.Empty);
							fontStyle.SetFont(baseFont);

							_isCJK = true;
							break;
						}
					}
					if (!_isCJK)
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
						baseFont = PdfFontFactory.CreateFont(fontName, PdfEncodings.WINANSI, PdfFontFactory.EmbeddingStrategy.PREFER_NOT_EMBEDDED);
					}
				}
				else
				{//True type font

					if (IsEmbeddedFont(fontName))
					{
						fontStyle = new Style();
						if (fontItalic) fontStyle.SetItalic();
						if (fontBold) fontStyle.SetBold();
					}
					string fontPath = GetFontLocation(fontName);
					bool foundFont = true;
					if (string.IsNullOrEmpty(fontPath))
					{
						MSPDFFontDescriptor fontDescriptor = new MSPDFFontDescriptor();
						fontPath = fontDescriptor.getTrueTypeFontLocation(fontName);
						if (string.IsNullOrEmpty(fontPath))
						{
							baseFont = CreateDefaultFont();
							foundFont = false;
						}
						else
						{
							props.setProperty(Const.MS_FONT_LOCATION, fontName, fontPath);
						}
					}
					if (foundFont)
					{
						if (IsEmbeddedFont(fontName))
						{
							try
							{
								baseFont = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
								GXLogging.Debug(log, "EMBEED_SECTION Font");
							}
							catch (IOException ioEx)
							{
								Exception exDetailed = new Exception($"Error creating {fontPath}. Check font is installed for the current user", ioEx);
								throw exDetailed;
							}
						}
						else
						{

							fontStyle = new Style();
							if (fontItalic) fontStyle.SetItalic();
							if (fontBold) fontStyle.SetBold();

							GXLogging.Debug(log, "NOT EMBEED_SECTION Font");

							baseFont = PdfFontFactory.CreateFont(fontPath, PdfEncodings.WINANSI, PdfFontFactory.EmbeddingStrategy.PREFER_NOT_EMBEDDED);
							fontStyle.SetFont(baseFont);
						}
					}
					else
					{
						GXLogging.Debug(log, "NOT foundFont fontName:" + fontName);
					}
				}
			}
			catch (PdfException de)
			{
				GXLogging.Debug(log, "GxAttris DocumentException", de);
				throw de;
			}
			catch (Exception e)
			{
				GXLogging.Debug(log, "GxAttris error", e);
				baseFont = CreateDefaultFont();
			}
		}

		private PdfFont CreateDefaultFont()
		{
			if (defaultFont == null)
			{
				if (IsPdfA())
					defaultFont = PdfFontFactory.CreateFont("Helvetica", PdfEncodings.CP1252, PdfFontFactory.EmbeddingStrategy.PREFER_NOT_EMBEDDED); 
				else
					defaultFont = PdfFontFactory.CreateFont("Helvetica", PdfEncodings.WINANSI, PdfFontFactory.EmbeddingStrategy.PREFER_NOT_EMBEDDED);
			}
			return PdfFontFactory.CreateFont("Helvetica", PdfEncodings.WINANSI, PdfFontFactory.EmbeddingStrategy.PREFER_NOT_EMBEDDED);
		}

		public override void setAsianFont(String fontName, String style)
		{
			//LoadAsianFontsDll();
			try
			{
					if (fontName.Equals("Japanese"))
						baseFont = PdfFontFactory.CreateFont("HeiseiMin-W3", "UniJIS-UCS2-H", PdfFontFactory.EmbeddingStrategy.PREFER_NOT_EMBEDDED);
					if (fontName.Equals("Japanese2"))
						baseFont = PdfFontFactory.CreateFont("HeiseiKakuGo-W5", "UniJIS-UCS2-H", PdfFontFactory.EmbeddingStrategy.PREFER_NOT_EMBEDDED);
					if (fontName.Equals("SimplifiedChinese"))
						baseFont = PdfFontFactory.CreateFont("STSong-Light", "UniGB-UCS2-H", PdfFontFactory.EmbeddingStrategy.PREFER_NOT_EMBEDDED);
					if (fontName.Equals("TraditionalChinese"))
						baseFont = PdfFontFactory.CreateFont("MHei-Medium", "UniCNS-UCS2-H", PdfFontFactory.EmbeddingStrategy.PREFER_NOT_EMBEDDED);
					if (fontName.Equals("Korean"))
						baseFont = PdfFontFactory.CreateFont("HYSMyeongJo-Medium", "UniKS-UCS2-H", PdfFontFactory.EmbeddingStrategy.PREFER_NOT_EMBEDDED);
			}
			catch (PdfException de)
			{
				GXLogging.Debug(log, "setAsianFont  error", de);
			}
			catch (IOException ioe)
			{
				GXLogging.Debug(log, "setAsianFont io error", ioe);
			}
		}

		public override void GxDrawText(String sTxt, int left, int top, int right, int bottom, int align, int htmlformat, int border, int valign)
		{
			GXLogging.Debug(log, "GxDrawText, text:" + sTxt);

			bool printRectangle = false;
			if (props.getBooleanGeneralProperty(Const.BACK_FILL_IN_CONTROLS, true))
				printRectangle = true;

			if (printRectangle && (border == 1 || backFill))
				GxDrawRect(left, top, right, bottom, border, foreColor, backFill ? 1 : 0, backColor, 0, 0, 0, 0, 0, 0, 0, 0);

			PdfCanvas canvas = new PdfCanvas(pdfPage);
			sTxt = sTxt.TrimEnd(TRIM_CHARS);

			PdfFont font = baseFont;
			canvas.SetFontAndSize(this.baseFont, fontSize);
			canvas.SetFillColor(foreColor);
			float captionHeight = baseFont.GetAscent(sTxt, fontSize) / 1000;
			float rectangleWidth = baseFont.GetWidth(sTxt, fontSize);
			float lineHeight = (1 / 1000) * (baseFont.GetAscent(sTxt, fontSize) - baseFont.GetDescent(sTxt, fontSize)) + (fontSize * 1.2f);
			float textBlockHeight = (float)convertScale(bottom - top);
			int linesCount = (int)(textBlockHeight / lineHeight);
			int bottomOri = bottom;
			int topOri = top;

			if (linesCount >= 2 && !((align & 16) == 16) && htmlformat != 1)
				if (valign == (int)VerticalAlign.TOP)
					bottom = top + (int)reconvertScale(lineHeight);
				else if (valign == (int)VerticalAlign.BOTTOM)
					top = bottom - (int)reconvertScale(lineHeight);

			float bottomAux = (float)convertScale(bottom) - ((float)convertScale(bottom - top) - captionHeight) / 2;
			float topAux = (float)convertScale(top) + ((float)convertScale(bottom - top) - captionHeight) / 2; ;

			float leftAux = (float)convertScale(left);
			float rightAux = (float)convertScale(right);
			int alignment = align & 3;
			bool autoResize = (align & 256) == 256;

			GXLogging.Debug(log, "GxDrawText left: " + left + ",top:" + top + ",right:" + right + ",bottom:" + bottom + ",captionHeight:" + captionHeight + ",fontSize:" + fontSize);
			GXLogging.Debug(log, "GxDrawText leftAux: " + leftAux + ",leftMargin:" + leftMargin + ",pageSize.Top:" + pageSize.GetTop() + ",bottomAux:" + bottomAux + ",topMargin:" + topMargin + ",bottomMargin:" + bottomMargin);
			if (htmlformat == 1)
			{
				try
				{
					ConverterProperties converterProperties = new ConverterProperties();
					FontProvider fontProvider = document.GetFontProvider();
					if (IsTrueType(baseFont))
					{
						Hashtable locations = GetFontLocations();
						foreach (string fontName in locations.Keys)
						{
							string fontPath = (string)locations[fontName];
							if (string.IsNullOrEmpty(fontPath))
							{
								MSPDFFontDescriptor fontDescriptor = new MSPDFFontDescriptor();
								fontPath = fontDescriptor.getTrueTypeFontLocation(fontName);
							}
							if (!string.IsNullOrEmpty(fontPath))
							{

								fontProvider.AddFont(fontPath);
							}
						}
					}
					converterProperties.SetFontProvider(fontProvider);
					bottomAux = (float)convertScale(bottom);
					topAux = (float)convertScale(top);
					float drawingPageHeight = this.pageSize.GetTop() - topMargin - bottomMargin;

					float llx = leftAux + leftMargin;
					float lly = drawingPageHeight - bottomAux;
					float urx = rightAux + leftMargin;
					float ury = drawingPageHeight - topAux;

					Rectangle htmlRectangle = new Rectangle(llx, lly, urx - llx, ury - lly);
					YPosition yPosition = new YPosition(htmlRectangle.GetTop());

					PdfCanvas htmlPdfCanvas = new PdfCanvas(pdfPage);
					Canvas htmlCanvas = new Canvas(canvas, htmlRectangle);


					//Iterate over the elements (a.k.a the parsed HTML string) and handle each case accordingly
					IList<IElement> elements = HtmlConverter.ConvertToElements(sTxt, converterProperties);
					foreach (IElement element in elements)
						ProcessHTMLElement(htmlRectangle, yPosition, (IBlockElement)element);
				}
				catch (Exception ex1)
				{
					GXLogging.Debug(log, "Error adding html: ", ex1);
				}

			}
			else if (barcode != null)
			{
				GXLogging.Debug(log, "Barcode" + barcode.GetType().ToString());
				try
				{
					barcode.SetCode(sTxt);
					barcode.SetTextAlignment(alignment);
					Rectangle rectangle = new Rectangle(0, 0);

					switch (alignment)
					{
						case 1: // Center Alignment
							rectangle = rectangle.SetBbox((leftAux + rightAux) / 2 + leftMargin - rectangleWidth / 2,
								(float)this.pageSize.GetTop() - (float)convertScale(bottom) - topMargin - bottomMargin,
								(leftAux + rightAux) / 2 + leftMargin + rectangleWidth / 2,
								(float)this.pageSize.GetTop() - (float)convertScale(top) - topMargin - bottomMargin);
							break;
						case 2: // Right Alignment
							rectangle = rectangle.SetBbox(rightAux + leftMargin - rectangleWidth,
								(float)this.pageSize.GetTop() - (float)convertScale(bottom) - topMargin - bottomMargin,
								rightAux + leftMargin,
								(float)this.pageSize.GetTop() - (float)convertScale(top) - topMargin - bottomMargin);
							break;
						case 0: // Left Alignment
							rectangle = rectangle.SetBbox(leftAux + leftMargin,
								(float)this.pageSize.GetTop() - (float)convertScale(bottom) - topMargin - bottomMargin,
								leftAux + leftMargin + rectangleWidth,
								(float)this.pageSize.GetTop() - (float)convertScale(top) - topMargin - bottomMargin);
							break;
					}
					barcode.SetAltText(string.Empty);
					barcode.SetBaseline(0);

					if (fontSize < Const.LARGE_FONT_SIZE)
						barcode.SetX(Const.OPTIMAL_MINIMU_BAR_WIDTH_SMALL_FONT);
					else
						barcode.SetX(Const.OPTIMAL_MINIMU_BAR_WIDTH_LARGE_FONT);


					Image imageCode = new Image(barcode.CreateFormXObject(backFill ? backColor : null, foreColor, pdfDocument));
					imageCode.SetFixedPosition(leftAux + leftMargin, rectangle.GetBottom());
					barcode.SetBarHeight(rectangle.GetHeight());
					imageCode.ScaleToFit(rectangle.GetWidth(), rectangle.GetHeight());
					document.Add(imageCode);
				}
				catch (Exception ex)
				{
					GXLogging.Error(log, "Error generating Barcode " + barcode.GetType().ToString(), ex);
				}
			}
			else
			{

				if (sTxt.Trim().ToLower().Equals("{{pages}}"))
				{
					GXLogging.Debug(log, "GxDrawText addTemplate-> (" + (leftAux + leftMargin) + "," + (this.pageSize.GetTop() - bottomAux - topMargin - bottomMargin) + ") ");
					templateRectangle = new Rectangle(leftAux + leftMargin, (float)this.pageSize.GetTop() - bottomAux - topMargin - bottomMargin, rightAux + leftMargin, (float)this.pageSize.GetTop() - topAux - topMargin - bottomMargin);

					templatex = leftAux + leftMargin;
					templatey = this.pageSize.GetTop() - bottomAux - topMargin - bottomMargin;
					templateFont = this.baseFont;
					templateFontSize = fontSize;
					templateColorFill = foreColor;
					templateAlignment = alignment;
					templateCreated = true;
				}

				float textBlockWidth = rightAux - leftAux;
				float TxtWidth = baseFont.GetWidth(sTxt, fontSize);
				Boolean justified = (alignment == 3) && textBlockWidth < TxtWidth;
				Boolean wrap = ((align & 16) == 16);

				float leading = (float)Convert.ToDouble(props.getGeneralProperty(Const.LEADING), CultureInfo.InvariantCulture.NumberFormat);
				Style style = new Style();
				if (fontBold) style.SetBold();
				if (fontItalic) style.SetItalic();
				if (fontStrikethru) style.SetUnderline(fontSize / 6, fontSize / 2);
				if (fontUnderline) style.SetUnderline(fontSize / 6, 0);
				style.SetFont(font);
				style.SetFontSize(fontSize);
				style.SetFontColor(foreColor);

				if (wrap || justified)
				{
					bottomAux = (float)convertScale(bottomOri);
					topAux = (float)convertScale(topOri);

					float llx = leftAux + leftMargin;
					float lly = this.pageSize.GetTop() - bottomAux - topMargin - bottomMargin;
					float urx = rightAux + leftMargin;
					float ury = this.pageSize.GetTop() - topAux - topMargin - bottomMargin;

					DrawTextColumn(llx, lly, urx, ury, leading, sTxt, valign, alignment, style, wrap);
				}
				else
				{
					try
					{
						if (!autoResize)
						{
							String newsTxt = sTxt;
							while (TxtWidth > textBlockWidth && (newsTxt.Length - 1 >= 0))
							{
								sTxt = newsTxt;
								newsTxt = newsTxt.Substring(0, newsTxt.Length - 1);
								TxtWidth = this.baseFont.GetWidth(newsTxt, fontSize);
							}
						}

						Paragraph p = new Paragraph(sTxt);
						p.AddStyle(style);

						switch (alignment)
						{
							case 1: // Center Alignment
								document.ShowTextAligned(p, ((leftAux + rightAux) / 2) + leftMargin, this.pageSize.GetTop() - bottomAux - topMargin - bottomMargin, this.getPage(), TextAlignment.CENTER, VerticalAlignment.MIDDLE, 0);
								break;
							case 2: // Right Alignment
								document.ShowTextAligned(p, rightAux + leftMargin, this.pageSize.GetTop() - bottomAux - topMargin - bottomMargin, this.getPage(), TextAlignment.RIGHT, VerticalAlignment.MIDDLE, 0);
								break;
							case 0: // Left Alignment
							case 3: // Justified, only one text line
								document.ShowTextAligned(p, leftAux + leftMargin, this.pageSize.GetTop() - bottomAux - topMargin - bottomMargin, this.getPage(), TextAlignment.LEFT, VerticalAlignment.MIDDLE, 0);
								break;
						}
					}
					catch (Exception e)
					{
						GXLogging.Error(log, "GxDrawText failed to draw simple text: ", e);
					}
				}
			}
		}

		private void ProcessHTMLElement(Rectangle htmlRectangle, YPosition currentYPosition, IBlockElement blockElement)
		{
			Div div = blockElement as Div;
			if (div != null)
			{
				// Iterate through the children of the Div and process each child element recursively
				foreach (IElement child in div.GetChildren())
					if (child is IBlockElement)
						ProcessHTMLElement(htmlRectangle, currentYPosition, (IBlockElement)child);

			}

			float blockElementHeight = GetBlockElementHeight(blockElement, htmlRectangle);
			float availableSpace = currentYPosition.CurrentYPosition - htmlRectangle.GetBottom();
			if (blockElementHeight > availableSpace)
			{
				GXLogging.Error(log, "You are trying to render an element of height " + blockElementHeight + " in a space of height " + availableSpace);
				return;
			}

			if (blockElement is Paragraph p)
			{
				p.SetFixedPosition(this.getPage(), htmlRectangle.GetX(), currentYPosition.CurrentYPosition - blockElementHeight, htmlRectangle.GetWidth());
				document.Add(p);
			}
			else if (blockElement is Table table)
			{
				table.SetFixedPosition(this.getPage(), htmlRectangle.GetX(), currentYPosition.CurrentYPosition - blockElementHeight, htmlRectangle.GetWidth());
				document.Add(table);
			}
			else if (blockElement is List list)
			{
				list.SetFixedPosition(this.getPage(), htmlRectangle.GetX(), currentYPosition.CurrentYPosition - blockElementHeight, htmlRectangle.GetWidth());
				document.Add(list);
			}
			else if (blockElement is Link anchor)
			{
				anchor.SetFixedPosition(this.getPage(), htmlRectangle.GetX(), currentYPosition.CurrentYPosition - blockElementHeight, htmlRectangle.GetWidth());
				document.Add((IBlockElement)anchor);
			}
			else if (blockElement is Image image)
			{
				image.SetFixedPosition(this.getPage(), htmlRectangle.GetX(), currentYPosition.CurrentYPosition - blockElementHeight, htmlRectangle.GetWidth());
				document.Add(image);
			}
			currentYPosition.CurrentYPosition = currentYPosition.CurrentYPosition - blockElementHeight;

			return;
		}

		private float GetBlockElementHeight(IBlockElement blockElement, Rectangle htmlRectangle)
		{
			return blockElement.CreateRendererSubTree().SetParent(document.GetRenderer()).Layout(new LayoutContext(new LayoutArea(this.getPage(), htmlRectangle))).GetOccupiedArea().GetBBox().GetHeight();
		}

		//Utility class used to know where the cursor is left after each block element (HTML tag) is rendered
		private class YPosition
		{
			public YPosition(float initialYPosition)
			{
				CurrentYPosition = initialYPosition;
			}

			public float CurrentYPosition { get; set; }
		}

		private BaseDirection? GetBaseDirection(int runDirection)
		{
			switch (runDirection)
			{
				case 2: return BaseDirection.LEFT_TO_RIGHT;
				default: return null;
			}
		}

		private VerticalAlignment GetVerticalAlignment(float valign)
		{
			if (valign == (int)VerticalAlign.TOP)
				return VerticalAlignment.TOP;
			else if (valign == (int)VerticalAlign.BOTTOM)
				return VerticalAlignment.BOTTOM;
			else
				return VerticalAlignment.MIDDLE;
		}

		private TextAlignment? GetTextAlignment(int alignment)
		{
			switch (alignment)
			{
				case 1: return TextAlignment.CENTER;
				case 2: return TextAlignment.RIGHT;
				case 0: return TextAlignment.LEFT;
				case 3: return TextAlignment.JUSTIFIED;
			}
			return null;
		}

		void DrawTextColumn(float llx, float lly, float urx, float ury, float leading, String text, int valign, int alignment, Style style, Boolean wrap)
		{
			Paragraph p = new Paragraph(text);

			if (valign == (int)VerticalAlign.MIDDLE)
			{
				ury = ury + leading;
				p.SetVerticalAlignment(VerticalAlignment.MIDDLE);
			}
			else if (valign == (int)VerticalAlign.BOTTOM)
			{
				ury = ury + leading;
				p.SetVerticalAlignment(VerticalAlignment.BOTTOM);
			}
			else if (valign == (int)VerticalAlign.TOP)
			{
				ury = ury + leading / 2;
				p.SetVerticalAlignment(VerticalAlignment.TOP);
			}
			Rectangle rect = new Rectangle(llx, lly, urx - llx, ury - lly);
			p.SetTextAlignment(GetTextAlignment(alignment));
			p.AddStyle(style);

			if (wrap)
			{
				p.SetProperty(Property.SPLIT_CHARACTERS, new CustomSplitCharacters());
				Table table = new Table(1);
				table.SetFixedPosition(this.getPage(), rect.GetX(), rect.GetY(), rect.GetWidth());
				Cell cell = new Cell();
				cell.SetWidth(rect.GetWidth());
				cell.SetHeight(rect.GetHeight());
				cell.SetBorder(Border.NO_BORDER);
				cell.SetVerticalAlignment(VerticalAlignment.MIDDLE);
				cell.Add(p);
				table.AddCell(cell);
				document.Add(table);
			}
			else
			{
				try
				{
					PdfCanvas pdfCanvas = new PdfCanvas(pdfPage);
					Canvas canvas = new Canvas(pdfCanvas, rect);
					canvas.Add(p);
					canvas.Close();
				}
				catch (Exception e) { GXLogging.Error(log, "GxDrawText failed to justify text column: ", e); }
			}
		}

		public class CustomSplitCharacters : DefaultSplitCharacters
		{
			public override bool IsSplitCharacter(GlyphLine text, int glyphPos)
			{
				if (!text.Get(glyphPos).HasValidUnicode())
				{
					return false;
				}

				bool baseResult = base.IsSplitCharacter(text, glyphPos);
				bool myResult = false;
				Glyph glyph = text.Get(glyphPos);

				if (glyph.GetUnicode() == '_')
				{
					myResult = true;
				}
				return myResult || baseResult;
			}
		}

#pragma warning restore CS0612 // Type or member is obsolete

		private PageSize ComputePageSize(float leftMargin, float topMargin, int width, int length, bool marginsInsideBorder)
		{
			if ((leftMargin == 0 && topMargin == 0) || marginsInsideBorder)
			{
				if (length == 23818 && width == 16834)
					return PageSize.A3;
				else if (length == 16834 && width == 11909)
					return PageSize.A4;
				else if (length == 11909 && width == 8395)
					return PageSize.A5;
				else if (length == 20016 && width == 5731)
					return PageSize.B4;
				else if (length == 14170 && width == 9979)
					return PageSize.B5;
				else if (length == 15120 && width == 10440)
					return PageSize.EXECUTIVE;
				else if (length == 20160 && width == 12240)
					return PageSize.LEGAL;
				else if (length == 15840 && width == 12240)
					return PageSize.LETTER;
				else
					return new PageSize(new Rectangle((int)(width / PAGE_SCALE_X), (int)(length / PAGE_SCALE_Y)));
			}
			return new PageSize(new Rectangle((int)(width / PAGE_SCALE_X) + leftMargin, (int)(length / PAGE_SCALE_Y) + topMargin));
		}
		public override void GxEndDocument()
		{
			//{{Pages}}
			if (templateCreated)
			{
				int totalPages = pdfDocument.GetNumberOfPages();
				for (int i = 0; i < totalPages; i++)
				{
					PdfPage page = pdfDocument.GetPage(i);
					Canvas canvas = new Canvas(page, templateRectangle);
					canvas.ShowTextAligned(i.ToString(CultureInfo.InvariantCulture), templatex, templatey, TextAlignment.CENTER).SetBackgroundColor(templateColorFill).SetFont(templateFont).SetFontSize(templateFontSize);
				}
			}

			int copies = 1;
			try
			{
				copies = Convert.ToInt32(printerSettings.getProperty(form, Const.COPIES));
				GXLogging.Debug(log, "Setting number of copies to " + copies);
				PdfViewerPreferences preferences = new PdfViewerPreferences();
				preferences.SetNumCopies(copies);

				int duplex = Convert.ToInt32(printerSettings.getProperty(form, Const.DUPLEX));
				PdfViewerPreferences.PdfViewerPreferencesConstants duplexValue;
				if (duplex == 1)
					duplexValue = PdfViewerPreferences.PdfViewerPreferencesConstants.SIMPLEX;
				else if (duplex == 2)
					duplexValue = PdfViewerPreferences.PdfViewerPreferencesConstants.DUPLEX_FLIP_LONG_EDGE;
				else if (duplex == 3)
					duplexValue = PdfViewerPreferences.PdfViewerPreferencesConstants.DUPLEX_FLIP_SHORT_EDGE;
				else if (duplex == 4)
					duplexValue = PdfViewerPreferences.PdfViewerPreferencesConstants.DUPLEX_FLIP_LONG_EDGE;
				else
					duplexValue = PdfViewerPreferences.PdfViewerPreferencesConstants.NONE;
				GXLogging.Debug(log, "Setting duplex to " + duplexValue.ToString());
				preferences.SetDuplex(duplexValue);
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Setting viewer preference error", ex);
			}

			bool printingScript = false;
			String serverPrinting = props.getGeneralProperty(Const.SERVER_PRINTING);
			bool fit = props.getGeneralProperty(Const.ADJUST_TO_PAPER).Equals("true");
			if ((outputType == Const.OUTPUT_PRINTER || outputType == Const.OUTPUT_STREAM_PRINTER) && serverPrinting.Equals("false"))
			{

				printingScript = true;
				StringBuilder javascript = new StringBuilder(); ;

				javascript.Append("var pp = this.getPrintParams();");
				String printer = printerSettings.getProperty(form, Const.PRINTER).Replace("\\", "\\\\");
				if (!string.IsNullOrEmpty(printer))
					javascript.Append("pp.printerName = \"" + printer + "\";\n");

				if (fit)
				{
					javascript.Append("pp.pageHandling = pp.constants.handling.fit;\n");
				}
				else
				{
					javascript.Append("pp.pageHandling = pp.constants.handling.none;\n");
				}

				GXLogging.Debug(log, "MODE:" + printerSettings.getProperty(form, Const.MODE) + ",form:" + form);

				if (printerSettings.getProperty(form, Const.MODE, "3").StartsWith("0"))//Show printer dialog Never
				{
					javascript.Append("pp.interactive = pp.constants.interactionLevel.automatic;\n");

					for (int i = 0; i < copies; i++)
					{
						javascript.Append("this.print(pp);\n");
					}

					//writer.addJavaScript("this.print({bUI: false, bSilent: true, bShrinkToFit: true});");
					//No print dialog is displayed. During printing a progress monitor and cancel
					//dialog is displayed and removed automatically when printing is complete.
				}
				else //Show printer dialog is sent directly to printer | always
				{
					javascript.Append("pp.interactive = pp.constants.interactionLevel.full;\n");
					//Displays the print dialog allowing the user to change print settings and requiring
					//the user to press OK to continue. During printing a progress monitor and cancel
					//dialog is displayed and removed automatically when printing is complete.

					javascript.Append("this.print(pp);\n");

				}
				pdfDocument.GetCatalog().SetOpenAction(PdfAction.CreateJavaScript(javascript.ToString()));
			}

			if (IsPdfA())
			{
				/*using (Stream iccProfile = ReadResource("sRGB Color Space Profile.icm"))
				{
					ICC_Profile icc = ICC_Profile.GetInstance(iccProfile);
					writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
				}

				writer.ExtraCatalog.Put(PdfName.LANG, new PdfString(Config.GetCultureForLang(language).Name));
				PdfDictionary markInfo = new PdfDictionary(PdfName.MARKINFO);
				markInfo.Put(PdfName.MARKED, new PdfBoolean(PdfBoolean.TRUE));
				writer.ExtraCatalog.Put(PdfName.MARKINFO, markInfo);

				writer.CreateXmpMetadata();*/

			}
			document.Close();
			GXLogging.Debug(log, "GxEndDocument!");
			try
			{
				props.save();
				GXLogging.Debug(log, "props.save()");
			}
			catch (IOException e)
			{
				GXLogging.Error(log, "props.save() error", e);

			}
			GXLogging.Debug(log, "outputType: " + outputType + ",docName:" + docName);

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
					catch (Exception){}
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
					catch (Exception){}
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

			GXLogging.Debug(log, "GxEndDocument End");
		}
		public override void GxStartPage()
		{
			try
			{
				pdfPage = pdfDocument.AddNewPage();
				GXLogging.Debug(log, "GxStartPage pages:" + pages + ",new page:" + pages + 1);
				pages = pages + 1;
			}
			catch (PdfException de)
			{
				GXLogging.Error(log, "GxStartPage error", de);
			}
		}

		private bool IsTrueType(PdfFont font)
		{
			return font.GetFontProgram() is TrueTypeFont;
		}
	}
}

