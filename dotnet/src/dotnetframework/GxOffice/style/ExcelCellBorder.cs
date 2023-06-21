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

		public ExcelBorder BorderBottom { get => borderBottom; set => borderBottom = value; }

		public ExcelBorder BorderTop => borderTop;
		public void GetBorderTop(ExcelBorder value)
		{
			borderTop = value; 
		}

		public ExcelBorder BorderLeft => borderLeft;
		public void GetBorderLeft(ExcelBorder value)
		{
			borderLeft = value;
		}

		public ExcelBorder BorderRight { get => borderRight; set => borderRight = value; }

		public ExcelBorder BorderDiagonalUp { get => borderDiagonalUp; set => borderDiagonalUp = value; }

		public ExcelBorder BorderDiagonalDown { get => borderDiagonalDown; set => borderDiagonalDown = value; }
	}
}
