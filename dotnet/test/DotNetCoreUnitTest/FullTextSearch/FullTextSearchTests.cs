using System;
using System.IO;
using GeneXus.Utils;
using UnitTesting;
using Xunit;

namespace xUnitTesting
{
	public class FullTextSearch : FileSystemTest
	{
		[Fact]
		public void HtmlClean()
		{
			string htmlFileName = Path.Combine(BaseDir, "FullTextSearch", "Ugly.html");
			string html = File.ReadAllText(htmlFileName);
			string text = DocumentHandler.GetText(htmlFileName, "html");
			string prettyHTML = DocumentHandler.HTMLClean(html);
			Assert.Contains("atencionalcliente@gmail.com", text, StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith("<html>", prettyHTML, StringComparison.OrdinalIgnoreCase);

		}
		[Fact]
		public void FileHtmlClean()
		{
			GxFile file = new GxFile();
			string htmlFileName = Path.Combine(BaseDir, "FullTextSearch", "Ugly.html");
			file.Source = htmlFileName;
			string prettyHTML = file.HtmlClean();
			Assert.StartsWith("<html>", prettyHTML, StringComparison.OrdinalIgnoreCase);
		}
	}
}