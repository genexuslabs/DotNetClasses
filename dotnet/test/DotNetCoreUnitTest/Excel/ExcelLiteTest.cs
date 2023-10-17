using System.IO;
using GeneXus.Office;
using GeneXus.Utils;
using UnitTesting;
using Xunit;

namespace DotNetCoreUnitTest.Excel
{
	public class ExcelLiteTest : FileSystemTest
	{
		[Fact]
		public void ExcelLiteReadTest()
		{
			ExcelDocumentI excelDocument= new ExcelDocumentI();
			string fileName = Path.Combine(BaseDir, "SampleXLS.xls");
			excelDocument.Open(fileName);
			Assert.Equal(0, excelDocument.ErrCode);
			double number = excelDocument.get_Cells(2, 1).Number;
			Assert.Equal(1, number);
			string text = excelDocument.get_Cells(2, 2).Text;
			Assert.Equal("A", text);

			number = excelDocument.get_Cells(3, 1).Number;
			Assert.Equal(2, number);
			text = excelDocument.get_Cells(3, 2).Text;
			Assert.Equal("A", text);

			number = excelDocument.get_Cells(101, 1).Number;
			Assert.Equal(100, number);
			text = excelDocument.get_Cells(101, 2).Text;
			Assert.Equal("A", text);

			excelDocument.Close();
		}
	}
}
