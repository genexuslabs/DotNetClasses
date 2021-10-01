using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using log4net;
using GeneXus.Application;
using GeneXus.Cache;
using GeneXus.Configuration;
using GeneXus.Utils;
#if !NETCORE
using ConnectionBuilder;
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using TZ4Net;
using System.Collections.Generic;
using System.Data.SqlTypes;
using GeneXus.Services;
using GeneXus.Data.NTier.ADO;
using System.Net;
using GeneXus.Http;
using System.Globalization;
using GeneXus.Metadata;
using System.Data.Common;

namespace GeneXus.Data
{
	public enum GXMetaType
	{
		LVChar=0,
		VChar=1,
		MMedia=2,
		Blob=3

	}
	public enum GXType
	{
		Number = 0,
		Int16 = 1,
		Int32 = 2,
		Int64 = 3,
		Date = 4,
		DateTime = 5,
		DateTime2 = 17,
		Byte = 6,
		NChar = 7,
		NClob = 8,
		NVarChar = 9,
		Char = 10,
		LongVarChar = 11,
		Clob = 12,
		VarChar = 13,
		Raw = 14,
		Blob = 15,
		Undefined = 16,
		Boolean = 18,
		Decimal = 19,
		NText = 20,
		Text = 21,
		Image = 22,
		UniqueIdentifier = 23,
		Xml = 24,
		Geography=25,
		Geopoint=26,
		Geoline=27,
		Geopolygon=28,
		DateAsChar=29
	}

	public interface IGxDataRecord
    {
        byte GetByte(IGxDbCommand cmd, IDataRecord DR, int i);
        long GetBytes(IGxDbCommand cmd, IDataRecord DR, int i, long fieldOffset, byte[] buffer, int bufferOffset, int length);
        short GetShort(IGxDbCommand cmd, IDataRecord DR, int i);
        int GetInt(IGxDbCommand cmd, IDataRecord DR, int i);
        long GetLong(IGxDbCommand cmd, IDataRecord DR, int i);
        double GetDouble(IGxDbCommand cmd, IDataRecord DR, int i);
        string GetString(IGxDbCommand cmd, IDataRecord DR, int i);
		string GetString(IGxDbCommand cmd, IDataRecord DR, int i, int size);
		string GetBinary(IGxDbCommand cmd, IDataRecord DR, int i);
        DateTime GetDateTimeMs(IGxDbCommand cmd, IDataRecord DR, int i);
        DateTime GetDateTime(IGxDbCommand cmd, IDataRecord DR, int i);
        DateTime GetDate(IGxDbCommand cmd, IDataRecord DR, int i);
        Boolean GetBoolean(IGxDbCommand cmd, IDataRecord DR, int i);
        Guid GetGuid(IGxDbCommand cmd, IDataRecord DR, int i);
        decimal GetDecimal(IGxDbCommand cmd, IDataRecord DR, int i);
        IGeographicNative GetGeospatial(IGxDbCommand cmd, IDataRecord DR, int i);
        DateTime Dbms2NetDateTime(DateTime dt, Boolean precision);
        DateTime Dbms2NetDate(IGxDbCommand cmd, IDataRecord DR, int i);
        Object Net2DbmsDateTime(IDbDataParameter parm, DateTime dt);
		Object Net2DbmsGeo(GXType type, IGeographicNative parm);
		String DbmsTToC(DateTime dt);
        void GetValues(IDataReader reader, ref object[] values);

        [Obsolete("ProcessError with 6 arguments is deprecated", false)]
        bool ProcessError(uint e, string emsg, GxErrorMask errMask, ref int status, ref bool retry, int retryCount);

        bool ProcessError(int e, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount);
        bool IsDBNull(IGxDbCommand cmd, IDataRecord DR, int i);
        string FalseCondition();
        string ToDbmsConstant(short Value);
        string ToDbmsConstant(int Value);
        string ToDbmsConstant(long Value);
        string ToDbmsConstant(decimal Value);
        string ToDbmsConstant(float Value);
        string ToDbmsConstant(double Value);
        string ToDbmsConstant(string Value);
        string ToDbmsConstant(DateTime Value);
        string ToDbmsConstant(Boolean Value);
        int GetCommandTimeout();
        IDbCommand GetCommand(IGxConnection con, string stmt, GxParameterCollection parameters);
        IDbCommand GetCommand(IGxConnection con, string stmt, GxParameterCollection parameters, bool isCursor, bool forFirst, bool isRpc);
        IDataReader GetDataReader(IGxConnectionManager connManager, IGxConnection connection,
            GxParameterCollection parameters, string stmt, ushort fetchSize, bool forFirst,
            int handle, bool cached, SlidingTime expiration, bool hasNested, bool dynStmt);
        IDataReader GetCacheDataReader(CacheItem item, bool computeSize, string keyCache);
        IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec);
        IDbDataParameter CreateParameter();
        void SetParameter(IDbDataParameter parameter, Object value);
        object SetParameterValue(IDbDataParameter parameter, Object value);
        void SetParameterLVChar(IDbDataParameter parameter, string value, IGxDataStore datastore);
		void SetParameterBlob(IDbDataParameter parameter, string value, bool dbBlob);
		void SetParameterChar(IDbDataParameter parameter, string value);
        void SetParameterVChar(IDbDataParameter parameter, string value);
        string DataSource { get; }
        void CreateDataBase(string dbname, IGxConnection con);
        DbDataAdapterElem GetDataAdapter(IGxConnection con, string stmt, GxParameterCollection parameters);
        DbDataAdapterElem GetDataAdapter(IGxConnection con, string stmt, int batchSize, string stmtId);
        int BatchUpdate(DbDataAdapterElem da);
        int LockTimeout { get; set; }
        int LockRetryCount { set; get; }
        bool Retry(GxErrorMask errMask, int retryCount);
        void SetParameterDir(GxParameterCollection parms, int num, ParameterDirection dir);
        object[] ExecuteStoredProcedure(IDbCommand cmd);
        bool AllowsDuplicateParameters { get; }
		void SetAdapterInsertCommand(DbDataAdapterElem da, IGxConnection con, string stmt, GxParameterCollection parameters);
		void SetCursorDef(CursorDef cursorDef);
		string ConcatOp(int pos);
		string AfterCreateCommand(string stmt, GxParameterCollection parmBinds);
	}


	public interface IGxDbCommand : IDbCommand
	{

		IGxDataRecord Db{get; set;}
		
		Boolean NoError{get; }
		bool HasMoreRows{get; set;}
		GxErrorMask ErrorMask{get;set;}
		void FetchData(out IDataReader DR);
		void ExecuteStmt();
		
		void SetParameter( int num, Object value);
		IGxConnection Conn { get; }
		
	}

	
	public interface IGxDataStore
	{
		string Id { get; set; }
		int Handle { get; set; }
		IGxConnection Connection {get;}
		IGxContext Context { get; }
		IGxDataRecord Db {get; set;}
        string Schema { get; }
        string UserId {get;}
		short ErrCode {get;}
		string ErrDescription {get;}      
        GxSmartCacheProvider SmartCacheProvider {get;}
		OlsonTimeZone ClientTimeZone { get; }
        void CloseConnections();
		void Release();
		IDbTransaction BeginTransaction();
		void Commit();
		void Rollback();
		void Close();
		void Disconnect();
		DateTime DateTime { get;}
		DateTime DateTimeMs { get; }
		string Version { get;}
		bool BeforeConnect();
		bool AfterConnect();
    }
	
	public interface IGxConnectionManager
	{
		IGxConnection IncOpenHandles(int handle, string dataSource);
		void DecOpenHandles(int handle, string dataSource);
		void RefreshTimeStamp(int handle, string dataSource);
		IGxConnection SetAvailable(int handle, string dataSource, bool available);
		IGxConnection NewConnection(int handle, string dataSource);
		void RemoveAllConnections(int handle);
		void RemoveConnection(int handle, string dataSource);
		bool CloseUserConnections(int handle, string dataSource);
		object GetUserInformation(int handle, string dataSource);
		void run();
	}

	
	public interface IGxConnection: IDbConnection
	{
		string Data {get ;set ;}
		short Method { get;}
        new string Database { get; set; }
		string DatabaseName { get;}
		OlsonTimeZone ClientTimeZone { get;}
		string DataSourceName {get ;set ;}
		string DriverName {get ;set ;}
		string FileDataSourceName {get ;set ;}
		short ShowPrompt {get ;	set ;}
		string UserId {	get ; set ;	}
		string UserPassword	{ get ; set ;}
		string Port { get ; set ;}
        string CurrentSchema { get; set; }
		DateTime ServerDateTime {get;}
		DateTime ServerDateTimeMs { get; }
		string ServerVersion { get;}
		short ErrCode {get ;}
		string ErrDescription { get ;}
		short FullConnect();
		short Disconnect();
		IDbTransaction rollbackTransaction();
		IDbTransaction commitTransaction();
		void SetTransactionalIntegrity( short trnInt);
        GxAbstractConnectionWrapper InternalConnection{get; }
		
		bool Available{ get;set;}
		void RefreshTimeStamp();
		int OpenHandles{ get;}
		void IncOpenHandles();
		void DecOpenHandles();
		void MonitorEnter();
		void MonitorExit();
		string BlobPath{ get; set;}
		string MultimediaPath { get; }
		bool Cache{get; set;}
		
		bool Opened{get; set;}

		GxConnectionCache ConnectionCache{get;set;}
		string CurrentStmt{get;set;}
		GxDataRecord DataRecord{get;set;}
		IGxDataStore DataStore{get;set;}
		
		void SetIsolationLevel( ushort isolationLevel); 
		bool UncommitedChanges{get;set;}
        void FlushBatchCursors(GXBaseObject obj=null);
		void BeforeRollback();
		
		string LastObject
		{
			get;set;
		}
		bool LastSQLStatementEnded
		{
			get;set;
		}
        string DSVersion
        {
            get;
            set;
        }

		
	}
	
	public static class GxCacheFrequency
	{
		public static int OFF = 0;
		public static int TIME_TO_TIME = 1;
		public static int HARDLY_EVER = 2;
		public static int ALMOST_NEVER = 3;		
	}

	[Flags]
	public enum GxErrorMask 
	{
		GX_NOMASK			= 1,
		GX_MASKLOCKERR		= 2,
		GX_MASKNOTFOUND		= 4,
		GX_MASKDUPKEY		= 8,
		GX_MASKOBJEXIST		= 16,
		GX_MASKLOOPLOCK		= 32,
		GX_MASKFOREIGNKEY	= 64,
		GX_ROLLBACKSAVEPOINT= 128
	}
	public abstract class GxDataRecord : IGxDataRecord
	{

		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.GxDataRecord));
		public static int RETRY_SLEEP = 500; //500 milliseconds.
		protected string m_connectionString;
		protected string m_datasource;
		protected IsolationLevel isolationLevel;
		protected int m_lockRetryCount;
		protected int m_lockTimeout;
        protected string m_dataBaseName;
		protected bool m_avoidDataTruncationError;

		public GxDataRecord()
		{
			if (Config.GetValueOf("AvoidDataTruncationError", out string str))
				m_avoidDataTruncationError = (str.ToLower() == "y");
		}
		public abstract IDataReader GetDataReader(IGxConnectionManager connManager,IGxConnection connection, 
			GxParameterCollection parameters, string stmt, ushort fetchSize, bool forFirst, int handle, 
			bool cached, SlidingTime expiration, bool hasNested, bool dynStmt);
#if !NETCORE
		protected void GetConnectionDialogString(int dbmsType)
		{
			if (Dialogs.DialogFactory != null)
			{
				IConnectionDialog dlg = Dialogs.DialogFactory.GetConnectionDialog(dbmsType);
				if (dlg.Show(" "))
				{
					m_connectionString = dlg.ConnectionString;
				}
				else
				{
					GXLogging.Error(log, "GxConnection.Open No se puedo obtener informacion de string de conexion");
					throw (new GxADODataException("Cancel"));
				}
			}
		}
