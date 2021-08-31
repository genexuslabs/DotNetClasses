
using GeneXus.Application;
using GeneXus.Cache;
using GeneXus.Configuration;
using GeneXus.Data.NTier.ADO;
using GeneXus.Management;
using GeneXus.Reorg;
using GeneXus.Utils;
using GeneXus.XML;
using log4net;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Threading;
using TZ4Net;

namespace GeneXus.Data.ADO
{

	public class ServerUserInformation : ConcurrentDictionary<string, GxConnection>
	{
#region WMI Methods
		string lastSQLStatementTime;
		string lastSQLStatement;
		public string  LastSQLStatementTime
		{
			get{ return lastSQLStatementTime;}
			set{ lastSQLStatementTime = value;}
		}
		public string  LastSQLStatement
		{
			get{ return lastSQLStatement;}
			set{ lastSQLStatement = value;}
		}
#endregion
	}
	public sealed class GxConnectionManagerWin : GxConnectionManager
	{
		private object syncConnection = new object();

		public override void IncOpenHandlesImpl(GxConnection con)
		{
			lock (syncConnection)
			{
				base.IncOpenHandlesImpl(con);
			}
		}
		public override void DecOpenHandlesImpl(GxConnection con)
		{
			lock (syncConnection)
			{
				base.DecOpenHandlesImpl(con);
			}
		}
	}

	public class GxConnectionManager :IGxConnectionManager
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.ADO.GxConnectionManager));

		private static volatile GxConnectionManager instance;
        private static object instanceSync = new Object();

        private ConcurrentDictionary<int, ServerUserInformation> userConnections = new ConcurrentDictionary<int, ServerUserInformation>();
		private static long CONN_TIMEOUT = 30 * TimeSpan.TicksPerSecond; 
		private static int SLEEP_TIME = 30 * 1000;
		
		protected GxConnectionManager()
		{
			string value;
			
			{
				
			}
			if (Config.GetValueOf("CONN_TIMEOUT", out value))
			{
				try
				{
					CONN_TIMEOUT = int.Parse(value) * TimeSpan.TicksPerSecond;
					GXLogging.Debug(log, "Setting CONN_TIMEOUT to: " + value + " seconds");
				}
				catch(Exception e){

					GXLogging.Error(log, "Error setting CONN_TIMEOUT ", e);
				}
			}

		}
		
		public static IGxConnectionManager Instance
		{
			get 
			{
				if (instance == null) 
				{
					lock (instanceSync) 
					{
						if (instance == null) 
						{
							if (GxContext.IsHttpContext || GxContext.IsRestService)
								instance = new GxConnectionManager();
							else
								instance = new GxConnectionManagerWin();
						}
					}
				}
				return instance;
			}
		}

		
		public void	RefreshTimeStamp(int handle, string dataSource)
		{
			ServerUserInformation sui = userConnections[handle];
			sui[dataSource].RefreshTimeStamp();
		}

		public IGxConnection IncOpenHandles(int handle, string dataSource)
		{
			ServerUserInformation sui = userConnections[handle];

			GxConnection con = sui[dataSource];
			IncOpenHandlesImpl(con);
			GXLogging.Debug(log, "GxConnectionManager.IncOpenHandles   handle '" + handle + "', datasource '" + dataSource + "', openhandles " + con.OpenHandles);
			return con;
		}

		public virtual void IncOpenHandlesImpl(GxConnection con)
		{
			if (!con.Opened)
			{
				con.Open();
			}
			con.IncOpenHandles();
		}

		public void DecOpenHandles(int handle, string dataSource)
		{
			ServerUserInformation sui = userConnections[handle];
			GxConnection con = sui[dataSource];
			DecOpenHandlesImpl(con);
			GXLogging.Debug(log, "GxConnectionManager.DecOpenHandles   handle '" + handle + "', datasource '" + dataSource + "', openhandles " + con.OpenHandles);
		}

		public virtual void DecOpenHandlesImpl(GxConnection con)
		{
			con.DecOpenHandles();
		}

		public IGxConnection SetAvailable(int handle, string dataSource, bool available)
		{
			GXLogging.Debug(log, "GxConnectionManager.SetAvailable   handle '" + handle + "', available '" + available +"', datasource '" + dataSource + "'" );
			ServerUserInformation sui = userConnections[handle];
			GxConnection con = sui[dataSource];
			
			if (!con.Opened)
			{
				con.Open();
			}
			con.Available=available;
			con.RefreshTimeStamp();
			return con;
		}

		public IGxConnection NewConnection(int handle, string dataSource)
		{
			GXLogging.Debug(log, "GxConnectionManager.NewConnection   handle: " + handle + ", datasource:" + dataSource);
			GxConnection con = new GxConnection(handle); 
			if (userConnections.ContainsKey(handle))
			{
				userConnections[handle].TryAdd(dataSource, con);
			}
			else
			{
				ServerUserInformation s = new ServerUserInformation();
				s[dataSource] = con;
				userConnections[handle]=s;
			}
			return con;
		}

		public void RemoveAllConnections(int handle)
		{
			try
			{
				if (userConnections.ContainsKey(handle))
				{
					GXLogging.Debug(log, "RemoveAllConnections   handle " + handle );
					ServerUserInformation sui = userConnections[handle];
					IEnumerator connections = sui.Values.GetEnumerator();
					while (connections != null && connections.MoveNext())
					{
						GxConnection con = ((GxConnection)connections.Current);
						if (con!=null) 
						{
							con.Dispose();
						}
					}
                    ServerUserInformation removedServerUI;
					userConnections.TryRemove(handle, out removedServerUI);
				}
			}
			catch(Exception e)
			{
				GXLogging.Error(log, "RemoveAllConnections Error", e);
				throw e;
			}
		}

		public void RemoveConnection(int handle, string dataSource)
		{
			
            ServerUserInformation sui;
			if (userConnections.TryGetValue(handle, out sui))
			{
				GXLogging.Debug(log, "RemoveConnection   handle " + handle + ",datasource:" + dataSource);
				GxConnection con = sui[dataSource];
                if (sui.TryRemove(dataSource, out con))
                    con.Dispose();
                ServerUserInformation suiDeleted;
				if (sui.Count==0) userConnections.TryRemove(handle, out suiDeleted);
			}
		}


		public void run()
		{

			GXLogging.Debug(log, "Start GxConnectionManager.run");
			while (true)
			{
				try 
				{
					Thread.Sleep(SLEEP_TIME);
					GXLogging.Debug(log, "GxConnectionManager.run.sleep");
				}
				catch 
				{
				}

				killConnections();
			}
		}
		public object GetUserInformation(int handle, string dataSource)
		{	
            ServerUserInformation data;
            userConnections.TryGetValue(handle, out data);
            return data;
		}

		public bool CloseUserConnections(int handle, string dataSource)
		{
			
            ServerUserInformation sui = userConnections[handle];
			if (userConnections.TryGetValue(handle, out sui))
			{
				GXLogging.Debug(log, "RemoveConnection   handle " + handle + ",datasource:" + dataSource);
				GxConnection con = sui[dataSource];
				if (con!=null && con.Opened)
					if (con.Available && con.OpenHandles<=0)
					{
						con.Close();
						return true;
					}
					else 	return false;
			}
			return true;
		}

		private void killConnections()
		{
			long now = DateTime.Now.Ticks;
				
			GXLogging.Debug(log, "Start GxConnectionManager.killConnections, Parameters: now '" + now + "'");
			int count=userConnections.Count;
			IEnumerator keys= userConnections.Keys.GetEnumerator();

			for (int i=0; i<count && keys.MoveNext(); i++)
			{
				int handle=(int)keys.Current;
				ServerUserInformation sui = userConnections[handle];
				IEnumerator suiKeys= sui.Keys.GetEnumerator();
						
				while (suiKeys.MoveNext())
				{
					string dataSource=(string)suiKeys.Current;

					GxConnection userConn = sui[dataSource];
					GXLogging.Debug(log, "User handle:", ()=> handle + ", dataSource:" + dataSource + ", openHandles: "+ userConn.OpenHandles + " available: "+ userConn.Available + ", opened:" + userConn.Opened);

					if (userConn.Opened && userConn.Available && (userConn.OpenHandles<=0) && (  now - userConn.TimeStamp > CONN_TIMEOUT))
					{
						GXLogging.Debug(log, "GxConnectionManager.killConnections disconnecting  handle " + userConn.Handle+ ", datasource: "+ dataSource +". Connection timed out - inactive for " + (( now - userConn.TimeStamp ) / TimeSpan.TicksPerSecond) + " seconds.");
						userConn.Close();
					}
				}
			}
			GXLogging.Debug(log, "Return GxConnectionManager.killConnections");

		}
#region WMI Members 

		public int Size
		{
			get
			{
				int count=userConnections.Count;
				IEnumerator keys= userConnections.Keys.GetEnumerator();
				int size =0;
				for (int i=0; i<count && keys.MoveNext(); i++)
				{
					int handle=(int)keys.Current;
					ServerUserInformation sui = (ServerUserInformation)userConnections[handle];
					size += sui.Count;
				}
				return size;
			}
		}

		public bool UnlimitedSize
		{
			get
			{
				return false;
			}
		}

#endregion
	}
    enum SHOW_PROMPT:short { ALWAYS=1, NEVER=2, IF_REQUIRED=3 };

#if !NETCORE
	
	public class GxConnection: MarshalByRefObject, IGxConnection
#else
    public class GxConnection: IGxConnection
