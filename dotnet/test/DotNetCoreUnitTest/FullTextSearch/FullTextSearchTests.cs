using Xunit;
using GeneXus.Application;
using System.Collections.Generic;
using System.Linq;
using System;
using GeneXus.Cryptography;
using GeneXus.Utils;
using System.IO;
namespace xUnitTesting
{
	public class FullTextSearch
	{
		[Fact]
		public void HtmlClean()
		{
			string htmlFileName = Path.Combine("FullTextSearch", "Ugly.html");
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
			string htmlFileName = Path.Combine("FullTextSearch", "Ugly.html");
			file.Source = htmlFileName;
			string prettyHTML = file.HtmlClean();
			Assert.StartsWith("<html>", prettyHTML, StringComparison.OrdinalIgnoreCase);
		}
	}
}