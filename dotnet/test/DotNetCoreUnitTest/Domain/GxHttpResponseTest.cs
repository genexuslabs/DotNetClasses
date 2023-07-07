using System;
using GeneXus.Application;
using GeneXus.Http.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace xUnitTesting
{

	public class GxHttpResponseTest
	{
		[Fact]
		public void CacheControlHeaderTest()
		{
			DefaultHttpContext httpContext = new DefaultHttpContext();

			httpContext.Features.Set<IHttpRequestFeature>(new HttpRequestFeature());
			httpContext.Features.Set<IHttpResponseFeature>(new HttpResponseFeature());

			int maxAgeSeconds = 1800;
			int sharedMaxAgeSeconds = 3900;
			GxContext context = new GxContext();
			context.HttpContext = httpContext;
			GxHttpResponse httpresponse = new GxHttpResponse(context);
			httpresponse.AppendHeader("Cache-Control", $"public, s-maxage={sharedMaxAgeSeconds}, max-age={maxAgeSeconds}, stale-while-revalidate=15, stale-if-error=3600");

			CacheControlHeaderValue cacheControlHeaderValue = httpresponse.Response.GetTypedHeaders().CacheControl;

			TimeSpan maxAge = TimeSpan.FromSeconds(maxAgeSeconds);
			TimeSpan sMaxAge = TimeSpan.FromSeconds(sharedMaxAgeSeconds);

			Assert.True(cacheControlHeaderValue.Public);
			Assert.Equal(sMaxAge, cacheControlHeaderValue.SharedMaxAge);
			Assert.Equal(maxAge, cacheControlHeaderValue.MaxAge);
			Assert.Contains("stale-if-error=3600", cacheControlHeaderValue.ToString(), StringComparison.OrdinalIgnoreCase);
		}
	
	}
}
