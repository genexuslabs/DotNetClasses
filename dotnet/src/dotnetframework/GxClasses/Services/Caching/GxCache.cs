using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using log4net;
using System.Threading;
using System.Globalization;
using GeneXus.Management;
using System.Runtime.Serialization;
using GeneXus.Configuration;
using GeneXus.Utils;
using System.Collections.Generic;
using GeneXus.Services;
using System.Linq;
using System.Security;
using System.Text.Json.Serialization;
using System.Text.Json;
#if NETCORE
using GxClasses.Helpers;
using System.IO;
#endif
namespace GeneXus.Cache
{
    internal delegate void AddDataHandler(string key,
    ICacheItemExpiration expiration);

	[Obsolete("Not for public use. Replaced by ICacheService2", false)]
	public interface ICacheService
    {
        bool Get<T>(string cacheid, string key, out T value);
		void Set<T>(string cacheid, string key, T value);
        void Set<T>(string cacheid, string key, T value, int durationMinutes);
        void Clear(string cacheid, string key);
        void ClearCache(string cacheid);
        void ClearKey(string key);
        void ClearAllCaches();
	}
	public interface ICacheService2 : ICacheService
	{
		IDictionary<string, T> GetAll<T>(string cacheid, IEnumerable<string> keys);
		void SetAll<T>(string cacheid, IEnumerable<string> keys, IEnumerable<T> values, int durationMinutes = 0);
	}
	[SecurityCritical]
	public class DBNullConverter : JsonConverter<DBNull>
	{
		[SecurityCritical]
		public override DBNull Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			throw new NotSupportedException();
		}
		[SecurityCritical]
		public override void Write(Utf8JsonWriter writer, DBNull value, JsonSerializerOptions options)
		{
			writer.WriteNullValue();
		}
	}
	[SecurityCritical]
	public class ObjectToInferredTypesConverter : JsonConverter<object>
	{
		[SecurityCritical]
		public override bool CanConvert(Type typeToConvert)
		{
			return typeof(object) == typeToConvert;
		}
		[SecurityCritical]
		public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.True:
					return true;
				case JsonTokenType.False:
					return false;
				case JsonTokenType.Number:
					if (reader.TryGetInt64(out long l))
						return l;
					else return reader.GetDouble();
				case JsonTokenType.String:
					if (reader.TryGetDateTime(out DateTime datetime))
						return datetime;
					else return reader.GetString();
				default:
					using (JsonDocument document = JsonDocument.ParseValue(ref reader))
					{
						return document.RootElement.Clone().ToString();
					}
			}
		}
		[SecurityCritical]
		public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}

	public class CacheFactory
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Cache.CacheFactory));
        public static string CACHE_SD = "SD";
        public static string CACHE_DB = "DB";
        public static string CACHE_FILES = "FL";
		public static string FORCE_HIGHEST_TIME_TO_LIVE = "FORCE_HIGHEST_TIME_TO_LIVE";
		private static volatile ICacheService instance;
        private static object syncRoot = new Object();
		private static bool forceHighestTimetoLive = false;


		public static ICacheService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
							
							GXServices services = GXServices.Instance;
							if (services != null)
							{
								GXService providerService = services.Get(GXServices.CACHE_SERVICE);
								if (providerService != null)
								{
									GXLogging.Debug(log, "Loading CACHE_PROVIDER: ", providerService.ClassName);
									try
									{
#if NETCORE
										Type type = new AssemblyLoader(FileUtil.GetStartupDirectory()).GetType(providerService.ClassName);
#else
										Type type = Type.GetType(providerService.ClassName, true, true);
#endif
										instance = (ICacheService)Activator.CreateInstance(type);
										if (providerService.Properties.ContainsKey(FORCE_HIGHEST_TIME_TO_LIVE))
										{
											int ttl;
											if (Int32.TryParse(providerService.Properties.Get(FORCE_HIGHEST_TIME_TO_LIVE), out ttl) && ttl==1)
											{
												forceHighestTimetoLive = true;
											}
										}
									}
									catch (Exception e)
									{
										GXLogging.Error(log, "Couldn't create CACHE_PROVIDER as ICacheService: ", providerService.ClassName, e);
										throw e;
									}
								}
							}
                            if (instance == null)
                            {
                                GXLogging.Debug(log, "Loading Default CACHE_PROVIDER InMemoryCache");
                                instance = new InProcessCache();
                            }
                        }
                    }
                }

                return instance;
            }
        }
        public static void RestartCache()
        {
            instance.ClearAllCaches();
        }
		public static bool ForceHighestTimetoLive
		{
			get {
				if (Instance != null)
					return forceHighestTimetoLive;
				else
					return false;
			}
		}
    }
    public sealed class InProcessCache : ICacheService2
	{
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Cache.InProcessCache));
        ICacheStorage cacheStorage;
        IScavengingAlgorithm storageScavengingImplementation;

        private HybridDictionary itemsExpiration;
		private WMICache wmicache;

		private static object syncRoot = new Object();
        private ulong hits;
        private ulong misses;

        private const int CONVERT_TO_MILLISECONDS_VALUE = 1000;

        public InProcessCache()
        {
            itemsExpiration = new HybridDictionary(false);
            cacheStorage = new SingletonCacheStorage();
            GXLogging.Debug(log, "Start InProcessCache.Ctr, initialize default scavenging process with '", cacheStorage.ToString(), "'");
            storageScavengingImplementation = new LruScavenging();

            storageScavengingImplementation.Init(this, cacheStorage, null, null);

            // Start a background thread and start monitoring
            // for expirations
            ThreadStart expirationThreadStart = new ThreadStart(
                this.MonitorForExpirations);
            Thread expirationThread = new Thread(expirationThreadStart);
            expirationThread.IsBackground = true;
            expirationThread.Start();
			if (Preferences.Instrumented)
            {
                wmicache = new WMICache(this);
            }
		}

        public ulong HitCount
        {
            get { return hits; }
        }
        public ulong Misses
        {
            get { return misses; }
        }
        public long Size
        {
            get { return cacheStorage.Size; }
            set { cacheStorage.Size = value; }
        }
        public void StopMonitor()
        {
        }
        public void AddSize(string key, long size)
        {
            Size += size;
            CacheItem item = cacheStorage.GetData(key) as CacheItem;
            if (item != null)
                item.SizeInBytes = size;
        }
        public void Set<T>(string cacheid, string keyValue, T tvalue, int duration)
        {
            Set<T>(Key(cacheid, keyValue), tvalue, duration);
        }

        private void Set<T>(string key, T tvalue, int duration)
        {
            GXLogging.Debug(log, "Set<T> key:", () => key + " value " + tvalue + " valuetype:" + tvalue.GetType());
            if (key != null)
            {
                SlidingTime expiration = null;
                if (duration > 0)
                    expiration = new SlidingTime(new TimeSpan(TimeSpan.TicksPerMinute * duration));
				if (Preferences.Instrumented)
				{
					wmicache.Add(new WMICacheItem(key, expiration, tvalue));
                }
				cacheStorage.Add(key, tvalue);
                AddMetadata(key, expiration);
            }
        }


        public void Set<T>(string cacheid, string key, T value)
        {
            Set<T>(Key(cacheid, key), value, -1);
        }


        private void Set<T>(string key, T value)
        {
            Set<T>(key, value, -1);
        }

        private bool Get<T>(string key, out T result)
        {
            result = default(T);
            if (key != null)
            {
                Notify(key);
                object value = cacheStorage.GetData(key);
                if (value != null && typeof(T) == value.GetType())
                {
                    result = (T)value;
                    if (Preferences.Instrumented)
                    {
                        CacheItem item = result as CacheItem;
                        if (item != null)
                            item.Hits++;
                    }
                    hits++;
                    return true;
                }
                else
                {
                    GXLogging.Debug(log, "Get<T>, misses key '", key, "'");
                    misses++;
                    return false;
                }
            }
            else
            {
                GXLogging.Error(log, "GetData, Error: Key is null");
                return false;
            }
        }

        private string Key(string cacheid, string key)
        {
            return FormatKey(cacheid, KeyPrefix(cacheid), key);
        }
		private long KeyPrefix(string cacheid)
		{
			long prefix;
			if (!Get<long>(cacheid, out prefix))
			{
				prefix = DateTime.Now.Ticks;
				Set<long>(cacheid, prefix);
			}
			return prefix;
		}
		private string FormatKey(string cacheid, long prefix, string key)
		{
			return cacheid + prefix + key;
		}
		private IEnumerable<string> Key(string cacheid, IEnumerable<string> keys)
		{
			long prefix = KeyPrefix(cacheid);
			return keys.Select(k => FormatKey(cacheid, prefix, k));
		}
		public bool Get<T>(string cacheid, string keyvalue, out T result)
        {
            string key = Key(cacheid, keyvalue);
            GXLogging.Debug(log, "Get<T> cacheid:", cacheid, " key:", key);
            return Get<T>(key, out result);
        }
		public IDictionary<string, T> GetAll<T>(string cacheid, IEnumerable<string> keys)
		{
			if (keys != null)
			{
				IEnumerable<string> prefixedKeys = Key(cacheid, keys);
				IDictionary<string, T> results = new Dictionary<string, T>();
				foreach (string key in prefixedKeys)
				{
					Get<T>(key, out T result);
					results.Add(key, result);
				}
				return results;
			}
			else
				return null;
		}
		public void SetAll<T>(string cacheid, IEnumerable<string> keys, IEnumerable<T> values, int duration = 0)
		{
			if (keys != null && values!=null)
			{
				var prefixedKeys = Key(cacheid, keys);
				IEnumerator<T> valuesEnumerator = values.GetEnumerator();
				foreach (string key in prefixedKeys)
				{
					if (valuesEnumerator.MoveNext())
					{
						Set<T>(key, valuesEnumerator.Current, duration);
					}
				}
			}
		}
		private void AddMetadata(string keyVal,
            ICacheItemExpiration expiration)
        {            
            lock (itemsExpiration)
            {
                itemsExpiration.Remove(keyVal);
            }

            if (expiration != null)
            {
                lock (itemsExpiration)
                {
                    itemsExpiration[keyVal] = expiration;
                }
            }

            storageScavengingImplementation.Add(keyVal);

            // Notifying the addition of key to Cacheservice 
            // so that the Cacheservice will take the time the
            // data is added as the last time used for calculating
            // the sliding expirations
            Notify(keyVal);

            storageScavengingImplementation.Execute();

        }


        public void Flush()
        {
            lock (syncRoot)
            {
                // Remove all the cache items from storage
                cacheStorage.Flush();

                // Call the CacheService's Asynch Remove method
                // to flush the item's expirations, dependencies
                // and its priority for the cache items 
                FlushMetadata();
            }
        }

        public void FlushMetadata()
        {
            lock (itemsExpiration)
            {
                itemsExpiration.Clear();
            }
            storageScavengingImplementation.Flush();
        }


        public void Notify(string key)
        {

            // Notify the item's expiration classes and
            // the scavenging class
            storageScavengingImplementation.Notify(key);

            if (itemsExpiration.Contains(key))
            {
                ((ICacheItemExpiration)itemsExpiration[key]).Notify();

            }
        }

        internal void RemoveMetadata(string key)
        {

            // Remove all metadata from the CacheService, if the storage
            // implements ICacheMetadata remove item's metadata from there
            // as well
            RemoveItem(key);
        }

        internal void RemoveItem(string key)
        {
            GXLogging.Debug(log, "RemoveItem (Metadata), key: '", key, "'");
            if (itemsExpiration.Contains(key))
            {
                lock (itemsExpiration)
                {
                    itemsExpiration.Remove(key);
                }
            }
            storageScavengingImplementation.Remove(key);

        }


        private void MonitorForExpirations()
        {
            int checkIntervalInSeconds;
            int checkIntervalInMilliseconds;
            int counter;
            string key;
            List<string> expiredItems = new List<string>();

            checkIntervalInSeconds = 10;
            checkIntervalInMilliseconds =
                checkIntervalInSeconds * CONVERT_TO_MILLISECONDS_VALUE;

            GXLogging.Debug(log, "Start MonitorForExpirations loop ");

            while (true)
            {

                // The use of enumerations is not a thread safe operation
                lock (itemsExpiration)
                {
                    // Iterate over the expirations list and
                    // check for expired items
                    foreach (DictionaryEntry dictionary in
                        itemsExpiration)
                    {
                        ICacheItemExpiration exp =
                            (ICacheItemExpiration)dictionary.Value;

                        if (exp.HasExpired())
                        {
                            key = dictionary.Key.ToString();
                            expiredItems.Add(key);
                        }
                    }
                }
                for (counter = 0;
                    counter < expiredItems.Count;
                    counter++)
                {
                    cacheStorage.Remove(expiredItems[counter].ToString());
                    RemoveItem(expiredItems[counter].ToString());
                }
                expiredItems.Clear();
                Thread.Sleep(checkIntervalInMilliseconds);

            }
        }

        public void Clear(string cacheid, string keyValue)
        {
            string key = Key(cacheid, keyValue);
            ClearKey(key);
        }
        public void ClearKey(string key)
        {
            lock (syncRoot)
            {
                cacheStorage.Remove(key);
				if (Preferences.Instrumented)
				{
					wmicache.Remove(key);
                }
				// Call the CacheService's Asynch Remove method
				// to remove the item's expirations, dependencies
				// and its priority
				RemoveMetadata(key);
            }

        }

        public void ClearCache(string cacheid)
        {
            Set<long>(cacheid, DateTime.Now.Ticks);
        }

        public void ClearAllCaches()
        {
            Flush();
        }
    }

    public interface ICacheItemExpiration
    {
        bool HasExpired();
        void Notify();
        void Key(string keyVal);
        TimeSpan ItemSlidingExpiration { get; }
    }

    public interface ICacheMetadata
    {
        void Add(string key, ICacheItemExpiration expiration);
        void Remove(string key);
        Hashtable GetMetadata();
        void Flush();
    }
    public interface IScavengingAlgorithm
    {
        void Init(ICacheService cacheService,
            ICacheStorage cacheStorage,
            ICacheMetadata cacheMetadata,
            XmlNode config);

        /// <summary>
        ///	Notifies that the element with the specified key was recently used.
        /// </summary>
        void Notify(string key);

        /// <summary>
        ///	Executes the algorithm.
        /// </summary>
        void Execute();

        /// <summary>
        ///	Adds a new element to the item algorithm list. This list is used 
        ///	when the algorithm is executed.
        /// </summary>
        void Add(string key);

        /// <summary>
        ///	Removes the element with the specified key from the item 
        ///	algorithm list.
        /// </summary>
        void Remove(string key);

        /// <summary>
        ///	Removes all elements from the item algorithm list.
        /// </summary>
        void Flush();
    }
    [Serializable]
    public class CacheItem
	{
		public CacheItem()
		{
		}

		public CacheItem(GxArrayList data, bool hasnext, int blockSize, long sizeInBytes)
        {
            Data = data;
            HasNext = hasnext;
            BlockSize = blockSize;
            SizeInBytes = sizeInBytes;
        }
        public GxArrayList Data { get; set; }
        public int BlockSize { get; set; }
        public bool HasNext { get; set; }
        public long SizeInBytes { get; set; }
        public long Hits { get; set; }
        public String OriginalKey { get; set; }
    }

    public interface ICacheStorage
    {
        /// <summary>
        ///	Inits the storage provider.
        /// </summary>
        void Init(XmlNode config);

        /// <summary>
        ///	Adds an element with the specified key and value into the storage.
        /// </summary>
        void Add(string key, object keyData);

        /// <summary>
        ///	Removes all elements from the storage.
        /// </summary>
        void Flush();

        /// <summary>
        ///	Gets the element with the specified key.
        /// </summary>
        object GetData(string key);

        /// <summary>
        ///	Removes the element with the specified key.
        /// </summary>
        void Remove(string key);

        /// <summary>
        ///	Updates the element with the specified key.
        /// </summary>
        void Update(string key, object keyData);

        /// <summary>
        ///	Gets the number of elements actually contained in the storage.
        /// </summary>
        long Size { get; set; }
    }

    public class SingletonCacheStorage : ICacheStorage
    {

        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Cache.SingletonCacheStorage));
        private HybridDictionary cacheStorage = new HybridDictionary();
        private long size;

        public SingletonCacheStorage()
        {
        }

        long ICacheStorage.Size
        {
            get { return size; }
            set { size = value; }
        }

        void ICacheStorage.Init(XmlNode configSection)
        {
        }

        void ICacheStorage.Add(string key, object keyData)
        {
            ((ICacheStorage)this).Update(key, keyData);
        }

        void ICacheStorage.Flush()
        {
            
            lock (cacheStorage)
            {
                cacheStorage.Clear();
            }
        }


        object ICacheStorage.GetData(string key)
        {

            return cacheStorage[key];
        }

        void ICacheStorage.Remove(string key)
        {
			
			long beforeSize = size;			
            lock (cacheStorage)
            {
                CacheItem cacheItem = cacheStorage[key] as CacheItem;
                if (cacheItem != null)
                    size = size - cacheItem.SizeInBytes - (10 + (2 * key.Length));
                cacheStorage.Remove(key);
            }
			GXLogging.Debug(log, "RemoveItem (Data), key: '", () => key + "', size before: " + beforeSize.ToString() + " , size after: " + size.ToString());			
        }

        void ICacheStorage.Update(string key, object keyData)
        {
            
            lock (cacheStorage)
            {
                if (!cacheStorage.Contains(key))
                {
                    CacheItem cacheItem = keyData as CacheItem;
                    if (cacheItem != null)
                        size = size + cacheItem.SizeInBytes + 10 + (2 * key.Length);
                }
                cacheStorage[key] = keyData;
            }
        }
    }

    public class LruScavenging : IScavengingAlgorithm
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Cache.LruScavenging));
        private HybridDictionary itemsLastUsed;
        private ICacheService cachingService;
        private ICacheStorage cacheStorage;
        private ICacheMetadata cacheMetadata;
        private int maxCacheStorageSize;

        public LruScavenging()
        {
        }

        void IScavengingAlgorithm.Init(ICacheService cachingService,
            ICacheStorage cacheStorage,
            ICacheMetadata cacheMetadata,
            XmlNode configSection)
        {

            GXLogging.Debug(log, "Start LruScavenging.Init, initialize cacheStorage to '", () => cacheStorage + "'");


            this.cachingService = cachingService;
            this.cacheStorage = cacheStorage;
            this.cacheMetadata = cacheMetadata;

            itemsLastUsed = new HybridDictionary(false);
            string size;
            if (Config.GetValueOf("CACHE_STORAGE_SIZE", out size) && Convert.ToInt32(size) > 0)
                maxCacheStorageSize = Convert.ToInt32(size) * 1024;
            else
                maxCacheStorageSize = -1;

        }

        void IScavengingAlgorithm.Execute()
        {
            
            long storageSize = cacheStorage.Size;
            
            if (maxCacheStorageSize > 0 && storageSize >= maxCacheStorageSize)
            {
				GXLogging.Debug(log, "Start LruScavenging.Execute, maxCacheStorageSize '", () => maxCacheStorageSize + "',storageSize='" + storageSize + "'");
				
				while (storageSize >= maxCacheStorageSize)
                {
                    string key = GetLruItem();
                    
                    cacheStorage.Remove(key);
                    if (cacheMetadata != null)
                    {
                        lock (cacheMetadata)
                        {
                            cacheMetadata.Remove(key);
                        }
                    }

                    cachingService.ClearKey(key);

                    lock (itemsLastUsed)
                    {
                        itemsLastUsed.Remove(key);
                    }
                    storageSize = cacheStorage.Size;
                }
            }
        }

        void IScavengingAlgorithm.Notify(string key)
        {

            lock (itemsLastUsed)
            {
                if (itemsLastUsed.Contains(key))
                {
                    itemsLastUsed[key] = DateTime.Now;
                }
            }
        }

        void IScavengingAlgorithm.Add(string key)
        {

            lock (itemsLastUsed)
            {
                itemsLastUsed[key] = DateTime.Now;
            }
        }

        void IScavengingAlgorithm.Remove(string key)
        {

            lock (itemsLastUsed)
            {
                itemsLastUsed.Remove(key);
            }
        }

        void IScavengingAlgorithm.Flush()
        {
            lock (itemsLastUsed)
            {
                itemsLastUsed.Clear();
            }
        }

        private string GetLruItem()
        {
            string lruItemKey = "";
            DateTime tmpDateTime = DateTime.Now;

            lock (itemsLastUsed)
            {
                foreach (DictionaryEntry dictEntry in itemsLastUsed)
                {
                    if (DateTime.Compare(tmpDateTime,
                        (DateTime)dictEntry.Value) > 0)
                    {
                        tmpDateTime = (DateTime)dictEntry.Value;
                        lruItemKey = dictEntry.Key.ToString();
                    }
                }
            }
            return lruItemKey;
        }
    }

    [Serializable]
    public class SlidingTime : ICacheItemExpiration
    {
        private DateTime timeLastUsed;
        private TimeSpan itemSlidingExpiration;
        private long expirationTicks;

        public SlidingTime(TimeSpan slidingExpiration)
        {
            itemSlidingExpiration = slidingExpiration;
        }
        public TimeSpan ItemSlidingExpiration
        {
            get { return itemSlidingExpiration; }
        }

        void ICacheItemExpiration.Key(string keyVal)
        {
        }

        bool ICacheItemExpiration.HasExpired()
        {
            bool isItemexpired = CheckSlidingExpiration(DateTime.Now,
                timeLastUsed,
                itemSlidingExpiration);
            return isItemexpired;
        }

        void ICacheItemExpiration.Notify()
        {
            timeLastUsed = DateTime.Now;
        }

        private bool CheckSlidingExpiration(DateTime nowDateTime,
            DateTime lastUsed,
            TimeSpan slidingExpiration)
        {
            
            DateTime tmpNowDateTime;
            DateTime tmpLastUsed;
            bool isExpired = false;

            // Convert to UTC in order to compensate for time zones
            tmpNowDateTime = nowDateTime.ToUniversalTime();

            // Calculate the ticks only once
            if (expirationTicks == 0)
            {
                // Convert to UTC in order to compensate for time zones
                tmpLastUsed = lastUsed.ToUniversalTime();

                expirationTicks = tmpLastUsed.Ticks
                    + slidingExpiration.Ticks;
            }


            if (tmpNowDateTime.Ticks > expirationTicks)
            {
                isExpired = true;
            }
            else
            {
                isExpired = false;
            }
            return isExpired;
        }

        protected SlidingTime(SerializationInfo info,
            StreamingContext context)
        {
            timeLastUsed = Convert.ToDateTime(
                info.GetValue("lastUsed", typeof(DateTime)),
                DateTimeFormatInfo.CurrentInfo);
            itemSlidingExpiration = (TimeSpan)info.GetValue(
                "slidingExpiration",
                typeof(TimeSpan));
        }
    }
}
