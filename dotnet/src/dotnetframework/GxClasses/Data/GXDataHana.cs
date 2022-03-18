using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Text;
using GeneXus.Cache;
using GeneXus.Metadata;
using GeneXus.Utils;
using GxClasses.Helpers;
using log4net;

namespace GeneXus.Data
{
	public class GxHana : GxDataRecord
    {
        static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static Assembly _hanaAssembly;
        const string HanaDbTypeEnum = "Sap.Data.Hana.HanaDbType";
#if NETCORE
        const string _hanaAssemblyName = "Sap.Data.Hana.Core.v2.1";
#else
        const string _hanaAssemblyName = "Sap.Data.Hana.v3.5";
#endif

		public static Assembly HanaAssembly
        {
            get
            {
                try
                {
                    if (_hanaAssembly == null)
                    {
#if NETCORE
						string assemblyPath = Path.Combine(FileUtil.GetStartupDirectory(), $"{_hanaAssemblyName}.dll");
						GXLogging.Debug(log, $"Loading {_hanaAssemblyName} from:" + assemblyPath);
						_hanaAssembly = AssemblyLoader.LoadAssembly(new AssemblyName(_hanaAssemblyName));
#else
						GXLogging.Debug(log, "Loading Sap.Data.Hana.v3.5 from GAC");
            _hanaAssembly = Assembly.LoadWithPartialName("Sap.Data.Hana.v3.5");
#endif
						GXLogging.Debug(log, $"{_hanaAssemblyName} Loaded:" + _hanaAssembly.FullName + " location: " + _hanaAssembly.Location);
					}
				}

				catch (Exception ex)
                {
                    GXLogging.Error(log, $"Error loading {_hanaAssemblyName}", ex);
                }
                if (_hanaAssembly == null)
                {
                    _hanaAssembly = Assembly.Load(_hanaAssemblyName);
                }
                return _hanaAssembly;
            }
        }

        public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId,
            string userPassword, string databaseName, string port, string schema,string extra, GxConnectionCache connectionCache)
        {
            if (m_connectionString == null)
                m_connectionString = BuildConnectionString(datasourceName, userId, userPassword, databaseName, port, schema, extra);
			GXLogging.Debug(log, "Setting connectionString property ", ConnectionStringForLog);

            return new HanaConnectionWrapper(m_connectionString, connectionCache, isolationLevel);
        }


