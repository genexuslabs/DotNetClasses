using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneXus.SD.Store
{
	public enum Constants
	{
		PRODUCT_TYPE_SUSCRIPTION = 2,
		PRODUCT_TYPE_OTHER = 1		
	}

	public enum PurchaseStatus
	{
		Invalid = 0,
		Valid = 1,
		Expired = 2,
		Cancelled = 3
	}

	public enum ProductType
	{
		Product = 1,
		Subscription = 2
	}
	public enum ErrorCodes
	{
		MISSING_STORE_CONFIGURATION = 5
	}
}
