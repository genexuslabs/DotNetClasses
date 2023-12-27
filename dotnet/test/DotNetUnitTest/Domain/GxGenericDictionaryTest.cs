using GeneXus.Utils;
using Xunit;

namespace xUnitTesting
{
	public class GxGenericDictionaryTest
	{
		[Fact]
		public void ToJsonTest()
		{
			GxGenericDictionary<string, int> dic = new GxGenericDictionary<string, int>
			{
				{ "key1", 1 },
				{ "key2", 2 }
			};
			string json = dic.ToJson();
			string expectedJson = "{\"key1\":1,\"key2\":2}";
			Assert.Equal(expectedJson, json);
		}

	}
}
