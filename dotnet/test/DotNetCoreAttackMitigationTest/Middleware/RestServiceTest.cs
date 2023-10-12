using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using GeneXus.Http;
using GeneXus.Metadata;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace xUnitTesting
{

	public class RestServiceTest : MiddlewareTest
	{
		public RestServiceTest() : base()
		{
			ClassLoader.FindType("apps.append", "GeneXus.Programs.apps", "append", Assembly.GetExecutingAssembly(), true);//Force loading assembly for append procedure
			ClassLoader.FindType("apps.saveimage", "GeneXus.Programs.apps", "saveimage", Assembly.GetExecutingAssembly(), true);//Force loading assembly for saveimage procedure
			server.AllowSynchronousIO = true;
			ClassLoader.FindType("webhook", "GeneXus.Programs", "webhook", Assembly.GetExecutingAssembly(), true);//Force loading assembly for webhook procedure
			server.AllowSynchronousIO = true;

		}

		[Fact]
		public async Task TestSimpleRestPost()
		{
			server.AllowSynchronousIO = true;
			HttpClient client = server.CreateClient();
			StringContent body = new StringContent("{\"Image\":\"imageName\",\"ImageDescription\":\"imageDescription\"}");
			HttpResponseMessage response = await client.PostAsync("rest/apps/saveimage", body);
			Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
		}

		[Fact]
		public async Task RunController()
		{


			CookieContainer cookies = new System.Net.CookieContainer();
			HttpClient client = server.CreateClient();
			string requestUri = "rest/apps/append";
			Uri requestUriObj = new Uri("http://localhost/" + requestUri);
			HttpResponseMessage response = await client.GetAsync(requestUri);
			string csrfToken = string.Empty;

			IEnumerable<string> values;
			Assert.True(response.Headers.TryGetValues("Set-Cookie", out values));

			foreach (var item in SetCookieHeaderValue.ParseList(values.ToList()))
				cookies.Add(requestUriObj, new Cookie(item.Name.Value, item.Value.Value, item.Path.Value));

			var setCookie = SetCookieHeaderValue.ParseList(values.ToList()).FirstOrDefault(t => t.Name.Equals(HttpHeader.X_CSRF_TOKEN_COOKIE, StringComparison.OrdinalIgnoreCase));
			csrfToken = setCookie.Value.Value;

			response.EnsureSuccessStatusCode();
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode); //When failed, turn on log.config to see server side error.

			StringContent body = new StringContent("{\"Image\":\"imageName\",\"ImageDescription\":\"imageDescription\"}");
			client.DefaultRequestHeaders.Add(HttpHeader.X_CSRF_TOKEN_HEADER, csrfToken);
			client.DefaultRequestHeaders.Add("Cookie", values);// //cookies.GetCookieHeader(requestUriObj));

			response = await client.PostAsync("rest/apps/saveimage", body);
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
		}
		[Fact]
		public async Task HttpFirstPost()
		{
			HttpClient client = server.CreateClient();
			HttpResponseMessage response = await client.PostAsync("webhook.aspx", null);
			IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
			foreach (string cookie in cookies)
			{
				Assert.False(cookie.StartsWith(HttpHeader.X_CSRF_TOKEN_COOKIE));
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
				Assert.False(cookie.StartsWith(HttpHeader.X_CSRF_TOKEN_COOKIE));
			}

			response.EnsureSuccessStatusCode();
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
		}
	}
}
