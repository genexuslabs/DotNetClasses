using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Threading;

using log4net;
using GeneXus.Application;
using GeneXus.Cache;
using GeneXus.Configuration;
using GeneXus.Utils;
using System.Globalization;
using GeneXus.Metadata;
using GxClasses.Helpers;
using System.IO;

namespace GeneXus.Data
{
	public class GxInformix : GxDataRecord
	{
		static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		static Assembly _ifxAssembly;
#if NETCORE
		internal static string InformixAssemblyName = "Informix.Net.Core";
		const string InformixDbTypeEnum = "Informix.Net.Core.IfxType";
#else
		internal static string InformixAssemblyName = "IBM.Data.Informix";
		const string InformixDbTypeEnum = "IBM.Data.Informix.IfxType";
#endif
		public static string SQL_NULL_DATE_10 = "0000-00-00";
		public static string SQL_NULL_DATE_8 = "00000000";
		private string m_serverInstance;
		public GxInformix()
		{
		}
		public GxInformix(string id)
		{
			string str;
			if (Config.GetValueOf("Connection-" + id + "-ServerInstance", out str))
				m_serverInstance = str;
		}
		public static Assembly IfxAssembly
		{
			get
			{
				try
				{
					if (_ifxAssembly == null)
					{
#if NETCORE
						string informixDir = Environment.GetEnvironmentVariable("INFORMIXDIR");
						string assemblyPath = FileUtil.GetStartupDirectory();
						string informixBinDir = null;
						if (!string.IsNullOrEmpty(informixDir)) {
							try
							{
								informixBinDir = Path.Combine(informixDir, "bin");

							}catch(Exception ex)
							{
								GXLogging.Warn(log, $"Error reading INFORMIXDIR env var", ex);
							}
							if (!string.IsNullOrEmpty(informixBinDir) && File.Exists(Path.Combine(informixBinDir, $"{InformixAssemblyName}.dll")))
							{
								assemblyPath = informixBinDir;
							}
						}
						GXLogging.Debug(log, $"Loading {InformixAssemblyName}.dll from:" + assemblyPath);

						_ifxAssembly = AssemblyLoader.LoadAssembly(new AssemblyName(InformixAssemblyName));
#else
						GXLogging.Debug(log, $"Loading {InformixAssemblyName} from GAC");
						_ifxAssembly = Assembly.LoadWithPartialName(InformixAssemblyName);
						GXLogging.Debug(log, $"{InformixAssemblyName} Loaded from GAC");
#endif
					}

				}
				catch (Exception ex)
				{
					GXLogging.Error(log, $"Error loading {InformixAssemblyName} from GAC", ex);
				}
				if (_ifxAssembly == null)
				{
					_ifxAssembly = Assembly.Load(InformixAssemblyName);
				}
				return _ifxAssembly;
			}
		}
		public override void GetValues(IDataReader reader, ref object[] values)
		{
			try
			{
				for (int i = 0; i < reader.FieldCount; i++)
				{
					if (reader.GetFieldType(i) == typeof(decimal))
					{
						GXLogging.Debug(log, "GetValues fieldtype decimal value");
						decimal result = GetIfxDecimal(reader, i);
						values[i] = result;
						GXLogging.Debug(log, "GetValues decimal:" + result);
					}
#if NETCORE
					else if (reader.GetFieldType(i) == typeof(DateTime))
					{
						values[i] = reader.GetDateTime(i); //IfxDateTime
					}
#endif
					else
					{
						values[i] = reader.GetValue(i);
					}
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "GetValues error", ex);
			}
		}
		internal static decimal GetIfxDecimal(IDataReader reader, int i)
		{
			try
			{
				decimal result;
				string ifxDecimal = ClassLoader.Invoke(reader, "GetIfxDecimal", new object[] { i }).ToString();
				Decimal.TryParse(ifxDecimal, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
				return result;
			}catch(Exception)
			{
				return Convert.ToInt64(reader.GetValue(i));
			}
		}
		public override IDataReader GetDataReader(IGxConnectionManager connManager, IGxConnection con, GxParameterCollection parameters, string stmt, ushort fetchSize, bool forFirst, int handle, bool cached, SlidingTime expiration, bool hasNested, bool dynStmt)
		{
			GXLogging.Debug(log, "ExecuteReader: client cursor=", () => hasNested + ", handle '" + handle + "'" + ", hashcode " + this.GetHashCode());
			return new GxInformixDataReader(connManager, this, con, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt);
		}
		public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra, GxConnectionCache connectionCache)
		{
			if (m_connectionString == null)
				m_connectionString = BuildConnectionString(datasourceName, userId, userPassword, databaseName, port, schema, extra);
			GXLogging.Debug(log, "Setting connectionString property ", ConnectionStringForLog);

			return new InformixConnectionWrapper(m_connectionString, connectionCache, isolationLevel);
		}


