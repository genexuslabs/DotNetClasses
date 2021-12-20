using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

	}
}
