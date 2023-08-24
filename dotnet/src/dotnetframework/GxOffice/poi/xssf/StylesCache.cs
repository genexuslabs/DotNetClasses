using System.Collections.Generic;
using NPOI.XSSF.UserModel;

namespace GeneXus.MSOffice.Excel.Poi.Xssf
{
	public class StylesCache
	{
		private XSSFWorkbook pWorkbook;
		private Dictionary<string, XSSFCellStyle> stylesByFont;
		private Dictionary<string, XSSFCellStyle> stylesByFormat;

		public StylesCache(XSSFWorkbook pWorkbook)
		{
			this.pWorkbook = pWorkbook;
			this.stylesByFont = new Dictionary<string, XSSFCellStyle>();
			this.stylesByFormat = new Dictionary<string, XSSFCellStyle>();
		}

		public XSSFCellStyle GetCellStyle(XSSFFont newFont)
		{
			string fontKey = $"{newFont.FontHeightInPoints}{newFont.FontName}{newFont.IsBold}{newFont.IsItalic}{newFont.Underline}{newFont.Color}";

			if (stylesByFont.ContainsKey(fontKey))
			{
				return stylesByFont[fontKey];
			}

			XSSFCellStyle newStyle = (XSSFCellStyle)pWorkbook.CreateCellStyle();
			stylesByFont[fontKey] = newStyle;
			return newStyle;
		}

		public XSSFCellStyle GetCellStyle(short format)
		{
			string formatKey = format.ToString();

			if (stylesByFormat.ContainsKey(formatKey))
			{
				return stylesByFormat[formatKey];
			}

			XSSFCellStyle newStyle = (XSSFCellStyle)pWorkbook.CreateCellStyle();
			stylesByFormat[formatKey] = newStyle;
			return newStyle;
		}
	}
}