		protected override string BuildConnectionString(string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra)
		{
			StringBuilder connectionString = new StringBuilder();

			if (!string.IsNullOrEmpty(datasourceName) && !hasKey(extra, "Host"))
			{
				connectionString.AppendFormat("Host={0};", datasourceName);
				
			}
			if (m_serverInstance != null && m_serverInstance.Trim().Length > 0 && !hasKey(extra, "Server"))
			{
				connectionString.AppendFormat("Server={0};", m_serverInstance);
				
			}
			if (port != null && port.Trim().Length > 0 && !hasKey(extra, "Service"))
			{
				connectionString.AppendFormat("Service={0};", port);
				
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
			return connectionString.ToString();
		}

		public override IDbDataParameter CreateParameter()
		{
			return (IDbDataParameter)ClassLoader.CreateInstance(IfxAssembly, $"{InformixAssemblyName}.IfxParameter");
		}
		public override IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec)
		{
			object ifxType = GXTypeToIfxType((GXType)dbtype);
			object[] args = new object[] { name, ifxType, gxlength };
			IDbDataParameter parm = (IDbDataParameter)ClassLoader.CreateInstance(IfxAssembly, $"{InformixAssemblyName}.IfxParameter", args);
			
			ClassLoader.SetPropValue(parm, "IfxType", ifxType);
			parm.Precision = (byte)gxdec;
			parm.Scale = (byte)gxdec;
			return parm;
		}

		private object GXTypeToIfxType(GXType type)
		{
			switch (type)
			{
				case GXType.Number: return ClassLoader.GetEnumValue(IfxAssembly, InformixDbTypeEnum, "Decimal");
				case GXType.Int16: return ClassLoader.GetEnumValue(IfxAssembly, InformixDbTypeEnum, "SmallInt");
				case GXType.Int32: return ClassLoader.GetEnumValue(IfxAssembly, InformixDbTypeEnum, "Integer");
				case GXType.Int64: return ClassLoader.GetEnumValue(IfxAssembly, InformixDbTypeEnum, "Int8");
				case GXType.LongVarChar: return ClassLoader.GetEnumValue(IfxAssembly, InformixDbTypeEnum, "Text");
				case GXType.DateTime2: return ClassLoader.GetEnumValue(IfxAssembly, InformixDbTypeEnum, "DateTime");
				case GXType.UniqueIdentifier:return ClassLoader.GetEnumValue(IfxAssembly, InformixDbTypeEnum, "Char");
#if NETCORE
				case GXType.Boolean: return ClassLoader.GetEnumValue(IfxAssembly, InformixDbTypeEnum, "Bit");
#endif
				default: return ClassLoader.GetEnumValue(IfxAssembly, InformixDbTypeEnum, type.ToString());
			}
		}

		protected override void PrepareCommand(IDbCommand cmd)
		{
			
		}
		public override DbDataAdapter CreateDataAdapeter()
		{
			Type odpAdapter = IfxAssembly.GetType($"{InformixAssemblyName}.IfxDataAdapter");
			return (DbDataAdapter)Activator.CreateInstance(odpAdapter);
		}

