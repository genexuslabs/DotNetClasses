using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneXus.Data.NTier
{
	public class DynamoScan : DynamoQuery
	{
		public DynamoScan(DynamoDBDataStoreHelper dataStoreHelper):base(dataStoreHelper)
		{
			
		}
	}
}
