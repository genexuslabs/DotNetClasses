using Amazon.DynamoDBv2.Model;
using GeneXus.Data.NTier;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneXus.Data.Dynamo
{
	public class DynamoDBMap : Map
	{
		internal bool NeedsAttributeMap { get; private set; }
		public DynamoDBMap(string name): base(RemoveSharp(name))
		{
			NeedsAttributeMap = name.StartsWith("#", StringComparison.InvariantCulture);
		}
		private static string RemoveSharp(string name) => name.StartsWith("#", StringComparison.InvariantCulture) ? name.Substring(1) : name;

		public override object GetValue(IOServiceContext context, RecordEntryRow currentEntry)
		{
			Dictionary<string, AttributeValue> values = ((DynamoDBRecordEntry)currentEntry).CurrentRow;

			AttributeValue val = null;
			values.TryGetValue(GetName(context), out val);				
			return val;
		}

		public override void SetValue(RecordEntryRow currentEntry, object value)
		{
			throw new NotImplementedException();
		}		

	}

	public class DynamoDBRecordEntry: RecordEntryRow
	{
		public Dictionary<string, AttributeValue> CurrentRow { get; }


		public DynamoDBRecordEntry(Dictionary<string, AttributeValue> cRow)
		{
			CurrentRow = cRow;
		}
	}
}
