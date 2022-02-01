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
		bool _isMain;
		protected bool _isApi;

		public virtual IGxContext context
		{
			set { _Context = value; }
			get { return _Context; }
			
		}
		public bool IsMain
		{
			set { _isMain = value; }
			get { return _isMain; }
		}
		public bool IsApiObject
		{
			set { _isApi = value; }
			get { return _isApi; }
		}
		public virtual void cleanup() { }

		virtual public bool UploadEnabled() { return false; }
		public bool IntegratedSecurityEnabled2 { get { return IntegratedSecurityEnabled; } }
		public GAMSecurityLevel IntegratedSecurityLevel2 { get { return IntegratedSecurityLevel; } }
		public bool IsSynchronizer2 { get { return IsSynchronizer; } }
		public string ExecutePermissionPrefix2 { get { return ExecutePermissionPrefix; } }

		public virtual string ServiceExecutePermissionPrefix { get { return string.Empty; } }
		public virtual string ServiceDeletePermissionPrefix { get { return string.Empty; } }
		public virtual string ServiceInsertPermissionPrefix { get { return string.Empty; } }
		public virtual string ServiceUpdatePermissionPrefix { get { return string.Empty; } }

		protected virtual bool IntegratedSecurityEnabled { get { return false; } }
		protected virtual GAMSecurityLevel IntegratedSecurityLevel { get { return 0; } }
		protected virtual bool IsSynchronizer { get { return false; } }
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

	}

}
