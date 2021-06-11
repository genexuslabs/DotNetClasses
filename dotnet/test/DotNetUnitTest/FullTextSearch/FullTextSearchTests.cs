using Xunit;
using GeneXus.Application;
using System.Collections.Generic;
using System.Linq;
using System;
using GeneXus.Cryptography;
using GeneXus.Utils;
using System.IO;
#if NETCORE
namespace xUnitTesting
#else
namespace UnitTesting
#endif
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
#if NETCORE
			File.WriteAllText("textNETCORE.txt", text);
			File.WriteAllText("cleanhtmlNETCORE.html", prettyHTML);
#else
			File.WriteAllText("textNET.txt", text);
			File.WriteAllText("cleanhtmlNET.html", prettyHTML);
#endif
			Assert.Contains("atencionalcliente@gmail.com", text, StringComparison.OrdinalIgnoreCase);
			Assert.DoesNotContain("<html><html>", prettyHTML, StringComparison.OrdinalIgnoreCase);

		}
	}
}