using System;
using System.IO;
using GeneXus.Configuration;
using GeneXus.Programs;
using GeneXus.Utils;
using Xunit;

namespace UnitTesting
{
	public class PDFTests
	{
		[Fact]
		public void TestITextFormat()
		{
			string report = "PDFFormat.pdf";
			if (File.Exists(report))
				File.Delete(report);
			try
			{
				apdfformat test = new apdfformat();
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