#endif
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.ADO.GxConnection));
		private IDbTransaction transaction;
		private short lastErrorCode;
		private string lastErrorMsg;
		protected GxAbstractConnectionWrapper connection;

        private GxConnectionCache connectionCache;
	
		private string userId;
		private string userPassword;
		private string port;
        private string currentSchema;
        private string databaseName=string.Empty;
		private string datasourceName;
		private string driverName;
		private int timeout=30;
		private string data;
		private SHOW_PROMPT showPrompt;
		private bool available;
		private long timeStamp;
		private int handle;
		private int openHandles;
		private short spid;
		private string currentStmt;
		private DateTime startTimeCurrentStmt;
		private DateTime createTime;
		private bool autoCommit;
		private string blobPath;
		private bool cache;
		private GxDataRecord m_dataRecord;
		private bool m_opened;
		private IGxDataStore dataStore;
		private bool inBeforeConnect;
		private bool inAfterConnect;
		private bool uncommitedChanges;
		private bool multithreadSafe;
		private int executingCursors;
		private object executingCursorSync;
		//WMI
		private WMIConnection wmiconnection;
		private string lastSQLStatement;
        private string lastObject;
		private bool lastSqlStatementEnded;
        
		public OlsonTimeZone ClientTimeZone
		{
			get { return dataStore.ClientTimeZone; } 
		}
		public string CurrentStmt
		{
			get
			{
				return currentStmt;
			}
			set
			{
				currentStmt=value;
				if (Preferences.Instrumented && value!=null)
				{
					startTimeCurrentStmt = DateTime.Now;
					lastSQLStatement = value;
				}
			}
		}

		public void Dispose()
		{
			Close();
		}

		public IGxDataStore DataStore
		{
			get{ return dataStore;}
			set{ dataStore=value;}
		}

		public GxConnection(int hnd)
		{
			handle = hnd;
			connectionCache=new GxConnectionCache(this);
			available=true;
#if !NETCORE
			string value;
            if (!GxContext.IsHttpContext && !(GxContext.isReorganization && GXReorganization._ReorgReader!=null && !GXReorganization._ReorgReader.GuiDialog))
			{
				//Win && ! -nogui reorganization
                
				if (Config.GetValueOf("REORG_SHOW_PROMPT", out value) && value.Equals("N", StringComparison.OrdinalIgnoreCase))
					showPrompt = SHOW_PROMPT.NEVER;
				else
					showPrompt = SHOW_PROMPT.IF_REQUIRED;
			}
			else
#endif
			showPrompt = SHOW_PROMPT.NEVER;
			autoCommit=GxContext.isReorganization;
			executingCursorSync=new object();
		}

		public bool Opened
		{
			get { return m_opened;}
			set { m_opened=value;}
		}

        public GxConnectionCache ConnectionCache
		{
			get{return connectionCache;}
			set{connectionCache=value;}
		}
		
		public int StmtCachedCount
		{
			get{return connectionCache.StmtCachedCount;}
		}
		public GxDataRecord DataRecord
		{
			get{ return m_dataRecord;}
			set{ m_dataRecord=value;}
		}
		private void BeforeConnect()
		{
			if (!inBeforeConnect)
			{
				
				inBeforeConnect=true;
				if (DataStore.BeforeConnect()) 
				{
					Disconnect();
				}
				inBeforeConnect=false;
			}
		}
		public void AfterConnect()
		{
			m_dataRecord.SetTimeout(GxConnectionManager.Instance, this, handle);
			if (!inAfterConnect)
			{
				
				inAfterConnect = true;
				DataStore.AfterConnect();
				inAfterConnect = false;
			}
		}
		public void Open()
		{
				
			GXLogging.Debug(log, "Start GxConnection.Open, autoCommit=" +autoCommit + " handle:" + handle + " datastore:" + DataStore.Id);
			if (Preferences.Instrumented)
			{
				createTime = DateTime.Now;
			}

			multithreadSafe = m_dataRecord.MultiThreadSafe;
			BeforeConnect();
			bool retry = true;
			try
			{
				connection = m_dataRecord.GetConnection(checkShowPromp(), datasourceName, userId, userPassword, databaseName, port, currentSchema, data, connectionCache);
				connection.AutoCommit = autoCommit;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "GxConnection.Open Error ", ex);
				lastErrorCode = 3;
				lastErrorMsg = "Internal error: Function call failed (" + ex.Message + ")";
				throw ex;
			}

			while (retry)
			{
				try
				{
					retry = false;
					connection.Open();
					transaction = connection.Transaction;
                    m_opened = true;
                    AfterConnect();
				}
				catch(Exception ex)
				{
					GxADODataException e =new GxADODataException(ex);
					int status=0;
					if (!m_dataRecord.ProcessError(e.DBMSErrorCode, e.ErrorInfo, GxErrorMask.GX_NOMASK, this, ref status, ref retry, 0))
					{
						GXLogging.Error(log, "GxConnection.Open Error ",e);
						lastErrorCode = 3;
						lastErrorMsg = "Internal error: Function call failed ("+  e.ErrorInfo + ")";
						throw e;
					}
					if (status == 104)
					{
						connection.Close();
						retry = true;
					}
				}
			}
			if (Preferences.Instrumented)
			{
				wmiconnection = new WMIConnection(this);
			}
		}

		public void FlushBatchCursors(GXBaseObject obj)
		{
			ICollection adapters = ConnectionCache.GetDataAdapters();
			foreach (DbDataAdapterElem elem in adapters)
			{
				if (elem.DataTable.Rows.Count > 0 && (obj==null || elem.OnCommitEventInstance==obj))
				{
					elem.OnCommitEventInstance.GetType().GetMethod(elem.OnCommitEventMethod, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(
						elem.OnCommitEventInstance, null);
					elem.DataTable.Rows.Clear();
				}
			}
		}

		public void BeforeRollback()
		{
			ICollection adapters = ConnectionCache.GetDataAdapters();
			foreach (DbDataAdapterElem elem in adapters)
			{
				if (elem.DataTable.Rows.Count > 0)
				{
					elem.DataTable.Rows.Clear();
				}
			}
		}

        public IDbTransaction commitTransaction()
        {
            if (UncommitedChanges)
            {
                    if (transaction != null)
                    {
                        GXLogging.Debug(log, "Commit Transaction");
                        connection.CheckCursors(OpenHandles, true);
                        try
                        {
                            MonitorEnter();
                            transaction.Commit();
                        }
                        finally
                        {
                            MonitorExit();
                            UncommitedChanges = false;
                        }
                        connection.CheckCursors(OpenHandles, false);
                    }
                    if (connection != null && !autoCommit)
                    {
                       try
                        {
                            MonitorEnter();
                            transaction = connection.BeginTransaction(m_dataRecord.IsolationLevelTrn);
                        }
                        finally
                        {
                            MonitorExit();
                        }
                    }
            }
            return transaction;
        }

        public IDbTransaction rollbackTransaction()
        {
            if (uncommitedChanges)
            {
                if (transaction != null)
                {
                    GXLogging.Debug(log, "Rollback transaction, uncommitedcahnges, change state:" + UncommitedChanges + ", openHandles:" + OpenHandles);
                    connection.CheckStateBeforeRollback(OpenHandles);
                    connection.CheckCursors(OpenHandles, true);
                    try
                    {
                        MonitorEnter();
                        transaction.Rollback();
                    }
                    finally
                    {
                        MonitorExit();
                        UncommitedChanges = false;
                    }
                    connection.CheckCursors(OpenHandles, false);
                }
                if (connection != null && !autoCommit)
                {
                    GXLogging.Debug(log, "RollbackTransaction");
                    try
                    {
                        MonitorEnter();
                        transaction = connection.BeginTransaction(m_dataRecord.IsolationLevelTrn);
                    }
                    finally
                    {
                        MonitorExit();
                    }
                }
            }
            return transaction;
        }
		private void rollbackTransactionOnly()
		{
			if (transaction!=null)
			{
                connection.CheckStateBeforeRollback(OpenHandles);
                connection.CheckCursors(OpenHandles, true);
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex)
                {
					GXLogging.Warn(log, "rollbackTransactionOnly failed", ex);
                    
                }
                connection.CheckCursors(OpenHandles, false);
            }
		}

		public bool Cache
		{
			get{ return cache;}
			set{ cache=value;}
		}
		public short FullConnect()
		{
			try
			{
				Disconnect();
				
				Open();
				return 0;
			}
			catch(Exception ex)
			{
				GXLogging.Error(log, "FullConnect Error " + databaseName, ex);
				return 1;
			}
		}
		
		public void Close()
		{
			if (connection!=null) 
			{
				GXLogging.Debug(log, "GxConnection.Close Id " + " connection State '" + connection.State + "'" + " handle:" + handle + " datastore:" + DataStore.Id);
			}
			if (connection!=null && ((connection.State & ConnectionState.Closed) == 0 )) 
			{
				try
				{
					connectionCache.Clear();
				}
				catch(Exception e){
					GXLogging.Warn(log, "GxConnection.Close can't close all prepared cursors" ,e);
				}
				
				GXLogging.Debug(log, "UncommitedChanges before Close:" + UncommitedChanges );
                try
                {
                    if (UncommitedChanges)
                    {
                        rollbackTransactionOnly();
                        UncommitedChanges = false;
                    }
                }
                catch (Exception e)
                {
					GXLogging.Warn(log, "GxConnection.Close can't rollback transaction", e);
                }
                try
                {
                    connection.Close();
                    if (transaction != null)
                    {
                        transaction.Dispose();
                        transaction = null;
                    }

                }
                catch (Exception e)
                {
					GXLogging.Warn(log, "GxConnection.Close can't close connection", e);
                }
                spid = 0;
				GXLogging.Debug(log, "GxConnection.Close connection is closed " );
			}
			m_opened=false;
			if (Preferences.Instrumented  && wmiconnection!=null)
			{
				wmiconnection.CleanUp();
			}
		}

		public int OpenHandles
		{
			get{return openHandles;}
		}
		public void IncOpenHandles()
		{
            Interlocked.Increment(ref openHandles);			
			connection.CheckStateInc();
			if (Preferences.Instrumented)
			{
				RefreshTimeStamp();
			}
		}
		public void MonitorEnter()
		{
			if (!multithreadSafe)
			{
			Monitor.Enter(executingCursorSync);
			}
			if (log.IsDebugEnabled)
			{	
				executingCursors++;
			}
		}
		public void MonitorExit()
		{
			if (!multithreadSafe)
			{
			Monitor.Exit(executingCursorSync);
			}
			if (log.IsDebugEnabled)
			{
				executingCursors--;
			}
		}
        
		public void DecOpenHandles()
		{
 
            Interlocked.Decrement(ref openHandles);
            connection.CheckStateDec();
		}

		public short Disconnect()
		{
			Close();
			ConnectionString=null;
			return 0;
		}

		public  String ConnectionString
		{

			get { return m_dataRecord.ConnectionString;}
			set	{m_dataRecord.ConnectionString = value;}
		}
		public String UserId
		{
			get {
                if (!String.IsNullOrEmpty(InternalUserId))
				{
                    return InternalUserId;
				}
				else
				{
					string stmt = m_dataRecord.GetServerUserIdStmt();
					if (string.IsNullOrEmpty(stmt))
						return UserId;
					else
					{
						GxCommand cmd = new GxCommand(m_dataRecord, stmt, dataStore, 0, false, true, null);
						cmd.ErrorMask = GxErrorMask.GX_NOMASK | GxErrorMask.GX_MASKLOOPLOCK;
						IDataReader reader;
						cmd.FetchData(out reader);
                        string s = string.Empty;
                        if (reader != null)
                        {
                            s = reader.GetString(0);
                            reader.Close();
                        }
						return s;
					}
				}
			}
			set {userId =value;}
        }
        internal string InternalUserId
        {
            get {
                if (!String.IsNullOrEmpty(userId) && data.IndexOf("Integrated Security=yes") < 0)
                {
                    return userId;
                }
                else
                    return null;
            }
		}
		public string UserPassword
		{
			get { return userPassword;}
			set {userPassword = value;}
		}
		public string Port
		{
			get{ return port;}
			set{ port=value;}
		}        
        public string CurrentSchema
        {
            get { return currentSchema; }
            set { currentSchema = value; }
        }
        public void SetIsolationLevel( ushort level)
		{
			if (level==2)
			{
				m_dataRecord.IsolationLevelTrn = IsolationLevel.ReadCommitted;
				GXLogging.Debug(log, "Setting IsolationLevel : Read Commited" );
			}
            else if (level==3)
            {
                m_dataRecord.IsolationLevelTrn = IsolationLevel.Serializable;
                GXLogging.Debug(log, "Setting IsolationLevel : Serializable  " + ((int)m_dataRecord.IsolationLevelTrn));
            }
			else
			{
				m_dataRecord.IsolationLevelTrn = IsolationLevel.ReadUncommitted;
				GXLogging.Debug(log, "Setting IsolationLevel : Read UnCommited  " + ((int)m_dataRecord.IsolationLevelTrn) );
			}
		}

		public void SetTransactionalIntegrity( short trnInt)
		{
			if (trnInt==1 && !GxContext.isReorganization)
			{
				autoCommit=false;
			}
			else if (trnInt==0)
			{
				autoCommit=true;
			}

		}

        bool checkShowPromp()
		{
            switch(showPrompt)
            {
                case SHOW_PROMPT.ALWAYS: //The connection dialog is always shown.
				    return true;
                case SHOW_PROMPT.NEVER: //The dialog is never shown
			        return false;
                case SHOW_PROMPT.IF_REQUIRED: //The dialog is shown if required
				if (data != null)
				{
					
					bool res= ((userId==null || userId.Length==0) && data.IndexOf("Integrated Security=yes")<0);
					
					res= res || ((datasourceName==null || datasourceName.Length==0)  && data.IndexOf("SERVER")<0);
					return res;
				}
				else
				{
					return false;
				}
                default: return false;
			}
		}


		public String Database
		{
            get
            {
                if (connection != null && ((connection.State & ConnectionState.Closed) == 0))
                    return connection.Database;
                else if (string.IsNullOrEmpty(databaseName) && data != null)
                {
                    string databaseNameFromData = string.Empty;
                    int dbPos = data.ToLower().IndexOf("database=");
                    if (dbPos >= 0)
                    {
                        dbPos += 9;
                        int dbEnd = data.IndexOf(";", dbPos);
                        if (dbEnd >= 0)
                            databaseNameFromData = data.Substring(dbPos, data.Length - dbEnd);
                        else
                            databaseNameFromData = data.Substring(dbPos);
                        return databaseNameFromData;
                    }
                }
                return databaseName;
            }
			set
			{
                if (data != null)
                {
                    int dbPos = data.ToLower().IndexOf("database=");
                    if (dbPos >= 0)
                    {
                        dbPos += 9;
                        int dbEnd = data.IndexOf(";", dbPos);
                        if (dbEnd >= 0)
                            data = data.Substring(0, dbPos) + value + data.Substring(dbEnd, data.Length - dbEnd);
                        else
                            data = data.Substring(0, dbPos) + value;
                    }
                    else
                    {
                        databaseName = value;
                    }
                }
				
			}
		}

		public string DatabaseName
		{
			get
			{
				return m_dataRecord.DataBaseName;
			}
		}

		public GxAbstractConnectionWrapper InternalConnection
		{
			get { return connection; }
		}
		public int ConnectionTimeout
		{
			get {return timeout;}
		}
		public ConnectionState State
		{
			get {  if (connection!=null ) return connection.State; else return ConnectionState.Closed; }
		}
		public IDbTransaction BeginTransaction()
		{
			return transaction;
		}
		public IDbTransaction BeginTransaction(IsolationLevel il)
		{
			return transaction;
		}
		public void ChangeDatabase(string databaseName)
		{
		}
		public IDbCommand CreateCommand()
		{
			return  null;
		}
		public ushort ConnId
		{
			get {return 0;}
		}

		public int Handle
		{
			get {return handle;}
			set { handle=value;}
		}

		public string Data
		{
			get { return data;}
			set {data=value;}
		}
		public short Method
		{
			get { return 1;}	
		}
		public short ErrCode
		{
			get {return lastErrorCode;}
		}
		public string ErrDescription
		{
			get { return lastErrorMsg;}
		}
		public string DataSourceName
		{
			get { return datasourceName;}
			set {datasourceName= value;}
		}
		public string DriverName
		{
			get { return driverName;}
			set { driverName=value; }
		}
		public string FileDataSourceName
		{
			get { return datasourceName;	}
			set {datasourceName=value;}
		}
		public short ShowPrompt
		{
			get { return (short)showPrompt;	}
			set {showPrompt=(SHOW_PROMPT)value;}
		}
		public bool UncommitedChanges
		{
			get{ return uncommitedChanges;}
			set{ uncommitedChanges=value;}
		}
		public bool Available
		{
			get { return available; }
			set 
			{
				available=value; 
				connection.CheckState(value);
			}
		}

		public void RefreshTimeStamp()
		{
			timeStamp=DateTime.Now.Ticks;
		}
		public long TimeStamp
		{
			get {return  timeStamp;}
			set {}
		}
		public string Name 
		{
			get{return GxUserInfo.getProperty(handle,GxDefaultProps.USER_NAME);}
		}
		public string StartTime 
		{
			get{return GxUserInfo.getProperty(handle,GxDefaultProps.START_TIME);}
		}
		public string PgmName 
		{
			get{return GxUserInfo.getProperty(handle,GxDefaultProps.PGM_NAME);}
		}
		public int 	PreparedCursorsCount
		{
			get{ return connectionCache.CountPreparedStmt();}
		}
		public int 	PreparedCommandsCount
		{
			get{ return connectionCache.CountPreparedCommand();}
		}

		public DateTime ServerDateTimeMs
		{
			get
			{
				return ServerDateTimeEx(true);
			}
		}

		public DateTime ServerDateTime
		{
			get
			{
				return ServerDateTimeEx(false);
			}
		}
		public DateTime ServerDateTimeEx(bool hasMiliseconds)
		{		
			string stmt = m_dataRecord.GetServerDateTimeStmt(this);
			if (string.IsNullOrEmpty(stmt))
			{
				return DateTime.Now;
			}
			else
			{
				GxCommand cmd = new GxCommand(m_dataRecord, stmt, dataStore, 0, false, true, null);
				cmd.ErrorMask = GxErrorMask.GX_NOMASK | GxErrorMask.GX_MASKLOOPLOCK;
				IDataReader reader;
				cmd.FetchData(out reader);
				DateTime d = DateTimeUtil.NullDate();
				if (reader != null)
				{
					d = reader.GetDateTime(0);
					if (!hasMiliseconds)
						d = DateTimeUtil.ResetMilliseconds(d);
					reader.Close();
				}
				return d;
			}
		}
        public string ServerVersion
        {
            get
            {
				string stmt = m_dataRecord.GetServerVersionStmt();
				GxCommand cmd = new GxCommand(m_dataRecord, stmt, dataStore, 0, false, true, null);
				cmd.ErrorMask = GxErrorMask.GX_NOMASK | GxErrorMask.GX_MASKLOOPLOCK;
				IDataReader reader;
				cmd.FetchData(out reader);
                string s = string.Empty;
                if (reader != null)
                {
                    int index = reader.IsDBNull(0) ? 1 : 0; 
                    s = reader.GetString(index);
                    if (m_dataRecord is GxSqlServer)
                        s = s.Replace("10.", "9.");
                    reader.Close();
                }
                return s;
            }
        }
        public string DSVersion 
        { 
            get; set; 
        }
		public string BlobPath
		{
			get {return blobPath;}
			set {blobPath=value;}
		}
		public string MultimediaPath
		{
			get { return Path.Combine(BlobPath, GXDbFile.MultimediaDirectory); }
		}
