using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using System;
using System.Collections.Generic;
using System.Text;
using static Lucene.Net.Search.SimpleFacetedSearch;

namespace GeneXus.Search
{
    public static class CompatibilityExtensions
    {
		public static void SetAllowLeadingWildcard(this QueryParser queryParser, bool value)
		{
			queryParser.AllowLeadingWildcard = value;
		}
		public static void SetDefaultOperator(this QueryParser queryParser, QueryParser.Operator value)
		{
			queryParser.DefaultOperator = value;
		}
		
	}
	public static class FieldExtensions
	{

		public static string StringValue(this Field field)
		{
			return field.StringValue;
		}
		public static string Name(this Field field)
		{
			return field.Name;
		}
		public static int Length(this Hits hits)
		{
			return (int)hits.TotalHitCount;
		}
	}

}
