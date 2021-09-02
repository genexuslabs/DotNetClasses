using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Threading;
using GeneXus.Cache;
using GeneXus.Metadata;
using GeneXus.Utils;
using GxClasses.Helpers;
using log4net;

namespace GeneXus.Data
{
	public class GxInformixIds : GxDb2
	{
		GxInformix informixCommon = new GxInformix();
		public override string GetServerDateTimeStmt(IGxConnection connection)
		{
			return informixCommon.GetServerDateTimeStmt(connection);
		}
		public override string GetServerDateTimeStmtMs(IGxConnection connection)
		{
			return informixCommon.GetServerDateTimeStmtMs(connection);
		}
		public override string GetServerVersionStmt()
		{
			return informixCommon.GetServerVersionStmt();
		}
		public override string GetServerUserIdStmt()
		{
			return informixCommon.GetServerUserIdStmt();
		}
		public override void SetTimeout(IGxConnectionManager connManager, IGxConnection connection, int handle)
		{
			informixCommon.SetTimeout(connManager, connection, handle);
		}
		public override string SetTimeoutSentence(long milliseconds)
		{
			return informixCommon.SetTimeoutSentence(milliseconds);
		}
		public override bool ProcessError(int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)
		{
			return informixCommon.ProcessError(dbmsErrorCode, emsg, errMask, con, ref status, ref retry, retryCount);
		}
	}
	public class GxDb2ISeriesIds : GxDb2
	{
		const string DEFAULT_ISERIES_PORT = "446";
		GxDb2ISeries iseriesCommon;
		public GxDb2ISeriesIds(string id) {
			iseriesCommon = new GxDb2ISeries(id);
		}
		public override string GetServerDateTimeStmtMs(IGxConnection connection)
		{
			return iseriesCommon.GetServerDateTimeStmt(connection);
		}

		public override string GetServerDateTimeStmt(IGxConnection connection)
		{
			return iseriesCommon.GetServerDateTimeStmt(connection);
		}
		public override string GetServerUserIdStmt()
		{
			return iseriesCommon.GetServerUserIdStmt();
		}
		public override string GetServerVersionStmt()
		{
			return iseriesCommon.GetServerVersionStmt();
		}
		public override bool ProcessError(int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)
		{
			return iseriesCommon.ProcessError(dbmsErrorCode, emsg, errMask, con, ref status, ref retry, retryCount);
		}
		public override object Net2DbmsDateTime(IDbDataParameter parm, DateTime dt)
		{
			return iseriesCommon.Net2DbmsDateTime(parm, dt);
		}
		public override DateTime Dbms2NetDate(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return iseriesCommon.Dbms2NetDate(cmd, DR, i);
		}
		protected override string BuildConnectionString(string datasourceName, string userId, string userPassword, string databaseName, string port, string schema, string extra)
		{
			if (string.IsNullOrEmpty(port))
			{
				port = DEFAULT_ISERIES_PORT;
			}
			return base.BuildConnectionString(datasourceName, userId, userPassword, databaseName, port, schema, extra);
		}
	}
	public class GxDb2 : GxDataRecord
	{

		static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public static string SQL_NULL_DATE_10="0000-00-00";
		public static string SQL_NULL_DATE_8="00000000";
        static Assembly _db2Assembly;
#if NETCORE
		internal static string Db2AssemblyName = "IBM.Data.Db2";
		const string dB2DbTypeEnum = "IBM.Data.Db2.DB2Type";
#else
		internal static string Db2AssemblyName = "IBM.Data.DB2";
		const string dB2DbTypeEnum = "IBM.Data.DB2.DB2Type";
#endif

		public static Assembly Db2Assembly
        {
            get
            {
                try
                {
                    if (_db2Assembly == null)
                    {
#if NETCORE
						string assemblyPath = FileUtil.GetStartupDirectory();
						GXLogging.Debug(log, $"Loading {Db2AssemblyName}.dll from:" + assemblyPath);
						var asl = new AssemblyLoader(assemblyPath);
						_db2Assembly = asl.LoadFromAssemblyName(new AssemblyName(Db2AssemblyName));
#else
						GXLogging.Debug(log, $"Loading {Db2AssemblyName} from GAC");
                        _db2Assembly = Assembly.LoadWithPartialName(Db2AssemblyName);
						GXLogging.Debug(log, $"{Db2AssemblyName} Loaded from GAC");
#endif
					}

				}
                catch (Exception ex)
                {
                    GXLogging.Error(log, $"Error loading {Db2AssemblyName} from GAC", ex);
                }
                if (_db2Assembly == null)
                {
                    _db2Assembly = Assembly.Load(Db2AssemblyName);
                }
                return _db2Assembly;
            }
        }

