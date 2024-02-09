using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using GeneXus;
using GeneXus.Configuration;
using GeneXus.Metadata;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;

namespace com.genexus.reports
{
	public class PDFReportItextSharp : PDFReportBase
	{

		static IGXLogger log = GXLoggerFactory.GetLogger<PDFReportItextSharp>();
		private BaseFont baseFont;
		Barcode barcode = null;
		//Color for, BaseColor for => Itext5
		private object backColor, foreColor, templateColorFill;
		private Rectangle pageSize;
		private Document document;
		private PdfWriter writer;
		private PdfTemplate template;
		private BaseFont templateFont;
		private bool asianFontsDllLoaded = false;
		internal Dictionary<string, Image> documentImages;
		int justifiedType;
		static Assembly iTextAssembly = typeof(Document).Assembly;

		public PDFReportItextSharp(String appPath):base(appPath)
		{

			document = null;
			documentImages = new Dictionary<string, Image>();
		}

		protected override void init(ref int gxYPage, ref int gxXPage, int pageWidth, int pageLength)
		{
			this.pageSize = ComputePageSize(leftMargin, topMargin, pageWidth, pageLength, props.getBooleanGeneralProperty(Const.MARGINS_INSIDE_BORDER, Const.DEFAULT_MARGINS_INSIDE_BORDER));
			gxXPage = (int)this.pageSize.Right;
			if (props.getBooleanGeneralProperty(Const.FIX_SAC24437, true))
				gxYPage = (int)(pageLength / GX_PAGE_SCALE_Y);
			else
				gxYPage = (int)(pageLength / GX_PAGE_SCALE_Y_OLD);

			if (props.getBooleanGeneralProperty(Const.JUSTIFIED_TYPE_ALL, false))
				justifiedType = Element.ALIGN_JUSTIFIED_ALL;
			else
				justifiedType = Element.ALIGN_JUSTIFIED;


			document = new Document(this.pageSize, 0, 0, 0, 0);

			Document.Compress = true;
			try
			{
				writer = PdfWriter.GetInstance(document, outputStream);
				string level = props.getGeneralProperty(Const.COMPLIANCE_LEVEL);
				if (Enum.TryParse(level, true, out complianceLevel))
				{
					if (SetComplainceLevel(complianceLevel))
						writer.SetTagged();
				}
				document.Open();

			}
			catch (DocumentException de)
			{
				GXLogging.Debug(log, "init error", de);
			}
		}

