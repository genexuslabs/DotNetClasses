using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using GeneXus.Metadata;
using Microsoft.Net.Http.Headers;
using Xunit;
namespace xUnitTesting
{
	public class SessionTest : MiddlewareTest
	{
		public SessionTest() : base()
		{
			ClassLoader.FindType("apps.createsession", "GeneXus.Programs.apps", "createsession", Assembly.GetExecutingAssembly(), true);//Force loading assembly for append createsession
			ClassLoader.FindType("apps.returnsession", "GeneXus.Programs.apps", "returnsession", Assembly.GetExecutingAssembly(), true);//Force loading assembly for saveimage returnsession
			server.AllowSynchronousIO = true;
		}
		[Fact]
		public void TestConcurrentRequest()
		{
			HttpClient client = server.CreateClient();
			CreateSession(client).Wait();
			TestSessionGrids(client).Wait();

		}
		CookieHeaderValue SessionCookie;
		async Task CreateSession(HttpClient client)
		{
			string[] url1 = new string[]{ "rest/apps/createsession?gxid=48796585&Type=C1&Title=Component",
										"rest/apps/createsession?gxid=20200694&Type=C3&Title=Component%203",
										"rest/apps/createsession?gxid=30382714&Type=C2&Title=Component%202",
										"rest/apps/createsession?gxid=9559029&Type=C4&Title=Component%204",
										"rest/apps/createsession?gxid=51975664&Type=C5&Title=Component%205",
										"rest/apps/createsession?gxid=51975665&Type=C6&Title=Component%206",
										"rest/apps/createsession?gxid=51975666&Type=C7&Title=Component%207",
										"rest/apps/createsession?gxid=51975667&Type=C8&Title=Component%208",
										"rest/apps/createsession?gxid=51975668&Type=C9&Title=Component%209",
										"rest/apps/createsession?gxid=51975669&Type=C10&Title=Component%210",
										"rest/apps/createsession?gxid=51975670&Type=C11&Title=Component%211",
										"rest/apps/createsession?gxid=51975671&Type=C12&Title=Component%212"};

			//Fix sessionID
			var c = await client.GetAsync(url1[0]);
			string result = await c.Content.ReadAsStringAsync();
			Console.WriteLine(result);
			foreach (var header in c.Headers)
			{
				if (header.Key == "Set-Cookie")
				{
					string cookieValue = header.Value.First();
					string[] cookieValues = cookieValue.Split(';', '=');
					SessionCookie = new CookieHeaderValue(cookieValues[0], cookieValues[1]);
					client.DefaultRequestHeaders.Add("Cookie", SessionCookie.ToString());
				}
			}
			List<Task> tasks = new List<Task>();
			foreach (string s1 in url1)
			{
				tasks.Add(Task.Run(() => ExecuteGet(client, s1)));
			}

			await Task.WhenAll(tasks);
		}
		async Task ExecuteGet(HttpClient client, string url)
		{
			var r = await client.GetAsync(url);
		}
		async Task ExecuteGetGrid(HttpClient client, string url)
		{
			var r = await client.GetAsync(url);
			string result = await r.Content.ReadAsStringAsync();
			int idx = url.IndexOf("gxid=");

			string gxidAndTyep = url.Substring(idx);
			gxidAndTyep = gxidAndTyep.Substring(0, gxidAndTyep.IndexOf("&Title"));

			Assert.Contains(gxidAndTyep, result, StringComparison.OrdinalIgnoreCase);
		}
		async Task TestSessionGrids(HttpClient client)
		{
			string[] url2 = new string[] {"rest/apps/returnsession?gxid=48796585&Type=C1&Title=Component%201&start=0&count=30",
										"rest/apps/returnsession?gxid=30382714&Type=C2&Title=Component%202&start=0&count=30",
										"rest/apps/returnsession?gxid=51975664&Type=C5&Title=Component%205&start=0&count=30",
										"rest/apps/returnsession?gxid=9559029&Type=C4&Title=Component%204&start=0&count=30",
										"rest/apps/returnsession?gxid=20200694&Type=C3&Title=Component%203&start=0&count=30",
										"rest/apps/returnsession?gxid=51975665&Type=C6&Title=Component%206&start=0&count=30",
										"rest/apps/returnsession?gxid=51975666&Type=C7&Title=Component%207&start=0&count=30",
										"rest/apps/returnsession?gxid=51975667&Type=C8&Title=Component%208&start=0&count=30",
										"rest/apps/returnsession?gxid=51975668&Type=C9&Title=Component%209&start=0&count=30",
										"rest/apps/returnsession?gxid=51975669&Type=C10&Title=Component%210&start=0&count=30",
										"rest/apps/returnsession?gxid=51975670&Type=C11&Title=Component%211&start=0&count=30",
										"rest/apps/returnsession?gxid=51975671&Type=C12&Title=Component%212&start=0&count=30",};
			List<Task> tasksGrid = new List<Task>();
			foreach (string s1 in url2)
			{
				tasksGrid.Add(Task.Run(() => ExecuteGetGrid(client, s1)));
			}
			await Task.WhenAll(tasksGrid);
		}
	}

}
