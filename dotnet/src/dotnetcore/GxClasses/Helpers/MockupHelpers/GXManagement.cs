using System;
using System.Collections.Generic;
using System.Text;
using GeneXus.Cache;
using GeneXus.Data;
using GeneXus.Data.ADO;
using GeneXus.Data.NTier;

namespace GeneXus.Management
{
	public class WMIDataSource
	{
		private GxDataStore gxDataStore;

		public WMIDataSource(GxDataStore gxDataStore)
		{
			this.gxDataStore = gxDataStore;
		}

		internal void CleanUp()
		{
			throw new NotImplementedException();
		}
	}
	public class WMICache
	{
		private InProcessCache inProcessCache;

		public WMICache(InProcessCache inProcessCache)
		{
			this.inProcessCache = inProcessCache;
		}

		internal void Add(WMICacheItem wMICacheItem)
		{
			throw new NotImplementedException();
		}

		internal void Remove(string key)
		{
			throw new NotImplementedException();
		}
	}
	public class WMICacheItem {
		public WMICacheItem(string SQLStatement, ICacheItemExpiration expiration, object item) { }
	}
	public class WMIConnection
	{
		private GxConnection gxConnection;

		public WMIConnection(GxConnection gxConnection)
		{
			this.gxConnection = gxConnection;
		}

		internal void CleanUp()
		{
			throw new NotImplementedException();
		}
	}

}