        protected override string BuildConnectionString(string datasourceName, string userId,
        string userPassword, string databaseName, string port, string schema, string extra)
        {
            StringBuilder connectionString = new StringBuilder();

            if (!string.IsNullOrEmpty(datasourceName) && port != null && port.Trim().Length > 0 && !hasKey(extra, "Server"))
            {
                connectionString.AppendFormat("Server={0}:{1};", datasourceName, port);
            }
            else if (datasourceName != null && !hasKey(extra, "Server"))
            {
                connectionString.AppendFormat("Server={0}:30015;", datasourceName);
            }
            if (userId != null)
            {
                connectionString.AppendFormat(";UserID={0};Password={1}", userId, userPassword);
            }
            if (databaseName != null && databaseName.Trim().Length > 0 && !hasKey(extra, "Database"))
            {
                connectionString.AppendFormat(";Database={0}", databaseName);
            }
            if (schema != null && schema.Trim().Length > 0 && !hasKey(extra, "Current Schema"))
            {
                connectionString.AppendFormat(";Current Schema={0}", schema);
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
            return (IDbDataParameter)ClassLoader.CreateInstance(HanaAssembly, "Sap.Data.Hana.HanaParameter");
        }

        public override IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec)
        {
            IDbDataParameter parm = (IDbDataParameter)ClassLoader.CreateInstance(HanaAssembly, "Sap.Data.Hana.HanaParameter");
            ClassLoader.SetPropValue(parm, "HanaDbType", GXTypeToHanaType((GXType)dbtype));
            ClassLoader.SetPropValue(parm, "Size", gxlength);
            ClassLoader.SetPropValue(parm, "Precision", (byte)gxlength);
            ClassLoader.SetPropValue(parm, "Scale", (byte)gxdec);
            ClassLoader.SetPropValue(parm, "ParameterName", name);
            return parm;
        }

       
        private Object GXTypeToHanaType(GXType type)
        {

			switch (type)
			{
				case GXType.Int16: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, "SmallInt");
				case GXType.Int32: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, "Integer");
				case GXType.Int64: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, "BigInt");
				case GXType.Number: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, "Decimal");
				case GXType.VarChar: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, "VarChar");
				case GXType.NVarChar: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, "NVarChar");
				case GXType.Geography:
				case GXType.Geoline:
				case GXType.Geopoint:
				case GXType.Geopolygon:
				case GXType.UniqueIdentifier:
				case GXType.Char: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, "VarChar");
				case GXType.NChar: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, "NVarChar");
				case GXType.LongVarChar: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, "VarChar");
				case GXType.Clob: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, "Clob");
				case GXType.NClob: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, "NClob");
				case GXType.Blob: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, "Blob");
				case GXType.DateTime: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, "SecondDate");
				case GXType.DateTime2: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, "TimeStamp");
				case GXType.Date: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, "Date");
				case GXType.Byte: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, "TinyInt");
				default: return ClassLoader.GetEnumValue(HanaAssembly, HanaDbTypeEnum, type.ToString());
			}
        }

        public override DbDataAdapter CreateDataAdapeter()
        {
            Type iAdapter = HanaAssembly.GetType("Sap.Data.Hana.HanaDataAdapter");
            return (DbDataAdapter)Activator.CreateInstance(iAdapter);
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
            GXLogging.Debug(log, "ExecuteReader: client cursor=", ()=> hasNested + ", handle '" + handle + "'" + ", hashcode " + this.GetHashCode());
            idatareader = new GxDataReader(connManager, this, con, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt);
            return idatareader;

        }

        public override bool ProcessError( int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)
		{
			GXLogging.Debug(log, "ProcessError: dbmsErrorCode=" + dbmsErrorCode +", emsg '"+ emsg + "'");
			switch (dbmsErrorCode)
			{
                case 301:		// Unique constraint violated
                    status = 1;
                    break;  
                case 1299: //No data found
                    status = 105;
                    break;
                case 429:		// Parent key not found
                    if ((errMask & GxErrorMask.GX_MASKFOREIGNKEY) == 0)
                    {
                        status = 500;		// ForeignKeyError
                        return false;
                    }
                    break;
                case -10709: // Timeout
                    status = 1;
                    break;
                default:
                    status = 999;
                    return false;
            }
            return true;

         }

        public override IGeographicNative Dbms2NetGeo(IGxDbCommand cmd, IDataRecord DR, int i)
        {
            return new Geospatial(DR.GetString(i));
        }

        public override Object Net2DbmsGeo(GXType type, IGeographicNative geo)
        {
            return geo.ToStringSQL();
        }
		public override void SetParameter(IDbDataParameter parameter, Object value)
		{
			if (value == null || value == DBNull.Value)
			{
				parameter.Value = DBNull.Value;
			}
			else if (IsBlobType(parameter))
			{
				GXLogging.Debug(log, "SetParameter BLOB value:" + value);
				SetBinary(parameter, GetBinary((string)value, false));
			}
			else if (value is Guid)
			{
				parameter.Value = value.ToString();
			}
			else if (parameter.DbType == DbType.Decimal)
			{

				int size = parameter.Size;
				byte scale = (byte)ClassLoader.GetPropValue(parameter, "Scale");
				Decimal intPart = Decimal.Truncate(Convert.ToDecimal(value));

				if (intPart.ToString().Length <= size)
				{
					parameter.Value = NumberUtil.Trunc((decimal)value, scale);
				}
				else
				{
					parameter.Value = 0;
				}
			}
			else
			{
				parameter.Value = CheckDataLength(value, parameter);
			}
		}
		public override void SetBinary(IDbDataParameter parameter, byte[] binary)
		{
			GXLogging.Debug(log, "SetParameter BLOB, binary.length:" + binary.Length);
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
		public override string GetServerDateTimeStmt(IGxConnection connection)
        {
            return "SELECT CURRENT_TIMESTAMP FROM DUMMY";
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
            return "CURENT_USER";
        }

		public override IDbCommand GetCachedCommand(IGxConnection con, string stmt)
        {
            return con.ConnectionCache.GetAvailablePreparedCommand(stmt);
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
                //WKT Representation
                Geospatial gtmp = new Geospatial();
                String geoStr = DR.GetString(i);
                
                gtmp.FromString(geoStr);
                return gtmp;
            }
        }

		private static readonly string[] ConcatOpValues = new string[] { string.Empty, " || ", string.Empty };
		public override string ConcatOp(int pos)
		{
			return ConcatOpValues[pos];
		}

	}
	sealed internal class HanaConnectionWrapper : GxAbstractConnectionWrapper
    {
        const string HanaIsolationEnum = "Sap.Data.Hana.HanaIsolationLevel";

        static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public HanaConnectionWrapper()
        {
            try
            {
                _connection = (IDbConnection)ClassLoader.CreateInstance(GxHana.HanaAssembly, "Sap.Data.Hana.HanaConnection");
            }
            catch (Exception ex)
            {
                GXLogging.Error(log, "Hana data provider Ctr error " + ex.Message + ex.StackTrace);
                throw ex;
            }
        }

        private Object GXIsolationToHanaIsolation(IsolationLevel level)
        {
            switch (level)
            {

                case IsolationLevel.ReadUncommitted: return ClassLoader.GetEnumValue(GxHana.HanaAssembly, HanaIsolationEnum, "ReadUncommitted");
                case IsolationLevel.ReadCommitted: return ClassLoader.GetEnumValue(GxHana.HanaAssembly, HanaIsolationEnum, "ReadCommitted");
                case IsolationLevel.RepeatableRead: return ClassLoader.GetEnumValue(GxHana.HanaAssembly, HanaIsolationEnum, "RepeatableRead");
                case IsolationLevel.Serializable: return ClassLoader.GetEnumValue(GxHana.HanaAssembly, HanaIsolationEnum, "Serializable");
                default: return ClassLoader.GetEnumValue(GxHana.HanaAssembly, HanaIsolationEnum, "Serializable");

            }
        }

        public HanaConnectionWrapper(String connectionString, GxConnectionCache connCache, IsolationLevel isolationLevel)
        {
            try
            {                
                string isoStr = "";
                switch (isolationLevel)
                {
                    
                    case IsolationLevel.Serializable:
                    case IsolationLevel.RepeatableRead:
                        isoStr = ";Isolation level=Serializable;";
                       
                        break;
                    default:
                        isoStr = "";
                        break;
                }
                _connection = (IDbConnection)ClassLoader.CreateInstance(GxHana.HanaAssembly, "Sap.Data.Hana.HanaConnection", new object[] { connectionString +  isoStr});
                ClassLoader.SetPropValue(_connection, "HanaIsolationLevel", GXIsolationToHanaIsolation(isolationLevel));
                m_isolationLevel = isolationLevel;
                m_connectionCache = connCache;
            }
            catch (Exception ex)
            {
                GXLogging.Error(log, "Hana data provider Ctr error " + ex.Message + ex.StackTrace);
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
}