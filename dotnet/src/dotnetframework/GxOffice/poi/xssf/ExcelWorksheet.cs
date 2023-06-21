using NPOI.XSSF.UserModel;

namespace GeneXus.MSOffice.Excel.Poi.Xssf
{
	public class ExcelWorksheet : IExcelWorksheet
	{
		private XSSFSheet _sheet;

		public ExcelWorksheet()
		{

		}

		public ExcelWorksheet(XSSFSheet sheet)
		{
			_sheet = sheet;
		}

		public string GetName()
		{
			return _sheet.SheetName;
		}

		public bool SetHidden(bool hidden)
		{
			if (_sheet != null)
			{
				XSSFWorkbook wb = (XSSFWorkbook)_sheet.Workbook;
				wb.SetSheetHidden(SheetIndex(wb), hidden ? NPOI.SS.UserModel.SheetState.Hidden : NPOI.SS.UserModel.SheetState.Visible);
				return true;
			}
			return false;
		}

		public bool IsHidden()
		{
			if (_sheet != null)
			{
				XSSFWorkbook wb = (XSSFWorkbook)_sheet.Workbook;
				return wb.IsSheetHidden(SheetIndex(wb));
			}
			return false;
		}

		public bool Rename(string newName)
		{
			if (_sheet != null)
			{
				XSSFWorkbook wb = (XSSFWorkbook)_sheet.Workbook;
				int sheetIndex = wb.GetSheetIndex(GetName());
				wb.SetSheetName(sheetIndex, newName);
				return GetName() == newName;
			}
			return false;
		}

		public bool Copy(string newName)
		{
			if (_sheet != null)
			{
				XSSFWorkbook wb = (XSSFWorkbook)_sheet.Workbook;
				if (wb.GetSheet(newName) == null)
				{
					wb.CloneSheet(wb.GetSheetIndex(GetName()), newName);
					return true;
				}
			}
			return false;
		}

		public void SetProtected(string password)
		{
			if (_sheet != null)
			{
				if (string.IsNullOrEmpty(password))
					_sheet.ProtectSheet(null);
				else
					_sheet.ProtectSheet(password);
			}
		}

		private int SheetIndex(XSSFWorkbook wb)
		{
			return wb.GetSheetIndex(GetName());
		}
	}
}
