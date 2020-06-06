using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using MSOracleProvider = System.Data.OracleClient;
using GeneXus.Application;
using GeneXus.Cache;
using GeneXus.Configuration;
using GeneXus.Metadata;
using GeneXus.Utils;
using log4net;
using System.IO;
#if NETCORE
using GxClasses.Helpers;
#endif

namespace GeneXus.Data
{
	public class GxODPCacheDataReader : GxOracleCacheDataReader
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GxODPCacheDataReader));
		public GxODPCacheDataReader(CacheItem cacheItem, bool computeSize, string keyCache)
			: base(cacheItem, computeSize, keyCache)
		{ }
		public override DateTime GetDateTime(int i)
		{
			try
			{
				return base.GetDateTime(i);
			}
			catch (InvalidCastException ex)
			{
				GXLogging.Warn(log, "GetDate invalidCast", ex);
				return Convert.ToDateTime(GxODPOracle.OracleDateValue(base.GetValue(i)));
			}

		}
		public override string GetString(int i)
		{
			try
			{
				if (GxODPOracle.IsClobType(block.Item(pos, i)))
				{
					try
					{
						base.GetString(i);
					}
					catch { }
					return GxODPOracle.OracleClobValue(block.Item(pos, i)).ToString();
				}
				else
				{

					return base.GetString(i);
				}
			}
			catch (InvalidCastException ex)
			{
				GXLogging.Warn(log, "GetString invalidCast", ex);
				return base.GetValue(i).ToString();
			}
		}
		public override long GetInt64(int i)
		{
			try
			{
				return base.GetInt64(i);
			}
			catch (InvalidCastException ex)
			{
				GXLogging.Warn(log, "GetInt64 invalidCast", ex);
				return Convert.ToInt64(GxODPOracle.OracleDecimalValue(base.GetValue(i)));
			}
		}
		public override decimal GetDecimal(int i)
		{
			try
			{
				return base.GetDecimal(i);
			}
			catch (InvalidCastException ex)
			{
				GXLogging.Warn(log, "GetDecimal invalidCast", ex);
				return Convert.ToDecimal(GxODPOracle.OracleDecimalValue(base.GetValue(i)));
			}
		}
		public override short GetInt16(int i)
		{
			try
			{
				return base.GetInt16(i);
			}
			catch (InvalidCastException ex)
			{
				GXLogging.Warn(log, "GetInt16 invalidCast", ex);
				return Convert.ToInt16(GxODPOracle.OracleDecimalValue(base.GetValue(i)));
			}
		}
		public override int GetInt32(int i)
		{
			try
			{
				return base.GetInt32(i);
			}
			catch (InvalidCastException ex)
			{
				GXLogging.Warn(log, "GetInt32 invalidCast", ex);
				return Convert.ToInt32(GxODPOracle.OracleDecimalValue(base.GetValue(i)));
			}
		}
		public override bool IsDBNull(int i)
		{
			return GxODPOracle.OracleDecimalNullValue(block.Item(pos, i)) || base.IsDBNull(i);
		}
	}
	public class GxODPManagedCacheDataReader : GxOracleCacheDataReader
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GxODPManagedCacheDataReader));
		public GxODPManagedCacheDataReader(CacheItem cacheItem, bool computeSize, string keyCache)
			: base(cacheItem, computeSize, keyCache)
		{ }
		public override DateTime GetDateTime(int i)
		{
			try
			{
				return base.GetDateTime(i);
			}
			catch (InvalidCastException ex)
			{
				GXLogging.Warn(log, "GetDateTime invalidCast", ex);
				return Convert.ToDateTime(GxODPManagedOracle.OracleDateValue(base.GetValue(i)));
			}
		}
		public override string GetString(int i)
		{
			try
			{
				if (GxODPManagedOracle.IsClobType(block.Item(pos, i)))
				{
					try
					{
						base.GetString(i);
					}
					catch { }
					return GxODPManagedOracle.OracleClobValue(block.Item(pos, i)).ToString();
				}
				else
				{

					return base.GetString(i);
				}
			}
			catch (InvalidCastException ex)
			{
				GXLogging.Warn(log, "GetString invalidCast", ex);
				return base.GetValue(i).ToString();
			}
		}
		public override long GetInt64(int i)
		{
			try
			{
				return base.GetInt64(i);
			}
			catch (InvalidCastException ex)
			{
				GXLogging.Warn(log, "GetInt64 invalidCast", ex);
				return Convert.ToInt64(GxODPManagedOracle.OracleDecimalValue(base.GetValue(i)));
			}
		}
		public override decimal GetDecimal(int i)
		{
			try
			{
				return base.GetDecimal(i);
			}
			catch (InvalidCastException ex)
			{
				GXLogging.Warn(log, "GetDecimal invalidCast", ex);
				return Convert.ToDecimal(GxODPManagedOracle.OracleDecimalValue(base.GetValue(i)));
			}
		}
		public override short GetInt16(int i)
		{
			try
			{
				return base.GetInt16(i);
			}
			catch (InvalidCastException ex)
			{
				GXLogging.Warn(log, "GetInt16 invalidCast", ex);
				return Convert.ToInt16(GxODPManagedOracle.OracleDecimalValue(base.GetValue(i)));
			}
		}
		public override int GetInt32(int i)
		{
			try
			{
				return base.GetInt32(i);
			}
			catch (InvalidCastException ex)
			{
				GXLogging.Warn(log, "GetInt32 invalidCast", ex);
				return Convert.ToInt32(GxODPManagedOracle.OracleDecimalValue(base.GetValue(i)));
			}
		}
		public override bool IsDBNull(int i)
		{
			return GxODPManagedOracle.OracleDecimalNullValue(block.Item(pos, i)) || base.IsDBNull(i);
		}
	}
	public class GxOracleCacheDataReader : GxCacheDataReader
	{
		public GxOracleCacheDataReader(CacheItem cacheItem, bool computeSize, string keyCache)
			: base(cacheItem, computeSize, keyCache)
		{ }

		public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			byte[] cell = (byte[])block.Item(pos, i);
			int j;
			for (j = 0; j < length && fieldOffset < cell.Length; j++)
			{
				buffer[bufferoffset] = cell[fieldOffset];
				fieldOffset++;
				bufferoffset++;
			}
			if (computeSizeInBytes) readBytes += j;
			if (j == 1) j = 0; 
			return j;
		}

	}
	
	public class GxODPOracle : GxOracle
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GxODPOracle));
		static Assembly _odpAssembly;
		const string OracleDbTypeEnum = "Oracle.DataAccess.Client.OracleDbType";

		public static Assembly OdpAssembly
		{
			get
			{
				try
				{
					if (_odpAssembly == null)
					{
						GXLogging.Debug(log, "Loading Oracle.DataAccess from GAC");
						_odpAssembly = Assembly.LoadWithPartialName("Oracle.DataAccess");
						GXLogging.Debug(log, "Oracle.DataAccess Loaded from GAC");
					}

				}
				catch (Exception ex)
				{
					GXLogging.Error(log, "Error loading Oracle.DataAccess from GAC", ex);
				}
				if (_odpAssembly == null)
				{
					_odpAssembly = Assembly.Load("Oracle.DataAccess");
				}
				return _odpAssembly;
			}
		}
		public override IDataReader GetDataReader(IGxConnectionManager connManager, IGxConnection con, GxParameterCollection parameters, string stmt, ushort fetchSize, bool forFirst, int handle, bool cached, SlidingTime expiration, bool hasNested, bool dynStmt)
		{
			return new GxODPOracleDataReader(connManager, this, con, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt);
		}

		public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra, GxConnectionCache connectionCache)
		{
#if !NETCORE
			if (showPrompt)
			{
				GetConnectionDialogString(1);
			}
			else
#endif
			{
				if (m_connectionString == null)
					m_connectionString = BuildConnectionString(datasourceName, userId, userPassword, databaseName, port, schema, extra);
			}
			GXLogging.Debug(log, "Setting connectionString property ", ConnectionStringForLog);
			return new OracleConnectionWrapper(m_connectionString, connectionCache, isolationLevel);
		}
		protected override string BuildConnectionString(string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra)
		{
			StringBuilder connectionString = new StringBuilder();

			if (!string.IsNullOrEmpty(datasourceName) && !hasKey(extra, "Data Source"))
			{
				connectionString.AppendFormat("Data Source={0};", datasourceName);
			}
			if (userId != null)
			{
				connectionString.AppendFormat(";User ID={0};Password={1}", userId, userPassword);
			}
			if (extra != null)
			{
				string res = ParseAdditionalData(extra, "database");
				res = ParseAdditionalData(res, "integrated security");

				if (!res.StartsWith(";") && res.Length > 1) connectionString.Append(";");
				connectionString.Append(res);
			}
			return connectionString.ToString();

		}
		public override IDataReader GetCacheDataReader(CacheItem item, bool computeSize, string keyCache)
		{
			return new GxODPCacheDataReader(item, computeSize, keyCache);
		}
		public override void AddParameters(IDbCommand cmd, GxParameterCollection parameters)
		{
			try
			{
				base.AddParameters(cmd, parameters);
			}
			catch (Exception ex)
			{
				GXLogging.Warn(log, "AddParameters error", ex);
				for (int j = 0; j < parameters.Count; j++)
				{

					cmd.Parameters.Add(ClassLoader.Invoke(parameters[j], "Clone", null));
				}

			}
		}

		public override IDbDataParameter CreateParameter()
		{
			return (IDbDataParameter)ClassLoader.CreateInstance(OdpAssembly, "Oracle.DataAccess.Client.OracleParameter");
		}
		public override int BatchUpdate(DbDataAdapterElem da)
		{
			DataRowCollection rows = da.DataTable.Rows;
			DbParameterCollection parms = da.Adapter.InsertCommand.Parameters;
			int columns = da.DataTable.Columns.Count;
			object[][] values = new object[columns][];
			for (int i = 0; i < columns; i++)
			{
				values[i] = new object[rows.Count];
			}
			for (int rowidx = 0; rowidx < rows.Count; rowidx++)
			{
				for (int colidx = 0; colidx < columns; colidx++)
				{
					values[colidx][rowidx] = rows[rowidx][colidx];
				}
			}
			for (int i = 0; i < parms.Count; i++)
			{
				parms[i].Value = values[i];
			}
			da.Command = da.Adapter.InsertCommand;

			object ocmd = da.Command;
			int oldArrayBindCount = (int)ClassLoader.GetPropValue(ocmd, "ArrayBindCount");
			ClassLoader.SetPropValue(ocmd, "ArrayBindCount", da.DataTable.Rows.Count);
			try
			{
				da.Command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				foreach (object p in da.Command.Parameters)
				{
					ClassLoader.SetPropValue(p, "ArrayBindStatus", null);
					ClassLoader.SetPropValue(p, "ArrayBindSize", null);
					object none = ClassLoader.GetEnumValue(OdpAssembly, "Oracle.DataAccess.Client.OracleCollectionType", "None");
					ClassLoader.SetPropValue(p, "CollectionType", none);
					ClassLoader.SetPropValue(p, "Value", null);
					object nullFetched = ClassLoader.GetEnumValue(OdpAssembly, "Oracle.DataAccess.Client.OracleParameterStatus", "NullFetched");
					ClassLoader.SetPropValue(p, "Status", nullFetched);
				}
				ClassLoader.SetPropValue(ocmd, "ArrayBindCount", oldArrayBindCount);
			}
			return 0;
		}
		public override DbDataAdapter CreateDataAdapeter()
		{
			Type odpAdapter = OdpAssembly.GetType("Oracle.DataAccess.Client.OracleDataAdapter");
			return (DbDataAdapter)Activator.CreateInstance(odpAdapter);
		}
		public override IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec)
		{
			IDbDataParameter parm = (IDbDataParameter)ClassLoader.CreateInstance(OdpAssembly, "Oracle.DataAccess.Client.OracleParameter");
			ClassLoader.SetPropValue(parm, "OracleDbType", GXTypeToOracleType((GXType)dbtype));
			ClassLoader.SetPropValue(parm, "Size", gxlength);
			ClassLoader.SetPropValue(parm, "Scale", (byte)gxdec);
			ClassLoader.SetPropValue(parm, "ParameterName", name);
			
			return parm;
		}
		private Object GXTypeToOracleType(GXType type)
		{

			switch (type)
			{
				case GXType.Number: return ClassLoader.GetEnumValue(OdpAssembly, OracleDbTypeEnum, "Decimal");
				case GXType.NVarChar: return ClassLoader.GetEnumValue(OdpAssembly, OracleDbTypeEnum, "NVarchar2");
				case GXType.LongVarChar: return ClassLoader.GetEnumValue(OdpAssembly, OracleDbTypeEnum, "Long");
				case GXType.VarChar: return ClassLoader.GetEnumValue(OdpAssembly, OracleDbTypeEnum, "Varchar2");
				case GXType.DateTime: return ClassLoader.GetEnumValue(OdpAssembly, OracleDbTypeEnum, "Date");
				case GXType.DateTime2: return ClassLoader.GetEnumValue(OdpAssembly, OracleDbTypeEnum, "TimeStamp");
				default: return ClassLoader.GetEnumValue(OdpAssembly, OracleDbTypeEnum, type.ToString());
			}
		}
		public override bool IsBlobType(IDbDataParameter idbparameter)
		{
			object otype = ClassLoader.GetPropValue(idbparameter, "OracleDbType");
			object blobType = ClassLoader.GetEnumValue(OdpAssembly, OracleDbTypeEnum, "Blob");
			return (int)otype == (int)blobType;
		}

		public override bool ProcessError(int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)
		
		{
			GXLogging.Debug(log, "ProcessError: dbmsErrorCode=", () => dbmsErrorCode + ", emsg '" + emsg + "'");
			switch (dbmsErrorCode)
			{
				case 54:        // Locked
					retry = Retry(errMask, retryCount);
					if (retry)
						status = 110;// Locked - Retry
					else
						status = 103;//Locked
					return retry;
				case 1:     // Duplicated record
				case 24381: // Duplicated record in array (batch insert)
					status = 1;
					break;
				case 942:       // table or view does not exist
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
				case 950:       // File not found
				case 1418:      // File not found
				case 1432:      // File not found
				case 4902:      // File not found
				case 2443:      // File not found
				case 2289:      // File not found
					status = 105;
					break;
				case 2291:      // Parent key not found
					if ((errMask & GxErrorMask.GX_MASKFOREIGNKEY) == 0)
					{
						status = 500;       // ForeignKeyError
						return false;
					}
					break;
				case 4080:      // Trigger not found
					break;
				default:
					status = 999;
					return false;
			}
			return true;
		}
		public override void SetParameterLVChar(IDbDataParameter parameter, string value, IGxDataStore datastore)
		{
			SetParameterVChar(parameter, value);
		}
		static Type OracleDecimal = OdpAssembly.GetType("Oracle.DataAccess.Types.OracleDecimal");
		static Type OracleDate = OdpAssembly.GetType("Oracle.DataAccess.Types.OracleDate");
		static Type OracleClob = OdpAssembly.GetType("Oracle.DataAccess.Types.OracleClob");
		internal static object OracleDecimalValue(object value)
		{

			if (value.GetType() == OracleDecimal)
			{
				return ClassLoader.GetPropValue(value, "Value");
			}
			else return value;
		}
		internal static bool OracleDecimalNullValue(object value)
		{

			if (value.GetType() == OracleDecimal)
			{
				return Convert.ToBoolean(ClassLoader.GetPropValue(value, "IsNull"));
			}
			else return false;
		}
		internal static object OracleDateValue(object value)
		{

			if (value.GetType() == OracleDate)
			{
				return ClassLoader.GetPropValue(value, "Value");
			}
			else return value;
		}
		internal static object OracleClobValue(object value)
		{

			if (value.GetType() == OracleClob)
			{
				return ClassLoader.GetPropValue(value, "Value");
			}
			else return value;
		}
		internal static bool IsClobType(object value)
		{

			return (value.GetType() == OracleClob);
		}
	}
	public class GxODPManagedOracle : GxOracle
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GxODPManagedOracle));
		static Assembly _odpAssembly;
		const string OracleDbTypeEnum = "Oracle.ManagedDataAccess.Client.OracleDbType";

		public static Assembly OdpAssembly
		{
			get
			{
				try
				{
					if (_odpAssembly == null)
					{
						string assemblyPath = Path.Combine(FileUtil.GetStartupDirectory(), "Oracle.ManagedDataAccess.dll");
						GXLogging.Debug(log, "Loading Oracle.ManagedDataAccess from:" + assemblyPath);
#if NETCORE
						var asl = new AssemblyLoader(FileUtil.GetStartupDirectory());
						_odpAssembly = asl.LoadFromAssemblyPath(assemblyPath);
#else
						if (File.Exists(assemblyPath))
						{
							_odpAssembly = Assembly.LoadFrom(assemblyPath);
						}
						else
						{
							GXLogging.Debug(log, "Loading Oracle.ManagedDataAccess from GAC");
							_odpAssembly = Assembly.LoadWithPartialName("Oracle.ManagedDataAccess");
						}
#endif
						GXLogging.Debug(log, "Oracle.ManagedDataAccess Loaded:" + _odpAssembly.FullName + " location: " + _odpAssembly.Location + " CodeBase:" + _odpAssembly.CodeBase);
					} 

				}
				catch (Exception ex)
				{
					GXLogging.Error(log, "Error loading Oracle.ManagedDataAccess", ex);
				}
				if (_odpAssembly == null)
				{
					_odpAssembly = Assembly.Load("Oracle.ManagedDataAccess");
				}
				return _odpAssembly;
			}
		}
		public override IDataReader GetDataReader(IGxConnectionManager connManager, IGxConnection con, GxParameterCollection parameters, string stmt, ushort fetchSize, bool forFirst, int handle, bool cached, SlidingTime expiration, bool hasNested, bool dynStmt)
		{
			return new GxODPManagedOracleDataReader(connManager, this, con, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt);
		}

		public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra, GxConnectionCache connectionCache)
		{
#if !NETCORE
			if (showPrompt)
			{
				GetConnectionDialogString(1);
			}
			else
#endif
			{
				if (m_connectionString == null)
					m_connectionString = BuildConnectionString(datasourceName, userId, userPassword, databaseName, port, schema, extra);
			}
			GXLogging.Debug(log, "Setting connectionString property ", ConnectionStringForLog);
			return new OracleManagedConnectionWrapper(m_connectionString, connectionCache, isolationLevel);
		}

		protected override string BuildConnectionString(string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra)
		{
			StringBuilder connectionString = new StringBuilder();

			if (!string.IsNullOrEmpty(datasourceName) && !hasKey(extra, "Data Source"))
			{
				connectionString.AppendFormat("Data Source={0};", datasourceName);
			}
			if (userId != null)
			{
				connectionString.AppendFormat(";User Id={0};Password={1}", userId, userPassword);
			}
			if (extra != null)
			{
				string res = ParseAdditionalData(extra, "database");
				res = ParseAdditionalData(res, "integrated security");

				if (!res.StartsWith(";") && res.Length > 1) connectionString.Append(";");
				connectionString.Append(res);
			}
			return connectionString.ToString();

		}
		public override IDataReader GetCacheDataReader(CacheItem item, bool computeSize, string keyCache)
		{
			return new GxODPManagedCacheDataReader(item, computeSize, keyCache);
		}
		public override void AddParameters(IDbCommand cmd, GxParameterCollection parameters)
		{
			try
			{
				base.AddParameters(cmd, parameters);
			}
			catch (Exception ex)
			{
				GXLogging.Warn(log, "AddParameters error", ex);
				for (int j = 0; j < parameters.Count; j++)
				{

					cmd.Parameters.Add(ClassLoader.Invoke(parameters[j], "Clone", null));
				}

			}
		}

		public override IDbDataParameter CreateParameter()
		{
			return (IDbDataParameter)ClassLoader.CreateInstance(OdpAssembly, "Oracle.ManagedDataAccess.Client.OracleParameter");
		}
		public override int BatchUpdate(DbDataAdapterElem da)
		{
			DataRowCollection rows = da.DataTable.Rows;
			DbParameterCollection parms = da.Adapter.InsertCommand.Parameters;
			int columns = da.DataTable.Columns.Count;
			object[][] values = new object[columns][];
			for (int i = 0; i < columns; i++)
			{
				values[i] = new object[rows.Count];
			}
			for (int rowidx = 0; rowidx < rows.Count; rowidx++)
			{
				for (int colidx = 0; colidx < columns; colidx++)
				{
					values[colidx][rowidx] = rows[rowidx][colidx];
				}
			}
			for (int i = 0; i < parms.Count; i++)
			{
				parms[i].Value = values[i];
			}
			da.Command = da.Adapter.InsertCommand;

			object ocmd = da.Command;
			int oldArrayBindCount = (int)ClassLoader.GetPropValue(ocmd, "ArrayBindCount");
			ClassLoader.SetPropValue(ocmd, "ArrayBindCount", da.DataTable.Rows.Count);
			try
			{
				da.Command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				foreach (object p in da.Command.Parameters)
				{
					ClassLoader.SetPropValue(p, "ArrayBindStatus", null);
					ClassLoader.SetPropValue(p, "ArrayBindSize", null);
					object none = ClassLoader.GetEnumValue(OdpAssembly, "Oracle.ManagedDataAccess.Client.OracleCollectionType", "None");
					ClassLoader.SetPropValue(p, "CollectionType", none);
					ClassLoader.SetPropValue(p, "Value", null);
					object nullFetched = ClassLoader.GetEnumValue(OdpAssembly, "Oracle.ManagedDataAccess.Client.OracleParameterStatus", "NullFetched");
					ClassLoader.SetPropValue(p, "Status", nullFetched);
				}
				ClassLoader.SetPropValue(ocmd, "ArrayBindCount", oldArrayBindCount);
			}
			return 0;
		}
		public override DbDataAdapter CreateDataAdapeter()
		{
			Type odpAdapter = OdpAssembly.GetType("Oracle.ManagedDataAccess.Client.OracleDataAdapter");
			return (DbDataAdapter)Activator.CreateInstance(odpAdapter);
		}
		public override IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec)
		{
			IDbDataParameter parm = (IDbDataParameter)ClassLoader.CreateInstance(OdpAssembly, "Oracle.ManagedDataAccess.Client.OracleParameter");
			ClassLoader.SetPropValue(parm, "OracleDbType", GXTypeToOracleType((GXType)dbtype));
			ClassLoader.SetPropValue(parm, "Size", gxlength);
			ClassLoader.SetPropValue(parm, "Scale", (byte)gxdec);
			ClassLoader.SetPropValue(parm, "ParameterName", name);
			
			return parm;
		}
		private Object GXTypeToOracleType(GXType type)
		{

			switch (type)
			{
				case GXType.Number: return ClassLoader.GetEnumValue(OdpAssembly, OracleDbTypeEnum, "Decimal");
				case GXType.NVarChar: return ClassLoader.GetEnumValue(OdpAssembly, OracleDbTypeEnum, "NVarchar2");
				case GXType.LongVarChar: return ClassLoader.GetEnumValue(OdpAssembly, OracleDbTypeEnum, "Long");
				case GXType.VarChar: return ClassLoader.GetEnumValue(OdpAssembly, OracleDbTypeEnum, "Varchar2");
				case GXType.DateTime: return ClassLoader.GetEnumValue(OdpAssembly, OracleDbTypeEnum, "Date");
				default: return ClassLoader.GetEnumValue(OdpAssembly, OracleDbTypeEnum, type.ToString());
			}
		}
		public override bool IsBlobType(IDbDataParameter idbparameter)
		{
			object otype = ClassLoader.GetPropValue(idbparameter, "OracleDbType");
			object blobType = ClassLoader.GetEnumValue(OdpAssembly, OracleDbTypeEnum, "Blob");
			return (int)otype == (int)blobType;
		}

		public override bool ProcessError(int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)
		
		{
			GXLogging.Debug(log, "ProcessError: dbmsErrorCode=", () => dbmsErrorCode + ", emsg '" + emsg + "'");
			switch (dbmsErrorCode)
			{
				case 54:        // Locked
					retry = Retry(errMask, retryCount);
					if (retry)
						status = 110;// Locked - Retry
					else
						status = 103;//Locked
					return retry;
				case 1:     // Duplicated record
				case 24381: // Duplicated record in array (batch insert)
					status = 1;
					break;
				case 942:       // table or view does not exist
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
				case 950:       // File not found
				case 1418:      // File not found
				case 1432:      // File not found
				case 4902:      // File not found
				case 2443:      // File not found
				case 2289:      // File not found
					status = 105;
					break;
				case 2291:      // Parent key not found
					if ((errMask & GxErrorMask.GX_MASKFOREIGNKEY) == 0)
					{
						status = 500;       // ForeignKeyError
						return false;
					}
					break;
				case 4080:      // Trigger not found
					break;
				default:
					status = 999;
					return false;
			}
			return true;
		}
		public override void SetParameterLVChar(IDbDataParameter parameter, string value, IGxDataStore datastore)
		{
			SetParameterVChar(parameter, value);
		}
		static Type OracleDecimal = OdpAssembly.GetType("Oracle.ManagedDataAccess.Types.OracleDecimal");
		static Type OracleDate = OdpAssembly.GetType("Oracle.ManagedDataAccess.Types.OracleDate");
		static Type OracleClob = OdpAssembly.GetType("Oracle.ManagedDataAccess.Types.OracleClob");
		internal static object OracleDecimalValue(object value)
		{

			if (value.GetType() == OracleDecimal)
			{
				return ClassLoader.GetPropValue(value, "Value");
			}
			else return value;
		}
		internal static bool OracleDecimalNullValue(object value)
		{

			if (value.GetType() == OracleDecimal)
			{
				return Convert.ToBoolean(ClassLoader.GetPropValue(value, "IsNull"));
			}
			else return false;
		}
		internal static object OracleDateValue(object value)
		{

			if (value.GetType() == OracleDate)
			{
				return ClassLoader.GetPropValue(value, "Value");
			}
			else return value;
		}
		internal static object OracleClobValue(object value)
		{

			if (value.GetType() == OracleClob)
			{
				return ClassLoader.GetPropValue(value, "Value");
			}
			else return value;
		}
		internal static bool IsClobType(object value)
		{

			return (value.GetType() == OracleClob);
		}
	}
	//Microsoft data provider
	public class GxOracle : GxDataRecord
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GxOracle));
#if !NETCORE
		public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra, GxConnectionCache connectionCache)
		{
			if (showPrompt)
			{
				GetConnectionDialogString(1);
			}
			else
			{
				if (m_connectionString == null)
					m_connectionString = BuildConnectionString(datasourceName, userId, userPassword, databaseName, port, schema, extra);
			}
			GXLogging.Debug(log, "Setting connectionString property ", ConnectionStringForLog);
			return new MSOracleConnectionWrapper(m_connectionString, connectionCache, isolationLevel);
		}
