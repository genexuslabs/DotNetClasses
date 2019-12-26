using GeneXus.Http;
using GeneXus.Utils;
using Jayrock.Json;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

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

		protected virtual bool IntegratedSecurityEnabled { get { return false; } }
		protected virtual GAMSecurityLevel IntegratedSecurityLevel { get { return 0; } }
		protected virtual string IntegratedSecurityPermissionName { get { return ""; } }

		public void CallWebObject(string url)
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
		private string GetCallTargetFromUrl(string url)
		{
			Uri parsedUri;
			if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out parsedUri))
			{
#if NETCORE
				if (parsedUri.IsAbsoluteUri || (!parsedUri.IsAbsoluteUri && Uri.TryCreate(new UriBuilder(context.HttpContext.Request.GetDisplayUrl()).Uri, url, out parsedUri)))
#else
				if (parsedUri.IsAbsoluteUri || (!parsedUri.IsAbsoluteUri && Uri.TryCreate(context.HttpContext.Request.Url, url, out parsedUri)))
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
