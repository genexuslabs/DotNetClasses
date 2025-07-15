using System.Collections.Generic;
using NPOI.HSSF.UserModel;

namespace GeneXus.MSOffice.Excel.Poi.Hssf
{
	public class StylesCache
	{
		private HSSFWorkbook pWorkbook;
		private Dictionary<string, HSSFCellStyle> stylesByFont;
		private Dictionary<string, HSSFCellStyle> stylesByFormat;

		public StylesCache(HSSFWorkbook pWorkbook)
		{
			this.pWorkbook = pWorkbook;
			this.stylesByFont = new Dictionary<string, HSSFCellStyle>();
			this.stylesByFormat = new Dictionary<string, HSSFCellStyle>();
		}

		public HSSFCellStyle GetCellStyle(HSSFFont newFont)
		{
			string fontKey = $"{newFont.FontHeightInPoints}{newFont.FontName}{newFont.IsBold}{newFont.IsItalic}{newFont.Underline}{newFont.Color}";

			if (stylesByFont.ContainsKey(fontKey))
			{
				return stylesByFont[fontKey];
			}

			HSSFCellStyle newStyle = (HSSFCellStyle)pWorkbook.CreateCellStyle();
			stylesByFont[fontKey] = newStyle;
			return newStyle;
		}

		public HSSFCellStyle GetCellStyle(short format)
		{
			string formatKey = format.ToString();

			if (stylesByFormat.ContainsKey(formatKey))
			{
				return stylesByFormat[formatKey];
			}

			HSSFCellStyle newStyle = (HSSFCellStyle)pWorkbook.CreateCellStyle();
			stylesByFormat[formatKey] = newStyle;
			return newStyle;
		}
	}
}