using System;
using GeneXus.Application;
using GeneXus.Cache;
using GeneXus.Configuration;
using GeneXus.Data;
using GeneXus.Data.ADO;
using GeneXus.Encryption;
using GxClasses.Helpers;

namespace GeneXus.Services
{
	public class GXSessionServiceFactory
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<GXSessionServiceFactory>();

		static ISessionService sessionService;
		static string REDIS = "REDIS";
		static string DATABASE = "DATABASE";
		public static ISessionService GetProvider()
		{
			if (sessionService != null)
				return sessionService;
			GXService providerService = GXServices.Instance?.Get(GXServices.SESSION_SERVICE);
			if (providerService != null)
			{
				try
				{
					Type type = null;
					string className = providerService.ClassName;
					//Compatibility 
					if (string.IsNullOrEmpty(className))
					{
						if (providerService.Name.Equals(REDIS, StringComparison.OrdinalIgnoreCase))
							type = typeof(GxRedisSession);
						else if (providerService.Name.Equals(DATABASE, StringComparison.OrdinalIgnoreCase))
							type = typeof(GxDatabaseSession);
					}
					else
					{
						GXLogging.Debug(log, "Loading Session provider:", className);
#if !NETCORE
						type = Type.GetType(className, true, true);
#else
						type = AssemblyLoader.GetType(className);
#endif
					}
					if (type != null)
					{
						sessionService = (ISessionService)Activator.CreateInstance(type, new object[] { providerService });
						return sessionService;
					}
				}
				catch (Exception e)
				{
					GXLogging.Error(log, "Couldn´t create Session provider.", e.Message, e);
					throw e;
				}
			}
			return null;
		}
	}
	public class GxRedisSession : ISessionService
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<GxRedisSession>();
		internal static string SESSION_ADDRESS = "SESSION_PROVIDER_ADDRESS";
		internal static string SESSION_INSTANCE = "SESSION_PROVIDER_INSTANCE_NAME";
		internal static string SESSION_PASSWORD = "SESSION_PROVIDER_PASSWORD";
		static string SESSION_TIMEOUT = "SESSION_PROVIDER_SESSION_TIMEOUT";
		public GxRedisSession(GXService serviceProvider)
		{
			string password = serviceProvider.Properties.Get(SESSION_PASSWORD);
			if (!string.IsNullOrEmpty(password))
			{
				password = CryptoImpl.Decrypt(password);
			}
			string host = serviceProvider.Properties.Get(SESSION_ADDRESS);
			string instanceName = serviceProvider.Properties.Get(SESSION_INSTANCE);
			ConnectionString = $"{host}";
			if (!string.IsNullOrEmpty(password))
			{
				ConnectionString += $",password={password}";
			}
			InstanceName = instanceName;

			int sessionTimeoutMinutes = Preferences.SessionTimeout;
			string sessionTimeoutStrCompatibility = serviceProvider.Properties.Get(SESSION_TIMEOUT);
			if (!string.IsNullOrEmpty(sessionTimeoutStrCompatibility))
				int.TryParse(sessionTimeoutStrCompatibility, out sessionTimeoutMinutes);

			SessionTimeout = sessionTimeoutMinutes;
			GXLogging.Debug(log, "Redis Host:", host, ", InstanceName:", instanceName);
			GXLogging.Debug(log, "Redis sessionTimeoutMinutes:", sessionTimeoutMinutes.ToString());
		}
		public GxRedisSession(string host, string password, string instanceName, int sessionTimeout)
		{
			ConnectionString = $"{host}";
			if (!string.IsNullOrEmpty(password))
			{
				ConnectionString += $",password={password}";
			}
			InstanceName = instanceName;
			SessionTimeout = sessionTimeout;
			GXLogging.Debug(log, "Redis Host:", host, ", InstanceName:", instanceName);
			GXLogging.Debug(log, "Redis sessionTimeoutMinutes:", sessionTimeout.ToString());
		}
		internal bool IsMultitenant
		{
			get { return InstanceName == CacheFactory.SUBDOMAIN; }
		}
		public string ConnectionString { get; }
		public string InstanceName { get; }
		public int SessionTimeout { get; }
		public string Schema => throw new NotImplementedException();
		public string TableName => throw new NotImplementedException();
	}
	public class GxDatabaseSession : ISessionService
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<GxDatabaseSession>();
		internal static string SESSION_ADDRESS = "SESSION_PROVIDER_ADDRESS";
		internal static string SESSION_PASSWORD = "SESSION_PROVIDER_PASSWORD";
		internal static string SESSION_SCHEMA = "SESSION_PROVIDER_SCHEMA";
		internal static string SESSION_TABLE_NAME = "SESSION_PROVIDER_TABLE_NAME";
		internal static string SESSION_DATASTORE = "SESSION_PROVIDER_DATASTORE";
		const string DEFAULT_SQLSERVER_SCHEMA = "dbo";
		public GxDatabaseSession(GXService serviceProvider)
		{
			string datastoreName = serviceProvider.Properties.Get(SESSION_DATASTORE);
			if (!string.IsNullOrEmpty(datastoreName))
			{
				GxContext context = GxContext.CreateDefaultInstance();
				IGxDataStore datastore = context.GetDataStore(datastoreName);
				string schema = datastore.Connection.CurrentSchema;
				if (string.IsNullOrEmpty(schema))
					schema = DEFAULT_SQLSERVER_SCHEMA;
				string tableName = serviceProvider.Properties.Get(SESSION_TABLE_NAME);
				GxConnection conn = datastore.Connection as GxConnection;
				Schema = schema;
				TableName = tableName;
				context.CloseConnections();
				GxDataRecord dr = datastore.Db as GxDataRecord;
				if (dr != null && conn != null)
				{
					ConnectionString = dr.BuildConnectionStringImpl(conn.DataSourceName, conn.InternalUserId, conn.UserPassword, conn.DatabaseName, conn.Port, conn.CurrentSchema, conn.Data);
					GXLogging.Debug(log, "Database ConnectionString:", dr.ConnectionStringForLog());
				}
			}
			else //Backward compatibility configuration
			{
				string password = serviceProvider.Properties.Get(SESSION_PASSWORD);
				if (!string.IsNullOrEmpty(password))
				{
					password = CryptoImpl.Decrypt(password);
				}
				string schema = serviceProvider.Properties.Get(SESSION_SCHEMA);
				string tableName = serviceProvider.Properties.Get(SESSION_TABLE_NAME);
				string sessionAddresCompatibility = serviceProvider.Properties.Get(SESSION_ADDRESS);
				if (!string.IsNullOrEmpty(sessionAddresCompatibility))
				{
					ConnectionString = sessionAddresCompatibility;
				}
				if (!string.IsNullOrEmpty(password))
				{
					ConnectionString += $";password={password}";
				}
				Schema = schema;
				TableName = tableName;
			}
			SessionTimeout = Preferences.SessionTimeout;
			GXLogging.Debug(log, "Database sessionTimeoutMinutes:", SessionTimeout.ToString());
		}
		public GxDatabaseSession(string host, string password, string schema, string tableName)
		{
			ConnectionString = $"{host}";
			if (!string.IsNullOrEmpty(password))
			{
				ConnectionString += $";password={password}";
			}
			Schema = schema;
			TableName = tableName;
		}
		public string ConnectionString { get; }
		public string Schema { get; }
		public string TableName { get; }
		public string InstanceName => throw new NotImplementedException();
		public int SessionTimeout { get; }
	}
	public interface ISessionService
	{
		string ConnectionString { get; }
		string Schema { get; }
		string TableName { get; }
		string InstanceName { get; }
		int SessionTimeout { get; }
	}
}
