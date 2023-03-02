using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Http.Client;
using GeneXus.XML;
using Xunit;

namespace DotNetCoreUnitTest.HttpClientTest
{
	public class StreamHandling
	{
		[Fact]
		public void CreateHttpClient()
		{
			using (GxHttpClient client = new GxHttpClient())
			{
				client.Execute("GET", "https://raw.githubusercontent.com/OFFLINE-GmbH/Online-FTP-S3/master/phpunit.xml");
				Assert.Equal(client.StatusCode, (short)System.Net.HttpStatusCode.OK);
				Assert.True(client.ToString().Length > 0);

				// No try to consume the string again in order to verify it works several times.
				Assert.True(client.ToString().Length > 0);

				using (GXXMLReader reader = new GXXMLReader())
				{
					reader.OpenResponse(client);
					Assert.True(reader.Read() > 0);
				}

				Assert.True(client.ToString().Length > 0);
			}
		}
		[Fact]
		public void HttpClientAbsoluteURLOnExecute()
		{
			using (GxHttpClient client = new GxHttpClient())
			{
				string url = "https://www.google.com/";
				client.Port = 80;
				client.Execute("GET", url);
				string requestUrl = client.GetRequestURL(url);
				Assert.Equal(client.StatusCode, (short)System.Net.HttpStatusCode.OK);
				Assert.True(Uri.IsWellFormedUriString(requestUrl, UriKind.Absolute), "GetRequestURL is an invalid url which will cause Invalid URI at execute");

			}
		}
		[Fact]
		public void HttpClientRelativeURLOnExecute_1()
		{
			using (GxHttpClient client = new GxHttpClient())
			{
				string url = "imghp";
				client.Port = 80;
				client.BaseURL = "https://www.google.com/";
				client.Execute("GET", url);
				string requestUrl = client.GetRequestURL(url);
				Assert.Equal(client.StatusCode, (short)System.Net.HttpStatusCode.OK);
				Assert.True(Uri.IsWellFormedUriString(requestUrl, UriKind.Absolute), "GetRequestURL is an invalid url which will cause Invalid URI at execute");

			}
		}
		[Fact]
		public void HttpClientRelativeURLOnExecute_2()
		{
			using (GxHttpClient client = new GxHttpClient())
			{
				string url = "/imghp";
				client.Port = 80;
				client.BaseURL = "https://www.google.com";
				client.Execute("GET", url);
				string requestUrl = client.GetRequestURL(url);
				Assert.Equal(client.StatusCode, (short)System.Net.HttpStatusCode.OK);
				Assert.True(Uri.IsWellFormedUriString(requestUrl, UriKind.Absolute), "GetRequestURL is an invalid url which will cause Invalid URI at execute");

			}
		}
	}
}
