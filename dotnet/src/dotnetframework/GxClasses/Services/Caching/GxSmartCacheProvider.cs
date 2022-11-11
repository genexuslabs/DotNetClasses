using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Cache;
using System.Collections.Concurrent;
using System.Linq;

namespace GeneXus.Data
{
    public class GxSmartCacheProvider
    {
		const string FORCED_INVALIDATE = "ForcedInvalidate";
		const string DEFAULT_SMART_CACHING = "1";

		// Modified Tables Record
		static ICacheService updatedTables = CacheFactory.Instance;
        
        static ConcurrentDictionary<string, List<string>> queryTables;

        List<string> tablesUpdatedInUTL;

		static bool enabled= (Config.GetValueOf("SMART_CACHING", DEFAULT_SMART_CACHING) =="0") ? false: true;

        private static object instanceSync = new Object();

	    public GxSmartCacheProvider()
        {
			if (enabled)
			{
				tablesUpdatedInUTL = new List<string>();
			}
        }
        /// <summary>
        /// Mark a table as modified. It is used from the generated pgms after an update on the table
        /// </summary>
        /// <param name="table"></param>
        public void SetUpdated(string table)
        {
            if (enabled && ! tablesUpdatedInUTL.Contains(table))
                tablesUpdatedInUTL.Add(table);
        }
		
		static public void InvalidateAll()
		{
			DateTime dt = DateTime.Now.ToUniversalTime();
			updatedTables.Set<DateTime>(CacheFactory.CACHE_SD, FORCED_INVALIDATE, new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Utc));		// Sin milisegundos
		}
		
		static public void Invalidate(String tableName)
		{
			updatedTables.Clear(CacheFactory.CACHE_SD, NormalizeKey(tableName));
		}
		/// <summary>
        /// Commit of updated data. The tables modified so far are registered with the commit timestamp.
        /// </summary>
        public void ReccordUpdates()
        {
			if (enabled && tablesUpdatedInUTL.Count>0)
			{
				DateTime dt = DateTime.Now.ToUniversalTime();
				dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Utc);      
				ICacheService2 updatedTablesBulk = updatedTables as ICacheService2;
				if (updatedTablesBulk != null)
				{
					updatedTablesBulk.SetAll(CacheFactory.CACHE_SD, tablesUpdatedInUTL.Select(tbl => NormalizeKey(tbl)), Enumerable.Repeat(dt, tablesUpdatedInUTL.Count));
				}
				else
				{
					foreach (string tbl in tablesUpdatedInUTL)
					{
						updatedTables.Set(CacheFactory.CACHE_SD, NormalizeKey(tbl), dt);
					}
				}
				tablesUpdatedInUTL.Clear();
			}
        }
        /// <summary>
        /// Rollback. Delete all pending tables without registering them.
        /// </summary>
        public void DiscardUpdates()
        {
			if (enabled)
			{
				tablesUpdatedInUTL.Clear();
			}
        }

        /// <summary>
        /// Given a query and an update date, check if the query info is current.
        /// </summary>
        /// <param name="queryId"></param>
        /// <param name="dateLastModified"></param>
        /// <param name="dateUpdated"></param>
        /// <returns>Unknown/Invalid/UpToDate</returns>
		static public DataUpdateStatus CheckDataStatus(string queryId, DateTime dateLastModified, out DateTime dateUpdated)
		{
			dateUpdated = GxContext.StartupDate;	// by default the data is as old as the startup moment of the app

			if (enabled)
			{
				if (!QueryTables.ContainsKey(queryId))      // There is no table definition for the query -> status unknown
					return DataUpdateStatus.Unknown;

				ICacheService2 updatedTablesBulk = updatedTables as ICacheService2;
				IDictionary<string, DateTime> dateUpdates;
				List<string> qTables = QueryTables[queryId];
				if (updatedTablesBulk != null)
				{
					dateUpdates = updatedTablesBulk.GetAll<DateTime>(CacheFactory.CACHE_SD, qTables); //Value is Date.MinValue for non-existing key in cache
				}
				else
				{
					dateUpdates = new Dictionary<string, DateTime>();
					foreach (string tbl in qTables)
					{
						if (updatedTables.Get<DateTime>(CacheFactory.CACHE_SD, tbl, out DateTime tblDt))
							dateUpdates[tbl] = tblDt;
					}
				}

				DateTime maxDateUpdated = dateUpdates.Values.Max();  //Get the newest modification date.
				if (maxDateUpdated > dateUpdated)
					dateUpdated = maxDateUpdated;

				if (dateUpdated > dateLastModified)    // If any of the query tables were modified -> the status of the info is INVALID, you have to refresh
					return DataUpdateStatus.Invalid;

				return DataUpdateStatus.UpToDate;
			}
			else
			{
				return DataUpdateStatus.Unknown;
			}

		}
        
        static void loadQueryTables()
        {
            string basePath = Path.Combine(GxContext.StaticPhysicalPath(), "Metadata", "TableAccess");
			queryTables = new ConcurrentDictionary<string, List<string>>();
			if (Directory.Exists(basePath))
			{
				foreach (string file in Directory.GetFiles(basePath, "*.xml"))
				{
					string objname = Path.GetFileNameWithoutExtension(file);
					List<string> lst = new List<string>();
					lst.Add(FORCED_INVALIDATE);
					bool readingTables = false;
                using (XmlTextReader tr = new XmlTextReader(Path.Combine(basePath, file)))
                {
                    while (tr.Read())
                    {
                        if (tr.Name == "Dependencies" && !readingTables)
                        {
                            readingTables = true;
                            continue;
                        }
                        if (tr.Name == "Dependencies" && readingTables)
                        {
                            readingTables = false;
							queryTables[NormalizeKey(objname)] = lst;
                            continue;
							}
							if (tr.HasAttributes)
							{
								string nme = tr.GetAttribute("name");
								if (tr.Name == "Table" && readingTables && nme != null && !lst.Contains(nme))
									lst.Add(NormalizeKey(nme));
							}
						}
					}
				}
			}
        }

        static public ConcurrentDictionary<string, List<string>> QueryTables
        {
            get
            {
                if (queryTables == null)
                {
                    lock (instanceSync)
                    {
                        if (queryTables == null)
                        {
                            loadQueryTables();
                        }
                    }
                }
                return queryTables;
            }
        }
		static string NormalizeKey(string key)
		{
			if (!string.IsNullOrEmpty(key))
				return key.ToLower();
			else
				return key;
		}
    }
    public enum DataUpdateStatus
    {
        Unknown,
        Invalid,
        UpToDate
    }
}
