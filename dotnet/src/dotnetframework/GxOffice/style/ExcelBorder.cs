namespace GeneXus.MSOffice.Excel.Style
{
	public class ExcelBorder : ExcelStyleDimension
	{
		private ExcelColor borderColor;
		private string borderStyle = string.Empty;

		public string Border
		{
			get => borderStyle;
			set
			{
				borderStyle = value;
				SetChanged();
			}
		}

		public ExcelBorder()
		{
			borderColor = new ExcelColor();
		}

		public ExcelColor BorderColor { get => borderColor; set => borderColor = value; }
	}
}
