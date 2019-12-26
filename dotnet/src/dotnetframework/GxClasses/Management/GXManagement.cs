using System;
using System.Collections;
using System.ComponentModel;
using System.Management.Instrumentation;
using System.Reflection;
using GeneXus.Cache;
using GeneXus.Configuration;
using GeneXus.Data.ADO;
using GeneXus.Utils;
using GeneXus.XML;
using log4net;
using System.Security;
using System.Security.Permissions;

[assembly:Instrumented(@"root\GeneXus")]
namespace GeneXus.Management
{
	[RunInstaller(true)]
	[SecurityCritical]
	public class WMIGxInstaller : DefaultManagementProjectInstaller{};

	public interface IWMIApplicationServer
	{
		String StartTime
		{
			get;
		}

		int RemoteProcRequestCount
		{
			get;
		}

		int TrnRuleRequestCount
		{
			get;
		}

		int DataStoreRequestCount
		{
			get;
		}

		int UserCount
		{
			get;
		}

		void ShutDown();
	}
	public interface IWMICache
	{
		long StorageSize
		{
			get;
		}
		long CurrentSize
		{
			get;
		}
		bool Enabled
		{
			get;set;
		}
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        int[] TimeToLive
		{
			get;
		}
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        int[] HitsToLive
		{
			get;set;
		}
		void Restart();
	}
	public interface IWMICacheItem
	{
		string SQLStatement
		{
			get;
		}

		long SizeInBytes
		{
			get;
		}

		long HitCount
		{
			get;
		}

		int ExpiryHitsCount
		{
			get;
		}

		double ExpiryTime
		{
			get;
		}

		DateTime TimeCreated
		{
			get;
		}
	}

	public interface IWMIConnection
	{
		int Id
		{
			get;
		}

		string PhysicalId
		{
			get;
		}

		DateTime CreateTime
		{
			get;
		}

		DateTime LastAssignedTime
		{
			get;
		}

		int LastUserId
		{
			get;
		}

		bool Error
		{
			get;
		}

		bool Available
		{
			get;
		}

		int OpenCursorCount
		{
			get;
		}

		bool UncommitedChanges
		{
			get;
		}

		int RequestCount
		{
			get;
		}

		DateTime LastSQLStatementTime
		{
			get;
		}

		string LastSQLStatement
		{
			get;
		}

		string LastObject
		{
			get;
		}

		bool LastSQLStatementEnded
		{
			get;
		}
	
		short Disconnect();

		void DumpConnectionInformation(GXXMLWriter writer);
	}

	public interface IWMIConnectionPool
	{
		int Size
		{
			get;
		}
	
		bool UnlimitedSize
		{
			get;
		}

		int ConnectionCount
		{
			get;
		}

		int FreeConnectionCount
		{
			get;
		}

		int CreatedConnectionCount
		{
			get;
		}

		int RecycledConnectionCount
		{
			get;
		}

		int DroppedConnectionCount
		{
			get;
		}

		int RequestCount
		{
			get;
		}

		float AverageRequestPerSec
		{
			get;
		}

		DateTime LastRequestTime
		{
			get;
		}

		int WaitedUserCount
		{
			get;
		}

		int WaitingUserCount
		{
			get;
		}

		long MaxUserWaitTime
		{
			get;
		}

		float AverageUserWaitTime
		{
			get;
		}
			
		void DumpPoolInformation();

		void Recycle();
	}

	public interface IWMIDataSource
	{
		string Name
		{
			get;
		}

		string UserName
		{
			get;
		}

		string ConnectionString
		{
			get;
		}

		int MaxCursors
		{
			get;
		}

		bool PoolEnabled
		{
			get;
		}

		bool ConnectAtStartup
		{
			get;
		}
  
	}

	public interface IWMIServerUserInformation
	{
		int Id
		{
			get;
		}

		string IP
		{
			get;
		}

		DateTime ConnectedTime
		{
			get;
		}

		long IdleSeconds
		{
			get;
		}

		string LastSQLStatement
		{
			get;
		}

		string LastObject
		{
			get;
		}

		string LastSQLStatementTime
		{
			get;
		}

		string LastConnectionId
		{
			get;set;
		}
  
		void Disconnect();
	}

