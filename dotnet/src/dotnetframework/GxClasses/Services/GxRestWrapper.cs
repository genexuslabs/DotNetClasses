using System;
using System.Collections.Generic;
using System.Text;
using log4net;
#if NETCORE
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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
using Jayrock.Json;


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
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Application.GxRestWrapper));
		protected HttpContext _httpContext;
		protected IGxContext _gxContext;
		private GXProcedure _procWorker;
		private const string EXECUTE_METHOD = "execute";
		public String ServiceMethod = "";
		public bool WrappedParameter = false;


		public GxRestWrapper(GXProcedure worker, HttpContext context, IGxContext gxContext, String serviceMethod) : this(worker, context, gxContext)
		{
			ServiceMethod = serviceMethod;
		}

		public GxRestWrapper(GXProcedure worker, HttpContext context, IGxContext gxContext):this(context, gxContext)
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
		protected virtual GXBaseObject Worker
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
					string synchronizer = PreProcessReplicatorParameteres(_procWorker, innerMethod, bodyParameters);
					if (!IsAuthenticated(synchronizer))
						return Task.CompletedTask;
				}
				else if (!IsAuthenticated())
				{
					return Task.CompletedTask;
				}
				if (Worker.UploadEnabled() && GxUploadHelper.IsUploadURL(_httpContext))
				{
					GXObjectUploadServices gxobject = new GXObjectUploadServices(_gxContext);
					gxobject.webExecute();
					return Task.CompletedTask;
				}
				if (!ProcessHeaders(_procWorker.GetType().Name))
					return Task.CompletedTask;
				_procWorker.IsMain = true;
				if (bodyParameters == null)
					bodyParameters = ReadBodyParameters();

				if (_procWorker.IsSynchronizer2)
				{
					innerMethod = SynchronizerMethod();
					PreProcessSynchronizerParameteres(_procWorker, innerMethod, bodyParameters);
					wrapped = false;
				}				

				if (!String.IsNullOrEmpty(this.ServiceMethod))
				{
					innerMethod = this.ServiceMethod;
				}
				Dictionary<string, object> outputParameters = ReflectionHelper.CallMethod(_procWorker, innerMethod, bodyParameters, _gxContext);
				wrapped = GetWrappedStatus(_procWorker ,wrapped, outputParameters, outputParameters.Count);				
				setWorkerStatus(_procWorker);
				_procWorker.cleanup();
				RestProcess(outputParameters);
				return Serialize(outputParameters, wrapped);
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

		private void setWorkerStatus(GXProcedure _procWorker)
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
		private string PreProcessReplicatorParameteres(GXProcedure procWorker, string innerMethod, Dictionary<string, object> bodyParameters)
		{
			var methodInfo = procWorker.GetType().GetMethod(innerMethod);
			object[] parametersForInvocation = ReflectionHelper.ProcessParametersForInvoke(methodInfo, bodyParameters);
			var synchroInfo = parametersForInvocation[1];
			return synchroInfo.GetType().GetProperty(Synchronizer.SYNCHRONIZER_INFO).GetValue(synchroInfo) as string;

		}

		private bool IsCoreEventReplicator(GXProcedure procWorker)
		{
			return procWorker.GetType().FullName == Synchronizer.CORE_OFFLINE_EVENT_REPLICATOR; 
		}

		private void PreProcessSynchronizerParameteres(GXProcedure instance, string method, Dictionary<string, object> bodyParameters)
		{
			var gxParameterName = instance.GetType().GetMethod(method).GetParameters().First().Name.ToLower();
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
			var queryParameters = ReadQueryParameters();
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
				if (!IsAuthenticated())
				{
					return Task.CompletedTask; 
				}
				if (!ProcessHeaders(_procWorker.GetType().Name))
					return Task.CompletedTask;
				_procWorker.IsMain = true;
				var queryParameters = ReadQueryParameters();
				String innerMethod = EXECUTE_METHOD;
				if (!String.IsNullOrEmpty(this.ServiceMethod))
				{
					innerMethod = this.ServiceMethod;
				}
				Dictionary<string, object> outputParameters = ReflectionHelper.CallMethod(_procWorker, innerMethod, queryParameters);
				int parCount = outputParameters.Count;
				setWorkerStatus(_procWorker);
				_procWorker.cleanup();
				RestProcess(outputParameters);			  
				bool wrapped = false;
				wrapped = GetWrappedStatus(_procWorker, wrapped, outputParameters, parCount);			
				return Serialize(outputParameters, wrapped);
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

		bool GetWrappedStatus(GXProcedure worker, bool wrapped, Dictionary<string, object> outputParameters, int parCount)
		{
			if (worker.IsApiObject)
			{
				if (outputParameters.Count == 1)
				{
					wrapped = false;
					Object v = outputParameters.First().Value;

					if (v.GetType().GetInterfaces().Contains(typeof(IGxGenericCollectionWrapped)))
					{
						wrapped = (v as IGxGenericCollectionWrapped).GetIsWrapped();
					}
					if (v is IGxGenericCollectionItem item)
					{
						if (item.Sdt is GxSilentTrnSdt)
						{
							wrapped = (parCount>1)?true:false;
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
			var bodyParameters = new Dictionary<string, object>();
			using (StreamReader streamReader = new StreamReader(stream))
			{
				if (!streamReader.EndOfStream)
				{
					try
					{
						string json = streamReader.ReadToEnd();
						var data = JSONHelper.ReadJSON<dynamic>(json);
						JObject jobj = data as JObject;
						JArray jArray = data as JArray;
						if (jobj != null)
						{
							foreach (string name in jobj.Names)
							{
								bodyParameters.Add(name.ToLower(), jobj[name]);
							}
						}
						else if (jArray != null)
						{
							bodyParameters.Add(string.Empty, jArray);
						}
					}
					catch (Exception ex)
					{
						GXLogging.Error(log, ex, "Parsing error in Body ");

					}
				}
			}
			return bodyParameters;
		}
		protected IDictionary<string, object> ReadQueryParameters()
		{
			var query = _httpContext.Request.GetQueryString();
			Dictionary<string, object> parameters = query.Keys.Cast<string>()
					.ToDictionary(k => k.ToLower(), v => (object)query[v].ToString());
			return parameters;
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
			return HttpHelper.SetResponseStatusAndJsonErrorAsync(context, code, message);
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
		public bool IsAuthenticated()
		{
			return IsAuthenticated(Worker.IntegratedSecurityLevel2, Worker.IntegratedSecurityEnabled2, Worker.ExecutePermissionPrefix2);
		}
		private bool IsAuthenticated(GAMSecurityLevel objIntegratedSecurityLevel, bool objIntegratedSecurityEnabled, string objPermissionPrefix)
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
							SetError(result.Code, result.Description);
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
							SetError(result.Code, result.Description);
							if (sessionOk)
							{
								SetStatusCode(HttpStatusCode.Forbidden);
							}
							else
							{
								AddHeader(HttpHeader.AUTHENTICATE_HEADER, HttpHelper.OatuhUnauthorizedHeader(_gxContext.GetServerName(), result.Code, result.Description));
								SetStatusCode(HttpStatusCode.Unauthorized);
							}
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
#if !NETCORE
				_httpContext.Response.StatusDescription = statusMessage;
#else
				_httpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = statusMessage.Replace(Environment.NewLine, string.Empty);

#endif
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
			GXLogging.Error(log, "WebException", ex);
			return SetError("500", ex.Message);
		}
		protected Task Serialize(Dictionary<string, object> parameters, bool wrapped)
		{
			string json;
			var knownTypes = new List<Type>();
			foreach (var k in parameters.Keys)
			{
				var val = parameters[k];
				knownTypes.Add(val.GetType());
			}
			if (parameters.Count == 1 && !wrapped) //In Dataproviders, with one parameter BodyStyle is WebMessageBodyStyle.Bare, Both requests and responses are not wrapped.
			{
				string key = parameters.First().Key;
				json = JSONHelper.WCFSerialize(parameters[key], Encoding.UTF8, knownTypes, true);
			}
			else
			{
				json = JSONHelper.WCFSerialize(parameters, Encoding.UTF8, knownTypes, true); 
			}
			_httpContext.Response.Write(json); //Use intermediate StringWriter in order to avoid chunked response
			return Task.CompletedTask;
		}
		protected Task Serialize(object value)
		{
#if NETCORE
			var responseStream = _httpContext.Response.Body;
#else
			var responseStream = _httpContext.Response.OutputStream;
#endif
			var knownTypes = new List<Type>();
			knownTypes.Add(value.GetType());
		
			JSONHelper.WCFSerialize(value, Encoding.UTF8, knownTypes, responseStream);
			return Task.CompletedTask;
		}

		protected void Deserialize(string value, ref GxSilentTrnSdt sdt)
		{
			sdt.FromJSonString(value);
		}

		private static void RestProcess(Dictionary<string, object> outputParameters)
		{
			foreach (var k in outputParameters.Keys.ToList())
			{
				GxUserType p = outputParameters[k] as GxUserType;
				if ((p != null) && !(p.ShouldSerializeSdtJson()))
				{
					outputParameters.Remove(k);
				}
			}
			MakeRestTypes(outputParameters);
		}
		private static void MakeRestTypes(Dictionary<string, object> parameters)
		{
			foreach (var key in parameters.Keys.ToList())
			{
				object o = MakeRestType(parameters[key]);
				if (o == null)
					parameters.Remove(key);
				else
					parameters[key] = o;
			}
		}
		protected static object MakeRestType(object v)
		{
			Type vType = v.GetType();
			Type itemType;
			if (vType.IsConstructedGenericType && typeof(IGxCollection).IsAssignableFrom(vType)) 
			{
				Type restItemType=null;
				itemType = v.GetType().GetGenericArguments()[0];
				if (typeof(IGXBCCollection).IsAssignableFrom(vType))//Collection<BCType> convert to GxGenericCollection<BCType_RESTLInterface>
				{
					restItemType = ClassLoader.FindType(Config.CommonAssemblyName, itemType.FullName + "_RESTLInterface", null);
				}
				if (restItemType == null)//Collection<SDTType> convert to GxGenericCollection<SDTType_RESTInterface>
				{
					restItemType = ClassLoader.FindType(Config.CommonAssemblyName, itemType.FullName + "_RESTInterface", null);
				}
				bool isWrapped = !restItemType.IsDefined(typeof(GxUnWrappedJson), false);
				bool isEmpty = !restItemType.IsDefined(typeof(GxOmitEmptyCollection), false);
				Type genericListItemType = typeof(GxGenericCollection<>).MakeGenericType(restItemType);
				object c = Activator.CreateInstance(genericListItemType, new object[] { v, isWrapped});
				// Empty collection serialized w/ noproperty
				if (c is IList restList)
				{
					if (restList.Count == 0 && !isEmpty)
						return null;
				}
				return c;			
			}
			else if (typeof(GxUserType).IsAssignableFrom(vType)) //SDTType convert to SDTType_RESTInterface
			{
				Type restItemType = ClassLoader.FindType(Config.CommonAssemblyName, vType.FullName + "_RESTInterface", null);
				return Activator.CreateInstance(restItemType, new object[] { v });
			}
			return v;
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
		public string Verb { get; set; }
	}
}