#else
		public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra, GxConnectionCache connectionCache)
		{
			return null;
		}
#endif
		protected override string BuildConnectionString(string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra)
		{
			StringBuilder connectionString = new StringBuilder();

			if (!string.IsNullOrEmpty(datasourceName) && !hasKey(extra, "Data Source"))
			{
				connectionString.AppendFormat("Data Source={0};", datasourceName);
			}

			if (userId != null)
			{
				connectionString.AppendFormat(";User ID={0};Password={1}", userId, userPassword);
			}
			if (extra != null)
			{
				string res = ParseAdditionalData(extra, "database");

				if (!res.StartsWith(";") && res.Length > 1) connectionString.Append(";");
				connectionString.Append(res);
			}
			return connectionString.ToString();

		}
		public override long GetBytes(IGxDbCommand cmd, IDataRecord DR, int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
		{
			long count = base.GetBytes(cmd, DR, i, fieldOffset, buffer, bufferOffset, length);
			if (count == 1 && buffer[0] == (byte)0 && fieldOffset == 0)
			{
				GXLogging.Debug(log, "GetBytes count=1, byte 0");
				count = 0; //1-byte array is considered empty_blob in oracle.
			}
			return count;
		}

		public override bool GetBoolean(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return (base.GetInt(cmd, DR, i) == 1);
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
				String geoStr = DR.GetString(i);
				
				gtmp.FromString(geoStr);
				return gtmp;
			}
		}

		public override bool AllowsDuplicateParameters
		{
			get
			{
				return false;
			}
		}
