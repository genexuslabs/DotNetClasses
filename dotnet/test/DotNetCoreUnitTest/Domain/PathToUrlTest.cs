using System.Runtime.InteropServices;
using GeneXus.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Xunit;

namespace xUnitTesting
{

	public class PathToUrlTest
	{
		[Fact]
		public void PathToUrl()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return;
			}
			DefaultHttpContext httpContext = new DefaultHttpContext();

			httpContext.Features.Set<IHttpRequestFeature>(new HttpRequestFeature());
			httpContext.Features.Set<IHttpResponseFeature>(new HttpResponseFeature());

			httpContext.Request.Host = new HostString("localhost");
			httpContext.Request.Scheme = "http";
			httpContext.Request.PathBase = "/test";

			string baseUrl = @"http://localhost/test/";
			GxContext context = new GxContext();
			context.HttpContext = httpContext;

			string imagePath = @"C:\Models\MyKB\NETModel\web\PublicTempStorage\dog6bf667c1-f15b-4af1-9ece-893381a470a0.jpg";
			string imageUrl = context.PathToUrl(imagePath);
			string expectedImagePath = baseUrl + @"dog6bf667c1-f15b-4af1-9ece-893381a470a0.jpg";
			Assert.Equal(expectedImagePath, imageUrl, true, true, false);


			imagePath = @"https://testsk3.blob.core.windows.net/skprivate/PublicTempStorage/dogeb918a65-0d8a-41e0-906a-6554457280e3.jpg?sv=2018-03-28&sp=r";
			imageUrl = context.PathToUrl(imagePath);
			expectedImagePath = imagePath;
			Assert.Equal(expectedImagePath, imageUrl, true, true, false);

			imagePath = @"file:///C:/Models/MyKB/NETModel/web/PublicTempStorage/dog6bf667c1-f15b-4af1-9ece-893381a470a0.jpg";
			imageUrl = context.PathToUrl(imagePath);
			expectedImagePath = baseUrl + @"dog6bf667c1-f15b-4af1-9ece-893381a470a0.jpg"; ;
			Assert.Equal(expectedImagePath, imageUrl, true, true, false);

		}
		[Fact]
		public void PathToUrlCmdLine()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return;
			}
			GxContext context = new GxContext();

			string imagePath = @"C:/Models/MyKB/NETModel/web/PublicTempStorage/dog6bf667c1-f15b-4af1-9ece-893381a470a0.jpg";
			string imageUrl = context.PathToUrl(imagePath);
			string expectedImagePath = string.Empty;
			Assert.Equal(expectedImagePath, imageUrl, true, true, false);

		}

	}
}
