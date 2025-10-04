using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotNetUnitTest;
using GeneXus.Application;
using GeneXus.Http.Client;
using Xunit;



#if !NETCORE
using System.Web.SessionState;
using System.Web;
#endif

namespace xUnitTesting
{

	public class GxHttpClientTest
	{
		const int MAX_CONNECTIONS = 5;

		public GxHttpClientTest()
		{
			Environment.SetEnvironmentVariable("GX_HTTPCLIENT_MAX_PER_ROUTE", MAX_CONNECTIONS.ToString(), EnvironmentVariableTarget.Process);
		}
		[Fact]
		public void HttpClientEmptyURLOnExecute()
		{
			using (GxHttpClient client = new GxHttpClient())
			{
				client.Host = "api.saia.ai";
				client.BaseURL = "/v1/forecast?timezone=America%2FMontevideo";
				string requestUrl = client.GetRequestURL(string.Empty);
				Assert.EndsWith("Montevideo", requestUrl, StringComparison.Ordinal);
			}
		}

		[Fact]
		public void AddHeaderWithSpecialCharactersDoesNotThrowException()
		{
			using (GxHttpClient httpclient = new GxHttpClient())
			{
				string headerValue = "d3890093-289b-4f87-adad-f2ebea826e8f!8db3bc7ac3d38933c3b0c91a3bcdab60b9bbb3f607a1c9b312b24374e750243f3a31d7e90a4c55@SSORT!d3890093-289b-4f87-adad-f2ebea826e8f!8c7564ac08514ff988ba6c8c6ba3fc0c";
				string headerName = "Authorization";
				httpclient.AddHeader(headerName, headerValue);
				httpclient.Host = "accountstest.genexus.com";
				httpclient.Secure = 1;
				httpclient.BaseURL = @"oauth/gam/v2.0/dummy/requesttokenanduserinfo";
				httpclient.Execute("GET", string.Empty);
				Assert.NotEqual(((int)HttpStatusCode.InternalServerError), httpclient.StatusCode);
			}
		}

		[Fact(Skip ="For Local Test")]
		public void HttpClientInvalidURLWithCustomPort()
		{
			using (GxHttpClient httpclient = new GxHttpClient())
			{
				string headerValue = "d3890093-289b-4f87-adad-f2ebea826e8f!8db3bc7ac3d38933c3b0c91a3bcdab60b9bbb3f607a1c9b312b24374e750243f3a31d7e90a4c55@SSORT!d3890093-289b-4f87-adad-f2ebea826e8f!8c7564ac08514ff988ba6c8c6ba3fc0c";
				string headerName = "Authorization";
				httpclient.AddHeader(headerName, headerValue);
				httpclient.Host = "sistema.planoscs.com.br";
				httpclient.Port = 70;
				httpclient.BaseURL = @"AnnA/Prestador/Parser.php";
				httpclient.Execute("GET", string.Empty);
				Assert.NotEqual(((int)HttpStatusCode.InternalServerError), httpclient.StatusCode);
			}
		}

		[WindowsOnlyFact]
		public void HttpClientCookieHeader()
		{
			string headerValue = "CognitoIdentityServiceProvider.3tgmin25m9bkg6vgi7vpavu7a9.M00000936.refreshToken=eyJjdHkiOiJKV1QiLCJlbmMiSkRCAmMpYqndvORnWLTfHw; CognitoIdentityServiceProvider.3tgmin25m9bkg6vgi7vpavu7a9.LastAuthUser=M00000936";
			string headerName = "Cookie";
			using (GxHttpClient httpclient = new GxHttpClient())
			{
				httpclient.AddHeader(headerName, headerValue);
				httpclient.Host = "localhost";
				httpclient.Port = 80;
				httpclient.BaseURL = @"NotFound/NotFound.php";
				httpclient.HttpClientExecute("GET", string.Empty);
				Assert.NotEqual(((int)HttpStatusCode.InternalServerError), httpclient.StatusCode);
			}
			using (GxHttpClient oldHttpclient = new GxHttpClient())
			{
				oldHttpclient.AddHeader(headerName, headerValue);
				oldHttpclient.Host = "localhost";
				oldHttpclient.Port = 80;
				oldHttpclient.BaseURL = @"NotFound/NotFound.php";
				oldHttpclient.Execute("GET", string.Empty);
				Assert.NotEqual(((int)HttpStatusCode.InternalServerError), oldHttpclient.StatusCode);
			}
		}
#if NETCORE
		[Fact(Skip = "For local testing only")]
		public void TestHttpClientMaxPoolSize()
		{
			HttpClientMaxPoolSize().Wait();
		}
		async Task HttpClientMaxPoolSize() {
			GxContext context = new GxContext();
			string baseUrl = "http://localhost:8082/HttpClientTestNETSQLServer/testcookies.aspx";

			var tasks = new List<Task>();

			for (int i = 0; i < MAX_CONNECTIONS * 10; i++)
			{
				string url = $"{baseUrl}?id=" + i;
				tasks.Add(Task.Run(() => ExecuteGet(url)));
			}
			await Task.WhenAll(tasks);

			Assert.Single(GxHttpClient._httpClientInstances);

			HttpClient c = GxHttpClient._httpClientInstances.First().Value;
			Assert.NotNull(c);

			Assert.True(pendingRequestsCount <= MAX_CONNECTIONS, $"Active connections ({pendingRequestsCount}) exceed MaxConnectionsPerServer ({MAX_CONNECTIONS})");
		}
		static private int pendingRequestsCount = 0;
		static private readonly object syncObject = new object();
		static private void IncrementPendingRequestsCount()
		{
			lock (syncObject)
			{
				pendingRequestsCount++;
			}
		}

