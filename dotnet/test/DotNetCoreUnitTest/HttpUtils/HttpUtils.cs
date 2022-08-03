using System;
using GeneXus.Application;
using GeneXus.Http;
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
			string encodedValue = HttpHelper.GetEncodedContentDisposition(contentDisposition, browserType);

			Assert.Equal(expectedContentDisposition, encodedValue);
		}
	}
}
