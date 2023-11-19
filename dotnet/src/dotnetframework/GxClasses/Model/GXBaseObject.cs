using GeneXus.Configuration;
using GeneXus.Data.NTier;
using GeneXus.Encryption;
using GeneXus.Http;
using GeneXus.Mock;
#if NETCORE
using GeneXus.Services.OpenTelemetry;
#endif
using GeneXus.Utils;
using Jayrock.Json;
#if NETCORE
using Microsoft.AspNetCore.Http.Extensions;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace GeneXus.Application
{
	public class GxObjectParameter
	{
		public ParameterInfo ParmInfo { get; set; }
		public string ParmName { get; set; }
	}
	public class GXBaseObject
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<GXBaseObject>();

#if NETCORE
		internal static ActivitySource activitySource;
#endif
		private Dictionary<string, string> callTargetsByObject = new Dictionary<string, string>();
		protected IGxContext _Context;
		bool _isMain;
		protected bool _isApi;
#if NETCORE
		internal static ActivitySource ActivitySource {
			get {
				if (activitySource == null)
					activitySource = new(OpenTelemetryService.GX_ACTIVITY_SOURCE_NAME);
				return activitySource;
			}
		}
#endif
		protected virtual bool GenOtelSpanEnabled() { return false; }
		
		protected virtual void ExecuteEx()
		{
			if (GxMockProvider.Provider != null)
			{
				List<GxObjectParameter> parmInfo = GetExecuteParameterMap();
				if (GxMockProvider.Provider.Handle(_Context, this, parmInfo))
					return;
			}
			ExecuteImpl();
		}
		protected virtual void ExecutePrivate()
		{
			
		}
#if NETCORE
		private void ExecuteUsingSpanCode()
		{
			Config.GetValueOf("AppMainNamespace", out string mainNamespace);
			string gxObjFullName = GetType().FullName;
			if (gxObjFullName.StartsWith(mainNamespace))
				gxObjFullName = gxObjFullName.Remove(0, mainNamespace.Length);
			using (Activity activity = ActivitySource.StartActivity($"{gxObjFullName}.execute"))
			{
				ExecutePrivate();
			}
		}
#endif
		protected virtual void ExecuteImpl()
		{
#if NETCORE
			if (GenOtelSpanEnabled())
				ExecuteUsingSpanCode();
			else
				ExecutePrivate();
#else
			ExecutePrivate();
#endif
		}
		protected virtual void ExecutePrivateCatch(object stateInfo)
		{
			try
			{
				((GXBaseObject)stateInfo).ExecutePrivate();
			}
			catch (Exception e)
			{
				GXUtil.SaveToEventLog("Design", e);
				Console.WriteLine(e.ToString());
			}
		}
		protected void SubmitImpl()
		{
			GxContext submitContext = new GxContext();
			DataStoreUtil.LoadDataStores(submitContext);
			IsMain = true;
			submitContext.SetSubmitInitialConfig(context);
			this.context= submitContext;
			initialize();
			Submit(ExecutePrivateCatch, this);
		}
		protected virtual void CloseCursors()
		{

		}
		public virtual void initialize() { throw new Exception("The method or operation is not implemented."); }
		protected void Submit(Action<object> executeMethod, object state)
		{
			ThreadUtil.Submit(PropagateCulture(new WaitCallback(executeMethod)), state);
		}
		public static WaitCallback PropagateCulture(WaitCallback action)
		{
			var currentCulture = Thread.CurrentThread.CurrentCulture;
			GXLogging.Debug(log, "Submit PropagateCulture " + currentCulture);
			var currentUiCulture = Thread.CurrentThread.CurrentUICulture;
			return (x) =>
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
				Thread.CurrentThread.CurrentUICulture = currentUiCulture;
				action(x);
			};
		}


		private List<GxObjectParameter> GetExecuteParameterMap()
		{
			ParameterInfo[] pars = GetType().GetMethod("execute").GetParameters();
			string[] parms = GetParameters();
			int idx = 0;
			List<GxObjectParameter> parmInfo = new List<GxObjectParameter>();
			if (pars != null && parms!=null && pars.Length == parms.Length)
			{
				foreach (ParameterInfo par in pars)
				{
					parmInfo.Add(new GxObjectParameter()
					{
						ParmInfo = par,
						ParmName = parms[idx]
					});
					idx++;
				}
			}
			return parmInfo;
		}
		protected virtual string[] GetParameters()
		{
			return null;
		}

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
		internal string ApiExecutePermissionPrefix2(string gxMethod) { return ApiExecutePermissionPrefix(gxMethod); }		
		internal GAMSecurityLevel ApiIntegratedSecurityLevel2(string gxMethod) { return ApiIntegratedSecurityLevel(gxMethod); }
		public virtual string ServiceExecutePermissionPrefix { get { return string.Empty; } }
		public virtual string ServiceDeletePermissionPrefix { get { return string.Empty; } }
		public virtual string ServiceInsertPermissionPrefix { get { return string.Empty; } }
		public virtual string ServiceUpdatePermissionPrefix { get { return string.Empty; } }

		protected virtual bool IntegratedSecurityEnabled { get { return false; } }
		protected virtual GAMSecurityLevel IntegratedSecurityLevel { get { return 0; } }
		protected virtual bool IsSynchronizer { get { return false; } }
		protected virtual string ExecutePermissionPrefix { get { return String.Empty; } }

		protected virtual string ApiExecutePermissionPrefix(string gxMethod) { return ExecutePermissionPrefix2; }
		protected virtual GAMSecurityLevel ApiIntegratedSecurityLevel(string gxMethod) {  return IntegratedSecurityLevel2; }
		public virtual void handleException(String gxExceptionType, String gxExceptionDetails, String gxExceptionStack) { }

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
			return Encrypt64(value, key, false);
		}
		private string Encrypt64(string value, string key, bool safeEncoding)
		{
			string sRet = string.Empty;
			try
			{
				sRet = Crypto.Encrypt64(value, key, safeEncoding);
			}
			catch (InvalidKeyException)
			{
				context.SetCookie("GX_SESSION_ID", string.Empty, string.Empty, DateTime.MinValue, string.Empty, context.GetHttpSecure());
				GXLogging.Error(log, "440 Invalid encryption key");
				SendResponseStatus(440, "Session timeout");
			}
			return sRet;
		}
		protected virtual void SendResponseStatus(int statusCode, string statusDescription)
		{
		}

		protected string UriEncrypt64(string value, string key)
		{
			return Encrypt64(value, key, true);
		}

		protected string Decrypt64(string value, string key)
		{
			return Decrypt64(value, key, false);
		}
		private string Decrypt64(string value, string key, bool safeEncoding)
		{
			String sRet = string.Empty;
			try
			{
				sRet = Crypto.Decrypt64(value, key, safeEncoding);
			}
			catch (InvalidKeyException)
			{
				context.SetCookie("GX_SESSION_ID", string.Empty, string.Empty, DateTime.MinValue, string.Empty, context.GetHttpSecure());
				GXLogging.Error(log, "440 Invalid encryption key");
				SendResponseStatus(440, "Session timeout");
			}
			return sRet;
		}
		protected string UriDecrypt64(string value, string key)
		{
			return Decrypt64(value, key, true);
		}
	}
}
