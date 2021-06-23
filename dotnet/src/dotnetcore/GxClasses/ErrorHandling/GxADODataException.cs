using GeneXus.Metadata;
using MySqlConnector;
using System;
using Microsoft.Data.SqlClient;
using System.Runtime.Serialization;

namespace GeneXus.Data
{
	[Serializable()]
	public class GxADODataException : Exception
	{
		public GxADODataException(SerializationInfo info, StreamingContext ctx) : base()
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

		public GxADODataException(ushort iConnId) : base("GeneXus ADO Data Exception")
		{
		}

		public GxADODataException(Exception ex) : base("GeneXus ADO Data Exception", ex)
		{
			m_sErrorType = ex.GetType().ToString();
			if (m_sErrorType == "Microsoft.Data.SqlClient.SqlException")
			{
				SqlException sqlEx = (SqlException)ex;
				m_sErrorInfo = sqlEx.Message;
				m_sDBMSErrorInfo = sqlEx.Message;
				m_iErrorCode = sqlEx.Number;
			}
			else if (ex.GetType().ToString() == "Oracle.ManagedDataAccess.Client.OracleException")
			{
				ParseOracleManagedException(ex);
			}
			else if (m_sErrorType == "MySqlConnector.MySqlException")
            {
                ParseMySqlException(ex);
            }
            else if (m_sErrorType == "Npgsql.NpgsqlException" || m_sErrorType == "Npgsql.PostgresException")
            {
                ParsePostgresException(ex, m_sErrorType);
            }
			else if (m_sErrorType == "Sap.Data.Hana.HanaException")
			{
				ParseHanaException(ex);
			}
			else if (ex.GetType() == typeof(GxADODataException))
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
		private void ParseHanaException(Exception ex)
		{
			m_sErrorInfo = (string)ClassLoader.GetPropValue(ex, "Message");
			m_sDBMSErrorInfo = m_sErrorInfo;
			m_iErrorCode = (int)ClassLoader.GetPropValue(ex, "NativeError");
		}
		private void ParseOracleManagedException(Exception ex)
		{
			m_sErrorInfo = (String)ClassLoader.GetPropValue(ex, "Message");
			m_sDBMSErrorInfo = (String)ClassLoader.GetPropValue(ex, "Message");
			m_iErrorCode = (int)ClassLoader.GetPropValue(ex, "Number");
		}
		private void ParsePostgresException(Exception ex, String errorType)
		{
			if (errorType == "Npgsql.PostgresException")
			{
				m_sSqlState = (String)ClassLoader.GetPropValue(ex, "Code");
				m_sErrorInfo = ex.Message.ToLower();
				m_sDBMSErrorInfo = ex.Message.ToLower();
				byte[] buf = BitConverter.GetBytes(ex.HResult);
				byte[] errCode = new byte[2];
				Array.Copy(buf, 0, errCode, 0, 2);
				m_iErrorCode = BitConverter.ToInt16(errCode, 0);
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
        private void ParseMySqlException(Exception ex)
        {
			MySqlException mysqlEx = (MySqlException)ex;
			if (mysqlEx.Number == 0 && ex.InnerException != null && ex.InnerException is MySqlException)
			{
				mysqlEx = ex.InnerException as MySqlException;
			}
            m_sErrorInfo = mysqlEx.Message;
            m_sDBMSErrorInfo = mysqlEx.Message;
            m_iErrorCode = (int)mysqlEx.Number;
			m_sSqlState = mysqlEx.SqlState;
		}
        public override string Message
		{
			get { return m_sErrorInfo; }
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
		private string m_sDBMSErrorInfo;
		private string m_sErrorType;
		private string m_sSqlState;
	}

}
