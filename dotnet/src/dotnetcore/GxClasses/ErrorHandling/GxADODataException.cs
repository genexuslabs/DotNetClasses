using GeneXus.Metadata;
using MySql.Data.MySqlClient;
using Npgsql;
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
			else if (m_sErrorType == "MySql.Data.MySqlClient.MySqlException")
            {
                ParseMySqlException(ex);
            }
            else if (m_sErrorType == "Npgsql.NpgsqlException" || m_sErrorType == "Npgsql.PostgresException")
            {
                ParsePostgresException(ex);
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
		private void ParseOracleManagedException(Exception ex)
		{
			m_sErrorInfo = (String)ClassLoader.GetPropValue(ex, "Message");
			m_sDBMSErrorInfo = (String)ClassLoader.GetPropValue(ex, "Message");
			m_iErrorCode = (int)ClassLoader.GetPropValue(ex, "Number");
		}
		private void ParsePostgresException(Exception ex)
        {
			PostgresException pEx1 = ex as PostgresException;
			if (pEx1 != null)
			{
				m_sSqlState = pEx1.Code;
				m_sErrorInfo = pEx1.Message.ToLower();
				m_sDBMSErrorInfo = pEx1.Message.ToLower();
				byte[] buf = BitConverter.GetBytes(pEx1.HResult);
				byte[] errCode = new byte[2];
				Array.Copy(buf, 0, errCode, 0, 2);
				m_iErrorCode = BitConverter.ToInt16(errCode, 0);
			}
			NpgsqlException pEx = ex as NpgsqlException;
			if (pEx != null)
			{
				m_sErrorInfo = pEx.Message.ToLower();
				m_sDBMSErrorInfo = pEx.Message.ToLower();
				byte[] buf = BitConverter.GetBytes(pEx.HResult);
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
