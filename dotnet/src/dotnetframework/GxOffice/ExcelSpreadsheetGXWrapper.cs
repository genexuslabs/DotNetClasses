using System;
using System.Collections.Generic;
using GeneXus.MSOffice.Excel.Poi.Xssf;
using log4net;

namespace GeneXus.MSOffice.Excel
{
	public class ExcelSpreadsheetGXWrapper : IGXError
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(ExcelSpreadsheetGXWrapper));
		private int _errCode;
		private string _errDescription = string.Empty;
		private IExcelWorksheet _currentWorksheet;
		private IExcelSpreadsheet _document;
		private bool _isReadonly = false;
		private bool _autofit = false;
		private const string DEFAULT_SHEET_NAME = "Sheet";

		public void SetAutofit(bool value)
		{
			_autofit = value;
			if (_document != null)
			{
				_document.SetAutofit(_autofit);
			}
		}

		private bool Initialize()
		{
			return Initialize(DEFAULT_SHEET_NAME);
		}

		private bool Initialize(string defaultSheetName)
		{
			bool ok = SelectFirstDefaultSheet(defaultSheetName);
			if (!ok)
			{
				SetErrCod(1);
				SetErrDes("Could not get/set first Sheet on Document");
			}
			else
			{
				SetErrCod(0);
				SetErrDes(string.Empty);
			}
			return ok;
		}

		public bool Open(string filePath)
		{
			return Open(filePath, string.Empty);
		}

		public bool Open(string filePath, string template)
		{
			try
			{
				logger.Debug("Opening Excel file: " + filePath);
				_document = ExcelFactory.Create(this, filePath, template);
				if (_autofit)
				{
					_document.SetAutofit(_autofit);
				}
			}
			catch (ExcelTemplateNotFoundException e)
			{
				SetError("Excel Template file not found", e);
			}
			catch (ExcelDocumentNotSupported e)
			{
				SetError("Excel file extension not supported", e);
			}
			catch (Exception e)//InvalidOpertaionException
			{
				logger.Error("Excel File could not be loaded", e);
				SetError(ErrorCodes.FILE_EXCEPTION, "Could not open file");
			}
			return _document != null;
		}

		public bool Save()
		{
			bool ok = false;
			if (Initialize())
			{
				try
				{
					ok = _document.Save();
					if (!ok)
					{
						SetError(ErrorCodes.FILE_NOT_SAVED, "Excel File could not be saved");
					}
				}
				catch (ExcelException e)
				{
					SetError("Excel File could not be saved", e);
				}
			}

			return ok;
		}

		public bool SaveAs(string newFileName, string password)
		{
			return SaveAsImpl(newFileName, password);
		}

		public bool SaveAs(string newFileName)
		{
			return SaveAsImpl(newFileName, null);
		}

		private bool SaveAsImpl(string newFileName, string password)
		{
			bool ok = true;
			if (Initialize())
			{
				try
				{
					_document.SaveAs(newFileName);
				}
				catch (ExcelException e)
				{
					SetError(e);
					ok = false;
				}
			}
			return ok;
		}

		public ExcelCells GetCell(int rowIdx, int colIdx)
		{
			if (Initialize())
			{
				try
				{
					return (ExcelCells)_document.GetCell(_currentWorksheet, rowIdx, colIdx);
				}
				catch (ExcelException e)
				{
					SetError(e);
				}
			}
			return null;
		}

		public void SetError(ExcelException e)
		{
			SetError(e.ErrorCode, e.ErrorDescription);
			logger.Error(e.ErrorDescription, e);
		}

		public void SetError(string errorMsg, ExcelException e)
		{
			SetError(e.ErrorCode, e.ErrorDescription);
			logger.Error(errorMsg);
		}

		public ExcelCells GetCells(int rowIdx, int colIdx, int rowCount, int colCount)
		{
			if (Initialize())
			{
				try
				{
					return (ExcelCells)_document.GetCells(_currentWorksheet, rowIdx, colIdx, rowCount, colCount);
				}
				catch (ExcelException e)
				{
					SetError(e);
				}
			}
			return null;
		}

		public bool SetCurrentWorksheet(int sheetIdx)
		{
			int zeroIndexSheet = sheetIdx - 1;
			if (zeroIndexSheet >= 0 && Initialize() && _document.GetWorksheets().Count > zeroIndexSheet)
			{
				_currentWorksheet = _document.GetWorksheets()[zeroIndexSheet];
				if (_currentWorksheet != null)
				{
					_document.SetActiveWorkSheet(_currentWorksheet.GetName());
				}
				return true;
			}
			return false;
		}

		public bool SetCurrentWorksheetByName(string sheetName)
		{
			if (Initialize())
			{
				ExcelWorksheet ws = _document.GetWorkSheet(sheetName);
				if (ws != null)
				{
					_currentWorksheet = ws;
					_document.SetActiveWorkSheet(sheetName);
					return true;
				}
			}
			return false;
		}

		public bool InsertRow(int rowIdx, int rowCount)
		{
			if (Initialize())
			{
				return _document.InsertRow(_currentWorksheet, rowIdx - 1, rowCount);
			}
			return false;
		}

		public bool InsertColumn(int colIdx, int colCount)
		{
			//throw new Exception("NotImplemented");
			return false;
			/*
             * if (isOK()) { //return _document.(_currentWorksheet, colIdx, colCount); }
             * return false;
             */
		}

		public bool DeleteRow(int rowIdx)
		{
			if (Initialize())
			{
				return _document.DeleteRow(_currentWorksheet, rowIdx - 1);
			}
			return false;
		}

		public bool DeleteColumn(int colIdx)
		{
			if (Initialize())
			{
				return _document.DeleteColumn(_currentWorksheet, colIdx - 1);
			}
			return false;
		}

		public bool InsertSheet(string sheetName)
		{
			try
			{
				return _document != null && _document.InsertWorksheet(sheetName, 0) && Initialize(sheetName);
			}
			catch (ExcelException e)
			{
				SetError("Could not insert new sheet", e);
			}
			return false;
		}

		public bool CloneSheet(string sheetName, string newSheetName)
		{
			if (Initialize())
			{
				try
				{
					return _document.CloneSheet(sheetName, newSheetName);
				}
				catch (ExcelException e)
				{
					SetError(2, e.Message);
				}
			}
			return false;
		}

		public bool ToggleColumn(int colIdx, bool visible)
		{
			if (Initialize())
			{
				return _document.ToggleColumn(_currentWorksheet, colIdx - 1, visible);
			}
			return false;
		}

		public bool ToggleRow(int rowIdx, bool visible)
		{
			if (Initialize())
			{
				return _document.ToggleRow(_currentWorksheet, rowIdx - 1, visible);
			}
			return false;
		}

		public bool DeleteSheet(string sheetName)
		{
			if (Initialize())
			{
				ExcelWorksheet ws = _document.GetWorkSheet(sheetName);
				if (ws != null)
					return _document.DeleteSheet(sheetName);
			}
			SetError(2, "Sheet not found");
			return false;
		}

		public bool DeleteSheet(int sheetIdx)
		{
			if (Initialize())
			{
				if (_document.GetWorksheets().Count >= sheetIdx)
					return _document.DeleteSheet(sheetIdx - 1);
			}
			SetError(2, "Sheet not found");
			return false;
		}


		public bool Close()
		{
			if (Initialize())
			{
				try
				{
					_document.Close();
				}
				catch (ExcelException e)
				{
					GXLogging.Error(logger, "Close error", e);
				}
			}
			_currentWorksheet = null;
			_document = null;
			return true;
		}

		private void SetError(int error, string description)
		{
			_errCode = error;
			_errDescription = description;
		}

		public int GetErrCode()
		{
			return _errCode;
		}

		public string GetErrDescription()
		{
			return _errDescription;
		}

		public ExcelWorksheet GetCurrentWorksheet()
		{
			if (Initialize())
			{
				return (ExcelWorksheet)_currentWorksheet;
			}
			return null;
		}

		public List<ExcelWorksheet> GetWorksheets()
		{
			if (Initialize())
				return _document.GetWorksheets();
			else
				return new List<ExcelWorksheet>();
		}

		private bool SelectFirstDefaultSheet(string sheetName)
		{
			if (_document != null)
			{
				int sheetsCount = _document.GetWorksheets().Count;
				if (sheetsCount == 0 && _isReadonly)
				{
					return true;
				}
				if (sheetsCount == 0)
				{
					try
					{
						_document.InsertWorksheet(sheetName, 0);
					}
					catch (ExcelException) { }
				}
				if (_currentWorksheet == null)
					_currentWorksheet = _document.GetWorksheets()[0];
			}
			return _currentWorksheet != null;
		}

		public void SetColumnWidth(int colIdx, int width)
		{
			if (colIdx > 0 && Initialize())
			{
				_document.SetColumnWidth(_currentWorksheet, colIdx, width);
			}
		}

		public void SetRowHeight(int rowIdx, int height)
		{
			if (rowIdx > 0 && Initialize())
			{
				_document.SetRowHeight(_currentWorksheet, rowIdx, height);
			}
		}

		public void SetErrCod(short arg0)
		{
			_errCode = arg0;
		}

		public void SetErrDes(string arg0)
		{
			_errDescription = arg0;
		}

	}
}