		static private void DecrementPendingRequestsCount()
		{
			lock (syncObject)
			{
				pendingRequestsCount--;
			}
		}
		private string ExecuteGet(string url)
		{
			GxContext context = new GxContext();
			using (GxHttpClient httpclient = new GxHttpClient(context))
			{
				IncrementPendingRequestsCount();
				httpclient.HttpClientExecute("GET", url);
				Assert.Equal((int)HttpStatusCode.OK, httpclient.StatusCode); //When failed, turn on log.config to see server side error.
				DecrementPendingRequestsCount();
				return httpclient.ToString();
			}
		}

#endif
		[Fact(Skip = "For local testing only")]
		public void HttpClientCookiesTest()
		{
			GxContext context = new GxContext();
			string baseUrl = "http://localhost:8082/HttpClientTestNETSQLServer/testcookies.aspx";

			using (GxHttpClient httpclient = new GxHttpClient(context))
			{
				string url = $"{baseUrl}?id=1";
				httpclient.HttpClientExecute("GET", url);
				Assert.Equal((int)HttpStatusCode.OK, httpclient.StatusCode);
				CookieContainer cookies = context.GetCookieContainer(url, true);
				Assert.NotNull(cookies);
				CookieCollection responseCookies = cookies.GetCookies(new Uri(url));
				Assert.NotEmpty(responseCookies);
				string result = httpclient.ToString();
				Assert.Contains("1", result, StringComparison.OrdinalIgnoreCase);

			}
			using (GxHttpClient httpclient = new GxHttpClient(context))
			{
				string url = $"{baseUrl}?id=2";
				httpclient.IncludeCookies = true;
				httpclient.HttpClientExecute("GET", url);
				Assert.Equal((int)HttpStatusCode.OK, httpclient.StatusCode);
				string result = httpclient.ToString();
				Assert.StartsWith("Cookie found ", result, StringComparison.OrdinalIgnoreCase);
				Assert.Contains("2", result, StringComparison.OrdinalIgnoreCase);
			}
			using (GxHttpClient httpclient = new GxHttpClient(context))
			{
				string url = $"{baseUrl}?id=3";
				httpclient.IncludeCookies = false;
				httpclient.HttpClientExecute("GET", url);
				Assert.Equal((int)HttpStatusCode.OK, httpclient.StatusCode);
				string result = httpclient.ToString();
				Assert.StartsWith("Cookie not found", result, StringComparison.OrdinalIgnoreCase);
				Assert.Contains("3", result, StringComparison.OrdinalIgnoreCase);
			}
			using (GxHttpClient httpclient = new GxHttpClient(context))
			{
				string url = "https://www.google.com/";
				httpclient.HttpClientExecute("GET", url);
				Assert.Equal((int)HttpStatusCode.OK, httpclient.StatusCode);
				CookieContainer cookies = context.GetCookieContainer(url, true);
				Assert.NotNull(cookies);
				CookieCollection responseCookies = cookies.GetCookies(new Uri(url));
				Assert.NotEmpty(responseCookies);
				string result = httpclient.ToString();
			}

		}

#if !NETCORE
		[Fact]
		public void NoStoreHeader()
		{
			var httpRequest = new HttpRequest("", "http://localhost/", "");
			var httpResponce = new HttpResponse(new StringWriter());
			var httpContext = new HttpContext(httpRequest, httpResponce);
			HttpContext.Current = httpContext;

			GxContext gxcontext = new GxContext();
			gxcontext.HttpContext = HttpContext.Current;
			byte result = gxcontext.SetHeader("CACHE", "no-store");
			Assert.Equal(0, result);
		}
#endif

#if NETCORE
		[Fact]
		public void BasicAuthenticationIncludesHeader()
		{
			GxContext context = new GxContext();
			using (GxHttpClient httpclient = new GxHttpClient(context))
			{
				string url= "https://www.google.com/";
				string username = "user";
				string password = "pass";
				string credentialsBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
				httpclient.AddAuthentication(0, string.Empty, username, password);

				HttpRequestMessage request = new HttpRequestMessage()
				{
					RequestUri = new Uri(url),
					Method = HttpMethod.Post,
				};
				httpclient.SetHeaders(request, null, out string contentType);
				string headerValue = request.Headers.GetValues("Authorization").FirstOrDefault();
				Assert.Equal(headerValue, $"Basic {credentialsBase64}");
			}
		}
#endif
	}

}
