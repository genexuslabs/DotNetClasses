using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GeneXus.SD.Store.Model
{
	[DataContract]
	public class PurchasesInformation
	{
		[DataMember(Name = "Receipt")]
		public string AppleReceipt { get; set; }

		[DataMember]
		public int PurchasePlatform { get; set; }

		[DataMember]
		public List<PurchaseResult> Purchases { get; set; }
	}
}
