namespace GeneXus.MSOffice.Excel.style
{
	public class ExcelColor : ExcelStyleDimension
	{
		private int? _alpha = null;
		public int? Alpha => _alpha;

		public int? Red => _red;

		public int? Green => _green;

		public int? Blue => _blue;

		private int? _red = null;
		private int? _green = null;
		private int? _blue = null;

		public ExcelColor()
		{

		}

		public ExcelColor(int alpha, int r, int g, int b)
		{
			SetColorImpl(alpha, r, g, b);
		}

		public bool SetColorRGB(int r, int g, int b)
		{
			SetColorImpl(0, r, g, b);
			return true;
		}

		public bool SetColorARGB(int alpha, int r, int g, int b)
		{
			SetColorImpl(alpha, r, g, b);
			return true;
		}

		private void SetColorImpl(int alpha, int r, int g, int b)
		{
			_alpha = alpha;
			_red = r;
			_green = g;
			_blue = b;
			SetChanged();
		}
	}
}
