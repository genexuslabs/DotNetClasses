using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeneXus.Encryption;

namespace GeneXus.Services
{
	public class GXSessionServiceFactory
	{
		static string REDIS = "REDIS";
		static string DATABASE = "DATABASE";
		static string SESSION_ADDRESS = "SESSION_PROVIDER_ADDRESS";
		static string SESSION_INSTANCE = "SESSION_PROVIDER_INSTANCE_NAME";
		static string SESSION_PASSWORD = "SESSION_PROVIDER_PASSWORD";
		static string SESSION_SCHEMA = "SESSION_PROVIDER_SCHEMA";
		static string SESSION_TABLE_NAME = "SESSION_PROVIDER_TABLE_NAME";
		public static ISessionService GetProvider()
		{
			var instance = GXServices.Instance?.Get(GXServices.SESSION_SERVICE);
			if (instance != null)
			{
				if (instance.Name.Equals(REDIS, StringComparison.OrdinalIgnoreCase))
				{
					return new GxRedisSession(instance.Properties.Get(SESSION_ADDRESS), CryptoImpl.Decrypt(instance.Properties.Get(SESSION_PASSWORD)), instance.Properties.Get(SESSION_INSTANCE));
				}
				else if (instance.Name.Equals(DATABASE, StringComparison.OrdinalIgnoreCase))
				{
					return new GxDatabaseSession(instance.Properties.Get(SESSION_ADDRESS), CryptoImpl.Decrypt(instance.Properties.Get(SESSION_PASSWORD))
						,instance.Properties.Get(SESSION_SCHEMA), instance.Properties.Get(SESSION_TABLE_NAME));
				}
			}
			return null;
				
		}
	}
	public class GxRedisSession : ISessionService
	{
		public GxRedisSession(string host, string password, string instanceName)
		{
			ConnectionString = $"{host}";
			if (!string.IsNullOrEmpty(password))
			{
				ConnectionString += $",password={password}";
			}
			InstanceName = instanceName;
		}
		public string ConnectionString { get; }
		public string InstanceName { get; }

		public string Schema => throw new NotImplementedException();

		public string TableName => throw new NotImplementedException();
	}
	public class GxDatabaseSession : ISessionService
	{
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
	}

	public interface ISessionService
	{
		string ConnectionString { get; }
		string Schema { get; }
		string TableName { get; }
		string InstanceName { get; }
	}
}
