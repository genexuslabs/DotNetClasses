using System;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using log4net;
using System.Data;
using System.Data.Common;
using GeneXus.Utils;
using GeneXus.Cache;
using System.Collections;
using GeneXus.Metadata;
#if NETCORE
using GxClasses.Helpers;
using System.Reflection;
using System.IO;
#endif

namespace GeneXus.Data
{
	public class GxPostgreSql : GxDataRecord
	{
		static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		const string ConnectionStringEncoding = "encoding";
		private byte[] _buffer;
#if NETCORE
		static Assembly assembly;
#endif
		public GxPostgreSql()
		{
#if NETCORE
			if (assembly == null)
			{
				using (var dynamicContext = new AssemblyResolver(Path.Combine(FileUtil.GetStartupDirectory(), "Npgsql.dll")))
				{
					assembly = dynamicContext.Assembly;
				}
			}
#endif
		}
		public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra, GxConnectionCache connectionCache)
		{
			if (m_connectionString == null)
				m_connectionString = BuildConnectionString(datasourceName, userId, userPassword, databaseName, port, schema, extra);
			GXLogging.Debug(log, "Setting connectionString property ", ConnectionStringForLog);
			return new PostgresqlConnectionWrapper(m_connectionString, connectionCache, isolationLevel);
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
			if (!string.IsNullOrEmpty(extra))
			{
				string res = ParseAdditionalData(extra, "integrated security");
				if (extra.IndexOf(ConnectionStringEncoding, StringComparison.OrdinalIgnoreCase) != extra.LastIndexOf(ConnectionStringEncoding, StringComparison.OrdinalIgnoreCase))
					res = RemoveDuplicates(res, ConnectionStringEncoding);

				if (!res.StartsWith(";") && res.Length > 1) connectionString.Append(";");
				connectionString.Append(res);

			}
			return connectionString.ToString();

		}
		public override object[] ExecuteStoredProcedure(IDbCommand cmd)
		{
			Hashtable returnParms = new Hashtable();
			object[] values = null;
			int count = cmd.Parameters != null ? cmd.Parameters.Count : 0;
			if (count > 0)
			{
				values = new object[count];
			}
			for (int i = 0; i < count; i++)
			{
				IDataParameter p = (IDataParameter)cmd.Parameters[i];
				if (p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue)
				{
					returnParms.Add(i, p);
				}
			}
			IDataReader reader = cmd.ExecuteReader();
			if (reader.Read())
			{
				int i = 0;
				for (int j = 0; j < count; j++)
				{
					if (returnParms.Contains(j))
					{
						values[j] = reader.GetValue(i);
						i++;
					}
				}
			}
			reader.Close();
			return values;
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
			return new NpgsqlParameter();
		}
		public override IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec)
		{
			NpgsqlParameter parm = new NpgsqlParameter();
			parm.NpgsqlDbType = (NpgsqlDbType)dbtype;
			
			parm.Size = gxlength;
			parm.Precision = (byte)gxlength;
			parm.Scale = (byte)gxdec;
			parm.ParameterName = name;
			return parm;
		}
        public override DbDataAdapter CreateDataAdapeter()
		{
			return new NpgsqlDataAdapter();
		}
#if NETCORE
		public override bool MultiThreadSafe
		{
			get
			{
				return false;
			}
		}
#endif
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
			GXLogging.Debug(log, "ExecuteReader: client cursor=" + hasNested + ", handle '" + handle + "'" + ", hashcode " + this.GetHashCode());
			idatareader = new GxDataReader(connManager, this, con, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt);
#else
			if (!hasNested)//Client Cursor
			{
				GXLogging.Debug(log, "ExecuteReader: client cursor=" + hasNested + ", handle '" + handle + "'" + ", hashcode " + this.GetHashCode());
				idatareader = new GxDataReader(connManager, this, con, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt);
			}
			else //Server Cursor
			{
				GXLogging.Debug(log, "ExecuteReader: server cursor=" + hasNested + ", handle '" + handle + "'" + ", hashcode " + this.GetHashCode());
				idatareader = new GxPostgresqlMemoryDataReader(connManager, this, con, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt);
			}
