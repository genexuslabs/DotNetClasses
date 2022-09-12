using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using GeneXus.Configuration;
using GeneXus.Metadata;
using Microsoft.Net.Http.Headers;
using Xunit;
using xUnitTesting;

namespace DotNetCoreUnitTest.Middleware
{
	public class CorsTest : MiddlewareTest
	{
		const string HttpCorsProgramName = "httpcors";
		const string HttpCorsProgramModule = "apps";
		string Origin = Preferences.CorsAllowedOrigins();
		string[] Headers = { "authorization","cache-control", "deviceid", "devicetype", "genexus-agent", "gxtzoffset" };
		public CorsTest()
		{
			ClassLoader.FindType($"{HttpCorsProgramModule}.{HttpCorsProgramName}", $"GeneXus.Programs.{HttpCorsProgramModule}", HttpCorsProgramName, Assembly.GetExecutingAssembly(), true);//Force loading assembly
			Assert.NotEmpty(Origin);
		}
		[Fact]
		public async Task TestCorsOnPost()
		{
			server.AllowSynchronousIO = true;
			HttpClient client = server.CreateClient();
			string deviceIdHeaderName = "deviceid";
			string deviceIdHeaderValue = "AndroidDevice";
			string contentType = "application/json";

			client.DefaultRequestHeaders.Add(HeaderNames.Origin, Origin);
			client.DefaultRequestHeaders.Add(HeaderNames.AccessControlRequestHeaders, Headers);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType)); 
			client.DefaultRequestHeaders.Add(deviceIdHeaderName, deviceIdHeaderValue);

			HttpResponseMessage response = await client.PostAsync($"rest/{HttpCorsProgramModule}/{HttpCorsProgramName}", null);
			response.EnsureSuccessStatusCode();
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

			string originHeader = GetHeader(response, HeaderNames.AccessControlAllowOrigin);
			Assert.Equal(Origin, originHeader);

			string deviceidHeader = GetHeader(response, deviceIdHeaderName);
			Assert.Equal(deviceIdHeaderValue, deviceidHeader);
		}
		[Fact]
		public async Task TestCorsOnPreflightRequest()
		{
			server.AllowSynchronousIO = true;
			HttpClient client = server.CreateClient();
			client.DefaultRequestHeaders.Add(HeaderNames.Origin, Origin);
			client.DefaultRequestHeaders.Add(HeaderNames.AccessControlRequestHeaders, Headers);
			client.DefaultRequestHeaders.Add(HeaderNames.AccessControlRequestMethod, HttpMethod.Options.Method);

			HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Options, $"rest/{HttpCorsProgramModule}/{HttpCorsProgramName}"));
			response.EnsureSuccessStatusCode();
			Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
			
			string originHeader = GetHeader(response, HeaderNames.AccessControlAllowOrigin);
			Assert.Equal(Origin, originHeader);

			string methodsHeader = GetHeader(response, HeaderNames.AccessControlAllowMethods);
			Assert.Equal(HttpMethod.Options.Method, methodsHeader);

			string credentialsHeader = GetHeader(response, HeaderNames.AccessControlAllowCredentials);
			Assert.Equal("true", credentialsHeader);

			string headersHeader = GetHeader(response, HeaderNames.AccessControlAllowHeaders);
			foreach(string header in Headers)
			{
				Assert.Contains(header, headersHeader, StringComparison.OrdinalIgnoreCase);
			}
		}
		private string GetHeader(HttpResponseMessage response, string headerName)
		{
			if (response.Headers.TryGetValues(headerName, out IEnumerable<string> value))
				return value.First();
			else
				return string.Empty;

		}

	}
}