#if !NETCORE
		public override IDbDataParameter CreateParameter()
		{
			return new MSOracleProvider.OracleParameter();
		}
		public override IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec)
		{
			MSOracleProvider.OracleParameter parm = new MSOracleProvider.OracleParameter();
			parm.OracleType = GXTypeToOracleType(dbtype);
			
			parm.Size = gxlength;
			parm.Scale = (byte)gxdec;
			parm.ParameterName = name;
			return parm;
		}
		private MSOracleProvider.OracleType GXTypeToOracleType(Object type)
		{
			if (type is MSOracleProvider.OracleType)
				return (MSOracleProvider.OracleType)type;
			else
			{
				GXType gxtype = (GXType)type;
				switch (gxtype)
				{
					case GXType.Blob: return MSOracleProvider.OracleType.Blob;
					case GXType.Byte: return MSOracleProvider.OracleType.Byte;
					case GXType.Char: return MSOracleProvider.OracleType.Char;
					case GXType.Clob: return MSOracleProvider.OracleType.Clob;
					case GXType.DateTime: return MSOracleProvider.OracleType.DateTime;
					case GXType.Int16: return MSOracleProvider.OracleType.Int16;
					case GXType.Int32: return MSOracleProvider.OracleType.Int32;
					case GXType.LongVarChar: return MSOracleProvider.OracleType.LongVarChar;
					case GXType.NChar: return MSOracleProvider.OracleType.NChar;
					case GXType.NClob: return MSOracleProvider.OracleType.NClob;
					case GXType.Number: return MSOracleProvider.OracleType.Number;
					case GXType.NVarChar: return MSOracleProvider.OracleType.NVarChar;
					case GXType.Raw: return MSOracleProvider.OracleType.Raw;
					case GXType.VarChar: return MSOracleProvider.OracleType.VarChar;
					default: return MSOracleProvider.OracleType.Char;
				}
			}
		}
		public override DbDataAdapter CreateDataAdapeter()
		{
			return new MSOracleProvider.OracleDataAdapter();
		}