#endif
		public virtual IDataReader GetCacheDataReader(CacheItem item, bool computeSize, string keyCache)
		{
			return new GxCacheDataReader (item, computeSize, keyCache);
		}
		public IsolationLevel IsolationLevelTrn
		{
			get{ return isolationLevel;}
			set { isolationLevel=value;}
		}
		public abstract GxAbstractConnectionWrapper GetConnection(
			bool showPrompt, string datasourceName, string userId, 
			string userPassword,string databaseName, string port, string schema, string extra,
			GxConnectionCache connectionCache);

		public abstract IDbDataParameter CreateParameter();

		public abstract IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec);

		protected abstract string BuildConnectionString(string datasourceName, string userId, 
			string userPassword,string databaseName, string port, string schema,  string extra); 

		public virtual object[] ExecuteStoredProcedure(IDbCommand cmd)
		{
			cmd.ExecuteNonQuery();
			int count=cmd.Parameters!=null ? cmd.Parameters.Count:0;
			if (count>0)
			{
				object[] values = new object[count];
				for (int i=0; i<count; i++)
				{
					values[i]=((IDataParameter)cmd.Parameters[i]).Value;
				}
				return values;
			}
			else
			{
				return null;
			}
		}
        public virtual bool AllowsDuplicateParameters 
        { 
            get{return true;} 
        }
		public virtual bool MultiThreadSafe
		{
			get{ return true;}
		}
		public virtual void SetParameterDir(GxParameterCollection parameters, int num, ParameterDirection dir)
		{
			parameters[num].Direction=dir;
		}
		public virtual void SetParameter(IDbDataParameter parameter, Object value)
		{
			if (value==null || value==DBNull.Value)
			{
				parameter.Value = DBNull.Value;
			}
#if !NETCORE
            else if ( value.GetType().ToString().Equals(SQLGeographyWrapper.SqlGeographyClass) &&
					 parameter.GetType() == typeof(SqlParameter))
            {
                parameter.Value = value;
				((SqlParameter)parameter).UdtTypeName = "Geography";                    
			}
#endif
			else if (!IsBlobType(parameter))
            {
				parameter.Value = CheckDataLength(value, parameter);
            }
            else
            {
				SetBinary(parameter, GetBinary((string)value, false));
			}
		}
		protected object CheckDataLength(object value, IDbDataParameter parameter)
		{
			if (m_avoidDataTruncationError)
			{
				string svalue = value.ToString();
				if (svalue != null && svalue.Length > parameter.Size)
				{
					return svalue.Substring(0, parameter.Size);
				}
			}
			return value;
		}

		public virtual void SetParameterBlob(IDbDataParameter parameter, string value, bool dbBlob)
		{
			byte[] binary = GetBinary(value, dbBlob);
			SetBinary(parameter, binary);
		}

		protected virtual void SetBinary(IDbDataParameter parameter, byte[] binary) {
			parameter.Value = binary;
			if (binary != null)
				parameter.Size = binary.Length;
 		}
		public virtual object SetParameterValue(IDbDataParameter parameter, Object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return DBNull.Value;
            }

            else if (!IsBlobType(parameter))
            {
                return value;
            }
            else
            {
                byte[] binary = GetBinary((string)value);
                return binary;
                
            }

        }
		public virtual void SetParameterChar(IDbDataParameter parameter, string value)
		{
			SetParameter(parameter, value);
		}
		public virtual void SetParameterLVChar(IDbDataParameter parameter, string value, IGxDataStore datastore)
		{
			SetParameter(parameter, value);
		}
		public virtual void SetParameterVChar(IDbDataParameter parameter, string value)
		{
			SetParameter(parameter, value);
		}

		public abstract string GetServerDateTimeStmt(IGxConnection connection);
		public abstract string GetServerDateTimeStmtMs(IGxConnection connection);


		public abstract string GetServerUserIdStmt();

		public abstract string GetServerVersionStmt();

		public virtual void SetTimeout(IGxConnectionManager connManager, IGxConnection connection, int handle)
		{
		}
		public virtual string SetTimeoutSentence(long milliseconds){ return null;}

		public int LockTimeout
		{
			get{ return m_lockTimeout;}
			set{ m_lockTimeout=value;}
		}

		public int LockRetryCount
		{
			get{ return m_lockRetryCount;}
			set{ m_lockRetryCount=value;}
		}
        public string DataBaseName
        {
            get { return m_dataBaseName; }
            set { m_dataBaseName = value; }
        }
        internal virtual void  CalculateRetryCount(out int maxRetryCount, out int sleepTimeout)
		{
			maxRetryCount = m_lockRetryCount;
			sleepTimeout = m_lockTimeout;
		}
		public bool Retry(GxErrorMask errMask, int retryCount)
		{
			GXLogging.Debug(log, "Lock Retry, retry:" + retryCount + ", maxRetryCount:" + m_lockRetryCount + ", lockTimeout: "  + m_lockTimeout + ",GX_MASKLOOPLOCK:" + ((errMask & GxErrorMask.GX_MASKLOOPLOCK) > 0));
			bool retry=false;
			int maxRetryCount, sleepTimeout;
			CalculateRetryCount( out maxRetryCount, out sleepTimeout);
			retry = ((errMask & GxErrorMask.GX_MASKLOOPLOCK) > 0) && ((maxRetryCount==0) || retryCount < maxRetryCount);
			if (retry)
			{
				Thread.Sleep(sleepTimeout);
			}
			return retry;
		}
		public virtual void CreateDataBase(string dbname, IGxConnection con)
		{
		}

		public virtual byte GetByte( IGxDbCommand cmd, IDataRecord DR, int i)
		{
			if ( !cmd.HasMoreRows || DR == null || DR.IsDBNull( i))
				return 0;
			else
				return DR.GetByte( i);
		}
		public virtual long GetBytes(IGxDbCommand cmd, IDataRecord DR, int i, long fieldOffset, byte[] buffer , int bufferOffset, int length )
		{
			if ( !cmd.HasMoreRows || DR == null || DR.IsDBNull( i))
				return 0;
			else
				return DR.GetBytes( i, fieldOffset, buffer , bufferOffset, length);
		}

		public virtual short GetShort( IGxDbCommand cmd, IDataRecord DR, int i)
		{
			if ( !cmd.HasMoreRows || DR == null || DR.IsDBNull( i))
				return 0;
			else
				return DR.GetInt16( i);
		}
		public virtual int GetInt( IGxDbCommand cmd, IDataRecord DR, int i)
		{
			if ( !cmd.HasMoreRows || DR == null || DR.IsDBNull( i))
				return 0;
			else
				return DR.GetInt32( i);
		}
		public virtual long GetLong( IGxDbCommand cmd, IDataRecord DR, int i)
		{
			if ( !cmd.HasMoreRows || DR == null || DR.IsDBNull( i))
				return 0;
			else
				return DR.GetInt64( i);
		}
		public virtual double GetDouble( IGxDbCommand cmd, IDataRecord DR, int i)
		{
			if ( !cmd.HasMoreRows || DR == null || DR.IsDBNull( i))
				return 0;
			else
				return DR.GetDouble( i);
		}
		public virtual string GetString( IGxDbCommand cmd, IDataRecord DR, int i)
		{
			if ( !cmd.HasMoreRows || DR == null || DR.IsDBNull( i))
				return string.Empty;
			else
				return DR.GetString( i);
		}
		public virtual string GetBinary( IGxDbCommand cmd, IDataRecord DR, int i)
		{
			if ( !cmd.HasMoreRows || DR == null || DR.IsDBNull( i))
				return string.Empty;
			else
				return DR.GetString( i).TrimEnd();
		}
        public virtual DateTime GetDateTimeMs(IGxDbCommand cmd, IDataRecord DR, int i)        
        {
            if (!cmd.HasMoreRows || DR == null || DR.IsDBNull(i))
                return DateTimeUtil.NullDate();
            else
                return Dbms2NetDateTime(DR.GetDateTime(i), true);
        }
        public virtual DateTime GetDateTime( IGxDbCommand cmd, IDataRecord DR, int i)
		{
			if ( !cmd.HasMoreRows || DR == null || DR.IsDBNull( i))
				return DateTimeUtil.NullDate();
			else
				return Dbms2NetDateTime( DR.GetDateTime( i) , false);
		}
		public virtual DateTime GetDate( IGxDbCommand cmd, IDataRecord DR, int i)
		{
			if ( !cmd.HasMoreRows || DR == null || DR.IsDBNull( i))
				return DateTimeUtil.NullDate();
			else
				return Dbms2NetDate(cmd, DR, i);
		}
		public virtual Boolean GetBoolean( IGxDbCommand cmd, IDataRecord DR, int i)
		{
			if ( !cmd.HasMoreRows || DR == null || DR.IsDBNull( i))
				return false;
			else
				return DR.GetBoolean( i);
		}
        public virtual Guid GetGuid(IGxDbCommand cmd, IDataRecord DR, int i)
        {
            if (!cmd.HasMoreRows || DR == null || DR.IsDBNull(i))
                return Guid.Empty;
            else
                return DR.GetGuid(i);
        }
        public virtual IGeographicNative GetGeospatial(IGxDbCommand cmd, IDataRecord DR, int i)
        {
			throw (new GxNotImplementedException());
        }
        public virtual decimal GetDecimal(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			if ( !cmd.HasMoreRows || DR == null || DR.IsDBNull( i))
				return 0;
			else
				return DR.GetDecimal( i);
		}
		public virtual DateTime Dbms2NetDateTime( DateTime dt, Boolean precision)
		{
			return dt;
		}
		public virtual Object Net2DbmsDateTime(IDbDataParameter parm, DateTime dt)
		{
			return dt;
		}
        public virtual IGeographicNative Dbms2NetGeo(IGxDbCommand cmd, IDataRecord DR, int i)
        {
			throw (new GxNotImplementedException());
        }
        public virtual Object Net2DbmsGeo(GXType type, IGeographicNative geo)
        {
			throw (new GxNotImplementedException());
        }
		public virtual DateTime Dbms2NetDate(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return DR.GetDateTime(i);
		}
		public virtual String DbmsTToC( DateTime dt)
		{
			return dt.ToString( "yyyy-MM-dd HH:mm:ss");
		}
		public bool ProcessError( uint dbmsErrorCode, string emsg, GxErrorMask errMask, ref int status, ref bool retry, int retryCount)
			
		{
			return ProcessError((int)dbmsErrorCode, emsg, errMask, null, ref status, ref retry, 0);
		}
		public virtual bool ProcessError( int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)
			
		{
			return false;
		}

		public virtual Boolean IsDBNull( IGxDbCommand cmd, IDataRecord DR, int i)
		{
			if ( !cmd.HasMoreRows || DR == null || DR.IsDBNull( i))
				return true;
			else
				return false;
		}
		public virtual string ToDbmsConstant(short Value)
		{
			return Value.ToString();
		}

		public virtual string ToDbmsConstant(int Value)
		{
			return Value.ToString();
		}

		public virtual string ToDbmsConstant(long Value)
		{
			return Value.ToString();
		}

		public virtual string ToDbmsConstant(decimal Value)
		{
			return Value.ToString(CultureInfo.InvariantCulture);
		}

		public virtual string ToDbmsConstant(float Value)
		{
			return Value.ToString(CultureInfo.InvariantCulture);
		}
		public virtual string ToDbmsConstant(double Value)
		{
			return Value.ToString(CultureInfo.InvariantCulture);
		}
		public virtual string ToDbmsConstant(string Value)
		{
			return "'" + Value.Replace("'","''") + "'";
		}
		public virtual string ToDbmsConstant(DateTime Value)
		{
			return "'" + Value.ToString("yyyy-MM-dd HH\\:mm\\:ss").Replace("'","''") + "'";
		}
		public virtual string ToDbmsConstant(Boolean Value)
		{
			return (Value)?"1":"0";
		}
		public virtual string FalseCondition()
		{
			return " 1=0 ";
		}
		internal virtual object CloneParameter(IDbDataParameter parameter)
		{
			return parameter;
			
		}

        public abstract DbDataAdapter CreateDataAdapeter();
		public virtual bool SupportUpdateBatchSize
		{
			get { return true; }
		}
		public virtual void SetAdapterInsertCommand(DbDataAdapterElem da, IGxConnection con, string stmt, GxParameterCollection parameters)
		{
			da.Adapter.InsertCommand = (DbCommand) GetCommand(con, stmt, parameters);
			da.Adapter.InsertCommand.UpdatedRowSource = UpdateRowSource.None;
		}
        public virtual DbDataAdapterElem GetDataAdapter(IGxConnection con, string stmt, int batchSize, string stmtId)
        {
            DbDataAdapterElem adapter = GetCachedDataAdapter(con, stmtId);
            if (adapter == null)
            {
                DbDataAdapter dbadapter = CreateDataAdapeter();
				if (SupportUpdateBatchSize)
				{
					dbadapter.UpdateBatchSize = batchSize;
				}
                DataTable dt = new DataTable();
				adapter = new DbDataAdapterElem(dbadapter, dt, batchSize);
                con.ConnectionCache.AddDataAdapter(stmtId, adapter);
            }
            return adapter;

        }
        public virtual DbDataAdapterElem GetDataAdapter(IGxConnection con, string stmt, GxParameterCollection parameters)
		{
            DbDataAdapterElem adapter = GetCachedDataAdapter(con, stmt);
            if (adapter == null)
            {
                DbDataAdapter dbadapter = con.InternalConnection.CreateDataAdapter();
                DataTable dt = new DataTable();
                adapter = new DbDataAdapterElem(dbadapter, dt, 0);
                con.ConnectionCache.AddDataAdapter(stmt, adapter);
            }
            return adapter;
		}
        protected virtual DbDataAdapterElem GetCachedDataAdapter(IGxConnection con, string stmt)
        {
            return con.ConnectionCache.GetDataAdapter(stmt);
        }
        public virtual int BatchUpdate(DbDataAdapterElem da)
        {
            return da.Adapter.Update(da.DataTable);
        }

		public virtual bool IsBlobType(IDbDataParameter idbparameter)
		{
			return idbparameter.DbType == DbType.Binary;
		}
		public virtual void AddParameters(IDbCommand cmd, GxParameterCollection parameters)
		{
			for (int j=0; j< parameters.Count; j++)
			{
				cmd.Parameters.Add(CloneParameter(parameters[j]));
			}

		}
		public virtual IDbCommand GetCommand(IGxConnection con, string stmt, GxParameterCollection parameters, bool isCursor, bool forFirst, bool isRpc)
		{
			return GetCommand(con, stmt, parameters);
		}
		protected virtual void PrepareCommand(IDbCommand cmd)
		{
		}
        public virtual int GetCommandTimeout()
        {
            return 0;
        }
		public virtual IDbCommand GetCommand(IGxConnection con, string stmt, GxParameterCollection parameters)
		{
			IDbCommand cmd = GetCachedCommand(con, stmt);

			if (cmd==null)
			{
				cmd = con.InternalConnection.CreateCommand();
				cmd.CommandText=stmt;
				cmd.Connection=con.InternalConnection.InternalConnection;
				
                cmd.CommandTimeout = GetCommandTimeout();
				AddParameters(cmd, parameters);
				cmd.Transaction=con.BeginTransaction();
				
				PrepareCommand(cmd);
			    con.ConnectionCache.AddPreparedCommand(stmt, cmd);
			}
			else
			{
				if (parameters.Count==cmd.Parameters.Count)
				{
					if (parameters.Count>0)
					{
						for (int j=0; j< parameters.Count; j++)
						{
							IDbDataParameter idbparameter = (IDbDataParameter)cmd.Parameters[j];
							object value = parameters[j].Value;
							idbparameter.Value=value;
							if( value!=null && IsBlobType(idbparameter))
							{
								try
								{
									idbparameter.Size = ((byte[])idbparameter.Value).Length;
								}
								catch(Exception ex)
								{
									GXLogging.Error(log, "Set Binary parameter length in cached command error", ex );
								}
							}
						}
					}
				}
				else
				{
					
					cmd.Parameters.Clear();

					AddParameters(cmd, parameters);
				}
				cmd.Connection=con.InternalConnection.InternalConnection;
				cmd.Transaction=con.BeginTransaction();
			}
			return cmd;
		}

		public virtual void DisposeCommand(IDbCommand command)
		{
			command.Dispose();
		}

		protected virtual  IDbCommand GetCachedCommand(IGxConnection con, string stmt)
		{
			return 	con.ConnectionCache.GetPreparedCommand(stmt);
		}

		public string ConnectionString 
		{
			get{ return m_connectionString;}
			set{ m_connectionString=value;}
		}

		protected static byte[] GetBinary(string fileName)
		{
			return GetBinary(fileName, false);
		}
		protected static byte[] GetBinary(string fileNameParm, bool dbBlob)
		{			
			Uri uri;
			string fileName = fileNameParm;
			bool inLocalStorage = dbBlob || GXServices.Instance == null || GXServices.Instance.Get(GXServices.STORAGE_SERVICE) == null;
			bool validFileName = !String.IsNullOrEmpty(fileName) && !String.IsNullOrEmpty(fileName.Trim()) && String.Compare(fileName, "about:blank", false) != 0;
			byte[] binary = Array.Empty<byte>();

			if (inLocalStorage && validFileName)
            {
                if (GxUploadHelper.IsUpload(fileName))
                    fileName = GxUploadHelper.UploadPath(fileName);

				bool ok = PathUtil.AbsoluteUri(fileName, out uri);
				if (ok && uri != null)
				{					
					switch (uri.Scheme)
					{
						case "http":
						case "https":						
							HttpStatusCode statusCode;
							binary = HttpHelper.DownloadFile(uri.AbsoluteUri, out statusCode);
							if (statusCode != HttpStatusCode.OK)
							{
								if (statusCode == HttpStatusCode.NotFound) //Error 404 Not found.
								{
									GXLogging.Error(log, "GxCommand. The filename does not exists in url: " + uri.AbsoluteUri);
									throw new GxADODataException("GxCommand. The filename does not exists in url: " + uri.AbsoluteUri);
								}
								else
								{
									GXLogging.Error(log, "GxCommand. An error occurred while downloading data from url " + uri.AbsoluteUri);
									throw new GxADODataException("GxCommand. An error occurred while downloading data from url " + uri.AbsoluteUri);
								}
							}
							break;
						case "file":
							try
							{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
								using (FileStream fs = new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read))
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
								{
									using (BinaryReader br = new BinaryReader(fs))
									{
										binary = br.ReadBytes((int)fs.Length);
									}
								}
							}
							catch (Exception e)
							{
								if (ServiceFactory.GetExternalProvider() != null) 
								{
									GxFile file = new GxFile(string.Empty, fileNameParm, GxFileType.PrivateAttribute);
									if (file.Exists())
									{
										binary = file.ToByteArray();
										return binary;
									}
								}
								GXLogging.Debug(log, "GxCommand. An error occurred while getting data from file path ", uri.AbsolutePath, e);
								throw e;
							}
							break;
						default:
							GXLogging.Error(log, "Schema not supported: ", fileName);
							break;
					}				
					GXLogging.Debug(log, "GetBinary fileName ", uri.AbsolutePath, ",ReadBytes:", binary != null ? binary.Length.ToString() : "0");
				}
				else
				{
					GXLogging.Error(log, "Not a valid URI: ", fileName);
					throw new GxADODataException("GxCommand. Not a valid uri:  " + fileName);
				}
				return binary;
			}
			else
			{
				return Array.Empty<byte>();
			}
		}			

		public string DataSource
		{
			get {return m_datasource;}
			set{m_datasource=value;}
		}
		public string ConnectionStringForLog()
		{
			string result="";
			if (m_connectionString!=null)
			{
				int i1 = m_connectionString.IndexOf("Password");
				int i=0;
				if (i1>0)  i=i1;
				int j = m_connectionString.IndexOf( ";", i)!=-1 ? m_connectionString.IndexOf( ";", i): m_connectionString.Length;
				result = m_connectionString.Substring(0,i);
				result += (i1>=0 ? "Password=xxxxx" : "") + m_connectionString.Substring(j);
			}
			return result;
		}
		protected virtual bool hasKey(string data, string key)
		{
			if (!string.IsNullOrEmpty(data) && data.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				string[] props = data.Split(';');
				foreach (string s in props)
				{
					string[] parts = s.Split('=');
					if (parts[0].Equals(key, StringComparison.OrdinalIgnoreCase))
						return true;
				}
			}
			return false;
		}
		protected virtual string ParseAdditionalData(string data, string extractWord)
		{
			char[] sep = {';'};
			StringBuilder res=new StringBuilder("");
			string [] props = data.Split(sep);
			foreach (string s in props)
			{
				if ( s!=null && s.Length>0 && !s.ToLower().StartsWith(extractWord))
				{
					res.Append(s);
					res.Append(';');
				}
			}
			return res.ToString();
		}
		protected virtual string ReplaceKeyword(string data, string keyword, string newKeyword)
		{
			char[] sep = { ';' };
			StringBuilder res = new StringBuilder("");
			string[] props = data.Split(sep);
			foreach (string s in props)
			{
				if (s != null && s.Length > 0)
				{
					string[] prop = s.Split('=');
					if (prop != null && prop.Length == 2 && prop[0].Trim().Equals(keyword, StringComparison.OrdinalIgnoreCase))
					{
						res.Append(newKeyword);
						res.Append('=');
						res.Append(prop[1]);
					}
					else
					{
						res.Append(s);
						res.Append(';');
					}
				}
			}
			return res.ToString();
		}
		protected virtual string RemoveDuplicates(string data, string extractWord)
		{
			char[] sep = { ';' };
			StringBuilder res = new StringBuilder("");
			string[] props = data.Split(sep);
			bool added = false;
			for (int i=props.Length-1; i>=0; i--)
			{
				string s = props[i];
				if (s != null && s.Length > 0)
				{
					if (s.Trim().StartsWith(extractWord, StringComparison.OrdinalIgnoreCase))
					{
						if (!added)
						{
							res.Append(s);
							res.Append(';');
							added = true;
						}
					}
					else
					{
						res.Append(s);
						res.Append(';');
					}
				}
			}
			return res.ToString();
		}

		public virtual void GetValues(IDataReader reader,ref object[] values)
		{
			reader.GetValues( values);
		}
		public string GetParameterValue(string conStr, string searchParam) 
		{
			if (string.IsNullOrEmpty(conStr))
				return string.Empty;
			else
			{
				
				int posBegin = 0;
				int posEnd = 0;
				string paramVal = string.Empty;

				posBegin = conStr.IndexOf(searchParam, StringComparison.OrdinalIgnoreCase);
				if (posBegin > -1)
				{
					
					posBegin += searchParam.Length + 1;
					if (conStr.LastIndexOf(';') > posBegin)
						
						posEnd = conStr.IndexOf(';', posBegin);
					else
						
						posEnd = conStr.Length;

					paramVal = conStr.Substring(posBegin, (posEnd - posBegin));
				}
				return paramVal;
			}
		}

		public virtual DateTime DTFromString( string s)
		{
			if (s.Trim().Length == 0)
				return DateTime.MinValue;
			else if (s.Trim().Length == 8)
				return new DateTime( 
					Convert.ToInt32(s.Substring(0, 2)), 
					Convert.ToInt32(s.Substring(3, 2)), 
					Convert.ToInt32(s.Substring(6, 2)),
					0,0,0);
			else if (s.Trim().Length == 10)
				return new DateTime( 
					Convert.ToInt32(s.Substring(0, 4)), 
					Convert.ToInt32(s.Substring(5, 2)), 
					Convert.ToInt32(s.Substring(8, 2)),
					0,0,0);
			else
				return new DateTime( 
					Convert.ToInt32(s.Substring(0, 4)), 
					Convert.ToInt32(s.Substring(5, 2)), 
					Convert.ToInt32(s.Substring(8, 2)),
					Convert.ToInt32(s.Substring(11, 2)),
					Convert.ToInt32(s.Substring(14, 2)),
					Convert.ToInt32(s.Substring(17, 2)));
		}

		public virtual void SetCursorDef(CursorDef cursorDef)
		{
		}
		private static readonly string[] ConcatOpValues = new string[] { "CONCAT(", ", ", ")" };
		public virtual string ConcatOp(int pos)
		{
			return ConcatOpValues[pos];
		}

		public virtual string GetString(IGxDbCommand cmd, IDataRecord DR, int i, int size)
		{
			return GetString(cmd, DR, i);
		}
		public virtual string AfterCreateCommand(string stmt, GxParameterCollection parmBinds)
		{
			return stmt;
		}

	}

	public class DbDataAdapterElem
    {
        public DbDataAdapterElem(DbDataAdapter adapter, DataTable table, int updateBatchSize)
        {
            Adapter = adapter;
            DataTable = table;
            BlockValues = new ArrayList();
			UpdateBatchSize = updateBatchSize;
        }
        public void Clear()
        {
            DataTable.Rows.Clear();
            BlockValues.Clear();
        }
        public object OnCommitEventInstance;
        public string OnCommitEventMethod;
        public bool Initialized;
        public ArrayList BlockValues;

        public IDbCommand Command;

        public DbDataAdapter Adapter;
        public DataTable DataTable;
		public int UpdateBatchSize;
    }
	public class SqlUtil
	{
		public static Hashtable mapping;
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.SqlUtil));

		public static string SqlTypeToDbType(SqlParameter parameter)
		{
			SqlDbType type = parameter.SqlDbType;
			switch (type)
			{
				case SqlDbType.SmallInt:return "smallint";
				case SqlDbType.Decimal: return "decimal(" + parameter.Precision + "," + parameter.Scale +")";
				case SqlDbType.VarChar:
					if (parameter.Size < 8000) {
#if !NETCORE
						if (parameter.Value != null && parameter.Value != DBNull.Value && ((parameter.Value as String)!=null))
							return $"varchar({ Math.Max(parameter.Size, Encoding.Default.GetByteCount(parameter.Value as string))})";
						else
#endif
							return $"varchar({parameter.Size})";
					} else
						return "varchar(MAX)";
				case SqlDbType.DateTime:return "datetime";
                case SqlDbType.DateTime2: return "datetime2(3)";
                case SqlDbType.Float:return "float";
				case SqlDbType.BigInt:return "bigint";
				case SqlDbType.NChar:return "nchar(" + parameter.Size+ ")";
				case SqlDbType.NText:return "ntext";
				case SqlDbType.NVarChar:return "nvarchar(" + parameter.Size+ ")";
				case SqlDbType.Real:return "real";
				case SqlDbType.Binary:return "binary(8000)";
				case SqlDbType.Bit:return "bit";
				case SqlDbType.Char:return "char(" + parameter.Size+ ")";
				case SqlDbType.Image:return "image";
				case SqlDbType.Int:return "int";
				case SqlDbType.Money:return "money";
				case SqlDbType.SmallDateTime:return "smalldatetime";
				case SqlDbType.SmallMoney:return "smallmoney";
				case SqlDbType.Text:return "text";
				case SqlDbType.Timestamp:return "timestamp";
				case SqlDbType.TinyInt:return "tinyint";
				case SqlDbType.UniqueIdentifier:return "uniqueidentifier";
				case SqlDbType.VarBinary:return  "varbinary(" + parameter.Size+ ")";
				case SqlDbType.Variant:return "sql_variant";
				default: throw (new GxNotImplementedException());
			}
		}
        public static Type SqlTypeToType(SqlDbType dbType)
        {
            Type toReturn = typeof(DBNull);

            switch (dbType)
            {
                case SqlDbType.BigInt:
                    toReturn = typeof(Int64);
                    break;
                case SqlDbType.Binary:
                    toReturn = typeof(Byte[]);
                    break;
                case SqlDbType.Bit:
                    toReturn = typeof(Boolean);
                    break;
                case SqlDbType.Char:
                    toReturn = typeof(String);
                    break;
                case SqlDbType.DateTime:
                    toReturn = typeof(DateTime);
                    break;
				case SqlDbType.DateTime2:
                    toReturn = typeof(DateTime);
                    break;	
                case SqlDbType.Decimal:
                    toReturn = typeof(Decimal);
                    break;
                case SqlDbType.Float:
                    toReturn = typeof(Double);
                    break;
                case SqlDbType.Image:
                    toReturn = typeof(Byte[]);
                    break;
                case SqlDbType.Int:
                    toReturn = typeof(Int32);
                    break;
                case SqlDbType.Money:
                    toReturn = typeof(Decimal);
                    break;
                case SqlDbType.NChar:
                    toReturn = typeof(string);
                    break;
                case SqlDbType.NText:
                    toReturn = typeof(String);
                    break;
                case SqlDbType.NVarChar:
                    toReturn = typeof(String);
                    break;
                case SqlDbType.Real:
                    toReturn = typeof(Single);
                    break;
                case SqlDbType.SmallDateTime:
                    toReturn = typeof(DateTime);
                    break;
                case SqlDbType.SmallInt:
                    toReturn = typeof(Int16);
                    break;
                case SqlDbType.SmallMoney:
                    toReturn = typeof(Decimal);
                    break;
                case SqlDbType.Variant:
                    toReturn = typeof(Object);
                    break;
                case SqlDbType.Text:
                    toReturn = typeof(String);
                    break;
                case SqlDbType.Timestamp:
                    toReturn = typeof(Byte[]);
                    break;
                case SqlDbType.TinyInt:
                    toReturn = typeof(Byte);
                    break;
                case SqlDbType.UniqueIdentifier:
                    toReturn = typeof(Guid);
                    break;
                case SqlDbType.Udt:
                    toReturn = typeof(Geospatial);
                    break;
                case SqlDbType.VarBinary:
                    toReturn = typeof(Byte[]);
                    break;
                case SqlDbType.VarChar:
                    toReturn = typeof(String);
                    break;                
            }

            return toReturn;
        }
        public static Type DbTypeToType(DbType dbType)
        {
            Type toReturn = typeof(DBNull);
            switch (dbType)
            {
                case DbType.String:
                    toReturn = typeof(string);
                    break;

                case DbType.UInt64:
                    toReturn = typeof(UInt64);
                    break;

                case DbType.Int64:
                    toReturn = typeof(Int64);
                    break;

                case DbType.Int32:
                    toReturn = typeof(Int32);
                    break;

                case DbType.UInt32:
                    toReturn = typeof(UInt32);
                    break;

                case DbType.Single:
                    toReturn = typeof(float);
                    break;

                case DbType.Date:
                    toReturn = typeof(DateTime);
                    break;

                case DbType.DateTime:
                    toReturn = typeof(DateTime);
                    break;
					
				case DbType.DateTime2:
                    toReturn = typeof(DateTime);
                    break;

                case DbType.Time:
                    toReturn = typeof(DateTime);
                    break;

                case DbType.StringFixedLength:
                    toReturn = typeof(string);
                    break;

                case DbType.UInt16:
                    toReturn = typeof(UInt16);
                    break;

                case DbType.Int16:
                    toReturn = typeof(Int16);
                    break;

                case DbType.SByte:
                    toReturn = typeof(byte);
                    break;

                case DbType.Object:
                    toReturn = typeof(object);
                    break;

                case DbType.AnsiString:
                    toReturn = typeof(string);
                    break;

                case DbType.AnsiStringFixedLength:
                    toReturn = typeof(string);
                    break;

                case DbType.VarNumeric:
                    toReturn = typeof(decimal);
                    break;

                case DbType.Currency:
                    toReturn = typeof(double);
                    break;

                case DbType.Binary:
                    toReturn = typeof(byte[]);
                    break;

                case DbType.Decimal:
                    toReturn = typeof(decimal);
                    break;

                case DbType.Double:
                    toReturn = typeof(Double);
                    break;

                case DbType.Guid:
                    toReturn = typeof(Guid);
                    break;

                case DbType.Boolean:
                    toReturn = typeof(bool);
                    break;
            }

            return toReturn;
        }
		public static void AddBlockToCache(string key, CacheItem data, IGxConnection con, int duration)
		{
			GXLogging.Debug(log, "AddBlockToCache SizeInBytes:'" + data.SizeInBytes + "', key:'" + key + "'" );
			con.ConnectionCache.IncStmtCachedCount();
			data.OriginalKey = key;
			CacheFactory.Instance.Set<CacheItem>(CacheFactory.CACHE_DB, key, data, duration);
		}
		public static String GetKeyStmtValues(GxParameterCollection parameters,
			string statement,bool isFetchOne)
		{
			StringBuilder s=new StringBuilder();
			if (parameters!=null)
			{
				for (int i=0; i<parameters.Count; i++)
				{
					IDbDataParameter p = (IDbDataParameter) parameters[i];
                    DbType pDbtype = p.DbType;
                    if (pDbtype == DbType.String || pDbtype == DbType.StringFixedLength ||
                        pDbtype == DbType.AnsiString || pDbtype == DbType.AnsiStringFixedLength)
					{
						s.Append( p.Value);
						s.Append( p.Value.ToString().Length);
					}
					else
					{
						s.Append( p.Value);
					}
					s.Append(',');
				}
			}
			s.Append(isFetchOne);
			s.Append(',');
			s.Append(statement);
			return s.ToString();
		}

	}

	public class GxSqlDataReader : GxDataReader
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.GxSqlDataReader));

		public GxSqlDataReader(IGxConnectionManager connManager, GxDataRecord dr, IGxConnection connection, GxParameterCollection parameters,
			string stmt, ushort fetchSize,bool forFirst, int handle, bool withCached, SlidingTime expiration, bool dynStmt):base(connManager, dr, connection, parameters,
			stmt, fetchSize, forFirst, handle, withCached, expiration, dynStmt)
		{}
		public override short GetInt16(int i)
		{
			readBytes += 2;
			try
			{
				Type type = reader.GetFieldType(i);
				if (type == typeof(int))
				{
					return Convert.ToInt16(reader.GetInt32(i));
				}
				else if (type == typeof(string))
				{
					return Convert.ToInt16(reader.GetDecimal(i));
				}
				else if (type == typeof(long))
				{
					return Convert.ToInt16(reader.GetInt64(i));
				}
				else if (type == typeof(float))
				{
					return Convert.ToInt16(reader.GetFloat(i));
				}
				else if (type == typeof(double))
				{
					return Convert.ToInt16(reader.GetDouble(i));
				}
				else if (type == typeof(decimal))
				{
					SqlDataReader sqlReader = reader as SqlDataReader;
					return GxSqlServer.ReadSQLDecimalToShort(sqlReader, i);
				}
				else
					return Convert.ToInt16(reader.GetValue(i));
			}
			catch
			{
				return reader.GetInt16(i);
			}
		}
		public override int GetInt32(int i)
		{
			readBytes += 4;
			try
			{
				Type type = reader.GetFieldType(i);
				if (type == typeof(int))
				{
					return Convert.ToInt32(reader.GetInt32(i));
				}
				else if (type == typeof(long))
				{
					return Convert.ToInt32(reader.GetInt64(i));
				}
				else if (type == typeof(float))
				{
					return Convert.ToInt32(reader.GetFloat(i));
				}
				else if (type == typeof(double))
				{
					return Convert.ToInt32(reader.GetDouble(i));
				}
				else if (type == typeof(decimal))
				{
					SqlDataReader sqlreader = reader as SqlDataReader;
					return GxSqlServer.ReadSQLDecimalToInt(sqlreader, i);
				}
				else
					return Convert.ToInt32(reader.GetValue(i));
			}
			catch (InvalidCastException)
			{
				return reader.GetInt32(i);
			}
		}
		public override decimal GetDecimal(int i)
		{
			readBytes += 12;
			try
			{
				return reader.GetDecimal(i);
			}
			catch (Exception ex1)
			{
				GXLogging.Warn(log, "GetDecimal Exception, parameter " + i + " type:" + reader.GetFieldType(i), ex1);
				Type type = reader.GetFieldType(i);
				if (type == typeof(int))
				{
					return Convert.ToDecimal(reader.GetInt32(i));
				}
				else if (type == typeof(long))
				{
					return Convert.ToDecimal(reader.GetInt64(i));
				}
				else if (type == typeof(float))
				{
					return Convert.ToDecimal(reader.GetFloat(i));
				}
				else if (type == typeof(double))
				{
					return Convert.ToDecimal(reader.GetDouble(i));
				}
				else if (type == typeof(decimal))
				{
					SqlDataReader sqlReader = reader as SqlDataReader;
					return GxSqlServer.ReadSQLDecimal(sqlReader, i);
				}
				else
					return Convert.ToDecimal(reader.GetValue(i));
			}
		}
	}
	public class GxSqlServer : GxDataRecord
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.GxSqlServer));

		private static int MAX_NET_DECIMAL_PRECISION = 28;
		private static int MAX_GX_DECIMAL_SCALE = 15;
		private static int MAX_NET_INT_PRECISION = 9;
		private static int MAX_NET_SHORT_PRECISION = 4;
		private int m_FailedConnections;
		private int m_FailedCreate;
		private int MAX_TRIES;
		private int MAX_CREATE_TRIES=3;
		private const int MILLISECONDS_BETWEEN_RETRY_ATTEMPTS=500;
		private const string MULTIPLE_DATAREADERS = "MultipleActiveResultSets";
		private bool multipleDatareadersEnabled;

        public override int GetCommandTimeout()
        {
            return base.GetCommandTimeout();
        }

		public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId, 
			string userPassword,string databaseName, string port, string schema, string extra, GxConnectionCache connectionCache)
		{
#if !NETCORE
			if (showPrompt)
			{
				GetConnectionDialogString(0);
			}
			else
#endif
			{
				if (m_connectionString == null)
					m_connectionString=BuildConnectionString(datasourceName, userId, userPassword, databaseName, port, schema, extra);
			}

			GXLogging.Debug(log, "Setting connectionString property ", ConnectionStringForLog);
			MssqlConnectionWrapper connection=new MssqlConnectionWrapper(m_connectionString,connectionCache, isolationLevel);

			m_FailedConnections = 0;
			m_FailedCreate=0;

			return connection;
		}
        public override bool AllowsDuplicateParameters
        {
            get
            {
                return false;
            }
        }
		public override IDbDataParameter CreateParameter()
		{
			return new SqlParameter();
		}
		public override IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec)
		{
			SqlParameter parm =new SqlParameter();
			SqlDbType type= GXTypeToSqlDbType(dbtype);
			parm.SqlDbType=type;
			parm.IsNullable=true;
			parm.Size = gxlength;
			if(type==SqlDbType.Decimal)
			{
				parm.Precision = (byte)gxlength;
				parm.Scale= (byte)gxdec;
			}
			
#if !NETCORE
			else if (type == SqlDbType.Udt)
			{
				parm.UdtTypeName = "Geography";                    
			}
#endif
			parm.ParameterName=name;
			return parm;
		}
		private SqlDbType GXTypeToSqlDbType(object type)
		{
			if (!(type is GXType))
				return (SqlDbType)type;

			switch (type)
			{
#if NETCORE
				case GXType.Geography:
				case GXType.Geoline:
				case GXType.Geopoint:
				case GXType.Geopolygon:
					return SqlDbType.NChar;
#else
				case GXType.Geography:
				case GXType.Geoline:
				case GXType.Geopoint:
				case GXType.Geopolygon:
					return SqlDbType.Udt;
#endif
				case GXType.Int16: return SqlDbType.SmallInt; 
				case GXType.Int32: return SqlDbType.Int;
				case GXType.Int64: return SqlDbType.BigInt;
				case GXType.Number: return SqlDbType.Float;
				case GXType.Decimal: return SqlDbType.Decimal; 
				case GXType.DateTime: return SqlDbType.DateTime;
				case GXType.DateTime2: return SqlDbType.DateTime2;
				case GXType.NChar: return SqlDbType.NChar;
				case GXType.NVarChar: return SqlDbType.NVarChar;
				case GXType.NText: return SqlDbType.NText;
				case GXType.Char: return SqlDbType.Char;
				case GXType.VarChar: return SqlDbType.VarChar;
				case GXType.Text: return SqlDbType.Text;
				case GXType.Date: return SqlDbType.DateTime;
				case GXType.Boolean: return SqlDbType.Bit;
				case GXType.Xml: return SqlDbType.Xml;
				case GXType.UniqueIdentifier: return SqlDbType.UniqueIdentifier;
				case GXType.Blob: return SqlDbType.VarBinary;
				case GXType.Image: return SqlDbType.Image;
				default: return SqlDbType.Char;
			}
		}
		internal override object CloneParameter(IDbDataParameter p)
		{
			return ((ICloneable)p).Clone();
		}
		protected override IDbCommand GetCachedCommand(IGxConnection con, string stmt)
		{
			if (multipleDatareadersEnabled)
			{
				
				return con.ConnectionCache.GetAvailablePreparedCommand(stmt);
			}
			else
			{
				return base.GetCachedCommand(con, stmt);
			}
		}
		public override DbDataAdapter CreateDataAdapeter()
        {
            return new SqlDataAdapter();
        }
        public override bool MultiThreadSafe
		{
			get
			{
				return false;
			}
		}
		public override IDataReader GetDataReader(
			IGxConnectionManager connManager,
			IGxConnection con, 
			GxParameterCollection parameters ,
			string stmt, ushort fetchSize, 
			bool forFirst, int handle, 
			bool cached, SlidingTime expiration,
			bool hasNested,
			bool dynStmt)
		{
		
			IDataReader idatareader;
			if (!hasNested || multipleDatareadersEnabled)//Client Cursor
			{
				idatareader= new GxSqlDataReader(connManager,this, con,parameters,stmt,fetchSize,forFirst,handle,cached,expiration,dynStmt);
			}
			else //Server Cursor
			{
				idatareader= new GxSqlCursorDataReader(connManager,this, con,parameters,stmt,fetchSize,forFirst,handle,cached,expiration,dynStmt); 
			}
			return idatareader;

		}

		public override bool IsBlobType(IDbDataParameter idbparameter)
		{
            SqlDbType type = ((SqlParameter)idbparameter).SqlDbType;
			return (type == SqlDbType.Image || type == SqlDbType.VarBinary);
		}

		public override string GetServerDateTimeStmt(IGxConnection connection)
		{
			return "SELECT GETDATE()";
		}
		public override string GetServerDateTimeStmtMs(IGxConnection connection)
		{
			return GetServerDateTimeStmt(connection);
		}
		public override string GetServerUserIdStmt()
		{
			return "SELECT SUSER_SNAME()";
		}
		public override string GetServerVersionStmt()
		{
			return "SELECT SERVERPROPERTY('ResourceVersion'), SERVERPROPERTY('productversion')";
		}
		public override void SetTimeout(IGxConnectionManager connManager, IGxConnection connection, int handle)
		{
			if (m_lockTimeout>0)
			{
				GXLogging.Debug(log, "Set Lock Timeout to " +m_lockTimeout/1000);
				IDbCommand cmd = GetCommand(connection,SetTimeoutSentence(m_lockTimeout), new GxParameterCollection());
				cmd.ExecuteNonQuery();
			}
		}

		public override string SetTimeoutSentence(long milliseconds)
		{
			return "SET LOCK_TIMEOUT " + milliseconds;
		}
		public override bool ProcessError( int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)
			
		{
			GXLogging.Debug(log, "ProcessError: dbmsErrorCode=" + dbmsErrorCode +", emsg '"+ emsg + "'");
			switch (dbmsErrorCode)
			{
				case 1801: //Database '%.*ls' already exists. 
				case 15032: //The database '%s' already exists.
					break;
                case 20:    /*The instance of SQL Server you attempted to connect to does not support encryption. (PMcE: amazingly, this is transient)*/
                case 64:    /*A connection was successfully established with the server, but then an error occurred during the login process.*/
                case 233:   /*The client was unable to establish a connection because of an error during connection initialization process before login*/
                case 10053: /*A transport-level error has occurred when receiving results from the server.*/
                case 10060: /*A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible.*/
                case 40143: /*The service has encountered an error processing your request. Please try again.*/
                case 40197: /*The service has encountered an error processing your request. Please try again.*/
                case 40501: /*The service is currently busy. Retry the request after 10 seconds.*/
                case 40613: /*Database '%.*ls' on server '%.*ls' is not currently available. Please retry the connection later.*/
				case 53:/*A network-related or instance-specific error occurred while establishing a connection to SQL Server*/
                case 11:
				case 121: /*A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - The semaphore timeout period has expired.*/
				case 10054://A transport-level error has occurred when sending the request to the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)
				case 6404: //The connection is broken and recovery is not possible.  The connection is marked by the server as unrecoverable.  No attempt was made to restore the connection.
					if (con!=null && m_FailedConnections < MAX_TRIES)//Si es una operacion Open se reintenta.
					{
                        try
                        {
                            con.Close();
                        }
                        catch { }
						status=104; // General network error.  Check your network documentation - Retry [Max Pool Size] times.
						m_FailedConnections++;
						Thread.Sleep(MILLISECONDS_BETWEEN_RETRY_ATTEMPTS);
						retry=true;
						GXLogging.Debug(log, "ProcessError: General network error, FailedConnections:" + m_FailedConnections);
					}
					else
					{
						return false;
					}
					break;
				case 1807://Could not obtain exclusive lock on database 'model'. Retry the operation later.
					if (GxContext.isReorganization && con != null && m_FailedCreate < MAX_CREATE_TRIES)
					{
						try
						{
							con.Close();
						}
						catch { }
						status = 104; 
						m_FailedCreate++;
						Thread.Sleep(MILLISECONDS_BETWEEN_RETRY_ATTEMPTS);
						retry = true;
						GXLogging.Debug(log, "ProcessError: ould not obtain exclusive lock on database, FailedCreate:" + m_FailedCreate);
					}
					else
					{
						return false;
					}
					break;
				case 903:		// Locked
				case 1222:
					retry = Retry(errMask, retryCount);
					if (retry)
						status=110;// Locked - Retry
					else 
						status=103;//Locked
					return retry;
				case 2601:		// Duplicated record
				case 2627:		// Duplicated record
					status = 1; 
					break;
				case 3701:		// File not found
				case 3703:		// File not found
				case 3704:		// File not found
				case 3731:		// File not found
				case 4902:		// File not found
				case 3727:		// File not found
				case 3728:		// File not found
					status = 105; 
					break;
				case 503:		// Parent key not found
				case 547:		//conflicted with COLUMN FOREIGN KEY constraint
					if ((errMask & GxErrorMask.GX_MASKFOREIGNKEY) == 0)
					{
						status = 500;		// ForeignKeyError
						return false;
					}
					break;
				default:
					status = 999;
					return false;
			}
			return true;
		}
		public override string ToDbmsConstant(DateTime Value)
		{
			if (Value == System.DateTime.MinValue) 
				Value = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
			return "'" + Value.ToString("yyyy-MM-dd HH\\:mm\\:ss").Replace("'","''") + "'";
		}

		public override IGeographicNative GetGeospatial(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			if (!cmd.HasMoreRows || DR == null || DR.IsDBNull(i) || DR.GetValue(i) == null)
				return new Geospatial();
			else
			{
				Geospatial gtmp = new Geospatial();
				gtmp.FromString(DR.GetValue(i).ToString());
				return gtmp;
			}
		}

		public override void GetValues(IDataReader reader, ref object[] values)
		{
			try
			{
				SqlDataReader sqlReader = reader as SqlDataReader;
				for (int i = 0; i < sqlReader.FieldCount; i++)
				{
					try
					{
						values[i] = reader.GetValue(i);
					}
					catch (OverflowException oex)
					{
						GXLogging.Warn(log, "GetValues OverflowException:" + oex);
						if (sqlReader.GetFieldType(i) == typeof(decimal))
						{
							GXLogging.Debug(log, "GetValues fieldtype decimal value:" + sqlReader.GetSqlDecimal(i).ToString());
							values[i] = ReadSQLDecimal(sqlReader, i);

						}
						else
							throw oex;
					}
					catch (Exception ex)
					{
#if !NETCORE
						FileNotFoundException fex = ex as FileNotFoundException;
						FileLoadException flex = ex as FileLoadException;
						if (flex !=null || fex != null)
						{
							GXLogging.Warn(log, "GetValues Error " + ex);
							if ((flex!=null && flex.FileName.StartsWith(SQLGeographyWrapper.SqlGeographyAssemby, StringComparison.OrdinalIgnoreCase))//geography type
							|| (fex != null && fex.FileName.StartsWith(SQLGeographyWrapper.SqlGeographyAssemby, StringComparison.OrdinalIgnoreCase)))//Could not load file or assembly Microsoft.SqlServer.Types
							{
								values[i] = SQLGeographyWrapper.Deserialize(sqlReader.GetSqlBytes(i));
							}
						}
						else
						{
							throw ex;
						}
#else
						throw ex;
#endif

					}
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "GetValues error", ex);
			}
		}
		static internal decimal ReadSQLDecimal(SqlDataReader sqlReader, int idx){
			//Reduce the precision
			//The SQlServer data type NUMBER can hold up to 38 precision, and the .NET Decimal type can hold up to 28 precision
			SqlDecimal sqldecimal = sqlReader.GetSqlDecimal(idx);
			if (sqldecimal.Precision > MAX_NET_DECIMAL_PRECISION)
				return SqlDecimal.ConvertToPrecScale(sqldecimal, MAX_NET_DECIMAL_PRECISION, MAX_GX_DECIMAL_SCALE).Value;
			else
				return sqldecimal.Value;
		}
		static internal Int32 ReadSQLDecimalToInt(SqlDataReader sqlReader, int idx)
		{
			try
			{
				return Convert.ToInt32(sqlReader.GetDecimal(idx));
			}
			catch (OverflowException oex)
			{
				GXLogging.Warn(log, "ReadSQLDecimalToInt OverflowException:" + oex);
				SqlDecimal sqldecimal = sqlReader.GetSqlDecimal(idx);
				if (sqldecimal.Precision > MAX_NET_INT_PRECISION)
					return Convert.ToInt32(SqlDecimal.ConvertToPrecScale(sqlReader.GetSqlDecimal(idx), MAX_NET_INT_PRECISION, 0).Value);
				else
					return Convert.ToInt32(sqldecimal.Value);
			}
		}
		static internal Int16 ReadSQLDecimalToShort(SqlDataReader sqlReader, int idx)
		{
			try
			{
				return Convert.ToInt16(sqlReader.GetDecimal(idx));
			}
			catch (OverflowException oex)
			{
				GXLogging.Warn(log, "ReadSQLDecimalToShort OverflowException:" + oex);
				SqlDecimal sqldecimal = sqlReader.GetSqlDecimal(idx);
				if (sqldecimal.Precision > MAX_NET_SHORT_PRECISION)
					return Convert.ToInt16(SqlDecimal.ConvertToPrecScale(sqlReader.GetSqlDecimal(idx), MAX_NET_SHORT_PRECISION, 0).Value);
				else
					return Convert.ToInt16(sqldecimal.Value);
			}
		}
		protected override string BuildConnectionString(string datasourceName, string userId, 
			string userPassword,string databaseName, string port, string schema, string extra)
		{
			StringBuilder connectionString = new StringBuilder();
			string port1 = port!=null ? port.Trim() : "";
			if (!string.IsNullOrEmpty(datasourceName) && port1.Length > 0 && !hasKey(extra, "Data Source"))
			{
				connectionString.AppendFormat("Data Source={0},{1};",datasourceName, port1);
			}
			else if (!string.IsNullOrEmpty(datasourceName) && !hasKey(extra, "Data Source"))
			{
				connectionString.AppendFormat("Data Source={0};",datasourceName);
			}
			if (userId!=null)
			{
				connectionString.AppendFormat(";User ID={0};Password={1}",userId,userPassword);
			}
			if (databaseName != null && databaseName.Trim().Length > 0 && !hasKey(extra, "Database"))
			{
				connectionString.AppendFormat(";Database={0}",databaseName);
			}
			if (!string.IsNullOrEmpty(extra))
			{
				if (!extra.StartsWith(";"))	connectionString.Append(";");
				connectionString.Append(extra);

				if (GetParameterValue(extra, MULTIPLE_DATAREADERS).Equals("true", StringComparison.OrdinalIgnoreCase))
					multipleDatareadersEnabled = true;
			}

			string connstr = connectionString.ToString();
            string maxpoolSize = GetParameterValue(connstr, "Max Pool Size");
			string pooling = GetParameterValue(connstr, "Pooling");
			if (!String.IsNullOrEmpty(pooling) && (pooling.Equals("false", StringComparison.OrdinalIgnoreCase) || pooling.Equals("no", StringComparison.OrdinalIgnoreCase)))
				MAX_TRIES = 0;
			else if (String.IsNullOrEmpty(maxpoolSize)) 
				MAX_TRIES = 100; //Max Pool Size default de ado.net
			else 
				MAX_TRIES = Convert.ToInt32(maxpoolSize);
			GXLogging.Debug(log, "MAX_TRIES=" + MAX_TRIES);

			return connstr;

		}

