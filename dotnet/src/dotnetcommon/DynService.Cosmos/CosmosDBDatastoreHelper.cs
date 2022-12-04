using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Data.Cosmos;

namespace GeneXus.Data.NTier
{
	public class CosmosDBDatastoreHelper : DynServiceDataStoreHelperBase
	{

		//TODO Esto no aplica a CosmosDB
		public CosmosDBQuery NewQuery() => new CosmosDBQuery(this);

		public CosmosDBQuery NewScan() => new CosmosDBQuery(this);

		public CosmosDBMap Map(string name)
		{
			return new CosmosDBMap(name);
		}

	}
	public class CosmosDBQuery : Query
	{
		public string Index { get; set; }
		
		public override Query OrderBy(string index)
		{
			////TO DO///
			return this;
		}

		public string PartitionKey { get; private set; }
		public override Query SetKey(string partitionKey)
		{
			PartitionKey = partitionKey;
			return this;
		}
		internal IEnumerable<string> KeyFilters { get; set; } = Array.Empty<string>();
		public override Query KeyFilter(string[] filters)
		{
			KeyFilters = filters;
			return this;
		}

		public CosmosDBQuery(CosmosDBDatastoreHelper dataStoreHelper) : base(dataStoreHelper) { }
	}
}
