namespace GeneXus.MSOffice.Excel
{
	public class ExcelDocumentNotSupported : ExcelException
	{
		public ExcelDocumentNotSupported() : base(ErrorCodes.EXTENSION_NOT_SUPPORTED, "File extension not supported")
		{
		}
	}
}
