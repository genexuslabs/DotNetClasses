using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Http.Client;
using GeneXus.XML;
using Xunit;

namespace DotNetCoreUnitTest.HttpClient
{
	public class StreamHandling
	{
		[Fact]
		public void CreateHttpClient()
		{
			GxHttpClient client = new GxHttpClient();

			client.Execute("GET", "https://raw.githubusercontent.com/OFFLINE-GmbH/Online-FTP-S3/master/phpunit.xml");
			Assert.Equal(client.StatusCode, (short) System.Net.HttpStatusCode.OK);
			Assert.True( client.ToString().Length > 0);

			// No try to consume the string again in order to verify it works several times.
			Assert.True(client.ToString().Length > 0);

			GXXMLReader reader = new GXXMLReader();
			reader.OpenResponse(client);
			Assert.True(reader.Read() > 0);

			Assert.True(client.ToString().Length > 0);


		}
	}
}