		public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId, 
			string userPassword,string databaseName, string port, string schema, string extra, GxConnectionCache connectionCache)
		{
			if (m_connectionString == null)
				m_connectionString=BuildConnectionString(datasourceName, userId, userPassword, databaseName, port, schema, extra);
			GXLogging.Debug(log, "Setting connectionString property ", ConnectionStringForLog);

			return new Db2ConnectionWrapper(m_connectionString,connectionCache, isolationLevel);
		}

		
		protected override string BuildConnectionString(string datasourceName, string userId, 
			string userPassword,string databaseName, string port, string schema, string extra)
		{
			StringBuilder connectionString = new StringBuilder();
			
			if (!string.IsNullOrEmpty(datasourceName) && port != null && port.Trim().Length > 0 && !hasKey(extra, "Server"))
			{
				connectionString.AppendFormat("Server={0}:{1};",datasourceName, port);
			}
			else if (datasourceName != null && !hasKey(extra, "Server"))
			{

				connectionString.AppendFormat("Server={0};",datasourceName);
			}
			if (userId!=null)
			{
                connectionString.AppendFormat(";UID={0};Password={1}", userId, userPassword);
			}
			if (databaseName != null && databaseName.Trim().Length > 0 && !hasKey(extra, "Database"))
			{
				connectionString.AppendFormat(";Database={0}",databaseName);
			}
			if (extra!=null)
			{
				string res = ParseAdditionalData(extra,"integrated security");

				if (!res.StartsWith(";") && res.Length>1)	connectionString.Append(";");
				connectionString.Append(res);
			}
			return connectionString.ToString();
		}