#region WMI Members

		public string PhysicalId
		{
			get
			{
				return spid.ToString();
			}
		}

		public DateTime CreateTime
		{
			get
			{
				return createTime;
			}
		}

		public DateTime LastAssignedTime
		{
			get
			{
				return  new DateTime(TimeStamp);
			}
		}

		public int LastUserAssigned
		{
			get
			{
				return Handle;
			}
		}

		public bool Error
		{
			get
			{
				return (lastErrorCode!=0);
			}
		}

		public bool AvailableWMI
		{
			get
			{
				return (available && !UncommitedChanges && openHandles==0);
			}
		}

		public int OpenCursorCount
		{
			get
			{
				return OpenHandles;
			}
		}

		public int RequestCount
		{
			get
			{
				return 0;
			}
		}

		public DateTime LastSQLStatementTime
		{
			get
			{
				return startTimeCurrentStmt;
			}
		}

		public string LastSQLStatement
		{
			get
			{
                return lastSQLStatement;
			}
		}

		public string LastObject
		{
			get
			{
				return lastObject;
			}
			set
			{
				lastObject = value;
			}
		}

		public bool LastSQLStatementEnded
		{
			get
			{
				return lastSqlStatementEnded;
			}
			set
			{
				lastSqlStatementEnded=value;
			}
		}

		public void DumpConnectionInformation(GXXMLWriter writer)
		{
			writer.WriteStartElement("Connection_Information");
			writer.WriteAttribute("Id", Handle.ToString());
			writer.WriteElement("PhysicalId", PhysicalId);
			writer.WriteElement("CreateTime", CreateTime.ToString());
			writer.WriteElement("LastAssignedTime", LastAssignedTime.ToString());
			writer.WriteElement("LastUserAssigned", LastUserAssigned);
			writer.WriteElement("Error", Error.ToString());
			writer.WriteElement("Available", AvailableWMI.ToString());
			writer.WriteElement("OpenCursorCount", OpenCursorCount);
			writer.WriteElement("UncommitedChanges", UncommitedChanges.ToString());
			writer.WriteElement("RequestCount", RequestCount);
			writer.WriteStartElement("LastSQLStatement");
			writer.WriteCData(LastSQLStatement);
			writer.WriteEndElement();
			writer.WriteElement("LastSQLStatementTime", LastSQLStatementTime.ToString());
			writer.WriteElement("LastSQLStatementEnded", LastSQLStatementEnded.ToString());
			writer.WriteElement("LastObject", LastObject);
			writer.WriteEndElement();
		}
