using System;
using System.IO;
using Xunit;

namespace UnitTesting
{
	public class XsltFunctions : FileSystemTest
	{
		[Fact]
		public void XsltApply()
		{
			GxFile file = new GxFile(Path.Combine(BaseDir, "./resources/xml"), "error.xml");
			string html = file.XsltApply("xmlTohtml1.xsl");
			Assert.StartsWith("<html>", html, StringComparison.OrdinalIgnoreCase);
		}

	}
}
