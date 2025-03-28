using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using com.genexus.reports;
using DotNetUnitTest;
using GeneXus.Programs;
using GeneXus.Utils;
using Xunit;

namespace UnitTesting
{
	public class PDFTests
	{
		[Fact]
		public void ConcurrencyPDFSearchPaths()
		{
			Parallel.For(0, 50, i =>
			{
				apdfbasictest test = new apdfbasictest();
				test.execute();
			});
			Parallel.For(0, 300, i =>
			{
				//System.ArgumentException : Source array was not long enough. Check srcIndex and length, and the array's lower bounds.
				Utilities.addPredefinedSearchPaths(new String[] { "A", "B", "C" });
			});

		}
		[WindowsOnlyFact]
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
				Assert.True(false, $"Unexpected error in TestIText5: {ex.Message}");
			}
		}
		[Fact]
		public void TestIText4LGPLIsUsed()
		{
			Assembly pdfReports = Assembly.Load("GxPdfReportsCS");
			AssemblyName itextsharp = pdfReports.GetReferencedAssemblies().First(t => t.Name=="itextsharp");

			Assert.Equal(4, itextsharp.Version.Major);
		}

	}
}
