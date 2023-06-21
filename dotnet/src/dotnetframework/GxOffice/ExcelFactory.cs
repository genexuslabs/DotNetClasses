using System.IO;
using GeneXus.Application;
using GeneXus.MSOffice.Excel.exception;
using GeneXus.MSOffice.Excel.Poi.Xssf;

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

			if (filePath.EndsWith(ExcelSpreadsheet.DefaultExtension) || string.IsNullOrEmpty(Path.GetExtension(filePath)))
			{
				return new ExcelSpreadsheet(handler, filePath, template);
			}
			throw new ExcelDocumentNotSupported();
		}
	}
}