	[InstrumentationClass(InstrumentationType.Instance)]
	[ManagedName("GeneXusApplicationServer")]
	[SecuritySafeCritical]
	public class WMIApplicationServer : IWMIApplicationServer
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(WMIApplicationServer));
		private static WMIApplicationServer instance;
		private static object syncObj = new object();
		public static WMIApplicationServer Instance()
		{
			lock(syncObj)
			{
				if (instance==null)
				{
					try
					{
						instance = new WMIApplicationServer();
						Instrumentation.Publish(instance);
					}
					catch(Exception e)
					{
						GXLogging.Error(log, "WMI Error", e);
					}

				}
			}
			return instance;
		}

		public string StartTime
		{
			get
			{
				return "";
			}
		}

		public int RemoteProcRequestCount
		{
			get
			{
				return 0;
			}
		}

		public int TrnRuleRequestCount
		{
			get
			{
				try
				{
					Assembly a= Assembly.Load("gxclassrDotNet");
					Type t = a.GetType("com.genexus.distributed.GXServerTransaction");
					int trnrulerequestcount = (int) t.GetMethod("getTransacctionRequestNumber").Invoke(t,null);
					return trnrulerequestcount;
				}
				catch(Exception e)
				{
					GXLogging.Error(log, "WMI Error TrnRuleRequestCount", e);
					return 0;
				}
			}
		}

		public int DataStoreRequestCount
		{
			get
			{
				return 0;
			}
		}

		public int UserCount
		{
			get
			{
				return GxUserInfo.UserInfo.Count;
			}
		}

		public void ShutDown()
		{
		}

		public void CleanUp()
		{
			Instrumentation.Revoke(this);
		}
	}
	[InstrumentationClass(InstrumentationType.Instance)]
	[ManagedName("Cache")]
	[SecuritySafeCritical]
	public class WMICache : IWMICache
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(WMICache));
		InProcessCache cache;
		Hashtable wmicacheItems;
		long maxCacheStorageSize;
		#region IWMICache Members

		public WMICache(InProcessCache cache)
		{
			try
			{
				this.cache = cache;
				string size;
				if(Config.GetValueOf("CACHE_STORAGE_SIZE",out size) && Convert.ToInt32(size)>0)
					maxCacheStorageSize=Convert.ToInt64(size)*1024;
				else
					maxCacheStorageSize=-1;
				wmicacheItems = new Hashtable();
				Instrumentation.Publish(this);
			}
			catch(Exception e)
			{
				GXLogging.Error(log, "WMI Error", e);
			}

		}
		public long StorageSize
		{
			get
			{
				return maxCacheStorageSize;
			}
		}

		public long CurrentSize
		{
			get
			{
				return cache.Size;
			}
		}

		public bool Enabled
		{
			get
			{
				string strCache="";
				return (Config.GetValueOf("CACHING",out strCache) && strCache.Equals("1"));
			}
			set
			{
				
			}
		}

        public int[] TimeToLive
		{
			get
			{
				Hashtable ttl = Preferences.CachingTTLs();
				int[] ttlarr =  new int[ttl.Count];
				int i=0;
				foreach (int t in  ttl.Values)
				{
					ttlarr[i] = t;
					i++;
				}
				return ttlarr;
			}
		}
        public int[] HitsToLive
		{
			get
			{
				
				return null;
			}
			set
			{
				
			}
		}

		public void Restart()
		{
			
		}

		#endregion

		public void Add( WMICacheItem item)
		{
			if (!wmicacheItems.Contains(item.SQLStatement))
				wmicacheItems.Add(item.SQLStatement, item);
		}
		public void Remove(string key)
		{
			WMICacheItem cacheitem = (WMICacheItem)wmicacheItems[key];
			if (cacheitem!=null)
			{
				cacheitem.CleanUp();
				wmicacheItems.Remove(key);
			}
		}
	}
	[InstrumentationClass(InstrumentationType.Instance)]
	[ManagedName("CacheItem")]
	[SecuritySafeCritical]
	public class WMICacheItem :IWMICacheItem
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(WMICacheItem));

		string stmt;
		ICacheItemExpiration itemExpiration;
		object cacheItem;
		DateTime timeCreated;
		long keySize;
		public WMICacheItem(string SQLStatement, ICacheItemExpiration expiration, object item)
		{
			try
			{
				this.stmt = SQLStatement;
				this.cacheItem = item;
				timeCreated = DateTime.Now;
				itemExpiration = expiration;
				keySize = 10 + (2 * SQLStatement.Length);
				Instrumentation.Publish(this);
			}
			catch(Exception e)
			{
				GXLogging.Error(log, "WMI Error", e);
			}

		}

		public string SQLStatement
		{
			get
			{
				return stmt;
			}
		}

		public long SizeInBytes
		{
			get
			{
				CacheItem item = cacheItem as CacheItem;
				if (item != null)
					return item.SizeInBytes + keySize; 
				else
					return 0;
			}
		}

		public long HitCount
		{
			get
			{
				CacheItem item = cacheItem as CacheItem;
				if (item != null)
					return item.Hits;
				else
					return 0;
			}
		}

		public int ExpiryHitsCount
		{
			get
			{
				return -1;
			}
		}

		public double ExpiryTime
		{
			get
			{
				return itemExpiration.ItemSlidingExpiration.TotalSeconds;
			}
		}

		public DateTime TimeCreated
		{
			get
			{
				return timeCreated;
			}
		}

		public void CleanUp()
		{
			Instrumentation.Revoke(this);
		}
	}

	[InstrumentationClass(InstrumentationType.Instance)]
	[ManagedName("Connection")]
	[SecuritySafeCritical]
	public class WMIConnection : IWMIConnection
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(WMIConnection));
		GxConnection connection;

		public WMIConnection(GxConnection connection)
		{
			try
			{
				this.connection = connection;
				Instrumentation.Publish(this);
			}
			catch(Exception e)
			{
				GXLogging.Error(log, "WMI Error", e);
			}

		}

		public int Id
		{
			get
			{
				return connection.Handle;
			}
		}

		public string PhysicalId
		{
			get
			{
				return connection.PhysicalId;
			}
		}

		public DateTime CreateTime
		{
			get
			{
				return connection.CreateTime;
			}
		}

		public DateTime LastAssignedTime
		{
			get
			{
				return connection.LastAssignedTime;
			}
		}

		public int LastUserId
		{
			get
			{
				return connection.LastUserAssigned;
			}
		}

		public bool Error
		{
			get
			{
				return connection.Error;
			}
		}

		public bool Available
		{
			get
			{
				return connection.AvailableWMI;
			}
		}

		public int OpenCursorCount
		{
			get
			{
				return connection.OpenCursorCount;
			}
		}

		public bool UncommitedChanges
		{
			get
			{
				return connection.UncommitedChanges;
			}
		}

		public int RequestCount
		{
			get
			{
				return connection.RequestCount;
			}
		}

		public DateTime LastSQLStatementTime
		{
			get
			{
				return connection.LastSQLStatementTime;
			}
		}

		public string LastSQLStatement
		{
			get
			{
				return connection.LastSQLStatement;
			}
		}

		public string LastObject
		{
			get
			{
				return connection.LastObject;
			}
		}

		public bool LastSQLStatementEnded
		{
			get
			{
				return connection.LastSQLStatementEnded;
			}
		}

		public short Disconnect()
		{
			return connection.Disconnect();
		}

		public void DumpConnectionInformation(GXXMLWriter writer)
		{
			connection.DumpConnectionInformation(writer);
		}

		public void CleanUp()
		{
			GXLogging.Debug(log, "Revoke");
			Instrumentation.Revoke(this);
		}
	}

	[InstrumentationClass(InstrumentationType.Instance)]
	[ManagedName("DataStore")]
	[SecuritySafeCritical]
	public class WMIDataSource : IWMIDataSource
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(WMIDataSource));
		GxDataStore dataSource;

		public WMIDataSource(GxDataStore dataSource)
		{
			try
			{
				this.dataSource = dataSource;
				Instrumentation.Publish(this);
			}
			catch(Exception e)
			{
				GXLogging.Error(log, "WMI Error", e);
			}

		}

		public string Name
		{
			get
			{
				return dataSource.Name;
			}
		}

		public string UserName
		{
			get
			{
				return dataSource.UserName;
			}
		}

		public string ConnectionString
		{
			get
			{
				return dataSource.ConnectionString;
			}
		}

		public int MaxCursors
		{
			get
			{
				return dataSource.MaxCursors;
			}
		}

		public bool PoolEnabled
		{
			get
			{
				return dataSource.PoolEnabled;
			}
		}

		public bool ConnectAtStartup
		{
			get
			{
				return dataSource.ConnectAtStartup;
			}
		}
		public void CleanUp()
		{
			Instrumentation.Revoke(this);
		}
	}

	[InstrumentationClass(InstrumentationType.Instance)]
	[ManagedName("User")]
	[SecuritySafeCritical]
	public class WMIServerUserInformation :IWMIServerUserInformation
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(WMIServerUserInformation));

		ServerUserInformation userInfo;
		string lastConnectionId;
		int handle;
		public WMIServerUserInformation(int handle, ServerUserInformation userInfo)
		{
			try
			{
				this.userInfo = userInfo;
				this.handle = handle;
				Instrumentation.Publish(this);
			}
			catch(Exception e)
			{
				GXLogging.Error(log, "WMI Error", e);
			}
		}

		public int Id
		{
			get
			{
				return handle;
			}
		}

		public string IP
		{
			get
			{
				return GxUserInfo.getProperty(handle, GxDefaultProps.WORKSTATION);
			}
		}

		public DateTime ConnectedTime
		{
			get
			{
				return DateTime.Parse(GxUserInfo.getProperty(handle, GxDefaultProps.START_TIME));
			}
		}

		public long IdleSeconds
		{
			get
			{
					return -1;
			}
		}

		public string LastSQLStatement
		{
			get
			{
				return userInfo.LastSQLStatement;
			}
		}

		public string LastObject
		{
			get
			{
				return GxUserInfo.getProperty(handle, GxDefaultProps.PGM_NAME);
			}
		}

		public string LastSQLStatementTime
		{
			get
			{
				return userInfo.LastSQLStatementTime;
			}
		}

		public string LastConnectionId
		{
			get
			{
				return lastConnectionId;
			}
			set
			{
				lastConnectionId=value;
			}
		}

		public void Disconnect()
		{
			
		}

		public void CleanUp()
		{
			Instrumentation.Revoke(this);
		}
	}
}
