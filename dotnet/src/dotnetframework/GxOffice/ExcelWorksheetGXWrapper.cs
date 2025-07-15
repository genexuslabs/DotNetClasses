namespace GeneXus.MSOffice.Excel
{
	public class ExcelWorksheetGXWrapper : IExcelWorksheet
	{
		private IExcelWorksheet _value;

		public ExcelWorksheetGXWrapper(IExcelWorksheet value)
		{
			_value = value;
		}

		public ExcelWorksheetGXWrapper()
		{
		}

		public string Name
		{
			get => _value.Name;
		}

		public bool Hidden { get => _value.Hidden; set => _value.Hidden=value; }

		public bool Rename(string newName)
		{
			return _value.Rename(newName);
		}

		public bool Copy(string newName)
		{
			return _value.Copy(newName);
		}

		public void SetProtected(string password)
		{
			_value.SetProtected(password);
		}
	}
}
