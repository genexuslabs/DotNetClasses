using System.IO;
using System.Net;
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

		[Fact]
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

		[Fact]
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
	}
}
