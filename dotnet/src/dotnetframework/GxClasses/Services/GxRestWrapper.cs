using System;
using System.Collections.Generic;
using System.Text;
using log4net;
#if NETCORE
using Microsoft.AspNetCore.Http;
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

namespace GeneXus.Application

{
#if NETCORE
	public class GxRestWrapper
#else
	public class GxRestWrapper : IHttpHandler
#endif
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Application.GxRestWrapper));
		protected HttpContext _httpContext;
		protected IGxContext _gxContext;
		private GXProcedure _procWorker;
		private const string EXECUTE_METHOD = "execute";
		public String ServiceMethod = "";


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
			AddHeader("Content-type", "application/json; charset=utf-8"); //MediaTypesNames.ApplicationJson);
			RunAsMain = true;
		}
		
		public virtual void Cleanup()
		{
			if (RunAsMain)
				_gxContext.CloseConnections();
		}
		public virtual Task Post()
		{
			try
			{
				if (!IsAuthenticated())
				{
					return null;
				}
				if (!ProcessHeaders(_procWorker.GetType().Name))
					return Task.CompletedTask;
				_procWorker.IsMain = true;
#if NETCORE
				var bodyParameters = ReadRequestParameters(_httpContext.Request.Body);
#else
				var bodyParameters = ReadRequestParameters(_httpContext.Request.GetInputStream());
#endif
				String innerMethod = EXECUTE_METHOD;
				if (!String.IsNullOrEmpty(this.ServiceMethod))
				{
					innerMethod = this.ServiceMethod;
				}
				Dictionary<string, object> outputParameters = ReflectionHelper.CallMethod(_procWorker, innerMethod, bodyParameters);
				_procWorker.cleanup();
				MakeRestTypes(outputParameters);
				return Serialize(outputParameters, true);
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
		public virtual Task Get(object key)
		{
			try
			{
				if (!IsAuthenticated())
				{
					return null;
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
				var outputParameters = ReflectionHelper.CallMethod(_procWorker, innerMethod, queryParameters);
				_procWorker.cleanup();
				MakeRestTypes(outputParameters);
				return Serialize(outputParameters, false);
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
		public bool RunAsMain
		{
			get;set;
		}

		public bool IsReusable => throw new NotImplementedException();

		public virtual Task Delete(object key)
		{
			return Task.CompletedTask;
		}
		public virtual Task Put(object key)
		{
			return Task.CompletedTask;
		}
		public Dictionary<string, object> ReadRequestParameters(Stream stream)
		{
			var bodyParameters = new Dictionary<string, object>();
			using (StreamReader streamReader = new StreamReader(stream))
			{
				if (!streamReader.EndOfStream)
				{
					Jayrock.Json.JsonTextReader reader = new Jayrock.Json.JsonTextReader(streamReader);
					var data = reader.DeserializeNext();
					Jayrock.Json.JObject jobj = data as Jayrock.Json.JObject;
					if (jobj != null)
					{
						foreach (string name in jobj.Names)
						{
							bodyParameters.Add(name.ToLower(), jobj[name]);
						}
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
			return HttpHelper.SetResponseStatusAndJsonErrorAsync(_httpContext, code, message);
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
					String restServiceName = nspace + "." + assemblyName + "_services";
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
			return IsAuthenticated(_procWorker.IntegratedSecurityLevel2, _procWorker.IntegratedSecurityEnabled2, _procWorker.ExecutePermissionPrefix2);
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
				_httpContext.Response.Headers[header] =value;
			}
		}

		protected bool ProcessHeaders(string queryId)
		{
			var headers = GetHeaders();
			String language = null, etag = null;
			if (headers != null)
			{
				language = headers["GeneXus-Language"];
				etag = headers["If-Modified-Since"];
			}

			if (!string.IsNullOrEmpty(language))
				_gxContext.SetLanguage(language);

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
			var serializer = new Newtonsoft.Json.JsonSerializer();
			serializer.Converters.Add(new SDTConverter());
			TextWriter ms = new StringWriter();
			if (parameters.Count == 1 && !wrapped) //In Dataproviders, with one parameter BodyStyle is WebMessageBodyStyle.Bare, Both requests and responses are not wrapped.
			{
				string key = parameters.First().Key;
				using (var writer = new Newtonsoft.Json.JsonTextWriter(ms))
				{
					serializer.Serialize(writer, parameters[key]);
				}
			}
			else
			{
				using (var writer = new Newtonsoft.Json.JsonTextWriter(ms))
				{
					serializer.Serialize(writer, parameters);
				}
			}
			_httpContext.Response.Write(ms.ToString()); //Use intermediate StringWriter in order to avoid chunked response
			return Task.CompletedTask;
		}
		protected Task Serialize(object value)
		{
			var serializer = new Newtonsoft.Json.JsonSerializer();
			serializer.Converters.Add(new SDTConverter());
#if NETCORE
			var responseStream = _httpContext.Response.Body;
#else
			var responseStream = _httpContext.Response.OutputStream;
#endif
			using (var writer = new Newtonsoft.Json.JsonTextWriter(new StreamWriter(responseStream)))
			{
				serializer.Serialize(writer, value);
			}
			return Task.CompletedTask;
		}

		protected void Deserialize(string value, ref GxSilentTrnSdt sdt)
		{
			sdt.FromJSonString(value);
		}
		private static void MakeRestTypes(Dictionary<string, object> parameters)
		{
			foreach (var key in parameters.Keys.ToList())
			{
				parameters[key] = MakeRestType(parameters[key]);
			}
		}
		protected static object MakeRestType(object v)
		{
			Type vType = v.GetType();
			if (vType.IsConstructedGenericType && typeof(IGxCollection).IsAssignableFrom(vType)) //Collection<SDTType> convert to GxGenericCollection<SDTType_RESTInterface>
			{
				Type itemType = v.GetType().GetGenericArguments()[0];
				Type restItemType = ClassLoader.FindType(Config.CommonAssemblyName, itemType.FullName + "_RESTInterface", null);

				Type genericListItemType = typeof(GxGenericCollection<>).MakeGenericType(restItemType);
				return Activator.CreateInstance(genericListItemType, new object[] { v });
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
	}
}
