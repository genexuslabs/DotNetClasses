using System;
using GeneXus.Configuration;
using GeneXus.Encryption;
using GxClasses.Helpers;
using log4net;

namespace GeneXus.Services
{
	public class GXSessionServiceFactory
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GXSessionServiceFactory));
		static string REDIS = "REDIS";
		static string DATABASE = "DATABASE";
		
		public static ISessionService GetProvider()
		{
			ISessionService sessionService = null;
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
		}
		public string ConnectionString { get; }
		public string InstanceName { get; }
		public int SessionTimeout { get; }

		public string Schema => throw new NotImplementedException();

		public string TableName => throw new NotImplementedException();
	}
	public class GxDatabaseSession : ISessionService
	{
		internal static string SESSION_ADDRESS = "SESSION_PROVIDER_ADDRESS";
		internal static string SESSION_PASSWORD = "SESSION_PROVIDER_PASSWORD";
		internal static string SESSION_SCHEMA = "SESSION_PROVIDER_SCHEMA";
		internal static string SESSION_TABLE_NAME = "SESSION_PROVIDER_TABLE_NAME";
		internal static string SESSION_PROVIDER_SERVER = "SESSION_PROVIDER_SERVER";
		internal static string SESSION_PROVIDER_DATABASE = "SESSION_PROVIDER_DATABASE";
		internal static string SESSION_PROVIDER_USER = "SESSION_PROVIDER_USER";

		public GxDatabaseSession(GXService serviceProvider)
		{
			string password = serviceProvider.Properties.Get(SESSION_PASSWORD);
			if (!string.IsNullOrEmpty(password))
			{
				password = CryptoImpl.Decrypt(password);
			}
			string serverName = serviceProvider.Properties.Get(SESSION_PROVIDER_SERVER);
			string userName = serviceProvider.Properties.Get(SESSION_PROVIDER_USER);
			string database = serviceProvider.Properties.Get(SESSION_PROVIDER_DATABASE);
			string schema = serviceProvider.Properties.Get(SESSION_SCHEMA);
			string tableName = serviceProvider.Properties.Get(SESSION_TABLE_NAME);

			string sessionAddresCompatibility = serviceProvider.Properties.Get(GxDatabaseSession.SESSION_ADDRESS);
			if (!string.IsNullOrEmpty(sessionAddresCompatibility))
			{
				ConnectionString = sessionAddresCompatibility;
			}

			if (!string.IsNullOrEmpty(serverName))
			{
				ConnectionString += $"Data Source={serverName};";
			}
			if (!string.IsNullOrEmpty(database))
			{
				ConnectionString += $"Initial Catalog={database}";
			}
			if (!string.IsNullOrEmpty(password))
			{
				ConnectionString += $";password={password}";
			}
			if (!string.IsNullOrEmpty(userName))
			{
				ConnectionString += $";user={userName}";
			}
			Schema = schema;
			TableName = tableName;
			SessionTimeout = Preferences.SessionTimeout;
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
