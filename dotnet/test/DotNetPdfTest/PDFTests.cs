using System;
using System.IO;
using GeneXus.Programs;
using Xunit;

namespace UnitTesting
{
	public class PDFTests
	{
		[Fact]
		public void TestITextFormat()
		{
			string report = "PDFFormat.pdf";
			string outputFileDirectory = "temp";

			string reportFullPath = Path.Combine(Directory.GetCurrentDirectory(), outputFileDirectory, report);
			Directory.CreateDirectory(Path.GetDirectoryName(reportFullPath));

			if (File.Exists(reportFullPath))
				File.Delete(reportFullPath);
			try
			{
				apdfformats2 test = new apdfformats2();
				test.execute();
				longHtml test2 = new longHtml();
				test2.execute();
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				Console.WriteLine(ex.ToString());
			}
			Assert.True(File.Exists(reportFullPath));
		}
	}
}
