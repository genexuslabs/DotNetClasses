using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Http;
using GeneXus.Metadata;
using Xunit;
namespace xUnitTesting
{
	public class WebPanelTest : MiddlewareTest
	{
		public WebPanelTest():base()
		{
			ClassLoader.FindType("webhook", "GeneXus.Programs", "webhook", Assembly.GetExecutingAssembly(), true);//Force loading assembly for webhook procedure
			server.AllowSynchronousIO=true;
		}

		[Fact]
		public async Task HttpFirstPost()
		{
			HttpClient client = server.CreateClient();
			HttpResponseMessage response = await client.PostAsync("webhook.aspx", null);
			IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
			foreach (string cookie in cookies)
			{
				Assert.False(cookie.StartsWith(HttpHeader.X_GXCSRF_TOKEN));
			}
			response.EnsureSuccessStatusCode();
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
		}
		[Fact]
		public async Task HttpFirstGet()
		{
			HttpClient client = server.CreateClient();
			HttpResponseMessage response = await client.GetAsync("webhook.aspx");
			IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
			foreach (string cookie in cookies)
			{
				Assert.False(cookie.StartsWith(HttpHeader.X_GXCSRF_TOKEN));
			}

			response.EnsureSuccessStatusCode();
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
		}
	}

}
