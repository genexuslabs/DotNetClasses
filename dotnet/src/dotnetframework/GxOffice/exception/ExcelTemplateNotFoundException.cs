namespace GeneXus.MSOffice.Excel
{
	public class ExcelTemplateNotFoundException : ExcelException
	{
		public ExcelTemplateNotFoundException() : base(ErrorCodes.TEMPLATE_NOT_FOUND, "Template not found")
		{
		}
	}
}
