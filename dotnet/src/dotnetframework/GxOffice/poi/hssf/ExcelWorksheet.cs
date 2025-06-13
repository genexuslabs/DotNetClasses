using NPOI.HSSF.UserModel;

namespace GeneXus.MSOffice.Excel.Poi.Hssf
{
	public class ExcelWorksheet : IExcelWorksheet
	{
		private HSSFSheet _sheet;

		public ExcelWorksheet()
		{

		}

		public ExcelWorksheet(HSSFSheet sheet)
		{
			_sheet = sheet;
		}

		public string Name => _sheet.SheetName;

		public bool Hidden
		{
			get
			{
				if (_sheet != null)
				{
					HSSFWorkbook wb = (HSSFWorkbook)_sheet.Workbook;
					return wb.IsSheetHidden(SheetIndex(wb));
				}
				return false;
			}

			set
			{
				if (_sheet != null)
				{
					HSSFWorkbook wb = (HSSFWorkbook)_sheet.Workbook;
					wb.SetSheetHidden(SheetIndex(wb), value ? NPOI.SS.UserModel.SheetState.Hidden : NPOI.SS.UserModel.SheetState.Visible);
				}
			}
		}

		public bool Rename(string newName)
		{
			if (_sheet != null)
			{
				HSSFWorkbook wb = (HSSFWorkbook)_sheet.Workbook;
				int sheetIndex = wb.GetSheetIndex(Name);
				wb.SetSheetName(sheetIndex, newName);
				return Name == newName;
			}
			return false;
		}

		public bool Copy(string newName)
		{
			if (_sheet != null)
			{
				HSSFWorkbook wb = (HSSFWorkbook)_sheet.Workbook;
				if (wb.GetSheet(newName) == null)
				{
					int srcIndex = wb.GetSheetIndex(Name);
					wb.CloneSheet(srcIndex);
					int newIndex = wb.NumberOfSheets - 1;
					wb.SetSheetName(newIndex, newName);
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

		private int SheetIndex(HSSFWorkbook wb)
		{
			return wb.GetSheetIndex(Name);
		}
	}
}