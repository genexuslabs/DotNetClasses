using System.Management.Instrumentation;
using System.Collections;
using System;
using GeneXus.Data;
using GeneXus.Data.ADO;
using GeneXus.Data.NTier;
using GeneXus.Data.NTier.ADO;
using GeneXus.XML;
using System.Diagnostics;
using log4net;
using System.Security;
using System.Collections.Concurrent;

namespace GeneXus.Performance
{

	[InstrumentationClass(InstrumentationType.Instance)]
	[ManagedName("Procedure")]
	[SecuritySafeCritical]
	public class WMIProcedure
	{
		private long count;  
		private string name;
		private DateTime timeLastExecute;
		private long totalTimeExecute;
		private float averageTimeExecute;
		private long worstTimeExecute;
		private long bestTimeExecute;
	
		public WMIProcedure(string name)
		{
			this.name = name;
			Instrumentation.Publish(this);
		}	
	
		public long Count
		{
			get
			{
				return count;
			}
		}
 		
		public string Name
		{
			get
			{
				return name;
			}
		}
	
		public DateTime TimeLastExecute
		{
			get
			{
				return timeLastExecute;
			}
		}
	
		public long TotalTimeExecute
		{
			get
			{
				return totalTimeExecute;
			}
		}
	
		public float AverageTimeExecute
		{
			get
			{
				return averageTimeExecute;
			}
		}
	
		public long WorstTimeExecute
		{
			get
			{
				return worstTimeExecute;
			}
		}
	
		public long BestTimeExecute
		{
			get
			{
				return bestTimeExecute;
			}
		}

		public void incCount()
		{
			count ++;
			timeLastExecute = new DateTime();
		}
		
		public void setTimeExecute(long time)
		{
			totalTimeExecute = totalTimeExecute + time;
			averageTimeExecute = totalTimeExecute / count;
			if (time > worstTimeExecute)
				worstTimeExecute = time;
			if (time < bestTimeExecute || bestTimeExecute == 0)
				bestTimeExecute = time;
		}
	}

	public class ProceduresInfo
	{
		static private Hashtable procedureInfo = new Hashtable();	
		public WMIProcedure wmiprocedure;
	
		public ProceduresInfo()
		{
		}
  	
		static public ProcedureInfo addProcedureInfo(String name)
		{
			if (!procedureInfo.ContainsKey(name))
			{
				ProcedureInfo pInfo = new ProcedureInfo(name);
				procedureInfo.Add(name, pInfo);
			}
			return (ProcedureInfo) procedureInfo[name];
		}	
	
		static public ProcedureInfo getProcedureInfo(String name)
		{
			return (ProcedureInfo) procedureInfo[name];;
		}  
	}

	public class ProcedureInfo
	{
		private long count;  
		private String name;
		private DateTime timeLastExecute;
		private long totalTimeExecute;
		private float averageTimeExecute;
		private long worstTimeExecute;
		private long bestTimeExecute;
	
		public ProcedureInfo(String name)
		{
			this.name = name;
		}	
	
		public long getCount()
		{
			return count;
		}
  
		public void incCount()
		{
			count ++;
			timeLastExecute = DateTime.Now;
		}	
	
		public String getName()
		{
			return name;
		}
	
		public DateTime getTimeLastExecute()
		{
			return timeLastExecute;
		}
	
		public long getTotalTimeExecute()
		{
			return totalTimeExecute;
		}
	
		public float getAverageTimeExecute()
		{
			return averageTimeExecute;
		}
	
		public long getWorstTimeExecute()
		{
			return worstTimeExecute;
		}
	
		public long getBestTimeExecute()
		{
			return bestTimeExecute;
		}
	
		public void setTimeExecute(long time)
		{
			totalTimeExecute = totalTimeExecute + time;
			averageTimeExecute = totalTimeExecute / count;
			if (time > worstTimeExecute)
				worstTimeExecute = time;
			if (time < bestTimeExecute || bestTimeExecute == 0)
				bestTimeExecute = time;
		}
	}

	public class DataStoreProviderInfo
	{
		private long sentenceCount;
		private long sentenceSelectCount;
		private long sentenceUpdateCount;
		private long sentenceDeleteCount;
		private long sentenceInsertCount;
		private long sentenceCallCount;	
		private long sentenceDirectSQLCount;

		private Hashtable sentenceInfo;		

		public DataStoreProviderInfo(string name)
		{
			this.sentenceInfo = new Hashtable();
		}
		public Hashtable SentenceInfo
		{
			get{ return sentenceInfo;}
			set{sentenceInfo = value;}
		}

