using System;
using System.Collections;
#if !NETCORE
using System.Data.SqlClient;
using System.Data.SQLite;
using Microsoft.HostIntegration.MsDb2Client;
using MySQLDriverCS;
using MSOracleProvider = System.Data.OracleClient;
using System.Reflection;
#else
using Microsoft.Data.SqlClient;
#endif
using MySqlConnector;
using System.Runtime.Serialization;
using System.Security;
using GeneXus.Metadata;

namespace GeneXus.Data
{

	[Serializable()]      
	public class GxADODataException : Exception
	{
		public GxADODataException(SerializationInfo info,StreamingContext ctx):base(info,ctx)
		{
			
		}
		public GxADODataException(string mes, Exception ex) : base(mes, ex)
		{
			if (ex != null)
			{
				m_sErrorType = ex.GetType().ToString();
			}
			m_sErrorInfo = mes; 
		}
		public GxADODataException(string mes) : base(mes)
		{
			m_sErrorInfo = mes; 
		}

		public GxADODataException(ushort iConnId): base("GeneXus ADO Data Exception")
		{
		}

		public GxADODataException(Exception ex)	: base("GeneXus ADO Data Exception", ex)
		{
			m_sErrorType = ex.GetType().ToString();
#if NETCORE
			if (m_sErrorType == "Microsoft.Data.SqlClient.SqlException")
			{
				SqlException sqlEx = (SqlException)ex;
				m_sErrorInfo = sqlEx.Message;
				m_sDBMSErrorInfo = sqlEx.Message;
				m_iErrorCode = sqlEx.Number;
			}
#else
			if (m_sErrorType == "System.Data.SqlClient.SqlException")
			{
				SqlException sqlEx = (SqlException)ex;
				m_sErrorInfo = sqlEx.Message;
				m_sDBMSErrorInfo = sqlEx.Message;
				m_iErrorCode = sqlEx.Number;
				
				if (sqlEx.Number == 0 && sqlEx.ErrorCode != 0)
				{
					byte[] buf = BitConverter.GetBytes(sqlEx.ErrorCode);
					byte[] errCode = new byte[2];
					Array.Copy(buf, 0, errCode, 0, 2);
					int code = BitConverter.ToInt16(errCode, 0);
					m_iErrorCode = code;
				}
			}
			else if (m_sErrorType == "System.Data.OracleClient.OracleException")
			{
				MSOracleProvider.OracleException orclEx = (MSOracleProvider.OracleException)ex;
				m_sErrorInfo = orclEx.Message;
				m_sDBMSErrorInfo = orclEx.Message;
				m_iErrorCode = orclEx.Code;
			}
			else if (m_sErrorType == "Oracle.DataAccess.Client.OracleException")
			{
				ParseOracleException(ex);//It prevents the dll from loading in runtime when it is not necessary
			}
#endif
			else if (m_sErrorType == "Oracle.ManagedDataAccess.Client.OracleException")
			{
				ParseOracleManagedException(ex);
			}
#if !NETCORE
			else if (m_sErrorType == "System.Data.SQLite.SQLiteException")
			{
				ParseSQLiteException(ex);
			}
#endif
			else if (m_sErrorType == $"{GxDb2.Db2AssemblyName}.DB2Exception")
			{
				ParseDB2Exception(ex);
			}
			else if (m_sErrorType == $"{GxInformix.InformixAssemblyName}.IfxException")
			{
				ParseIfxException(ex);
			}
			else if (m_sErrorType == "Npgsql.NpgsqlException" || m_sErrorType == "Npgsql.PostgresException")
			{
				ParsePostgresException(ex, m_sErrorType);
			}
#if !NETCORE
			else if (m_sErrorType.StartsWith("IBM.Data.DB2.iSeries"))

			{
				ParseIDB2Exception(ex);
			}
			else if (m_sErrorType.StartsWith("Microsoft.HostIntegration.MsDb2Client.MsDb2Exception"))
				
			{
				ParseIDB2HISException(ex);
			}
			else if (m_sErrorType == "MySQLDriverCS.MySQLException")
			{
				ParseMySqlException(ex);
			}
#endif
			else if (m_sErrorType == "MySqlConnector.MySqlException")
			{
				ParseMySqlConnectorException(ex);
			}
			else if (m_sErrorType == "Sap.Data.Hana.HanaException")
            {
                ParseHanaException(ex); 
            }
#if !NETCORE
			else if (m_sErrorType ==  "GeneXus.Data.DynService.Fabric.FabricException")
			{
				ParseFabricException(ex); 
			}
#endif
			else if (ex.GetType()==typeof(GxADODataException))
			{
				GxADODataException adoex = (GxADODataException)ex;
				m_iErrorCode = adoex.DBMSErrorCode;
				this.m_sDBMSErrorInfo = adoex.DBMSErrorInfo;
				this.m_sErrorInfo = adoex.ErrorInfo;
			}
			else
			{
				m_sErrorInfo = ex.Message;
			}
		}
#if !NETCORE
		private void ParseSQLiteException(Exception ex)
		{
			SQLiteException sqlEx = (SQLiteException)ex;
			m_sErrorInfo = sqlEx.Message;
			m_sDBMSErrorInfo = sqlEx.Message;
			m_iErrorCode = sqlEx.ErrorCode;
		}
#endif
		private void ParseOracleException(Exception ex)
		{			
			m_sErrorInfo = (String)ClassLoader.GetPropValue(ex, "Message");
			m_sDBMSErrorInfo = (String)ClassLoader.GetPropValue(ex, "Message");
			m_iErrorCode = (int)ClassLoader.GetPropValue(ex, "Number");
		}
		private void ParseOracleManagedException(Exception ex)
		{
			m_sErrorInfo = (String)ClassLoader.GetPropValue(ex, "Message");
			m_sDBMSErrorInfo = (String)ClassLoader.GetPropValue(ex, "Message");
			m_iErrorCode = (int)ClassLoader.GetPropValue(ex, "Number");
		}
#if !NETCORE
		private void ParseIDB2HISException(Exception ex)
		{
			MsDb2Exception idb2Ex = (MsDb2Exception)ex;
			m_sErrorInfo = idb2Ex.Message;
			m_sDBMSErrorInfo = idb2Ex.Message;
			m_iErrorCode = idb2Ex.SqlCode;
		}
		private void ParseIDB2Exception(Exception ex)
		{
            m_sErrorInfo = (string)ClassLoader.GetPropValue(ex, "Message");
            m_sDBMSErrorInfo = m_sErrorInfo;
            m_iErrorCode = (int)ClassLoader.GetPropValue(ex, "MessageCode");
			string details = (string)ClassLoader.GetPropValue(ex, "MessageDetails");
			if (!string.IsNullOrEmpty(details))
				m_sErrorInfo += " " + details;
		}
#endif
		private void ParseIfxException(Exception ex)
		{
			m_sErrorInfo = (String)ClassLoader.GetPropValue(ex, "Message");
			m_sDBMSErrorInfo = (String)ClassLoader.GetPropValue(ex, "Message");
			ICollection errors = (ICollection) ClassLoader.GetPropValue(ex, "Errors");
			if (errors != null && errors.Count > 0)
			{
				IEnumerator enumErrors =  errors.GetEnumerator();
				if (enumErrors.MoveNext())
				{
					object ifxError = enumErrors.Current;
					m_iErrorCode = (int)ClassLoader.GetPropValue(ifxError, "NativeError");
					m_sSqlState = (String)ClassLoader.GetPropValue(ifxError, "SQLState");
				}
			}
		}
		private void ParsePostgresException(Exception ex, String errorType)
		{
			if (errorType == "Npgsql.PostgresException")
			{
#if NETCORE
				m_sSqlState = (String)ClassLoader.GetPropValue(ex, "Code");
				m_sErrorInfo = ex.Message.ToLower();
				m_sDBMSErrorInfo = ex.Message.ToLower();
				byte[] buf = BitConverter.GetBytes(ex.HResult);
				byte[] errCode = new byte[2];
				Array.Copy(buf, 0, errCode, 0, 2);
				m_iErrorCode = BitConverter.ToInt16(errCode, 0);
#else
				m_sErrorInfo = ex.Message.ToLower();
				m_sDBMSErrorInfo = ex.Message;
				m_sSqlState = (String)ClassLoader.GetPropValue(ex, "Code");
				PropertyInfo prop = ex.GetType().GetProperty("HResult", BindingFlags.NonPublic | BindingFlags.Instance);
				if (prop == null)
				{
					prop = ex.GetType().GetProperty("HResult");
				}
				if (prop == null)
				{
					Int32.TryParse(m_sSqlState, out m_iErrorCode);
				}
				else
				{
					byte[] buf = BitConverter.GetBytes((int)prop.GetValue(ex, null));
					byte[] errCode = new byte[2];
					Array.Copy(buf, 0, errCode, 0, 2);
					int code = BitConverter.ToInt16(errCode, 0);
					m_iErrorCode = code;
				}
#endif
			}
			else
			{
				m_sErrorInfo = ex.Message.ToLower();
				m_sDBMSErrorInfo = ex.Message.ToLower();
				byte[] buf = BitConverter.GetBytes(ex.HResult);
				byte[] errCode = new byte[2];
				Array.Copy(buf, 0, errCode, 0, 2);
				m_iErrorCode = BitConverter.ToInt16(errCode, 0);
			}
		}
		private void ParseDB2Exception(Exception ex)
		{
            m_sErrorInfo = (string)ClassLoader.GetPropValue(ex, "Message");
            m_sDBMSErrorInfo = m_sErrorInfo;
            ICollection DB2Errors = (ICollection)ClassLoader.GetPropValue(ex, "Errors");
            if (DB2Errors != null && DB2Errors.Count > 0)
            {
                IEnumerator errors =  DB2Errors.GetEnumerator();
                if (errors != null && errors.MoveNext() && errors.Current!=null)
                {
                    m_iErrorCode = (int)ClassLoader.GetPropValue(errors.Current, "NativeError");
                }
            }
		}
#if !NETCORE
		private void ParseMySqlException(Exception ex)
		{
			MySQLException mysqlEx = (MySQLException)ex;
			m_sErrorInfo = mysqlEx.Message;
			m_sDBMSErrorInfo = mysqlEx.Message;
			m_iErrorCode = (int)mysqlEx.Number;
			//m_sSqlState = mysqlEx.SqlState;
		}
#endif
		[SecuritySafeCritical]
		private void ParseMySqlConnectorException(Exception ex)
		{
			MySqlException mysqlEx = (MySqlException)ex;
#if NETCORE
			if (mysqlEx.Number == 0 && ex.InnerException != null && ex.InnerException is MySqlException)
			{
				mysqlEx = ex.InnerException as MySqlException;
			}
			m_sSqlState = mysqlEx.SqlState;
#endif
			m_sErrorInfo = mysqlEx.Message;
			m_sDBMSErrorInfo = mysqlEx.Message;
			m_iErrorCode = (int)mysqlEx.Number;
		}

