
using System;
using GeneXus.MSOffice.Excel.Style;
using GeneXus.Utils;
using log4net;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.Model;
using NPOI.XSSF.UserModel;
using NPOI.XSSF.UserModel.Extensions;

namespace GeneXus.MSOffice.Excel.Poi.Xssf
{
	public class ExcelCells : IExcelCellRange
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(ExcelCells));
		protected IGXError _errorHandler;
		protected ExcelSpreadsheet doc;
		protected int cellCount;
		protected int pWidth;
		protected int pHeight;
		protected int colStartIdx;
		protected int colEndIdx;
		protected int rowStartIdx;
		protected int rowEndIdx;
		protected XSSFWorkbook pWorkbook;
		protected ISheet pSelectedSheet;
		protected bool fitColumnWidth;
		protected bool readonlyFlag;
		protected StylesCache stylesCache;
		protected XSSFCell[] pCells;
		protected ExcelStyle cellStyle;
		public ExcelCells(IGXError errAccess, ExcelSpreadsheet document, XSSFWorkbook workBook, XSSFSheet selectedSheet,
			int rowPos, int colPos, int height, int width, StylesCache stylesCache) : this(errAccess, document, workBook, selectedSheet, rowPos, colPos, height, width, false, stylesCache)
		{ }

		public ExcelCells()
		{
		}

		public ExcelCells(IGXError errAccess, ExcelSpreadsheet document, XSSFWorkbook workBook, XSSFSheet selectedSheet,
			int rowPos, int colPos, int height, int width, bool isReadonly, StylesCache stylesCache)
		{
			_errorHandler = errAccess;
			doc = document;
			cellCount = 0;
			pWidth = width;
			pHeight = height;
			colStartIdx = colPos;
			colEndIdx = colPos + (width - 1);
			rowStartIdx = rowPos;
			rowEndIdx = rowPos + (height - 1);
			pWorkbook = workBook;
			pSelectedSheet = selectedSheet;
			fitColumnWidth = true;
			readonlyFlag = isReadonly;
			this.stylesCache = stylesCache;
			pCells = new XSSFCell[(width * height) + 1];

			try
			{
				for (int y = rowPos; y < (rowPos + pHeight); y++)
				{
					XSSFRow pRow = GetExcelRow(selectedSheet, y);
					if (pRow != null)
					{
						for (int x = colPos; x < (colPos + pWidth); x++)
						{
							ICell pCell = GetExcelCell(pRow, x);
							if (pCell != null)
							{
								cellCount++;
								pCells[cellCount] = (XSSFCell)pCell;
							}
						}
					}
				}
			}
			catch (Exception)
			{
				throw new ExcelException(8, "Invalid cell coordinates");
			}
		}

		protected XSSFRow GetExcelRow(XSSFSheet sheet, int rowPos)
		{
			XSSFRow row = (XSSFRow)sheet.GetRow(rowPos);

			if (row == null)
			{
				row = (XSSFRow)sheet.CreateRow(rowPos);
			}

			return row;
		}

		protected XSSFCell GetExcelCell(XSSFRow row, int colPos)
		{
			XSSFCell cell = (XSSFCell)row.GetCell(colPos);

			if (cell == null)
			{
				cell = (XSSFCell)row.CreateCell(colPos);
			}

			return cell;
		}

		public bool SetNumber(double value)
		{
			try
			{
				for (int i = 1; i <= cellCount; i++)
				{
					pCells[i].SetCellValue(value);
				}
				return true;
			}
			catch (Exception)
			{
				throw new ExcelException(7, "Invalid cell value");
			}
		}

		public decimal GetNumber()
		{
			try
			{
				return GetValue();
			}
			catch (Exception)
			{
				throw new ExcelException(7, "Invalid cell value");
			}
		}

		public bool SetDate(DateTime value)
		{
			CheckReadonlyDocument();

			try
			{
				if (value != DateTime.MinValue)
				{
					DateTime d = value;

					string dformat = "m/d/yy h:mm";

					if (value.Minute == 0 && value.Hour == 0 && value.Second == 0 && value.Millisecond == 0 && dformat.IndexOf(' ') > 0)
						dformat = dformat.Substring(0, dformat.IndexOf(' '));

					XSSFDataFormat df = (XSSFDataFormat)pWorkbook.CreateDataFormat();

					for (int i = 1; i <= cellCount; i++)
					{
						XSSFCellStyle cellStyle = (XSSFCellStyle)pCells[i].CellStyle;
						if (!DateUtil.IsCellDateFormatted(pCells[i]))
						{
							XSSFCellStyle newStyle = (XSSFCellStyle)pWorkbook.CreateCellStyle();
							CopyPropertiesStyle(newStyle, cellStyle);
							newStyle.DataFormat = df.GetFormat(dformat);
							pCells[i].CellStyle = newStyle;
							FitColumnWidth(i, dformat.Length + 4);
						}
						else
						{
							FitColumnWidth(i, cellStyle.GetDataFormatString().Length + 4);
						}
						pCells[i].SetCellValue(value);
					}
					return true;
				}
			}
			catch (Exception)
			{
				throw new ExcelException(7, "Invalid cell value");
			}
			return false;
		}

		public DateTime GetDate()
		{
			DateTime returnValue = DateTimeUtil.NullDate();
			try
			{
				returnValue = pCells[1].DateCellValue;
			}
			catch (Exception)
			{
				throw new ExcelException(7, "Invalid cell value");
			}
			return returnValue;
		}

		public bool SetTextImpl(string value)
		{
			CheckReadonlyDocument();

			try
			{
				for (int i = 1; i <= cellCount; i++)
				{
					if (value.Length > 0 && value[0] == '=')
					{
						try
						{
							pCells[i].CellFormula = value.Substring(1);
						}
						catch (Exception)
						{
							pCells[i].SetCellType(CellType.String);
							pCells[i].SetCellValue(value);
						}
					}
					else
						pCells[i].SetCellValue(value);
				}
				return true;
			}
			catch (Exception e)
			{
				throw new ExcelException(7, "Invalid cell value", e);
			}
		}

		private void CheckReadonlyDocument()
		{
			if (readonlyFlag)
			{
				throw new ExcelReadonlyException();
			}
		}
		public string GetText()
		{
			try
			{
				if (pCells[1].CellType == CellType.Formula)
					return "=" + pCells[1].CellFormula;
				else if (pCells[1].CellType == CellType.Numeric)
				{
					if (DateUtil.IsCellDateFormatted(pCells[1]))
					{
						return pCells[1].DateCellValue.ToString();
					}
					else
					{
						return pCells[1].NumericCellValue.ToString();
					}
				}
				else
					return pCells[1].StringCellValue;
			}
			catch (Exception)
			{
				_errorHandler.SetErrCod(7);
				_errorHandler.SetErrDes("Invalid cell value");
			}
			return null;
		}

		public decimal GetValue()
		{
			decimal value = 0;
			try
			{
				CellType cType = pCells[1].CellType;
				switch (cType)
				{
					case CellType.Formula:
						string type = GetFormulaType();
						if (type.Equals("N"))
							value = new decimal(pCells[1].NumericCellValue);
						else if (type.Equals("D"))
							value = new decimal(GetDate().ToOADate());
						break;
					case CellType.Boolean:
						bool b = pCells[1].BooleanCellValue;
						value = new decimal((b) ? 1 : 0);
						break;
					default:
						value = new decimal(pCells[1].NumericCellValue);
						break;
				}
			}
			catch (Exception)
			{
				throw new ExcelException(7, "Invalid cell value");
			}
			return value;
		}

		public string GetCellType()
		{
			string type;
			switch (pCells[1].CellType)
			{
				case CellType.Blank:
					type = "U";
					break;
				case CellType.Boolean:
					type = "N";
					break;
				case CellType.Error:
					type = "U";
					break;
				case CellType.Formula:
					type = GetFormulaType();
					break;
				case CellType.Numeric:
					if (DateUtil.IsCellDateFormatted(pCells[1]))
					{
						type = "D";
					}
					else
					{
						type = "N";
					}
					break;
				case CellType.String:
					type = "C";
					break;
				default:
					type = string.Empty;
					break;
			}
			return type;
		}

		private string GetFormulaType()
		{
			try
			{
				DataFormatter formatter = new DataFormatter();

				FormatBase format = formatter.GetDefaultFormat(pCells[1]);
				if (format.GetType() == typeof(System.Globalization.DateTimeFormatInfo))
				{
					return "D";
				}
				else
				{
					return "N";
				}
			}
			catch (Exception)
			{
				try
				{
					DateTime dVal = pCells[1].DateCellValue;
					if (dVal != null)
					{
						return "D";
					}
				}
				catch (Exception)
				{
				}
			}
			string sVal = string.Empty;
			try
			{
				sVal = pCells[1].StringCellValue;
			}
			catch (Exception)
			{
			}
			if (!string.IsNullOrEmpty(sVal))
			{
				return "C";
			}
			else
			{
				return "U";
			}
		}

		public double GetSize()
		{
			return pWorkbook.GetFontAt(pCells[1].CellStyle.FontIndex).FontHeightInPoints;
		}

		public void SetSize(double value)
		{
			CheckReadonlyDocument();

			try
			{
				for (int i = 1; i <= cellCount; i++)
				{
					ICellStyle cellStyle = pCells[1].CellStyle;
					XSSFFont fontCell = (XSSFFont)pWorkbook.GetFontAt(cellStyle.FontIndex);
					XSSFCellStyle newStyle = null;
					XSSFFont newFont = null;

					if (fontCell.FontHeightInPoints != value)
					{
						newFont = GetInternalFont(fontCell.IsBold, fontCell.Color, (short)value,
							fontCell.FontName, fontCell.IsItalic, fontCell.IsStrikeout,
							fontCell.TypeOffset, fontCell.Underline);
						CopyPropertiesFont(newFont, fontCell);

						newFont.FontHeightInPoints = (short)value;

						newStyle = stylesCache.GetCellStyle(newFont);
						CopyPropertiesStyle(newStyle, cellStyle);

						newStyle.SetFont(newFont);
						pCells[1].CellStyle = newStyle;
					}
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, "SetSize error", ex);
			}
		}

		public string GetFont()
		{
			return pWorkbook.GetFontAt(pCells[1].CellStyle.FontIndex).FontName;
		}

		protected XSSFFont GetInternalFont(bool bold, short color, double fontHeight, string name, bool italic,
			bool strikeout, FontSuperScript typeOffset, FontUnderlineType underline)
		{
			IFont font = pWorkbook.FindFont(bold, color, (short)fontHeight, name, italic, strikeout, typeOffset, underline);
			if (font == null)
			{
				font = pWorkbook.CreateFont();
			}
			return (XSSFFont)font;
		}

		public void SetFont(string value)
		{
			CheckReadonlyDocument();

			try
			{
				for (int i = 1; i <= cellCount; i++)
				{
					ICellStyle cellStyle = pCells[i].CellStyle;
					IFont fontCell = pWorkbook.GetFontAt(cellStyle.FontIndex);
					XSSFCellStyle newStyle = null;
					XSSFFont newFont = null;

					if (!fontCell.FontName.Equals(value))
					{
						newFont = GetInternalFont(fontCell.IsBold, fontCell.Color, (short)fontCell.FontHeight, value,
							fontCell.IsItalic, fontCell.IsStrikeout, fontCell.TypeOffset,
							fontCell.Underline);
						CopyPropertiesFont(newFont, fontCell);

						newFont.FontName = value;

						newStyle = stylesCache.GetCellStyle(newFont);
						CopyPropertiesStyle(newStyle, cellStyle);

						newStyle.SetFont(newFont);
						pCells[i].CellStyle = newStyle;
					}
				}
			}
			catch (Exception)
			{
				throw new ExcelException(7, "Invalid cell value");
			}
		}
		public short GetBold()
		{
			if (pWorkbook.GetFontAt(pCells[1].CellStyle.FontIndex).IsBold)
			{
				return 1;
			}
			return 0;
		}

		public void SetBold(short value)
		{
			CheckReadonlyDocument();

			try
			{
				for (int i = 1; i <= cellCount; i++)
				{
					ICellStyle cellStyle = pCells[i].CellStyle;
					IFont fontCell = pWorkbook.GetFontAt(cellStyle.FontIndex);
					XSSFCellStyle newStyle = null;
					XSSFFont newFont = null;

					switch (value)
					{
						case 0:
							if (fontCell.IsBold)
							{
								newFont = GetInternalFont(true, fontCell.Color, (short)fontCell.FontHeight,
									fontCell.FontName, fontCell.IsItalic, fontCell.IsStrikeout,
									fontCell.TypeOffset, fontCell.Underline);
								CopyPropertiesFont(newFont, fontCell);
								newFont.IsBold = true;

								newStyle = stylesCache.GetCellStyle(newFont);
								CopyPropertiesStyle(newStyle, cellStyle);

								newStyle.SetFont(newFont);
								pCells[i].CellStyle = newStyle;
							}
							break;
						case 1:
							if (!fontCell.IsBold)
							{
								newFont = GetInternalFont(true, fontCell.Color, (short)fontCell.FontHeight,
									fontCell.FontName, fontCell.IsItalic, fontCell.IsStrikeout,
									fontCell.TypeOffset, fontCell.Underline);
								CopyPropertiesFont(newFont, fontCell);
								newFont.IsBold = true;

								newStyle = stylesCache.GetCellStyle(newFont);
								CopyPropertiesStyle(newStyle, cellStyle);

								newStyle.SetFont(newFont);
								pCells[i].CellStyle = newStyle;
							}
							break;
						default:
							throw new ExcelException(6, "Invalid font properties");
					}
				}
			}
			catch (Exception)
			{
				throw new ExcelException(6, "Invalid bold value");
			}
		}

		public short GetItalic()
		{
			if (pWorkbook.GetFontAt(pCells[1].CellStyle.FontIndex).IsItalic)
			{
				return 1;
			}
			return 0;
		}

		public void SetItalic(short value)
		{
			CheckReadonlyDocument();

			try
			{
				for (int i = 1; i <= cellCount; i++)
				{
					ICellStyle cellStyle = pCells[i].CellStyle;
					IFont fontCell = pWorkbook.GetFontAt(cellStyle.FontIndex);
					XSSFCellStyle newStyle = null;
					XSSFFont newFont = null;

					switch (value)
					{
						case 0:
							if (fontCell.IsItalic)
							{
								newFont = GetInternalFont(fontCell.IsBold, fontCell.Color, fontCell.FontHeight,
									fontCell.FontName, false, fontCell.IsStrikeout, fontCell.TypeOffset,
									fontCell.Underline);
								CopyPropertiesFont(newFont, fontCell);
								newFont.IsItalic = false;

								newStyle = stylesCache.GetCellStyle(newFont);
								CopyPropertiesStyle(newStyle, cellStyle);

								newStyle.SetFont(newFont);
								pCells[i].CellStyle = newStyle;
							}
							break;
						case 1:
							if (!fontCell.IsItalic)
							{
								newFont = GetInternalFont(fontCell.IsBold, fontCell.Color, fontCell.FontHeight,
									fontCell.FontName, true, fontCell.IsStrikeout, fontCell.TypeOffset,
									fontCell.Underline);
								CopyPropertiesFont(newFont, fontCell);
								newFont.IsItalic = true;

								newStyle = stylesCache.GetCellStyle(newFont);
								CopyPropertiesStyle(newStyle, cellStyle);

								newStyle.SetFont(newFont);
								pCells[i].CellStyle = newStyle;
							}
							break;
						default:
							throw new ExcelException(6, "Invalid font properties");
					}
				}
			}
			catch (Exception)
			{
				throw new ExcelException(6, "Invalid font properties");
			}
		}
		public short GetUnderline()
		{
			if (pWorkbook.GetFontAt(pCells[1].CellStyle.FontIndex).Underline != FontUnderlineType.None)
			{
				return 1;
			}
			return 0;
		}

		public void SetUnderline(short value)
		{
			CheckReadonlyDocument();

			try
			{
				for (int i = 1; i <= cellCount; i++)
				{
					ICellStyle cellStyle = pCells[i].CellStyle;
					IFont fontCell = pWorkbook.GetFontAt(cellStyle.FontIndex);
					XSSFCellStyle newStyle = null;
					XSSFFont newFont = null;

					switch (value)
					{
						case 0:
							if (fontCell.Underline != FontUnderlineType.None)
							{
								newFont = GetInternalFont(fontCell.IsBold, fontCell.Color, fontCell.FontHeight,
									fontCell.FontName, fontCell.IsItalic, fontCell.IsStrikeout,
									fontCell.TypeOffset, FontUnderlineType.None);
								CopyPropertiesFont(newFont, fontCell);

								newFont.Underline = FontUnderlineType.None;

								newStyle = stylesCache.GetCellStyle(newFont);
								CopyPropertiesStyle(newStyle, cellStyle);

								newStyle.SetFont(newFont);
								pCells[i].CellStyle = newStyle;
							}
							break;
						case 1:
							if (fontCell.Underline != FontUnderlineType.Single)
							{
								newFont = GetInternalFont(fontCell.IsBold, fontCell.Color, fontCell.FontHeight,
									fontCell.FontName, fontCell.IsItalic, fontCell.IsStrikeout,
									fontCell.TypeOffset, FontUnderlineType.Single);
								CopyPropertiesFont(newFont, fontCell);

								newFont.Underline = FontUnderlineType.Single;

								newStyle = stylesCache.GetCellStyle(newFont);
								CopyPropertiesStyle(newStyle, cellStyle);

								newStyle.SetFont(newFont);
								pCells[i].CellStyle = newStyle;
							}
							break;
						default:
							throw new ExcelException(6, "Invalid font property");
					}
				}
			}
			catch (Exception)
			{
				throw new ExcelException(6, "Invalid font properties");
			}
		}

		public long GetColor()
		{
			return pWorkbook.GetFontAt(pCells[1].CellStyle.FontIndex).Color - 7;
		}

		public void SetColor(short value)
		{
			SetColor((long)value);
		}

		public void SetColor(int value)
		{
			SetColor((long)value);
		}

		// This version optimizes the existing color palette in the spreadsheet.
		// It searches for similar colors and if found, it uses them to avoid reloading
		// the color palette, which has a maximum of 40h-10h positions.
		public void SetColor(long value)
		{
			CheckReadonlyDocument();

			try
			{
				for (int i = 1; i <= cellCount; i++)
				{
					XSSFCellStyle cellStyle = (XSSFCellStyle)pCells[i].CellStyle;
					XSSFFont fontCell = (XSSFFont)pWorkbook.GetFontAt(cellStyle.FontIndex);
					XSSFCellStyle newStyle = null;
					XSSFFont newFont = null;
					XSSFColor newColor = null;

					XSSFColor fontColor = fontCell.GetXSSFColor();

					int val = (int)value;
					int red = val >> 16 & 0xff;
					int green = val >> 8 & 0xff;
					int blue = val & 0xff;

					if (red != 0 || green != 0 || blue > 56)
					{
						if ((fontColor != null && (fontColor.GetRgb() == null || (fontColor.GetRgb()[0] == 0
								&& fontColor.GetRgb()[1] == 0 && fontColor.GetRgb()[2] == 0))))
						{
							if ((red + green + blue) != 0)
							{
								newColor = new XSSFColor(new byte[] { (byte)red, (byte)green, (byte)blue });

								newFont = (XSSFFont)pWorkbook.CreateFont();
								CopyPropertiesFont(newFont, fontCell);

								newFont.SetColor(newColor);

								newStyle = (XSSFCellStyle)pWorkbook.CreateCellStyle();
								CopyPropertiesStyle(newStyle, cellStyle);

								newStyle.SetFont(newFont);
								pCells[i].CellStyle = newStyle;
							}
						}
						else
						{
							if (fontColor != null)
							{
								byte[] triplet = fontColor.GetRgb();

								if (triplet[0] != red || triplet[1] != green || triplet[2] != blue)
								{

									newColor = new XSSFColor(new byte[] { (byte)red, (byte)green, (byte)blue });

									newFont = (XSSFFont)pWorkbook.CreateFont();
									CopyPropertiesFont(newFont, fontCell);

									newFont.SetColor(newColor);

									newStyle = (XSSFCellStyle)pWorkbook.CreateCellStyle();
									CopyPropertiesStyle(newStyle, cellStyle);

									newStyle.SetFont(newFont);
									pCells[i].CellStyle = newStyle;
								}
							}
						}
					}
					else
					{
						value = value + 7;
						if (fontColor != null)
						{
							if (fontColor.Indexed != value)
							{
								newFont = GetInternalFont(fontCell.IsBold, (short)value,
									fontCell.FontHeight, fontCell.FontName, fontCell.IsItalic,
									fontCell.IsStrikeout, fontCell.TypeOffset, fontCell.Underline);
								CopyPropertiesFont(newFont, fontCell);

								newFont.Color = (short)value;

								newStyle = stylesCache.GetCellStyle(newFont);
								CopyPropertiesStyle(newStyle, cellStyle);

								newStyle.SetFont(newFont);
								pCells[i].CellStyle = newStyle;
							}
						}
						else
						{
							newFont = GetInternalFont(fontCell.IsBold, (short)value,
								fontCell.FontHeight, fontCell.FontName, fontCell.IsItalic,
								fontCell.IsStrikeout, fontCell.TypeOffset, fontCell.Underline);
							CopyPropertiesFont(newFont, fontCell);

							newFont.Color = (short)value;

							newStyle = stylesCache.GetCellStyle(newFont);
							CopyPropertiesStyle(newStyle, cellStyle);

							newStyle.SetFont(newFont);
							pCells[i].CellStyle = newStyle;
						}
					}
				}
			}
			catch (Exception e)
			{
				throw new ExcelException(6, "Invalid font properties", e);
			}
		}

		protected void CopyPropertiesStyle(XSSFCellStyle dest, ICellStyle source)
		{
			dest.CloneStyleFrom(source);
		}

		protected void CopyPropertiesFont(XSSFFont dest, IFont source)
		{
			dest.FontHeightInPoints = source.FontHeightInPoints;
			dest.FontName = source.FontName;
			dest.IsBold = source.IsBold;
			dest.IsItalic = source.IsItalic;
			dest.Underline = source.Underline;
			dest.Color = source.Color;
		}

		private void FitColumnWidth(int i, int data)
		{
			if (fitColumnWidth)
			{
				int colW = pSelectedSheet.GetColumnWidth((int)(i + colStartIdx - 1));
				if ((256 * data) > colW)
				{
					colW = (short)(256 * data);
				}
				pSelectedSheet.SetColumnWidth((short)(i + colStartIdx - 1), colW);
			}
		}

		public void SetFitColumnWidth(bool fitCol)
		{
			fitColumnWidth = fitCol;
		}

		public bool GetFitColumnWidth()
		{
			return fitColumnWidth;
		}


		public int GetRowStart()
		{
			return rowStartIdx + 1;
		}

		public int GetRowEnd()
		{
			return rowEndIdx + 1;
		}

		public int GetColumnStart()
		{
			return colStartIdx + 1;
		}

		public int GetColumnEnd()
		{
			return colEndIdx + 1;
		}

		public string GetCellAdress()
		{
			return null;
		}

		public string GetValueType()
		{
			return this.GetCellType();
		}

		public decimal GetNumericValue()
		{
			try
			{
				return this.GetNumber();
			}
			catch (ExcelException e)
			{
				_errorHandler.SetErrCod((short)e.ErrorCode);
				_errorHandler.SetErrDes(e.ErrorDescription);
			}
			return new decimal(0);
		}

		public DateTime GetDateValue()
		{
			try
			{
				return this.GetDate();
			}
			catch (ExcelException e)
			{
				_errorHandler.SetErrCod((short)e.ErrorCode);
				_errorHandler.SetErrDes(e.ErrorDescription);
			}
			return DateTimeUtil.NullDate();
		}

		public bool SetText(string value)
		{
			try
			{
				return this.SetTextImpl(value);
			}
			catch (ExcelException e)
			{
				_errorHandler.SetErrCod((short)e.ErrorCode);
				_errorHandler.SetErrDes(e.ErrorDescription);
			}
			return false;
		}

		public bool SetNumericValue(decimal d)
		{
			try
			{
				return this.SetNumber(Convert.ToDouble(d));
			}
			catch (ExcelException e)
			{
				_errorHandler.SetErrCod((short)e.ErrorCode);
				_errorHandler.SetErrDes(e.ErrorDescription);
			}
			return false;
		}
		public bool SetDateValue(DateTime value)
		{
			try
			{
				return this.SetDate(value);
			}
			catch (ExcelException e)
			{
				_errorHandler.SetErrCod((short)e.ErrorCode);
				_errorHandler.SetErrDes(e.ErrorDescription);
			}
			return false;
		}

		public bool Empty()
		{
			for (int i = 1; i <= cellCount; i++)
			{
				pCells[i].SetCellValue(string.Empty);
			}
			return this.cellCount > 0;
		}

		public bool MergeCells()
		{
			CellRangeAddress cellRange = new CellRangeAddress(rowStartIdx, rowEndIdx, colStartIdx, colEndIdx);
			for (int i = 0; i < pSelectedSheet.NumMergedRegions; i++)
			{
				CellRangeAddress mergedRegion = pSelectedSheet.GetMergedRegion(i);
				if (cellRange.Intersects(mergedRegion))
				{
					pSelectedSheet.RemoveMergedRegion(i);
				}
			}
			pSelectedSheet.AddMergedRegion(cellRange);
			return true;
		}

		public ExcelStyle GetCellStyle()
		{
			return cellStyle;
		}

		public bool SetCellStyle(ExcelStyle newCellStyle)
		{
			if (cellCount > 0)
			{
				XSSFCellStyle style = (XSSFCellStyle)pWorkbook.CreateCellStyle();

				style.CloneStyleFrom(style);
				ApplyNewCellStyle(style, newCellStyle);
				for (int i = 1; i <= cellCount; i++)
				{
					pCells[i].CellStyle = style;
				}
			}
			return cellCount > 0;
		}

		private XSSFColor ToColor(ExcelColor color)
		{
			return new XSSFColor(new byte[] { (byte)color.Red, (byte)color.Green, (byte)color.Blue });
		}
		private XSSFCellStyle ApplyNewCellStyle(XSSFCellStyle cellStyle, ExcelStyle newCellStyle)
		{
			ExcelFont cellFont = newCellStyle.GetCellFont();
			if (cellFont != null && cellFont.IsDirty())
			{
				XSSFFont cellStyleFont = (XSSFFont)pWorkbook.CreateFont();
				cellStyle.SetFont(cellStyleFont);
				ExcelFont font = newCellStyle.GetCellFont();
				if (font != null)
				{
					if (font.GetBold())
					{
						cellStyleFont.IsBold = font.GetBold();
					}
					if (font.GetFontFamily() != null && font.GetFontFamily().Length > 0)
					{
						cellStyleFont.FontName = font.GetFontFamily();
					}
					if (font.GetItalic())
					{
						cellStyleFont.IsItalic = font.GetItalic();
					}
					if (font.GetStrike())
					{
						cellStyleFont.IsStrikeout = font.GetStrike();
					}
					cellStyleFont.FontHeight = font.GetSize();

					if (font.GetUnderline())
					{
						cellStyleFont.Underline = font.GetUnderline() ? FontUnderlineType.Single : FontUnderlineType.None;
					}
					if (font.GetColor() != null && font.GetColor().IsDirty())
					{
						cellStyleFont.SetColor(ToColor(font.GetColor()));
					}
				}
			}
			ExcelFill cellfill = newCellStyle.GetCellFill();
			if (cellfill != null && cellfill.GetCellBackColor() != null && cellfill.GetCellBackColor().IsDirty())
			{
				cellStyle.SetFillForegroundColor(ToColor(cellfill.GetCellBackColor()));
				cellStyle.FillPattern = FillPattern.SolidForeground;
			}

			ExcelAlignment alignment = newCellStyle.GetCellAlignment();
			if (alignment != null && alignment.IsDirty())
			{
				if (alignment.GetHorizontalAlignment() != 0)
				{
					HorizontalAlignment align;
					switch (alignment.GetHorizontalAlignment())
					{
						case ExcelAlignment.HORIZONTAL_ALIGN_CENTER:
							align = HorizontalAlignment.Center;
							break;
						case ExcelAlignment.HORIZONTAL_ALIGN_LEFT:
							align = HorizontalAlignment.Left;
							break;
						case ExcelAlignment.HORIZONTAL_ALIGN_RIGHT:
							align = HorizontalAlignment.Right;
							break;
						default:
							align = (HorizontalAlignment)alignment.GetHorizontalAlignment();
							break;
					}
					cellStyle.Alignment = align;
				}
				if (alignment.GetVerticalAlignment() != 0)
				{
					VerticalAlignment align;
					switch (alignment.GetHorizontalAlignment())
					{
						case ExcelAlignment.VERTICAL_ALIGN_BOTTOM:
							align = VerticalAlignment.Bottom;
							break;
						case ExcelAlignment.VERTICAL_ALIGN_MIDDLE:
							align = VerticalAlignment.Center;
							break;
						case ExcelAlignment.VERTICAL_ALIGN_TOP:
							align = VerticalAlignment.Top;
							break;
						default:
							align = (VerticalAlignment)alignment.GetHorizontalAlignment();
							break;
					}
					cellStyle.VerticalAlignment = align;
				}
			}

			if (newCellStyle.IsLocked())
			{
				cellStyle.IsLocked = newCellStyle.IsLocked();
			}

			if (newCellStyle.IsHidden())
			{
				cellStyle.IsHidden = newCellStyle.IsHidden();
			}

			if (newCellStyle.GetShrinkToFit())
			{
				cellStyle.ShrinkToFit = newCellStyle.GetShrinkToFit();
			}

			if (newCellStyle.GetWrapText())
			{
				cellStyle.WrapText = newCellStyle.GetWrapText();
			}

			if (newCellStyle.GetTextRotation() != 0)
			{
				cellStyle.Rotation = (short)newCellStyle.GetTextRotation();
			}

			if (newCellStyle.GetIndentation() >= 0)
			{
				cellStyle.Indention = (short)newCellStyle.GetIndentation();
			}

			if (newCellStyle.GetDataFormat() != null && newCellStyle.GetDataFormat().Length > 0)
			{
				cellStyle.DataFormat = pWorkbook.CreateDataFormat().GetFormat(newCellStyle.GetDataFormat());
			}

			if (newCellStyle.GetBorder() != null)
			{
				ExcelCellBorder cellBorder = newCellStyle.GetBorder();
				ApplyBorderSide(cellStyle, BorderCellSide.TOP, cellBorder.GetBorderTop());
				ApplyBorderSide(cellStyle, BorderCellSide.BOTTOM, cellBorder.GetBorderBottom());
				ApplyBorderSide(cellStyle, BorderCellSide.LEFT, cellBorder.GetBorderLeft());
				ApplyBorderSide(cellStyle, BorderCellSide.RIGHT, cellBorder.GetBorderRight());

				bool hasDiagonalUp = cellBorder.GetBorderDiagonalUp() != null && cellBorder.GetBorderDiagonalUp().IsDirty();
				bool hasDiagonalDown = cellBorder.GetBorderDiagonalDown() != null && cellBorder.GetBorderDiagonalDown().IsDirty();
				if (hasDiagonalUp || hasDiagonalDown)
				{
					CT_Xf _cellXf = cellStyle.GetCoreXf();
					ExcelBorder border = (hasDiagonalUp) ? cellBorder.GetBorderDiagonalUp() : cellBorder.GetBorderDiagonalDown();
					XSSFColor diagonalColor = ToColor(border.GetBorderColor());
					BorderStyle.TryParse(border.GetBorder(), out BorderStyle borderStyle);
					SetBorderDiagonal(borderStyle, diagonalColor, this.pWorkbook.GetStylesSource(), _cellXf, hasDiagonalUp, hasDiagonalDown);
				}
			}
			return cellStyle;
		}

		private static CT_Border GetCTBorder(StylesTable _stylesSource, CT_Xf _cellXf)
		{
			CT_Border ct;
			if (_cellXf.applyBorder)
			{
				int idx = (int)_cellXf.borderId;
				XSSFCellBorder cf = _stylesSource.GetBorderAt(idx);
				ct = (CT_Border)cf.GetCTBorder().Copy();
			}
			else
			{
				ct = new CT_Border();
			}
			return ct;
		}

		public static void SetBorderDiagonal(BorderStyle border, XSSFColor color, StylesTable _stylesSource, CT_Xf _cellXf, bool up, bool down)
		{
			CT_Border ct = GetCTBorder(_stylesSource, _cellXf);
			CT_BorderPr pr = ct.IsSetDiagonal() ? ct.diagonal : ct.AddNewDiagonal();

			ct.diagonalDown = down;
			ct.diagonalUp = up;
			pr.style = ToStBorderStyle(border);
			pr.color = ConvertToCTColor(color);

			int idx = _stylesSource.PutBorder(new XSSFCellBorder(ct));
			_cellXf.borderId = (uint)idx;
			_cellXf.applyBorder = true;
		}
		static CT_Color ConvertToCTColor(XSSFColor xssfColor)
		{
			CT_Color ctColor = new CT_Color();

			if (xssfColor != null)
			{
				ctColor = new CT_Color();

				if (xssfColor.IsRGB)
				{
					byte[] rgb = xssfColor.RGB;
					ctColor.rgb = rgb;
				}
				else if (xssfColor.IsIndexed)
				{
					ctColor.indexed = (uint)xssfColor.Indexed;
				}
				else if (xssfColor.IsThemed)
				{
					ctColor.theme = (uint)xssfColor.Theme;
				}
				else if (xssfColor.IsAuto)
				{
					ctColor.auto = true;
				}
			}

			return ctColor;
		}
		private static ST_BorderStyle ToStBorderStyle(BorderStyle borderStyle)
		{
			ST_BorderStyle stBorderStyle;

			switch (borderStyle)
			{
				case BorderStyle.None:
					stBorderStyle = ST_BorderStyle.none;
					break;
				case BorderStyle.Thin:
					stBorderStyle = ST_BorderStyle.thin;
					break;
				case BorderStyle.Medium:
					stBorderStyle = ST_BorderStyle.medium;
					break;
				case BorderStyle.Dashed:
					stBorderStyle = ST_BorderStyle.dashed;
					break;
				case BorderStyle.Dotted:
					stBorderStyle = ST_BorderStyle.dotted;
					break;
				case BorderStyle.Thick:
					stBorderStyle = ST_BorderStyle.thick;
					break;
				case BorderStyle.Double:
					stBorderStyle = ST_BorderStyle.@double;
					break;
				case BorderStyle.Hair:
					stBorderStyle = ST_BorderStyle.hair;
					break;
				case BorderStyle.MediumDashed:
					stBorderStyle = ST_BorderStyle.mediumDashed;
					break;
				case BorderStyle.DashDot:
					stBorderStyle = ST_BorderStyle.dashDot;
					break;
				case BorderStyle.MediumDashDot:
					stBorderStyle = ST_BorderStyle.mediumDashDot;
					break;
				case BorderStyle.DashDotDot:
					stBorderStyle = ST_BorderStyle.dashDotDot;
					break;
				case BorderStyle.MediumDashDotDot:
					stBorderStyle = ST_BorderStyle.mediumDashDotDot;
					break;
				case BorderStyle.SlantedDashDot:
					stBorderStyle = ST_BorderStyle.slantDashDot;
					break;
				default:
					stBorderStyle = ST_BorderStyle.none; //Default value
					break;
			}
			return stBorderStyle;

		}
		private void ApplyBorderSide(XSSFCellStyle cellStyle, BorderCellSide bSide, ExcelBorder border)
		{
			if (border != null && border.IsDirty())
			{
				if (border.GetBorderColor().IsDirty())
				{
					try
					{
						BorderSide borderSide = (BorderSide)Enum.Parse(typeof(BorderSide), bSide.ToString());
						cellStyle.SetBorderColor(borderSide, ToColor(border.GetBorderColor()));
					}
					catch (ArgumentException) { }
				}
				if (border.GetBorder() != null && border.GetBorder().Length > 0)
				{
					BorderStyle bs = ConvertToBorderStyle(border.GetBorder());
					if (bSide == BorderCellSide.BOTTOM)
					{
						cellStyle.BorderBottom = bs;
					}
					else if (bSide == BorderCellSide.TOP)
					{
						cellStyle.BorderTop = bs;
					}
					else if (bSide == BorderCellSide.LEFT)
					{
						cellStyle.BorderLeft = bs;
					}
					else if (bSide == BorderCellSide.RIGHT)
					{
						cellStyle.BorderRight = bs;
					}
				}
			}
		}
		internal BorderStyle ConvertToBorderStyle(string style)
		{
			BorderStyle borderStyle;
			if (Enum.TryParse(style, true, out borderStyle))
			{
				return borderStyle;
			}
			else
			{
				throw new ArgumentException("Invalid border style: " + style);
			}
		}
		internal enum BorderCellSide
		{
			RIGHT, LEFT, TOP, BOTTOM, DIAGONALUP, DIAGONALDOWN
		}

	}
}