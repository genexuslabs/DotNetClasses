using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Metadata;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
namespace GeneXus.Security
{
	public class GxResult
	{
		public string Code{get; set;}
		public string Description{get; set;}
	}
	public class OutData : Dictionary<string, object>
	{
		public String JsonString { get; set; }
	}
	public interface ISecurityProvider
	{
		GxResult checkaccesstoken(IGxContext context, String token, out bool isOK);
		GxResult checkaccesstokenprm(IGxContext context, String token, String permissionPrefix, out bool sessionOk, out bool permissionOk);
		void checksession(IGxContext context, string CleanAbsoluteUri, out bool isOK);
		void checksessionprm(IGxContext context, string pathAndQuery, String permissionPrefix, out bool isOK, out bool isPermissionOK);
		GxResult refreshtoken(IGxContext context, String clientId, String clientSecret, String refreshToken, out OutData outData, out bool flag);
		GxResult logindevice(IGxContext context, String clientId, String clientSecret, out OutData outData, out bool flag);
		GxResult externalauthenticationfromsdusingtoken(IGxContext context, String grantType, String nativeToken, String nativeVerifier, String clientId, String clientSecret, ref String scope, out OutData outData, out bool flag);
		GxResult externalauthenticationfromsdusingtoken(IGxContext context, String grantType, String nativeToken, String nativeVerifier, String clientId, String clientSecret, ref String scope, String additionalParameters, out OutData outData, out bool flag);
		GxResult oauthauthentication(IGxContext context, String grantType, String userName, String userPassword, String clientId, String clientSecret, String scope, out OutData outData, out String URL, out bool flag);
		GxResult oauthauthentication(IGxContext context, String grantType, String userName, String userPassword, String clientId, String clientSecret, String scope, String additionalParameters, out OutData outData, out String URL, out bool flag);
		void oauthgetuser(IGxContext context, out String userJson, out bool isOK);
		void oauthlogout(IGxContext context);

	}
	public class GxSecurityProvider 
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Security.GxSecurityProvider));

		private static volatile ISecurityProvider provider;
		private static object syncRoot = new Object();
		internal static string GAMSecurityProvider = "GeneXus.Security.GAMSecurityProvider";
		internal static string GAMSecurityProviderAssembly = "GeneXus.Security";

		public static ISecurityProvider Provider {
			get
			{
				if (provider == null)
				{
					string value;
					if (Config.GetValueOf("EnableIntegratedSecurity", out value) && value.Equals("1"))
					{
						try
						{
							
							lock (syncRoot)
							{
								if (provider == null)
								{
									provider = (ISecurityProvider)ClassLoader.GetInstance(GAMSecurityProviderAssembly, GAMSecurityProvider, null);
								}
							}
						}
						catch (Exception ex)
						{
							GXLogging.Debug(log, "Error loading Security provider " + GAMSecurityProvider, ex);
						}
					}
					else
					{
						provider = new NoSecurityProvider();
					}
					GXLogging.Debug(log, "Security provider: " + provider.GetType().FullName);
				}
				return provider;
			}
			set{
				provider = value;
			}
		}
	}

	public class NoSecurityProvider : ISecurityProvider
	{
		public GxResult checkaccesstoken(IGxContext context, string token, out bool isOK)
		{
			isOK = true;
			return new GxResult();
		}

		public GxResult checkaccesstokenprm(IGxContext context, string token, string permissionPrefix, out bool sessionOk, out bool permissionOk)
		{
			permissionOk = true;
			sessionOk = true;
			return new GxResult();
		}

		public void checksession(IGxContext context, string CleanAbsoluteUri, out bool isOK)
		{
			isOK = true;
		}

		public void checksessionprm(IGxContext context, string pathAndQuery, string permissionPrefix, out bool isOK, out bool isPermissionOK)
		{
			isOK = true;
			isPermissionOK = true;
		}

		public GxResult refreshtoken(IGxContext context, string clientId, string clientSecret, string refreshToken, out OutData outData, out bool flag)
		{
			flag = true;
			outData = new OutData();
			return new GxResult();
		}

		public GxResult logindevice(IGxContext context, string clientId, string clientSecret, out OutData outData, out bool flag)
		{
			flag = true;
			outData = new OutData();
			return new GxResult();
		}

		public GxResult externalauthenticationfromsdusingtoken(IGxContext context, string grantType, string nativeToken, string nativeVerifier, string clientId, string clientSecret, ref string scope, out OutData outData, out bool flag)
		{
			scope = string.Empty;
			flag = true;
			outData = new OutData();
			return new GxResult();
		}

		public GxResult externalauthenticationfromsdusingtoken(IGxContext context, string grantType, string nativeToken, string nativeVerifier, string clientId, string clientSecret, ref string scope, string additionalParameters, out OutData outData, out bool flag)
		{
			scope = string.Empty;
			flag = true;
			outData = new OutData();
			return new GxResult();
		}
		public GxResult oauthauthentication(IGxContext context, string grantType, string userName, string userPassword, string clientId, string clientSecret, string scope, out OutData outData, out string URL, out bool flag)
		{
			URL = string.Empty;
			flag = true;
			outData = new OutData();
			return new GxResult();
		}

		public GxResult oauthauthentication(IGxContext context, string grantType, string userName, string userPassword, string clientId, string clientSecret, string scope, string additionalParameters, out OutData outData, out string URL, out bool flag)
		{
			URL = string.Empty;
			flag = true;
			outData = new OutData();
			return new GxResult();
		}

		public void oauthgetuser(IGxContext context, out string userJson, out bool isOK)
		{
			userJson = string.Empty;
			isOK = true;
		}

		public void oauthlogout(IGxContext context)
		{
		}
	}

}