		public override bool IsBlobType(IDbDataParameter idbparameter)
		{
			object otype = ClassLoader.GetPropValue(idbparameter, "IfxType");
			object blobType = ClassLoader.GetEnumValue(IfxAssembly, InformixDbTypeEnum, "Byte");
			return (int)otype == (int)blobType;
		}
		public override void SetParameterDir(GxParameterCollection parameters, int num, ParameterDirection dir)
		{
			if (dir == ParameterDirection.Output)
			{
				parameters[num].Direction = ParameterDirection.ReturnValue;
			}
			else
			{
				parameters[num].Direction = dir;
			}

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
				if (p.Direction == ParameterDirection.ReturnValue)
				{
					returnParms.Add(i, p);
				}
			}
			foreach (IDataParameter p in returnParms.Values)
			{
				cmd.Parameters.Remove(p);
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


		public override void SetParameter(IDbDataParameter parameter, Object value)
		{

			if (value == null || value == DBNull.Value)
			{
				parameter.Value = DBNull.Value;
			}
			else if (!IsBlobType(parameter))
			{
				if (value is Guid)
					parameter.Value = value.ToString();
				else
					parameter.Value = CheckDataLength(value, parameter);
			}
			else
			{
				SetBinary(parameter, GetBinary((string)value, false));
			}
		}
		public override string GetServerDateTimeStmt(IGxConnection connection)
		{
			return "SELECT CURRENT YEAR TO SECOND FROM informix.SYSTABLES WHERE tabname = 'systables'";
		}
		public override string GetServerDateTimeStmtMs(IGxConnection connection)
		{
			return "SELECT CURRENT YEAR TO FRACTION(3) FROM informix.SYSTABLES WHERE tabname = 'systables'";
		}
		public override string GetServerVersionStmt()
		{
			throw new GxNotImplementedException();
		}
		public override string GetServerUserIdStmt()
		{
			return "SELECT USER";
		}
		public override void SetTimeout(IGxConnectionManager connManager, IGxConnection connection, int handle)
		{
			GXLogging.Debug(log, "Set Lock Timeout to " + m_lockTimeout / 1000);
			IDbCommand cmd = GetCommand(connection, SetTimeoutSentence(m_lockTimeout), new GxParameterCollection());
			cmd.ExecuteNonQuery();
		}
		public override string SetTimeoutSentence(long milliseconds)
		{
			if (milliseconds > 0)
				return "SET LOCK MODE TO WAIT " + milliseconds / 1000;
			else
				return "SET LOCK MODE TO WAIT";
		}

		public override bool ProcessError(int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)
		
		{

			switch (dbmsErrorCode)
			{
				case -243:      // Locked
				case -244:      // Locked
				case -245:      // Locked
					retry = Retry(errMask, retryCount);
					if (retry)
						status = 110;// Locked - Retry
					else
						status = 103;//Locked
					return retry;
				case -239:      // Duplicated record
				case -268:      // Duplicated record
				case -346:      // Duplicated record
					status = 1;
					break;
				case -203:      // File not found
				case -206:      // File not found
				case -319:      // File not found
				case -623:      // File not found
					status = 105;
					break;
				case 50:        // table or view does not exist
					if (!GxContext.isReorganization)
					{
						status = 999;
						return false;
					}
					else
					{
						status = 105;
					}
					break;

				case -691:      // Parent key not found
					if ((errMask & GxErrorMask.GX_MASKFOREIGNKEY) == 0)
					{
						status = 500;       // ForeignKeyError
						return false;
					}
					else
						status = 0;     // NoError
					break;
				default:
					GXLogging.Warn(log, "ProcessError dbmsErrorCode:'" + dbmsErrorCode + "'" + emsg);
					status = 999;
					return false;
			}
			return true;
		}

		public override string ToDbmsConstant(DateTime Value)
		{
			if (Value == System.DateTime.MinValue)
				return "'0001-01-01'";
			return "TIMESTAMP('" +
				Value.Year.ToString() + "-" +
				Value.Month.ToString() + "-" +
				Value.Day.ToString() + "-" +
				Value.Hour.ToString() + "." +
				Value.Minute.ToString() + "." +
				Value.Second.ToString() + "')";
		}

		public override DateTime DTFromString(string s)
		{
			string strim = s.Trim();

			if (strim.Length == 0)
				return DateTime.MinValue;
			else if (strim.Length == 8)
			{
				if (strim == SQL_NULL_DATE_8)
				{
					return DateTimeUtil.NullDate();
				}
				else
					return new DateTime(
						Convert.ToInt32(s.Substring(0, 2)),
						Convert.ToInt32(s.Substring(3, 2)),
						Convert.ToInt32(s.Substring(6, 2)),
						0, 0, 0);
			}
			else if (strim.Length == 10)
			{
				if (strim == SQL_NULL_DATE_10)
					return DateTimeUtil.NullDate();
				else
					return new DateTime(
						Convert.ToInt32(s.Substring(0, 4)),
						Convert.ToInt32(s.Substring(5, 2)),
						Convert.ToInt32(s.Substring(8, 2)),
						0, 0, 0);
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

		protected override IDbCommand GetCachedCommand(IGxConnection con, string stmt)
		{
			return con.ConnectionCache.GetAvailablePreparedCommand(stmt);
		}

	}

	public class GxInformixDataReader : GxDataReader
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GxInformixDataReader));
		public GxInformixDataReader(IGxConnectionManager connManager, GxDataRecord dr, IGxConnection connection, GxParameterCollection parameters,
			string stmt, int fetchSize, bool forFirst, int handle, bool cached, SlidingTime expiration, bool dynStmt)
			: base(connManager, dr, connection, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt)
		{ }
		public override short GetInt16(int i)
		{
			if (reader.GetFieldType(i) == typeof(decimal))
			{
				try
				{
					return reader.GetInt16(i);
				}
				catch (Exception fe) 
				{
					GXLogging.Debug(log, "GetInt16 fieldtype Error, decimal value", fe);
					decimal result = GxInformix.GetIfxDecimal(reader, i);
					GXLogging.Debug(log, "GetInt16 decimal:" + result);
					return Convert.ToInt16(result);
				}
			}
			else
			{
				return base.GetInt16(i);
			}
		}
		public override int GetInt32(int i)
		{
			if (reader.GetFieldType(i) == typeof(decimal))
			{
				try
				{
					return reader.GetInt32(i);
				}
				catch (Exception fe) 
				{
					GXLogging.Debug(log, "GetInt32 fieldtype Error, decimal value", fe);
					decimal result = GxInformix.GetIfxDecimal(reader, i);
					GXLogging.Debug(log, "GetInt32 decimal:" + result);
					return Convert.ToInt32(result);
				}
			}
			else
			{
				return base.GetInt32(i);
			}
		}
		public override long GetInt64(int i)
		{
			if (reader.GetFieldType(i) == typeof(decimal))
			{
				try
				{
					return reader.GetInt64(i);
				}
				catch (Exception fe) 
				{
					GXLogging.Debug(log, "GetInt64 fieldtype Error, decimal value", fe);
					decimal result = GxInformix.GetIfxDecimal(reader, i);
					GXLogging.Debug(log, "GetInt64 decimal:" + result);
					return Convert.ToInt64(result);
				}
			}
			else
			{
				return base.GetInt64(i);
			}
		}
		public override decimal GetDecimal(int i)
		{
			if (reader.GetFieldType(i) == typeof(decimal))
			{
				try
				{
					return reader.GetDecimal(i);
				}
				catch (Exception fe) 
				{
					GXLogging.Debug(log, "GetDecimal fieldtype Error, decimal value", fe);
					decimal result = GxInformix.GetIfxDecimal(reader, i);
					GXLogging.Debug(log, "GetDecimal decimal:" + result);
					return result;
				}
			}
			else
			{
				return base.GetDecimal(i);
			}
		}
#if NETCORE
		public override Guid GetGuid(int i)
		{
			readBytes += 16;
			if (reader.GetFieldType(i) == typeof(string))
				return new Guid(reader.GetString(i));
			else
				return reader.GetGuid(i);
		}
#endif
		public override object GetValue(int i)
		{
			if (reader.GetFieldType(i) == typeof(decimal))
			{
				try
				{
					return reader.GetDecimal(i);
				}
				catch (Exception fe)
				{
					GXLogging.Debug(log, "GetValues fieldtype Error, decimal value", fe);
					decimal result = GxInformix.GetIfxDecimal(reader, i);
					GXLogging.Debug(log, "GetValues decimal:" + result);
					return result;
				}
			}
			else
			{
				return base.GetValue(i);
			}

		}
	}

