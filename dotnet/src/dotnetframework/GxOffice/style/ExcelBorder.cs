using GeneXus.MSOffice.Excel.style;

namespace GeneXus.MSOffice.Excel.Style
{
	public class ExcelBorder : ExcelStyleDimension
	{
		private ExcelColor borderColor;
		private string borderStyle = string.Empty;

		public string GetBorder()
		{
			return borderStyle;
		}
		public void SetBorder(string value)
		{
				borderStyle = value;
				SetChanged();
		}

		public ExcelBorder()
		{
			borderColor = new ExcelColor();
		}

		public ExcelColor GetBorderColor()
		{
			return borderColor;
		}
		public void SetBorderColor(ExcelColor value)
		{
			borderColor = value;
		}
	}
}
