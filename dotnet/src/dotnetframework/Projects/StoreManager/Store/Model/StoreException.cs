using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneXus.SD.Store.Model
{
	public class StoreException: Exception
	{
		public StoreException(string errorDsc) : base(errorDsc) { }

		public StoreException(string errorDsc, Exception innerExcp) : base(errorDsc, innerExcp) { }		

	}

	public class StoreConfigurationException: StoreException
	{
		public StoreConfigurationException(string errorDsc) : base(errorDsc) { }
		public StoreConfigurationException(string errorDsc, Exception e) : base(errorDsc, e) { }
	}

	public class StoreInvalidPurchaseException : StoreException
	{
		public StoreInvalidPurchaseException(string errorDsc) : base(errorDsc) { }
		public StoreInvalidPurchaseException(string errorDsc, Exception e) : base(errorDsc, e) { }
	}

	public class StoreServerException : StoreException
	{
		public StoreServerException(string errorDsc) : base(errorDsc) { }
		public StoreServerException(string errorDsc, Exception e) : base(errorDsc, e) { }
	}

	public class StoreResponsePurchaseException : StoreException
	{
		public StoreResponsePurchaseException(string errorDsc) : base(errorDsc) { }
		public StoreResponsePurchaseException(string errorDsc, Exception e) : base(errorDsc, e) { }
	}

	public class StoreResponseEnvironmentException : StoreException
	{
		public StoreResponseEnvironmentException(string errorDsc) : base(errorDsc) { }
		public StoreResponseEnvironmentException(string errorDsc, Exception e) : base(errorDsc, e) { }
	}

	public class StorePurchaseNotFoundException : StoreException
	{
		public StorePurchaseNotFoundException(string errorDsc) : base(errorDsc) { }
		public StorePurchaseNotFoundException(string errorDsc, Exception e) : base(errorDsc, e) { }
	}
}
