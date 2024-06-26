using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using GeneXus.Metadata;
using GeneXus.Utils;
using Xunit;
namespace xUnitTesting
{
	public class HeadersTest : MiddlewareTest
	{
		public HeadersTest() : base()
		{
			ClassLoader.FindType("apps.httpheaders", "GeneXus.Programs.apps", "httpheaders", Assembly.GetExecutingAssembly(), true);//Force loading assembly for append procedure
			server.AllowSynchronousIO = true;

		}
		protected override void SetEnvironmentVars()
		{
			Environment.SetEnvironmentVariable("ASPNETCORE_FORWARDEDHEADERS_ENABLED", "true", EnvironmentVariableTarget.Process);

		}
		[Fact]
		public async Task TestForwardedHeaders()
		{
			const string host = "192.168.1.100";
			const string scheme = "https";
			const string remoteUrl = $"{scheme}:\\/\\/{host}";
			const string passwordWithSpecialCharacters = "mypasswordwithspecialcharacters:!*";
			const string userId = "myuser";
			HttpClient client = server.CreateClient();
			client.DefaultRequestHeaders.Add("X-Forwarded-For", host);
			client.DefaultRequestHeaders.Add("X-Forwarded-Proto", scheme);
			client.DefaultRequestHeaders.Add("Authorization", $"Basic {StringUtil.ToBase64(userId+ ":" + passwordWithSpecialCharacters)}");

			HttpResponseMessage response = await client.GetAsync("/rest/apps/httpheaders");
			response.EnsureSuccessStatusCode();
			string resp = await response.Content.ReadAsStringAsync();
			Assert.Contains(remoteUrl, resp, StringComparison.OrdinalIgnoreCase);
			Assert.Contains(userId, resp, StringComparison.OrdinalIgnoreCase);
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
		}
		
	}
}