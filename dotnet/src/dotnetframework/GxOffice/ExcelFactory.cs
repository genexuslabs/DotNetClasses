using System.IO;
using GeneXus.Application;

namespace GeneXus.MSOffice.Excel
{
	public class ExcelFactory
	{
		public static IExcelSpreadsheet Create(IGXError handler, string filePath, string template)
		{
			if (!string.IsNullOrEmpty(filePath))
			{
				filePath = Path.IsPathRooted(filePath) ? filePath : Path.Combine(GxContext.StaticPhysicalPath(), filePath);
			}
			if (!string.IsNullOrEmpty(template))
			{
				template = Path.IsPathRooted(template) ? template : Path.Combine(GxContext.StaticPhysicalPath(), template);
			}

			if (filePath.EndsWith(GeneXus.MSOffice.Excel.Poi.Xssf.ExcelSpreadsheet.DefaultExtension) || string.IsNullOrEmpty(Path.GetExtension(filePath)))
			{
				return new GeneXus.MSOffice.Excel.Poi.Xssf.ExcelSpreadsheet(handler, filePath, template);
			}
			else if (filePath.EndsWith(GeneXus.MSOffice.Excel.Poi.Hssf.ExcelSpreadsheet.DefaultExtension))
			{
				return new GeneXus.MSOffice.Excel.Poi.Hssf.ExcelSpreadsheet(handler, filePath, template);
			}
			throw new ExcelDocumentNotSupported();
		}
	}
}