#else
		public override IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec)
		{
			return null;
		}
		public override IDbDataParameter CreateParameter()
		{
			return null;
		}
		public override DbDataAdapter CreateDataAdapeter()
		{
			throw new NotImplementedException();
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
			GXLogging.Debug(log, "ExecuteReader: client cursor=", () => hasNested + ", handle '" + handle + "'" + ", hashcode " + this.GetHashCode());
			idatareader = new GxDataReader(connManager, this, con, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt);
			return idatareader;

		}
		public override IDataReader GetCacheDataReader(CacheItem item, bool computeSize, string keyCache)
		{
			return new GxOracleCacheDataReader(item, computeSize, keyCache);
		}
#if !NETCORE
		public override void AddParameters(IDbCommand cmd, GxParameterCollection parameters)
		{
			try
			{
				base.AddParameters(cmd, parameters);
			}
			catch (Exception ex)
			{
				GXLogging.Warn(log, "AddParameters error", ex);
				for (int j = 0; j < parameters.Count; j++)
				{
					MSOracleProvider.OracleParameter p1 = (MSOracleProvider.OracleParameter)parameters[j];
					MSOracleProvider.OracleParameter p2 =
						new MSOracleProvider.OracleParameter(p1.ParameterName, p1.OracleType, p1.Size,
						p1.Direction, p1.IsNullable, p1.Precision, p1.Scale, p1.SourceColumn, p1.SourceVersion, p1.Value);
					cmd.Parameters.Add(p2);
				}

			}
		}