		/**
		* @param hideCorners indicates whether corner triangles should be hidden when the side that joins them is hidden.
		*/
		private void drawRectangle(PdfContentByte cb, float x, float y, float w, float h,
			int styleTop, int styleBottom, int styleRight, int styleLeft,
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
		private void roundRectangle(PdfContentByte cb, float x, float y, float w, float h,
			float radioTL, float radioTR, float radioBL, float radioBR)
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

			PdfContentByte cb = writer.DirectContent;

			float penAux = (float)convertScale(pen);
			float rightAux = (float)convertScale(right);
			float bottomAux = (float)convertScale(bottom);
			float leftAux = (float)convertScale(left);
			float topAux = (float)convertScale(top);
			GXLogging.Debug(log, "GxDrawRect -> (" + left + "," + top + ") - (" + right + "," + bottom + ")  BackMode: " + backMode + " Pen:" + pen + ",leftMargin:" + leftMargin);
			cb.SaveState();

			float x1, y1, x2, y2;
			x1 = leftAux + leftMargin;
			y1 = pageSize.Top - bottomAux - topMargin - bottomMargin;
			x2 = rightAux + leftMargin;
			y2 = pageSize.Top - topAux - topMargin - bottomMargin;

			cb.SetLineWidth(penAux);
			cb.SetLineCap(PdfContentByte.LINE_CAP_PROJECTING_SQUARE);

			if (cornerRadioBL == 0 && cornerRadioBR == 0 && cornerRadioTL == 0 && cornerRadioTR == 0 && styleBottom == 0 && styleLeft == 0 && styleRight == 0 && styleTop == 0)
			{
				//border color must be the same as the fill if border=0 since setLineWidth does not work.
				if (pen > 0)
					cb.SetRGBColorStroke(foreRed, foreGreen, foreBlue);
				else
					cb.SetRGBColorStroke(backRed, backGreen, backBlue);
				cb.Rectangle(x1, y1, x2 - x1, y2 - y1);

				if (backMode != 0)
				{
					ClassLoader.Invoke(cb, "SetColorFill", new object[] { GetColor(backRed, backGreen, backBlue) });
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
					cb.SetRGBColorStroke(backRed, backGreen, backBlue);
					cb.SetLineWidth(0);
					roundRectangle(cb, x1, y1, w, h,
						cRadioTL, cRadioTR,
						cRadioBL, cRadioBR);
					ClassLoader.Invoke(cb, "SetColorFill", new object[] { GetColor(backRed, backGreen, backBlue) });
					cb.FillStroke();
					cb.SetLineWidth(penAux);

				}
				if (pen > 0)
				{
					//Rectangle edges
					cb.SetRGBColorStroke(foreRed, foreGreen, foreBlue);
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
			PdfContentByte cb = writer.DirectContent;

			float widthAux = (float)convertScale(width);
			float rightAux = (float)convertScale(right);
			float bottomAux = (float)convertScale(bottom);
			float leftAux = (float)convertScale(left);
			float topAux = (float)convertScale(top);

			GXLogging.Debug(log, "GxDrawLine leftAux: " + leftAux + ",leftMargin:" + leftMargin + ",pageSize.Top:" + pageSize.Top + ",bottomAux:" + bottomAux + ",topMargin:" + topMargin + ",bottomMargin:" + bottomMargin);

			GXLogging.Debug(log, "GxDrawLine -> (" + left + "," + top + ") - (" + right + "," + bottom + ")  Width: " + width);
			float x1, y1, x2, y2;
			x1 = leftAux + leftMargin;
			y1 = pageSize.Top - bottomAux - topMargin - bottomMargin;
			x2 = rightAux + leftMargin;
			y2 = pageSize.Top - topAux - topMargin - bottomMargin;

			GXLogging.Debug(log, "Line-> (" + (x1) + "," + y1 + ") - (" + x2 + "," + y2 + ") ");
			cb.SaveState();
			cb.SetRGBColorStroke(foreRed, foreGreen, foreBlue);
			cb.SetLineWidth(widthAux);

			if (lineCapProjectingSquare)
			{
				cb.SetLineCap(PdfContentByte.LINE_CAP_PROJECTING_SQUARE);
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
				iTextSharp.text.Image image;
				iTextSharp.text.Image imageRef;
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

							image = iTextSharp.text.Image.GetInstance(defaultRelativePrepend + bitmap);
							if (image == null)
							{
								bitmap = webAppDir + bitmap;
								image = iTextSharp.text.Image.GetInstance(bitmap);
							}
							else
							{
								bitmap = defaultRelativePrepend + bitmap;
							}
						}
						else
						{
							image = iTextSharp.text.Image.GetInstance(bitmap);
						}
					}
					catch (Exception)//absolute url
					{
						Uri uri = new Uri(bitmap);
						image = Image.GetInstance(uri);
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
					image.SetAbsolutePosition(leftAux + leftMargin, this.pageSize.Top - bottomAux - topMargin - bottomMargin);
					if (aspectRatio == 0)
						image.ScaleAbsolute(rightAux - leftAux, bottomAux - topAux);
					else
						image.ScaleToFit(rightAux - leftAux, bottomAux - topAux);

					PdfContentByte cb = writer.DirectContent;
					image.Alt = Path.GetFileName(bitmap);
					cb.AddImage(image);
				}
			}
			catch (DocumentException de)
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
				barcode = new Barcode128();
				barcode.CodeType = Barcode128.CODE128;
			}
			else
			{
				barcode = null;
			}
			this.fontUnderline = fontUnderline;
			this.fontStrikethru = fontStrikethru;
			this.fontSize = fontSize;
			foreColor = GetColor(foreRed, foreGreen, foreBlue);
			backColor = GetColor(backRed, backGreen, backBlue);

