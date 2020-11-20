using GeneXus.Application;
using GeneXus.Cache;
using GeneXus.Utils;
using log4net;
using MySQLCommand = MySqlConnector.MySqlCommand;
using MySQLParameter = MySqlConnector.MySqlParameter;
using MySQLConnection = MySqlConnector.MySqlConnection;
using MySQLException = MySqlConnector.MySqlException;
using MySQLDbType = MySqlConnector.MySqlDbType;
using MySQLDataAdapter = MySqlConnector.MySqlDataAdapter;
using System.IO;
using GxClasses.Helpers;
using System.Reflection;
using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Collections.Generic;
using System.Security;

namespace GeneXus.Data
{
	
	public class GxMySqlConnector : GxDataRecord 
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.GxMySqlConnector));
		private int MAX_TRIES;
		private int m_FailedConnections;
#if NETCORE
		static Assembly assembly;
#endif
		public GxMySqlConnector(string id)
		{
#if NETCORE
			if (assembly == null)
			{
				using (var dynamicContext = new AssemblyResolver(Path.Combine(FileUtil.GetStartupDirectory(), "MySqlConnector.dll")))
				{
					assembly = dynamicContext.Assembly;
				}
				using (var dynamicContext = new AssemblyResolver(Path.Combine(FileUtil.GetStartupDirectory(), "System.Configuration.ConfigurationManager.dll"))) { }
			}
#endif

		}
		public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra, GxConnectionCache connectionCache)
		{
			if (m_connectionString == null)
				m_connectionString = BuildConnectionString(datasourceName, userId, userPassword, databaseName, port, schema, extra);
			GXLogging.Debug(log, "Setting connectionString property ", ConnectionStringForLog);
			m_FailedConnections = 0;
			return new MySqlConnectorConnectionWrapper(m_connectionString, connectionCache, isolationLevel);
		}

		string convertToMySqlCall(string stmt, GxParameterCollection parameters)
		{
			if (parameters == null)
				return "";
			string pname;
			StringBuilder sBld = new StringBuilder();
			for (int i = 0; i < parameters.Count; i++)
			{
				if (i > 0)
					sBld.Append(", ");
				pname = "@" + parameters[i].ParameterName;  
				sBld.Append(pname);
				parameters[i].ParameterName = pname;
			}
			return "CALL " + stmt + "(" + sBld.ToString() + ")";    
		}
		[SecuritySafeCritical]
		public override IDbCommand GetCommand(IGxConnection con, string stmt, GxParameterCollection parameters, bool isCursor, bool forFirst, bool isRpc)
		{
			if (isRpc)
				stmt = convertToMySqlCall(stmt, parameters);
			MySQLCommand mysqlcmd = (MySQLCommand)base.GetCommand(con, stmt, parameters.Distinct());
			if (isCursor && !isRpc)        
			{
				mysqlcmd.Prepare();
			}
			return mysqlcmd;
		}
		public override string AfterCreateCommand(string stmt, GxParameterCollection parmBinds)
		{
			if (parmBinds == null  || parmBinds.Count==0 || stmt.IndexOf('?') < 0 )
				return stmt;
			bool inString = false;
			int parmIndex = 0;
			StringBuilder sBld = new StringBuilder();
			for (int i = 0; i < stmt.Length; i++)
			{
				char c = stmt[i];
				if (c == '`')
				{
					inString = !inString;
					sBld.Append(c);
				}
				else if (c == '?' && !inString)
				{
					sBld.Append('@');
					sBld.Append(parmBinds[parmIndex].ParameterName);
					parmIndex++;
				}
				else
				{
					sBld.Append(c);
				}
			}
			return sBld.ToString();
		}
		protected override string BuildConnectionString(string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra)
		{
			StringBuilder connectionString = new StringBuilder();

            if (!string.IsNullOrEmpty(datasourceName) && !hasKey(extra, "Server"))
			{
				connectionString.AppendFormat("Server={0};", datasourceName);
			}
            if (port != null && port.Trim().Length > 0 && !hasKey(extra, "Port"))
			{
				connectionString.AppendFormat("Port={0};", port);
			}
			if (userId != null)
			{
				connectionString.AppendFormat(";User ID={0};Password={1}", userId, userPassword);
			}
            if (databaseName != null && databaseName.Trim().Length > 0 && !hasKey(extra, "Database"))
            {
                connectionString.AppendFormat(";Database={0}", databaseName);
            }
            if (extra != null)
			{
				string res = ParseAdditionalData(extra, "integrated security");
                if (!res.StartsWith(";") && res.Length > 1) connectionString.Append(";");
				connectionString.Append(res);
			}
			string connstr = connectionString.ToString();
			string maxpoolSize = GetParameterValue(connstr, "Max Pool Size");
			if (String.IsNullOrEmpty(maxpoolSize)) MAX_TRIES = 100; 
			else MAX_TRIES = Convert.ToInt32(maxpoolSize);
			GXLogging.Debug(log, "MAX_TRIES=" + MAX_TRIES);
			return connstr;
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
			return new MySQLParameter();
		}
		[SecuritySafeCritical]
		public override IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec)
		{
			MySQLParameter parm = new MySQLParameter();
			parm.DbType = (DbType)dbtype;
            parm.MySqlDbType = DbtoMysqlType((DbType)dbtype);
            parm.Size = gxlength;
			parm.Precision = (byte)gxlength;
			parm.Scale = (byte)gxdec;
			parm.ParameterName = name;
			return parm;
		}
		[SecuritySafeCritical]
		public static MySQLDbType DbtoMysqlType(DbType dbtype)
        {
            switch (dbtype)
            {
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                case DbType.String:
                    return MySQLDbType.String;
                case DbType.AnsiString:
                    return MySQLDbType.VarChar;
                case DbType.Binary:
                    return MySQLDbType.Blob;
                case DbType.Boolean:
                    return MySQLDbType.Bit;
                case DbType.Byte:
                    return MySQLDbType.TinyText;
                case DbType.Date:
                    return MySQLDbType.Date;
                case DbType.DateTime:
                    return MySQLDbType.DateTime;
                case DbType.DateTime2:
                    return MySQLDbType.DateTime;
                case DbType.Decimal:
                    return MySQLDbType.Decimal;
                case DbType.Double:
                    return MySQLDbType.Double;
                case DbType.Guid:
                    return MySQLDbType.Guid;
                case DbType.Int16:
                    return MySQLDbType.Int16;
                case DbType.Int32:
                    return MySQLDbType.Int32;
                case DbType.Int64:
                    return MySQLDbType.Int64;
                case DbType.Single:
                    return MySQLDbType.Decimal;
				case DbType.Time:
                    return MySQLDbType.Time;
                default:
                    return MySQLDbType.Int16;

            }
        }
		public override DbDataAdapter CreateDataAdapeter()
		{
			return new MySQLDataAdapter();
		}
        public override IDataReader GetDataReader(
			IGxConnectionManager connManager,
			IGxConnection con,
			GxParameterCollection parameters,
			string stmt, ushort fetchSize,
			bool forFirst, int handle,
			bool cached, SlidingTime expiration,
			bool hasNested,
			bool dynStmt)
		{
            IDataReader idatareader;
			if (!hasNested)//Client Cursor
			{
				GXLogging.Debug(log, "ExecuteReader: client cursor=" + hasNested + ", handle '" + handle + "'" + ", hashcode " + this.GetHashCode());
				idatareader = new GxMySQLConnectorDataReader(connManager, this, con, parameters.Distinct(), stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt);
			}
			else //Server Cursor
			{
				GXLogging.Debug(log, "ExecuteReader: server cursor=" + hasNested + ", handle '" + handle + "'" + ", hashcode " + this.GetHashCode());
                idatareader = new GxMySqlMemoryDataReader(connManager, this, con, parameters.Distinct(), stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt);
            }
            return idatareader;
        }
        protected override IDbCommand GetCachedCommand(IGxConnection con, string stmt)
		{
			return con.ConnectionCache.GetAvailablePreparedCommand(stmt);
		}
		public override IDataReader GetCacheDataReader(CacheItem item, bool computeSize, string keyCache)
		{
			return new GxMySQLCacheDataReader(item, computeSize, keyCache);
		}
		public override string GetServerDateTimeStmt(IGxConnection connection)
		{
			return "SELECT NOW()";
		}
		public override string GetServerDateTimeStmtMs(IGxConnection connection)
		{
			return "SELECT NOW(3)";
		}
		public override string GetServerUserIdStmt()
		{
			return "SELECT USER()";
		}
		public override string GetServerVersionStmt()
		{
			return "SELECT VERSION()";
		}


		public override IGeographicNative Dbms2NetGeo(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return new Geospatial(DR.GetString(i));
		}
		public override Object Net2DbmsGeo(IDbDataParameter parm, IGeographicNative geo)
		{
			return geo.ToStringSQL();
		}


		public override bool ProcessError(int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)
		{
			GXLogging.Debug(log, "ProcessError: dbmsErrorCode=" + dbmsErrorCode + ", emsg '" + emsg + "'");
			switch (dbmsErrorCode)
			{
				case 2006://MySQL server has gone away
				case 1042: //SSL Authentication Error
					if (con != null && m_FailedConnections < MAX_TRIES)//Retry if it is an Open operation.
					{
						try
						{
							if (con.Opened)
								con.Close();
						}
						catch { }
						status = 104; // MySQL server has gone away.  Check your network documentation - Retry [Max Pool Size] times.
						m_FailedConnections++;
						con.Close();
						retry = true;
						GXLogging.Debug(log, "ProcessError: MySQL server has gone away, FailedConnections:" + m_FailedConnections);
					}
					else
					{
						return false;
					}
					break;
				case 1006: //Can't create database 'databaseName'
				case 1007: //Can't create database 'databaseName'. Database exists
					break;
				case 1205:      // Locked
					retry = Retry(errMask, retryCount);
					if (retry)
						status = 110;// Locked - Retry
					else
						status = 103;//Locked
					return retry;
				case 1062:      // Duplicated record ER_DUP_ENTRY
					status = 1;
					break;
				case 1051:      // File not found ER_BAD_TABLE_ERROR 
				case 1091:      // File not found ER_CANT_DROP_FIELD_OR_KEY 
								//case 1146:		// File not found ER_NO_SUCH_TABLE  => Do not catch it, it must be caught by the Reorganization
								//case 1064:		// File not found ER_PARSE_ERROR => Do not catch it, it is thrown when there is a sintax error
				case 1025:      // (Error on rename of) ER_CANT_DROP_FKEY
					status = 105;
					break;
				case 1146:
					if (!GxContext.isReorganization)
					{
						status = 105;
						return false;
					}
					else
					{
						status = 999;
						return false;
					}
				case 1216:      //foreign key constraint fails
					if ((errMask & GxErrorMask.GX_MASKFOREIGNKEY) == 0)
					{
						status = 500;       // ForeignKeyError
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
				return "TO_DATE('0001-01-01', 'YYYY-MM-DD')";
			return "to_date( '" +
				Value.Year.ToString() + "-" +
				Value.Month.ToString() + "-" +
				Value.Day.ToString() + " " +
				Value.Hour.ToString() + ":" +
				Value.Minute.ToString() + ":" +
				Value.Second.ToString() + "', 'YYYY-MM-DD HH24:MI:SS')";
		}

		public override DateTime Dbms2NetDate(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return Dbms2NetDateTime(DR.GetDateTime(i), false);
		}

		public override DateTime Dbms2NetDateTime(DateTime dt, Boolean precision)
		{
			
			if (dt.Equals(MYSQL_NULL_DATE))
			{
				return DateTimeUtil.NullDate();
			}

			if (dt.Year == MYSQL_NULL_DATE.Year &&
				dt.Month == MYSQL_NULL_DATE.Month &&
				dt.Day == MYSQL_NULL_DATE.Day)
			{

				return new DateTime(
					DateTimeUtil.NullDate().Year, DateTimeUtil.NullDate().Month,
					DateTimeUtil.NullDate().Day, dt.Hour, dt.Minute, dt.Second, ((precision)? dt.Millisecond:0));
			}
			return (precision)? DateTimeUtil.ResetMicroseconds(dt): DateTimeUtil.ResetMilliseconds(dt);
		}

		public override Object Net2DbmsDateTime(IDbDataParameter parm, DateTime dt)
		{
			
			if (dt.Equals(DateTimeUtil.NullDate()))
			{
				return MYSQL_NULL_DATE;
			}

			if (dt.CompareTo(MYSQL_NULL_DATE) < 0)
			{
				DateTime aux =
					new DateTime(
					MYSQL_NULL_DATE.Year, MYSQL_NULL_DATE.Month,
					MYSQL_NULL_DATE.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond );
				
				return aux;
			}
			else
			{
				return dt;
			}
		}

		static DateTime MYSQL_NULL_DATE = new DateTime(1000, 1, 1);
		public override IGeographicNative GetGeospatial(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			if (!cmd.HasMoreRows || DR == null || DR.IsDBNull(i))
				return new Geospatial();
			else
			{
				
				Geospatial gtmp = new Geospatial();
				String geoStr = DR.GetString(i);
				
				gtmp.FromString(geoStr);
				return gtmp;
			}
		}
		public override string GetString(IGxDbCommand cmd, IDataRecord DR, int i, int size)
		{
			if (!cmd.HasMoreRows || DR == null || DR.IsDBNull(i))
				return string.Empty;
			else
			{
				string value = DR.GetString(i);
				if (value != null && value.Length < size)
					value = value.PadRight(size);
				return value;
			}
		}

		public override void SetParameter(IDbDataParameter parameter, object value)
		{
			if (value is Guid)
			{
				parameter.Value = value.ToString();
				
			}
			else if (value is bool && parameter.DbType == DbType.Byte ) {
				bool boolValue = (bool)value;
				parameter.Value = boolValue ? (byte)1 : (byte)0;
			}
			else
				base.SetParameter(parameter, value);
		}
	}

	[SecuritySafeCritical]
	sealed internal class MySqlConnectorConnectionWrapper : GxAbstractConnectionWrapper
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.MySqlConnectorConnectionWrapper));
		[SecuritySafeCritical]
		public MySqlConnectorConnectionWrapper() : base(new MySQLConnection())
		{ }
		[SecuritySafeCritical]
		public MySqlConnectorConnectionWrapper(String connectionString, GxConnectionCache connCache, IsolationLevel isolationLevel) : base(new MySQLConnection(connectionString), isolationLevel)
		{
			m_connectionCache = connCache;
		}
		[SecuritySafeCritical]
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
					m_transaction = null;
				}
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Return GxConnection.Open Error ", e);
				throw (new GxADODataException(e));
			}
		}
		override public void Close()
		{
			try
			{
				InternalConnection.Close();
			}
			catch (MySQLException ex)
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
		public override short SetSavePoint(IDbTransaction transaction, string savepointName)
		{
			
			return 0;
		}
		public override short ReleaseSavePoint(IDbTransaction transaction, string savepointName)
		{
			return 0;
		}
		public override short RollbackSavePoint(IDbTransaction transaction, string savepointName)
		{
			
			return 0;
		}
	}
	[SecuritySafeCritical]
	public class GxMySQLConnectorDataReader : GxDataReader
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GxMySQLConnectorDataReader));
		public GxMySQLConnectorDataReader(IGxConnectionManager connManager, GxDataRecord dr, IGxConnection connection, GxParameterCollection parameters,
			string stmt, int fetchSize, bool forFirst, int handle, bool cached, SlidingTime expiration, bool dynStmt)
		{
			this.parameters = parameters;
			this.stmt = stmt;
			this.fetchSize = fetchSize;
			this.cache = connection.ConnectionCache;
			this.cached = cached;
			this.handle = handle;
			this.isForFirst = forFirst;
			_connManager = connManager;
			this.m_dr = dr;
			this.readBytes = 0;
			this.dynStmt = dynStmt;
			con = _connManager.IncOpenHandles(handle, m_dr.DataSource);
			con.CurrentStmt = stmt;
			con.MonitorEnter();
			MySQLCommand cmd = (MySQLCommand)dr.GetCommand(con, stmt, parameters);
			cmd.Prepare();
			reader = cmd.ExecuteReader();
			cache.SetAvailableCommand(stmt, false, dynStmt);
			open = true;
			block = new GxArrayList(fetchSize);
			pos = -1;
			if (cached)
			{
				key = SqlUtil.GetKeyStmtValues(parameters, stmt, isForFirst);
				this.expiration = expiration;
			}
		}
		public override string GetString(int i)
		{
			string res;
			try
			{
				res = Convert.ToString(reader.GetString(i));
			}
			catch (InvalidCastException ex)
			{
				GXLogging.Warn(log, "GetString error", ex);
				res = Convert.ToString(reader.GetValue(i).ToString());
			}
			readBytes += 10 + (2 * res.Length);
			return res;

		}

	}
	[SecuritySafeCritical]
	public class GxMySQLConnectorCursorDataReader : GxDataReader
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.GxDataReader));

		public GxMySQLConnectorCursorDataReader(IGxConnectionManager connManager, GxDataRecord dr, IGxConnection connection, GxParameterCollection parameters,
			string stmt, int fetchSize, bool forFirst, int handle, bool cached, SlidingTime expiration, bool hasNested, bool dynStmt)
		{
			this.parameters = parameters;
			this.stmt = stmt;
			this.fetchSize = fetchSize;
			this.cache = connection.ConnectionCache;
			this.cached = cached;
			this.handle = handle;
			this.isForFirst = forFirst;
			_connManager = connManager;
			this.m_dr = dr;
			this.readBytes = 0;
			this.dynStmt = dynStmt;
			con = _connManager.IncOpenHandles(handle, m_dr.DataSource);
			con.CurrentStmt = stmt;
			con.MonitorEnter();
			GXLogging.Debug(log, "Open GxMySQLCursorDataReader handle:'" + handle);
			MySQLCommand cmd = (MySQLCommand)dr.GetCommand(con, stmt, parameters);
			reader = cmd.ExecuteReader();
			cache.SetAvailableCommand(stmt, false, dynStmt);
			open = true;
			block = new GxArrayList(fetchSize);
			pos = -1;
			if (cached)
			{
				key = SqlUtil.GetKeyStmtValues(parameters, stmt, isForFirst);
				this.expiration = expiration;
			}
		}

		public override string GetString(int i)
		{
			string res = Convert.ToString(reader.GetString(i));
			readBytes += 10 + (2 * res.Length);
			return res;

		}
		public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			try
			{
				long res = reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
				readBytes += res;
				return res;
			}catch(IndexOutOfRangeException)
			{
				return 0;
			}
		}
	}
	public class GxMySQLCacheDataReader : GxCacheDataReader
	{

		public GxMySQLCacheDataReader(CacheItem cacheItem, bool computeSize, string keyCache)
			: base(cacheItem, computeSize, keyCache)
		{ }

		public override string GetString(int i)
		{
			string result;

			if (block.Item(pos, i) is byte[])
				result = Encoding.UTF8.GetString((byte[])block.Item(pos, i));
			else
				result = (string)block.Item(pos, i);

			if (computeSizeInBytes) readBytes += 10 + (2 * result.Length);
			return result;
		}
		public override DateTime GetDateTime(int i)
		{
			if (block.Item(pos, i) is DateTime)
			{
				return base.GetDateTime(i);
			}
			else
			{
				if (computeSizeInBytes) readBytes += 8;
				return Convert.ToDateTime(block.Item(pos, i), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
			}
		}

	}

	public class GxMySqlMemoryDataReader : GxDataReader
	{
		public GxMySqlMemoryDataReader(IGxConnectionManager connManager, GxDataRecord dr, IGxConnection connection, GxParameterCollection parameters,
			string stmt, ushort fetchSize, bool forFirst, int handle, bool cached, SlidingTime expiration, bool dynStmt) : base(connManager, dr, connection, parameters,
			stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt)
		{
			MemoryDataReader memoryDataReader = new MemoryDataReader(reader, con, parameters, stmt, fetchSize, isForFirst, cached, expiration);
			reader.Dispose();
			reader = memoryDataReader;
		}
		public override string GetString(int i)
		{
			string res = Convert.ToString(reader.GetString(i));
			readBytes += 10 + (2 * res.Length);
			return res;

		}
	}
}
