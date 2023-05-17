using System;
using System.Web;
using GeneXus.Application;
using GeneXus.Http;
using GeneXus.Utils;
using Xunit;

namespace DotNetCoreUnitTest.HttpUtils
{
	public class TestHttpUtils
	{

		[Fact]
		public void TestDoNotDoubleEncodeAmpersand()
		{
			string state = "{\"gxProps\":[\"FORM\":{\"Class\":\"form-horizontal Form\"}}], \"gxHiddens\":{\"gxhash_vA\":\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"," +
				"\"hsh\":\"C/CAcgMV0JZC/+o3ikT+R2Hhb1LcQ==\",\"Z3c\":\"&#039;\"}]}}";

			string jsonEncoded = HttpHelper.HtmlEncodeJsonValue(state);
			Assert.Contains("&amp;", jsonEncoded, StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith("{", jsonEncoded, StringComparison.OrdinalIgnoreCase);
		}

	}
}