#endif
		public override void SetParameterChar(IDbDataParameter parameter, string value)
		{
			if (!Preferences.CompatibleEmptyStringAsNull() && (String.IsNullOrEmpty(value) || String.IsNullOrEmpty(value.TrimEnd(' '))))
			{
				SetParameter(parameter, " ");
			}
			else
			{
				SetParameter(parameter, value);
			}
		}
#if !NETCORE
		public override void SetParameterLVChar(IDbDataParameter parameter, string value, IGxDataStore datastore)
		{
			if (!Preferences.CompatibleEmptyStringAsNull() && (String.IsNullOrEmpty(value) || String.IsNullOrEmpty(value.TrimEnd(' '))))
			{
				value = " ";
			}
			else
			{
				value = StringUtil.RTrim(value);
				//In oracle 'aa' != 'aa ', unlike other dbms, that's why rtrim is necessary.
			}
			MSOracleConnectionWrapper oracleConnection = null;
			if (IsClobType(parameter))
			{
				if (!datastore.Connection.Opened)
				{
					datastore.Connection.Open();
					oracleConnection = (MSOracleConnectionWrapper)datastore.Connection.InternalConnection;
				}
				if (value != null && value.Length > parameter.Size)
				{
					value = value.Substring(0, parameter.Size);
				}
			}
			if (IsClobType(parameter) && oracleConnection != null && oracleConnection.IsOracle8() && !oracleConnection.AutoCommit)
			{
				GXLogging.Debug(log, "Setting Clob parameter in Oracle8");
				string stmt = String.Format("declare {0} clob; begin dbms_lob.createtemporary({0}, false, 0); :tempclob := {0}; end;", parameter.ParameterName);
				Byte[] b = Encoding.Unicode.GetBytes(value);

				GxParameterCollection parmsColl = new GxParameterCollection();
				IDbDataParameter parm = CreateParameter("tempclob", MSOracleProvider.OracleType.Clob, parameter.Size, 0);
				parm.Direction = ParameterDirection.Output;
				parmsColl.Add(parm);
				IDbCommand cmd = GetCommand(datastore.Connection, stmt, parmsColl);
				cmd.ExecuteNonQuery();

				MSOracleProvider.OracleLob tempLob = (MSOracleProvider.OracleLob)((IDataParameter)cmd.Parameters[0]).Value;
				tempLob.BeginBatch(MSOracleProvider.OracleLobOpenMode.ReadWrite);
				tempLob.Write(b, 0, b.Length);
				tempLob.EndBatch();

				SetParameter(parameter, tempLob);

			}
			else
			{
				SetParameter(parameter, value);
			}
		}
