using System;
using System.Collections.Generic;
using System.IO;
using GeneXus.Application;
using GeneXus.MSOffice.Excel.exception;
using GeneXus.MSOffice.Excel.poi.xssf;
using log4net;
using NPOI.SS.UserModel;
using NPOI.Util;
using NPOI.XSSF.UserModel;

namespace GeneXus.MSOffice.Excel.Poi.Xssf
{
	public class ExcelSpreadsheet : IExcelSpreadsheet
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(ExcelSpreadsheet));
		private XSSFWorkbook _workbook;
		private string _documentFileName;
		private bool _autoFitColumnsOnSave = false;
		private IGXError _errorHandler;
		private StylesCache _stylesCache;
		internal static string DefaultExtension = ".xlsx";
		public ExcelSpreadsheet(IGXError errHandler, string fileName, string template)
		{
			_errorHandler = errHandler;
			if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
			{
				fileName += DefaultExtension;
			}

			if (!string.IsNullOrEmpty(template))
			{
				FileInfo templateFile = new FileInfo(template);
				if (templateFile.Exists)
				{
					_workbook = new XSSFWorkbook(template);
				}
				else
				{
					throw new ExcelTemplateNotFoundException();
				}
			}
			else
			{
				FileInfo file = new FileInfo(fileName);
				if (file.Exists)
				{
					_workbook = new XSSFWorkbook(file);
				}
				else
				{
					_workbook = new XSSFWorkbook();
				}
			}

			_documentFileName = fileName;

			_stylesCache = new StylesCache(_workbook);
		}

		public bool GetAutoFit()
		{
			return _autoFitColumnsOnSave;
		}

		public void SetAutofit(bool autoFitColumnsOnSave)
		{
			this._autoFitColumnsOnSave = autoFitColumnsOnSave;
		}

		public bool Save()
		{
			return SaveAsImpl(_documentFileName);
		}

		private bool SaveAsImpl(string fileName)
		{
			AutoFitColumns();
			RecalculateFormulas();

			try
			{
				ByteArrayOutputStream fs = new ByteArrayOutputStream();
				_workbook.Write(fs);

				ByteArrayInputStream inStream = new ByteArrayInputStream(fs.ToByteArray());
				fs.Close();
				_workbook.Close();

				GxFile file = new GxFile(GxContext.StaticPhysicalPath(), fileName, GxFileType.Private);
				file.Create(inStream);
				int errCode = file.ErrCode;
				inStream.Close();
				file.Close();
				return errCode == 0;
			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, "Save error", ex);
				return false;
			}
		}

		public bool SaveAs(string newFileName)
		{
			return SaveAsImpl(newFileName);
		}

		public bool Close()
		{
			return Save();
		}

		public IExcelCellRange GetCells(IExcelWorksheet worksheet, int startRow, int startCol, int rowCount, int colCount)
		{
			return new ExcelCells(_errorHandler, this, _workbook, (XSSFSheet)_workbook.GetSheet(worksheet.GetName()), startRow - 1, startCol - 1, rowCount, colCount, false, _stylesCache);
		}

		public IExcelCellRange GetCell(IExcelWorksheet worksheet, int startRow, int startCol)
		{
			return GetCells(worksheet, startRow, startCol, 1, 1);
		}


		public bool InsertRow(IExcelWorksheet worksheet, int rowIdx, int rowCount)
		{
			XSSFSheet sheet = (XSSFSheet)GetSheet(worksheet);

			int createNewRowAt = rowIdx; // Add the new row between row 9 and 10

			if (sheet != null)
			{
				for (int i = 1; i <= rowCount; i++)
				{
					int lastRow = Math.Max(0, sheet.LastRowNum);
					if (lastRow < rowIdx)
					{
						for (int j = lastRow; j <= rowIdx; j++)
						{
							sheet.CreateRow(j);
						}
					}
					else
					{
						if (sheet.GetRow(createNewRowAt) == null)
						{
							sheet.CreateRow(createNewRowAt);
						}
						sheet.ShiftRows(createNewRowAt, lastRow, 1, true, false);
					}
				}
				return true;
			}
			return false;
		}

		public bool InsertColumn(IExcelWorksheet worksheet, int colIdx, int colCount)
		{
			/*
			 * XSSFSheet sheet = GetSheet(worksheet); int createNewColumnAt = colIdx; //Add
			 * the new row between row 9 and 10
			 *
			 * if (sheet != null) { for (int i = 1; i<= colCount; i++) {
			 *
			 * int lastRow = sheet.getLastRowNum(); sheet.shi(createNewColumnAt, lastRow, 1,
			 * true, false); XSSFRow newRow = sheet.createRow(createNewColumnAt); } return
			 * true; } return false;
			 */
			return false; // POI not supported
		}

		public bool DeleteRow(IExcelWorksheet worksheet, int rowIdx)
		{
			XSSFSheet sheet = GetSheet(worksheet);
			if (sheet != null)
			{
				XSSFRow row = (XSSFRow)sheet.GetRow(rowIdx);
				if (row != null)
				{
					sheet.RemoveRow(row);
				}
				int rowIndex = rowIdx;
				int lastRowNum = sheet.LastRowNum;
				if (rowIndex >= 0 && rowIndex < lastRowNum)
				{
					sheet.ShiftRows(rowIndex + 1, lastRowNum, -1);
				}
			}
			return sheet != null;
		}

		public List<ExcelWorksheet> GetWorksheets()
		{
			List<ExcelWorksheet> list = new List<ExcelWorksheet>();
			for (int i = 0; i < _workbook.NumberOfSheets; i++)
			{
				XSSFSheet sheet = (XSSFSheet)_workbook.GetSheetAt(i);
				if (sheet != null)
				{
					list.Add(new ExcelWorksheet(sheet));
				}
			}
			return list;
		}

		public bool InsertWorksheet(string newSheetName, int idx)
		{
			XSSFSheet newSheet;
			if (_workbook.GetSheet(newSheetName) == null)
			{
				newSheet = (XSSFSheet)_workbook.CreateSheet(newSheetName);
			}
			else
			{
				throw new ExcelException(13, "The workbook already contains a sheet named:" + newSheetName);
			}
			return newSheet != null;
		}

		public bool CloneSheet(string sheetName, string newSheetName)
		{
			int idx = _workbook.GetSheetIndex(sheetName);
			if (_workbook.GetSheet(newSheetName) != null)
			{
				throw new ExcelException(13, "The workbook already contains a sheet named:" + newSheetName);
			}
			if (idx < 0)
			{
				throw new ExcelException(14, "The workbook does not contain a sheet named:" + sheetName);
			}
			_workbook.CloneSheet(idx, newSheetName);
			return true;
		}

		private XSSFSheet GetSheet(IExcelWorksheet sheet)
		{
			return (XSSFSheet)_workbook.GetSheet(sheet.GetName());
		}

		private void RecalculateFormulas()
		{
			try
			{
				_workbook.GetCreationHelper().CreateFormulaEvaluator().EvaluateAll();
				_workbook.SetForceFormulaRecalculation(true);
			}
			catch (Exception e)
			{
				logger.Error("recalculateFormulas", e);
			}
		}

		private void AutoFitColumns()
		{
			if (_autoFitColumnsOnSave)
			{
				int sheetsCount = _workbook.NumberOfSheets;
				for (int i = 0; i < sheetsCount; i++)
				{
					ISheet sheet = _workbook.GetSheetAt(i);
					int columnCount = 0;
					for (int j = 0; j <= sheet.LastRowNum; j++)
					{
						IRow row = sheet.GetRow(j);
						if (row != null)
						{
							columnCount = Math.Max(columnCount, row.LastCellNum);
						}
					}
					for (int j = 0; j < columnCount; j++)
					{
						sheet.AutoSizeColumn(j);
					}
				}
			}
		}

		public bool SetActiveWorkSheet(string name)
		{
			int idx = _workbook.GetSheetIndex(name);
			if (idx >= 0)
			{
				_workbook.GetSheetAt(idx).IsSelected = true;
				_workbook.SetActiveSheet(idx);
				_workbook.SetSelectedTab(idx);
			}
			return idx >= 0;
		}

		public ExcelWorksheet GetWorkSheet(string name)
		{
			XSSFSheet sheet = (XSSFSheet)_workbook.GetSheet(name);
			if (sheet != null)
				return new ExcelWorksheet(sheet);
			return null;
		}

		public bool GetAutofit()
		{
			return _autoFitColumnsOnSave;
		}

		public void SetColumnWidth(IExcelWorksheet worksheet, int colIdx, int width)
		{
			XSSFSheet sheet = (XSSFSheet)_workbook.GetSheet(worksheet.GetName());
			if (colIdx >= 1 && sheet != null && width <= 255)
			{
				sheet.SetColumnWidth(colIdx - 1, 256 * width);
			}
		}

		public void SetRowHeight(IExcelWorksheet worksheet, int rowIdx, int height)
		{
			XSSFSheet sheet = (XSSFSheet)_workbook.GetSheet(worksheet.GetName());
			if (rowIdx >= 1 && sheet != null)
			{
				rowIdx = rowIdx - 1;
				if (sheet.GetRow(rowIdx) == null)
				{
					sheet.CreateRow(rowIdx);
				}
				sheet.GetRow(rowIdx).HeightInPoints = (short)height;
			}
		}

		public bool DeleteColumn(IExcelWorksheet worksheet, int colIdx)
		{
			XSSFSheet sheet = (XSSFSheet)_workbook.GetSheet(worksheet.GetName());
			if (colIdx >= 0)
			{
				return DeleteColumnImpl(sheet, colIdx);
			}
			return false;
		}

		private bool DeleteColumnImpl(XSSFSheet sheet, int columnToDelete)
		{
			for (int rId = 0; rId <= sheet.LastRowNum; rId++)
			{
				IRow row = sheet.GetRow(rId);
				for (int cID = columnToDelete; row != null && cID <= row.LastCellNum; cID++)
				{
					ICell cOld = row.GetCell(cID);
					if (cOld != null)
					{
						row.RemoveCell(cOld);
					}
					ICell cNext = row.GetCell(cID + 1);
					if (cNext != null)
					{
						ICell cNew = row.CreateCell(cID, cNext.CellType);
						CloneCell(cNew, cNext);
						// Set the column width only on the first row.
						// Otherwise, the second row will overwrite the original column width set previously.
						if (rId == 0)
						{
							sheet.SetColumnWidth(cID, sheet.GetColumnWidth(cID + 1));
						}
					}
				}
			}
			return true;
		}

		private int GetNumberOfRows(XSSFSheet sheet)
		{
			int rowNum = sheet.LastRowNum + 1;
			return rowNum;
		}

		public int GetNrColumns(XSSFSheet sheet)
		{
			IRow headerRow = sheet.GetRow(0);
			return headerRow.LastCellNum;
		}

		public void InsertNewColumnBefore(XSSFSheet sheet, int columnIndex)
		{
			IFormulaEvaluator evaluator = _workbook.GetCreationHelper().CreateFormulaEvaluator();
			evaluator.ClearAllCachedResultValues();

			int nrRows = GetNumberOfRows(sheet);
			int nrCols = GetNrColumns(sheet);

			for (int row = 0; row < nrRows; row++)
			{
				IRow r = sheet.GetRow(row);

				if (r == null)
				{
					continue;
				}

				// Shift to the right
				for (int col = nrCols; col > columnIndex; col--)
				{
					ICell rightCell = r.GetCell(col);
					if (rightCell != null)
					{
						r.RemoveCell(rightCell);
					}

					ICell leftCell = r.GetCell(col - 1);

					if (leftCell != null)
					{
						ICell newCell = r.CreateCell(col, leftCell.CellType);
						CloneCell(newCell, leftCell);
					}
				}

				// Delete old column
				CellType cellType = CellType.Blank;

				ICell currentEmptyWeekCell = r.GetCell(columnIndex);
				if (currentEmptyWeekCell != null)
				{
					r.RemoveCell(currentEmptyWeekCell);
				}

				// Create new column
				r.CreateCell(columnIndex, cellType);
			}

			// Adjust the column widths
			for (int col = nrCols; col > columnIndex; col--)
			{
				sheet.SetColumnWidth(col, sheet.GetColumnWidth(col - 1));
			}
		}
		/*
		 * Takes an existing Cell and merges all the styles and formula into the new one
		 */
		private static void CloneCell(ICell cNew, ICell cOld)
		{
			cNew.CellComment = cOld.CellComment;
			cNew.CellStyle = cOld.CellStyle;

			switch (cOld.CellType)
			{
				case CellType.Boolean:
					{
						cNew.SetCellValue(cOld.BooleanCellValue);
						break;
					}
				case CellType.Numeric:
					{
						cNew.SetCellValue(cOld.NumericCellValue);
						break;
					}
				case CellType.String:
					{
						cNew.SetCellValue(cOld.StringCellValue);
						break;
					}
				case CellType.Error:
					{
						cNew.SetCellErrorValue(cOld.ErrorCellValue);
						break;
					}
				case CellType.Formula:
					{
						cNew.CellFormula = cOld.CellFormula;
						break;
					}
				default:
					// Ignore
					break;
			}
		}

		public bool DeleteSheet(int sheetIdx)
		{
			if (_workbook.NumberOfSheets > sheetIdx)
			{
				_workbook.RemoveSheetAt(sheetIdx);
				return true;
			}
			return false;
		}

		public bool DeleteSheet(string sheetName)
		{
			int sheetIndex = _workbook.GetSheetIndex(sheetName);
			if (sheetIndex >= 0)
			{
				_workbook.RemoveSheetAt(sheetIndex);
				return true;
			}
			return false;
		}

		public bool ToggleColumn(IExcelWorksheet worksheet, int colIdx, bool visible)
		{
			XSSFSheet sheet = _workbook.GetSheet(worksheet.GetName()) as XSSFSheet;
			if (sheet != null)
			{
				sheet.SetColumnHidden(colIdx, !visible);
				return true;
			}
			return false;
		}

		public bool ToggleRow(IExcelWorksheet worksheet, int i, bool visible)
		{
			XSSFSheet sheet = _workbook.GetSheet(worksheet.GetName()) as XSSFSheet;
			if (sheet != null)
			{
				IRow row = sheet.GetRow(i) as XSSFRow;
				if (row == null)
				{
					InsertRow(worksheet, i, 1);
					row = sheet.GetRow(i) as XSSFRow;
				}
				if (row != null)
				{
					ICellStyle style = _workbook.CreateCellStyle();
					style.IsHidden = !visible; 
					row.RowStyle = style;
					row.ZeroHeight = !visible;
				}
				return true;
			}
			return false;
		}
	}
}
