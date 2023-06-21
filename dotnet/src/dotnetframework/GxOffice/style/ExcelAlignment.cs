namespace GeneXus.MSOffice.Excel.Style
{
	public class ExcelAlignment : ExcelStyleDimension
	{
		public const int VERTICAL_ALIGN_MIDDLE = 1;
		public const int VERTICAL_ALIGN_TOP = 2;
		public const int VERTICAL_ALIGN_BOTTOM = 3;
		public const int HORIZONTAL_ALIGN_LEFT = 1;
		public const int HORIZONTAL_ALIGN_CENTER = 2;
		public const int HORIZONTAL_ALIGN_RIGHT = 3;

		private int horizontalAlignment;
		private int verticalAlignment;

		public ExcelAlignment()
		{
		}

		public void SetHorizontalAlignment(int value)
		{
			horizontalAlignment = value;
			SetChanged();
		}
		public int GetHorizontalAlignment()
		{
			return horizontalAlignment;
		}

		public int GetVerticalAlignment()
		{
			return verticalAlignment;
		}
		public void SetVerticalAlignment(int value)
		{
			verticalAlignment = value;
			SetChanged();
		}
	}
}
