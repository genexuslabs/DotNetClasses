using System.Collections.Generic;

namespace GeneXus.MSOffice.Excel
{
	public interface IExcelSpreadsheet
	{
		// General Methods
		bool Save();
		bool SaveAs(string newFileName);
		bool Close();

		// CellMethods
		IExcelCellRange GetCells(IExcelWorksheet worksheet, int startRow, int startCol, int rowCount, int colCount);
		IExcelCellRange GetCell(IExcelWorksheet worksheet, int startRow, int startCol);
		bool InsertRow(IExcelWorksheet worksheet, int rowIdx, int rowCount);
		bool DeleteRow(IExcelWorksheet worksheet, int rowIdx);
		bool DeleteColumn(IExcelWorksheet worksheet, int colIdx);

		// Worksheets
		List<IExcelWorksheet> GetWorksheets();
		IExcelWorksheet GetWorkSheet(string name);
		bool InsertWorksheet(string newSheetName, int idx);
		bool Autofit { set; }
		void SetColumnWidth(IExcelWorksheet worksheet, int colIdx, int width);
		void SetRowHeight(IExcelWorksheet worksheet, int rowIdx, int height);
		bool SetActiveWorkSheet(string name);
		bool DeleteSheet(int sheetIdx);
		bool DeleteSheet(string sheetName);
		bool ToggleColumn(IExcelWorksheet worksheet, int colIdx, bool visible);
		bool ToggleRow(IExcelWorksheet _currentWorksheet, int i, bool visible);
		bool CloneSheet(string sheetName, string newSheetName);
	}
}
