using Xunit;
using GeneXus.Application;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeneXus.Utils;

namespace xUnitTesting
{
	public class ReadRestParameters
	{
		[Fact]
		public void TestProcRestParameters()
		{
			string parameters = "{\"Geolocations\":[{\"Description\":\"LocationInfo(fused)\",\"Heading\":\" - 1\",\"Location\":\" - 34.8782486,-56.0789207\",\"Precision\":\"17.585\",\"Speed\":\" - 1\",\"Time\":\"2019 - 10 - 23 16:43:45\"},{\"Description\":\"LocationInfo(fused)\",\"Heading\":\" - 1\",\"Location\":\" - 34.8782486,-56.0789207\",\"Precision\":\"17.585\",\"Speed\":\" - 1\",\"Time\":\"2019 - 10 - 23 16:44:05\"},{\"Description\":\"LocationInfo(fused)\",\"Heading\":\" - 1\",\"Location\":\" - 34.8783232,-56.0788582\",\"Precision\":\"14.999\",\"Speed\":\" - 1\",\"Time\":\"2019 - 10 - 23 16:44:25\"},{\"Description\":\"LocationInfo(fused)\",\"Heading\":\" - 1\",\"Location\":\" - 34.8783232,-56.0788582\",\"Precision\":\"14.999\",\"Speed\":\" - 1\",\"Time\":\"2019 - 10 - 23 16:44:46\"}], \"SDT1\":{\"a\":1, \"b\":\"hola\"}, \"charcollection\":[\"uno\", \"dos\", \"tres\"]}";
			MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(parameters));

			Dictionary<string, object> parms = RestAPIHelpers.ReadRestBodyParameters(stream);
			Assert.Equal(3, parms.Count);
		}
	}
}
