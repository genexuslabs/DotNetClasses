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

		public string Name => _sheet.SheetName;

		public bool Hidden
		{
			get
			{
				if (_sheet != null)
				{
					XSSFWorkbook wb = (XSSFWorkbook)_sheet.Workbook;
					return wb.IsSheetHidden(SheetIndex(wb));
				}
				return false;
			}

			set
			{
				if (_sheet != null)
				{
					XSSFWorkbook wb = (XSSFWorkbook)_sheet.Workbook;
					wb.SetSheetVisibility(SheetIndex(wb), value ? NPOI.SS.UserModel.SheetVisibility.Hidden: NPOI.SS.UserModel.SheetVisibility.Visible);
				}
			}
		}

		public bool Rename(string newName)
		{
			if (_sheet != null)
			{
				XSSFWorkbook wb = (XSSFWorkbook)_sheet.Workbook;
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
				XSSFWorkbook wb = (XSSFWorkbook)_sheet.Workbook;
				if (wb.GetSheet(newName) == null)
				{
					wb.CloneSheet(wb.GetSheetIndex(Name), newName);
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
			return wb.GetSheetIndex(Name);
		}
	}
}