		private void ParseHanaException(Exception ex)
        {
            m_sErrorInfo = (string)ClassLoader.GetPropValue(ex, "Message");
            m_sDBMSErrorInfo = m_sErrorInfo;
            m_iErrorCode = (int) ClassLoader.GetPropValue(ex, "NativeError");
        }

		private void ParseFabricException(Exception ex)
		{
			m_sErrorInfo = (String)ClassLoader.GetPropValue(ex, "ErrorMsg");
			m_sDBMSErrorInfo = (String)ClassLoader.GetPropValue(ex, "ErrorMsg");
			m_iErrorCode = (int)ClassLoader.GetPropValue(ex, "ErrorCode");
		}
		public override string Message
		{
			get	{return m_sErrorInfo;}
		}
		public override string ToString()
		{
			string value = string.Empty;
			if (!string.IsNullOrEmpty(ErrorType))
				value = "Type:" + ErrorType + ".";
			if (DBMSErrorCode != 0)
				value += "DBMS Error Code:" + DBMSErrorCode + ".";
			return value + Message;
		}
		public void SetBaseDescription(ushort iConnId)
		{
		}
		public int DBMSErrorCode
		{
			get { return m_iErrorCode; }
		}
		public string ErrorInfo
		{
			get { return m_sErrorInfo; }
		}
		public string DBMSErrorInfo
		{
			get { return m_sDBMSErrorInfo; }
		}
		public string ErrorType
		{
			get { return m_sErrorType; }
		}
		public string SqlState
		{
			get { return m_sSqlState; }
		}

		private int m_iErrorCode;
		private string m_sErrorInfo;
		private string m_sSqlState;
		private string m_sDBMSErrorInfo;
		private string m_sErrorType;
	}


}