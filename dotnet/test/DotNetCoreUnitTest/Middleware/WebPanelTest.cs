using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Metadata;
using Xunit;
namespace xUnitTesting
{
	public class WebPanelTest : MiddlewareTest
	{
		Dictionary<string, string> parms = new Dictionary<string, string>();
		FormUrlEncodedContent formUrlEncodedContent;
		public WebPanelTest():base()
		{
			ClassLoader.FindType("webhook", "GeneXus.Programs", "webhook", Assembly.GetExecutingAssembly(), true);//Force loading assembly for webhook procedure
			server.AllowSynchronousIO=true;
			parms.Add("SmsMessageSid", "SM40d2cbda93b2de0a15df7a1598c7db83");
			parms.Add("NumMedia", "99");
			parms.Add("WaId", "5215532327636");
			formUrlEncodedContent = new FormUrlEncodedContent(parms);
		}
		[Fact]
		public async Task HtttpResponseBodyNotEmpty_WhenFormURLEncoded()
		{
			HttpClient client = server.CreateClient();

			HttpResponseMessage response = await client.PostAsync("webhook.aspx", formUrlEncodedContent);//"application/x-www-form-urlencoded"
			response.EnsureSuccessStatusCode();
			string resp = await response.Content.ReadAsStringAsync();
			string expected = await formUrlEncodedContent.ReadAsStringAsync();
			Assert.Equal(expected, resp);
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode); 
		}
		[Fact]
		public async Task HtttpResponseBodyNotEmpty_WhenTextPlain()
		{
			HttpClient client = server.CreateClient();
			string plainText = await formUrlEncodedContent.ReadAsStringAsync();
			StringContent content = new StringContent(plainText, Encoding.UTF8, "text/plain");
			HttpResponseMessage response = await client.PostAsync("webhook.aspx", content);
			response.EnsureSuccessStatusCode();
			string resp = await response.Content.ReadAsStringAsync();
			string expected = await content.ReadAsStringAsync();
			Assert.Equal(expected, resp);
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
		}
	}

}
