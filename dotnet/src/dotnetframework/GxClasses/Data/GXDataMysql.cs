using GeneXus.Application;
using GeneXus.Cache;
using GeneXus.Utils;
using log4net;
#if NETCORE
using MySQLCommand = MySql.Data.MySqlClient.MySqlCommand;
using MySQLParameter = MySql.Data.MySqlClient.MySqlParameter;
using MySQLConnection = MySql.Data.MySqlClient.MySqlConnection;
using MySQLException = MySql.Data.MySqlClient.MySqlException;
using MySQLDbType = MySql.Data.MySqlClient.MySqlDbType;
using MySQLDataAdapter = MySql.Data.MySqlClient.MySqlDataAdapter;
using System.IO;
using GxClasses.Helpers;
using System.Reflection;
#else
using MySQLDriverCS;
#endif
using System;
using System.Data;
using System.Data.Common;
using System.Text;

namespace GeneXus.Data
{
	public class GxMySql : GxDataRecord 
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.GxMySql));
		private int MAX_TRIES;
		private int m_FailedConnections;
		private bool preparedStmts;
#if NETCORE
		static Assembly assembly;
#endif
		public GxMySql(string id)
		{
#if NETCORE
			if (assembly == null)
			{
				using (var dynamicContext = new AssemblyResolver(Path.Combine(FileUtil.GetStartupDirectory(), "MySql.Data.dll")))
				{
					assembly = dynamicContext.Assembly;
				}
				using (var dynamicContext = new AssemblyResolver(Path.Combine(FileUtil.GetStartupDirectory(), "System.Configuration.ConfigurationManager.dll"))) { }
			}
#else
			if (GxContext.isReorganization && !GXUtil.ExecutingRunX86())
			{
				MYSQL_FIELD_FACTORY.GetInstance();//Force libmysql load
			}
#endif

	}
		public GxMySql(string id, bool prepStmt) : this(id)
		{
			preparedStmts = prepStmt;
		}
		public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra, GxConnectionCache connectionCache)
		{
			if (m_connectionString == null)
				m_connectionString = BuildConnectionString(datasourceName, userId, userPassword, databaseName, port, schema, extra);
			GXLogging.Debug(log, "Setting connectionString property ", ConnectionStringForLog);
			m_FailedConnections = 0;
			return new MySqlConnectionWrapper(m_connectionString, connectionCache, isolationLevel);
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
		public override IDbCommand GetCommand(IGxConnection con, string stmt, GxParameterCollection parameters, bool isCursor, bool forFirst, bool isRpc)
		{
			if (isRpc)
				stmt = convertToMySqlCall(stmt, parameters);    
			MySQLCommand mysqlcmd = (MySQLCommand)base.GetCommand(con, stmt, parameters);

			if (preparedStmts && isCursor && !isRpc)        
			{
#if !NETCORE
				mysqlcmd.UsePreparedStatement = true;
#endif
				mysqlcmd.Prepare();
			}
			return mysqlcmd;
		}
		protected override string BuildConnectionString(string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra)
		{
			StringBuilder connectionString = new StringBuilder();
#if NETCORE

            if (!string.IsNullOrEmpty(datasourceName) && !hasKey(extra, "Server"))
			{
				connectionString.AppendFormat("Server={0};", datasourceName);
			}
#else
            if (!string.IsNullOrEmpty(datasourceName) && !hasKey(extra, "Location"))
			{
				connectionString.AppendFormat("Location={0};", datasourceName);
			}
#endif
            if (port != null && port.Trim().Length > 0 && !hasKey(extra, "Port"))
			{
				connectionString.AppendFormat("Port={0};", port);
			}
			if (userId != null)
			{
				connectionString.AppendFormat(";User ID={0};Password={1}", userId, userPassword);
			}
#if NETCORE
            if (databaseName != null && databaseName.Trim().Length > 0 && !hasKey(extra, "Database"))
            {
                connectionString.AppendFormat(";Database={0}", databaseName);
            }
#else
            if (databaseName != null && databaseName.Trim().Length > 0 && !hasKey(extra, "Data Source"))
			{
				connectionString.AppendFormat(";Data Source={0}", databaseName);
			}
#endif
            if (extra != null)
			{
				string res = ParseAdditionalData(extra, "integrated security");
#if !NETCORE
                res = ReplaceKeyword(res, "database", "Data Source");
#endif
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
				return preparedStmts;
			}
		}
		public override IDbDataParameter CreateParameter()
		{
			return new MySQLParameter();
		}
		public override IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec)
		{
			MySQLParameter parm = new MySQLParameter();
			parm.DbType = GXTypeToDbType(dbtype);
#if NETCORE
            parm.MySqlDbType = DbtoMysqlType(GXTypeToDbType(dbtype));
#endif

			parm.Size = gxlength;
			parm.Precision = (byte)gxlength;
			parm.Scale = (byte)gxdec;
			parm.ParameterName = name;
			return parm;
		}
		private DbType GXTypeToDbType(object type)
		{
			if (type is DbType)
				return (DbType)type;

			switch (type)
			{
				case GXType.Byte: return DbType.Byte;
				case GXType.Int16: return DbType.Int16;
				case GXType.Int32: return DbType.Int32;
				case GXType.Int64: return DbType.Int64;
				case GXType.Number: return DbType.Single;
				case GXType.DateTime: return DbType.DateTime;
				case GXType.DateTime2: return DbType.DateTime2;
				case GXType.Date: return DbType.Date;
				case GXType.Boolean: return DbType.Byte;
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
#if NETCORE
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
#endif
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
#if !NETCORE
            GXLogging.Debug(log, "ExecuteReader: client cursor=" + hasNested + ", handle '" + handle + "'" + ", hashcode " + this.GetHashCode() + " PreparedStmt " + preparedStmts);
			if (preparedStmts)
				idatareader = new GxMySQLCursorDataReader(connManager, this, con, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, hasNested, dynStmt);
			else
				idatareader = new GxMySQLDataReader(connManager, this, con, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt, preparedStmts);
#else
            if (!hasNested)//Client Cursor
            {
                GXLogging.Debug(log, "ExecuteReader: client cursor=" + hasNested + ", handle '" + handle + "'" + ", hashcode " + this.GetHashCode() + " PreparedStmt " + preparedStmts);
                if (preparedStmts)
                    idatareader = new GxMySQLCursorDataReader(connManager, this, con, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, hasNested, dynStmt);
                else
                    idatareader = new GxMySQLDataReader(connManager, this, con, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt, preparedStmts);
            }
            else //Server Cursor
            {
                GXLogging.Debug(log, "ExecuteReader: server cursor=" + hasNested + ", handle '" + handle + "'" + ", hashcode " + this.GetHashCode());
                idatareader = new GxMySqlMemoryDataReader(connManager, this, con, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt);
            }
#endif
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
		public override Object Net2DbmsGeo(GXType type, IGeographicNative geo)
		{
			string defaultValue;
			switch (type)
			{
				case GXType.Geopoint:
					defaultValue = Geospatial.ALT_EMPTY_POINT;
					break;
				case GXType.Geography:
					defaultValue = Geospatial.EMPTY_GEOMETRY;
					break;
				case GXType.Geoline:
					defaultValue = Geospatial.ALT_EMPTY_LINE;
					break;
				case GXType.Geopolygon:
					defaultValue = Geospatial.ALT_EMPTY_POLY;
					break;
				case GXType.Undefined:
					defaultValue = string.Empty;
					break;
				default:
					defaultValue = Geospatial.EMPTY_GEOMETRY;
					break;
			}
			if (!string.IsNullOrEmpty(defaultValue))
				return geo.ToStringSQL(defaultValue);
			else
				return geo.ToStringSQL();
		}


		public override bool ProcessError(int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)
		{
			GXLogging.Debug(log, "ProcessError: dbmsErrorCode=" + dbmsErrorCode + ", emsg '" + emsg + "'");
			switch (dbmsErrorCode)
			{
				case 2006://MySQL server has gone away
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
#if !NETCORE
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
#endif
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
#if NETCORE
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

#endif
		public override void SetParameter(IDbDataParameter parameter, object value)
		{
			if (value is Guid)
			{
				parameter.Value = value.ToString();
				
			}
#if NETCORE
			else if (value is bool && parameter.DbType == DbType.Byte ) {
				bool boolValue = (bool)value;
				parameter.Value = boolValue ? (byte)1 : (byte)0;
			}
#endif
			else
				base.SetParameter(parameter, value);
		}
	}

	sealed internal class MySqlConnectionWrapper : GxAbstractConnectionWrapper
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.MySqlConnectionWrapper));
		public MySqlConnectionWrapper() : base(new MySQLConnection())
		{ }

		public MySqlConnectionWrapper(String connectionString, GxConnectionCache connCache, IsolationLevel isolationLevel) : base(new MySQLConnection(connectionString), isolationLevel)
		{
			m_connectionCache = connCache;
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
			MySQLConnection sc = InternalConnection as MySQLConnection;
			if (null == sc)
				throw new InvalidOperationException("InvalidConnType00" + InternalConnection.GetType().FullName);

			return sc.CreateCommand();
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
	public class GxMySQLDataReader : GxDataReader
	{

		public GxMySQLDataReader(IGxConnectionManager connManager, GxDataRecord dr, IGxConnection connection, GxParameterCollection parameters,
			string stmt, int fetchSize, bool forFirst, int handle, bool cached, SlidingTime expiration, bool dynStmt, bool preparedStmts)
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
#if !NETCORE
            cmd.UsePreparedStatement = preparedStmts;
#else
			cmd.Prepare();
#endif
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

	}
	public class GxMySQLCursorDataReader : GxDataReader
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.GxDataReader));

		public GxMySQLCursorDataReader(IGxConnectionManager connManager, GxDataRecord dr, IGxConnection connection, GxParameterCollection parameters,
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
#if !NETCORE
			cmd.ServerCursor = hasNested;
			cmd.FetchSize = (uint)fetchSize;
#endif
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
#if NETCORE
				result = System.Text.Encoding.UTF8.GetString((byte[])block.Item(pos, i));
#else
				result = System.Text.Encoding.Default.GetString((byte[])block.Item(pos, i));
#endif
			else
				result= (string)block.Item(pos, i);

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
            string stmt, int fetchSize, bool forFirst, int handle, bool cached, SlidingTime expiration, bool dynStmt) : base(connManager, dr, connection, parameters,
            stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt)
        {
            MemoryDataReader memoryDataReader = new MemoryDataReader(reader);
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
