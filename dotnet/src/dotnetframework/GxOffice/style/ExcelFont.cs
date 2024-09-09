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

		public string FontFamily
		{
			get => fontFamily;
			set
			{
				this.fontFamily = value;
				SetChanged();
			}
		}

		public bool Italic
		{
			get => italic;
			set
			{
				this.italic = value;
				SetChanged();
			}
		}

		public int Size
		{
			get => size;
			set
			{
				this.size = value;
				SetChanged();
			}
		}

		public bool Strike
		{
			get => strike;
			set
			{
				this.strike = value;
				SetChanged();
			}
		}

		public bool Underline
		{
			get => underline;
			set
			{
				this.underline = value;
				SetChanged();
			}
		}

		public bool Bold
		{
			get => bold;
			set
			{
				this.bold = value;
				SetChanged();
			}
		}

		public ExcelColor Color => color;
	}
}
