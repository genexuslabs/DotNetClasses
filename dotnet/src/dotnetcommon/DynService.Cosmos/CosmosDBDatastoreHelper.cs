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
	public static class CosmosFluentExtensions
	{
		public static Query SetPartitionKey(this Query cosmosQuery, string partitionKey)
		{
			return (cosmosQuery as CosmosDBQuery)?.SetKey(partitionKey);
		}
		public static Query KeyFilter(this Query cosmosQuery, string[] filters)
		{
			return (cosmosQuery as CosmosDBQuery)?.KeyFilter(filters);
		}
		public static Query SetKey(this Query cosmosQuery, string partitionKey)
		{
			return (cosmosQuery as CosmosDBQuery)?.SetKey(partitionKey);
		}

	}
	public class CosmosDBQuery : Query
	{
		public string Index { get; set; }
		
		public CosmosDBQuery OrderBy(string index)
		{
			////TO DO///
			return this;
		}

		public string PartitionKey { get; private set; }
		public CosmosDBQuery SetKey(string partitionKey)
		{
			PartitionKey = partitionKey;
			return this;
		}
		internal IEnumerable<string> KeyFilters { get; set; } = Array.Empty<string>();
		public CosmosDBQuery KeyFilter(string[] filters)
		{
			KeyFilters = filters;
			return this;
		}

		public CosmosDBQuery(CosmosDBDatastoreHelper dataStoreHelper) : base(dataStoreHelper) { }
	}
}
