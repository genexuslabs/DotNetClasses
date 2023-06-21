namespace GeneXus.MSOffice.Excel.Style
{
	public class ExcelFont : ExcelStyleDimension
	{
		private string fontFamily = null;
		private bool italic;
		private int size;
		private bool strike;
		private bool underline;
		private bool bold;
		private ExcelColor color = null;

		public ExcelFont()
		{
			color = new ExcelColor();
		}

		public string GetFontFamily()
		{
			return fontFamily;
		}
		public void SetFontFamily(string fontFamily)
		{
			this.fontFamily = fontFamily;
			SetChanged();
		}
		public bool GetItalic()
		{
			return italic;
		}
		public void SetItalic(bool italic)
		{
			this.italic = italic;
			SetChanged();
		}
		public int GetSize()
		{
			return size;
		}
		public void SetSize(int size)
		{
			this.size = size;
			SetChanged();
		}
		public bool GetStrike()
		{
			return strike;
		}
		public void SetStrike(bool strike)
		{
			this.strike = strike;
			SetChanged();
		}
		public bool GetUnderline()
		{
			return underline;
		}
		public void SetUnderline(bool underline)
		{
			this.underline = underline;
			SetChanged();
		}
		public bool GetBold()
		{
			return bold;
		}
		public void SetBold(bool bold)
		{
			this.bold = bold;
			SetChanged();
		}
		public ExcelColor GetColor()
		{
			return color;
		}
	}
}
