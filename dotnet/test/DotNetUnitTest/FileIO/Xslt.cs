using System;
using Xunit;

namespace UnitTesting
{
	public class XsltFunctions
	{
		[Fact]
		public void XsltApply()
		{
			GxFile file = new GxFile("./resources/xml", "error.xml");
			string html = file.XsltApply("xmlTohtml1.xsl");
			Assert.StartsWith("<html>", html, StringComparison.OrdinalIgnoreCase);
		}

	}
}