#if !NETCORE
		public override Object Net2DbmsGeo(GXType type, IGeographicNative geo)
        {

            return geo.InnerValue;
        }
#else
		public override Object Net2DbmsGeo(GXType type, IGeographicNative geo)
        {
            return geo.ToStringSQL("GEOMETRYCOLLECTION EMPTY");
        }
#endif

		public override IGeographicNative Dbms2NetGeo(IGxDbCommand cmd, IDataRecord DR, int i)
        {
            return new Geospatial(DR.GetValue(i));
        }

		public override DateTime Dbms2NetDate(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return Dbms2NetDateTime(DR.GetDateTime(i), false);
		}

		public override DateTime Dbms2NetDateTime( DateTime dt, Boolean precision)
		{
			//DBMS MinDate => Genexus null Date
			if (dt.Equals(SQLSERVER_NULL_DATE))
			{
				return DateTimeUtil.NullDate();
			}

			if (dt.Year==SQLSERVER_NULL_DATE.Year &&
				dt.Month==SQLSERVER_NULL_DATE.Month &&
				dt.Day==SQLSERVER_NULL_DATE.Day)
			{
				
				return 	new DateTime(
					DateTimeUtil.NullDate().Year,DateTimeUtil.NullDate().Month,
					DateTimeUtil.NullDate().Day,dt.Hour,dt.Minute,dt.Second, ((precision)? dt.Millisecond :0));
			}
			return (precision) ? DateTimeUtil.ResetMicroseconds(dt) : DateTimeUtil.ResetMilliseconds(dt);
		}

    
		public override Object Net2DbmsDateTime(IDbDataParameter parm, DateTime dt)
		{
			//Genexus null => save DBMS MinDate
			if(dt.Equals(DateTimeUtil.NullDate()))
			{
				return SQLSERVER_NULL_DATE;
			}

			//Date < DBMS MinDate => save DBMS MinDate keeping the Time component
			if (dt.CompareTo(SQLSERVER_NULL_DATE)<0)
			{
				DateTime aux = 
					new DateTime(
					SQLSERVER_NULL_DATE.Year,SQLSERVER_NULL_DATE.Month,
					SQLSERVER_NULL_DATE.Day,dt.Hour,dt.Minute,dt.Second,dt.Millisecond);
				
				return aux;
			}
			else
			{
				return dt;
			}
		}

		private static readonly string[] ConcatOpValues = new string[] { string.Empty, " + ", string.Empty };
		public override string ConcatOp(int pos)
		{
			return ConcatOpValues[pos];
		}

		static DateTime SQLSERVER_NULL_DATE = new DateTime(1753,1,1) ;
	}
	
	public class GxDataReader: IDataReader
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.GxDataReader));
		protected IDataReader reader;
		protected GxParameterCollection parameters;
		protected int fetchSize;
		protected string stmt;
		protected GxConnectionCache cache;
		protected IGxConnection con;
		protected GxArrayList block;
		protected string key;
		protected int pos;
		protected bool cached;
		protected bool open;
		protected bool isForFirst;
		protected int handle;
		protected SlidingTime expiration;
		protected IGxConnectionManager _connManager;
		protected GxDataRecord m_dr;
		protected long readBytes;
		protected bool dynStmt;

		protected GxDataReader(){}
		public GxDataReader( IGxConnectionManager connManager, GxDataRecord dr, IGxConnection connection, GxParameterCollection parameters ,
			string stmt, int fetchSize, bool forFirst, int handle, bool cached, SlidingTime expiration, bool dynStmt)
		{
			this.parameters=parameters;
			this.stmt=stmt;
			this.fetchSize=fetchSize;
			this.cache=connection.ConnectionCache;
			this.cached=cached;
			this.handle=handle;
			this.isForFirst=forFirst;
			_connManager = connManager;
			this.m_dr=dr;
			this.dynStmt=dynStmt;
			con = _connManager.IncOpenHandles(handle, m_dr.DataSource);
			con.CurrentStmt=stmt;
			con.MonitorEnter();
			GXLogging.Debug(log, "Open GxDataReader handle:'" + handle );
			reader=dr.GetCommand(con,stmt,parameters).ExecuteReader();
			cache.SetAvailableCommand(stmt, false, dynStmt); 
			open=true;
			block=new GxArrayList(fetchSize);
			pos=-1;
            if (cached)
            {
                key = SqlUtil.GetKeyStmtValues(parameters, stmt, isForFirst);
                this.expiration = expiration;
            }
		}

		public void AddToCache(bool hasNext)
		{
			if (hasNext)
			{
				object[] values = new object[reader.FieldCount];
				m_dr.GetValues(reader, ref values);
				block.Add(values);
			}
			else
			{
				SqlUtil.AddBlockToCache(key, new CacheItem(block, false, pos, readBytes), con, expiration != null ? (int)expiration.ItemSlidingExpiration.TotalMinutes : 0);
				Close();
			}
		}

		public bool Read()
		{
			try
			{
				bool res = false;
				++pos;

				if (!reader.IsClosed)
				{
					res = reader.Read();
				}
				if (cached)
				{
					AddToCache(res);
				}
				else if (!res)
				{
					Close();
				}
				return res;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "ReadError", ex);
				throw (new GxADODataException(ex));
			}
		}
		public DateTime GetDateTime(int i)
		{
			DateTime res; 
			readBytes += 8;
			try
			{
				res = reader.GetDateTime(i);				
			}
			catch (Exception)
			{				
				res = Convert.ToDateTime(reader.GetValue(i));
			}
			return res;
		}
		public virtual decimal GetDecimal(int i	)
		{
			readBytes += 12;
			try
			{
				return Convert.ToDecimal(reader.GetValue(i),System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				
			}
			catch(Exception ex)
			{
				GXLogging.Warn(log, "GetDecimal Exception, parameter " + i + " ", ex);
				try
				{
					return reader.GetDecimal(i);
				}
				catch (Exception ex1)
				{
					GXLogging.Warn(log, "GetDecimal Exception", ex1);
					return reader.GetInt64(i);
				}
			}
		}
		public double GetDouble(int i)
		{
			readBytes += 8;
			return Convert.ToDouble(reader.GetValue(i));
		}
		public virtual short GetInt16(int i)
		{
			readBytes += 2;
			try
			{
				return Convert.ToInt16(reader.GetValue(i));
			}
			catch
			{
				Type type = reader.GetFieldType(i);
				if (type == typeof(int))
				{
					return Convert.ToInt16(reader.GetInt32(i));
				}
				else if (type == typeof(string))
				{
					return Convert.ToInt16(reader.GetDecimal(i));
				}
				else if (type == typeof(long))
				{
					return Convert.ToInt16(reader.GetInt64(i));
				}
				else if (type == typeof(float))
				{
					return Convert.ToInt16(reader.GetFloat(i));
				}
				else if (type == typeof(double))
				{
					return Convert.ToInt16(reader.GetDouble(i));
				}
				else if (type == typeof(decimal))
				{
					return Convert.ToInt16(reader.GetDecimal(i));
				}
				else
					return Convert.ToInt16(reader.GetValue(i));
			}
		}
		public virtual int GetInt32(int i)
		{
			readBytes += 4;
			try
			{
				return Convert.ToInt32(reader.GetValue(i));
			}
			catch(InvalidCastException ex1)
			{
				
				Type type = reader.GetFieldType(i);
				GXLogging.Warn(log, "GetInt32 Exception field type:" + type + " value:" + reader.GetValue(i), ex1);
				try
				{
					if (type == typeof(int))
					{
						return Convert.ToInt32(reader.GetInt32(i));
					}
					else if (type == typeof(long))
					{
						return Convert.ToInt32(reader.GetInt64(i));
					}
					else if (type == typeof(float))
					{
						return Convert.ToInt32(reader.GetFloat(i));
					}
					else if (type == typeof(double))
					{
						return Convert.ToInt32(reader.GetDouble(i));
					}
					else if (type == typeof(decimal))
					{
						return Convert.ToInt32(reader.GetDecimal(i));
					}
					else
						return Convert.ToInt32(reader.GetInt64(i));
				}
				catch (InvalidCastException ex2)
				{
					GXLogging.Warn(log, "GetInt32 Exception", ex2);
					return Convert.ToInt32(reader.GetValue(i));
				}
			}
		}
		public virtual long GetInt64(int i)
		{
			readBytes += 8;
			try
			{
				return Convert.ToInt64(reader.GetValue(i)); 
				
			}
			catch(Exception)
			{
				
				return Convert.ToInt64(reader.GetDecimal(i));
			}
			
		}
		public virtual string GetString(int i)
		{
			try
			{
				string res = reader.GetString(i);
				readBytes += 10 + (2 * res.Length);
				return res;
			}
			catch (Exception e) 
			{
				GXLogging.Warn(log, "GetString Exception", e);
				GXLogging.Warn(log, "Reader.GetType(" + i + "):" + reader.GetFieldType(i));
				return Convert.ToString(reader.GetValue(i));
			}
		}
		public virtual object GetValue(int i)
		{
			return reader.GetValue(i);
		}
		public bool IsDBNull(int i)
		{
			return reader.IsDBNull(i);
		}
		public char GetChar(int i)
		{
			throw (new GxNotImplementedException());
		}
		public IDataReader GetData(int i)
		{
			throw (new GxNotImplementedException());
		}
		public string GetDataTypeName( int i)
		{
			throw (new GxNotImplementedException());
		}
		public void Close( )
		{
			if (open)
			{
				GXLogging.Debug(log, "Close GxDataReader, open:" + open);
				reader.Close();

				cache.SetAvailableCommand(stmt, true, dynStmt);

				_connManager.DecOpenHandles(handle, m_dr.DataSource);
				con.CurrentStmt=null;
				open=false;
				con.MonitorExit();

			}
			if (reader!=null) reader.Dispose(); 

		}
		public bool NextResult()
		{
			throw (new GxNotImplementedException());
		}
		public int RecordsAffected 
		{
			get { throw (new GxNotImplementedException()); }
		}
		public int FieldCount 
		{
			get	{ throw (new GxNotImplementedException());;}
		}
		public bool GetBoolean( int i)
		{
            readBytes += 1;
            return reader.GetBoolean(i);
		}
        public virtual Guid GetGuid(int i)
        {
            readBytes += 16;
            return reader.GetGuid(i);
        }
        public IGeographicNative GetGeospatial(int i)
        {
            return new Geospatial(GetValue(i));
        }
		public byte GetByte( int i)
		{
			throw (new GxNotImplementedException());
		}
		public virtual long GetBytes( int i, long fieldOffset, byte[] buffer, int bufferoffset, int length	)
		{
			long res = reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
			readBytes += res;
			return res;
		}
		public long GetChars( int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw (new GxNotImplementedException());
		}
		public DataTable GetSchemaTable()
		{	
			throw (new GxNotImplementedException());
		}
		public bool IsClosed 
		{
			get{return !open;}
		}
		public int Depth 
		{
			get {return 0;}
		}
		public void Dispose()
		{
		}
		public string GetName(int i)
		{
			throw (new GxNotImplementedException());
		}

		public Type GetFieldType(int i)
		{
			throw (new GxNotImplementedException());
		}
		public float GetFloat(int i)
		{
			throw (new GxNotImplementedException());
		}
		public object this[string name] 
		{
			get{ throw (new GxNotImplementedException());}
		}
		public object this[int i]
		{
			get { throw (new GxNotImplementedException());}
		}
		public int GetValues(object[] values)
		{
			throw (new GxNotImplementedException());
		}
		public int GetOrdinal(string name)
		{
			throw (new GxNotImplementedException());
		}

	}


	public class GxConnectionCache 
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.GxConnectionCache));
		private static  int MAX_SIZE=Preferences.GetMaximumOpenCursors();
		
		private SqlCommand fetchCommand;
		private SqlCommand prepExecCommand;
		private SqlCommand execCommand;
		private SqlCommand prepareCommand;
		private SqlCommand unprepareCommand;
		private SqlCommand closeCommand;
		
		private SqlParameter []prepExecParms;
		private SqlParameter []execParms;
		private SqlParameter []prepareParms;

		//Statements Cache
		[System.Diagnostics.CodeAnalysis.SuppressMessage("GxFxCopRules", "CR1000:EnforceThreadSafeType")]
		private Dictionary<int, GxParameterCollection> parametersCache = new Dictionary<int, GxParameterCollection>();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("GxFxCopRules", "CR1000:EnforceThreadSafeType")]
		private Dictionary<string, GxItemCommand> preparedCommandsCache; //Prepared Commands cache
		private GxPreparedStatementCache preparedStmtCache; 

		[System.Diagnostics.CodeAnalysis.SuppressMessage("GxFxCopRules", "CR1000:EnforceThreadSafeType")]
		private Dictionary<string, DbDataAdapterElem> dataAdapterCache = new Dictionary<string, DbDataAdapterElem>();
        private List<DbDataAdapterElem> dataAdapterCacheQueue;

		private Stack<string> stmtsToRemove; 
		private Stack<string> commandsToRemove;

		private int totalCachedCursors;

		private SqlCommand spidCommand;

		private IGxConnection conn;

		private bool isPrepFetch;
		private bool isPrepUnprepare;
		private bool isPrepClose;


		private int stmtCachedCount;

		public int StmtCachedCount
		{
			get{return stmtCachedCount;}
		}

		public void IncStmtCachedCount()
		{
			stmtCachedCount++;
		}
		public void PrepareFetch()
		{
			if(!isPrepFetch)
			{
				fetchCommand.Prepare();
				isPrepFetch=true;
			}
		}
		public void PrepareUnprepare()
		{
			if(!isPrepUnprepare)
			{
				unprepareCommand.Prepare();
				isPrepUnprepare=true;
			}
		}
		public void PrepareClose()
		{
			if(!isPrepClose)
			{
				closeCommand.Prepare();
				isPrepClose=true;
			}
		}

		public GxConnectionCache(IGxConnection gxconn,int maxSize)
		{
			preparedCommandsCache = new Dictionary<string, GxItemCommand>(MAX_SIZE);
			InitCommands();
			preparedStmtCache=new GxPreparedStatementCache(gxconn,maxSize);
			conn=gxconn;
            dataAdapterCacheQueue = new List<DbDataAdapterElem>();

		}
        public GxConnectionCache(IGxConnection gxconn)
        {
			preparedCommandsCache = new Dictionary<string, GxItemCommand>(MAX_SIZE);
            preparedStmtCache = new GxPreparedStatementCache(gxconn, MAX_SIZE);
            InitCommands();
            conn = gxconn;
            dataAdapterCacheQueue = new List<DbDataAdapterElem>();
        }
		public void AddParameter(int cursorId,  GxParameterCollection parameters)
		{
			GxParameterCollection parmInCache = new GxParameterCollection();
			for (int i=0; i<parameters.Count; i++)
				parmInCache.Add(parameters[i]);
			parametersCache[cursorId] = parmInCache;
		}

		public void AddPreparedStmt(string stmt, int cursorId)
		{
			GxItemStmt s;;
			if (!preparedStmtCache.TryGetValue(stmt, out s))
			{
				GXLogging.Info(log, "AddPreparedStmt, totalCachedStmtCursors:" + totalCachedCursors + ", cursorId: " + cursorId + ", stmt:" + stmt);
				totalCachedCursors++;
				CheckCacheSize();

				s=new GxItemStmt();
				s.opened = 0;
			}
			else
			{
				GXLogging.Debug(log, "AddPreparedStmt, cursor exists, totalCachedStmtCursors:" + totalCachedCursors + ",stmt:" + stmt);
			}
				
			s.cursorId = cursorId;
			preparedStmtCache[stmt]=s;

		}
        public void AddDataAdapter(string stmt, DbDataAdapterElem adapter)
        {
			if (!dataAdapterCache.ContainsKey(stmt))
			{
				dataAdapterCache.Add(stmt, adapter);
				dataAdapterCacheQueue.Add(adapter);
			}
        }

		public void AddPreparedCommand(string  stmt, IDbCommand cmd)
		{
			GxItemCommand s;
			if (!preparedCommandsCache.TryGetValue(stmt, out s))
			{
				totalCachedCursors++;
				CheckCacheSize();

				s=new GxItemCommand();
				s.opened = 0;
			}
		
			s.command = cmd;
			preparedCommandsCache[stmt]=s;

		}

		private void CheckCacheStmtSize()
		{

			if (totalCachedCursors >= MAX_SIZE)
			{

				GXLogging.Debug(log, "CheckCacheStmtSize, totalCachedCursors:" + totalCachedCursors  + ",MAX_SIZE:" + MAX_SIZE);
				List<string> toRemove = new List<string>();

				//Remove server cursors (sql server) that are prepared and closed until they are below the limit
				IEnumerator e= preparedStmtCache.Keys.GetEnumerator();
				while (e.MoveNext() &&  totalCachedCursors >= MAX_SIZE )
				{
					string key = (string)e.Current;
					GxItemStmt c = preparedStmtCache[key];
					if (c.opened <= 0)
					{
						toRemove.Add(key);
						preparedStmtCache.unprepare(c.cursorId);
						totalCachedCursors--;
					}
				}

				foreach (string s in toRemove)
				{
					GXLogging.Debug(log, "CheckCacheStmtSize, totalCachedCursors:" + totalCachedCursors + ", removed: " +s );
					preparedStmtCache.Remove(s);
				}
				toRemove.Clear();
			}

		}


		private void CheckCacheCommandSize()
		{
			if (totalCachedCursors >= MAX_SIZE)
			{
				if (totalCachedCursors >= MAX_SIZE)
				{
					GXLogging.Debug(log, "CheckCacheSize cmdcache.Count:" + preparedCommandsCache.Count + ", stmtcache.Count:" + preparedStmtCache.Count);
				}
				if (preparedCommandsCache.Count >= preparedStmtCache.Count)
				{
					RemoveCommands();
					RemoveStmts();
				}
				else
				{
					RemoveStmts();
					RemoveCommands();
				}
			}

			if (totalCachedCursors >= MAX_SIZE)
			{
				GXLogging.Warn(log, "Expanding prepared statement cache to " + totalCachedCursors);
			}

		}


		private void RemoveStmts()
		{
			if (stmtsToRemove==null) stmtsToRemove= new Stack<string>();
			RefreshToRemove(stmtsToRemove, preparedStmtCache);

			while (stmtsToRemove.Count >0 && totalCachedCursors >= MAX_SIZE)
			{
				string stmt = (string)stmtsToRemove.Pop();
				GxItemStmt c;

				if (preparedStmtCache.TryGetValue(stmt, out c) && c.opened <= 0)
				{
					preparedStmtCache.unprepare(c.cursorId);
                    parametersCache.Remove(c.cursorId);
					totalCachedCursors--;
					preparedStmtCache.Remove(stmt);
					GXLogging.Debug(log, "RemoveStmts, totalCachedCursors:" + totalCachedCursors + ", removed: " +stmt );
				}
			}
			GXLogging.Debug(log, "stmtsToRemove.Count:" + stmtsToRemove.Count);

		}
		private void RemoveCommands()
		{
			if (commandsToRemove==null) commandsToRemove= new Stack<string>();

			RefreshToRemove(commandsToRemove, preparedCommandsCache);
			GxItemCommand c;

			while (commandsToRemove.Count >0 && totalCachedCursors >= MAX_SIZE)
			{
				string stmt = (string)commandsToRemove.Pop();

				if (preparedCommandsCache.TryGetValue(stmt,out c) && c.opened <= 0)
				{
					totalCachedCursors--;
					conn.DataRecord.DisposeCommand(c.command);
					preparedCommandsCache.Remove(stmt);
					GXLogging.Debug(log, "RemoveCommands, totalCachedCursors:" + totalCachedCursors + ", removed: " +stmt );
				}
			}
			GXLogging.Debug(log, "commandsToRemove.Count:" + commandsToRemove.Count);
		}

		private void RefreshToRemove(Stack<string> toRemove, IDictionary cache)
		{
			if(toRemove!=null && toRemove.Count>0) return;

			GXLogging.Debug(log, "RefreshToRemove" );
			IEnumerator e = cache.Keys.GetEnumerator();
			while (e.MoveNext())
			{
				string stmt =  (string)e.Current;
				GxCursorBase c = (GxCursorBase) cache[stmt];
				if (c.opened <= 0)
				{
					toRemove.Push(stmt);
				}
			}
			GXLogging.Debug(log, "toRemove.Count:" + toRemove.Count);
		}

		
		private void CheckCacheSize()
		{

			if (preparedCommandsCache.Count > preparedStmtCache.Count)
			{
				CheckCacheCommandSize();
				CheckCacheStmtSize();
			}
			else
			{
				CheckCacheStmtSize();
				CheckCacheCommandSize();
			}

			if (totalCachedCursors >= MAX_SIZE)
			{
				GXLogging.Warn(log, "Expanding prepared statement pool to " + totalCachedCursors);
			}

		}


		public object GetParameters(int cursorId)
		{
			return parametersCache[cursorId];
		}

		public object GetPreparedStmt(string stmt)
		{
			GxItemStmt stmto;
			if (preparedStmtCache.TryGetValue(stmt, out stmto))
			{
               GXLogging.Debug(log, "GetPreparedStmt cached stmt:" + stmt + ", ((GxItemStmt)o).opened: " + stmto.opened);
                return stmto.cursorId;
			}
			else
			{
				GXLogging.Debug(log, "GetPreparedStmt stmt:", stmt, " null");
				return null;
			}
		}
        public ICollection GetDataAdapters()
        {
            return dataAdapterCacheQueue;
        }
        public DbDataAdapterElem GetDataAdapter(string stmt)
        {
			if (dataAdapterCache.ContainsKey(stmt))
				return (DbDataAdapterElem)dataAdapterCache[stmt];
			else
				return null;
        }
		public IDbCommand GetPreparedCommand(string  stmt)
		{
			GxItemCommand o;
			if (preparedCommandsCache.TryGetValue(stmt, out o))
			{
				GXLogging.Debug(log, "GetPreparedCommand cached stmt:", stmt );
				return o.command;
			}
			else
			{
				GXLogging.Debug(log, "GetPreparedCommand   stmt:", stmt);
				return null;
			}
		}

		public IDbCommand GetAvailablePreparedCommand(string  stmt)
		{
			GxItemCommand c;
			if (preparedCommandsCache.TryGetValue(stmt, out c))
			{
				if (c.opened<=0)
				{
					GXLogging.Debug(log, "GetAvailablePreparedCommand cached stmt:", stmt );
					
					return c.command;
				}
			}
			GXLogging.Debug(log, "GetAvailablePreparedCommand null stmt:", stmt );
			return null;
		}

        public void SetAvailableStmt(string stmt, bool available, bool dynstmt)
        {
            GxItemStmt c;
            if (preparedStmtCache.TryGetValue(stmt, out c))
            {
                if (available)
                {
                    c.opened--;
                    GXLogging.Debug(log, "SetAvailableStmt stmt: " + stmt + ",c.opened:" + ((GxItemStmt)preparedStmtCache[stmt]).opened + "=" + c.opened);

                }
                else
                {
                    c.opened++;
                }
            }
        }

		public void SetAvailableCommand(string stmt, bool available, bool dynstmt)
		{
            GxItemCommand c;
            if (preparedCommandsCache.TryGetValue(stmt, out c))
            {
                if (available)
                {
                    c.opened--;
                    GXLogging.Debug(log, "SetAvailableCommand stmt: " + stmt + ",c.opened:" + (preparedCommandsCache[stmt]).opened + "=" + c.opened);
                }
                else
                {
                    c.opened++;
                }
            }
		}

		public int CountPreparedCommand()
		{
			return preparedCommandsCache.Count;
		}

		public int CountPreparedStmt()
		{
			return preparedStmtCache.Count;
		}

		public bool ContainsPreparedStmt(string stmt)
		{
			return preparedStmtCache.ContainsKey(stmt);
		}

		public SqlCommand FetchCommand
		{
			get{ return fetchCommand;}
		}
		public SqlCommand PrepExecCommand
		{
			get{return prepExecCommand;}
		}
		public SqlCommand ExecCommand
		{
			get{return execCommand;}
		}
		public SqlCommand PrepareCommand
		{
			get{return prepareCommand;}
		}
		public SqlCommand UnprepareCommand
		{
			get{return unprepareCommand;}
		}
		public IDbCommand SPIDCommand
		{
			get{
				if (spidCommand == null)
				{
					spidCommand = new SqlCommand("SELECT @@SPID");
				}
				return spidCommand;
			}
		}
		public SqlCommand CloseCommand
		{
			get{return closeCommand;}
		}
        public ICollection PrepExecParms
		{
			get{return prepExecParms;}
		}
		public ICollection ExecParms
		{
			get{return execParms;}
		}
        public ICollection PrepareParms
		{
			get{return prepareParms;}
		}
		public void Clear()
		{
			preparedStmtCache.UnprepareClear();

			IEnumerator values = preparedCommandsCache.Values.GetEnumerator();
			while (values.MoveNext())
			{
				this.conn.DataRecord.DisposeCommand(((GxItemCommand)values.Current).command);
			}
			preparedCommandsCache.Clear();
			totalCachedCursors=0;
			
			parametersCache.Clear();

            dataAdapterCache.Clear();
            dataAdapterCacheQueue.Clear();
			if (stmtsToRemove!=null) stmtsToRemove.Clear(); 
			if (commandsToRemove!=null) commandsToRemove.Clear();
		}

		private void InitCommands()
		{
            int commandTimeout = 0;

			//----------------Fetch command
			fetchCommand=new SqlCommand("sp_cursorfetch");
			fetchCommand.CommandType=CommandType.StoredProcedure;
            fetchCommand.CommandTimeout = commandTimeout;
			SqlParameter p1 = fetchCommand.Parameters.Add("cursor", SqlDbType.Int);
			p1.Direction = ParameterDirection.Input;

			SqlParameter p2 = fetchCommand.Parameters.Add("fetchtype", SqlDbType.Int);
			p2.Direction = ParameterDirection.Input;
			p2.Value = 2;//0x0002  	Next row.

			SqlParameter p3 = fetchCommand.Parameters.Add("rownumber", SqlDbType.Int);
			p3.Direction = ParameterDirection.Input;
			p3.Value = 1;
			
			SqlParameter p5 = fetchCommand.Parameters.Add("nrows", SqlDbType.Int);
			p5.Direction = ParameterDirection.Input;

			//-----------------PrepExec command
			prepExecCommand=new SqlCommand("sp_cursorprepexec");
			prepExecCommand.CommandType=CommandType.StoredProcedure;
            prepExecCommand.CommandTimeout = commandTimeout;

			prepExecParms=new SqlParameter[7];
			prepExecParms[0]=new SqlParameter("cursor",SqlDbType.Int); 
			prepExecParms[0].Direction=ParameterDirection.Output;
			prepExecParms[0].Value=DBNull.Value;

			prepExecParms[1]=new SqlParameter("handle",SqlDbType.Int); 
			prepExecParms[1].Direction=ParameterDirection.Output;
			prepExecParms[1].Value=0;

			prepExecParms[2]=new SqlParameter("parameters",SqlDbType.NVarChar); 
			prepExecParms[2].Direction=ParameterDirection.Input;
			
			prepExecParms[3]=new SqlParameter("stmt",SqlDbType.NVarChar); 
			prepExecParms[3].Direction=ParameterDirection.Input;

			prepExecParms[4]=new SqlParameter("scrollopt",SqlDbType.Int);
			prepExecParms[4].Direction=ParameterDirection.InputOutput;

			prepExecParms[5]=new SqlParameter("ccopt",SqlDbType.Int);
			prepExecParms[5].Direction=ParameterDirection.InputOutput;
			prepExecParms[5].Value=1;//Readonly

			prepExecParms[6]=new SqlParameter("rowcount",SqlDbType.Int); 
			prepExecParms[6].Direction=ParameterDirection.InputOutput;

			//-----------------Exec command
			execCommand = new SqlCommand("sp_cursorexecute");
			execCommand.CommandType=CommandType.StoredProcedure;
            execCommand.CommandTimeout = commandTimeout;

			execParms=new SqlParameter[5];
			execParms[0]=new SqlParameter("cursorId",SqlDbType.Int);
			execParms[0].Direction = ParameterDirection.Input;
			execParms[1]=new SqlParameter("handle",SqlDbType.Int);
			execParms[1].Direction = ParameterDirection.Output;
			execParms[2]=new SqlParameter("scrollopt",SqlDbType.Int);
			execParms[2].Direction = ParameterDirection.InputOutput;
			execParms[3]=new SqlParameter("ccopt",SqlDbType.Int);
			execParms[3].Direction = ParameterDirection.InputOutput;
			execParms[3].Value=1;//Readonly
			execParms[4]=new SqlParameter("rowcount",SqlDbType.Int);
			execParms[4].Direction = ParameterDirection.InputOutput;

			//------------------prepareCursor
			prepareCommand = new SqlCommand("sp_cursorprepare");
			prepareCommand.CommandType=CommandType.StoredProcedure;
            prepareCommand.CommandTimeout = commandTimeout;
			prepareParms=new SqlParameter[6];
			
			prepareParms[0]=new SqlParameter("cursor",SqlDbType.Int);
			prepareParms[0].Direction = ParameterDirection.Output;
			prepareParms[0].Value=DBNull.Value;
			
			prepareParms[1]=new SqlParameter("parameters",SqlDbType.NVarChar);
			prepareParms[1].Direction = ParameterDirection.Input;

			prepareParms[2]=new SqlParameter("stmt",SqlDbType.NVarChar);
			prepareParms[2].Direction = ParameterDirection.Input;

			prepareParms[3]=new SqlParameter("options",SqlDbType.Int);
			prepareParms[3].Direction = ParameterDirection.Input;
			prepareParms[3].Value=1;

			prepareParms[4]=new SqlParameter("scrollopt",SqlDbType.Int);
			prepareParms[4].Direction = ParameterDirection.InputOutput;

			prepareParms[5]=new SqlParameter("ccopt",SqlDbType.Int);
			prepareParms[5].Direction = ParameterDirection.InputOutput;
			prepareParms[5].Value=4;

			//----------------Close command
			closeCommand = new SqlCommand("sp_cursorclose");
			closeCommand.CommandType=CommandType.StoredProcedure;
            closeCommand.CommandTimeout = commandTimeout;
			SqlParameter handle4 = closeCommand.Parameters.Add("handle", SqlDbType.Int);
			handle4.Direction = ParameterDirection.Input;

			//------------------unprepareCursor
			unprepareCommand =new SqlCommand("sp_cursorunprepare");
			unprepareCommand.CommandType = CommandType.StoredProcedure;
            unprepareCommand.CommandTimeout = commandTimeout;

			SqlParameter cursor1 = unprepareCommand.Parameters.Add("cursor", SqlDbType.Int);
			cursor1.Direction = ParameterDirection.Input;

		}
	}


	public class GxPreparedStatementCache : Dictionary<string, GxItemStmt>
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.GxPreparedStatementCache));
		private IGxConnection conn;

		public GxPreparedStatementCache(IGxConnection connection, int maxSize):base(maxSize)
		{
			conn=connection;
		}

		public  int getUsedCursors()
		{
			return Count;
		}
	
		public void unprepare(int cursorId )
		{
			try
			{
                conn.ConnectionCache.UnprepareCommand.Connection=(SqlConnection)conn.InternalConnection.InternalConnection;
				conn.ConnectionCache.UnprepareCommand.Transaction=(SqlTransaction) conn.BeginTransaction();
				conn.ConnectionCache.PrepareUnprepare();
				conn.ConnectionCache.UnprepareCommand.Parameters[0].Value=cursorId;
				conn.ConnectionCache.UnprepareCommand.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "GxCommand.unprepare Error ", e);
			}
		}
		public void UnprepareClear()
		{
			IEnumerator values = this.Values.GetEnumerator();
			while (values.MoveNext())
			{
				unprepare( ((GxItemStmt)values.Current).cursorId);
			}
			base.Clear();
		}

		public void dump()
		{
			GXLogging.Info(log, "Prepared cursors");

			foreach ( string stmt in base.Keys)
			{
				GXLogging.Info(log, this[stmt] + " ");
			}
		}

	}
	

	public class GxSqlCursorDataReader: IDataReader
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.GxSqlCursorDataReader));
		private static int STARTPOS = -1;
		private int pos= STARTPOS;
		private ushort fetchSize;
		private GxArrayList block;
		private int blockSize;
		private IGxConnection con;
		private bool hasnext;
		private int handle;
		private int fetchsCount;
		private GxConnectionCache cache;
		private int cursorId;
		private string key;
		private string stmt;
		private GxParameterCollection parameters;
		private int cursorHandle;
		private bool cursorUnopened;
		private string defParameters;
		private SlidingTime expiration;
		private bool cached;
		private int totalBlockSize;
		private bool isForFirst;
		private IGxConnectionManager _connManager;
		private GxDataRecord m_dr;
		private long readBytes;
		private bool dynStmt;
		
		public GxSqlCursorDataReader(IGxConnectionManager connManager,GxDataRecord dr, IGxConnection connection, GxParameterCollection parameters ,
			string stmt, ushort fetchSize,bool forFirst, int handle, bool withCached, SlidingTime expiration, bool dynStmt)
		{
			try
			{
				this.fetchSize = fetchSize;
				this.stmt=stmt;
				this.dynStmt=dynStmt;
				this.parameters=parameters;
				this.handle=handle;
				this.con=connection;
				this.cache=connection.ConnectionCache;
				this.isForFirst=forFirst;
				this.cached=withCached;
				this._connManager=connManager;
				this.m_dr=dr;
				if (cached)
				{
					this.key=SqlUtil.GetKeyStmtValues(parameters,stmt,isForFirst);
					this.expiration=expiration;
				}
				GXLogging.Debug(log, "Open GxSqlCursorDataReader handle:'" + handle + "'");
				con.MonitorEnter();
				LoadFirstBlock();
				con.MonitorExit();
				pos=STARTPOS;
			}
			catch(Exception e)
			{
				GXLogging.Warn(log, "Return GxDataReader Error ", e);
				try{
				con.MonitorExit();
				}
				catch{}

				throw (new GxADODataException(e));
			}
		}

		public void LoadFirstBlock()
		{
			GXLogging.Debug(log, "Start LoadFirstBlock, Parameters: Stmt '"+ stmt + "', FetchSize: " + fetchSize);
			con = _connManager.IncOpenHandles(handle, m_dr.DataSource);
			con.CurrentStmt=stmt;
			block = new GxArrayList(fetchSize);

			if (!cache.ContainsPreparedStmt(stmt))
			{
				if(con.InternalConnection.IsSQLServer7())
				{
					CursorPrepare();
					cache.AddPreparedStmt(stmt, cursorId);
					cache.AddParameter(cursorId, parameters);
					CursorExecute();
				}
				else
				{
					CursorPrepExec();
					cache.AddPreparedStmt(stmt, cursorId);
					cache.AddParameter(cursorId, parameters);
				}
			}
			else
			{
				cursorId = (int)cache.GetPreparedStmt(stmt);
				CursorExecute();
			}
			con.CurrentStmt=null;

			cache.SetAvailableStmt(stmt, false, dynStmt); 

			GXLogging.Debug(log, "Return LoadFirstBlock, Parameters: CursorHandle "+cursorHandle );
		}


        public void LoadBlock()
        {
            GXLogging.Debug(log, "Start LoadBlock, Parameters: CursorHandle " + cursorHandle);
            con.MonitorEnter();
            ++fetchsCount;
            SqlDataReader reader = null;
            try
            {
                InitCommand(cache.FetchCommand);
                cache.PrepareFetch();
                cache.FetchCommand.Parameters[0].Value = cursorHandle;
                cache.FetchCommand.Parameters[3].Value = fetchSize;

                if (cache.FetchCommand.Transaction == null)
                {
                    cache.FetchCommand.Transaction = (SqlTransaction)con.BeginTransaction();
                }
				using (reader = cache.FetchCommand.ExecuteReader(CommandBehavior.SingleResult))
				{
					con.CurrentStmt = null;
					ReadBlock(reader);
				}
				if (blockSize < fetchSize)
                {
                    cursorUnopened = AutoCloseCursor();
                    hasnext = false;
                }
            }
            catch (Exception e)
            {
				if (reader != null) reader.Close();
                GXLogging.Error(log, "stmt:" + this.stmt);
                GXLogging.Error(log, "LoadBlock error ", e);
                
                Dispose();
                hasnext = false;
                cursorUnopened = false;
            }
            finally
            {
                con.MonitorExit();
                GXLogging.Debug(log, "Return GxCommand.LoadBlock");
            }
        }


		private void InitCommand(SqlCommand command)
		{
			command.Connection=(SqlConnection)con.InternalConnection.InternalConnection;
			command.Transaction = (SqlTransaction) con.BeginTransaction();
			_connManager.RefreshTimeStamp(handle, m_dr.DataSource);
			con.CurrentStmt=stmt;
		}

		private string GetDefParmsStmt()
		{

			if (defParameters==null)
			{
				if (parameters.Count>0)
				{
					StringBuilder svalues = new StringBuilder();
					int count=parameters.Count;
					for (int j=0; j< count; j++)
					{
						SqlParameter parameter = (SqlParameter)parameters[j];
						if ((j<count)&& (j!=0)){ svalues.Append(',');}
						svalues.Append(parameter.ParameterName);
						svalues.Append(' ');
						svalues.Append(SqlUtil.SqlTypeToDbType(parameter));
					}
					defParameters=svalues.ToString();
					return defParameters;
				}
				else
				{
					return null;
				}
			}
			else
			{
				return defParameters;
			}
		}
		private void AddParmsStmt(SqlCommand cmd)
		{
			if (parameters.Count>0)
			{
				for (int j=0; j< parameters.Count; j++)
				{
					cmd.Parameters.Add(m_dr.CloneParameter(parameters[j]));
				}
			}
		}

		private void CursorPrepExec()
		{
			string paramStmtDef = GetDefParmsStmt();
			InitCommand(cache.PrepExecCommand);
			
			cache.PrepExecCommand.Parameters.Clear();
			foreach (SqlParameter p in cache.PrepExecParms)
			{
				cache.PrepExecCommand.Parameters.Add(p);
			}
			if (paramStmtDef==null)	cache.PrepExecCommand.Parameters[2].Value=DBNull.Value;
			else  cache.PrepExecCommand.Parameters[2].Value=paramStmtDef;
			cache.PrepExecCommand.Parameters[3].Value=stmt;
            if (AutoCloseCursor())
            {
                cache.PrepExecCommand.Parameters[4].Value = paramStmtDef == null ? 24592 : 28688;
            }
			else 
			{
				cache.PrepExecCommand.Parameters[4].Value=paramStmtDef==null ? 0x2000|0x0010 : 0x2000|0x0010|0x1000;
			}
			
			cache.PrepExecCommand.Parameters[6].Value=fetchSize;
			
			AddParmsStmt(cache.PrepExecCommand);
			
			{
				SqlDataReader reader=null;
				try
				{
					using (reader = cache.PrepExecCommand.ExecuteReader())
					{
						ReadBlock(reader);
						reader.NextResult();
						cursorHandle = (int)cache.PrepExecCommand.Parameters[1].Value;
						cursorId = (int)cache.PrepExecCommand.Parameters[0].Value;
					}
				}
				finally
				{
					if (reader != null) reader.Close();
				}

			}
            if (((int)cache.PrepExecCommand.Parameters[6].Value) < fetchSize)
			{ 
				hasnext=false;
                cursorUnopened = AutoCloseCursor();
			}
			else
			{
				hasnext=true;
				cursorUnopened = false;
			}
		}

		public void CursorPrepare()
		{

			string paramStmtDef = GetDefParmsStmt();
			InitCommand(cache.PrepareCommand);
			cache.PrepareCommand.Parameters.Clear();
			foreach (SqlParameter p in cache.PrepareParms)
			{
				cache.PrepareCommand.Parameters.Add(p);
			}
			if (paramStmtDef==null)	cache.PrepareCommand.Parameters[1].Value=DBNull.Value;
			else  cache.PrepareCommand.Parameters[1].Value=paramStmtDef;
			cache.PrepareCommand.Parameters[2].Value=stmt;
			cache.PrepareCommand.Parameters[4].Value = paramStmtDef==null ? 1:4100;
			
			{
				cache.PrepareCommand.ExecuteNonQuery();
			}
			cursorId= (int)cache.PrepareCommand.Parameters[0].Value;

		}
        private bool AutoCloseCursor()
        {
            return !con.InternalConnection.IsSQLServer9();
        }

		private void CursorExecute()
		{

			InitCommand(cache.ExecCommand);
			cache.ExecCommand.Parameters.Clear();
			foreach(SqlParameter p in cache.ExecParms)
			{
				cache.ExecCommand.Parameters.Add(p);
			}
			cache.ExecCommand.Parameters[0].Value=cursorId;
			if(AutoCloseCursor())
			{
                cache.ExecCommand.Parameters[2].Value = 24592;
			}
			else
			{
                cache.ExecCommand.Parameters[2].Value = 0x2000 | 0x0010;
			}
			if (con.InternalConnection.IsSQLServer7())
			{
				cache.ExecCommand.Parameters[3].Value = 4; 
			}
			cache.ExecCommand.Parameters[4].Value=fetchSize;

			GxParameterCollection pcol=(GxParameterCollection)cache.GetParameters(cursorId);
			for(int i=0; i<parameters.Count;i++)
			{
				pcol[i].Value=parameters[i].Value;
				cache.ExecCommand.Parameters.Add(pcol[i]);
			}
			
			{
				SqlDataReader reader =null;
				try
				{
					using (reader = cache.ExecCommand.ExecuteReader())
					{
						ReadBlock(reader);
						reader.NextResult();
						cursorHandle = (int)cache.ExecCommand.Parameters[1].Value;
					}
				}
				finally
				{
					if (reader!= null) reader.Close();
				}
			}

            if (((int)cache.ExecCommand.Parameters[4].Value) < fetchSize)
			{ 
				hasnext=false;
                cursorUnopened = AutoCloseCursor();
			}
			else
			{
				hasnext=true;
				cursorUnopened = false;
			}

		}

		private void ReadBlock(SqlDataReader reader)
		{
			int i=0;
			if (!cached)
			{	
				pos = STARTPOS+1;
								 
				block = new GxArrayList(fetchSize);
			}
			while (reader.Read()&& i<fetchSize)
			{
				object[] values = new object[reader.FieldCount];
				m_dr.GetValues(reader, ref values); 
				block.Add(values);
				++i;
			}
			totalBlockSize=block.Count;
			blockSize=i;
		}

		public bool Read()
		{
			if (++pos >= totalBlockSize)
			{
				if (hasnext)
				{
					LoadBlock();
					return (blockSize>0);
				}			
				else
				{
					return false;
				}
			}
			else
			{
				return true;
			}

		}

		public void Dispose()
		{
			blockSize=0;
		}

		public void Close() 
		{
			if (!hasnext && cached)
			{
				SqlUtil.AddBlockToCache(key, new CacheItem(block, false,
					(fetchsCount * fetchSize) + blockSize, readBytes), con, expiration != null ? (int)expiration.ItemSlidingExpiration.TotalMinutes : 0);
			}

			if (!cursorUnopened)
			{
				GXLogging.Debug(log, "Close GxSqlCursorDataReader");
				try
				{
					con.MonitorEnter();
					InitCommand(cache.CloseCommand);
					cache.PrepareClose();
					cache.CloseCommand.Parameters[0].Value=cursorHandle;
					
					{
						cache.CloseCommand.ExecuteNonQuery();
					}
					cursorUnopened = true;
				}
				catch(SqlException esql)
				{
					GXLogging.Debug(log, "Could not close cursor, Parameters: CursorHandle " + cursorHandle, esql);
				}
				finally 
				{
					try
					{
						con.MonitorExit();
					}
					catch{}
				}

			}
			cache.SetAvailableStmt(stmt, true, dynStmt); 

			_connManager.DecOpenHandles(handle, m_dr.DataSource);

			con.CurrentStmt=null;

		}
		public int Depth 
		{
			get 
			{	
				return 0; 
			}
		}

		public DataTable GetSchemaTable()
		{
			throw (new GxNotImplementedException());
		}

		public bool IsClosed 
		{
			get { return cursorUnopened;}
		}
		public bool NextResult()
		{
			throw (new GxNotImplementedException());
		}
		public int RecordsAffected 
		{
			get { throw (new GxNotImplementedException()); }
		}
		public int FieldCount 
		{
			get	{ throw (new GxNotImplementedException());;}
		}
		public bool GetBoolean( int i)
		{
            readBytes += 1;
            return (bool)block.Item(pos,i); 
		}
        public Guid GetGuid(int i)
        {
            readBytes += 16;
            return (Guid)block.Item(pos,i);
        }
        public IGeographicNative GetGeospatial(int i)
        {
            readBytes += 16;
            return (Geospatial)block.Item(pos, i);
        }
		public byte GetByte( int i)
		{
			throw (new GxNotImplementedException());
		}
		public long GetBytes( int i, long fieldOffset, byte[] buffer, int bufferoffset, int length	)
		{
			byte[] cell= (byte[])block.Item(pos,i);
			int j;
			for (j=0;j<length && fieldOffset < cell.Length; j++)
			{
				buffer[bufferoffset]=cell[fieldOffset];
				fieldOffset++;
				bufferoffset++;
			}
			readBytes += j;
			return j;

		}
		public long GetChars( int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw (new GxNotImplementedException());
		}
		public char GetChar(int i)
		{
			throw (new GxNotImplementedException());
		}
		public IDataReader GetData(int i)
		{
			throw (new GxNotImplementedException());
		}
		public string GetDataTypeName( int i)
		{
			throw (new GxNotImplementedException());
		}
		public DateTime GetDateTime(int i)
		{
			readBytes += 8;
			return (DateTime)block.Item(pos,i);
		}
		public decimal GetDecimal(int i	)
		{
			readBytes += 12;
			try
			{
				return Convert.ToDecimal(block.Item(pos,i)); 
			}
			catch(Exception ex)
			{
				GXLogging.Warn(log, "GetDecimal Exception, parameter " + i + ", type: " + block.Item(pos, i).GetType() 
										  + ", value: " + block.Item(pos,i), ex);
				return (decimal)block.Item(pos,i);
			}
		}
		public double GetDouble(int i)
		{
			readBytes += 8;
			return Convert.ToDouble(block.Item(pos,i)); 
		}
		public Type GetFieldType(int i)
		{
			throw (new GxNotImplementedException());
		}
		public float GetFloat(int i)
		{
			throw (new GxNotImplementedException());
		}
		public short GetInt16(int i)
		{
			readBytes += 2;
			return Convert.ToInt16(block.Item(pos,i)); 
		}
		public int GetInt32(int i)
		{
			readBytes += 4;
			return Convert.ToInt32(block.Item(pos,i));
		}
		public long GetInt64(int i)
		{
			readBytes += 8;
			return Convert.ToInt64(block.Item(pos,i)); 
		}
		public string GetName(int i)
		{
			throw (new GxNotImplementedException());
		}
		public int GetOrdinal(string name)
		{
			throw (new GxNotImplementedException());
		}
		public string GetString(int i)
		{
			try{
			readBytes += 10 + (2 * ((string)block.Item(pos,i)).Length);
			return (string)block.Item(pos,i); 
			}
			catch 
			{
				object value = block.Item(pos, i);
				if (value is Guid)
					return Encoding.ASCII.GetString(((Guid)block.Item(pos, i)).ToByteArray());
				else
					return Convert.ToString(value);
			}

		}
		public object GetValue(int i)
		{
			return block.Item(pos,i);
		}
		public int GetValues(object[] values)
		{
			throw (new GxNotImplementedException());
		}
		public bool IsDBNull(int i)
		{
			return block.Item(pos,i) == DBNull.Value;
		}
		public object this[string name] 
		{
			get{ throw (new GxNotImplementedException());}
		}
		public object this[int i]
		{
			get { throw (new GxNotImplementedException());}
		}
	}


	public class GxDbmsUtils
	{
		IGxDataRecord dbmsHandler;
		public GxDbmsUtils(IGxDataRecord db)
		{
			dbmsHandler = db;
		}
		public string ValueList(short[] Values, string Prefix, string Postfix)
		{
			if (Values == null || Values.Length == 0)
			{
				return dbmsHandler.FalseCondition();
			}
			else
			{
				return Prefix + ValueList(Values) + Postfix;
			}
		}
		public string ValueList(int[] Values, string Prefix, string Postfix)
		{
			if (Values == null || Values.Length == 0)
			{
				return dbmsHandler.FalseCondition();
			}
			else
			{
				return Prefix + ValueList(Values) + Postfix;
			}
		}
		public string ValueList(long[] Values, string Prefix, string Postfix)
		{
			if (Values == null || Values.Length == 0)
			{
				return dbmsHandler.FalseCondition();
			}
			else
			{
				return Prefix + ValueList(Values) + Postfix;
			}
		}
		public string ValueList(decimal[] Values, string Prefix, string Postfix)
		{
			if (Values == null || Values.Length == 0)
			{
				return dbmsHandler.FalseCondition();
			}
			else
			{
				return Prefix + ValueList(Values) + Postfix;
			}
		}
		public string ValueList(float[] Values, string Prefix, string Postfix)
		{
			if (Values == null || Values.Length == 0)
			{
				return dbmsHandler.FalseCondition();
			}
			else
			{
				return Prefix + ValueList(Values) + Postfix;
			}
		}
		public string ValueList(double[] Values, string Prefix, string Postfix)
		{
			if (Values == null || Values.Length == 0)
			{
				return dbmsHandler.FalseCondition();
			}
			else
			{
				return Prefix + ValueList(Values) + Postfix;
			}
		}
		public string ValueList(DateTime[] Values, string Prefix, string Postfix)
		{
			if (Values == null || Values.Length == 0)
			{
				return dbmsHandler.FalseCondition();
			}
			else
			{
				return Prefix + ValueList(Values) + Postfix;
			}
		}
		public string ValueList(string[] Values, string Prefix, string Postfix)
		{
			if (Values == null || Values.Length == 0)
			{
				return dbmsHandler.FalseCondition();
			}
			else
			{
				return Prefix + ValueList(Values) + Postfix;
			}
		}
		public string ValueList(bool[] Values, string Prefix, string Postfix)
		{
			if (Values == null || Values.Length == 0)
			{
				return dbmsHandler.FalseCondition();
			}
			else
			{
				return Prefix + ValueList(Values) + Postfix;
			}
		}
		public string ValueList(IList Values, string Prefix, string Postfix)
		{ 
			if (Values == null || Values.Count == 0)
			{
				return dbmsHandler.FalseCondition();
			}
			else
			{
				return Prefix + ValueList(Values) + Postfix;
			}
		}
		public string ValueList(short[] Values)
		{
			StringBuilder bufferString = new StringBuilder();
			string separatorString="";
			
			int index = 0;
			for (index = Values.GetLowerBound(0); index <= Values.GetUpperBound(0); index++)
			{
				bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant(Values[index]));
				separatorString = ", ";
			}
			return bufferString.ToString();
		}
		public string ValueList(int[] Values)
		{
			StringBuilder bufferString = new StringBuilder();
			string separatorString="";
			
			int index = 0;
			for (index = Values.GetLowerBound(0); index <= Values.GetUpperBound(0); index++)
			{
				bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant(Values[index]));
				separatorString = ", ";
			}
			return bufferString.ToString();
		}
		public string ValueList(long[] Values)
		{
			StringBuilder bufferString = new StringBuilder();
			string separatorString="";
			
			int index = 0;
			for (index = Values.GetLowerBound(0); index <= Values.GetUpperBound(0); index++)
			{
				bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant(Values[index]));
				separatorString = ", ";
			}
			return bufferString.ToString();
		}
		public string ValueList(decimal[] Values)
		{
			StringBuilder bufferString = new StringBuilder();
			string separatorString="";
			
			int index = 0;
			for (index = Values.GetLowerBound(0); index <= Values.GetUpperBound(0); index++)
			{
				bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant(Values[index]));
				separatorString = ", ";
			}
			return bufferString.ToString();
		}
		public string ValueList(float[] Values)
		{
			StringBuilder bufferString = new StringBuilder();
			string separatorString="";
			
			int index = 0;
			for (index = Values.GetLowerBound(0); index <= Values.GetUpperBound(0); index++)
			{
				bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant(Values[index]));
				separatorString = ", ";
			}
			return bufferString.ToString();
		}
		public string ValueList(double[] Values)
		{
			StringBuilder bufferString = new StringBuilder();
			string separatorString="";
			
			int index = 0;
			for (index = Values.GetLowerBound(0); index <= Values.GetUpperBound(0); index++)
			{
				bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant(Values[index]));
				separatorString = ", ";
			}
			return bufferString.ToString();
		}
		public string ValueList(DateTime[] Values)
		{
			StringBuilder bufferString = new StringBuilder();
			string separatorString="";
			
			int index = 0;
			for (index = Values.GetLowerBound(0); index <= Values.GetUpperBound(0); index++)
			{
				bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant(Values[index]));
				separatorString = ", ";
			}
			return bufferString.ToString();
		}
		public string ValueList(string[] Values)
		{
			StringBuilder bufferString = new StringBuilder();
			string separatorString="";
			
			int index = 0;
			for (index = Values.GetLowerBound(0); index <= Values.GetUpperBound(0); index++)
			{
				bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant(Values[index]));
				separatorString = ", ";
			}
			return bufferString.ToString();
		}
		public string ValueList(bool[] Values)
		{
			StringBuilder bufferString = new StringBuilder();
			string separatorString="";
			
			int index = 0;
			for (index = Values.GetLowerBound(0); index <= Values.GetUpperBound(0); index++)
			{
				bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant(Values[index]));
				separatorString = ", ";
			}
			return bufferString.ToString();
		}
		public string ValueList(IList Values)
		{
			StringBuilder bufferString = new StringBuilder();
			string separatorString="";
            string so;
			foreach(object o in Values)
			{
                if ((so = o as string)!=null)
    				bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant(so));
                else if (o is short)
                    bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant((short)o));
                else if (o is int)
                    bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant((int)o));
                else if (o is long)
                    bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant((long)o));
                else if (o is decimal)
                    bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant((decimal)o));
                else if (o is float)
                    bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant((float)o));
                else if (o is double)
                    bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant((double)o));
                else if (o is DateTime)
                    bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant((DateTime)o));
                else if (o is bool)
                    bufferString.Append( separatorString).Append( dbmsHandler.ToDbmsConstant((bool)o));
                else
                    bufferString.Append(separatorString).Append(dbmsHandler.ToDbmsConstant(o.ToString()));
                separatorString = ", ";
			}
			return bufferString.ToString();
		}
	}

	sealed internal class MssqlConnectionWrapper : GxAbstractConnectionWrapper
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.MssqlConnectionWrapper));
		int sqlserver7 = -1;
		int sqlserver9 = -1;

		public MssqlConnectionWrapper() : base(new SqlConnection())
		{ }
		public MssqlConnectionWrapper(String connectionString, GxConnectionCache connCache, IsolationLevel isolationLevel) 
		{
			try
			{
				Type sqlConnection = typeof(SqlConnection);
				_connection = (IDbConnection)ClassLoader.CreateInstance(sqlConnection.Assembly, sqlConnection.FullName, new object[] { connectionString });
				m_isolationLevel = isolationLevel;
				m_connectionCache = connCache;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "MS SQL data provider Ctr error " + ex.Message + ex.StackTrace);
				throw ex;
			}
		}
		override public void Open()
		{
			InternalConnection.Open();
			if (!m_autoCommit)
			{
				GXLogging.Debug(log, "Open connection InternalConnection.BeginTransaction() ");
				m_transaction = InternalConnection.BeginTransaction(m_isolationLevel);  
			}
			else
			{
				m_transaction = null;
			}

			if (Preferences.Instrumented)
			{
				
				m_connectionCache.SPIDCommand.Transaction = m_transaction;
				m_connectionCache.SPIDCommand.Connection = InternalConnection;
				m_spid = (short)m_connectionCache.SPIDCommand.ExecuteScalar();
			}
		}

		override public void Close()
		{
			try
			{
				InternalConnection.Close();
			}
			catch (SqlException ex)
			{
				throw new DataException(ex.Message, ex);
			}
		}
		public override short SetSavePoint(IDbTransaction transaction, string savepointName)
		{

			((SqlTransaction)transaction).Save(savepointName);

			return 0;
		}
		public override short ReleaseSavePoint(IDbTransaction transaction, string savepointName)
		{
			return 0;
		}
		public override short RollbackSavePoint(IDbTransaction transaction, string savepointName)
		{

			((SqlTransaction)transaction).Rollback(savepointName);

			return 0;
		}
		override public IDbCommand CreateCommand()
		{

			SqlConnection sc = InternalConnection as SqlConnection;
			if (null == sc)
				throw new InvalidOperationException("InvalidConnType00" + InternalConnection.GetType().FullName);
			return sc.CreateCommand();
		}
		public override DbDataAdapter CreateDataAdapter()
		{
			return new SqlDataAdapter();
		}
		public override bool IsSQLServer7()
		{

            if (sqlserver7==-1)
			{
				SqlConnection sc = InternalConnection as SqlConnection;
				if(null == sc)
					throw new InvalidOperationException("InvalidConnType00" + InternalConnection.GetType().FullName);
				sqlserver7 = sc.ServerVersion.StartsWith("07") ? 1:0;
			}
			return sqlserver7==1;

		}
		public override bool IsSQLServer9()
		{

			if (sqlserver9==-1)
			{
				SqlConnection sc = InternalConnection as SqlConnection;
				if(null == sc)
					throw new InvalidOperationException("InvalidConnType00" + InternalConnection.GetType().FullName);
				GXLogging.Debug(log, "sc.ServerVersion:", sc.ServerVersion);
                sqlserver9 = (sc.ServerVersion.StartsWith("09") || sc.ServerVersion.StartsWith("1")) ? 1 : 0;
			}
			return sqlserver9==1;

		}

	}


		public abstract class GxAbstractConnectionWrapper : IDbConnection 
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.GxAbstractConnectionWrapper));
		protected short m_spid;
		protected IDbConnection _connection;
		protected bool m_autoCommit;
		protected IDbTransaction m_transaction;
		protected GxConnectionCache m_connectionCache;
		protected IsolationLevel m_isolationLevel;

		protected GxAbstractConnectionWrapper(IDbConnection conn, IsolationLevel isolationLevel) :this(conn)
		{
			m_isolationLevel = isolationLevel;
		}
		protected GxAbstractConnectionWrapper(IDbConnection conn) 
		{
			if(null == conn)
				throw new ArgumentNullException(nameof(conn), "ParamReq00");

			_connection = conn;
		}
		protected GxAbstractConnectionWrapper()
		{
			//Ctr used for OracleDataProvider
		}
		public virtual void AfterConnect()
		{
			
		}
        public virtual void CheckStateBeforeRollback(int OpenHandles)
        {
            
        }
        public virtual void CheckCursors(int OpenHandles, bool beforeCommitRollback)
        {
        }

		public virtual void CheckStateInc()
		{
		}
		public virtual void CheckStateDec()
		{
		}
		public virtual void CheckState(bool value)
		{
		}
		public bool AutoCommit
		{
			get{ return m_autoCommit;}
			set{ m_autoCommit=value;}
		}

        public IDbConnection InternalConnection 
		{
			get {	return _connection;	}
		}
		public IDbTransaction Transaction
		{
			get{ return m_transaction;}
			set{ m_transaction=value;}
		}

		public void Dispose() 
		{
			InternalConnection.Dispose();
		}

		public String ConnectionString 
		{
			get {	return InternalConnection.ConnectionString;	}
			set {throw new NotSupportedException("NoChangeMsg00" + "ConnectionString");	}
		}

		public int ConnectionTimeout 
		{
			get {return InternalConnection.ConnectionTimeout;}
		}

		public virtual String Database 
		{
			get {return InternalConnection.Database;}
		}

		public ConnectionState State 
		{
			get {	return InternalConnection.State;	}
		}

		public virtual void Open()
		{
			InternalConnection.Open();
		}

		public virtual void Close()
		{
			InternalConnection.Close();
		}

		public void ChangeDatabase(String database) 
		{
            throw new NotSupportedException("NoChangeMsg00" + database);
		}

		public virtual IDbTransaction BeginTransaction() 
		{
			GXLogging.Debug(log, " InternalConnection.BeginTransaction() ");
			return InternalConnection.BeginTransaction();
		}

		public virtual IDbTransaction BeginTransaction(IsolationLevel isoLevel) 
		{
			GXLogging.Debug(log, " InternalConnection.BeginTransaction("+isoLevel+") ");
			return InternalConnection.BeginTransaction(isoLevel);
		}

		abstract public IDbCommand CreateCommand();
		abstract public DbDataAdapter CreateDataAdapter();
		public virtual bool IsSQLServer7()
		{
			return false;
		}
		public virtual bool IsSQLServer9()
		{
			return false;
		}
		public virtual short SetSavePoint(IDbTransaction transaction, string savepointName)
		{
			return 0;
		}
		public virtual short ReleaseSavePoint(IDbTransaction transaction, string savepointName)
		{
			return 0;
		}
		public virtual short RollbackSavePoint(IDbTransaction transaction, string savepointName)
		{
			return 0;
		}
	}
		
	public class GxCacheDataReader: IDataReader
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.GxCacheDataReader));
		protected GxArrayList block;
		protected int pos;
		int blockSize;
		protected bool computeSizeInBytes; 
		protected long readBytes;
		string key;
		bool closed;

		public GxCacheDataReader(CacheItem cacheItem, bool computeSize, string keyCache)
		{
			block=cacheItem.Data;
			pos=-1;
			blockSize=cacheItem.BlockSize;
			computeSizeInBytes = computeSize;
			key=keyCache;
		}

		public bool Read()
		{
			bool res = (++pos < blockSize);
			return res;
		}
		public virtual DateTime GetDateTime(int i)
		{
			if (computeSizeInBytes) readBytes += 8;
			object value = block.Item(pos,i);

			if (value is DateTime)
				return (DateTime)value;
			else
				return Convert.ToDateTime(value);
		}
		public virtual decimal GetDecimal(int i	)
		{
			if (computeSizeInBytes) readBytes += 12;
			return Convert.ToDecimal(block.Item(pos,i),System.Globalization.CultureInfo.InvariantCulture.NumberFormat); 
		}
		public double GetDouble(int i)
		{
			if (computeSizeInBytes) readBytes += 8;
			return Convert.ToDouble(block.Item(pos,i)); 
		}

		public virtual short GetInt16(int i)
		{
			if (computeSizeInBytes) readBytes += 2;
			return Convert.ToInt16(block.Item(pos,i)); 
		}
		public virtual int GetInt32(int i)
		{
			if (computeSizeInBytes) readBytes += 4;
			return Convert.ToInt32(block.Item(pos,i));
		}
		public virtual long GetInt64(int i)
		{
			if (computeSizeInBytes) readBytes += 8;
			return Convert.ToInt64(block.Item(pos,i)); 
		}
		public virtual string GetString(int i)
		{
			try
			{
				if (computeSizeInBytes) readBytes += 10 + (2 * ((string)block.Item(pos,i)).Length);
				return (string)block.Item(pos,i); 
			}
			catch 
			{
				object value = block.Item(pos,i);
				if(value is Guid)
					return Encoding.ASCII.GetString(((Guid)block.Item(pos, i)).ToByteArray());
				else
					return Convert.ToString(value);
			}

		}
		public object GetValue(int i)
		{
			return block.Item(pos,i);
		}
		public virtual bool IsDBNull(int i)
		{
			return (block.Item(pos, i) == DBNull.Value) || (block.Item(pos, i) is DBNull);
		}
		public char GetChar(int i)
		{
			throw (new GxNotImplementedException());
		}
		public IDataReader GetData(int i)
		{
			throw (new GxNotImplementedException());
		}
		public string GetDataTypeName( int i)
		{
			throw (new GxNotImplementedException());
		}
		public void Close( )
		{
			if (!closed)
			{
				if (computeSizeInBytes)
				{	
					GXLogging.Debug(log, "Close, readBytes: '" + readBytes + "'");
					InProcessCache memoryCache = CacheFactory.Instance as InProcessCache;
					if (memoryCache != null)
						memoryCache.AddSize(key, readBytes); 
					readBytes=0;
				}
				closed=true;
			}
			
		}
		public bool NextResult()
		{
			throw (new GxNotImplementedException());
		}
		public int RecordsAffected 
		{
			get { throw (new GxNotImplementedException()); }
		}
		public int FieldCount 
		{
			get	{ throw (new GxNotImplementedException());;}
		}
		public virtual bool GetBoolean( int i)
		{
            if (computeSizeInBytes) readBytes += 1;
            if ((block.Item(pos,i)) is byte)
                return (((byte)block.Item(pos,i)) == 1);
            else if ((block.Item(pos,i)) is bool)
                return (bool)block.Item(pos,i);
			else if ((block.Item(pos, i)) is string && bool.TryParse((string)block.Item(pos, i), out bool result))
				return result;
			else
				return (((int)block.Item(pos,i)) == 1);
		}
		public byte GetByte( int i)
		{
			throw (new GxNotImplementedException());
		}
		public virtual long GetBytes( int i, long fieldOffset, byte[] buffer, int bufferoffset, int length	)
		{
			byte[] cell= (byte[])block.Item(pos,i);
			int j;
			for (j=0;j<length && fieldOffset < cell.Length; j++)
			{
				buffer[bufferoffset]=cell[fieldOffset];
				fieldOffset++;
				bufferoffset++;
			}
			if (computeSizeInBytes) readBytes += j;
			return j;
		}
		public long GetChars( int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw (new GxNotImplementedException());
		}
		public DataTable GetSchemaTable()
		{	
			throw (new GxNotImplementedException());
		}
		public bool IsClosed 
		{
			get{ return closed;}
		}
		public int Depth 
		{
			get {return 0;}
		}
		public void Dispose()
		{
		}
		public string GetName(int i)
		{
			throw (new GxNotImplementedException());
		}

		public Type GetFieldType(int i)
		{
			throw (new GxNotImplementedException());
		}
		public float GetFloat(int i)
		{
			throw (new GxNotImplementedException());
		}
		public Guid GetGuid(int i)
		{
			if (computeSizeInBytes) readBytes += 16;
			object value = block.Item(pos, i);
			if (value is Guid)
				return (Guid)value;
			else
				return Guid.Parse(value as string);
		}
		public object this[string name] 
		{
			get{ throw (new GxNotImplementedException());}
		}
		public object this[int i]
		{
			get { throw (new GxNotImplementedException());}
		}
		public int GetValues(object[] values)
		{
			throw (new GxNotImplementedException());
		}
		public int GetOrdinal(string name)
		{
			throw (new GxNotImplementedException());
		}

	}

	public delegate void ErrorHandler();

	public class ReturnInErrorHandlerException : Exception
	{
	}

	public class GxErrorHandler
	{
		ErrorHandler _errorHandler;
		GxErrorHandlerInfo _errorHandlerInfo;

		public GxErrorHandler()
		{
		}
		public GxErrorHandler(ErrorHandler eh, GxErrorHandlerInfo ehi)
		{
			_errorHandler = eh;
			_errorHandlerInfo = ehi;
		}
		public void SetRoutine( ErrorHandler newErrorHandler)
		{
			_errorHandler = newErrorHandler;
		}
		public int Execute(int err, int gxDBErr, string gxDBTxt, string gxOper, string gxErrTbl, String gxSqlState)
		{
			int gxErrOpt = 3;
			if (_errorHandler != null)
			{
				_errorHandlerInfo.Gx_err = Convert.ToInt16(err);
				_errorHandlerInfo.Gx_dbe = gxDBErr;
				_errorHandlerInfo.Gx_dbt = gxDBTxt;
				_errorHandlerInfo.Gx_ope = gxOper;
				_errorHandlerInfo.Gx_etb = gxErrTbl;
				_errorHandlerInfo.Gx_dbsqlstate = gxSqlState;
				_errorHandler();
				gxErrOpt = _errorHandlerInfo.Gx_eop;
			}
			return gxErrOpt;
		}
	}
	public class GxErrorHandlerInfo
	{
		short _err;
		int _gxDBErr;
		string _gxDBTxt;
		string _gxOper;
		string  _gxErrTbl;
		short _gxErrOpt;
		string _gxDbSqlState;

		public GxErrorHandlerInfo()
		{
		}
		public short Gx_err
		{ 
			get {return _err;}
			set {_err = value;}
		}
		public int Gx_dbe
		{ 
			get {return _gxDBErr;}
			set {_gxDBErr = value;}
		}
		public string Gx_dbt
		{ 
			get {return _gxDBTxt;}
			set {_gxDBTxt = value;}
		}
		public string Gx_ope
		{ 
			get {return _gxOper;}
			set {_gxOper = value;}
		}
		public string Gx_etb
		{ 
			get {return _gxErrTbl;}
			set {_gxErrTbl = value;}
		}
		public string Gx_dbsqlstate
		{
			get { return _gxDbSqlState; }
			set { _gxDbSqlState = value; }
		}
		public short Gx_eop
		{ 
			get {return _gxErrOpt;}
			set {_gxErrOpt = value;}
		}

	}

}