			backFill = (backMode != 0);
			try
			{
				LoadAsianFontsDll();
				string f = fontName.ToLower();
				if (PDFFont.isType1(fontName))
				{
					//Asian font
					for (int i = 0; i < Type1FontMetrics.CJKNames.Length; i++)
					{
						if (Type1FontMetrics.CJKNames[i][0].ToLower().Equals(f) ||
							Type1FontMetrics.CJKNames[i][1].ToLower().Equals(f))
						{
							String style = "";
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
						baseFont = BaseFont.CreateFont(fontName, BaseFont.WINANSI, BaseFont.NOT_EMBEDDED);
					}
				}
				else
				{//True type font

					if (IsEmbeddedFont(fontName))
					{
						if (fontBold && fontItalic)
							fontName = fontName + ",BoldItalic";
						else
						{
							if (fontItalic)
								fontName = fontName + ",Italic";
							if (fontBold)
								fontName = fontName + ",Bold";
						}
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
								baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
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

							String style = "";
							if (fontBold && fontItalic)
							{
								style = ",BoldItalic";
							}
							else
							{
								if (fontItalic) style = ",Italic";
								if (fontBold) style = ",Bold";
							}

							GXLogging.Debug(log, "NOT EMBEED_SECTION Font");
							baseFont = BaseFont.CreateFont(fontPath + style, BaseFont.WINANSI, BaseFont.NOT_EMBEDDED);
						}
					}
					else
					{
						GXLogging.Debug(log, "NOT foundFont fontName:" + fontName);
					}
				}
			}
			catch (DocumentException de)
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
		BaseFont defaultFont;
		private BaseFont CreateDefaultFont()
		{
			if (defaultFont == null)
			{
				if (IsPdfA())
					defaultFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.EMBEDDED);
				else
					defaultFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.WINANSI, BaseFont.NOT_EMBEDDED);
			}
			return defaultFont;		
		}
		private void LoadAsianFontsDll()
		{
			try
			{
				if (!asianFontsDllLoaded)
				{
					Assembly itextAsian = Assembly.Load("iTextAsian");
					if (IsItext4())
						ClassLoader.InvokeStatic(iTextAssembly, "iTextSharp.text.pdf.BaseFont", "AddToResourceSearch", new object[] { itextAsian });
					else
						ClassLoader.InvokeStatic(iTextAssembly, "iTextSharp.text.io.StreamUtil", "AddToResourceSearch", new object[] { itextAsian });

					asianFontsDllLoaded = true;
				}
			}
			catch (Exception ae)
			{
				GXLogging.Debug(log, "LoadAsianFontsDll error", ae);
			}
		}
		private bool IsItext4()
		{
			return iTextAssembly.GetName().Version.Major == 4;
		}
		public override void setAsianFont(String fontName, String style)
		{
			LoadAsianFontsDll();
			try
			{
				if (string.IsNullOrEmpty(style))
				{
					if (fontName.Equals("Japanese"))
						baseFont = BaseFont.CreateFont("HeiseiMin-W3", "UniJIS-UCS2-H", BaseFont.NOT_EMBEDDED);
					if (fontName.Equals("Japanese2"))
						baseFont = BaseFont.CreateFont("HeiseiKakuGo-W5", "UniJIS-UCS2-H", BaseFont.NOT_EMBEDDED);
					if (fontName.Equals("SimplifiedChinese"))
						baseFont = BaseFont.CreateFont("STSong-Light", "UniGB-UCS2-H", BaseFont.NOT_EMBEDDED);
					if (fontName.Equals("TraditionalChinese"))
						baseFont = BaseFont.CreateFont("MHei-Medium", "UniCNS-UCS2-H", BaseFont.NOT_EMBEDDED);
					if (fontName.Equals("Korean"))
						baseFont = BaseFont.CreateFont("HYSMyeongJo-Medium", "UniKS-UCS2-H", BaseFont.NOT_EMBEDDED);
				}
				else
				{
					if (fontName.Equals("Japanese"))
						baseFont = BaseFont.CreateFont("HeiseiMin-W3," + style, "UniJIS-UCS2-H", BaseFont.NOT_EMBEDDED);
					if (fontName.Equals("Japanese2"))
						baseFont = BaseFont.CreateFont("HeiseiKakuGo-W5," + style, "UniJIS-UCS2-H", BaseFont.NOT_EMBEDDED);
					if (fontName.Equals("SimplifiedChinese"))
						baseFont = BaseFont.CreateFont("STSong-Light," + style, "UniGB-UCS2-H", BaseFont.NOT_EMBEDDED);
					if (fontName.Equals("TraditionalChinese"))
						baseFont = BaseFont.CreateFont("MHei-Medium," + style, "UniCNS-UCS2-H", BaseFont.NOT_EMBEDDED);
					if (fontName.Equals("Korean"))
						baseFont = BaseFont.CreateFont("HYSMyeongJo-Medium," + style, "UniKS-UCS2-H", BaseFont.NOT_EMBEDDED);
				}
			}
			catch (DocumentException de)
			{
				GXLogging.Debug(log, "setAsianFont  error", de);
			}
			catch (IOException ioe)
			{
				GXLogging.Debug(log, "setAsianFont io error", ioe);
			}
		}
	