#endif
			return idatareader;

		}
		public override long GetBytes(IGxDbCommand cmd, IDataRecord DR, int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
		{
			
			if (!cmd.HasMoreRows || DR == null || DR.IsDBNull(i))
				return 0;
			else
			{
				if (fieldOffset == 0)
				{
					_buffer = (byte[])DR.GetValue(i);
				}
				int count = 0;
				for (long index = fieldOffset; index < fieldOffset + length && index < _buffer.Length; index++)
				{
					buffer[bufferOffset + count] = _buffer[index];
					count++;
				}
				return count;
			}
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
		public override IGeographicNative GetGeospatial(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			if (!cmd.HasMoreRows || DR == null || DR.IsDBNull(i))
				return new Geospatial();
			else
			{
				
				Geospatial gtmp = new Geospatial();
				String[] geoStr = DR.GetValue(i).ToString().Split(new char[] { ';' }, 2);
				String[] srId = geoStr[0].Split(new char[] { '=' }, 2);
				gtmp.Srid = Int16.Parse(srId[1]);
				gtmp.FromString(geoStr[1]);
				return gtmp;
			}
		}
		public override IDataReader GetCacheDataReader(CacheItem item, bool computeSize, string keyCache)
		{
			return new GxCacheDataReader(item, computeSize, keyCache);
		}

		public override bool IsBlobType(IDbDataParameter idbparameter)
		{
			return ((NpgsqlParameter)idbparameter).NpgsqlDbType == NpgsqlDbType.Bytea;
		}

		public override DateTime Dbms2NetDateTime(DateTime dt, Boolean precision)
		{
			return (precision) ? DateTimeUtil.ResetMicroseconds(dt) : DateTimeUtil.ResetMillisecondsTicks(dt);
		}
		public override object Net2DbmsDateTime(IDbDataParameter parm, DateTime dt)
		{
			if (dt.Equals(DateTimeUtil.NullDate()))
			{
				return DateTime.MinValue.AddTicks(1);//Avoid -infinity DateTimes (sac 20807)
			}
			else
			{
				return base.Net2DbmsDateTime(parm, dt);
			}
		}

		public override string GetServerDateTimeStmtMs(IGxConnection connection)
		{
			return GetServerDateTimeStmt(connection);
		}
		public override string GetServerDateTimeStmt(IGxConnection connection)
		{
			if (string.IsNullOrEmpty(connection.DSVersion)) //>= 8.1
				return "SELECT clock_timestamp()";
			//clock_timestamp()	timestamp with time zone. Current date and time (changes during statement execution); 
			else
				return "SELECT CAST(timeofday() AS timestamp)";
			//timeofday()	text	Current date and time (like clock_timestamp, but as a text string); 
			//SELECT CURRENT_TIMESTAMP=> timestamp with time zone. Current date and time (start of current transaction); see Section 9.9.4
		}
		public override string GetServerUserIdStmt()
		{
			return "SELECT current_user";
		}
		public override string GetServerVersionStmt()
		{
			throw new GxNotImplementedException();
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
			if (emsg.IndexOf("42p01", StringComparison.OrdinalIgnoreCase) != -1)
				if ((emsg.IndexOf(" vista ") != -1 || emsg.IndexOf(" view ") != -1) || (emsg.IndexOf(" tabla ") != -1 || emsg.IndexOf(" table ") != -1))
				{
					//42p01 messages for views and tables are not captured
					//dbmsErrorCode=5632, emsg 'error: 42p01: no existe la vista xxxx
					//dbmsErrorCode=5632, error: 42p01: no existe la tabla â«ivafechafacturaâ»
					return false;
				}

			if (    //duplicate key violates unique constraint //ERRO: 23505: duplicar chave viola a restriÃ§Ã£o de unicidade "nfe010_pkey"
				emsg.IndexOf("23505") != -1 ||
				(emsg.IndexOf("duplicate key") != -1 && emsg.IndexOf("unique") != -1) ||
				(emsg.IndexOf("duplicar chave") != -1 && emsg.IndexOf("unicidade") != -1)
				)
			{
				status = 1; // Duplicate key
				return true;
			}
			else if
				(
					(emsg.IndexOf("42704") != -1) //42704 index "xxx" does not exist
					||
					(emsg.IndexOf("42p01") != -1)  //42P01	relation tableName does not exist //42P01: relaÃ§Ã£o "nuc011" nÃ£o existe
					||
					(emsg.IndexOf("42P01") != -1)  //42P01	relation tableName does not exist
					||
					((emsg.IndexOf("table") != -1 || emsg.IndexOf("relation") != -1 || emsg.IndexOf("sequence") != -1) && emsg.IndexOf("does not exist") != -1)
					||
					(emsg.IndexOf("index") != -1 && (emsg.IndexOf("nonexistent") != -1 || emsg.IndexOf("does not exist") != -1))
					||
					((emsg.IndexOf("seqüência") != -1 || emsg.IndexOf("ã­ndice") != -1) && (emsg.IndexOf("nã£o existe") != -1))//ERRO: 42704: Ã­ndice "iu_inuc002" nÃ£o existe
				)
			{
				status = 105;
				return true;
			}
#if NETCORE
			if (dbmsErrorCode == 16389 && emsg.IndexOf("42p06") >= 0) //42p06: ya existe el esquema
				return true;
#else
			if (dbmsErrorCode == 5632 && emsg.IndexOf("42p06") >= 0) //42p06: ya existe el esquema
				return true;
#endif

			switch ((int)dbmsErrorCode)
			{
				case 100:           
					status = 101;
					break;
				default:
					status = 999;
					return false;
			}
			return true;
		}

		private static readonly string[] ConcatOpValues = new string[] { string.Empty, " || ", string.Empty };
		public override string ConcatOp(int pos)
		{
			return ConcatOpValues[pos];
		}
	}
	sealed internal class PostgresqlConnectionWrapper : GxAbstractConnectionWrapper
	{
		static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public PostgresqlConnectionWrapper() : base(new NpgsqlConnection())
		{
		}

		public PostgresqlConnectionWrapper(String connectionString, GxConnectionCache connCache, IsolationLevel isolationLevel) 
		{
			try
			{
				Type postgresqlConnection = typeof(NpgsqlConnection);
				_connection = (IDbConnection)ClassLoader.CreateInstance(postgresqlConnection.Assembly, postgresqlConnection.FullName, new object[] { connectionString });
				m_isolationLevel = isolationLevel;
				m_connectionCache = connCache;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Npgsql data provider Ctr error " + ex.Message + ex.StackTrace);
				throw ex;
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
			catch (Exception ex)
			{
				throw new DataException(ex.Message, ex);
			}
		}
        public override DbDataAdapter CreateDataAdapter()
		{
			throw new GxNotImplementedException();
		}
		override public IDbCommand CreateCommand()
		{
			NpgsqlConnection sc = InternalConnection as NpgsqlConnection;
			if (null == sc)
				throw new InvalidOperationException("InvalidConnType00" + InternalConnection.GetType().FullName);
			return sc.CreateCommand();
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

	public class GxPostgresqlMemoryDataReader : GxDataReader
	{
		public GxPostgresqlMemoryDataReader(IGxConnectionManager connManager, GxDataRecord dr, IGxConnection connection, GxParameterCollection parameters,
			string stmt, int fetchSize, bool forFirst, int handle, bool cached, SlidingTime expiration, bool dynStmt) : base(connManager, dr, connection, parameters,
			stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt)
		{
			MemoryDataReader memoryDataReader = new MemoryDataReader(reader);
			Close();
			reader = memoryDataReader;
		}
	}

}