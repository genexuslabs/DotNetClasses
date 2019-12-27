
using GeneXus.SD.Store.Model;
using GeneXus.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneXus.SD.Store.Platforms
{
	public interface IStoreManager
	{
		StorePurchase GetPurchase(string productId, PurchaseResult purchaseResult);

		List<StorePurchase> GetPurchases(PurchasesInformation pInfo);
	}
}