#pragma warning disable CS0612 // Type or member is obsolete
		public override void GxDrawText(String sTxt, int left, int top, int right, int bottom, int align, int htmlformat, int border, int valign)
		{
			GXLogging.Debug(log, "GxDrawText, text:" + sTxt);
			bool printRectangle = false;
			if (props.getBooleanGeneralProperty(Const.BACK_FILL_IN_CONTROLS, true))
				printRectangle = true;

			if (printRectangle && (border == 1 || backFill))
			{
				GxDrawRect(left, top, right, bottom, border, GetColorR(foreColor), GetColorG(foreColor), GetColorB(foreColor), backFill ? 1 : 0, GetColorR(backColor), GetColorG(backColor), GetColorB(backColor), 0, 0);
			}
			Font font = new Font(baseFont, fontSize);
			int arabicOptions = 0;
			PdfContentByte cb = writer.DirectContent;
			cb.SetFontAndSize(baseFont, fontSize);
			ClassLoader.Invoke(cb, "SetColorFill", new object[] { foreColor });
			sTxt = sTxt.TrimEnd(TRIM_CHARS);
			float captionHeight = baseFont.GetFontDescriptor(BaseFont.CAPHEIGHT, fontSize);
			float rectangleWidth = baseFont.GetWidthPoint(sTxt, fontSize);
			float lineHeight = baseFont.GetFontDescriptor(BaseFont.BBOXURY, fontSize) - baseFont.GetFontDescriptor(BaseFont.BBOXLLY, fontSize);
			float textBlockHeight = (float)convertScale(bottom - top);
			int linesCount = (int)(textBlockHeight / lineHeight);
			int bottomOri = bottom;
			int topOri = top;
			//If the field has more than one line and it has no wrap, it is not justify, 
			//and it is not html, it is simulated that the field has only one line
			//assigning the top plus the lineHeight to the bottom
			if (linesCount >= 2 && !((align & 16) == 16) && htmlformat != 1)
			{
				if (valign == (int)VerticalAlign.TOP)
					bottom = top + (int)reconvertScale(lineHeight);
				else if (valign == (int)VerticalAlign.BOTTOM)
					top = bottom - (int)reconvertScale(lineHeight);

			}

			GXLogging.Debug(log, "lineHeight: " + lineHeight);

			float bottomAux = (float)convertScale(bottom) - ((float)convertScale(bottom - top) - captionHeight) / 2;
			//Space between the text and the edge of the textblock is substracted from bottom
			//Because coordinates x,y for a genexus report corresponds to the box containing the text,
			//and itext expects x,y of the text itself.
			//The box is bigger thant the text (depends on the type of font)
			float topAux = (float)convertScale(top) + ((float)convertScale(bottom - top) - captionHeight) / 2;

			float startHeight = bottomAux - topAux - captionHeight;

			float leftAux = (float)convertScale(left);
			float rightAux = (float)convertScale(right);
			int alignment = align & 3;
			bool autoResize = (align & 256) == 256;

			GXLogging.Debug(log, "GxDrawText left: " + left + ",top:" + top + ",right:" + right + ",bottom:" + bottom + ",captionHeight:" + captionHeight + ",fontSize:" + fontSize);
			GXLogging.Debug(log, "GxDrawText leftAux: " + leftAux + ",leftMargin:" + leftMargin + ",pageSize.Top:" + pageSize.Top + ",bottomAux:" + bottomAux + ",topMargin:" + topMargin + ",bottomMargin:" + bottomMargin);
			if (htmlformat == 1)
			{

				StyleSheet styles = new StyleSheet();
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
							FontFactory.Register(fontPath, fontName);
							styles.LoadTagStyle("body", "face", fontName);

							if (IsEmbeddedFont(fontName))
								styles.LoadTagStyle("body", "encoding", BaseFont.IDENTITY_H);
							else
								styles.LoadTagStyle("body", "encoding", BaseFont.WINANSI);
						}
					}
				}

				//Bottom and top are the absolutes, regardless of the actual height at which the letters are written.
				bottomAux = (float)convertScale(bottom);
				topAux = (float)convertScale(top);


				ColumnText Col = new ColumnText(cb);
				int colAlignment = ColumnAlignment(alignment);
				if (colAlignment != 0)
					Col.Alignment = colAlignment;
				ColumnText simulationCol = new ColumnText(null);
				float drawingPageHeight = (float)this.pageSize.Top - topMargin - bottomMargin;

				Rectangle rect = new Rectangle(leftAux + leftMargin,
				drawingPageHeight - bottomAux,
				 rightAux + leftMargin,
				drawingPageHeight - topAux);

				SetSimpleColumn(Col, rect);
				SetSimpleColumn(simulationCol, rect);

				try
				{
					PdfPCell cell = GetPdfCell();
					PdfPTable t = GetPdfTable(rightAux - leftAux);

					bool firstPar = true;
					ICollection objects = (ICollection)ClassLoader.InvokeStatic(iTextAssembly, typeof(HTMLWorker).FullName, "ParseToList", new object[] { new StringReader(sTxt), styles });
					bool drawWithTable = IsItext4() && DrawWithTable(objects);

					foreach (object element in objects)
					{
						if (PageHeightExceeded(bottomAux, drawingPageHeight))
						{
							simulationCol.AddElement((IElement)element);
							simulationCol.Go(true);

							if (simulationCol.YLine < bottomMargin)
							{
								rect = new Rectangle(leftAux + leftMargin, drawingPageHeight - bottomAux, rightAux + leftMargin, drawingPageHeight - topAux);

								bottomAux -= drawingPageHeight;

								if (drawWithTable)
								{
									ClassLoader.Invoke(t, "AddCell", new object[] { cell });
									t.WriteSelectedRows(0, -1, leftAux + leftMargin, drawingPageHeight - topAux, cb);
								}

								GxEndPage();
								GxStartPage();
								simulationCol = new ColumnText(null);
								SetSimpleColumn(simulationCol, rect);
								simulationCol.AddElement((IElement)element);

								if (drawWithTable)
								{
									cell = GetPdfCell();
									t = GetPdfTable(rightAux - leftAux);
								}
								else
								{
									Col = new ColumnText(cb);
									if (colAlignment != 0)
										Col.Alignment = colAlignment;
									SetSimpleColumn(Col, rect);
								}

							}
						}
						Paragraph p = element as Paragraph;
						if (p != null)
						{
							if (colAlignment != 0)
								p.Alignment = colAlignment;
							if (firstPar && drawWithTable)
							{
								p.SetLeading(0, 1);
								firstPar = false;
							}
						}
						if (drawWithTable)
							cell.AddElement((IElement)element);
						else
						{
							Col.AddElement((IElement)element);
							Col.Go();
						}
					}
					if (drawWithTable)
					{
						ClassLoader.Invoke(t, "AddCell", new object[] { cell });
						t.WriteSelectedRows(0, -1, leftAux + leftMargin, drawingPageHeight - topAux, cb);
					}
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
					barcode.Code = sTxt;
					barcode.TextAlignment = alignment;
					iTextSharp.text.Rectangle rectangle = new iTextSharp.text.Rectangle(0, 0);

					switch (alignment)
					{
						case 1: // Center Alignment
							rectangle = new iTextSharp.text.Rectangle((leftAux + rightAux) / 2 + leftMargin - rectangleWidth / 2,
								(float)this.pageSize.Top - (float)convertScale(bottom) - topMargin - bottomMargin,
								(leftAux + rightAux) / 2 + leftMargin + rectangleWidth / 2,
								(float)this.pageSize.Top - (float)convertScale(top) - topMargin - bottomMargin);
							break;
						case 2: // Right Alignment
							rectangle = new iTextSharp.text.Rectangle(rightAux + leftMargin - rectangleWidth,
								(float)this.pageSize.Top - (float)convertScale(bottom) - topMargin - bottomMargin,
								rightAux + leftMargin,
								(float)this.pageSize.Top - (float)convertScale(top) - topMargin - bottomMargin);
							break;
						case 0: // Left Alignment
							rectangle = new iTextSharp.text.Rectangle(leftAux + leftMargin,
								(float)this.pageSize.Top - (float)convertScale(bottom) - topMargin - bottomMargin,
								leftAux + leftMargin + rectangleWidth,
								(float)this.pageSize.Top - (float)convertScale(top) - topMargin - bottomMargin);
							break;
					}
					barcode.AltText = "";
					barcode.Baseline = 0;

					if (fontSize < Const.LARGE_FONT_SIZE)
						barcode.X = Const.OPTIMAL_MINIMU_BAR_WIDTH_SMALL_FONT;
					else
						barcode.X = Const.OPTIMAL_MINIMU_BAR_WIDTH_LARGE_FONT;

					Image imageCode = (Image)ClassLoader.Invoke(barcode, "CreateImageWithBarcode", new object[] { cb, backFill ? backColor : null, foreColor });
					imageCode.SetAbsolutePosition(leftAux + leftMargin, rectangle.Bottom);
					barcode.BarHeight = rectangle.Height;
					imageCode.ScaleToFit(rectangle.Width, rectangle.Height);
					document.Add(imageCode);
				}
				catch (Exception ex)
				{
					GXLogging.Error(log, "Error generating Barcode " + barcode.GetType().ToString(), ex);
				}
			}
			else
			{

				if (backFill)
				{
					iTextSharp.text.Rectangle rectangle = new iTextSharp.text.Rectangle(0, 0);
					//Text with background
					switch (alignment)
					{
						case 1: // Center Alignment Element.ALIGN_CENTER
							rectangle = new iTextSharp.text.Rectangle((leftAux + rightAux) / 2 + leftMargin - rectangleWidth / 2, (float)this.pageSize.Top - bottomAux - topMargin - bottomMargin, (leftAux + rightAux) / 2 + leftMargin + rectangleWidth / 2, (float)this.pageSize.Top - topAux - topMargin - bottomMargin);
							break;
						case 2: // Right Alignment  Element.ALIGN_RIGHT
							rectangle = new iTextSharp.text.Rectangle(rightAux + leftMargin - rectangleWidth, (float)this.pageSize.Top - bottomAux - topMargin - bottomMargin, rightAux + leftMargin, (float)this.pageSize.Top - topAux - topMargin - bottomMargin);
							break;
						case 0: // Left Alignment Element.ALIGN_LEFT
							rectangle = new iTextSharp.text.Rectangle(leftAux + leftMargin, (float)this.pageSize.Top - bottomAux - topMargin - bottomMargin, leftAux + leftMargin + rectangleWidth, (float)this.pageSize.Top - topAux - topMargin - bottomMargin);
							break;
					}
					ClassLoader.SetPropValue(rectangle, "BackgroundColor", backColor);
					try
					{
						document.Add(rectangle);
					}
					catch (DocumentException de)
					{
						GXLogging.Error(log, "GxDrawText error", de);
					}
				}

				float underlineSeparation = lineHeight / 5;//Separation between the text and the underline
				int underlineHeight = (int)underlineSeparation + (int)(underlineSeparation / 4);
				iTextSharp.text.Rectangle underline;
				//Underlined text
				if (fontUnderline)
				{
					GXLogging.Debug(log, "underlineSeparation: " + underlineSeparation);
					GXLogging.Debug(log, "Kerning: " + GetKerning(baseFont, 'a', 'b'));

					underline = new iTextSharp.text.Rectangle(0, 0);
					switch (alignment)
					{
						case 1: // Center Alignment
							underline = new iTextSharp.text.Rectangle(
								(leftAux + rightAux) / 2 + leftMargin - rectangleWidth / 2,
								this.pageSize.Top - bottomAux - topMargin - bottomMargin + startHeight - underlineSeparation,
								(leftAux + rightAux) / 2 + leftMargin + rectangleWidth / 2,
								this.pageSize.Top - bottomAux - topMargin - bottomMargin + startHeight - underlineHeight);
							break;
						case 2: // Right Alignment
							underline = new iTextSharp.text.Rectangle(rightAux + leftMargin - rectangleWidth,
								this.pageSize.Top - bottomAux - topMargin - bottomMargin + startHeight - underlineSeparation,
								rightAux + leftMargin,
								this.pageSize.Top - bottomAux - topMargin - bottomMargin + startHeight - underlineHeight);
							break;
						case 0: // Left Alignment
							underline = new iTextSharp.text.Rectangle(leftAux + leftMargin,
								this.pageSize.Top - bottomAux - topMargin - bottomMargin + startHeight - underlineSeparation,
								leftAux + leftMargin + rectangleWidth,
								this.pageSize.Top - bottomAux - topMargin - bottomMargin + startHeight - underlineHeight);
							break;
					}
					ClassLoader.SetPropValue(underline, "BackgroundColor", foreColor);
					try
					{
						document.Add(underline);
					}
					catch (DocumentException de)
					{
						GXLogging.Error(log, "GxDrawText error", de);
					}
				}

				//Crossed text
				if (fontStrikethru)
				{
					underline = new iTextSharp.text.Rectangle(0, 0);
					float strikethruSeparation = lineHeight / 2;

					GXLogging.Debug(log, "underlineSeparation: " + underlineSeparation);
					GXLogging.Debug(log, "Kerning: " + GetKerning(baseFont, 'a', 'b'));
					switch (alignment)
					{
						case 1: // Center Alignment
							underline = new iTextSharp.text.Rectangle(
								(leftAux + rightAux) / 2 + leftMargin - rectangleWidth / 2,
								this.pageSize.Top - bottomAux - topMargin - bottomMargin + startHeight - underlineSeparation + strikethruSeparation,
								(leftAux + rightAux) / 2 + leftMargin + rectangleWidth / 2,
								this.pageSize.Top - bottomAux - topMargin - bottomMargin + startHeight - underlineHeight + strikethruSeparation);
							break;
						case 2: // Right Alignment
							underline = new iTextSharp.text.Rectangle(rightAux + leftMargin - rectangleWidth,
								this.pageSize.Top - bottomAux - topMargin - bottomMargin + startHeight - underlineSeparation + strikethruSeparation,
								rightAux + leftMargin,
								this.pageSize.Top - bottomAux - topMargin - bottomMargin + startHeight - underlineHeight + strikethruSeparation);
							break;
						case 0: // Left Alignment
							underline = new iTextSharp.text.Rectangle(leftAux + leftMargin,
								this.pageSize.Top - bottomAux - topMargin - bottomMargin + startHeight - underlineSeparation + strikethruSeparation,
								leftAux + leftMargin + rectangleWidth,
								this.pageSize.Top - bottomAux - topMargin - bottomMargin + startHeight - underlineHeight + strikethruSeparation);
							break;
					}
					ClassLoader.SetPropValue(underline, "BackgroundColor", foreColor);
					try
					{
						document.Add(underline);
					}
					catch (DocumentException de)
					{
						GXLogging.Error(log, "GxDrawText error", de);
					}
				}

				if (sTxt.Trim().ToLower().Equals("{{pages}}"))
				{
					if (!templateCreated)
					{
						template = cb.CreateTemplate((float)convertScale(right - left), (float)convertScale(bottom - top));
						templateCreated = true;
					}
					GXLogging.Debug(log, "GxDrawText addTemplate-> (" + (leftAux + leftMargin) + "," + (this.pageSize.Top - bottomAux - topMargin - bottomMargin) + ") ");

					cb.AddTemplate(template, leftAux + leftMargin, this.pageSize.Top - bottomAux - topMargin - bottomMargin);
					templateFont = baseFont;
					templateFontSize = fontSize;
					templateColorFill = foreColor;
					templateAlignment = alignment;
					return;
				}

				float textBlockWidth = rightAux - leftAux;
				float TxtWidth = baseFont.GetWidthPoint(sTxt, fontSize);
				bool justified = (alignment == 3) && textBlockWidth < TxtWidth;
				bool wrap = (align & 16) == 16;

				if (wrap || justified)
				{

					bottomAux = (float)convertScale(bottomOri);
					topAux = (float)convertScale(topOri);

					float leading = (float)Convert.ToDouble(props.getGeneralProperty(Const.LEADING), CultureInfo.InvariantCulture.NumberFormat);
					Paragraph p = new Paragraph(sTxt, font);

					float llx = leftAux + leftMargin;
					float lly = (float)this.pageSize.Top - bottomAux - topMargin - bottomMargin;
					float urx = rightAux + leftMargin;
					float ury = (float)this.pageSize.Top - topAux - topMargin - bottomMargin;

					DrawColumnText(cb, llx, lly, urx, ury, p, leading, runDirection, valign, alignment);
				}
				else //no wrap
				{
					startHeight = 0;
					if (!autoResize)
					{
						//It removes the last char from the text until it reaches a string whose width is passed only by one character
						// of the width of the textblock. That is the most similar text to design in genexus
						String newsTxt = sTxt;
						while (TxtWidth > textBlockWidth && (newsTxt.Length - 1 >= 0))
						{
							sTxt = newsTxt;
							newsTxt = newsTxt.Remove(newsTxt.Length - 1, 1);
							TxtWidth = baseFont.GetWidthPoint(newsTxt, fontSize);
						}
					}

					Phrase phrase = new Phrase(sTxt, font);
					switch (alignment)
					{
						case 1: // Center Alignment
							ColumnText.ShowTextAligned(cb, PdfContentByte.ALIGN_CENTER, phrase, ((leftAux + rightAux) / 2) + leftMargin, this.pageSize.Top - bottomAux - topMargin - bottomMargin + startHeight, 0, runDirection, arabicOptions);
							break;
						case 2: // Right Alignment
							ColumnText.ShowTextAligned(cb, PdfContentByte.ALIGN_RIGHT, phrase, rightAux + leftMargin, this.pageSize.Top - bottomAux - topMargin - bottomMargin + startHeight, 0, runDirection, arabicOptions);
							break;
						case 0: // Left Alignment 
						case 3: // Justified, only one text line
							ColumnText.ShowTextAligned(cb, PdfContentByte.ALIGN_LEFT, phrase, leftAux + leftMargin, this.pageSize.Top - bottomAux - topMargin - bottomMargin + startHeight, 0, runDirection, arabicOptions);
							break;
					}
				}
			}
		}

		private PdfPTable GetPdfTable(float totalWidth)
		{
			PdfPTable table = new PdfPTable(1);
			table.TotalWidth = totalWidth;
			return table;
		}

		private PdfPCell GetPdfCell()
		{
			PdfPCell cell = new PdfPCell();
			cell.Border = Rectangle.NO_BORDER;
			cell.Padding = 0;
			return cell;
		}

		private bool DrawWithTable(ICollection objects)
		{
			bool allImages = true;
			foreach (object element in objects)
			{
				Paragraph p = element as Paragraph;
				if (p != null)
				{
					foreach (Chunk ch in p.Chunks)
					{
						if (!ch.HasAttributes() || ch.Attributes["IMAGE"] == null)
							return false;
					}
				}
				else
				{
					allImages = false;
				}
			}
			return allImages;
		}

		private int GetColorB(object backColor)
		{
			return (int)ClassLoader.GetPropValue(backColor, "B");
		}

		private int GetColorG(object backColor)
		{
			return (int)ClassLoader.GetPropValue(backColor, "G");
		}

		private int GetColorR(object backColor)
		{
			return (int)ClassLoader.GetPropValue(backColor, "R");
		}

		private int GetKerning(BaseFont baseFont, char v1, char v2)
		{
			if (IsItext4())
				return baseFont.GetKerning(v1, v2);
			else
				return (int)ClassLoader.Invoke(baseFont, "GetKerning", new object[] { (int)Char.GetNumericValue(v1), (int)Char.GetNumericValue(v2) });

		}

		bool PageHeightExceeded(float bottomAux, float drawingPageHeight)
		{
			return bottomAux > drawingPageHeight;
		}

