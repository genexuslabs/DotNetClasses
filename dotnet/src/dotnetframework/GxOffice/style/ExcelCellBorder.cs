namespace GeneXus.MSOffice.Excel.Style
{
	public class ExcelCellBorder
	{
		private ExcelBorder borderTop = new ExcelBorder();
		private ExcelBorder borderBottom = new ExcelBorder();
		private ExcelBorder borderLeft = new ExcelBorder();
		private ExcelBorder borderRight = new ExcelBorder();
		private ExcelBorder borderDiagonalUp = new ExcelBorder();
		private ExcelBorder borderDiagonalDown = new ExcelBorder();

		public void SetAll(ExcelBorder borderStyle)
		{
			borderTop = borderStyle;
			borderBottom = borderStyle;
			borderLeft = borderStyle;
			borderRight = borderStyle;
		}

		public ExcelBorder GetBorderBottom()
		{
			return borderBottom;
		}
		public void SetBorderBottom(ExcelBorder value)
		{
			borderBottom = value;
		}

		public ExcelBorder GetBorderTop()
		{
			return borderTop; 
		}
		public void GetBorderTop(ExcelBorder value)
		{
			borderTop = value; 
		}

		public ExcelBorder GetBorderLeft()
		{
			return borderLeft;
		}
		public void GetBorderLeft(ExcelBorder value)
		{
			borderLeft = value;
		}

		public ExcelBorder GetBorderRight()
		{
			return borderRight;
		}
		public void SetBorderRight(ExcelBorder value)
		{
			borderRight = value;
		}

		public ExcelBorder GetBorderDiagonalUp()
		{
			return borderDiagonalUp;
		}
		public void SetBorderDiagonalUp(ExcelBorder value)
		{
			borderDiagonalUp = value;
		}
		public ExcelBorder GetBorderDiagonalDown()
		{
			return borderDiagonalDown;
		}
		public void SetBorderDiagonalDown(ExcelBorder value)
		{
			borderDiagonalDown = value;
		}
	}
}
