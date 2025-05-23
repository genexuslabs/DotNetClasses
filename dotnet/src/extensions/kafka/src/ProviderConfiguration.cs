using System.Collections.Generic;

namespace GeneXus.Messaging.Core
{
	public class ProviderConfiguration
	{
		public string Key { get; set; }
		public string Value { get; set; }
		public int IntValue { get; set; }

		public List<ProviderConfiguration> NestedValue { get; set; }
	}
}