#pragma warning restore CS0612 // Type or member is obsolete

		ColumnText SimulateDrawColumnText(PdfContentByte cb, Rectangle rect, Paragraph p, float leading, int runDirection, int alignment)
		{
			ColumnText Col = new ColumnText(cb);
			Col.RunDirection = runDirection;
			Col.Alignment = alignment;
			Col.SetLeading(leading, 1);
			SetSimpleColumn(Col, rect);
			Col.AddText(p);
			Col.Go(true);
			return Col;
		}

		void DrawColumnText(PdfContentByte cb, float llx, float lly, float urx, float ury, Paragraph p, float leading, int runDirection, int valign, int alignment)
		{
			Rectangle rect = new Rectangle(llx, lly, urx, ury);
			ColumnText ct = SimulateDrawColumnText(cb, rect, p, leading, runDirection, alignment);//add the column in simulation mode
			float y = ct.YLine;
			int linesCount = ct.LinesWritten;

			//calculate a new rectangle for valign = middle 
			if (valign == (int)VerticalAlign.MIDDLE)
				ury = ury - ((y - lly) / 2) + leading;
			else if (valign == (int)VerticalAlign.BOTTOM)
				ury = ury - (y - lly - leading);
			else if (valign == (int)VerticalAlign.TOP)
				ury = ury + leading / 2;

			rect = new Rectangle(llx, lly, urx, ury); //Rectangle for new ury

			ColumnText Col = new ColumnText(cb);
			Col.RunDirection = runDirection;

			Col.Alignment = ColumnAlignment(alignment);

			if (linesCount <= 1)
				Col.SetLeading(0, MULTIPLIED_LEADING);
			else
				Col.SetLeading(leading, MULTIPLIED_LEADING);
			SetSimpleColumn(Col, rect);
			Col.AddText(p);
			Col.Go();
		}
		private int ColumnAlignment(int alignment)
		{
			if (alignment == Element.ALIGN_JUSTIFIED)
				return justifiedType;
			else
				return alignment;
		}

		private Rectangle ComputePageSize(float leftMargin, float topMargin, int width, int length, bool marginsInsideBorder)
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
					return new iTextSharp.text.Rectangle((int)(width / PAGE_SCALE_X), (int)(length / PAGE_SCALE_Y));
			}
			return new iTextSharp.text.Rectangle((int)(width / PAGE_SCALE_X) + leftMargin, (int)(length / PAGE_SCALE_Y) + topMargin);
		}

	
		public override void GxEndDocument()
		{
			if (document.PageNumber == 0)
			{
				writer.PageEmpty = false;
			}

			//{{Pages}}
			if (template != null)
			{

				template.BeginText();
				template.SetFontAndSize(templateFont, templateFontSize);
				ClassLoader.Invoke(template, "SetColorFill", new object[] { templateColorFill });
				switch (templateAlignment)
				{
					case 1: // Center Alignment
						template.ShowTextAligned(PdfContentByte.ALIGN_CENTER, pages.ToString(), template.Width / 2, 0, 0);
						break;
					case 2: // Right Alignment
						template.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, pages.ToString(), template.Width, 0, 0);
						break;
					case 0: // Left Alignment	
						template.SetTextMatrix(0, 0);
						template.ShowText(pages.ToString());
						break;
				}
				template.EndText();
			}
			int copies = 1;

			try
			{
				copies = Convert.ToInt32(printerSettings.getProperty(form, Const.COPIES));
				GXLogging.Debug(log, "Setting number of copies to " + copies);
				writer.AddViewerPreference(PdfName.NUMCOPIES, new PdfNumber(copies));

				int duplex = Convert.ToInt32(printerSettings.getProperty(form, Const.DUPLEX));
				PdfName duplexValue;
				if (duplex == 1)
					duplexValue = PdfName.SIMPLEX;
				else if (duplex == 2)
					duplexValue = PdfName.DUPLEX;
				else if (duplex == 3)
					duplexValue = PdfName.DUPLEXFLIPSHORTEDGE;
				else if (duplex == 4)
					duplexValue = PdfName.DUPLEXFLIPLONGEDGE;
				else
					duplexValue = PdfName.NONE;
				GXLogging.Debug(log, "Setting duplex to " + duplexValue.ToString());
				writer.AddViewerPreference(PdfName.DUPLEX, duplexValue);
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

				writer.AddJavaScript("var pp = this.getPrintParams();\n");
				String printer = printerSettings.getProperty(form, Const.PRINTER).Replace("\\", "\\\\");
				if (!string.IsNullOrEmpty(printer))
					writer.AddJavaScript("pp.printerName = \"" + printer + "\";\n");

				if (fit)
				{
					writer.AddJavaScript("pp.pageHandling = pp.constants.handling.fit;\n");
				}
				else
				{
					writer.AddJavaScript("pp.pageHandling = pp.constants.handling.none;\n");
				}

				GXLogging.Debug(log, "MODE:" + printerSettings.getProperty(form, Const.MODE) + ",form:" + form);

				if (printerSettings.getProperty(form, Const.MODE, "3").StartsWith("0"))//Show printer dialog Never
				{
					writer.AddJavaScript("pp.interactive = pp.constants.interactionLevel.automatic;\n");

					for (int i = 0; i < copies; i++)
					{
						writer.AddJavaScript("this.print(pp);\n");
					}

					//writer.addJavaScript("this.print({bUI: false, bSilent: true, bShrinkToFit: true});");
					//No print dialog is displayed. During printing a progress monitor and cancel
					//dialog is displayed and removed automatically when printing is complete.
				}
				else //Show printer dialog is sent directly to printer | always
				{
					writer.AddJavaScript("pp.interactive = pp.constants.interactionLevel.full;\n");
					//Displays the print dialog allowing the user to change print settings and requiring
					//the user to press OK to continue. During printing a progress monitor and cancel
					//dialog is displayed and removed automatically when printing is complete.

					writer.AddJavaScript("this.print(pp);\n");

				}

			}
			if (IsPdfA())
			{
				using (Stream iccProfile = ReadResource("sRGB Color Space Profile.icm"))
				{
					ICC_Profile icc = ICC_Profile.GetInstance(iccProfile);
					writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
				}

				writer.ExtraCatalog.Put(PdfName.LANG, new PdfString(Config.GetCultureForLang(language).Name));
				PdfDictionary markInfo = new PdfDictionary(PdfName.MARKINFO);
				markInfo.Put(PdfName.MARKED, new PdfBoolean(PdfBoolean.TRUE));
				writer.ExtraCatalog.Put(PdfName.MARKINFO, markInfo);

				writer.CreateXmpMetadata();

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
						;

						GXLogging.Error(log, "GxEndDocument OUTPUT_SCREEN error", e);

					}
					try { showReport(docName, modal); }
					catch (Exception)
					{

					}

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
					catch (Exception)
					{

					}
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

						;
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

				bool ret = document.NewPage();
				GXLogging.Debug(log, "GxStartPage pages:" + pages + ", opened:" + ret + ",new page:" + pages + 1);
				pages = pages + 1;
			}
			catch (DocumentException de)
			{
				GXLogging.Error(log, "GxStartPage error", de);
			}
		}

	
		private object GetColor(int backRed, int backGreen, int backBlue)
		{
			Type color;
			if (IsItext4())
				color = iTextAssembly.GetType("iTextSharp.text.Color");
			else
				color = iTextAssembly.GetType("iTextSharp.text.BaseColor");
			return ClassLoader.CreateInstance(iTextAssembly, color.FullName, new object[] { backRed, backGreen, backBlue });
		}

		private bool IsTrueType(BaseFont baseFont)
		{
			return baseFont.FontType == BaseFont.FONT_TYPE_TT;
		}

		private void SetSimpleColumn(ColumnText col, Rectangle rect)
		{
			col.SetSimpleColumn(rect.Left, rect.Bottom, rect.Right, rect.Top);
		}

		internal override bool SetComplainceLevel(PdfConformanceLevel level)
		{
			switch (level)
			{
				case PdfConformanceLevel.Pdf_A1A:
					writer.PDFXConformance = PdfWriter.PDFA1A;
					return true;
				case PdfConformanceLevel.Pdf_A1B:
					writer.PDFXConformance = PdfWriter.PDFA1B;
					return true;
				default:
					return false;
			}
		}
	}

}
