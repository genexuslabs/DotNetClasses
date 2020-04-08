using GeneXus.Http;
using GeneXus.Utils;
using Jayrock.Json;
#if NETCORE
using Microsoft.AspNetCore.Http.Extensions;
#endif
using System;
using System.Collections.Generic;

namespace GeneXus.Application
{

	public class GXBaseObject
	{
		private Dictionary<string, string> callTargetsByObject = new Dictionary<string, string>();
		protected IGxContext _Context;
		public virtual IGxContext context
		{
			set { _Context = value; }
			get { return _Context; }
			
		}
		public bool IntegratedSecurityEnabled2 { get { return IntegratedSecurityEnabled; } }
		public GAMSecurityLevel IntegratedSecurityLevel2 { get { return IntegratedSecurityLevel; } }
		public string IntegratedSecurityPermissionName2 { get { return IntegratedSecurityPermissionName; } }
		public bool IsSynchronizer2 { get { return IsSynchronizer; } }
		public string ExecutePermissionPrefix2 { get { return ExecutePermissionPrefix; } }

		protected virtual bool IntegratedSecurityEnabled { get { return false; } }
		protected virtual GAMSecurityLevel IntegratedSecurityLevel { get { return 0; } }
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
			return jumpURL.Trim();
		}
		public virtual string UrlEncode(string s)
		{
			return GXUtil.UrlEncode(s);
		}

	}

}
