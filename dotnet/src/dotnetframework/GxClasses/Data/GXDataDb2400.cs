using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
#if !NETCORE
using Microsoft.HostIntegration.MsDb2Client;
#endif
using log4net;
using GeneXus.Application;
using GeneXus.Cache;
using GeneXus.Configuration;
using GeneXus.Utils;

using GeneXus.Metadata;
using System.Globalization;

namespace GeneXus.Data
{
	public class GxDb2ISeriesDataReader : GxDataReader
	{
		public GxDb2ISeriesDataReader( IGxConnectionManager connManager, GxDataRecord dr, IGxConnection connection, GxParameterCollection parameters ,
			string stmt, int fetchSize, bool forFirst, int handle, bool cached, SlidingTime expiration, bool dynStmt)
			:base(connManager, dr, connection, parameters ,stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt)
		{}

		public override string GetString(int i)
		{
            string value = GxDb2ISeries.GetDB2String(reader, i);
			if (value!=null) 
				return value;
			else 
				return base.GetString( i);
		}
		public override object GetValue(int i)
		{
            string value = GxDb2ISeries.GetDB2String(reader, i);
			if (value!=null) 
				return value;
			else 
				return base.GetValue( i);

		}

	}
	sealed internal class Db2ISeriesConnectionWrapper : GxAbstractConnectionWrapper
	{
		static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public Db2ISeriesConnectionWrapper()
		{
			try
			{
				_connection = (IDbConnection)ClassLoader.CreateInstance(GxDb2ISeries.iAssembly, "IBM.Data.DB2.iSeries.iDB2Connection");
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "iSeries data provider Ctr error " + ex.Message + ex.StackTrace);
				throw ex;
			}
		}

		public Db2ISeriesConnectionWrapper(String connectionString, GxConnectionCache connCache, IsolationLevel isolationLevel)
		{
			try
			{
				_connection = (IDbConnection)ClassLoader.CreateInstance(GxDb2ISeries.iAssembly, "IBM.Data.DB2.iSeries.iDB2Connection", new object[] { connectionString });
				m_isolationLevel = isolationLevel;
				m_connectionCache = connCache;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "iSeries data provider Ctr error " + ex.Message + ex.StackTrace);
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
				GXLogging.Error(log, "Return GxConnection.Open Error m_isolationLevel:" + m_isolationLevel, e);
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

	}

	public class GxDb2ISeries : GxDataRecord
	{

		static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private bool m_UseCharInDate;
		public static string SQL_NULL_DATE="00000000";
		private string m_InitialCatalog;
		private static int closeCursorMethod=-1;
		private static int freeUnmanagedMemoryMethod=-1;
		const string iDB2DbTypeEnum = "IBM.Data.DB2.iSeries.iDB2DbType";
		static Assembly _iAssembly;

        public static Assembly iAssembly
        {
            get
            {
                try
                {
                    if (_iAssembly == null)
                    {
                        _iAssembly = Assembly.Load("IBM.Data.DB2.iSeries");
						if (_iAssembly != null) GXLogging.Debug(log, "IBM.Data.DB2.iSeries Loaded from bin ", _iAssembly.FullName);
                    }
                }
                catch (Exception ex)
                {
                    GXLogging.Error(log, "Error loading IBM.Data.DB2.iSeries from GAC", ex);
                }
                if (_iAssembly == null)
                {
					GXLogging.Debug(log, "Loading IBM.Data.DB2.iSeries from GAC");
                    _iAssembly = Assembly.LoadWithPartialName("IBM.Data.DB2.iSeries");
					if (_iAssembly != null) GXLogging.Debug(log, "IBM.Data.DB2.iSeries Loaded from GAC", _iAssembly.FullName);
                }
                return _iAssembly;
            }
        }

		public GxDb2ISeries(string id)
		{
			string userCharInDate;
			
			bool isConfigured = Config.GetValueOf("Connection-"+id+"-DB2400_DATE_DATATYPE",out userCharInDate);
			
			m_UseCharInDate= !isConfigured || (isConfigured && userCharInDate.ToLower()=="character");

			string catalog="";
			Config.GetValueOf("Connection-"+id+"-Catalog",out catalog);
			m_InitialCatalog=catalog;
		}

