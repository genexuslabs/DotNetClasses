using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Data.Cosmos;
using GeneXus.Utils;

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
		public object empty(GXType gxtype)
		{
			switch (gxtype)
			{
				case GXType.Number:
				case GXType.Int16:
				case GXType.Int32:
				case GXType.Int64: return 0;
				case GXType.Date:
				case GXType.DateTime:
				case GXType.DateTime2: return DateTimeUtil.NullDate();
				case GXType.Byte:
				case GXType.NChar:
				case GXType.NClob:
				case GXType.NVarChar:
				case GXType.Char:
				case GXType.LongVarChar:
				case GXType.Clob:
				case GXType.VarChar:
				case GXType.Raw:
				case GXType.Blob: return string.Empty;
				case GXType.Boolean: return false;
				case GXType.Decimal: return 0f;
				case GXType.NText:
				case GXType.Text:
				case GXType.Image:
				case GXType.UniqueIdentifier:
				case GXType.Xml: return string.Empty;
				case GXType.Geography:
				case GXType.Geopoint:
				case GXType.Geoline:
				case GXType.Geopolygon: return new Geospatial();
				case GXType.DateAsChar: return string.Empty;
				case GXType.Undefined:
				default: return null;

			}
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
