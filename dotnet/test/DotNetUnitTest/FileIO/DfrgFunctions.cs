using System;
using System.IO;
using GeneXus.Application;
using GeneXus.Configuration;
using Xunit;

namespace UnitTesting
{
	public class DfrgFunctions
	{
		const string APPLICATIONS_CONTENT = "[  {    \"Id\": \"4caaaed5-1160-4132-b54f-0191e527a84a\",    \"Type\": 1,    \"EnvironmentGUID\": \"b3730606-0f2a-4e8a-b395-d8fdf226def8\",    \"IsNew\": false  }]";
		const string DOCUMENT_CONTENT = "Line 1Line 2Line 3";
		const string MS923_CONTENT = "1234567890123";
		[Fact]
		public void dfrgtxtANSITest()
		{
			string fileName = "Document.txt";
			string result = string.Empty;
			GxContext context = new GxContext();
			string line;
			int code = context.FileIOInstance.dfropen(fileName, 1024, "*", "*", "ANSI");
			Assert.Equal(0, code);
			while (context.FileIOInstance.dfrnext() == 0)
			{
				code = context.FileIOInstance.dfrgtxt(out line, 0);
				result += line;
			}
			code = context.FileIOInstance.dfrclose();
			Assert.Equal(0, code);
			Assert.Equal(DOCUMENT_CONTENT, result);
		}
		[Fact]
		public void dfrgtxtTest()
		{
			string fileName = "applications.json";
			string result = string.Empty;
			GxContext context = new GxContext();
			string line;
			Console.WriteLine("Full file name:" + Path.Combine(GxContext.StaticPhysicalPath(), fileName));
			int code = context.FileIOInstance.dfropen(fileName, 1024, "", "", "UTF-8");
			Assert.Equal(0, code);
			while (context.FileIOInstance.dfrnext() == 0)
			{
				code = context.FileIOInstance.dfrgtxt(out line, 100);
				result += line;
			}
			code = context.FileIOInstance.dfrclose();
			Assert.Equal(0, code);
			Assert.Equal(APPLICATIONS_CONTENT, result);
		}
		[Fact]
		public void dfrgtxtOverflowTest()
		{
			string fileName = "MS923.txt";
			GxContext context = new GxContext();
			string line;
			//overflow
			int code = context.FileIOInstance.dfropen(fileName, 1024, string.Empty, string.Empty, "SHIFT-JIS");
			Assert.Equal(0, code);
			if (context.FileIOInstance.dfrnext() == 0)
			{
				code = context.FileIOInstance.dfrgtxt(out line, 10);
				Assert.Equal(MS923_CONTENT.Substring(0, 10), line);
				Assert.Equal(-6, code);
			}
			code = context.FileIOInstance.dfrclose();
			Assert.Equal(0, code);

			//overflow when encoding is not specified
			code = context.FileIOInstance.dfropen(fileName, 1024, string.Empty, string.Empty, string.Empty);
			Assert.Equal(0, code);
			if (context.FileIOInstance.dfrnext() == 0)
			{
				code = context.FileIOInstance.dfrgtxt(out line, 10);
				Assert.Equal(MS923_CONTENT.Substring(0,10), line);
				Assert.Equal(-6, code);
			}
			code = context.FileIOInstance.dfrclose();
			Assert.Equal(0, code);

			//do not overflow
			code = context.FileIOInstance.dfropen(fileName, 1024, string.Empty, string.Empty, "SHIFT-JIS");
			Assert.Equal(0, code);
			if (context.FileIOInstance.dfrnext() == 0)
			{
				code = context.FileIOInstance.dfrgtxt(out line, 13);
				Assert.Equal(MS923_CONTENT, line);
			}
			code = context.FileIOInstance.dfrclose();
			Assert.Equal(0, code);
		}
	}
}
