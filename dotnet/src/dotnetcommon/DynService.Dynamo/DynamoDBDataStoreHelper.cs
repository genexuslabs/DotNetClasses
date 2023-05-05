using GeneXus.Data.Dynamo;
using GeneXus.Utils;
using System;
using System.Collections.Generic;

namespace GeneXus.Data.NTier
{
	public class DynamoDBDataStoreHelper: DynServiceDataStoreHelperBase
	{

		public DynamoQuery NewQuery() => new DynamoQuery(this);
		public DynamoQuery NewScan() => new DynamoScan(this);

		public DynamoDBMap Map(string name)
		{
			return new DynamoDBMap(name);
		}

		public object empty(GXType gxtype)
		{
			switch(gxtype)
			{
				case GXType.Number:
				case GXType.Int16:
				case GXType.Int32:
				case GXType.Int64: return 0;
				case GXType.Date: 
				case GXType.DateTime: 
				case GXType.DateTime2:	return DateTimeUtil.NullDate();
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

	public class DynamoQuery : Query
	{
		public string Index { get; set; }
		public bool ScanIndexForward = true;
		private const string RANGE_KEY_INDEX = "RangeKey";
		private static readonly char[] indexTrimChars = new char[] { '(', ')' };

		public override Query OrderBy(string index)
		{
			if(index.StartsWith("(", StringComparison.InvariantCulture))
			{
				ScanIndexForward = false;
				index = index.Trim(indexTrimChars);
			}
			if (index != RANGE_KEY_INDEX)
				Index = index;
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
		public DynamoQuery(DynamoDBDataStoreHelper dataStoreHelper) : base(dataStoreHelper) { }
	}
}