	sealed internal class InformixConnectionWrapper : GxAbstractConnectionWrapper
	{
		private static int changeConnState = -1;
		private int openDataReaders;
		static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public InformixConnectionWrapper() 
		{
			try
			{
				GXLogging.Debug(log, "Creating Informix data provider ");
				_connection = (IDbConnection)ClassLoader.CreateInstance(GxInformix.IfxAssembly, $"{GxInformix.InformixAssemblyName}.IfxConnection");
				GXLogging.Debug(log, "Informix data provider created");
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Informix data provider Ctr error " + ex.Message + ex.StackTrace);
				throw ex;
			}
		}

		public InformixConnectionWrapper(String connectionString, GxConnectionCache connCache, IsolationLevel isolationLevel)
		{
			try
			{
				_connection = (IDbConnection)ClassLoader.CreateInstance(GxInformix.IfxAssembly, $"{GxInformix.InformixAssemblyName}.IfxConnection", new object[] { connectionString });
				m_isolationLevel = isolationLevel;
				m_connectionCache = connCache;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Informix data provider Ctr error " + ex.Message + ex.StackTrace);
				throw ex;
			}

		}
		override public void CheckStateInc()
		{
			if (openDataReaders > 0)
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
			if (!value && openDataReaders > 0)
			{
				ChangeConnectionState(InternalConnection);
				GXLogging.Debug(log, "CheckState, ChangeConnectionState");
			}
		}
		private void ChangeConnectionState(IDbConnection con)
		{
			if (changeConnState == -1)
			{
				
				MethodInfo m = con.GetType().GetMethod("SetStateFetchingFalse", BindingFlags.Instance | BindingFlags.NonPublic);
				changeConnState = (m == null) ? 0 : 1;
			}
			if (changeConnState == 1)
			{
				con.GetType().InvokeMember("SetStateFetchingFalse",
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

		override public IDbCommand CreateCommand()
		{
			return InternalConnection.CreateCommand();
		}
		public override DbDataAdapter CreateDataAdapter()
		{
			throw new GxNotImplementedException();
		}
		public override string Database
		{
			get
			{
				string db = base.Database;
				if (String.IsNullOrEmpty(db))
				{
					string cfgBuf = InternalConnection.ConnectionString.ToLower();

					if (cfgBuf.IndexOf("database=") >= 0)
					{
						int dbPos = cfgBuf.IndexOf("database=");
						if (dbPos >= 0)
						{
							int dbEnd = cfgBuf.IndexOf(";", dbPos);
							if (dbEnd >= 0)
								db = cfgBuf.Substring(dbPos + "database=".Length, dbEnd - (dbPos + "database=".Length));
							else
								db = cfgBuf.Substring(dbPos + "database=".Length);
						}
					}
				}
				return db;
			}
		}
		public override IDbTransaction BeginTransaction(IsolationLevel isoLevel)
		{
			try
			{
				IDbTransaction trn = InternalConnection.BeginTransaction(isoLevel);
				return trn;
			}
			catch (Exception e)
			{
				GXLogging.Warn(log, "BeginTransaction Error ", e);
				IDbTransaction trn = InternalConnection.BeginTransaction(IsolationLevel.Unspecified);
				if (trn.IsolationLevel != isoLevel)
				{
					GXLogging.Error(log, "BeginTransaction Error, could not open new transaction isoLevel=" + isoLevel, e);
					throw new GxADODataException("Begin transaction error in informix", e);
				}
				return trn;
			}
		}


	}
}
