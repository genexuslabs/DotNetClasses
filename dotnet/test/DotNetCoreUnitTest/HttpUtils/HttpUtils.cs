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
		public void TestContentDispositionHeaderEncoding1()
		{
			String contentDisposition = "attachment; filename=file.pdf";
			String expectedContentDisposition = contentDisposition;
			DoTest(contentDisposition, expectedContentDisposition);
		}

		[Fact]
		public void TestContentDispositionHeaderEncoding2()
		{
			String contentDisposition = "attachment; filename=file.pdf";
			String expectedContentDisposition = contentDisposition;
			DoTest(contentDisposition, expectedContentDisposition, GxContext.BROWSER_SAFARI);
		}

		[Fact]
		public void TestContentDispositionHeaderEncoding3()
		{
			String contentDisposition = "attachment; filename=注文詳細.xlsx";
			String expectedContentDisposition = "attachment; filename=\"=?utf-8?B?5rOo5paH6Kmz57SwLnhsc3g=?=\"";
			DoTest(contentDisposition, expectedContentDisposition);
		}

		[Fact]
		public void TestContentDispositionHeaderEncoding4()
		{
			String contentDisposition = "attachment; filename=注文詳細.xlsx";
			String expectedContentDisposition = contentDisposition;
			//Safari does not support rfc5987
			DoTest(contentDisposition, expectedContentDisposition, GxContext.BROWSER_SAFARI);
		}

		[Fact]
		public void TestContentDispositionHeaderEncoding5()
		{
			String contentDisposition = "form-data; filename=file.pdf";
			String expectedContentDisposition = contentDisposition;
			DoTest(contentDisposition, expectedContentDisposition);
		}

		[Fact]
		public void TestContentDispositionHeaderEncoding6()
		{
			String contentDisposition = "ATTACHMENT; FILEname=注文詳細.xlsx";
			String expectedContentDisposition = "ATTACHMENT; filename=\"=?utf-8?B?5rOo5paH6Kmz57SwLnhsc3g=?=\"";
			DoTest(contentDisposition, expectedContentDisposition);
		}

		private static void DoTest(string contentDisposition, string expectedContentDisposition, int browserType = GxContext.BROWSER_CHROME)
		{
			string encodedValue = GXUtil.EncodeContentDispositionHeader(contentDisposition, browserType);

			Assert.Equal(expectedContentDisposition, encodedValue);
		}

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
