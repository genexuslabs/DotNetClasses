using System;
using System.Collections.Generic;
using System.Text;
#if NETCORE
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Text.Json;
using System.Text.Json.Serialization;
#else
using System.Web.SessionState;
#endif
using GeneXus.Utils;
using System.Net;
using GeneXus.Http;
using GeneXus.Procedure;
using GeneXus.Data;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using GeneXus.Metadata;
using GeneXus.Configuration;
using System.Web;
using System.Collections.Specialized;
using GeneXus.Security;
using System.Collections;
#if !NETCORE
using Jayrock.Json;
#endif
using System.Net.Http;
using System.Diagnostics;
using GeneXus.Diagnostics;
using System.Xml.Linq;


namespace GeneXus.Application

{

	internal static class Synchronizer
	{
		internal const string SYNC_METHOD_ALL = "gxAllSync";
		internal const string SYNC_METHOD_CHECK = "gxCheckSync";
		internal const string SYNC_METHOD_CONFIRM = "gxconfirmsync";
		internal const string SYNC_EVENT_PARAMETER = "event";
		internal const string CORE_OFFLINE_EVENT_REPLICATOR = "GeneXus.Core.genexus.sd.synchronization.offlineeventreplicator";
		internal const string SYNCHRONIZER_INFO = "gxTpr_Synchronizer";
	}

#if NETCORE
	public class GxRestWrapper
#else
	public class GxRestWrapper : IHttpHandler, IRequiresSessionState
#endif
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<GxRestWrapper>();
		protected HttpContext _httpContext;
		protected IGxContext _gxContext;
		private GXBaseObject _procWorker;
		private const string EXECUTE_METHOD = "execute";
		private string _serviceMethod = string.Empty;
		private Dictionary<string, string> _variableAlias = null;
		private Dictionary<string, object> _routeParms= null;
		private string _serviceMethodPattern;
		public bool WrappedParameter = false;

		public GxRestWrapper(GXBaseObject worker, HttpContext context, IGxContext gxContext, string serviceMethod, Dictionary<string,string> variableAlias, Dictionary<string,object> routeParms) : this(worker, context, gxContext)
		{
			_serviceMethod = serviceMethod;
			_variableAlias = variableAlias;
			_routeParms = routeParms;
		}

		public GxRestWrapper(GXBaseObject worker, HttpContext context, IGxContext gxContext, string serviceMethod, string serviceMethodPattern) : this(worker, context, gxContext)
		{
			_serviceMethod = serviceMethod;
			_serviceMethodPattern = serviceMethodPattern;
		}

		public GxRestWrapper(GXBaseObject worker, HttpContext context, IGxContext gxContext):this(context, gxContext)
		{
			_procWorker = worker;
		}
		public GxRestWrapper(HttpContext context, IGxContext gxContext)
		{
			_httpContext = context;
			_gxContext = gxContext;
			if (_httpContext != null)_httpContext.Response.ContentType = "application/json; charset=utf-8";		
			RunAsMain = true;
		}
		internal virtual GXBaseObject Worker
		{
			get { return _procWorker; }
		}
		public virtual void Cleanup()
		{
			if (RunAsMain)
				_gxContext.CloseConnections();
		}

