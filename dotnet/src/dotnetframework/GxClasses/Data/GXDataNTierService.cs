using System.Text;

using System;
using System.Data;
using System.Collections;
using GeneXus.Data.ADO;
using GeneXus.Configuration;
using GeneXus.Cache;
using GeneXus.Utils;
using System.IO;
using log4net;
using GeneXus.Application;
using System.Collections.Generic;
using GeneXus.Data.NTier.ADO;
using System.Reflection;
using GeneXus.Metadata;
using System.Data.Common;
using GeneXus.Helpers;

namespace GeneXus.Data.NTier
{

	public class ServiceError
	{
		public const string RecordNotFound = "Record not found";
		public const string RecordAlreadyExists = "Record already exists";
	}

	public class ServiceException : Exception
	{
		public ServiceException(string msg):base(msg)
		{ }

		public ServiceException(string msg, Exception innerException): base(msg, innerException)
		{ }
	}

	public class ServiceCursorDef : CursorDef
	{
		public enum CursorType
		{
			Select,
			Insert,
			Update,
			Delete
		}

		public ServiceCursorDef(string name, object query, bool current, GxErrorMask nmask, bool hold,
			IDataStoreHelper parent, Object[] parmBinds, short blockSize, int cachingCategory, bool hasNested,
			bool isForFirst) : base(name, QueryId(name, query), current, nmask, hold, parent, parmBinds, blockSize,
			cachingCategory, hasNested, isForFirst)
		{
			Query = query;
		}

		public ServiceCursorDef(string name, object query, GxErrorMask nmask, Object[] parmBinds) : base(name, QueryId(name, query), nmask, parmBinds)
		{ 
			Query = query;
		}

		private static string QueryId(string name, object query) { return string.Format("Service:{0}_{1}", name, query.GetHashCode()); }

		public object Query { get; internal set; }
	}

	public class GxServiceFactory
	{
		protected static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static GxService Create(string id, string providerId, string serviceClass)		
		{
			return (GxService)Activator.CreateInstance(AssemblyHelper.GetRuntimeType(serviceClass), new object[2] { id, providerId });
		}
	}

	public class GxService : GxDataRecord
	{
		protected static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private Type m_ServiceType;
		private CursorDef m_CursorDef;

		public GxService(string id, string providerId)
		{			
		}

		public GxService(string id, string providerId, string runtimeClass)
		{
			m_ServiceType = AssemblyHelper.GetRuntimeType(runtimeClass);
		}

		public GxService(string id, string providerId, Type serviceType)
		{
			m_ServiceType = serviceType;
		}

		public override void SetCursorDef(CursorDef cursorDef)
		{
			m_CursorDef = cursorDef;
		}
		public override IDbCommand GetCommand(IGxConnection con, string stmt, GxParameterCollection parameters)
		{
			IDbCommand cmd = base.GetCommand(con, stmt, parameters);
			IServiceCommand iCmd = cmd as IServiceCommand;
			if (iCmd != null)
				iCmd.CursorDef = m_CursorDef;
			return cmd;
		}

		public static void log_msg(string msg)
		{			
			GXLogging.Debug(log, msg);
		}

		public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra, GxConnectionCache connectionCache)
		{
			if (string.IsNullOrEmpty(m_connectionString))
				m_connectionString = BuildConnectionString(datasourceName, userId, userPassword, databaseName, port, schema, extra);
			GXLogging.Debug(log, "Setting connectionString property ", () => BuildConnectionString(datasourceName, userId, NaV, databaseName, port, schema, extra));

			return new ServiceConnectionWrapper(m_ServiceType, m_connectionString, connectionCache, isolationLevel, DataSource);
		}

		protected override string BuildConnectionString(string datasourceName, string userId,
		string userPassword, string databaseName, string port, string schema, string extra)
		{

			StringBuilder connectionString = new StringBuilder();
			if (!string.IsNullOrEmpty(datasourceName) )
			{
				connectionString.AppendFormat("Data Source={0};", datasourceName);
			}
			if (userId != null)
			{
				connectionString.AppendFormat(";User ID={0};Password={1}", userId, userPassword);
			}
			if (!string.IsNullOrEmpty(extra))
			{
			
				connectionString.AppendFormat(";{0}", extra);
			}
			return connectionString.ToString();
		}

