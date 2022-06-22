using GeneXus.Cache;
using GeneXus.Utils;
using log4net;
using System;
using System.Text;
using System.Data;
using System.Data.Common;
#if NETCORE
using Microsoft.Data.SqlClient;
using SQLiteParameter = Microsoft.Data.Sqlite.SqliteParameter;
using SQLiteCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SQLiteConnection = Microsoft.Data.Sqlite.SqliteConnection;
#else
using System.Data.SQLite;
using System.Data.SqlClient;
#endif

namespace GeneXus.Data
{
	public class GxSqlite : GxDataRecord
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.GxSqlite));
		private int MAX_TRIES = 10;
		private int m_FailedConnections;

		public GxSqlite()
		{
		}

		public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra, GxConnectionCache connectionCache)
		{
			if (m_connectionString == null)
				m_connectionString = BuildConnectionString(datasourceName, userId, userPassword, databaseName, port, schema, extra);
			GXLogging.Debug(log, "Setting connectionString property ", ConnectionStringForLog);
			SQLiteConnectionWrapper connection = new SQLiteConnectionWrapper(m_connectionString, connectionCache, isolationLevel);
			m_FailedConnections = 0;
			return connection;
		}
		public string GetConnectionString(string datasourceName, string userPassword)
		{
			if (m_connectionString == null)
				return BuildConnectionString(datasourceName, null, userPassword, null, null, null, null);
			else return m_connectionString;
		}

		public override IDbDataParameter CreateParameter()
		{
			return new SQLiteParameter();
		}
		public override IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec)
		{
			SQLiteParameter parm = new SQLiteParameter();
			parm.DbType = GXTypeToDbType(dbtype);
			parm.Size = gxlength;
			parm.ParameterName = name;
			return parm;
		}
		private DbType GXTypeToDbType(object type)
		{
			if (!(type is GXType))
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
				case GXType.Char:return DbType.String;
				case GXType.Blob: return DbType.Binary;
				case GXType.Geography:
				case GXType.Geoline:
				case GXType.Geopoint:
				case GXType.Geopolygon:
				case GXType.UniqueIdentifier:
					return DbType.String;
				default: return DbType.String;
			}
		}
#if !NETCORE
		public override DbDataAdapter CreateDataAdapeter()
		{
			return new SQLiteDataAdapter();
		}
#else
		public override DbDataAdapter CreateDataAdapeter()
		{
			throw new NotImplementedException();
		}