#endif
		public override void SetParameterVChar(IDbDataParameter parameter, string value)
		{
			if (!Preferences.CompatibleEmptyStringAsNull() && (String.IsNullOrEmpty(value) || String.IsNullOrEmpty(value.TrimEnd(' '))))
			{
				SetParameter(parameter, " ");
			}
			else
			{
				SetParameter(parameter, StringUtil.RTrim(value));
				//In oracle 'aa' != 'aa ', unlike other dbms, that's why rtrim is necessary
			}
		}
#if !NETCORE
		public override bool IsBlobType(IDbDataParameter idbparameter)
		{
			return ((MSOracleProvider.OracleParameter)idbparameter).OracleType == MSOracleProvider.OracleType.Blob;
		}

		private bool IsClobType(IDbDataParameter idbparameter)
		{
			return ((MSOracleProvider.OracleParameter)idbparameter).OracleType == MSOracleProvider.OracleType.Clob;
		}
#endif
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
		protected override void SetBinary(IDbDataParameter parameter, byte[] binary)
		{
			GXLogging.Debug(log, "SetParameter BLOB, binary.length:", () => binary != null ? binary.Length.ToString() : "null");
			if (binary != null && binary.Length == 0)
			{
				binary = new byte[1];
			}
			else
			{
				parameter.Size = binary.Length;
			}
			parameter.Value = binary;
		}
		public override string GetServerDateTimeStmtMs(IGxConnection connection)
		{
			return "SELECT SYSDATE(3) FROM DUAL";
		}
		public override string GetServerDateTimeStmt(IGxConnection connection)
		{
			return "SELECT SYSDATE FROM DUAL";
		}
		public override string GetServerUserIdStmt()
		{
			return "SELECT USER FROM DUAL";
		}
		public override string GetServerVersionStmt()
		{
			return "SELECT version from product_component_version where PRODUCT like '%Oracle%'";
		}
		internal override void CalculateRetryCount(out int maxRetryCount, out int sleepTimeout)
		{
			//In Oracle, the time-out lock is simulated waiting on the client
			if (m_lockTimeout == 0)
			{
				maxRetryCount = 0;      
				sleepTimeout = RETRY_SLEEP;
			}
			else
			{
				maxRetryCount = m_lockRetryCount;
				sleepTimeout = m_lockTimeout;
			}
		}
		public override bool ProcessError(int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)
		
		{
			GXLogging.Debug(log, "ProcessError: dbmsErrorCode=", () => dbmsErrorCode + ", emsg '" + emsg + "'");
			switch (dbmsErrorCode)
			{
				case 54:        // Locked
					retry = Retry(errMask, retryCount);
					if (retry)
						status = 110;// Locked - Retry
					else
						status = 103;//Locked
					return retry;
				case 1:     // Duplicated record
					status = 1;
					break;
				case 942:       // table or view does not exist
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
				case 950:       // File not found
				case 1418:      // File not found
				case 1432:      // File not found
				case 4902:      // File not found
				case 2443:      // File not found
				case 2289:      // File not found
					status = 105;
					break;
				case 2291:      // Parent key not found
					if ((errMask & GxErrorMask.GX_MASKFOREIGNKEY) == 0)
					{
						status = 500;       // ForeignKeyError
						return false;
					}
					break;
				case 4080:      // Trigger not found
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

		public override IGeographicNative Dbms2NetGeo(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return new Geospatial(DR.GetString(i));
		}
		public override Object Net2DbmsGeo(IDbDataParameter parm, IGeographicNative geo)
		{
			return geo.ToStringSQL();
		}

		private static readonly string[] ConcatOpValues = new string[] { string.Empty, " || ", string.Empty };
		public override string ConcatOp(int pos)
		{
			return ConcatOpValues[pos];
		}

	}
	public class GxODPOracleDataReader : GxDataReader
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GxODPOracleDataReader));
		public GxODPOracleDataReader(IGxConnectionManager connManager, GxDataRecord dr, IGxConnection connection, GxParameterCollection parameters,
			string stmt, int fetchSize, bool forFirst, int handle, bool cached, SlidingTime expiration, bool dynStmt)
			: base(connManager, dr, connection, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt)
		{ }
		public override string GetString(int i)
		{
			string result = string.Empty;
			try
			{
				result = reader.GetString(i);
				readBytes += 10 + (2 * result.Length);
				return result;
			}
			catch (Exception ex)
			{
				Type type = reader.GetFieldType(i);
				GXLogging.Warn(log, "GetString InvalidCastException field type:" + type, ex);
				if (ex is InvalidCastException && type == typeof(Decimal))
				{
					object oracleDecimalValue = ClassLoader.Invoke(reader, "GetOracleDecimal", new object[] { i });
					return oracleDecimalValue.ToString();
				}
				else
				{
					GXLogging.Warn(log, "GetString Exception", ex);
					GXLogging.Warn(log, "Reader.GetType(" + i + "):" + reader.GetFieldType(i));
					return Convert.ToString(reader.GetValue(i));
				}
			}
		}
		public override decimal GetDecimal(int i)
		{
			readBytes += 12;
			try
			{
				return reader.GetDecimal(i);
			}
			catch (OverflowException ex)
			{
				Type type = reader.GetFieldType(i);
				GXLogging.Warn(log, "GetDecimal InvalidCastException field type:" + type, ex);
				try
				{
					//Reduce the precision
					//The Oracle data type NUMBER can hold up to 38 precision, and the .NET Decimal type can hold up to 28 precision. 
					object oracleDecimalValue = ClassLoader.Invoke(reader, "GetOracleDecimal", new object[] { i });
					object dvalue = ClassLoader.InvokeStatic(GxODPOracle.OdpAssembly, "Oracle.DataAccess.Types.OracleDecimal", "SetPrecision", new object[] { oracleDecimalValue, 28 });
					return Convert.ToDecimal(GxODPOracle.OracleDecimalValue(dvalue));
				}
				catch (Exception ex1)
				{
					GXLogging.Error(log, "Error setting OracleDecimal Precision", ex1);
					throw ex1;
				}
			}
			catch (Exception ex1)
			{
				GXLogging.Warn(log, "Oracle GetDecimal Exception, parameter " + i + " ", ex1);
				return Convert.ToDecimal(reader.GetValue(i));
			}
		}
	}

	public class GxODPManagedOracleDataReader : GxDataReader
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GxODPManagedOracleDataReader));
		public GxODPManagedOracleDataReader(IGxConnectionManager connManager, GxDataRecord dr, IGxConnection connection, GxParameterCollection parameters,
			string stmt, int fetchSize, bool forFirst, int handle, bool cached, SlidingTime expiration, bool dynStmt)
			: base(connManager, dr, connection, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt)
		{ }
		public override string GetString(int i)
		{
			string result = string.Empty;
			try
			{
				result = reader.GetString(i);
				readBytes += 10 + (2 * result.Length);
				return result;
			}
			catch (Exception ex)
			{
				Type type = reader.GetFieldType(i);
				GXLogging.Warn(log, "GetString InvalidCastException field type:" + type, ex);
				if (ex is InvalidCastException && type == typeof(Decimal))
				{
					object oracleDecimalValue = ClassLoader.Invoke(reader, "GetOracleDecimal", new object[] { i });
					return oracleDecimalValue.ToString();
				}
				else
				{
					GXLogging.Warn(log, "GetString Exception", ex);
					GXLogging.Warn(log, "Reader.GetType(" + i + "):" + reader.GetFieldType(i));
					return Convert.ToString(reader.GetValue(i));
				}
			}
		}
		public override decimal GetDecimal(int i)
		{
			readBytes += 12;
			try
			{
				return reader.GetDecimal(i);
			}
			catch (InvalidCastException ex)
			{
				Type type = reader.GetFieldType(i);
				GXLogging.Warn(log, "GetDecimal InvalidCastException field type:" + type, ex);
				try
				{
					//Reduce the precision
					//The Oracle data type NUMBER can hold up to 38 precision, and the .NET Decimal type can hold up to 28 precision. 
					object oracleDecimalValue = ClassLoader.Invoke(reader, "GetOracleDecimal", new object[] { i });
					object dvalue = ClassLoader.InvokeStatic(GxODPManagedOracle.OdpAssembly, "Oracle.ManagedDataAccess.Types.OracleDecimal", "SetPrecision", new object[] { oracleDecimalValue, 28 });
					return Convert.ToDecimal(GxODPManagedOracle.OracleDecimalValue(dvalue));
				}
				catch (Exception ex1)
				{
					GXLogging.Error(log, "Error setting OracleDecimal Precision", ex1);
					throw ex1;
				}
			}
			catch (Exception ex1)
			{
				GXLogging.Warn(log, "Oracle GetDecimal Exception, parameter " + i + " ", ex1);
				return Convert.ToDecimal(reader.GetValue(i));
			}
		}
	}
