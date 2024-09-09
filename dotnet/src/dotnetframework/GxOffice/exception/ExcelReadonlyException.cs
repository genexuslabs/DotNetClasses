namespace GeneXus.MSOffice.Excel
{
	public class ExcelReadonlyException : ExcelException
	{
		public ExcelReadonlyException() : base(13, "Can not modify a readonly document")
		{
		}
	}
}
