using GeneXus.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace GeneXus.SD.Store.Model
{
	[DataContract]
	public class PurchaseResult
	{
		[DataMember]
		public string PurchaseId { get; set; }

		[DataMember]
		public string ProductIdentifier { get; set; }

		[DataMember(Name ="PurchasePlatform")]
		public int Platform { get; set; }

		[DataMember]
		public string TransactionData { get; set; }

	}
}
