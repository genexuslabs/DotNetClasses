using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using GeneXus.Metadata;
using Xunit;
namespace xUnitTesting
{
	public class HttpProcTest : MiddlewareTest
	{
		Dictionary<string, string> parms = new Dictionary<string, string>();
		FormUrlEncodedContent formUrlEncodedContent;
		public HttpProcTest():base()
		{
			ClassLoader.FindType("aprochttpgetstatic", "GeneXus.Programs", "aprochttpgetstatic", Assembly.GetExecutingAssembly(), true);//Force loading assembly for webhook procedure
			server.AllowSynchronousIO=true;
			parms.Add("client_id", "SM40d2cbda93b2de0a15df7a1598c7db83");
			parms.Add("refresh_token", "99");
			formUrlEncodedContent = new FormUrlEncodedContent(parms);
		}
		[Fact]
		public async Task HtttpPostTest()
		{
			HttpClient client = server.CreateClient();

			HttpResponseMessage response = await client.PostAsync("aprochttpgetstatic.aspx", formUrlEncodedContent);//"application/x-www-form-urlencoded"
			response.EnsureSuccessStatusCode();
			string resp = await response.Content.ReadAsStringAsync();
			Assert.NotEmpty(resp);
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode); 
		}
	}

}