		public SentenceInfo AddSentenceInfo(String key, String sqlSentence)
		{
			if (!sentenceInfo.ContainsKey(key))
			{
				SentenceInfo sInfo = new SentenceInfo(key, sqlSentence);
				sentenceInfo.Add(key, sInfo);
				sInfo.sentence = new WMISentence(this, key);
			}
			return (SentenceInfo) sentenceInfo[key];
		}	
	
		public SentenceInfo GetSentenceInfo(String key)
		{
			return (SentenceInfo) sentenceInfo[key];;
		}  
		public void incSentenceCount()
		{
			sentenceCount ++;
		}
  
		public long getSentenceSelectCount()
		{
			return sentenceSelectCount;
		}
  
		public void incSentenceSelectCount()
		{
			sentenceSelectCount ++;
		}  
  
		public long getSentenceUpdateCount()
		{
			return sentenceUpdateCount;
		}
  
		public void incSentenceUpdateCount()
		{
			sentenceUpdateCount ++;
		}  
  
		public long getSentenceDeleteCount()
		{
			return sentenceDeleteCount;
		}
  
		public void incSentenceDeleteCount()
		{
			sentenceDeleteCount ++;
		}  
  
		public long getSentenceInsertCount()
		{
			return sentenceInsertCount;
		}
  
		public void incSentenceInsertCount()
		{
			sentenceInsertCount ++;
		}  
  
		public long getSentenceCallCount()
		{
			return sentenceCallCount;
		}  
  
		public void incSentenceCallCount()
		{
			sentenceCallCount ++;
		}  
  
		public long getSentenceDirectSQLCount()
		{
			return sentenceDirectSQLCount;
		}
  
		public void incSentenceDirectSQLCount()
		{
			sentenceDirectSQLCount ++;
		}  
	
	}
	[InstrumentationClass(InstrumentationType.Instance)]
	[ManagedName("Sentence")]
	[SecuritySafeCritical]
	public class WMISentence
	{
	
		SentenceInfo sentenceInfo;
	
		public WMISentence(DataStoreProviderInfo dsInfo, String name)
		{
			sentenceInfo = dsInfo.GetSentenceInfo(name);
			sentenceInfo.sentence = this;
			Instrumentation.Publish(this);

		}

		public String Name
		{
			get { return sentenceInfo.ObjectName; }
		}

		public long Count
		{
			get{return sentenceInfo.SentenceCount;}
		}
  
		public String SQLStatement
		{
			get{ return sentenceInfo.SQLSentence;}
		}
  
		public DateTime LastExecute
		{
			get{return sentenceInfo.TimeLastExecute;}
		}
  
		public long TotalTime
		{
			get{return sentenceInfo.TotalTimeExecute;}
		}
  
		public float AverageTime
		{
			get{return sentenceInfo.AverageTimeExecute;}
		}
  
		public long WorstTime
		{
			get{return sentenceInfo.WorstTimeExecute;}
		}
  
		public long BestTime
		{
			get{return sentenceInfo.BestTimeExecute;}
		} 
  
	}

	[SecuritySafeCritical]
	public class SentenceInfo
	{
		private long sentenceCount;  
		private String sqlSentence;
		private String name;
		private DateTime timeLastExecute;
		private long totalTimeExecute;
		private float averageTimeExecute;
		private long worstTimeExecute;
		private long bestTimeExecute;
		private long maxTimeForNotification = 10000;
		
		public WMISentence sentence;

		public SentenceInfo(string key, String sqlSentence)
		{
			this.sqlSentence = sqlSentence;
			this.name = key;
			sentenceCount=0;
			Instrumentation.Publish(this);
		}	
	
		public long SentenceCount
		{
			get{return sentenceCount;}
		}
  
		public void incSentenceCount()
		{
			sentenceCount ++;
			timeLastExecute = DateTime.Now;
		}	
	
		public String SQLSentence
		{
			get{return sqlSentence;}
		}
	
		public DateTime TimeLastExecute
		{
			get{return timeLastExecute;}
		}
	
		public long TotalTimeExecute
		{
			get{return totalTimeExecute;}
		}
	
		public float AverageTimeExecute
		{
			get{ return averageTimeExecute;}
		}
	
		public long WorstTimeExecute
		{
			get{return worstTimeExecute;}
		}
	
		public long BestTimeExecute
		{
			get{return bestTimeExecute;}
		}
	
		public long MaxTimeForNotification
		{
			get{return maxTimeForNotification;}
		}

