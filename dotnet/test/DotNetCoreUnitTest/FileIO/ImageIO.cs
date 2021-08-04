using System;
using GeneXus.Application;
using GeneXus.Configuration;
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

		}
	}
}