		public static string GetDB2String(IDataReader reader, int i)
		{
            Type idb2Type = (Type)ClassLoader.Invoke(reader, "GetiDB2FieldType", new object[] { i });
			GXLogging.Debug(log, "GetDB2String field Type: " + idb2Type);

            if (idb2Type == iAssembly.GetType("IBM.Data.DB2.iSeries.iDB2CharBitData"))
			{
				return ClassLoader.Invoke(reader, "GetiDB2Char", new object[] { i }).ToString();
			}
			else if( idb2Type == iAssembly.GetType("IBM.Data.DB2.iSeries.iDB2VarCharBitData"))
			{
				return ClassLoader.Invoke(reader, "GetiDB2VarCharBitData", new object[] { i }).ToString();
			}
			else if (idb2Type == iAssembly.GetType("IBM.Data.DB2.iSeries.iDB2Clob"))
			{
				try
				{
					return (string)ClassLoader.GetPropValue(ClassLoader.Invoke(reader, "GetiDB2Clob", new object[] { i }), "Value");
				}
				catch (Exception ex)
				{
                    if (ex.GetType() == iAssembly.GetType("IBM.Data.DB2.iSeries.iDB2SQLParameterErrorException"))
                    {

                        try
                        {
                            GXLogging.Error(log, "GetiDB2Clob error", ex);
                           GXLogging.Debug(log, "GetiDB2Value " + ClassLoader.Invoke(reader, "GetiDB2Value", new object[] { i }));
                            return ClassLoader.Invoke(reader, "GetiDB2Value", new object[] { i }).ToString();
                        }
                        catch (Exception ex1)
                        {
                            if (ex1.GetType() == iAssembly.GetType("IBM.Data.DB2.iSeries.iDB2SQLParameterErrorException"))
                            {
                                GXLogging.Error(log, "GetiDB2Value error", ex1);
                               GXLogging.Debug(log, "GetString " + reader.GetString(i));
                                return reader.GetString(i);
                            }
                            else throw ex1;
                        }
                    }
                    else throw ex;
				}
			}
			else
			{
				return null;
			}

		}
		public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId, 
			string userPassword,string databaseName, string port, string schema, string extra, GxConnectionCache connectionCache)
		{
			if (m_connectionString == null)
				m_connectionString=BuildConnectionString(datasourceName, userId, userPassword, databaseName, port, schema, extra);
			GXLogging.Debug(log, "Setting connectionString property ", ConnectionStringForLog);

			return new Db2ISeriesConnectionWrapper(m_connectionString,connectionCache, isolationLevel);
		}

		
		protected override string BuildConnectionString(string datasourceName, string userId, 
			string userPassword,string databaseName, string port, string schema, string extra)
		{
			StringBuilder connectionString = new StringBuilder();

			if (!string.IsNullOrEmpty(datasourceName) && !hasKey(extra, "Data Source"))
			{
				connectionString.AppendFormat("Data Source={0};",datasourceName);
			}
			if (userId!=null)
			{
				connectionString.AppendFormat("User ID={0};Password={1};",userId,userPassword);
			}
			if (databaseName != null && databaseName.Trim().Length > 0 && !hasKey(extra, "Default Collection"))
			{
				connectionString.AppendFormat("Default Collection={0};", databaseName);
			}

			if (!String.IsNullOrEmpty(m_InitialCatalog) && !hasKey(extra, "Database"))
			{
				connectionString.AppendFormat("Database={0}", m_InitialCatalog);
			}

			if (extra!=null)
			{
				string res = ParseAdditionalData(extra,"integrated security");

				if (!res.StartsWith(";") && res.Length>1 && connectionString[connectionString.Length-1]!=';')	
					connectionString.Append(";");
				connectionString.Append(res);
			}
			return connectionString.ToString().Replace(";;", ";"); 

		}
        public override bool GetBoolean(IGxDbCommand cmd, IDataRecord DR, int i)
        {
            return (base.GetInt(cmd, DR, i) == 1);
        }
		protected override string ParseAdditionalData(string data,string extractWord)
		{
			char[] sep = {';'};
			StringBuilder res=new StringBuilder("");
			string [] props = data.Split(sep);
			foreach (string s in props)
			{
				if ( s!=null && s.Length>0 && !s.ToLower().StartsWith(extractWord))
				{
					if (s.ToLower().StartsWith("database"))
					{
						res.Append(s.ToUpper().Replace("DATABASE","Default Collection"));
					}
					else
					{
						res.Append(s);
					}
					res.Append(';');
				}
			}
			return res.ToString();
		}
		public override IDbDataParameter CreateParameter()
		{
            return (IDbDataParameter)ClassLoader.CreateInstance(iAssembly, "IBM.Data.DB2.iSeries.iDB2Parameter");
		}
		public override  IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec)
		{
            IDbDataParameter parm = (IDbDataParameter)ClassLoader.CreateInstance(iAssembly, "IBM.Data.DB2.iSeries.iDB2Parameter");
			ClassLoader.SetPropValue(parm, "iDB2DbType", GXTypeToiDB2DbType((GXType)dbtype));
			ClassLoader.SetPropValue(parm, "Size", gxlength);
            ClassLoader.SetPropValue(parm, "Precision", (byte)gxlength);
            ClassLoader.SetPropValue(parm, "Scale", (byte)gxdec);
            ClassLoader.SetPropValue(parm, "ParameterName", name);
            return parm;
		}
		private Object GXTypeToiDB2DbType(GXType type)
		{

			switch (type)
			{
				case GXType.Int16: return ClassLoader.GetEnumValue(iAssembly, iDB2DbTypeEnum, "iDB2SmallInt");
				case GXType.Int32: return ClassLoader.GetEnumValue(iAssembly, iDB2DbTypeEnum, "iDB2Integer");
				case GXType.Int64: return ClassLoader.GetEnumValue(iAssembly, iDB2DbTypeEnum, "iDB2BigInt");
				case GXType.Number: return ClassLoader.GetEnumValue(iAssembly, iDB2DbTypeEnum, "iDB2Double");
				case GXType.DateTime: return ClassLoader.GetEnumValue(iAssembly, iDB2DbTypeEnum, "iDB2TimeStamp");
                case GXType.DateTime2: return ClassLoader.GetEnumValue(iAssembly, iDB2DbTypeEnum, "iDB2TimeStamp");
				case GXType.UniqueIdentifier:
				case GXType.DateAsChar:
				case GXType.Char: return ClassLoader.GetEnumValue(iAssembly, iDB2DbTypeEnum, "iDB2Char");
				case GXType.Date: return ClassLoader.GetEnumValue(iAssembly, iDB2DbTypeEnum, "iDB2Date");
				case GXType.Clob: return ClassLoader.GetEnumValue(iAssembly, iDB2DbTypeEnum, "Clob");
				case GXType.VarChar: return ClassLoader.GetEnumValue(iAssembly, iDB2DbTypeEnum, "iDB2VarChar");
				case GXType.Blob: return ClassLoader.GetEnumValue(iAssembly, iDB2DbTypeEnum, "iDB2Blob");

				default: return ClassLoader.GetEnumValue(iAssembly, iDB2DbTypeEnum, type.ToString());
			}
		}
		public override bool SupportUpdateBatchSize
		{
			get
			{
				return false;
			}
		}

		internal bool UseCharInDate { get { return m_UseCharInDate; }}
		internal string InitialCatalog { get { return m_InitialCatalog; } set { m_InitialCatalog = value; } }

		public override DbDataAdapter CreateDataAdapeter()
        {
            Type iAdapter = iAssembly.GetType("IBM.Data.DB2.iSeries.iDB2DataAdapter");
            return (DbDataAdapter)Activator.CreateInstance(iAdapter);
        }
		public override void SetAdapterInsertCommand(DbDataAdapterElem da, IGxConnection con, string stmt, GxParameterCollection parameters)
		{
			Type iCommand = iAssembly.GetType("IBM.Data.DB2.iSeries.iDB2Command");
			ClassLoader.SetPropValue(da.Adapter, "InsertCommand", iCommand, GetCommand(con, stmt, parameters));
			object InsertCommand = ClassLoader.GetPropValue(da.Adapter, "InsertCommand", iCommand);
			ClassLoader.SetPropValue(InsertCommand, "UpdatedRowSource", UpdateRowSource.None);
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
			idatareader= new GxDb2ISeriesDataReader(connManager,this, con,parameters,stmt,fetchSize,forFirst,handle,cached,expiration,dynStmt);
			return idatareader;
		}
		public override bool IsBlobType(IDbDataParameter idbparameter)
		{

            object otype = ClassLoader.GetPropValue(idbparameter, "iDB2DbType");
            object blobType = ClassLoader.GetEnumValue(iAssembly, "IBM.Data.DB2.iSeries.iDB2DbType", "iDB2Blob");
            return (int)otype == (int)blobType;
		}

		public override void SetParameter(IDbDataParameter parameter, Object value)
		{
			if (value==null || value==DBNull.Value) 
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
				GXLogging.Debug(log, "SetParameter BLOB value:" + value);
				SetBinary(parameter, GetBinary((string)value, false));
			}

		}
		protected override void SetBinary(IDbDataParameter parameter, byte[] binary)
		{
			GXLogging.Debug(log, "SetParameter BLOB, binary.length:" + binary != null ? binary.Length.ToString() : "null");
			parameter.Value = binary;
		}
		public override string GetServerDateTimeStmtMs(IGxConnection connection)
		{
			return GetServerDateTimeStmt(connection);
		}

		public override string GetServerDateTimeStmt(IGxConnection connection)
		{
			string namingConvention = GetParameterValue(connection.Data, "Naming");
			if (namingConvention.Equals("system", StringComparison.OrdinalIgnoreCase))
				return "SELECT CURRENT_TIMESTAMP FROM SYSIBM/SYSDUMMY1";
			else
				return "SELECT CURRENT_TIMESTAMP FROM SYSIBM.SYSDUMMY1";
		}
		public override string GetServerUserIdStmt()
		{
			return null;
		}
		public override string GetServerVersionStmt()
		{
			throw new GxNotImplementedException();
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
				case -204:case -205:		// File not found
					if (GxContext.isReorganization)
					{
						status = 105; 
					}
					else
					{
						return false;
					}
					break;
				case -601: // File or LIB already exists
							//Message: SQL0601 AREORG002N en *N de tipo *LIB ya existe. => catchable code
							//Message: SQL0601 TSTRGZ4 en AREORG002N de tipo *FILE ya existe. => non-catchable code
					if (emsg.IndexOf("*LIB") < 0)
					{
						status = 999;
						return false;
					}
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

        public override DateTime Dbms2NetDate(IGxDbCommand cmd, IDataRecord DR, int i)
        {
            GXLogging.Debug(log, "GetDateTime - index : " + i);
            DateTime value;
			if (m_UseCharInDate)
			{
				string valueString = base.GetString(cmd, DR, i);

				GXLogging.Debug(log, "GetDateTime - value as string : ", valueString);
				bool parseOk = true;
				if (valueString == SQL_NULL_DATE || valueString.Trim().Length == 0)
					value = DateTimeUtil.NullDate();
				else
					parseOk = DateTime.TryParseExact(valueString, "yyyyMMdd", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out value);
				GXLogging.Debug(log, "GetDateTime as Char - value as date : " + value);
				if (!parseOk)
					value = DR.GetDateTime(i);

			}
			else
			{
				value = DR.GetDateTime(i);
				GXLogging.Debug(log, "GetDateTime - value2 : " + value);
			}
            return value;
        }
		
		public override Object Net2DbmsDateTime(IDbDataParameter parm, DateTime dateValue)
		{
            object otype = ClassLoader.GetPropValue(parm, "iDB2DbType");
            object charType = ClassLoader.GetEnumValue(iAssembly, "IBM.Data.DB2.iSeries.iDB2DbType", "iDB2Char");
            int typeSize = (int)ClassLoader.GetPropValue(parm, "Size");

            if (m_UseCharInDate && (int)otype == (int)charType && typeSize == 8)
			{
				string resString;
				if (dateValue.Equals(DateTimeUtil.NullDate()))
				{
					resString=SQL_NULL_DATE;
				}
				else
				{	
					resString=DateTimeUtil.getYYYYMMDD(dateValue);
				}
				return resString;
			}
			else
			{
				return dateValue;
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
        public override void GetValues(IDataReader reader, ref object[] values)
		{
			if (values!=null)
			{
				for (int i=0; i<values.Length; i++)
				{	
					if (!reader.IsDBNull(i))
					{
						if (reader.GetFieldType(i) == typeof(Decimal))
						{
							values[i] = reader.GetDecimal(i);
						}
						else if (reader.GetFieldType(i) == typeof(Byte[]))
						{
							// GetValue method on a null Blob cell throws an exception so GetBytes is used
							byte[] buf = new byte[255];  
							long retval = reader.GetBytes(i, 0, buf, 0, 255);
							if (retval == 0)
								values[i] = Array.Empty<byte>();
							else
								values[i] = reader.GetValue(i);
						}
						else
						{
							string value = GetDB2String(reader, i);
							if (value != null)
								values[i] = value;
							else
								values[i] = reader.GetValue(i);
						}
					}
					else
					{
						values[i] = reader.GetValue(i);
					}

				}
			}
		}
		internal override object CloneParameter(IDbDataParameter p)
		{
            return (IDbDataParameter)ClassLoader.CreateInstance(iAssembly, "IBM.Data.DB2.iSeries.iDB2Parameter",
                new object[]{ p.ParameterName, ClassLoader.GetPropValue(p, "iDB2DbType"), p.Size, p.Direction, p.IsNullable, p.Precision
                    , p.Scale, p.SourceColumn, p.SourceVersion, p.Value});
		}

		protected override IDbCommand GetCachedCommand(IGxConnection con, string stmt)
		{
			return 	con.ConnectionCache.GetAvailablePreparedCommand(stmt);
		}

		public override void DisposeCommand(IDbCommand command)
		{
			GXLogging.Debug(log, "DisposeCommand:" + command.CommandText);
            try
            {

                if (closeCursorMethod == -1)
                {
                    //Version 8.2.3 of the db2 udb does not have this method in the dataprovider, and supports more than one datareader per connection.
                    MethodInfo m = command.GetType().GetMethod("closeCursor", BindingFlags.Instance | BindingFlags.NonPublic);
                    closeCursorMethod = (m == null) ? 0 : 1;
                }
                int freeUnmanagedMemoryMethodParamCount = 0;
                if (freeUnmanagedMemoryMethod == -1)
                {
                    
                    MethodInfo m = command.GetType().GetMethod("freeUnmanagedMemory", BindingFlags.Instance | BindingFlags.NonPublic);
                    freeUnmanagedMemoryMethod = (m == null) ? 0 : 1;
                    freeUnmanagedMemoryMethodParamCount = m.GetParameters().Length;
                }
                if (freeUnmanagedMemoryMethod == 1 && command.Parameters.Count > 0)
                {
                    if (freeUnmanagedMemoryMethodParamCount > 0)
                    {
                        Type MpDcDataType = ClassLoader.FindType("IBM.Data.DB2.iSeries", "IBM.Data.DB2.iSeries.MpDcData", null);
                        Array MpDcDataArray = Array.CreateInstance(MpDcDataType, 0);
                        Object[] margs = new Object[] { MpDcDataArray };
                        command.GetType().InvokeMember("freeUnmanagedMemory",
                            BindingFlags.Instance |
                            BindingFlags.NonPublic |
                            BindingFlags.InvokeMethod,
                            null, command, margs);
                    }
                    else
                    {
                        command.GetType().InvokeMember("freeUnmanagedMemory",
                            BindingFlags.Instance |
                            BindingFlags.NonPublic |
                            BindingFlags.InvokeMethod,
                            null, command, null);
                    }
                }
                if (closeCursorMethod == 1)
                {
                    command.GetType().InvokeMember("closeCursor",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic |
                        BindingFlags.InvokeMethod,
                        null, command, null);
                }
                command.Dispose();
            }
            catch (Exception ex)
            {
                GXLogging.Error(log, "DisposeCommand error", ex);

            }
		}

	}
#if !NETCORE

	public class GxAccess : GxDataRecord
	{

        public override DbDataAdapter CreateDataAdapeter()
        {
            throw new Exception("The method or operation is not implemented.");
        }
		public override IDataReader GetDataReader(IGxConnectionManager connManager,IGxConnection connection, 
			GxParameterCollection parameters, string stmt, ushort fetchSize, bool forFirst, int handle, 
			bool cached, SlidingTime expiration, bool hasNested,bool dynStmt)
		{
			throw new GxNotImplementedException();
		}

		public override GxAbstractConnectionWrapper GetConnection(
			bool showPrompt, string datasourceName, string userId, 
			string userPassword,string databaseName, string port, string schema, string extra,
			GxConnectionCache connectionCache)
		{
			throw new GxNotImplementedException();
		}

		public override IDbDataParameter CreateParameter()
		{
			throw new GxNotImplementedException();
		}

		public override  IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec)
		{
			throw new GxNotImplementedException();
		}

		protected override string BuildConnectionString(string datasourceName, string userId, 
			string userPassword,string databaseName, string port, string schema,  string extra)
		{
			throw new GxNotImplementedException();
		}

		public override void SetParameter(IDbDataParameter parameter, Object value)
		{
			throw new GxNotImplementedException();
		}
		public override string GetServerDateTimeStmt(IGxConnection connection)
		{
			throw new GxNotImplementedException();
		}
		public override string GetServerDateTimeStmtMs(IGxConnection connection)
		{
			throw new GxNotImplementedException();
		}
		public override string GetServerUserIdStmt()
		{
			throw new GxNotImplementedException();
		}
		public override string GetServerVersionStmt()
		{
			throw new GxNotImplementedException();
		}
		public override bool ProcessError( int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)
			
		{
			switch (dbmsErrorCode)
			{
				case -1612:
				case -1613:		//conflicted with COLUMN FOREIGN KEY constraint
					if ((errMask & GxErrorMask.GX_MASKFOREIGNKEY) == 0)
					{
						status = 500;		// ForeignKeyError
						return false;
					}
					break; 
				case -1304:		// Locked
					retry = Retry(errMask, retryCount);
					if (retry)
						status=110;// Locked - Retry
					else 
						status=103;//Locked
					return retry;
				case -1605: // Duplicated record
					status = 1; 
					break; 
				case -3815: //File not found
				case -1404: 
				case -1305: 
					status = 105; 
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
				return "DATETIME( 0001-01-01) YEAR TO DAY";
			return "DATETIME("+
				Value.Year.ToString()+ "-"+
				Value.Month.ToString()+"-"+
				Value.Day.ToString()+" "+
				Value.Hour.ToString()+":"+
				Value.Minute.ToString()+":"+
				Value.Second.ToString()+") YEAR TO SECOND";
		}
	}
	sealed internal class Db2ISeriesHISConnectionWrapper : GxAbstractConnectionWrapper 
	{
		static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public Db2ISeriesHISConnectionWrapper() : base(new MsDb2Connection()) 
		{	}

		public Db2ISeriesHISConnectionWrapper(String connectionString, GxConnectionCache connCache, IsolationLevel isolationLevel) : base(new MsDb2Connection(connectionString),isolationLevel) 
		{
			m_connectionCache=connCache;
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
				GXLogging.Error(log, "Return GxConnection.Open Error m_isolationLevel:"+m_isolationLevel , e);
				throw (new GxADODataException(e));
			}
		}

		override public void Close() 
		{
			try 
			{
				InternalConnection.Close();
			}
			catch(MsDb2Exception ex) 
			{
				GXLogging.Error(log, "Return GxConnection.Close Error " , ex);
				throw new DataException(ex.Message, ex);
			}
		}

		override public IDbCommand CreateCommand() 
		{
			MsDb2Connection sc = (MsDb2Connection)InternalConnection;
			if(null == sc)
				throw new InvalidOperationException("InvalidConnType00" + InternalConnection.GetType().FullName);
			return sc.CreateCommand();
		}
		public override DbDataAdapter CreateDataAdapter()
		{
			throw new GxNotImplementedException();
		}

	}
	public class GxISeriesHIS : GxDataRecord
	{

		static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private bool m_UseCharInDate;
		private string m_InitialCatalog;
		private string m_SqlPackage;

		public static string SQL_NULL_DATE = "00000000";

		public GxISeriesHIS(string id)
		{
			string userCharInDate;
			bool isConfigured = Config.GetValueOf("Connection-" + id + "-DB2400_DATE_DATATYPE", out userCharInDate);

			m_UseCharInDate = !isConfigured || (isConfigured && userCharInDate.ToLower() == "character");

			string str = "";
			Config.GetValueOf("Connection-" + id + "-Catalog", out str);
			m_InitialCatalog = str;

			Config.GetValueOf("Connection-" + id + "-Package", out str);
			m_SqlPackage = str;
		}

		public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra, GxConnectionCache connectionCache)
		{
			if (m_connectionString == null)
				m_connectionString = BuildConnectionString(datasourceName, userId, userPassword, databaseName, port, schema, extra);
			GXLogging.Debug(log, "Setting connectionString property ", ConnectionStringForLog);

			return new Db2ISeriesHISConnectionWrapper(m_connectionString, connectionCache, isolationLevel);
		}


		protected override string BuildConnectionString(string datasourceName, string userId,
			string userPassword, string databaseName, string port, string schema, string extra)
		{

			StringBuilder connectionString = new StringBuilder();
			connectionString.Append("Provider=DB2OLEDB;Network Port=446;Network Transport Library=TCP;");

			if (!string.IsNullOrEmpty(datasourceName) && !hasKey(extra, "Network Address"))
			{
				connectionString.AppendFormat("Network Address={0};", datasourceName);
			}
			if (userId != null)
			{
				connectionString.AppendFormat("User ID={0};Password={1};", userId, userPassword);
			}
			if (databaseName != null && databaseName.Trim().Length > 0 && !hasKey(extra, "Default Qualifier"))
			{
				connectionString.AppendFormat("Default Qualifier={0};", databaseName);
			}
			if (!String.IsNullOrEmpty(m_InitialCatalog) && !hasKey(extra, "Initial Catalog"))
			{
				connectionString.AppendFormat("Initial Catalog={0};", m_InitialCatalog);
			}
			if (!string.IsNullOrEmpty(m_SqlPackage) && !hasKey(extra, "Package Collection"))
			{
				connectionString.AppendFormat("Package Collection={0}", m_SqlPackage);
			}

			if (extra != null)
			{
				string res = ParseAdditionalData(extra, "integrated security");
				res = ParseAdditionalData(res, "librarylist");

				if (!res.StartsWith(";") && res.Length > 1) connectionString.Append(";");
				connectionString.Append(res);
			}
			return connectionString.ToString();

		}

		protected override string ParseAdditionalData(string data, string extractWord)
		{
			char[] sep = { ';' };
			StringBuilder res = new StringBuilder("");
			data = data.Replace("=,", "=");
			string[] props = data.Split(sep);
			foreach (string s in props)
			{
				if (s != null && s.Length > 0 && !s.ToLower().StartsWith(extractWord))
				{
					if (s.ToLower().StartsWith("database"))
					{
						res.Append(s.ToUpper().Replace("DATABASE", "Default Qualifier"));
					}
					else
					{
						res.Append(s);
					}
					res.Append(';');
				}
			}
			return res.ToString();
		}

		public override IDbDataParameter CreateParameter()
		{
			return new MsDb2Parameter();
		}
		public override IDbDataParameter CreateParameter(string name, Object dbtype, int gxlength, int gxdec)
		{
			MsDb2Parameter parm = new MsDb2Parameter();
			parm.MsDb2Type = GXTypeToMsDb2Type(dbtype);

			parm.Size = gxlength;
			parm.Scale = (byte)gxdec;
			parm.Precision = (byte)gxlength;
			parm.ParameterName = name;
			return parm;
		}
		private MsDb2Type GXTypeToMsDb2Type(object type)
		{
			if (type is MsDb2Type)
				return (MsDb2Type)type;

			switch (type)
			{
				case GXType.Int16: return MsDb2Type.SmallInt;
				case GXType.Int32: return MsDb2Type.Int;
				case GXType.Int64: return MsDb2Type.BigInt;
				case GXType.Number: return MsDb2Type.Double;
				case GXType.DateTime2:
				case GXType.DateTime: return MsDb2Type.Timestamp;
				case GXType.Date: return MsDb2Type.Date;
				case GXType.UniqueIdentifier:
				case GXType.DateAsChar:
				case GXType.Char: return MsDb2Type.Char;
				case GXType.VarChar: return MsDb2Type.VarChar;
				case GXType.Blob: return MsDb2Type.VarBinary;
				default: return MsDb2Type.Char;
			}
		}
		public override DbDataAdapter CreateDataAdapeter()
		{
			return new MsDb2DataAdapter();
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
			GXLogging.Debug(log, "ExecuteReader: client cursor=" + hasNested + ", handle '" + handle + "'" + ", hashcode " + this.GetHashCode());
			idatareader = new GxDataReader(connManager, this, con, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, dynStmt);
			return idatareader;
		}
		public override void SetParameter(IDbDataParameter parameter, Object value)
		{
			if (value != null)
			{
				parameter.Value = CheckDataLength(value, parameter);
			}
			else
				parameter.Value = DBNull.Value;
		}
		public override string GetServerDateTimeStmt(IGxConnection connection)
		{
			return "SELECT CURRENT_TIMESTAMP FROM SYSIBM.SYSDUMMY1";
		}
		public override string GetServerDateTimeStmtMs(IGxConnection connection)
		{
			return GetServerDateTimeStmt(connection);
		}
		public override string GetServerUserIdStmt()
		{
			return null;
		}
		public override string GetServerVersionStmt()
		{
			throw new GxNotImplementedException();
		}
		public override bool ProcessError(int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)

		{

			GXLogging.Debug(log, "ProcessError: dbmsErrorCode=" + dbmsErrorCode + ", emsg '" + emsg + "'" + ", status " + status);
			switch (dbmsErrorCode)
			{
				case -911:      // Locked
					retry = Retry(errMask, retryCount);
					if (retry)
						status = 110;// Locked - Retry
					else
						status = 103;//Locked
					return retry;
				case -803:      // Duplicated record
					status = 1;
					break;
				case -519:      // File not found
				case -372:      // File not found
				case -204:      // File not found
					status = 105;
					break;
				case -530:      // Parent key not found
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
				return "'0001-01-01'";
			return "TIMESTAMP('" +
				Value.Year.ToString() + "-" +
				Value.Month.ToString() + "-" +
				Value.Day.ToString() + "-" +
				Value.Hour.ToString() + "." +
				Value.Minute.ToString() + "." +
				Value.Second.ToString() + "')";
		}

		public override DateTime Dbms2NetDate(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			GXLogging.Debug(log, "GetDateTime - index : " + i);
			DateTime value;
			if (m_UseCharInDate)
			{
				string valueString = base.GetString(cmd, DR, i);
				if (valueString == SQL_NULL_DATE || valueString.Trim().Length == 0)
					value = DateTimeUtil.NullDate();
				else
					DateTime.TryParseExact(valueString, "yyyyMMdd", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out value);

				GXLogging.Debug(log, "GetDateTime as Char - value1 : " + value + " string " + valueString);
			}
			else
			{
				value = DR.GetDateTime(i);
				GXLogging.Debug(log, "GetDateTime - value2 : " + value);
			}
			return value;
		}

		public override Object Net2DbmsDateTime(IDbDataParameter parm, DateTime dateValue)
		{

			MsDb2Parameter idb2parameter = (MsDb2Parameter)parm;
			if (m_UseCharInDate && idb2parameter.MsDb2Type == MsDb2Type.Char && idb2parameter.Size == 8)
			{
				string resString;
				if (dateValue.Equals(DateTimeUtil.NullDate()))
				{
					resString = SQL_NULL_DATE;
				}
				else
				{
					resString = DateTimeUtil.getYYYYMMDD(dateValue);
				}
				return resString;
			}
			else
			{
				return dateValue;
			}
		}
		public override void GetValues(IDataReader reader, ref object[] values)
		{
			if (values != null)
			{
				for (int i = 0; i < values.Length; i++)
				{   /* The iDB2Decimal value is too large to fit into a Decimal. Use ToString() to retrieve the value instead */
					if (!reader.IsDBNull(i) && reader.GetFieldType(i) == typeof(Decimal))
						values[i] = reader.GetDecimal(i);
					else
						values[i] = reader.GetValue(i);
				}
			}
		}
		internal override object CloneParameter(IDbDataParameter p)
		{
			return ((ICloneable)p).Clone();
		}

		protected override IDbCommand GetCachedCommand(IGxConnection con, string stmt)
		{
			return con.ConnectionCache.GetAvailablePreparedCommand(stmt);
		}

		public override IDbCommand GetCommand(IGxConnection con, string stmt, GxParameterCollection parameters)
		{
			IDbCommand cmd = GetCachedCommand(con, stmt);

			if (cmd == null)
			{
				cmd = con.InternalConnection.CreateCommand();
				cmd.CommandText = stmt;
				cmd.Connection = con.InternalConnection.InternalConnection;

				for (int j = 0; j < parameters.Count; j++)
				{
					cmd.Parameters.Add(CloneParameter(parameters[j]));
				}
				cmd.Transaction = con.BeginTransaction();

				con.ConnectionCache.AddPreparedCommand(stmt, cmd);
			}
			else
			{
				if (parameters.Count > 0)
				{
					for (int j = 0; j < parameters.Count; j++)
					{
						((IDbDataParameter)cmd.Parameters[j]).Value = parameters[j].Value;

					}
				}
				cmd.Connection = con.InternalConnection.InternalConnection;
				cmd.Transaction = con.BeginTransaction();
			}
			return cmd;
		}

	}

#endif
}