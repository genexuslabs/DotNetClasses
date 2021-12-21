using System;
using System.IO;
using GeneXus.Programs;
using GeneXus.Utils;
using Xunit;

namespace UnitTesting
{
	public class PDFTests
	{
		[Fact]
		public void ExtractTextFromPDF()
		{
			string text = DocumentHandler.GetText("sample.pdf", "pdf");
			Assert.Contains("The end, and just as well.", text, StringComparison.InvariantCulture);
		}
		[Fact]
		public void TestIText5()
		{
			string report = "Report.pdf";
			if (File.Exists(report))
				File.Delete(report);
			try
			{
				apdfbasictest test = new apdfbasictest();
				test.execute();
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				Console.WriteLine(ex.ToString());	
			}
			Assert.True(File.Exists(report));
		}

	}
}
