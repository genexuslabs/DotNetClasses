namespace GeneXus.MSOffice.Excel.Style
{
	public class ExcelFill : ExcelStyleDimension
	{
		private ExcelColor cellBackColor;

		public ExcelFill()
		{
			cellBackColor = new ExcelColor();
		}

		public ExcelColor CellBackColor => cellBackColor;

		public override bool IsDirty()
		{
			return base.IsDirty() || cellBackColor.IsDirty();
		}
	}
}