		public override IDbDataParameter CreateParameter()
		{
			return new ServiceParameter();
		}
		public override IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec)
		{
			ServiceParameter parm = new ServiceParameter();
			parm.DbType = GXTypeToDbType((GXType)dbtype);
			
			parm.Size = gxlength;
			parm.Precision = (byte)gxlength;
			parm.Scale = (byte)gxdec;
			parm.ParameterName = name;
			return parm;
		}

		public override Object Net2DbmsGeo(GXType type, IGeographicNative geo)
		{
			return geo.InnerValue;
		}

		public static DbType GXTypeToDbType(GXType type)
		{
			switch (type)
			{
				case GXType.Number: return DbType.Int32;
				case GXType.Int16: return DbType.Int16;
				case GXType.Int32: return DbType.Int32;
				case GXType.Int64: return DbType.Int64;
				case GXType.Date: return DbType.Date;
				case GXType.DateTime: return DbType.DateTime;
                case GXType.DateTime2: return DbType.DateTime2;
                case GXType.Byte: return DbType.Byte;
				case GXType.NChar: 
				case GXType.NClob:
				case GXType.NVarChar: 
				case GXType.Char: 
				case GXType.LongVarChar: return DbType.String;
				case GXType.Clob: return DbType.Binary;
				case GXType.VarChar: return DbType.String;
				case GXType.Raw:
					return DbType.Object;
				case GXType.Blob:
					return DbType.Binary;
				case GXType.Boolean:
					return DbType.Boolean;
				case GXType.Undefined:
				default:
					return DbType.Object;
			}
		}

		public override DbDataAdapter CreateDataAdapeter()
		{
			throw new NotImplementedException();
			
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
			GXLogging.Debug(log, "ExecuteReader: client cursor=", () => hasNested + ", handle '" + handle + "'" + ", hashcode " + this.GetHashCode());
			idatareader = new GxDataReader(connManager, this, con, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt);
			return idatareader;
		}

		public override bool ProcessError(int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)
		{
			switch (emsg)
			{
				case ServiceError.RecordNotFound:
					{
						status = Cursor.EOF;
						retry = false;
						return true;
					}
				case ServiceError.RecordAlreadyExists:
					{
						status = 1;
						retry = false;
						return true;
					}
				default:
					{
						if (con?.InternalConnection?.InternalConnection.GetType().Name == "FabricConnection")
						{
							status = 1;
							retry = false;
							return true;
						}
						else
						{
							// SAP
							int idx = emsg.IndexOf("<code>SY/530</code>");
							if (idx > 0 &&
									(emsg.IndexOf("not found.</message>", idx) > 0 ||
									emsg.IndexOf("missing.</message>", idx) > 0
									))
							{ // in SAP an error is returned when record is not found
								status = Cursor.EOF;
								retry = false;
								return true;
							}
						}
						break;
					}
			}
			return false;
		}

		public override void SetParameter(IDbDataParameter parameter, Object value)
		{
			
				base.SetParameter(parameter, value);
		}
		public override string GetServerDateTimeStmt(IGxConnection connection)
		{
			throw new NotImplementedException();			
		}
		public override string GetServerDateTimeStmtMs(IGxConnection connection)
		{
			throw new NotImplementedException();
		}
		public override string GetServerVersionStmt()
		{
			throw new GxNotImplementedException();
		}
		public override string GetServerUserIdStmt()
		{
			throw new NotImplementedException();
			
		}

		public override IDbCommand GetCachedCommand(IGxConnection con, string stmt)
		{
			return con.ConnectionCache.GetAvailablePreparedCommand(stmt);
		}

		public override IGeographicNative GetGeospatial(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			if (!cmd.HasMoreRows || DR == null || DR.IsDBNull(i))
				return new Geospatial();
			else
			{
				Geospatial gtmp = new Geospatial();
				String geoStr = DR.GetValue(i) as String; 
				if(geoStr != null)
					gtmp.FromString(geoStr);
				return gtmp;
			}
		}

		private static readonly string[] ConcatOpValues = new string[] { string.Empty, " + ", string.Empty };
		public override string ConcatOp(int pos)
		{
			return ConcatOpValues[pos];
		}
	}

	public interface IServiceCommand
	{
		CursorDef CursorDef { get; set; }
	}

	sealed public class ServiceConnectionWrapper : GxAbstractConnectionWrapper
	{
		static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public ServiceConnectionWrapper(Type runtimeClassType, String connectionString, GxConnectionCache connCache, IsolationLevel isolationLevel, String dataSource)
		{
			try
			{
				_connection = (IDbConnection)Activator.CreateInstance(runtimeClassType);
				if (_connection is IServiceConnection)
					(_connection as IServiceConnection).DataSource = dataSource;
				ClassLoader.SetPropValue(_connection, "ConnectionString", connectionString);
				m_isolationLevel = isolationLevel;
				m_connectionCache = connCache;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Service data provider Ctr error " + ex.Message + ex.StackTrace);
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

		override public IDbTransaction BeginTransaction(IsolationLevel isoLevel)
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
					throw new GxADODataException("Begin transaction error in Hana", e);
				}
				return trn;
			}
		}
	}

	public sealed class ServiceParameter : DbParameter, ICloneable
	{
		DbType m_dbType = DbType.Object;
		ParameterDirection m_direction = ParameterDirection.Input;
		bool m_fNullable = false;
		string m_sParamName;
		string m_sSourceColumn;
		DataRowVersion m_sourceVersion = DataRowVersion.Current;
		object m_value = null;
		int m_size;
		bool _sourceColumnNullMapping;

		public ServiceParameter()
		{
		}

		private ServiceParameter(DbType dbType, ParameterDirection direction, bool fNullable, string sParamName, string sSourceColumn, DataRowVersion sourceVersion, object value, byte precision, byte scale, int size)
		{
			m_dbType = dbType;
			m_direction = direction;
			m_fNullable = fNullable;
			m_sParamName = sParamName;
			m_sSourceColumn = sSourceColumn;
			m_sourceVersion = sourceVersion;
			m_value = value;
			Precision = precision;
			Scale = scale;
			m_size = size;
		}

		public ServiceParameter(string parameterName, DbType type)
		{
			m_sParamName = parameterName;
			m_dbType = type;
		}

		public ServiceParameter(string parameterName, object value)
		{
			m_sParamName = parameterName;
			this.Value = value;
		}

		public ServiceParameter(string parameterName, DbType dbType, string sourceColumn)
		{
			m_sParamName = parameterName;
			m_dbType = dbType;
			m_sSourceColumn = sourceColumn;
		}

		public override DbType DbType
		{
			get { return m_dbType; }
			set { m_dbType = value; }
		}

		public override ParameterDirection Direction
		{
			get { return m_direction; }
			set { m_direction = value; }
		}

		public override Boolean IsNullable
		{
			get { return m_fNullable; }
			set { m_fNullable = value; }
		}

		public override void ResetDbType()
		{

		}

		public override String ParameterName
		{
			get { return m_sParamName; }
			set { m_sParamName = value; }
		}

		public override String SourceColumn
		{
			get { return m_sSourceColumn; }
			set { m_sSourceColumn = value; }
		}

		public override DataRowVersion SourceVersion
		{
			get { return m_sourceVersion; }
			set { m_sourceVersion = value; }
		}

		public override object Value
		{
			get
			{
				return m_value;
			}
			set
			{
				m_value = value;
				if (!(value is Array))
				{
					m_dbType = _inferType(value);
				}
			}
		}

		private DbType _inferType(Object value)
		{
			TypeCode code;

			if (value == DBNull.Value || value == null)
				code = TypeCode.DBNull;
			else
				code = Type.GetTypeCode(value.GetType());
			switch (code)
			{
				case TypeCode.Empty:
					throw new SystemException("Invalid data type");

				case TypeCode.Object:
					return DbType.Object;

				case TypeCode.DBNull:
					return DbType.Object;

				case TypeCode.Char:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					
					throw new SystemException("Invalid data type");

				case TypeCode.Boolean:
					return DbType.Byte;

				case TypeCode.Byte:
					return DbType.Byte;

				case TypeCode.Int16:
					return DbType.Int16;

				case TypeCode.Int32:
					return DbType.Int32;

				case TypeCode.Int64:
					return DbType.Int64;

				case TypeCode.Single:
					return DbType.Single;

				case TypeCode.Double:
					return DbType.Double;

				case TypeCode.Decimal:
					return DbType.Decimal;

				case TypeCode.DateTime:
					if (m_dbType == DbType.Date)
						return DbType.Date;
					else
						return DbType.DateTime;

				case TypeCode.String:
					return DbType.String;

				default:
					throw new SystemException("Value is of unknown data type");
			}
		}
		#region IDbDataParameter Members
		
		public override int Size
		{
			get { return m_size; }
			set { m_size = value; }
		}

		#endregion

		#region ICloneable Members

		public object Clone()
		{
			return new ServiceParameter(m_dbType, m_direction, m_fNullable, m_sParamName,
				m_sSourceColumn, m_sourceVersion, m_value, Precision, Scale, m_size);
		}

		#endregion

		public override bool SourceColumnNullMapping
		{
			get
			{
				return this._sourceColumnNullMapping;
			}
			set
			{
				this._sourceColumnNullMapping = value;
			}
		}
	}
}