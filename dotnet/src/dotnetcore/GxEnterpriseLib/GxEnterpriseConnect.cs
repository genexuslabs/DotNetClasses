using System;
using GeneXus.Utils;
using GeneXus.Application;
using SapNwRfc;

namespace  GeneXus.SAP
{
	public class GxEnterpriseConnect { 
		public GxEnterpriseConnect(GXECSessionManager manager)
		{
			if (manager != null)
			{
			   this.SessionManager = manager;
			}
		}
		public GxEnterpriseConnect(IGxContext context)
		{
			if (context!=null)
			{
				_sessionManager = new GXECSessionManager(context);
				_sessionManager.Connect();
				_connection = _sessionManager.GetCurrentConnection();
			}
		}
		public ISapFunction CreateFunction(string functionName)
		{
			if (_connection != null)
				return _connection.CreateFunction(functionName);
			else
				return null;
		}

		ISapConnection _connection = null;
		private GXECSessionManager _sessionManager = null;
		public string ConnectionString { get => connectionString; set => connectionString = value; }
		public GXECSessionManager SessionManager { get => _sessionManager; set => _sessionManager = value; }
		public ISapConnection Connection { get => _connection; set => _connection = value; }

		private string connectionString;
	}
}
