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
		static string ADDRESS = "SESSION_PROVIDER_ADDRESS";
		static string PASSWORD = "SESSION_PROVIDER_PASSWORD";
		public static ISessionService GetProvider()
		{
			var instance = GXServices.Instance.Get(GXServices.SESSION_SERVICE);
			if (instance != null && instance.Name.Equals(REDIS, StringComparison.OrdinalIgnoreCase))
			{
				return new RedisSession(instance.Properties.Get(ADDRESS), CryptoImpl.Decrypt(instance.Properties.Get(PASSWORD)));
			}
			return null;
				
		}
	}
	public class RedisSession : ISessionService
	{
		public RedisSession(string host, string password)
		{
			ConnectionString = $"{host}";
			if (!string.IsNullOrEmpty(password))
			{
				ConnectionString += $",password={password}";
			}
		}
		public string ConnectionString { get; }
	}
	public interface ISessionService
	{
		string ConnectionString { get; }
	}
}
