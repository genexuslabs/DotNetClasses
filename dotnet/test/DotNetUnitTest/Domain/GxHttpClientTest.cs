using System.Net;
using GeneXus.Http.Client;
using Xunit;

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

	}
}