		public virtual Task MethodBodyExecute(object key)
		{
			try
			{
				String innerMethod = EXECUTE_METHOD;
				bool wrapped = true;
				Dictionary<string, object> bodyParameters = null;
				if (IsCoreEventReplicator(_procWorker))
				{
					bodyParameters = ReadBodyParameters();
					string synchronizer = PreProcessReplicatorParameteres( _procWorker, innerMethod, bodyParameters);
					if (!IsAuthenticated(synchronizer))
						return Task.CompletedTask;
				}
				else if (!IsAuthenticatedMethod(this._serviceMethod, _procWorker.IsApiObject))
				{
					return Task.CompletedTask;
				}
				if (Worker.UploadEnabled() && GxUploadHelper.IsUploadURL(_httpContext))
				{
					GXObjectUploadServices gxobject = new GXObjectUploadServices(_gxContext);
					gxobject.webExecute();
					return Task.CompletedTask;
				}
				if (!ProcessHeaders(GXBaseObject.GetObjectNameWithoutNamespace(_procWorker.GetType().FullName)))
					return Task.CompletedTask;
				_procWorker.IsMain = true;
				if (bodyParameters == null)
					bodyParameters = ReadBodyParameters();
				addPathParameters(bodyParameters);
				if (_procWorker.IsSynchronizer2)
				{
					innerMethod = SynchronizerMethod();
					PreProcessSynchronizerParameteres(_procWorker, innerMethod, bodyParameters);
					wrapped = false;
				}				
				if (!String.IsNullOrEmpty(this._serviceMethod))
				{
					innerMethod = this._serviceMethod;
					bodyParameters = PreProcessApiSdtParameter( _procWorker, innerMethod, bodyParameters, this._variableAlias);
				}
				ServiceHeaders();
				Dictionary<string, object> outputParameters = ReflectionHelper.CallMethod(_procWorker, innerMethod, bodyParameters, _gxContext);
				Dictionary<string, string> formatParameters = ReflectionHelper.ParametersFormat(_procWorker, innerMethod);				
				setWorkerStatus(_procWorker);
				_procWorker.cleanup();
				int originalParameterCount = outputParameters.Count;
				RestProcess(_procWorker, outputParameters);
				wrapped = GetWrappedStatus(_procWorker, wrapped, outputParameters, outputParameters.Count, originalParameterCount);
				return Serialize(outputParameters, formatParameters, wrapped);
			}
			catch (Exception e)
			{
				return WebException(e);
			}
			finally
			{
				Cleanup();
			}	
		}

		public virtual Task Post()
		{
			return MethodBodyExecute(null);
		}

		private void setWorkerStatus(GXBaseObject _procWorker)
		{			
			if (ReflectionHelper.HasMethod(_procWorker, "getrestcode"))
			{
				Dictionary<string, object> outVal = ReflectionHelper.CallMethod(_procWorker, "getrestcode", new Dictionary<string, object>());
				short statusCode = (short)outVal.Values.First<object>();
				if (statusCode > 0)
					this.SetStatusCode((HttpStatusCode) statusCode);
			}
			if (ReflectionHelper.HasMethod(_procWorker, "getrestmsg"))
			{
				Dictionary<string, object> outVal = ReflectionHelper.CallMethod(_procWorker, "getrestmsg", new Dictionary<string, object>());
				string statusMsg = outVal.Values.First<object>().ToString();
				if (!String.IsNullOrEmpty(statusMsg))
						this.SetStatusMessage(statusMsg);
			}
		}

		private Dictionary<string, object> ReadBodyParameters()
		{
#if NETCORE
			return ReadRequestParameters(_httpContext.Request.Body);
#else
			return ReadRequestParameters(_httpContext.Request.GetInputStream());
#endif
		}