#endregion
	}
	public class ParDef
	{
		public string Name { get; set; }
		public string Tbl { get; set; }
		public string Fld { get; set; }
		public GXType GxType { get; set; }
		public int Size { get; set; }
		public int Scale { get; set; }
		public int ImgIdx { get; set; }
		public bool Nullable { get; set; }
		public bool ChkEmpty { get; set; }
		public bool Return { get; set; }
		public bool InDB { get; set; }
		public bool AddAtt { get; set; }
		public bool Preload { get; set; }
		public bool InOut { get; set; }
		public bool Out { get; set; }
		public ParDef(string name, GXType type, int size, int scale)
		{
			Name = name;
			GxType = type;
			Size = size;
			Scale = scale;
		}
	}
	public class GxCommand: IGxDbCommand
	{
		internal List<ParDef> ParmDefinition;
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.ADO.GxCommand));
		string stmt;
        String stmtId;
        GxParameterCollection parameters;
		ushort fetchSize=256;
		int timeOut;
		IGxDataStore dataStore;
		GxErrorMask errMask;
		bool hasMoreRows;
		int status;
		IGxDataRecord dataRecord;
        List<object> errorRecords;
        DbDataAdapterElem da;
        int errorRecordIndex;
		int blockSize;
		bool hasnext;
		GxArrayList block;
		int handle;
		IGxConnection con;
		int timeToLive;
		SlidingTime expiration;
		short TimeStatic=-1;
		bool withCached;
		bool hasNested;
		bool isForFirst;
		bool dynamicStmt;
		private IDataReader idatareader;
		private CommandType commandType=CommandType.Text;
		GxErrorHandler _errorHandler;
		private object syncCommand = new Object();	
		private bool isCursor;

		public bool IsCursor
		{	
			get{ return isCursor;}
			set{ isCursor=value;}

		}

		public GxCommand( IGxDataRecord db, String statement, IGxDataStore ds, int ttl, 
			bool hasNestedCursor, bool forFirst, GxErrorHandler errorHandler)
		{

			dataRecord = db;
			parameters = new GxParameterCollection();
			hasnext=true;
			handle=ds.Handle;
			timeToLive=GetTimeToLive(ttl);
			hasNested=hasNestedCursor;
			_errorHandler = errorHandler;
			ParmDefinition = new List<ParDef>();
			try
			{
				dataStore = ds;
				con=(GxConnection) ds.Connection;
				withCached=	(timeToLive!=0) && con.Cache;
				isForFirst=forFirst;
				if (withCached && timeToLive!= TimeStatic)
				{
					expiration=new SlidingTime(new TimeSpan(TimeSpan.TicksPerMinute * timeToLive));
				}
				stmt=statement;
			}
			catch (GxADODataException e)
			{ 
			
				bool retry=false;
				if (! dataRecord.ProcessError( e.DBMSErrorCode, e.ErrorInfo, errMask, con, ref status, ref retry, 0))
				{
					GXLogging.Error(log, "Return Error GxCommand ", e);
					throw (new GxADODataException(e.ErrorInfo, e));
				}
			}
		}

		private int GetTimeToLive(int ttl)
		{
			if (CacheFactory.ForceHighestTimetoLive)
				return -1;
			else
				return ttl;
		}

		public GxCommand( IGxDataRecord db, String statement, short updatable, 
			IGxDataStore ds, string objName, string stmtId, int ttl, bool hasNested,bool isForFirst, GxErrorHandler errorHandler):this(db, statement,ds, ttl,hasNested, isForFirst, errorHandler)
		{
            this.stmtId = stmtId;
		}
        public GxCommand(IGxDataRecord db, String statement, short updatable,
            IGxDataStore ds, string objName, string stmtId, int ttl, bool hasNested, bool isForFirst, GxErrorHandler errorHandler, int batchSize)
            : this(db, statement, updatable, ds, objName, stmtId, ttl, hasNested, isForFirst, errorHandler)
        {
            if (batchSize > 0)
            {
                this.da = dataRecord.GetDataAdapter(con, stmt, batchSize, stmtId);
            }
        }

		public void rollbackSavePoint()
		{
			IDbCommand cmd = dataRecord.GetCommand(con, "ROLLBACK TO SAVEPOINT gxupdate", new GxParameterCollection());
			cmd.ExecuteNonQuery();
		}
		public bool DynamicStmt
		{
			get{ return dynamicStmt;}
			set{ dynamicStmt=value;}
		}

		public int Handle
		{
			get 
			{
				GXLogging.Debug(log, "Get handle, handle '" + handle + "'");
				return handle;
			}
			set 
			{
				handle=value;
				GXLogging.Debug(log, "Set handle, handle '" + value + "'");
			}
		}

		private static CacheItem GetBlockFromCache(string key)
		{
			GXLogging.Debug(log, "GetBlockFromCache, key: '"+ key + "'");

			CacheItem item;
			if (CacheFactory.Instance.Get<CacheItem>(CacheFactory.CACHE_DB, key, out item) && item.OriginalKey == key)
				return item;
			else
				return null;
		}

		public void Close()
		{
			if (idatareader!=null)
			{
				idatareader.Close(); 
			}
            else if (da != null)
            {
                da.Clear();
            }
		}
		internal string ParametersString()
		{
			string parms = "";
			if (parameters != null && parameters.Count > 0)
			{
				int count = parameters.Count;
				for (int j = 0; j < count; j++)
				{
					parms += parameters[j].ParameterName;
					if (parameters[j].Value != null && parameters[j].Value is DateTime)
					{
						parms += "='" + ((DateTime)parameters[j].Value) + ":" +
							((DateTime)parameters[j].Value).Millisecond + "' ";
					}
					else
					{
						parms += "='" + parameters[j].Value + "' ";
					}
				}
			}
			return parms;
		}

		public int ExecuteNonQuery()
		{
			try
			{
				GXLogging.Debug(log, "Start GxCommand.ExecuteNonQuery: Parameters ", ParametersString);
				
				con = GxConnectionManager.Instance.SetAvailable(handle, dataRecord.DataSource, false);
				con.CurrentStmt=stmt;
				con.MonitorEnter();
				dataRecord.SetCursorDef(CursorDef);
				IDbCommand cmd =dataRecord.GetCommand(con,stmt,parameters, isCursor, false, false);
				cmd.CommandType=commandType;
				int res=cmd.ExecuteNonQuery();
				con.MonitorExit();

				GXLogging.Debug(log, "Return GxCommand.ExecuteNonQuery");
				return res;
			}
			catch(Exception e)
			{
				if ((errMask & GxErrorMask.GX_ROLLBACKSAVEPOINT) > 0)
				{
                    try
                    {
                        rollbackSavePoint();
                    }
                    catch { }
				}
				GXLogging.Error(log, e, "Return GxCommand.ExecuteNonQuery Error "); 
				try
				{
				con.MonitorExit();
				}
				catch{}
				throw (new GxADODataException(e));
			}

		}

		private CacheItem ExecStmtFetch1()
		{
            IDataReader reader = null;
            con = GxConnectionManager.Instance.IncOpenHandles(handle, dataRecord.DataSource);
			con.CurrentStmt = stmt;
			block = new GxArrayList(1);
			con.MonitorEnter();
			try
			{
                reader = dataRecord.GetCommand(con, stmt, parameters, isCursor, true, false).ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read())
				{
					object[] values = new object[reader.FieldCount];
					dataRecord.GetValues(reader, ref values);
					block.Add(values);
					blockSize = 1;
				}
				else
				{
					blockSize = 0;
				}
				return new CacheItem(block, false, blockSize, 0);
			}
			finally
			{
				if (reader != null && !reader.IsClosed)
				{
					reader.Close();
					reader.Dispose();
				}
				GxConnectionManager.Instance.DecOpenHandles(handle, dataRecord.DataSource);
				con.MonitorExit();
				con.CurrentStmt = null;
			}
		}

		internal string ParametersTypeString()
		{
			string parms = "";
			if (parameters != null && parameters.Count > 0)
			{
				for (int j = 0; j < parameters.Count; j++)
				{
					parms += parameters[j].ParameterName + "='" + parameters[j].Value + "' ";
					if (parameters[j].Value != null) parms += "type:" + parameters[j].Value.GetType() + " ";
				}
			}
			return parms;
		}
		private IDataReader ExecRpc()
		{
			try
			{
				GXLogging.Debug(log, "ExecuteReader: Parameters ", ParametersTypeString);

				con = GxConnectionManager.Instance.IncOpenHandles(handle, dataRecord.DataSource);
				con.MonitorEnter();
				con.CurrentStmt=stmt;
				block=new GxArrayList(1);
				GXLogging.Debug(log, "ExecRpc, SqlConnection ");

				IDbCommand cmd= dataRecord.GetCommand(con,stmt,parameters, isCursor, false, true);
				cmd.CommandType=CommandType.StoredProcedure;
				object[] values = dataRecord.ExecuteStoredProcedure(cmd);

				if (values!=null)
				{
					block.Add(values);
					blockSize=1;
				}
				else
				{
					blockSize=0;
				}

				GxConnectionManager.Instance.DecOpenHandles(handle, dataRecord.DataSource);

				con.CurrentStmt=null;
				GXLogging.Debug(log, "Return GxCommand.ExecRpc, Parameters: Stmt ", stmt);

				idatareader=  dataRecord.GetCacheDataReader(new CacheItem(block, false, blockSize, 0), false, null);
				return idatareader;
			}
			catch(Exception e)
			{
				GXLogging.Error(log, e, "Return GxCommand.ExecRpc Error "); 
				throw (new GxADODataException(e));
			}
			finally
			{
				try{
				con.MonitorExit();
				}
				catch{}

			}

		}

		public string ExecuteDataSet()
		{
			bool retry = true;
			int retryCount=0;
			while (retry)
			{
				try	
				{
					retry = false;
					
					try
					{
						GXLogging.Debug(log, "Start GxCommand.ExecuteDataSet");
						con = GxConnectionManager.Instance.SetAvailable(handle, dataRecord.DataSource, false);
						con.CurrentStmt=stmt;
						DataSet ds=new DataSet();
						DbDataAdapterElem da=dataRecord.GetDataAdapter(con,stmt,parameters);
                        da.Adapter.SelectCommand = (DbCommand) dataRecord.GetCommand(con, stmt, new GxParameterCollection());
                        da.Adapter.Fill(ds);
						con = GxConnectionManager.Instance.SetAvailable(handle, dataRecord.DataSource, true);
						status=0;
						return ds.GetXml();
					}
					catch(Exception e)
					{
						GXLogging.Error(log, e, "Return GxCommand.ExecuteDataSet Error "); 
						throw (new GxADODataException(e));
					}
				}
				catch(GxADODataException e) 
				{
					bool pe = dataRecord.ProcessError( e.DBMSErrorCode, e.ErrorInfo, errMask, con, ref  status, ref retry, retryCount);
					retryCount++;
					processErrorHandler( status, e.DBMSErrorCode, e.SqlState, e.ErrorInfo, errMask, "EXECUTE", ref pe, ref retry);
					if (! pe)
					{
						GXLogging.Error(log, e, "GxCommand.ExecuteDataSet Error ");
						throw (new GxADODataException(e.ToString(), e));
					}
				}
			}
			return "";
		}

		public IDataReader ExecuteReader()
		{
			try
			{
				GXLogging.Debug(log,  "ExecuteReader: Parameters ", ParametersString);

				con.CurrentStmt = null;
				if (withCached)
				{
					CacheItem cacheItem = GetBlockFromCache(SqlUtil.GetKeyStmtValues(parameters, stmt, isForFirst));
					if (cacheItem != null) //Cached
					{
						GXLogging.Debug(log, "ExecuteReader: Cached, handle '", handle.ToString(), "'");

						idatareader = dataRecord.GetCacheDataReader(cacheItem, false, null);
						return idatareader;
					}
				}
				dataRecord.SetCursorDef(CursorDef);
				if (isForFirst)//Client Cursor
				{
					CacheItem data;
					data = ExecStmtFetch1();
					string key = null;
					if (withCached)
					{
						key = SqlUtil.GetKeyStmtValues(parameters, stmt, true);
						SqlUtil.AddBlockToCache(key, data, con, timeToLive);
					}
					idatareader = dataRecord.GetCacheDataReader(data, withCached, key);
					return idatareader;
				}

				//Client o Server Cursor
				idatareader = dataRecord.GetDataReader(GxConnectionManager.Instance, con, parameters, stmt, fetchSize, isForFirst, handle, (withCached && timeToLive != 0), expiration, hasNested, dynamicStmt);

				return idatareader;
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Return GxCommand.ExecuteReader Error ", e);

				throw (new GxADODataException(e));
			}

		}

		public void FetchData(out IDataReader dr)
		{
			bool retry = true;
			int retryCount = 0;
			dr = null;
			while (retry)
			{
				try
				{
					retry = false;
					lock (syncCommand)  
					{

						dr = ExecuteReader();
						hasMoreRows=dr.Read();
					}
				}
				catch (GxADODataException e)
				{ 
					status=0;
					bool pe = dataRecord.ProcessError( e.DBMSErrorCode, e.ErrorInfo, errMask, con, ref status, ref retry, retryCount);
					retryCount++;
					processErrorHandler( status, e.DBMSErrorCode, e.SqlState, e.ErrorInfo, errMask, "FETCH", ref pe, ref retry);
					if (! pe)
					{
						GXLogging.Error(log, e, "GxCommand.FetchData Error ");
						try
						{
							Close();
							con.Close();
						}
						catch(Exception ex)
						{
							GXLogging.Error(log, ex, "GxCommand.FetchData-Close Error ");
						}
						throw (new GxADODataException(e.ToString(), e));
					}
				}
			}
		}

		public void FetchDataRPC(out IDataReader dr)
		{
			bool retry = true;
			int retryCount=0;
			dr = null;
			while (retry)
			{
				try
				{
					retry = false;
					dr = ExecRpc();
					hasMoreRows=dr.Read();
				}
				catch (GxADODataException e)
				{ 
					status=0;
					bool pe = dataRecord.ProcessError( e.DBMSErrorCode, e.ErrorInfo, errMask, con, ref status, ref retry, retryCount);
					retryCount++;
					processErrorHandler( status, e.DBMSErrorCode, e.SqlState, e.ErrorInfo, errMask, "FETCH", ref pe, ref retry);
					if (! pe)
					{
						GXLogging.Error(log, e, "GxCommand.FetchDataRPC Error ");
						throw (new GxADODataException(e.ToString(), e));
					}
				}
			}
		}

		public void CreateDataBase()
		{
			dataRecord.CreateDataBase(stmt, con);
		}