		public string ObjectName
		{
			get { return name; }
		}

		public void setMaxTimeForNotification(long value)
		{
			maxTimeForNotification = value;
		}	
	
		public void Dump(GXXMLWriter writer)
		{
			writer.WriteStartElement("SQLStatement");
			writer.WriteStartElement("SQLStatement");
			writer.WriteCData(sqlSentence);
			writer.WriteEndElement();
			writer.WriteElement("Count",sentenceCount);
			writer.WriteElement("LastExecute",timeLastExecute.ToString());
			writer.WriteElement("TotalTime",totalTimeExecute);
			writer.WriteElement("AverageTime",averageTimeExecute);
			writer.WriteElement("WorstTime",worstTimeExecute);
			writer.WriteElement("BestTime",bestTimeExecute);
			writer.WriteEndElement();
		}
	
		public void setTimeExecute(long time)
		{
			totalTimeExecute = totalTimeExecute + time;
			averageTimeExecute = totalTimeExecute / sentenceCount;
			if (time > worstTimeExecute)
				worstTimeExecute = time;
			if (time < bestTimeExecute || bestTimeExecute == 0)
				bestTimeExecute = time;
		}
	}

	public class PerformanceCounter
	{
		long val;
		public long RawValue
		{
			get{ return val;}
		}
		public void Increment()
		{
			val++;
		}
		public PerformanceCounter(string name, string name1, string instance, bool read)
		{
		}
	}
	[InstrumentationClass(InstrumentationType.Abstract)]
	[ManagedName("DataStoreProviders")]
	[SecuritySafeCritical]
	public class WMIDataStoreProvidersBase
	{
		protected PerformanceCounter statementCounter;
		protected PerformanceCounter statementSelectCounter;
		protected PerformanceCounter statementUpdateCounter;
		protected PerformanceCounter statementDeleteCounter;
		protected PerformanceCounter statementInsertCounter;
		protected PerformanceCounter statementCallCounter;
		protected PerformanceCounter statementDirectSQLCounter;
		[IgnoreMember]
		protected static WMIDataStoreProvidersBase instance;
		[IgnoreMember]
		protected static ConcurrentDictionary<string, DataStoreProviderInfo> dataStoreInfo;
		[IgnoreMember]
		protected static ConcurrentDictionary<string, WMIDataStoreProvider> dataStoreProviderInfo;
		[IgnoreMember]
		protected static object syncObj=new object();

		public long StatementCount
		{
			get
			{
				return statementCounter.RawValue;
			}
		}
    
		public long StatementSelectCount
		{
			get
			{
				return statementSelectCounter.RawValue;
			}
		}
    
		public long StatementUpdateCount
		{
			get
			{
				return statementUpdateCounter.RawValue;
			}
		}
    
		public long StatementDeleteCount
		{
			get
			{
				return statementDeleteCounter.RawValue;
			}
		}
    
		public long StatementInsertCount
		{
			get
			{
				return statementInsertCounter.RawValue;
			}
		}
    
		public long StoredProcedureCount
		{
			get
			{
				return statementCallCounter.RawValue;
			}
		}  
  
		public long StatementDirectSQLCount
		{
			get
			{
				return statementDirectSQLCounter.RawValue;
			}
		}
		public DataStoreProviderInfo GetDataStoreProviderInfo(string datastoreName)
		{
			return dataStoreInfo[datastoreName];
		}

		public void AddDataStoreInfo(string datastoreName, bool isremote)
		{
			if (dataStoreInfo == null)
			{
				dataStoreInfo = new ConcurrentDictionary<string, DataStoreProviderInfo>();
			}
			if (!dataStoreInfo.ContainsKey(datastoreName))
			{
				dataStoreInfo.TryAdd(datastoreName, new DataStoreProviderInfo(datastoreName));
			}
		}

		public WMIDataStoreProvider AddDataStoreProvider(string datastoreName)
		{
			if (dataStoreProviderInfo == null)
			{
				dataStoreProviderInfo = new ConcurrentDictionary<string, WMIDataStoreProvider>();
			}
			if (dataStoreProviderInfo.ContainsKey(datastoreName))
			{
				return dataStoreProviderInfo[datastoreName];
			}
			else
			{
				WMIDataStoreProvider dInfo = new WMIDataStoreProvider(datastoreName);
				dataStoreProviderInfo.TryAdd(datastoreName, dInfo);
				return dInfo;
			}
		}