		private Dictionary<string, object> SetAlias(Dictionary<string, object> bodyParameters, Dictionary<string, string> varAlias)
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			foreach (string k in bodyParameters.Keys)
			{
				if (k != null)
				{
					string keyLowercase = k.ToLower();
					if (varAlias == null)
						parameters[keyLowercase] = bodyParameters[k];
					else
					{
						if (varAlias.ContainsKey(keyLowercase))
						{
							string alias = varAlias[keyLowercase].ToLower();
							parameters[alias] = bodyParameters[k];
						}
						else if (!varAlias.ContainsValue(keyLowercase))
						{
							parameters[keyLowercase] = bodyParameters[k];
						}
					}
				}
			}
			return parameters;
		}

		private Dictionary<string, object> PreProcessApiSdtParameter(GXBaseObject procWorker, string innerMethod,
				Dictionary<string,object> bodyParameters, Dictionary<string, string> varAlias)
		{
			Dictionary<string, object> bP = SetAlias(bodyParameters, varAlias);
			return ReflectionHelper.GetWrappedParameter(procWorker, innerMethod, bP);
		}

		private string PreProcessReplicatorParameteres(GXBaseObject procWorker, string innerMethod, Dictionary<string, object> bodyParameters)
		{
			var methodInfo = procWorker.GetType().GetMethod(innerMethod);
			object[] parametersForInvocation = ReflectionHelper.ProcessParametersForInvoke(methodInfo, bodyParameters);
			object synchroInfo = parametersForInvocation[1];
			return synchroInfo.GetType().GetProperty(Synchronizer.SYNCHRONIZER_INFO).GetValue(synchroInfo) as string;

		}

		private bool IsCoreEventReplicator(GXBaseObject procWorker)
		{
			return procWorker.GetType().FullName == Synchronizer.CORE_OFFLINE_EVENT_REPLICATOR; 
		}

		private void PreProcessSynchronizerParameteres(GXBaseObject instance, string method, Dictionary<string, object> bodyParameters)
		{
			string gxParameterName = instance.GetType().GetMethod(method).GetParameters().First().Name.ToLower();
			GxUnknownObjectCollection hashList;
			if (bodyParameters.ContainsKey(string.Empty))
				hashList = (GxUnknownObjectCollection)ReflectionHelper.ConvertStringToNewType(bodyParameters[string.Empty], typeof(GxUnknownObjectCollection));
			else
				hashList = new GxUnknownObjectCollection();
			bodyParameters[gxParameterName] = TableHashList(hashList);
		}
		internal GxUnknownObjectCollection TableHashList(GxUnknownObjectCollection tableHashList)
		{
			GxUnknownObjectCollection result = new GxUnknownObjectCollection();
			if (tableHashList != null && tableHashList.Count > 0)
			{
				foreach (JArray list in tableHashList)
				{
					GxStringCollection tableHash = new GxStringCollection();
					foreach (string data in list)
					{
						tableHash.Add(data);
					}
					result.Add(tableHash);
				}
			}
			return result;
		}

		private string SynchronizerMethod()
		{
			string method = string.Empty;
			var queryParameters = ReadQueryParameters(this._variableAlias);
			string gxevent = string.Empty;
			if (queryParameters.ContainsKey(Synchronizer.SYNC_EVENT_PARAMETER))
				gxevent = (string)queryParameters[Synchronizer.SYNC_EVENT_PARAMETER];

			if (string.IsNullOrEmpty(gxevent))
			{
				method = Synchronizer.SYNC_METHOD_ALL;
			}
			else
			{
				if (gxevent.Equals(Synchronizer.SYNC_METHOD_CHECK, StringComparison.OrdinalIgnoreCase))
				{
					method = Synchronizer.SYNC_METHOD_CHECK;
				}
				else
				{
					if (gxevent.Equals(Synchronizer.SYNC_METHOD_CONFIRM, StringComparison.OrdinalIgnoreCase))
					{
						method = Synchronizer.SYNC_METHOD_CONFIRM;
					}
				}
			}
			return method;
		}
		public virtual Task Get(object key)
		{
			return MethodUrlExecute(key);
		}
		public virtual Task  MethodUrlExecute(object key)
		{
			try
			{
				if (!IsAuthenticatedMethod(this._serviceMethod, _procWorker.IsApiObject))
				{
					return Task.CompletedTask; 
				}
				if (!ProcessHeaders(GXBaseObject.GetObjectNameWithoutNamespace(_procWorker.GetType().FullName)))
					return Task.CompletedTask;
				_procWorker.IsMain = true;
				IDictionary<string,object> queryParameters = ReadQueryParameters(this._variableAlias);
				addPathParameters(queryParameters);
				string innerMethod = EXECUTE_METHOD;
				Dictionary<string, object> outputParameters;
				Dictionary<string, string> formatParameters = new Dictionary<string, string>();
				ServiceHeaders();
				if (!string.IsNullOrEmpty(_serviceMethodPattern))
				{
					innerMethod = _serviceMethodPattern;
					outputParameters = ReflectionHelper.CallMethodPattern(_procWorker, innerMethod, queryParameters);
				}
				else 
				{
					if (!string.IsNullOrEmpty(_serviceMethod))
					{
						innerMethod = _serviceMethod;
					}
					outputParameters = ReflectionHelper.CallMethod(_procWorker, innerMethod, queryParameters);
					formatParameters = ReflectionHelper.ParametersFormat(_procWorker, innerMethod);
				}
				int parCount = outputParameters.Count;
				setWorkerStatus(_procWorker);
				_procWorker.cleanup();
				int originalParameterCount = outputParameters.Count;
				RestProcess(_procWorker, outputParameters);			  
				bool wrapped = false;
				wrapped = GetWrappedStatus(_procWorker, wrapped, outputParameters, parCount, originalParameterCount);
				return Serialize(outputParameters, formatParameters, wrapped);
			}
			catch (Exception e)
			{
				return WebException(e);
			}
			finally
			{
				Cleanup();
			}
		}
		private bool GetWrappedStatus(GXBaseObject worker, bool defaultWrapped, Dictionary<string, object> outputParameters, int parCount, int originalParCount)
		{
			bool wrapped = defaultWrapped;

			if (worker.IsApiObject)
			{
				if (outputParameters.Count == 1)
				{
					if ((originalParCount == 1) || (originalParCount > 1 && !Preferences.WrapSingleApiOutput))
					{
						wrapped = GetCollectionWrappedStatus(outputParameters, parCount, false, true);
					}
					if (originalParCount > 1 && Preferences.WrapSingleApiOutput)
					{
						wrapped = true; //Ignore defaultWrapped parameter.
					}
				}
			}
			else
			{
					if (originalParCount == 1)
						wrapped = GetCollectionWrappedStatus(outputParameters, parCount, wrapped, false);				
			}
			return wrapped;
		}


		private bool GetCollectionWrappedStatus(Dictionary<string, object> outputParameters , int parCount, bool defaultWrapped, bool isAPI)
		{
			bool wrapped = defaultWrapped;
			if (outputParameters.Count > 0)
			{
				Object v = outputParameters.First().Value;				
				if (v.GetType().GetInterfaces().Contains(typeof(IGxGenericCollectionWrapped)))
				{
					IGxGenericCollectionWrapped icollwrapped = v as IGxGenericCollectionWrapped;
					if (icollwrapped != null)
					{
						if (icollwrapped.GetWrappedStatus().Equals("default"))
							wrapped = defaultWrapped;
						else
							wrapped = icollwrapped.GetIsWrapped();
					}
				}

				if (isAPI)
				{
					if (v is IGxGenericCollectionItem item)
					{
						if (item.Sdt is GxSilentTrnSdt)
						{
							wrapped = (parCount > 1) ? true : false;
						}
					}
				}
			}
			return wrapped;
		}

		public bool RunAsMain
		{
			get;set;
		}

		public bool IsReusable => throw new NotImplementedException();

		public virtual Task Delete(object key)
		{
			return MethodUrlExecute(key);
		}		

		public virtual Task Put(object key)
		{
			return MethodBodyExecute(key);
		}

		public virtual Task Patch(object key)
		{
			return MethodBodyExecute(key);
		}
		public Dictionary<string, object> ReadRequestParameters(Stream stream)
		{
			return RestAPIHelpers.ReadRestBodyParameters(stream);
		}

		protected IDictionary<string, object> ReadQueryParameters(Dictionary<string,string>  varAlias)
		{
			NameValueCollection query = _httpContext.Request.GetQueryString();
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			foreach(string k in query.AllKeys)
			{
				if (k!=null)
				{
					string keyLowercase = k.ToLower();
					if (varAlias==null)
						parameters[keyLowercase] = query[k];
					else
					{
						if (varAlias.ContainsKey(keyLowercase))
						{
							string alias = varAlias[keyLowercase].ToLower();
							parameters[alias] = query[k];
						}
						else if (!varAlias.ContainsValue(keyLowercase))
						{
							parameters[keyLowercase] = query[k];
						}
					}
				}
			}
			return parameters;
		} 
		
		protected void addPathParameters(IDictionary<string, object> parameters)
		{
#if NETCORE
			var route = _httpContext.Request.RouteValues;
			foreach (KeyValuePair<string, object> kv in route)
			{
				if (!parameters.ContainsKey(kv.Key))
					parameters.Add(kv.Key, kv.Value);
			}
#else
			if(_routeParms != null)
			{
				foreach(KeyValuePair<string,object> kv in _routeParms)
				{
					parameters.Add(kv.Key, kv.Value);
				}
			}
#endif
		}
		
		public bool IsRestParameter(string parameterName)
		{
			try
			{
				string pValue = _httpContext.Request.GetQueryString()[parameterName];
				if (pValue != null)
				{
					return pValue.Equals("true", StringComparison.OrdinalIgnoreCase);
				}
				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}
		protected void SetMessages(msglist messages)
		{
			StringBuilder header = new StringBuilder();
			bool emptyHeader = true, encoded = false;
			const string EncodedFlag = "Encoded:";
			const string SystemMsg = "System";
			const string UserMsg = "User";
			string typeMsg;

			foreach (msglistItem msg in messages)
			{
				if (msg.gxTpr_Type == 0)
				{
					string value = msg.gxTpr_Description;
					encoded = false;
					if (GXUtil.ContainsNoAsciiCharacter(value))
					{
						value = GXUtil.UrlEncode(value);
						encoded = true;
					}
					typeMsg = msg.IsGxMessage ? SystemMsg : UserMsg;

					header.AppendFormat("{0}299 {1} \"{2}{3}:{4}\"", emptyHeader ? string.Empty : ",", _httpContext.Request.GetHost(), encoded ? EncodedFlag : string.Empty, typeMsg, value);
					if (emptyHeader)
					{
						emptyHeader = false;
					}
				}
			}
			if (!emptyHeader)
			{
				AddHeader(HttpHeader.WARNING_HEADER, header.ToString());
			}
		}
		public Task SetError(string code, string message)
		{
			return SetError(_httpContext, code, message);
		}
		public static Task SetError(HttpContext context, string code, string message)
		{
			HttpHelper.SetError(context, code, message);
			return Task.CompletedTask;
		}
		public bool IsAuthenticated(string synchronizer)
		{
			GXLogging.Debug(log, "IsMainAuthenticated synchronizer:" + synchronizer);
			bool validSynchronizer = false;
			try
			{
				if (!string.IsNullOrEmpty(synchronizer))
				{
					string nspace;
					if (!Config.GetValueOf("AppMainNamespace", out nspace))
						nspace = "GeneXus.Programs";
					String assemblyName = synchronizer.ToLower();
					String restServiceName = nspace + "." + assemblyName;
					GXProcedure synchronizerService = (GXProcedure)ClassLoader.GetInstance(assemblyName, restServiceName, null);
					if (synchronizerService != null && synchronizerService.IsSynchronizer2)
					{
						validSynchronizer = true;
						return IsAuthenticated(synchronizerService.IntegratedSecurityLevel2, synchronizerService.IntegratedSecurityEnabled2, synchronizerService.ExecutePermissionPrefix2);
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, ex, "IsMainAuthenticated error ");
				return false;
			}
			finally
			{
				if (!validSynchronizer)
					SetError("0", "Invalid Synchronizer " + synchronizer);
			}
		}
		protected bool IsAuthenticatedMethod(string serviceMethod, bool isApi)
		{
			if (!String.IsNullOrEmpty(serviceMethod) && isApi)
			{
				bool integratedSecurityEnabled = ( Worker.IntegratedSecurityEnabled2 && Worker.ApiIntegratedSecurityLevel2(serviceMethod) != GAMSecurityLevel.SecurityNone);
				return IsAuthenticated(Worker.ApiIntegratedSecurityLevel2(serviceMethod), integratedSecurityEnabled, Worker.ApiExecutePermissionPrefix2(serviceMethod));
			}
			else
				return IsAuthenticated();
		}
		public bool IsAuthenticated()
		{
			return IsAuthenticated( Worker.IntegratedSecurityLevel2, Worker.IntegratedSecurityEnabled2, Worker.ExecutePermissionPrefix2);
		}
		protected bool IsAuthenticated(GAMSecurityLevel objIntegratedSecurityLevel, bool objIntegratedSecurityEnabled, string objPermissionPrefix)
		{
			if (!objIntegratedSecurityEnabled)
			{
				return true;
			}
			else
			{
				String token = GetHeader("Authorization");
				if (token == null)
				{
					SetError("0", "This service needs an Authorization Header");
					return false;
				}
				else
				{
					token = token.Replace("OAuth ", "");
					if (objIntegratedSecurityLevel == GAMSecurityLevel.SecurityLow)
					{
						bool isOK;
						GxResult result = GxSecurityProvider.Provider.checkaccesstoken(_gxContext, token, out isOK);
						if (!isOK)
						{
							HttpHelper.SetGamError(_httpContext, result.Code, result.Description);
							return false;
						}
					}
					else if (objIntegratedSecurityLevel == GAMSecurityLevel.SecurityHigh)
					{
						bool sessionOk, permissionOk;
						GxResult result = GxSecurityProvider.Provider.checkaccesstokenprm(_gxContext, token, objPermissionPrefix, out sessionOk, out permissionOk);
						if (permissionOk)
						{
							return true;
						}
						else
						{
							HttpStatusCode defaultStatusCode = sessionOk ? HttpStatusCode.Forbidden : HttpStatusCode.Unauthorized;
							HttpHelper.SetGamError(_httpContext, result.Code, result.Description, defaultStatusCode);
							return false;
						}
					}
				}
				return true;
			}
		}
		protected void SetStatusCode(HttpStatusCode code)
		{
			if (_httpContext != null)
			{
				_httpContext.Response.StatusCode = (int)code;
			}
		}
		protected void SetStatusMessage(String statusMessage)
		{
			if (_httpContext != null)
			{
				_httpContext.SetReasonPhrase(statusMessage);
			}
		}
#if NETCORE
		IHeaderDictionary GetHeaders()
		{
			if (_httpContext != null)
			{
				return _httpContext.Request.Headers;
			}
			else return null;
		}
#else
		NameValueCollection GetHeaders()
		{
			if (_httpContext != null)
			{
				return _httpContext.Request.Headers;
			}
			else return null;
		}

#endif
		string GetHeader(string header)
		{
			return GetHeaders()[header];
		}
		void AddHeader(string header, string value)
		{
			if (_httpContext != null)
			{
				_httpContext.Response.Headers[header] = value;
			}
		}

		protected bool ProcessHeaders(string queryId)
		{
			var headers = GetHeaders();
			String language = null, theme=null, etag = null;
			if (headers != null)
			{
				language = headers["GeneXus-Language"];
				theme = headers["GeneXus-Theme"];
				etag = headers["If-Modified-Since"];
			}

			if (!string.IsNullOrEmpty(language))
				_gxContext.SetLanguage(language);

			if (!string.IsNullOrEmpty(theme))
				_gxContext.SetTheme(theme);

			DateTime dt = HTMLDateToDatetime(etag);
			DateTime newDt;
			DataUpdateStatus status;

			if (etag == null)
			{
				status = DataUpdateStatus.Invalid;
				GxSmartCacheProvider.CheckDataStatus(queryId, dt, out newDt);
			}
			else
			{
				status = GxSmartCacheProvider.CheckDataStatus(queryId, dt, out newDt);
			}
			AddHeader("Last-Modified", dateTimeToHTMLDate(newDt));

			if (status == DataUpdateStatus.UpToDate)
			{
				SetStatusCode(HttpStatusCode.NotModified);
				return false;
			}
			return true;
		}
		private void ServiceHeaders()
		{
			SendCacheHeaders();
			HttpHelper.CorsHeaders(_httpContext);
			HttpHelper.AllowHeader(_httpContext, new List<string>() { $"{HttpMethod.Get.Method},{HttpMethod.Post.Method}" });

		}

		private void SendCacheHeaders()
		{
			if (string.IsNullOrEmpty(_gxContext.GetHeader(HttpHeader.CACHE_CONTROL)))
				AddHeader("Cache-Control", HttpHelper.CACHE_CONTROL_HEADER_NO_CACHE);
		}
		DateTime HTMLDateToDatetime(string s)
		{
			// Formato fecha: RFC 1123
			DateTime dt;
			if (DateTime.TryParse(s, DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.AdjustToUniversal, out dt))
				return dt;
			return DateTime.MinValue;
		}
		string dateTimeToHTMLDate(DateTime dt)
		{
			return dt.ToUniversalTime().ToString(DateTimeFormatInfo.InvariantInfo.RFC1123Pattern, DateTimeFormatInfo.InvariantInfo);
		}
		public Task WebException(Exception ex)
		{
#if NETCORE
			GxHttpActivitySourceHelper.SetException(Activity.Current, ex);
#endif
			GXLogging.Error(log, "WebException", ex);
			if (ex is FormatException)
			{
				HttpHelper.SetUnexpectedError(_httpContext, HttpStatusCode.BadRequest, ex);
			}
			else
			{
				HttpHelper.SetUnexpectedError(_httpContext, HttpStatusCode.InternalServerError, ex);
			}
			return Task.CompletedTask;
		}
		protected Task Serialize(Dictionary<string, object> parameters, Dictionary<string, string> fmtParameters, bool wrapped)
		{
			string json;
			var knownTypes = new List<Type>();
			foreach (string k in parameters.Keys)
			{
				object val = parameters[k];
				knownTypes.Add(val.GetType());
			}
			if (parameters.Count == 1 && !wrapped && !PrimitiveType(knownTypes[0])) //In Dataproviders, with one parameter BodyStyle is WebMessageBodyStyle.Bare, Both requests and responses are not wrapped.
			{
				string key = parameters.First().Key;
				object strVal = null;
				if (parameters[key].GetType() == typeof(DateTime))
				{
					strVal = SerializeDateTime((DateTime)parameters[key], key, fmtParameters);
				}				
				else
					strVal = parameters[key];
				json = JSONHelper.WCFSerialize( strVal, Encoding.UTF8, knownTypes, true);
			}
			else
			{
				Dictionary<string, object> serializablePars = new Dictionary<string, object>();
				foreach (KeyValuePair<string,object> kv in parameters)
				{
					string strKey = kv.Key;					

					IGxGenericCollectionItem ut = kv.Value as IGxGenericCollectionItem;
					if (ut != null)
					{						
						Type uType = ut.Sdt.GetType();
						object[] attributes = uType.GetCustomAttributes(true);
						GxJsonName jsonName = (GxJsonName) attributes.Where(a => a.GetType() == typeof(GxJsonName)).FirstOrDefault();
						if (jsonName != null)
							strKey = jsonName.Name;
					}
					if (kv.Value.GetType() == typeof(DateTime))
					{
						object strVal = SerializeDateTime((DateTime)kv.Value, kv.Key, fmtParameters);
						serializablePars.Add(strKey, strVal);
					}
					else
						serializablePars.Add(strKey, kv.Value);
				}
				json = JSONHelper.WCFSerialize(serializablePars, Encoding.UTF8, knownTypes, true); 
			}
			_httpContext.Response.Write(json); //Use intermediate StringWriter in order to avoid chunked response
			return Task.CompletedTask;
		}

		private object SerializeDateTime(DateTime dt, string key, Dictionary<string, string> fmtParameters)
		{
			DateTime udt = (dt==DateTimeUtil.NullDate()) ? dt: dt.ToUniversalTime();
			
			if (fmtParameters.ContainsKey(key) && !String.IsNullOrEmpty(fmtParameters[key]))
			{
				return DateTimeUtil.DToC2(udt, fmtParameters[key]);
			}
			return udt;
		}

		private bool PrimitiveType(Type type)
		{
			return type.IsPrimitive || type == typeof(string) || type.IsValueType;
		}
		protected Task Serialize(object value)
		{
#if NETCORE
			var responseStream = _httpContext.Response.Body;
#else
			var responseStream = _httpContext.Response.OutputStream;
#endif
			var knownTypes = new List<Type>
			{
				value.GetType()
			};
		
			JSONHelper.WCFSerialize(value, Encoding.UTF8, knownTypes, responseStream);
			return Task.CompletedTask;
		}

		protected void Deserialize(string value, ref GxSilentTrnSdt sdt)
		{
			sdt.FromJSonString(value);
		}

		private static void RestProcess(GXBaseObject worker, Dictionary<string, object> outputParameters)
		{
			foreach (string k in outputParameters.Keys.ToList())
			{
				GxUserType p = outputParameters[k] as GxUserType;
				if ((p != null) && !p.ShouldSerializeSdtJson())
				{
					outputParameters.Remove(k);
				}
				else
				{
					object o = MakeRestType(outputParameters[k], worker.IsApiObject);
					if (p !=null && p.SdtSerializeAsNull())
					{						
						outputParameters[k] = JNull.Value;
					}
					else
					{
						if (o == null)
							outputParameters.Remove(k);
						else
							outputParameters[k] = o;
					}
				}
			}			
		}
		
		protected static object MakeRestType( object collectionValue, bool isApiObject)
		{
			Type vType = collectionValue.GetType();
			Type itemType;
			if (vType.IsConstructedGenericType && typeof(IGxCollection).IsAssignableFrom(vType)) 
			{				
				bool isWrapped = (isApiObject)?false:true;				
				bool isEmpty = false;
				object collectionObject = null;
				string wrappedStatus = "";
				Type restItemType=null;
				itemType = collectionValue.GetType().GetGenericArguments()[0];
				if (vType.GetGenericTypeDefinition() == typeof(GxSimpleCollection<>) && isApiObject)
				{
					restItemType = itemType;
					isEmpty = true;
					isWrapped = false;
					collectionObject = collectionValue;
				}
				else
				{
					if ((typeof(IGXBCCollection).IsAssignableFrom(vType)) && !isApiObject)//Collection<BCType> convert to GxGenericCollection<BCType_RESTLInterface>
					{
						restItemType = ClassLoader.FindType(Config.CommonAssemblyName, itemType.FullName + "_RESTLInterface", null);
					}
					if (restItemType == null)//Collection<SDTType> convert to GxGenericCollection<SDTType_RESTInterface>
					{
						restItemType = ClassLoader.FindType(Config.CommonAssemblyName, itemType.FullName + "_RESTInterface", null);
					}
					object[] attributes = restItemType.GetCustomAttributes(typeof(GxJsonSerialization),  false);
					IEnumerable<object>  serializationAttributes = attributes.Where(a => a.GetType() == typeof(GxJsonSerialization));
					if (serializationAttributes != null && serializationAttributes.Any<object>())
					{
						GxJsonSerialization attFmt = (GxJsonSerialization)serializationAttributes.FirstOrDefault();
						wrappedStatus = attFmt.JsonUnwrapped;
						isWrapped = (isApiObject)? ((wrappedStatus == "wrapped")? true: false): ((wrappedStatus == "unwrapped") ? false : true);
					}
					isEmpty = !restItemType.IsDefined(typeof(GxOmitEmptyCollection), false);
					Type genericListItemType = typeof(GxGenericCollection<>).MakeGenericType(restItemType);
					collectionObject = Activator.CreateInstance(genericListItemType, new object[] { collectionValue, isWrapped , wrappedStatus});
				}
				// Empty collection serialized w/ noproperty
				if (collectionObject is IList restList)
				{
					if (restList.Count == 0 && !isEmpty)
						return null;
				}
				return collectionObject;			
			}
			else if (typeof(GxUserType).IsAssignableFrom(vType)) //SDTType convert to SDTType_RESTInterface
			{
				Type restItemType = ClassLoader.FindType(Config.CommonAssemblyName, vType.FullName + "_RESTInterface", null);
				return Activator.CreateInstance(restItemType, new object[] { collectionValue });
			}
			return collectionValue;
		}

#if !NETCORE
		public void ProcessRequest(HttpContext context)
		{
			try
			{
				ControllerInfo controllerInfo = new ControllerInfo() { MethodName = context.Request.HttpMethod, Name = "execute", Parameters = "querystring" };
				if (context.Request.HttpMethod == "GET")
					Get(controllerInfo.Parameters);
				else if (context.Request.HttpMethod == "POST")
					Post();
				else if (context.Request.HttpMethod == "PUT")
					Put(controllerInfo.Parameters);
				else if (context.Request.HttpMethod == "DELETE")
					Delete(controllerInfo.Parameters);
				else if (context.Request.HttpMethod == "PATCH")
					Patch(controllerInfo.Parameters);
				else
				{
					context.Response.StatusCode = (int)HttpStatusCode.NotFound;
					context.Response.Headers.Clear();
				}
			}
			catch (Exception ex)
			{
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				context.Response.StatusDescription = ex.Message;
			}
		}
#endif
	}
	public class ControllerInfo
	{
		public string Name { get; set; }
		public string Parameters { get; set; }
		public string MethodName { get; set; }
		public string MethodPattern { get; set; }
		public string Verb { get; set; }
		public Dictionary<string, string> VariableAlias { get; set; }
	}
}