#endif

		internal override object CloneParameter(IDbDataParameter p)
		{
			IDbDataParameter copy = this.CreateParameter();
			copy.ParameterName = p.ParameterName;
			copy.DbType=p.DbType;
			copy.Value=p.Value;
			copy.Size=p.Size;
			copy.Precision=p.Precision;
			copy.Scale=p.Scale;
			return copy;
		}
		public override void DisposeCommand(IDbCommand command)
		{
			SQLiteCommand cmd = (SQLiteCommand)command;
			cmd.Parameters.Clear();
			base.DisposeCommand(command);
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
			idatareader = new GxDataReader(connManager, this, con, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt);
			return idatareader;
		}

		public override string GetServerDateTimeStmtMs(IGxConnection connection)
		{
			return GetServerDateTimeStmt(connection);
		}
		public override string GetServerDateTimeStmt(IGxConnection connection)
		{
			return "SELECT date('now')";
		}
		public override string GetServerUserIdStmt()
		{
			return null;
		}
		public override string GetServerVersionStmt()
		{
			return "select sqlite_version()";
		}
		public override bool ProcessError(int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)
		
		{
			GXLogging.Debug(log, "ProcessError: dbmsErrorCode=" + dbmsErrorCode + ", emsg '" + emsg + "'");
			switch (dbmsErrorCode)
			{
				case 11:
					if (con != null && m_FailedConnections < MAX_TRIES)
					{
						status = 104; // General network error.  Check your network documentation - Retry [Max Pool Size] times.
						m_FailedConnections++;
						con.Close();
						retry = true;
						GXLogging.Debug(log, "ProcessError: General network error, FailedConnections:" + m_FailedConnections);
					}
					else
					{
						return false;
					}
					break;
				case 903:       // Locked
					retry = Retry(errMask, retryCount);
					if (retry)
						status = 110;// Locked - Retry
					else
						status = 103;//Locked
					return retry;
				case 2601:      // Duplicated record
				case 2627:      // Duplicated record
				case 25016:     //A duplicate value cannot be inserted into a unique index
					status = 1;
					break;
				case 3639:      //sql ce specific
								//case 3631:		//sql ce specific The column cannot contain null values No deberia capturarse.
				case 3637:      //"The specified index does not exist"
				case 3723:      //sql ce specific The reference does not exist
				case 3638:      //"The specified index was in use"
								//case 3647:	//"The specified table already exists" => Do not catch it, it must be caught by the Reorganization
				case 25060:     //The reference does not exist (DROP de una constraint que no existe)
				case 3701:      // File not found
				case 3703:      // File not found
				case 3704:      // File not found
				case 3731:      // File not found
				case 4902:      // File not found
				case 3727:      // File not found
				case 3728:      // File not found
					status = 105;
					break;
				case 503:       // Parent key not found
				case 547:       //conflicted with COLUMN FOREIGN KEY constraint
					if ((errMask & GxErrorMask.GX_MASKFOREIGNKEY) > 0)
						status = 500;       // ForeignKeyError
					break;
				default:
					return false;
			}
			return true;
		}
		public override string ToDbmsConstant(DateTime Value)
		{
			if (Value == System.DateTime.MinValue)
				Value = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
			return "'" + Value.ToString("yyyy-MM-dd HH\\:mm\\:ss").Replace("'", "''") + "'";
		}

		protected override string BuildConnectionString(string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra)
		{
			StringBuilder connectionString = new StringBuilder();
			if (string.IsNullOrEmpty(databaseName))
			{
				databaseName = GetParameterValue(extra, "Database");
			}
			if (databaseName != null && databaseName.Trim().Length > 0)
			{
				connectionString.AppendFormat(";Data Source={0}", databaseName);
			}
			if (userPassword != null && userPassword.Length > 0)
			{
				connectionString.Append(";Password=");
				connectionString.Append(userPassword);
			}

			return connectionString.ToString();
		}

		public override DateTime Dbms2NetDate(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return Dbms2NetDateTime(DR.GetDateTime(i), false);
		}

		public override DateTime Dbms2NetDateTime(DateTime dt, Boolean precision)
		{
			
			if (dt.Equals(SQLSERVER_NULL_DATE))
			{
				return DateTimeUtil.NullDate();
			}

			if (dt.Year == SQLSERVER_NULL_DATE.Year &&
				dt.Month == SQLSERVER_NULL_DATE.Month &&
				dt.Day == SQLSERVER_NULL_DATE.Day)
			{

				return new DateTime(
					DateTimeUtil.NullDate().Year, DateTimeUtil.NullDate().Month,
					DateTimeUtil.NullDate().Day, dt.Hour, dt.Minute, dt.Second,
					(precision)?dt.Millisecond:0);
			}
            return (precision) ? DateTimeUtil.ResetMicroseconds(dt) : DateTimeUtil.ResetMilliseconds(dt);            
		}

		public override Object Net2DbmsDateTime(IDbDataParameter parm, DateTime dt)
		{
			
			if (dt.Equals(DateTimeUtil.NullDate()))
			{
				return SQLSERVER_NULL_DATE;
			}

			if (dt.CompareTo(SQLSERVER_NULL_DATE) < 0)
			{
				DateTime aux =
					new DateTime(
					SQLSERVER_NULL_DATE.Year, SQLSERVER_NULL_DATE.Month,
					SQLSERVER_NULL_DATE.Day, dt.Hour, dt.Minute, dt.Second,
					dt.Millisecond);
				
				return aux;
			}
			else
			{
				return dt;
			}
		}
		static DateTime SQLSERVER_NULL_DATE = new DateTime(1753, 1, 1);

		public override IDbCommand GetCachedCommand(IGxConnection con, string stmt)
		{
			return con.ConnectionCache.GetAvailablePreparedCommand(stmt);
		}

		private static readonly string[] ConcatOpValues = new string[] { string.Empty, " || ", string.Empty };
		public override string ConcatOp(int pos)
		{
			return ConcatOpValues[pos];
		}

		public override void SetParameterBlob(IDbDataParameter parameter, string value, bool dbBlob)
		{
			if (value == null)
			{
				parameter.Value = DBNull.Value;
			}
			else 
			{
				
				parameter.Value = value;
			}
		}

	}


	sealed internal class SQLiteConnectionWrapper : GxAbstractConnectionWrapper
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.SQLiteConnectionWrapper));

		public SQLiteConnectionWrapper()
			: base(new SQLiteConnection())
		{ }
		public SQLiteConnectionWrapper(String connectionString, GxConnectionCache connCache, IsolationLevel isolationLevel)
			: base(new SQLiteConnection(connectionString), isolationLevel)
		{
			m_connectionCache = connCache;
		}

		public override IDbTransaction BeginTransaction(IsolationLevel isoLevel)
		{
			return base.BeginTransaction(IsolationLevel.ReadCommitted);
		}

		override public void Open()
		{
			InternalConnection.Open();
			if (!m_autoCommit)
			{
				GXLogging.Debug(log, "Open connection InternalConnection.BeginTransaction() ");
				m_transaction = InternalConnection.BeginTransaction(IsolationLevel.ReadCommitted);
			}
			else
			{
				m_transaction = null;
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
		override public IDbCommand CreateCommand()
		{

			SQLiteConnection sc = InternalConnection as SQLiteConnection;
			if (null == sc)
				throw new InvalidOperationException("InvalidConnType00" + InternalConnection.GetType().FullName);
			return sc.CreateCommand();
		}
#if !NETCORE
		public override DbDataAdapter CreateDataAdapter()
		{
			return new SQLiteDataAdapter();
		}
#else
		public override DbDataAdapter CreateDataAdapter()
		{
			throw new NotImplementedException();
		}
#endif

	}

}
