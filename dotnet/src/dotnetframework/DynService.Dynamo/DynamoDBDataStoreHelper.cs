using GeneXus.Data.Dynamo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneXus.Data.NTier
{
	public class DynamoDBDataStoreHelper: DataStoreHelperBase
	{

		public DynamoDBMap Map(string name)
		{
			return new DynamoDBMap(name);
		}
	}
	

}