		public override bool GetBoolean(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return (base.GetInt(cmd, DR, i)==1);
		}
        public override Guid GetGuid(IGxDbCommand cmd, IDataRecord DR, int i)
        {
            string guid = base.GetString(cmd, DR, i);
            try
            {
                return new Guid(guid);
            }
            catch (FormatException)
            {
                return Guid.Empty;
            }
        }
        public override IDbDataParameter CreateParameter()
		{
            return (IDbDataParameter)ClassLoader.CreateInstance(Db2Assembly, $"{Db2AssemblyName}.DB2Parameter");
		}
		public override  IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec)
		{
            IDbDataParameter parm = (IDbDataParameter)ClassLoader.CreateInstance(Db2Assembly, $"{Db2AssemblyName}.DB2Parameter");
#if NETCORE
			parm.DbType = GXTypeToDbType((GXType)dbtype);
#else
			ClassLoader.SetPropValue(parm, "DB2Type", GXTypeToDB2DbType((GXType)dbtype));
#endif

			ClassLoader.SetPropValue(parm, "Size", gxlength);//This property is used for binary and strings types
            ClassLoader.SetPropValue(parm, "Precision", (byte)gxlength);//This property is used only for decimal and numeric input parameters
            ClassLoader.SetPropValue(parm, "Scale", (byte)gxdec);//This property is used only for decimal and numeric input parameters
            ClassLoader.SetPropValue(parm, "ParameterName", name);
			return parm;
		}
		private DbType GXTypeToDbType(object type)
		{
			if (!(type is GXType))
				return (DbType)type;

			switch (type)
			{
				case GXType.Byte: return DbType.Binary;
				case GXType.Int16: return DbType.Int16;
				case GXType.Int32: return DbType.Int32;
				case GXType.Int64: return DbType.Int64;
				case GXType.Number: return DbType.Decimal;
				case GXType.DateTime: return DbType.DateTime;
				case GXType.DateTime2: return DbType.DateTime2;
				case GXType.Date: return DbType.Date;
				case GXType.Boolean: return DbType.Boolean;
				case GXType.UniqueIdentifier:
				case GXType.VarChar:
				case GXType.Char: return DbType.String;
				case GXType.Blob: return DbType.Binary;
				case GXType.Geography:
				case GXType.Geoline:
				case GXType.Geopoint:
				case GXType.Geopolygon:
					return DbType.String;
				default: return DbType.String;
			}
		}
		private Object GXTypeToDB2DbType(GXType type)
		{
			switch (type)
			{
				case GXType.Int16: return ClassLoader.GetEnumValue(_db2Assembly, dB2DbTypeEnum, "SmallInt");
				case GXType.Int32: return ClassLoader.GetEnumValue(_db2Assembly, dB2DbTypeEnum, "Integer");
				case GXType.Int64: return ClassLoader.GetEnumValue(_db2Assembly, dB2DbTypeEnum, "BigInt");
				case GXType.Number: return ClassLoader.GetEnumValue(_db2Assembly, dB2DbTypeEnum, "Float");
				case GXType.DateTime: return ClassLoader.GetEnumValue(_db2Assembly, dB2DbTypeEnum, "Timestamp");
                case GXType.DateTime2: return ClassLoader.GetEnumValue(_db2Assembly, dB2DbTypeEnum, "Timestamp");
				case GXType.UniqueIdentifier:
				case GXType.Char: return ClassLoader.GetEnumValue(_db2Assembly, dB2DbTypeEnum, "Char");
				case GXType.Date: return ClassLoader.GetEnumValue(_db2Assembly, dB2DbTypeEnum, "Date");
				case GXType.Clob: return ClassLoader.GetEnumValue(_db2Assembly, dB2DbTypeEnum, "Clob");
				case GXType.VarChar: return ClassLoader.GetEnumValue(_db2Assembly, dB2DbTypeEnum, "VarChar");
				case GXType.Blob: return ClassLoader.GetEnumValue(_db2Assembly, dB2DbTypeEnum, "Blob");

				default: return ClassLoader.GetEnumValue(_db2Assembly, dB2DbTypeEnum, type.ToString());
			}
		}
		public override DbDataAdapter CreateDataAdapeter()
        {
            Type iAdapter = Db2Assembly.GetType($"{Db2AssemblyName}.DB2DataAdapter");
            return (DbDataAdapter)Activator.CreateInstance(iAdapter);
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
			GXLogging.Debug(log, "ExecuteReader: client cursor=" + hasNested +", handle '"+ handle + "'"+ ", hashcode " + this.GetHashCode());
			idatareader= new GxDataReader(connManager,this, con,parameters,stmt,fetchSize,forFirst,handle,cached,expiration,dynStmt);
			return idatareader;

		}
		public override void AddParameters(IDbCommand cmd, GxParameterCollection parameters)
		{
			for (int j = 0; j < parameters.Count; j++)
			{
				try
				{
					cmd.Parameters.Add(CloneParameter(parameters[j]));
				}
				catch (Exception ex)//System.ArgumentException ParameterName 'XXXX' is already contained by this DB2ParameterCollection
				{
	                GXLogging.Warn(log, "AddParameters error", ex);
					if (cmd.Parameters.Contains(parameters[j].ParameterName))
					{
						parameters[j].ParameterName = parameters[j].ParameterName + j.ToString();
					}
					cmd.Parameters.Add(CloneParameter(parameters[j]));
				}
			}
		}
		public override bool IsBlobType(IDbDataParameter idbparameter)
		{
#if NETCORE
			return idbparameter.DbType == DbType.Binary;
#else
			object otype = ClassLoader.GetPropValue(idbparameter, "DB2Type");
            object blobType = ClassLoader.GetEnumValue(Db2Assembly, dB2DbTypeEnum, "Blob");
            return (int)otype == (int)blobType;
#endif
		}

		public override void SetParameter(IDbDataParameter parameter, Object value)
		{
			if (value==null || value==DBNull.Value)
			{
				parameter.Value = DBNull.Value;
			}
			else if (!IsBlobType(parameter)) 
			{
				if (value!=null)
				{
					if (value is Guid)
						parameter.Value = value.ToString();
					else if (value is bool)
						parameter.Value = ((bool)value) ? 1 : 0;
					else
						parameter.Value = CheckDataLength(value, parameter);
				}
			}
			else
			{
				SetBinary(parameter, GetBinary((string)value, false));
			}
		}
		public override string GetServerDateTimeStmt(IGxConnection connection)
		{
			return "SELECT CURRENT TIMESTAMP FROM SYSIBM.SYSTABLES WHERE NAME = 'SYSTABLES' AND CREATOR = 'SYSIBM'";
		}
		public override string GetServerDateTimeStmtMs(IGxConnection connection)
		{
			return GetServerDateTimeStmt(connection);
		}
		public override string GetServerVersionStmt()
		{
			throw new GxNotImplementedException();
		}
		public override string GetServerUserIdStmt()
		{
			return "VALUES(USER)";
		}
		public override bool ProcessError( int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)
			
		{
			 
			GXLogging.Debug(log, "ProcessError: dbmsErrorCode=" + dbmsErrorCode +", emsg '"+ emsg + "'"+ ", status " + status);
			switch (dbmsErrorCode)
			{
				case -911:		// Locked
					retry = Retry(errMask, retryCount);
					if (retry)
						status=110;// Locked - Retry
					else 
						status=103;//Locked
					return retry;
				case -803:		// Duplicated record
					status = 1; 
					break;
				case -204:		// File not found
					status = 105; 
					break;
				case -530:		// Parent key not found
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
				return "'0001-01-01'";
			return "TIMESTAMP('"+
				Value.Year.ToString()+ "-"+
				Value.Month.ToString()+"-"+
				Value.Day.ToString()+"-"+
				Value.Hour.ToString()+"."+
				Value.Minute.ToString()+"."+
				Value.Second.ToString()+"')";
		}
		public override DateTime DTFromString( string s)
		{
			string strim = s.Trim();

			if (strim.Length == 0)
				return DateTime.MinValue;
			else if (strim.Length == 8)
			{
				if (strim==SQL_NULL_DATE_8)
				{
					return DateTimeUtil.NullDate();
				}
				else
					return new DateTime( 
						Convert.ToInt32(s.Substring(0, 2)), 
						Convert.ToInt32(s.Substring(3, 2)), 
						Convert.ToInt32(s.Substring(6, 2)),
						0,0,0);
			}
			else if (strim.Length == 10)
			{
				if (strim==SQL_NULL_DATE_10)
					return DateTimeUtil.NullDate();
				else
					return new DateTime( 
						Convert.ToInt32(s.Substring(0, 4)), 
						Convert.ToInt32(s.Substring(5, 2)), 
						Convert.ToInt32(s.Substring(8, 2)),
						0,0,0);
			}
			else
				return new DateTime( 
					Convert.ToInt32(s.Substring(0, 4)), 
					Convert.ToInt32(s.Substring(5, 2)), 
					Convert.ToInt32(s.Substring(8, 2)),
					Convert.ToInt32(s.Substring(11, 2)),
					Convert.ToInt32(s.Substring(14, 2)),
					Convert.ToInt32(s.Substring(17, 2)));
		}
		internal override object CloneParameter(IDbDataParameter p)
		{
            return ((ICloneable)p).Clone();
		}


		/* db2400:the same command cannot be executed a second time without first
		* close it (it is possible if they are different commands), then if it is cached,
		* and it is not closed, it cannot be used, a new one is created*/
		protected override IDbCommand GetCachedCommand(IGxConnection con, string stmt)
		{
			return 	con.ConnectionCache.GetAvailablePreparedCommand(stmt);
		}

		private static readonly string[] ConcatOpValues = new string[] { string.Empty, " CONCAT ", string.Empty };
		public override string ConcatOp(int pos)
		{
			return ConcatOpValues[pos];
		}

	}

	sealed internal class Db2ConnectionWrapper : GxAbstractConnectionWrapper 
	{
		private static int changeConnState=-1;
		private static int changeConnStateExecuting = -1;
		private int openDataReaders;
		static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public Db2ConnectionWrapper()
		{
            try
            {
                _connection = (IDbConnection)ClassLoader.CreateInstance(GxDb2.Db2Assembly, $"{GxDb2.Db2AssemblyName}.DB2Connection");
            }
            catch (Exception ex)
            {
                GXLogging.Error(log, "DB2 data provider Ctr error " + ex.Message + ex.StackTrace);
                throw ex;
            }
        }

		public Db2ConnectionWrapper(String connectionString, GxConnectionCache connCache, IsolationLevel isolationLevel) 
		{
            try
            {
                _connection = (IDbConnection)ClassLoader.CreateInstance(GxDb2.Db2Assembly, $"{GxDb2.Db2AssemblyName}.DB2Connection", new object[] { connectionString });
                m_isolationLevel = isolationLevel;
                m_connectionCache = connCache;
            }
            catch (Exception ex)
            {
                GXLogging.Error(log, "iSeries data provider Ctr error " + ex.Message + ex.StackTrace);
                throw ex;
            }
		}
		override public void CheckStateInc()
		{
			if (openDataReaders>0)
			{
                ChangeConnectionState(InternalConnection);
				GXLogging.Debug(log, "CheckStateInc, ChangeConnectionState");
			}
            Interlocked.Increment(ref openDataReaders);
		}
		override public void CheckStateDec()
		{
            Interlocked.Decrement(ref openDataReaders);
		}
		override public void CheckState(bool value)
		{
			if (!value && openDataReaders>0)
			{
                ChangeConnectionState(InternalConnection);
				GXLogging.Debug(log, "CheckState, ChangeConnectionState");
			}
		}
		private void ChangeConnectionState(IDbConnection con)
		{
			MethodInfo m;
			if (changeConnState == -1)
			{
				
				m = con.GetType().GetMethod("SetStateFetchingFalse", BindingFlags.Instance | BindingFlags.NonPublic);
				changeConnState = (m == null) ? 0 : 1;
				GXLogging.Debug(log, "ChangeConnectionState changeConnState:" + changeConnState);
			}

			if (changeConnState==1)
			{
				con.GetType().InvokeMember("SetStateFetchingFalse",
					BindingFlags.Instance|
					BindingFlags.NonPublic|
					BindingFlags.InvokeMethod,
					null, con, null);
			}

			if (changeConnStateExecuting == -1)
			{
				
				m = con.GetType().GetMethod("SetStateExecutingFalse", BindingFlags.Instance | BindingFlags.NonPublic);
				changeConnStateExecuting = (m == null) ? 0 : 1;
				GXLogging.Debug(log, "ChangeConnectionState changeConnStateExecuting:" + changeConnStateExecuting);
			}
			
			if (changeConnStateExecuting==1)
			{
				con.GetType().InvokeMember("SetStateExecutingFalse",
				BindingFlags.Instance |
				BindingFlags.NonPublic |
				BindingFlags.InvokeMethod,
				null, con, null);
			}
		}

		override public void Open() 
		{
			try 
			{
				InternalConnection.Open();
				if (!m_autoCommit)
				{
					m_transaction = InternalConnection.BeginTransaction(m_isolationLevel);  
				}
				else
				{
					m_transaction =null;
				}

			}
			catch(Exception e)
			{
				GXLogging.Error(log, "Return GxConnection.Open Error " , e);
				throw (new GxADODataException(e));
			}
		}
        public override void CheckCursors(int OpenHandles, bool beforeCommitRollback)
        {
			try
			{
				if (beforeCommitRollback)
                    CheckState(false);
			}
			catch (Exception ex)
			{
                GXLogging.Error(log, "CheckCursors error", ex);
			}
		}
		override public void Close() 
		{
			try 
			{
				CheckState(false);
				InternalConnection.Close();
			}
			catch(Exception ex) 
			{
				throw new DataException(ex.Message, ex);
			}
		}

		override public IDbCommand CreateCommand() 
		{
            return InternalConnection.CreateCommand();
        }
		public override DbDataAdapter CreateDataAdapter()
		{
			throw new GxNotImplementedException();
		}

		override public IDbTransaction BeginTransaction(IsolationLevel isoLevel) 
		{
			try
			{
				IDbTransaction trn= InternalConnection.BeginTransaction(isoLevel);
				return trn;
			}
			catch (Exception e)
			{
				GXLogging.Warn(log, "BeginTransaction Error ", e);
				IDbTransaction trn= InternalConnection.BeginTransaction(IsolationLevel.Unspecified);
				if (trn.IsolationLevel!=isoLevel)
				{
					GXLogging.Error(log, "BeginTransaction Error, could not open new transaction isoLevel=" + isoLevel, e);
					throw new GxADODataException("Begin transaction error in DB2", e);
				}
				return trn;
			}
		}


	}

}