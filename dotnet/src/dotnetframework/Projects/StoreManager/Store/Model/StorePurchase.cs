using Newtonsoft.Json;
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
	public class StorePurchase
	{
		[DataMember(IsRequired = true, Name = "purchaseId")]
		public string PurchaseId { get; set; }

		[DataMember(IsRequired = true, Name = "productId")]
		public string ProductIdentifier { get; set; }

		[DataMember(IsRequired = true, Name = "purchaseDate")]
		public DateTime PurchaseDate { get; set; }

		[DataMember(IsRequired = true, Name = "productType")]
		public int ProductType { get; set; }

		[DataMember(IsRequired = true, Name = "status")]
		public PurchaseStatus PurchaseStatus { get; set; }

		[DataMember(Name = "subscription")]
		public StorePurchaseSubscription Subscription { get; set; }

		[DataMember(Name = "advanced")]
		public StorePurchaseCustom Custom { get; set; }
				
		public StorePurchase()
		{
			PurchaseId = string.Empty;

		}
		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented,
				new JsonSerializerSettings
				{
					DateFormatHandling = DateFormatHandling.IsoDateFormat,
					Formatting = Formatting.Indented
				});
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			return this.GetType() == obj.GetType() && this.PurchaseId == ((StorePurchase)obj).PurchaseId;
		}
	}

	[DataContract]
	public class StorePurchaseSubscription
	{
		[DataMember(IsRequired = true, Name = "expiration")]
		public DateTime Expiration { get; set; }

		[DataMember(IsRequired = true, Name = "purchaseDate")]
		public DateTime FirstPurchased { get; set; }

	}

	[DataContract]
	public class StorePurchaseCustom
	{
		[DataMember(Name = "consumed")]
		public bool Consumed { get; set; }

		[DataMember(Name = "qty")]
		public int Quantity { get; set; }

		[DataMember(Name = "willRenew")]
		public bool WillAutoRenew { get; set; }

		[DataMember(Name = "cancelReason")]
		public int? CancelReason { get; set; }

		[DataMember(Name = "isTrial")]
		public bool IsTrialPeriod { get; set; }

		[DataMember(Name = "originalPurchase")]
		public PurchaseResult OriginalPurchase { get; set; }

		[DataMember(Name = "acknowledgementState")]
		public int AcknowledgementState { get; set; }

		public StorePurchaseCustom()
		{
			Quantity = 1;
		}
	}
}
