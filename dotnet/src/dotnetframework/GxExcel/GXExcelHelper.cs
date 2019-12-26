using System.Drawing;

namespace GeneXus.Office.Excel
{
	public static class GXExcelHelper
	{
		public static Color ResolveColor(int value)
		{
			int val = (int)value;			
			System.Drawing.Color color;

			if (val >= 1 && val <= Constants.IndexColors.Length)
			{
				color = ColorTranslator.FromHtml(Constants.IndexColors[val - 1]);
			}
			else
			{
				int red = val >> 16 & 0xff;
				int green = val >> 8 & 0xff;
				int blue = val & 0xff;
				color = System.Drawing.Color.FromArgb(red, green, blue);
			}
			return color;
		}
	}
}
