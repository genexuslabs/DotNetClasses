using Xunit;
using GeneXus.Application;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeneXus.Cache;

namespace xUnitTesting
{
	public class MemcachedTest
	{
		//[Fact]
		public void TestCache()
		{
			string value;
			List<string> keys = new List<string>();
			List<string> values = new List<string>();
			Memcached cache = new Memcached();
			cache.Set<string>("db", "mykey", "myvalue",-1);
			cache.Get<string>("db", "mykey", out value);
			Assert.Equal("myvalue", value);
			keys.Add("car");
			keys.Add("brand");
			values.Add("1");
			values.Add("toyota");
			cache.SetAll<string>("DB", keys, values, 0);
			IDictionary<string, string> cachedValues = cache.GetAll<string>("DB", keys);
			Assert.Equal(2, cachedValues.Count);
			Assert.True(cachedValues.Values.Contains("1"));
			Assert.True(cachedValues.Values.Contains("toyota"));
		}
	}
}