#if !NETCORE
		internal string DatatableParameters()
		{
				string parms = "";
				if (da.DataTable.Rows != null && da.DataTable.Rows.Count > 0)
				{
					int count = da.DataTable.Rows.Count;
					for (int j = 0; j < count; j++)
					{
						for (int h = 0; h < da.DataTable.Columns.Count; h++)
							parms += " " + da.DataTable.Rows[j][h];
					}
				}
				return parms; 
		}
#endif
		public int ExecuteBatchQuery()
        {
            try
            {
                int res = 0;
                if (da != null)
                {
#if !NETCORE
					GXLogging.Debug(log, "Start GxCommand.ExecuteBatchQuery: Parameters ", DatatableParameters);
#endif
					con = GxConnectionManager.Instance.SetAvailable(handle, dataRecord.DataSource, false);
                    con.CurrentStmt = stmt;
                    con.MonitorEnter();
					con.InternalConnection.SetSavePoint(Transaction, stmtId);
					dataRecord.SetAdapterInsertCommand(da, con, stmt, parameters);
#if !NETCORE
					da.Adapter.ContinueUpdateOnError = false;
#endif
                    res = dataRecord.BatchUpdate(da);

					con.InternalConnection.ReleaseSavePoint(Transaction, stmtId);

                    con.MonitorExit();

					GXLogging.Debug(log, "Return GxCommand.ExecuteNonQuery");
                }
                return res;
            }
            catch (Exception e)
            {
				GXLogging.Error(log, "Return GxCommand.ExecuteNonQuery Error ", e);
				con.InternalConnection.RollbackSavePoint(Transaction, stmtId);
				con.MonitorExit();
                throw (new GxADODataException(e));
            }
            finally
            {
                if (da!=null)
                {

					errorRecords = new List<object>(da.BlockValues.Count);
					foreach (object item in da.BlockValues)
					{
						errorRecords.Add(item);
					}
                    errorRecordIndex = -1;
                }
            }
        }

        public int ReadNextErrorRecord()
        {
            if (errorRecords != null && errorRecordIndex < errorRecords.Count - 1)
            {
                errorRecordIndex++;
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public object ErrorRecord(int idx)
        {
            if (errorRecords != null && idx < ((Object[])errorRecords[errorRecordIndex]).Length)
            {
                return ((Object[])errorRecords[errorRecordIndex])[idx];
            }
            else
            {
                throw new GxADODataException("Could not find error record (" + errorRecordIndex + "," + idx + ")");
            }

        }
        public void ExecuteBatch()
        {
            bool retry = true;
            int retryCount = 0;
            while (retry)
            {
                try
                {
                    retry = false;
                    lock (syncCommand)
                    {
                        ExecuteBatchQuery();
                    }
                    status = 0;
                }
                catch (GxADODataException e)
                {
                    bool pe = dataRecord.ProcessError(e.DBMSErrorCode, e.ErrorInfo, errMask, con, ref  status, ref retry, retryCount);
                    retryCount++;
                    processErrorHandler(status, e.DBMSErrorCode, e.SqlState, e.ErrorInfo, errMask, "EXECUTE", ref pe, ref retry);
                    if (!pe)
                    {
                        GXLogging.Error(log, "GxCommand.ExecuteStmt Error ", e);
						throw (new GxADODataException(e.ToString(), e));
                    }
                }
            }
        }
        private void execStmt()
        {
            bool retry = true;
            int retryCount = 0;

            while (retry)
            {
                try
                {
                    retry = false;
                    lock (syncCommand)
                    {
                        int res = ExecuteNonQuery();
						hasMoreRows = res > 0;
                    }
                    status = 0;
                }
                catch (GxADODataException e)
                {
                    bool pe = dataRecord.ProcessError(e.DBMSErrorCode, e.ErrorInfo, errMask, con, ref  status, ref retry, retryCount);
                    retryCount++;
                    processErrorHandler(status, e.DBMSErrorCode, e.SqlState, e.ErrorInfo, errMask, "EXECUTE", ref pe, ref retry);
                    if (!pe)
                    {
                        GXLogging.Error(log, "GxCommand.ExecuteStmt Error ", e);
						throw (new GxADODataException(e.ToString(), e));
                    }
                }
            }

        }
        public void ExecuteStmt()
        {
            if (GxContext.isReorganization)
            {
                if (!GXReorganization.ExecutedBefore(stmt))
                {
                    execStmt();
                    GXReorganization.AddExecutedStatement(stmt);
                }
            }
            else
            {
                execStmt();
            }
			Conn.UncommitedChanges = true;
		}

		public void Drop()
		{
			this.Close();
		}
		public void Cancel()
		{
		}
		public void Prepare()
		{
		}
		public IGxDataRecord Db
		{
			get	{ return dataRecord; }
			set	{dataRecord = value; }
		}
		public void Dispose()
		{ 
			Close();
			if (parameters!=null)
			{
				parameters.Clear();
			}
		}

		public string CommandText 
		{
			get { return stmt;} 
			set { stmt = value;}
		}

		public int CommandTimeout 
		{	
			get { return timeOut;}
			set { timeOut = value;}
		}

		public CommandType CommandType 
		{	
			get { return commandType;} 
			set {commandType=value;}
		}

		public IDbDataParameter CreateParameter()
		{
			IDbDataParameter newParam = dataRecord.CreateParameter();
			parameters.Add( newParam);
			return newParam;
		}
        public void OnCommitEvent(object instance, string method)
        {
            if (da != null)
            {
                da.OnCommitEventInstance = instance;
                da.OnCommitEventMethod = method;
            }
        }
        public void AddRecord(Object[] execParms)
        {
            if (da != null)
            {
				int parameterCount = parameters.Count;
				Object[] parms = new Object[parameterCount];
				
				for (int i = 0; i < parameterCount; i++)
				{
					parms[i] = parameters[i].Value;
				}
                if (!da.Initialized)
                {
                    for (int i = 0; i < parameterCount; i++)
                    {
                        if (parms[i].GetType() != typeof(DBNull))
                            da.DataTable.Columns.Add(parameters[i].ParameterName, parms[i].GetType());
                        else
							da.DataTable.Columns.Add(parameters[i].ParameterName, SqlUtil.DbTypeToType(parameters[i].DbType));
                    }
                    da.Initialized = true;
                }
                da.BlockValues.Add(parms);
                da.DataTable.Rows.Add(parms);
            }
        }

		private string OriginalCmd = null;
		public void SetParameterRT(string name, string value)
		{
			string op = value;
			bool isLike = false;
			switch (value)
			{
				case "=": break;
				case ">": break;
				case ">=": break;
				case "<": break;
				case "<=": break;
				case "<>": break;
				case "like": isLike = true;  break;
				default: op = "="; break;
			}
			OriginalCmd = OriginalCmd ?? CommandText;

			string[] parts = CommandText.Split('\'');
			string placeholder = $"{{{{{ name }}}}}";
			for (int i = 0; i < parts.Length; i+=2)
			{
				if (isLike)
					while (ProcessLike(parts, i, placeholder, op)) { }
				else parts[i] = parts[i].Replace(placeholder, op);
			}
			CommandText = string.Join("'", parts);
		}

		private bool ProcessLike(string [] parts, int i, string placeholder, string op)
		{
			
			int placeholderIdx = parts[i].IndexOf(placeholder);
			if (placeholderIdx == -1)
				return false;
			int parenCount = 0;
			int idx = placeholderIdx + placeholder.Length;
			int j = i;
			bool hasVariable = false;
			string currentPart = parts[j];
			while(parenCount >= 0)
			{
				if(idx == placeholder.Length)
				{
					j += 2;
					idx = 0;
					if (j < parts.Length)
						currentPart = parts[j];
					else break;
					continue;
				}
				switch(currentPart[idx])
				{
					case '(': parenCount++; break;
					case ')': parenCount--; break;
					case '@':
					case ':':
					case '?':
						hasVariable = true; break;
				}
				idx++;
			}
			if(hasVariable && j < parts.Length)
			{
				idx--;
				parts[j] = $"{ parts[j].Substring(0, idx) }){ Db.ConcatOp(1) }'%'{ Db.ConcatOp(2) }{ parts[j].Substring(idx) }";
				op += $" { Db.ConcatOp(0) }RTRIM(";
			}

			parts[i] = $"{ parts[i].Substring(0, placeholderIdx) }{ op }{ parts[i].Substring(placeholderIdx + placeholder.Length) }";
			return true;
		}

		public void RestoreParametersRT()
		{
			if (OriginalCmd != null)
				CommandText = OriginalCmd;
		}

		public void SetParameter( int num, Object value)
		{
			if (parameters!=null && parameters.Count>num)
			{
				dataRecord.SetParameter(parameters[num],value);
			}
			else
			{
				GXLogging.Error(log, "GxCommand.SetParameter Error num= " + num + ", size=" + (parameters!=null ? parameters.Count : 0));
				throw (new GxADODataException("Index was out of range: (num=)" + num + ", range=" + (parameters!=null ? parameters.Count : 0)));

			}
		}
		public void SetParameterVChar( int num, string value)
		{
			if (parameters!=null && parameters.Count>num)
			{
				dataRecord.SetParameterVChar(parameters[num],value);
			}
			else
			{
				GXLogging.Error(log, "GxCommand.SetParameterVChar Error num= " + num + ", size=" + (parameters!=null ? parameters.Count : 0));
				throw (new GxADODataException("Index was out of range: (num=)" + num + ", range=" + (parameters!=null ? parameters.Count : 0)));

			}
		}
		public void SetParameterChar( int num, string value)
		{
			if (parameters!=null && parameters.Count>num)
			{
				dataRecord.SetParameterChar(parameters[num],value);
			}
			else
			{
				GXLogging.Error(log, "GxCommand.SetParameterChar Error num= " + num + ", size=" + (parameters!=null ? parameters.Count : 0));
				throw (new GxADODataException("Index was out of range: (num=)" + num + ", range=" + (parameters!=null ? parameters.Count : 0)));

			}
		}

		public void SetParameterLVChar( int num, string value)
		{
			if (parameters!=null && parameters.Count>num)
			{
				
				dataRecord.SetParameterLVChar(parameters[num],value, dataStore);
			}
			else
			{
				GXLogging.Error(log, "GxCommand.SetParameterLVChar Error num= " + num + ", size=" + (parameters!=null ? parameters.Count : 0));
				throw (new GxADODataException("Index was out of range: (num=)" + num + ", range=" + (parameters!=null ? parameters.Count : 0)));

			}
		}

		public void SetParameterBlob(int num, string value, bool dbBlob)
		{
			if (parameters != null && parameters.Count > num)
			{

				dataRecord.SetParameterBlob(parameters[num], value, dbBlob);
			}
			else
			{
				GXLogging.Error(log, "GxCommand.SetParameterBlob Error num= " + num + ", size=" + (parameters != null ? parameters.Count : 0));
				throw (new GxADODataException("Index was out of range: (num=)" + num + ", range=" + (parameters != null ? parameters.Count : 0)));

			}
		}

		public void SetParameterDir(int num, ParameterDirection dir)
		{
			if (parameters!=null && parameters.Count>num)
			{
				dataRecord.SetParameterDir(parameters, num, dir);
			}
			else
			{
				GXLogging.Error(log, "GxCommand.SetParameterDir Error num= " + num + ", size=" + (parameters!=null ? parameters.Count : 0));
				throw (new GxADODataException("Index was out of range: (num=)" + num + ", range=" + (parameters!=null ? parameters.Count : 0)));

			}
		}
        public void ClearParameters()
        {
            if (parameters.Count>0)
                parameters.Clear();
			if (ParmDefinition.Count > 0)
				ParmDefinition.Clear();
        }
		public void AddParameter( string name, Object dbtype, int gxlength, int gxdec)
		{
			
			IDbDataParameter parm = dataRecord.CreateParameter(name, dbtype, gxlength, gxdec);
			parm.SourceColumn = parm.ParameterName;
			parameters.Add( parm);
		}
		public void AddParameter( string name, object value)
		{
			IDbDataParameter parm = CreateParameter();
			parm.ParameterName=name;
			parm.Value=value;
			if (value!=null && value is string)
			{
				
				parm.Size = 1024;
			}
		}

		public IDataReader ExecuteQuery()
		{
			throw (new GxNotImplementedException());
		}

		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			throw (new GxNotImplementedException());
		}

		public object ExecuteScalar()
		{
			throw (new GxNotImplementedException());
		}

		public IDataParameterCollection Parameters 
		{
			get {return parameters;}
		}
		public IDbTransaction Transaction 
		{
			get { return dataStore.BeginTransaction();} 
			set { }
		}
		public UpdateRowSource UpdatedRowSource 
		{
			get { return UpdateRowSource.None;}
			set {}
		}

		public IDbConnection Connection 
		{
			get { return dataStore.Connection.InternalConnection;} 
			set { }
		}

		public ushort FetchSize 
		{
			get { return fetchSize; }
			set 
			{
				fetchSize = value;
				GXLogging.Debug(log, "GxCommand.FetchSize: " + value  +", handle '" + handle  +"'");
			}
		}
		public Boolean NoError
		{
			get { return status==0;  }
		}
		public bool HasMoreRows
		{
			get { return hasMoreRows; }
			set	{ hasMoreRows = value; }
		}
		public int Status
		{
			get {return status;}
		}
        public int BatchSize
		{
            get
            {
                if (da != null) return da.UpdateBatchSize;
                else return 0;
            }
		}
        public int RecordCount
        {
            get
            {
                if (da != null && da.DataTable.Rows != null)
                    return da.DataTable.Rows.Count;
                else return 0;
            }
        }
        public int BlockSize
		{
			get { return blockSize; }
		}
		public bool HasNext
		{
			get { return hasnext; }
		}

		public GxErrorMask ErrorMask
		{
			get { return errMask; }
			set	{ errMask = value; }
		}

		public IGxConnection Conn
		{
			get {return con;}
		}

		public CursorDef CursorDef { get; internal set; }

		public void processErrorHandler( int status, int errorCode, string gxSqlState, string errorInfo, GxErrorMask errMask, string operation, ref bool pe, ref bool retry)
		{
			if (_errorHandler != null)
			{
				int ehStatus = _errorHandler.Execute(status, errorCode, errorInfo, operation, this.stmt, gxSqlState);
				if (ehStatus == 0)			// Igonre
				{
					pe = true;
					retry = false;
                    if (status == 104)//Network error => new connection and retry
                    {
                        retry = true;
                    }
                }
                else if (ehStatus == 1)		// Retry
				{
					pe = true;
					retry = true;
				}
				else if (ehStatus == 2)		// Cancel
				{
					pe = false;
					retry = false;
				}
                GXLogging.Warn(log, "processErrorHandler status " + status + " errorCode:" + errorCode + " errorInfo:" + errorInfo + " retry:" + retry + " pe:" + pe);
                if (con != null && con.DataStore != null) GXLogging.Debug(log, "processErrorHandler datastore:" + con.DataStore.Id);
            }
            
            if ( ! pe )
			{
				string showError;
                if (Config.GetValueOf("ReportAccessError", out showError) && !GxContext.isReorganization)
				{
#if !NETCORE
					if (showError == "MessageBox")
					{
						try
						{
							if (status == 999)//Unexpected error, display message and abort application
							{
								GXUtil.WinMessage(errorInfo, "Data Access Error");
							}
							else //known error
							{
								if ((errMask & GxErrorMask.GX_MASKLOOPLOCK) > 0)
								{
									GXUtil.WinMessage(errorInfo, "Data Access Error");
								}
								else
								{
									pe = true; 
								}
							}
						}
						catch (Exception)
						{
							GXLogging.Warn(log, "Exit application failed on " + errorInfo);
						}
					}
#endif
				}
			}
		}

		internal void DelDupPars()
        {
            if (!dataRecord.AllowsDuplicateParameters)
            {
				parameters = parameters.Distinct();
            }
        }

		internal void AfterCreateCommand()
		{
			stmt = dataRecord.AfterCreateCommand(stmt, parameters);
		}
	}
	
	public class GxDataStore : IGxDataStore
	{
        string id;
		IGxConnection connection;
		int handle;		
		GxDataRecord datarecord;
		IGxContext context;
        GxSmartCacheProvider smartCacheProvider;
		WMIDataSource wmidatasource;
        public GxDataStore()
		{
			id = "";
		}
		public GxDataStore( string id) : this( null, id, null, "")
		{
		}
		public GxDataStore( IGxDataRecord db, string id) : this( db, id, null, "")
		{
		}
		public OlsonTimeZone ClientTimeZone
		{
			get {
				return context.GetOlsonTimeZone();
			}		
		}
        public GxSmartCacheProvider SmartCacheProvider
        {
            get
            {
                if (smartCacheProvider == null)
                    smartCacheProvider = new GxSmartCacheProvider();
                return smartCacheProvider;
            }

        }

#region YiConstructors
		[Obsolete("GxDataStore ctr of 2 parameteres is deprecated. Yi Remoting", false)]
		public GxDataStore( string id, IGxContext context) : this( null, id, context, "")
		{
		}
		public GxDataStore( IGxDataRecord db, string id, IGxContext context) : this( db, id, context, "")
		{
			
		}
		public GxDataStore( IGxDataRecord db, string id, IGxContext context, string connectionString )
		{
			Initialize(db, id, -1, context, connectionString);
		}

#endregion

#region OlimarConstructors
		[Obsolete("GxDataStore ctr of 2 parameteres is deprecated. Olimar Remoting", false)]
		public GxDataStore( string id, int handle) : this( null, id, handle, "")
		{
		}
		public GxDataStore( IGxDataRecord db, string id, int handle) : this( db, id, handle, "")
		{
			
		}

		public GxDataStore( IGxDataRecord db, string id, int handle, string connectionString )
		{
			Initialize(db, id, handle, null, connectionString);
		}

#endregion

		public IGxContext Context
		{
			get { return context; }
		}
		private void Initialize(IGxDataRecord db, string id, int hnd, IGxContext context, string connectionString)
		{
			this.id = id;
			this.handle=context!=null ? context.handle : hnd;
			this.context=context;
			connection = GxConnectionManager.Instance.NewConnection(handle, id);
            
			string cs, cfgBuf;
			string addData = "";
            bool cfg = false;
			string ds = Config.DATASTORE_SECTION + id;
			
			if ( Config.GetValueOf(ds, "Connection-"+id+"-DBMS", out cfgBuf) )
			{
                if (cfgBuf.IndexOf(',') > 0)
                    cfgBuf = cfgBuf.Split(',')[0];
				datarecord = getDbmsDataRecord(id, cfgBuf);
			}
			else
			{
				datarecord = (GxDataRecord)db;
			}
			datarecord.DataSource=id;
			connection.DataRecord=datarecord;
			
			if ( Config.GetValueOf(ds,"Connection-"+id, out cs) )
			{
				connection.ConnectionString = cs;
				return;
			}
			
			if (configDataSource(id,out cfgBuf))
			{
				connection.DataSourceName = cfgBuf;
				cfg = true;
			}
			else if ( Config.GetValueOf(ds, "Connection-"+id+"-Driver", out cfgBuf) )
			{
				connection.DriverName = cfgBuf;
				cfg = true;
			}
			else if ( Config.GetValueOf(ds, "Connection-"+id+"-File", out cfgBuf) )
			{
				connection.FileDataSourceName = cfgBuf;
				cfg = true;
			}
			else if ( Config.GetValueOf(ds, "Connection-"+id+"-DataSource", out cfgBuf) )
			{
				connection.DataSourceName = cfgBuf;
				cfg = true;
			}
            else if (Config.GetValueOf(ds, "Connection-" + id + "-Version", out cfgBuf))
            {
                connection.DSVersion = cfgBuf;
                cfg = true;
            }
            
			if ( configUser( id, out cfgBuf))
				connection.UserId = cfgBuf;
			
			if ( configPassword( id, out cfgBuf))
				connection.UserPassword = cfgBuf;
			
            if (configDBName(id, out cfgBuf))
			{
                if (cfgBuf.Length > 0)
                {
                    addData = ";database=" + cfgBuf;
                    datarecord.DataBaseName = cfgBuf;
                }
				cfg = true;
			}
            if ( configSchema(id, out cfgBuf)) 
			{
                if (cfgBuf.Length > 0)
                    connection.CurrentSchema = cfgBuf;
                else
                    connection.CurrentSchema = null;
            }
            string port="";
			if (Config.GetValueOf(ds, "Connection-"+id+"-Port", out port) && !String.IsNullOrEmpty(port))
			{
				connection.Port=port;
			}
			string lockSTimeout;
			if (Config.GetValueOf(ds, "Connection-"+id+"-LockTimeout", out lockSTimeout) && !String.IsNullOrEmpty(lockSTimeout))
			{
				datarecord.LockTimeout=Convert.ToInt32(lockSTimeout) * 1000;
			}
			if (Config.GetValueOf(ds, "Connection-"+id+"-LockRetryCount", out lockSTimeout) && !String.IsNullOrEmpty(lockSTimeout))
			{
				datarecord.LockRetryCount=Convert.ToInt32(lockSTimeout);
			}
			else
			{
				datarecord.LockRetryCount=0; 
			}           
            
            if ( Config.GetValueOf(ds, "Connection-"+id+"-Opts", out cfgBuf) )
			{
				// If the database comes between the opts and had already set it in the DB I do not consider the Opts
				if ( addData.ToLower().IndexOf("database=") >= 0)
				{
					int dbPos = cfgBuf.ToLower().IndexOf("database=");
					if (dbPos >= 0)
					{
						int dbEnd = cfgBuf.IndexOf(";",dbPos);
						if (dbEnd >= 0)
							cfgBuf = cfgBuf.Substring(0,dbPos) + cfgBuf.Substring(dbEnd,cfgBuf.Length-dbEnd);
						else
							cfgBuf = cfgBuf.Substring(0,dbPos);
					}
				}
				connection.Data = cfgBuf;
				cfg = true;
			}
			if (addData.Length > 0 )
				connection.Data = connection.Data + addData;
            
            if ( Config.GetValueOf(ds, "Connection-"+id+"-TrnInt", out cfgBuf) )
				if (cfgBuf.IndexOfAny( new char[] {'0', '1'}) != -1)
					connection.SetTransactionalIntegrity( Convert.ToInt16(cfgBuf));

			if ( ! cfg)
				connection.ConnectionString = connectionString;

			connection.BlobPath = Preferences.getBLOB_PATH();
			string strCache;
			connection.Cache=((Config.GetValueOf("CACHING",out strCache) && strCache.Equals("1")) || CacheFactory.ForceHighestTimetoLive) && ! GxContext.isReorganization;
			connection.DataStore = this;

			string isolevel;
			ushort isoLevelNum=1;
            if (Config.GetValueOf(ds, "Connection-" + id + "-IsolationLevel", out isolevel) && isolevel.ToUpper() == "CR")
            {
                isoLevelNum = 2;
            }
            else if (Config.GetValueOf(ds, "Connection-" + id + "-IsolationLevel", out isolevel) && isolevel.ToUpper() == "SE")
			{ 
                isoLevelNum = 3;
            }
			else if (Config.GetValueOf("ISOLATION_LEVEL", out isolevel) && isolevel.ToUpper() == "CR") 
			{
				isoLevelNum = 2;
			}
			else if (Config.GetValueOf("ISOLATION_LEVEL", out isolevel) && isolevel.ToUpper() == "SE")
			{
				isoLevelNum = 3;
			}

			connection.SetIsolationLevel(isoLevelNum);

			if (Preferences.Instrumented)
			{
				wmidatasource = new WMIDataSource(this);
			}
		}
		public bool AfterConnect()
		{
            if (context != null)
            {
                return context.ExecuteAfterConnect(this.Id);
            }
            else return false;
		}
		public bool BeforeConnect()
		{
			if (context!=null)
				return context.ExecuteBeforeConnect(this);
			else return false;
		}
       
        bool configDataSource(string id, out string ret)
        {
			return Config.GetEncryptedDataStoreProperty(id, "-Datasource", out ret);            
        }
        bool configDBName(string id, out string ret)
        {
			return Config.GetEncryptedDataStoreProperty(id, "-DB", out ret);            
        }
		bool configUser( string id, out string ret)
		{
			return Config.GetEncryptedDataStoreProperty(id, "-User", out ret);                                 
		}
        
        bool configPassword(string id, out string ret)
        {
			return Config.GetEncryptedDataStoreProperty(id, "-Password", out ret);
        }
        bool configSchema(string id, out string ret)
        {
            
            return Config.GetEncryptedDataStoreProperty(id, "-Schema", out ret);
        }
		
		GxDataRecord getDbmsDataRecord(string id, string dbms)
		{
			string cfgBuf;
			switch (dbms)
			{
                case "sqlserver":
                    return new GxSqlServer();
				case "mysql":
#if NETCORE
					return new GxMySqlConnector(id);
#else
				bool prepStmt = true;
				if (Config.GetValueOf("PREPARED_STMT_MYSQL", out cfgBuf))
				{
					if (cfgBuf.ToUpper().StartsWith("N"))
						prepStmt = false;
				}
				if (Config.GetValueOf("Connection-" + id + "-PROVIDER", out cfgBuf) && cfgBuf.ToLower() == "mysqlconnector")
					return new GxMySqlConnector(id);
				else
					return new GxMySql(id, prepStmt);
#endif
				case "sqlite":
                    return new GxSqlite();
                case "postgresql":
                    return new GxPostgreSql();
				case "oracle7":
#if NETCORE
					return new GxODPManagedOracle();
#else
					if (Config.GetValueOf("Connection-" + id + "-PROVIDER", out cfgBuf) && cfgBuf.ToLower() == "managed")
						return new GxODPManagedOracle();
					else if (Config.GetValueOf("Connection-" + id + "-PROVIDER", out cfgBuf) && cfgBuf.ToLower() == "microsoft")
						return new GxOracle();
					else
						return new GxODPOracle();
#endif
				case "as400":
#if NETCORE
					return new GxDb2ISeriesIds();
#else
					if (Config.GetValueOf("Connection-" + id + "-PROVIDER", out cfgBuf) && cfgBuf.ToLower() == "his")
						return new GxISeriesHIS(id);
					else
						return new GxDb2ISeries(id);
#endif
				case "db2":
					return new GxDb2();
				case "informix":
#if NETCORE
					return new GxInformixIds();
#else
					return new GxInformix(id);
#endif
				case "hana":
					return new GxHana();
				case "service":
					{
						string runtimeProvider;
						Config.GetValueOf($"Connection-{id}-DatastoreProvider", out cfgBuf);
						Config.GetValueOf($"Connection-{id}-DatastoreProviderRuntime", out runtimeProvider);
						return NTier.GxServiceFactory.Create(id, cfgBuf, runtimeProvider);												
					}
				default:
					return null;
			}
		}
		public string Id
		{
			get	{ return id; }
			set	{ id = value; }
		}

		public int Handle
		{
			get	{ return handle; }
			set	{ handle = value; }
		}

		public IGxConnection Connection
		{
			get	{ return connection; }
		}
		public IGxDataRecord Db
		{
			get	{ return datarecord; }
			set	{datarecord =  (GxDataRecord)value; }
		}
		public string UserId
		{
			get	{ return connection.UserId; }
		}
        public string Schema
        {
            get {
                string schema = "";
                configSchema(this.id, out schema);
                return schema;
            }
        }
		public short ErrCode
		{
			get
			{
				if ( this.connection == null)
					return 3;
				else
					return Connection.ErrCode;
			}
		}
		public string ErrDescription
		{
			get
			{
				if ( this.connection == null)
					return "No GeneXus datastore attached";
				else
					return Connection.ErrDescription;
			}
		}
		public void CloseConnections()
		{
			GxConnectionManager.Instance.RemoveConnection(handle, id);
		}
		public void Release()
		{
		}
		public IDbTransaction BeginTransaction()
		{
			return connection.BeginTransaction();
		}
		public void Disconnect()
		{
			GxConnectionManager.Instance.RemoveAllConnections(handle);
			if (Preferences.Instrumented)
			{
				wmidatasource.CleanUp();
			}
		}
		public void Commit()
		{
            connection.FlushBatchCursors();
			if (connection.Opened)
			{
				connection.commitTransaction();
				GxConnectionManager.Instance.SetAvailable(handle, id, true);
                if (smartCacheProvider != null)
                    smartCacheProvider.ReccordUpdates();
             }
		}
		public void Rollback()
		{
			connection.BeforeRollback();
			connection.rollbackTransaction();
            if (smartCacheProvider != null)
                smartCacheProvider.DiscardUpdates();
        }
		public void Close()
		{
			if ((connection.State & ConnectionState.Open) != 0 )
				connection.Close();
		}
		public DateTime DateTime 
		{ 
			get 
			{
				return connection.ServerDateTime;
			}
		}
		public DateTime DateTimeMs
		{
			get
			{
				return connection.ServerDateTimeMs;
			}
		}
		public string Version
        {
            get
            {
                return connection.ServerVersion;
            }
        }
#region WMI Members

		public string Name
		{
			get
			{
				return id;
			}
		}

		public string UserName
		{
			get
			{
				if (connection!=null) return connection.UserId;
				else return "";
			}
		}

		public string ConnectionString
		{
			get
			{
				if (connection!=null) return connection.ConnectionString;
				else return "";
			}
		}

		public int MaxCursors
		{
			get
			{
				return Preferences.GetMaximumOpenCursors();
			}
		}

		public bool PoolEnabled
		{
			get
			{
				if (connection!=null)
				{
					string connstr = connection.ConnectionString.Trim().ToLower();
					return (connstr.IndexOf("pooling=false")>=0 || connstr.IndexOf("pooling='false'")>=0);
				}
				else
				{
					return false;
				}

			}
		}

		public bool ConnectAtStartup
		{
			get
			{
				
				return false;
			}
		}

#endregion
	}
}