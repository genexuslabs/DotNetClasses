using System;
using GeneXus.Application;
using GeneXus.Configuration;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace UnitTesting
{
	public class ImageIO
	{
		[Fact]
		public void GetCompleteURL()
		{
			GxContext context = new GxContext();
			string imageUrl = "data:image/png;base64";
			context.StaticContentBase = "StaticResources/";
			string url = context.GetCompleteURL(imageUrl);
			Assert.Equal(imageUrl, url);

			var httpctx = new DefaultHttpContext();
			context.HttpContext = httpctx;
			httpctx.Request.PathBase = new PathString("/VirtualDirectoryName");
			httpctx.Request.Host = new HostString("localhost");
			httpctx.Request.Scheme = Uri.UriSchemeHttp;
			imageUrl = "/hostrelative/image.png";
			context.StaticContentBase = string.Empty;
			url = context.GetCompleteURL(imageUrl);
			Assert.StartsWith(context.GetContextPath(), url, StringComparison.InvariantCulture);
		}
	}
}
