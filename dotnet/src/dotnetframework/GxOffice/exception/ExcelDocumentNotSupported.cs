using System;

namespace GeneXus.MSOffice.Excel.exception
{
	public class ExcelDocumentNotSupported : ExcelException
	{
		public ExcelDocumentNotSupported() : base(ErrorCodes.EXTENSION_NOT_SUPPORTED, "File extension not supported")
		{
		}
	}
}