		private void IncStatementCount()
		{
			statementCounter.Increment();
		}
		private void IncStatementSelectCount()
		{
			statementSelectCounter.Increment();
		}
		private void IncStatementUpdateCount()
		{
			statementUpdateCounter.Increment();
		}
		private void IncStatementDeleteCount()
		{
			statementDeleteCounter.Increment();
		}
		private void IncStatementInsertCount()
		{
			statementInsertCounter.Increment();
		}
		private void IncStoredProcedureCount()
		{
			statementCallCounter.Increment();
		}
		private void IncStatementDirectSQLCount()
		{
			statementDirectSQLCounter.Increment();
		}

		public void IncSentencesCount(ICursor cursor)
		{
			IncStatementCount();

			string sqlSnt = cursor.SQLStatement;

			if (cursor is ForEachCursor)
			{
				IncStatementSelectCount();
			}
			else if (cursor is UpdateCursor)
			{
				if (sqlSnt.ToUpper().StartsWith("UPDATE"))
				{
					IncStatementUpdateCount();
				}
				else if (sqlSnt.ToUpper().StartsWith("DELETE"))
				{
					IncStatementDeleteCount();
				}
				else if (sqlSnt.ToUpper().StartsWith("INSERT"))
				{
					IncStatementInsertCount();
				}
			}
			else if (cursor is CallCursor)
			{
				IncStoredProcedureCount();
			}
			else if (cursor is DirectStatement)
			{
				IncStatementDirectSQLCount();
			}

		}

 	}
	[InstrumentationClass(InstrumentationType.Instance)]
	[ManagedName("LocalDataStoreProviders")]
	[SecuritySafeCritical]
	public class WMIDataStoreProviders:WMIDataStoreProvidersBase
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(WMIDataStoreProviders));

		private WMIDataStoreProviders()
		{
			Instrumentation.Publish(this);

			try
			{
				
				if (!PerformanceCounterCategory.Exists("GeneXusCounters"))
				{
					CounterCreationDataCollection CounterDatas = new CounterCreationDataCollection();
					CounterCreationData statementCount = new CounterCreationData();
					statementCount.CounterName = "StatementCount";
					statementCount.CounterHelp = "SQL Statement Count";
					statementCount.CounterType = PerformanceCounterType.NumberOfItems64;
					CounterDatas.Add(statementCount);
					PerformanceCounterCategory.Create( "GeneXusCounters", "GeneXus Counters", CounterDatas);
				}
			}
			catch(Exception e)
			{
				GXLogging.Error(log, "WMI Performance countes error", e);
			}

			try{
				statementCounter = new PerformanceCounter( "GeneXusCounters","StatementCount","instance1", false);
				statementSelectCounter = new PerformanceCounter("GeneXusCounters","StatementCount","instance1", false);
				statementUpdateCounter = new PerformanceCounter("GeneXusCounters","StatementUpdateCount","instance1", false);
				statementDeleteCounter = new PerformanceCounter("GeneXusCounters","StatementDeleteCount","instance1", false);
				statementInsertCounter = new PerformanceCounter("GeneXusCounters","StatementInsertCount","instance1", false);
				statementCallCounter = new PerformanceCounter("GeneXusCounters","StatementCallCount","instance1", false);
				statementDirectSQLCounter = new PerformanceCounter("GeneXusCounters","StatementDirectSQLCount","instance1", false);
			}
			catch(Exception e)
			{
				GXLogging.Error(log, "WMI Performance countes error", e);
			}
		}

		public static WMIDataStoreProvidersBase Instance()
		{
			lock(syncObj)
			{
				if (instance==null)
				{
					instance = new WMIDataStoreProviders();
				}
			}
			return instance;
		}
	}

	[InstrumentationClass(InstrumentationType.Instance)]
	[ManagedName("LocalDataStoreProvider")]
	[SecuritySafeCritical]

	public class WMIDataStoreProvider:WMIDataStoreProviderBase
	{
		public WMIDataStoreProvider(string datastoreName) : base()
		{
			instanceProvidersBase = WMIDataStoreProviders.Instance();
			Name = datastoreName;
			instanceProvidersBase.AddDataStoreInfo(Name, false);
			Instrumentation.Publish(this);
		}

	}

	[InstrumentationClass(InstrumentationType.Abstract)]
	[ManagedName("DataStoreProvider")]
	[SecuritySafeCritical]
	public class WMIDataStoreProviderBase
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(WMIDataStoreProviderBase));

		private long statementCount;
		private long statementSelectCount;
		private long statementUpdateCount;
		private long statementDeleteCount;
		private long statementInsertCount;
		private long statementCallCount;	
		private long statementDirectSQLCount;
		protected static bool firstTime=true;
		protected DateTime beginExecute;

		private string name;
		protected WMIDataStoreProvidersBase instanceProvidersBase;
		
		public WMIDataStoreProviderBase()
		{
		}

		public string Name
		{
			get
			{
				return name;
			}set{ name=value;}
		}
    
		public long StatementCount
		{
			get
			{
				return statementCount;
			}
		}
    
		public long StatementSelectCount
		{
			get
			{
				return statementSelectCount;
			}
		}
    
		public long StatementUpdateCount
		{
			get
			{
				return statementUpdateCount;
			}
		}
    
		public long StatementDeleteCount
		{
			get
			{
				return statementDeleteCount;
			}
		}
    
		public long StatementInsertCount
		{
			get
			{
				return statementInsertCount;
			}
		}
    
		public long StatementCallCount
		{
			get
			{
				return statementCallCount;
			}
		}  
  
		public long StatementDirectSQLCount
		{
			get
			{
				return statementDirectSQLCount;
			}
		}

		public void DumpDataStoreInformation(GXXMLWriter writer)
		{
			writer.WriteStartElement("DataStoreProvider");
			writer.WriteAttribute("Name", name);
			writer.WriteElement("Total_SQLStatementCount", statementCount);
			writer.WriteElement("Select_SQLStatementCount", statementSelectCount);
			writer.WriteElement("Update_SQLStatementCount", statementUpdateCount);			
			writer.WriteElement("Delete_SQLStatementCount", statementDeleteCount);			
			writer.WriteElement("Insert_SQLStatementCount", statementInsertCount);
			writer.WriteElement("StoredProcedureCount", statementCallCount);		
			writer.WriteElement("SQLCommandCount", statementDirectSQLCount);	
			
			writer.WriteEndElement();
		}

		public void EndExecute(ICursor cursor, IGxConnection con)
		{
			if (con != null)
				con.LastSQLStatementEnded = true;

			DataStoreProviderInfo dsInfo = WMIDataStoreProviders.Instance().GetDataStoreProviderInfo(this.name);
			SentenceInfo sInfo = dsInfo.GetSentenceInfo(this.name + "_" + cursor.Id);
			long diff = DateTime.Now.Subtract(beginExecute).Ticks / TimeSpan.TicksPerMillisecond;
			sInfo.setTimeExecute(diff);
		}

		public void BeginExecute(ICursor cursor, IGxConnection con)
		{
			if (con != null)
			{
				con.LastObject = this.name;
				con.LastSQLStatementEnded = false;
				ServerUserInformation sui = (ServerUserInformation)GxConnectionManager.Instance.GetUserInformation(con.DataStore.Handle, con.DataStore.Id);
				if (sui!=null)
				{
					sui.LastSQLStatement = cursor.SQLStatement;
					sui.LastSQLStatementTime = DateTime.Now.ToString();
				}
			}
		}
		public void IncSentencesCount(ICursor cursor)
		{
			try
			{
				GXLogging.Debug(log, "IncSentencesCount");
				DataStoreProviderInfo dsInfo =  (DataStoreProviderInfo)instanceProvidersBase.GetDataStoreProviderInfo(this.name);
				dsInfo.incSentenceCount();
				SentenceInfo sInfo = dsInfo.AddSentenceInfo(this.name + "_" + cursor.Id, cursor.SQLStatement);
				sInfo.incSentenceCount();

				instanceProvidersBase.IncSentencesCount(cursor);

				beginExecute = DateTime.Now;

				this.statementCount++;

				string sqlSnt = cursor.SQLStatement;

				if (cursor is ForEachCursor)
				{
					statementSelectCount++;
				}
				else if (cursor is UpdateCursor)
				{
					if (sqlSnt.ToUpper().StartsWith("UPDATE"))
					{
						statementUpdateCount++;
					}
					else if (sqlSnt.ToUpper().StartsWith("DELETE"))
					{
						statementDeleteCount++;
					}
					else if (sqlSnt.ToUpper().StartsWith("INSERT"))
					{
						statementInsertCount++;
					}
				}
				else if (cursor is CallCursor)
				{
					statementCallCount++;
				}
				else if (cursor is DirectStatement)
				{
					statementDirectSQLCount++;
				}

			}
			catch(Exception e)
			{
				GXLogging.Error(log, "IncSentencesCount Error", e);
			}
		}

	}

}
