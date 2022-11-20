using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using GeneXus.Configuration;
using GeneXus.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Xunit;
using xUnitTesting;

namespace DotNetCoreUnitTest.Middleware
{
	public class AspNetCoreTestConfigurationOverride : MiddlewareTest
	{
		const string ConfigSettingsProgramName = "configsettings";
		const string ConfigSettingsProgramModule = "apps";
		const string DEVELOPMENT_VALUE = "DEVELOPMENT_VALUE";
		const string MY_CUSTOM_PTY = "MY_CUSTOM_PTY";
		public AspNetCoreTestConfigurationOverride()
		{
			ClassLoader.FindType($"{ConfigSettingsProgramModule}.{ConfigSettingsProgramName}", $"GeneXus.Programs.{ConfigSettingsProgramModule}", ConfigSettingsProgramName, Assembly.GetExecutingAssembly(), true);//Force loading assembly
		}
		[Fact]
		public async Task TestConfigurationSettingOverridenInDevelopment()
		{
			server.AllowSynchronousIO = true;
			HttpClient client = server.CreateClient();

			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("ConfigurationSetting", MY_CUSTOM_PTY);

			HttpResponseMessage response = await client.PostAsync($"rest/{ConfigSettingsProgramModule}/{ConfigSettingsProgramName}", null);
			response.EnsureSuccessStatusCode();
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

			string originHeader = GetHeader(response, "ConfigurationSettingValue");
			Assert.Equal(DEVELOPMENT_VALUE, originHeader);
		}
		

	}
}
