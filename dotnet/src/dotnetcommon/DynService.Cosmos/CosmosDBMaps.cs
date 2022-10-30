using GeneXus.Data.NTier;
using System;
using System.Collections.Generic;

namespace GeneXus.Data.Cosmos
{
	public class CosmosDBMap : Map
	{
		internal bool NeedsAttributeMap { get; }

		//TODO
		public CosmosDBMap(string name): base(name)
		{
			
		}
		public override object GetValue(IOServiceContext context, RecordEntryRow currentEntry)
		{
			Dictionary<string, object> values = ((CosmosDBRecordEntry)currentEntry).CurrentRow;

			values.TryGetValue(GetName(context), out object val);
			return val;
		}

		public override void SetValue(RecordEntryRow currentEntry, object value)
		{
			throw new NotImplementedException();
		}

	}
	public class CosmosDBRecordEntry : RecordEntryRow
	{
		public Dictionary<string, object> CurrentRow { get; }

		public CosmosDBRecordEntry(Dictionary<string, object> cRow)
		{
			CurrentRow = cRow;
		}
	}
}
