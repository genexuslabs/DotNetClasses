using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
		[Fact]
		public void TestIText4LGPLIsUsed()
		{
			Assembly pdfReports = Assembly.Load("GxPdfReportsCS");
			AssemblyName itextsharp = pdfReports.GetReferencedAssemblies().First(t => t.Name=="itextsharp");

			Assert.Equal(4, itextsharp.Version.Major);
		}

	}
}
