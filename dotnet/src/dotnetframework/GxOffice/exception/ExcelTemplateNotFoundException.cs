namespace GeneXus.MSOffice.Excel.exception
{
	public class ExcelTemplateNotFoundException : ExcelException
	{
		public ExcelTemplateNotFoundException() : base(ErrorCodes.TEMPLATE_NOT_FOUND, "Template not found")
		{
		}
	}
}
