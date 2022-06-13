using GeneXus.Diagnostics;
using GeneXus.Encryption;
using GeneXus.Http;
using GeneXus.Utils;
using Jayrock.Json;
using log4net;
#if NETCORE
using Microsoft.AspNetCore.Http.Extensions;
#endif
using System;
using System.Collections.Generic;

namespace GeneXus.Application
{

	public class GXBaseObject
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GXBaseObject));
		private Dictionary<string, string> callTargetsByObject = new Dictionary<string, string>();
		protected IGxContext _Context;
		public virtual IGxContext context
		{
			set { _Context = value; }
			get { return _Context; }
			
		}
		public bool IntegratedSecurityEnabled2 { get { return IntegratedSecurityEnabled; } }
		public GAMSecurityLevel IntegratedSecurityLevel2 { get { return IntegratedSecurityLevel; } }
		public bool IsSynchronizer2 { get { return IsSynchronizer; } }
		public string ExecutePermissionPrefix2 { get { return ExecutePermissionPrefix; } }

		protected virtual bool IntegratedSecurityEnabled { get { return false; } }
		protected virtual GAMSecurityLevel IntegratedSecurityLevel { get { return 0; } }
		[Obsolete("IntegratedSecurityPermissionName is deprecated, it is here for compatibility.Use ExecutePermissionPrefix instead", false)]
		protected virtual string IntegratedSecurityPermissionName { get { return ""; } }
		protected virtual bool IsSynchronizer { get { return false; } }
		[Obsolete("It is here for backward compatibility", false)]
		protected virtual string ExecutePermissionPrefix { get { return ""; } }

		public virtual void CallWebObject(string url)
		{
			string target = GetCallTargetFromUrl(url);
			if (String.IsNullOrEmpty(target))
			{
				context.wjLoc = url;
			}
			else
			{
				JObject cmdParms = new JObject();
				cmdParms.Put("url", url);
				cmdParms.Put("target", target);
				context.httpAjaxContext.appendAjaxCommand("calltarget", cmdParms);
			}
		}
		private string GetCallTargetFromUrl(string urlString)
		{
			Uri parsedUri;
			if (Uri.TryCreate(urlString, UriKind.RelativeOrAbsolute, out parsedUri))
			{
#if NETCORE
				if (parsedUri.IsAbsoluteUri || (!parsedUri.IsAbsoluteUri && Uri.TryCreate(new UriBuilder(context.HttpContext.Request.GetDisplayUrl()).Uri, urlString, out parsedUri)))
#else
				if (parsedUri.IsAbsoluteUri || (!parsedUri.IsAbsoluteUri && Uri.TryCreate(context.HttpContext.Request.Url, urlString, out parsedUri)))
#endif
				{
					string uriPath = parsedUri.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);
					int slashPos = uriPath.LastIndexOf('/');
					if (slashPos >= 0)
						uriPath = uriPath.Substring(slashPos + 1);
					string objClass = RemoveExtensionFromUrlPath(uriPath).ToLower();
					string target;
					if (callTargetsByObject.TryGetValue(objClass, out target) && ShouldLoadTarget(target))
						return target;
				}
			}
			return "";
		}

		private string RemoveExtensionFromUrlPath(string urlPath)
		{
			if (urlPath.EndsWith(".aspx"))
				return urlPath.Substring(0, urlPath.Length - 5);
			return urlPath;
		}
		private bool ShouldLoadTarget(string target)
		{
			return (target == "top" || target == "right" || target == "bottom" || target == "left");
		}
		public void SetCallTarget(string objClass, string target)
		{
			callTargetsByObject[objClass.ToLower().Replace("\\", ".")] = target.ToLower();
		}
		public string formatLink(string jumpURL)
		{
			return formatLink(jumpURL, Array.Empty<object>(), Array.Empty<string>());
		}
		protected string formatLink(string jumpURL, string[] parms, string[] parmsName)
		{
			return URLRouter.GetURLRoute(jumpURL, parms, parmsName, context.GetScriptPath());
		}
		protected string formatLink(string jumpURL, object[] parms, string[] parmsName)
		{
			return URLRouter.GetURLRoute(jumpURL, parms, parmsName, context.GetScriptPath());
		}
		public virtual string UrlEncode(string s)
		{
			return GXUtil.UrlEncode(s);
		}
		protected string GetEncryptedHash(string value, string key)
		{
			return Encrypt64(GXUtil.GetHash(GeneXus.Web.Security.WebSecurityHelper.StripInvalidChars(value), Cryptography.Constants.SecurityHashAlgorithm), key);
		}

		protected string Encrypt64(string value, string key)
		{
			string sRet = string.Empty;
			try
			{
				sRet = Crypto.Encrypt64(value, key);
			}
			catch (InvalidKeyException)
			{
				GXLogging.Error(log, "440 Invalid encryption key");
			}
			return sRet;
		}
		protected string UriEncrypt64(string value, string key)
		{
			return Encrypt64(value, key);
		}

		protected string Decrypt64(string value, string key)
		{
			String sRet = string.Empty;
			try
			{
				sRet = Crypto.Decrypt64(value, key);
			}
			catch (InvalidKeyException)
			{
				GXLogging.Error(log, "440 Invalid encryption key");
			}
			return sRet;
		}
		protected string UriDecrypt64(string value, string key)
		{
			return Decrypt64(value, key);
		}
	}
}