#if !NETCORE
	sealed internal class MSOracleConnectionWrapper : GxAbstractConnectionWrapper
	{
		static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		int oracle8 = -1;
		public MSOracleConnectionWrapper() : base(new MSOracleProvider.OracleConnection())
		{
		}

		public MSOracleConnectionWrapper(String connectionString, GxConnectionCache connCache, IsolationLevel isolationLevel)
		{
			try
			{
				Type oracleConnection = typeof(MSOracleProvider.OracleConnection);
				_connection = (IDbConnection)ClassLoader.CreateInstance(oracleConnection.Assembly, oracleConnection.FullName, new object[] { connectionString });
				m_isolationLevel = isolationLevel;
				m_connectionCache = connCache;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "MS Oracle data provider Ctr error " + ex.Message + ex.StackTrace);
				throw ex;
			}
		}

		public bool IsOracle8()
		{
			if (oracle8 == -1)
			{
				MSOracleProvider.OracleConnection sc = InternalConnection as MSOracleProvider.OracleConnection;
				if (null == sc)
					throw new InvalidOperationException("InvalidConnType00" + InternalConnection.GetType().FullName);
				oracle8 = sc.ServerVersion.StartsWith("8") ? 1 : 0;
			}
			return oracle8 == 1;
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
			MSOracleProvider.OracleConnection sc = InternalConnection as MSOracleProvider.OracleConnection;
			if (null == sc)
				throw new InvalidOperationException("InvalidConnType00" + InternalConnection.GetType().FullName);
			return sc.CreateCommand();
		}
		public override DbDataAdapter CreateDataAdapter()
		{
			return new MSOracleProvider.OracleDataAdapter();
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
#endif
	sealed internal class OracleConnectionWrapper : GxAbstractConnectionWrapper
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(OracleConnectionWrapper));
		public OracleConnectionWrapper()
		{
			try
			{
				GXLogging.Debug(log, "Creating Oracle data provider ");
				_connection = (IDbConnection)ClassLoader.CreateInstance(GxODPOracle.OdpAssembly, "Oracle.DataAccess.Client.OracleConnection");
				GXLogging.Debug(log, "Oracle data provider created");
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Oracle data provider Ctr error " + ex.Message + ex.StackTrace);
				throw ex;
			}
		}

		public OracleConnectionWrapper(String connectionString, GxConnectionCache connCache, IsolationLevel isolationLevel)
		{
			try
			{
				_connection = (IDbConnection)ClassLoader.CreateInstance(GxODPOracle.OdpAssembly, "Oracle.DataAccess.Client.OracleConnection", new object[] { connectionString });
				m_isolationLevel = isolationLevel;
				m_connectionCache = connCache;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Oracle data provider Ctr error " + ex.Message + ex.StackTrace);
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

		override public IDbCommand CreateCommand()
		{
			
			IDbCommand cmd = InternalConnection.CreateCommand();
			ClassLoader.SetPropValue(cmd, "BindByName", true);
			return cmd;
		}
		public override DbDataAdapter CreateDataAdapter()
		{
			return (DbDataAdapter)ClassLoader.CreateInstance(GxODPOracle.OdpAssembly, "Oracle.DataAccess.Client.OracleDataAdapter");
		}
		public override short SetSavePoint(IDbTransaction transaction, string savepointName)
		{
			ClassLoader.Invoke(transaction, "Save", new object[] { savepointName });
			return 0;
		}
		public override short ReleaseSavePoint(IDbTransaction transaction, string savepointName)
		{
			return 0;
		}
		public override short RollbackSavePoint(IDbTransaction transaction, string savepointName)
		{
			ClassLoader.Invoke(transaction, "Rollback", new object[] { savepointName });
			return 0;
		}
	}

	sealed internal class OracleManagedConnectionWrapper : GxAbstractConnectionWrapper
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(OracleManagedConnectionWrapper));
		public OracleManagedConnectionWrapper()
		{
			try
			{
				GXLogging.Debug(log, "Creating Managed Oracle data provider ");
				_connection = (IDbConnection)ClassLoader.CreateInstance(GxODPManagedOracle.OdpAssembly, "Oracle.ManagedDataAccess.Client.OracleConnection");
				GXLogging.Debug(log, "Managed Oracle data provider created");
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Managed Oracle data provider Ctr error " + ex.Message + ex.StackTrace);
				throw ex;
			}
		}

		public OracleManagedConnectionWrapper(String connectionString, GxConnectionCache connCache, IsolationLevel isolationLevel)
		{
			try
			{
				_connection = (IDbConnection)ClassLoader.CreateInstance(GxODPManagedOracle.OdpAssembly, "Oracle.ManagedDataAccess.Client.OracleConnection", new object[] { connectionString });
				m_isolationLevel = isolationLevel;
				m_connectionCache = connCache;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Managed Oracle data provider Ctr error " + ex.Message + ex.StackTrace);
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

		override public IDbCommand CreateCommand()
		{
			
			IDbCommand cmd = InternalConnection.CreateCommand();
			ClassLoader.SetPropValue(cmd, "BindByName", true);
			return cmd;
		}
		public override DbDataAdapter CreateDataAdapter()
		{
			return (DbDataAdapter)ClassLoader.CreateInstance(GxODPManagedOracle.OdpAssembly, "Oracle.ManagedDataAccess.Client.OracleDataAdapter");
		}
		public override short SetSavePoint(IDbTransaction transaction, string savepointName)
		{
			ClassLoader.Invoke(transaction, "Save", new object[] { savepointName });
			return 0;
		}
		public override short ReleaseSavePoint(IDbTransaction transaction, string savepointName)
		{
			return 0;
		}
		public override short RollbackSavePoint(IDbTransaction transaction, string savepointName)
		{
			ClassLoader.Invoke(transaction, "Rollback", new object[] { savepointName });
			return 0;
		}
	}
}