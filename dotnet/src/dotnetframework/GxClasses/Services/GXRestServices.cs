using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Web;
using GeneXus.Application;
using System.Web;
using System.Net;
using GeneXus.Data.NTier;
using GeneXus.Data;
using System.Globalization;
using GeneXus.Http;
using GeneXus.Configuration;
using log4net;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Channels;
using System.Runtime.Serialization;

using System.Collections.Specialized;
using GeneXus.Security;
using GeneXus.Mime;
using System.Reflection;
using GeneXus.Metadata;
using System.Net.Http;
using System.IO;

namespace GeneXus.Utils
{
    public class CustomHttpBehaviorExtensionElement : System.ServiceModel.Configuration.BehaviorExtensionElement
    {
		protected override object CreateBehavior()
		{
			return new CustomHttpBehavior() {
				
			};

		}

		public override Type BehaviorType
        {
            get { return typeof(CustomHttpBehavior); }
        }
    }
    public class CustomHttpBehavior : WebHttpBehavior
    {
		protected override WebHttpDispatchOperationSelector GetOperationSelector(ServiceEndpoint endpoint)
		{
			return new CustomOperationSelector(endpoint);
		}
        protected override QueryStringConverter GetQueryStringConverter(OperationDescription operationDescription)
        {
            return new CustomQueryStringConverter(base.GetQueryStringConverter(operationDescription));
        }
        protected override void AddServerErrorHandlers(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Clear();
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(new JsonErrorHandler());
        }
    }
	class CustomOperationSelector : WebHttpDispatchOperationSelector
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(CustomOperationSelector));
		public CustomOperationSelector(ServiceEndpoint endpoint) : base(endpoint) { }
		protected override string SelectOperation(ref Message message, out bool uriMatched)
		{
			bool messageModified = false;
			string result = base.SelectOperation(ref message, out uriMatched);
			if (!uriMatched || string.IsNullOrEmpty(result) || result=="LoadDefault")//In the BC POST, uriMatched is true but result = string.Empty.
			{
				string address = message.Headers.To.AbsoluteUri;
				if (address.EndsWith(","))
				{
					address = address + "gxempty";
				}
				if (address.Contains("/,"))
				{
					address= address.Replace("/,", "/gxempty,");
				}
				while (address.Contains(",,"))
				{
					address = address.Replace(",,", ",gxempty,");
				}
				if (uriMatched && !string.IsNullOrEmpty(result))
				{
					message.Properties.Remove("UriTemplateMatchResults");
					messageModified = true;
				}
				if (message.Headers.To.AbsoluteUri != address)
				{
					message.Headers.To = new Uri(address);
					messageModified = true;
				}
				if (messageModified)
				{
					try
					{
						result = base.SelectOperation(ref message, out uriMatched);
					}catch(Exception ex)
					{
						GXLogging.Warn(log, ex, "Could not rewrite wcf message to ", address);
					}
				}
			}
			return result;
		}
    }
    public class JsonErrorHandler : IErrorHandler
    {
        #region IErrorHandler Members
        public bool HandleError(Exception error)
        {
            return true;
        }

		public void ProvideFault(Exception error, MessageVersion version,
		  ref Message fault)
		{
			fault = this.GetJsonFaultMessage(version, error);

			var wcfcontext = WebOperationContext.Current;
			if (wcfcontext != null)
			{
				wcfcontext.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
			}
			this.ApplyJsonSettings(ref fault);
		}
        #endregion

        #region Protected Method(s)
        protected virtual void ApplyJsonSettings(ref Message fault)
        {
            var jsonFormatting = new WebBodyFormatMessageProperty(WebContentFormat.Json);
            fault.Properties.Add(WebBodyFormatMessageProperty.Name, jsonFormatting);
        }

        protected virtual Message GetJsonFaultMessage(MessageVersion version, Exception error)
        {
            JsonFault detail = null;
            var knownTypes = new List<Type>();
            string code = "500"; //Internal server error
            if ((error is FaultException) && (error.GetType().GetProperty("Detail") != null))
            {
                detail = (error.GetType().GetProperty("Detail").GetGetMethod().Invoke(error, null) as JsonFault);
                knownTypes.Add(detail.GetType());
                code = detail.error.Code;
            }
            JsonFault jsonFault = new JsonFault(error.Message, code);
#pragma warning disable SCS0028 // Unsafe deserialization possible from {1} argument passed to '{0}'
            var faultMessage = Message.CreateMessage(version, "", jsonFault,new DataContractJsonSerializer(jsonFault.GetType(), knownTypes));
#pragma warning restore SCS0028 // Unsafe deserialization possible from {1} argument passed to '{0}'
            return faultMessage;
        }
        #endregion
    }

    [DataContract]
    public class JsonFault
    {
        public JsonFault(string msg, string cod)
        {
            error = new JsonFaultDetail { Message = msg, Code = cod };
        }
        [DataMember(Name = "error")]
        public JsonFaultDetail error;

        [DataContract]
        public class JsonFaultDetail
        {
            [DataMember(Name = "message")]
            public string Message { get; set; }
            [DataMember(Name = "code")]
            public string Code { get; set; }
        }
    }
    public class CustomQueryStringConverter : QueryStringConverter
    {
        QueryStringConverter originalConverter;
        public CustomQueryStringConverter(QueryStringConverter originalConverter)
        {
            this.originalConverter = originalConverter;
        }
        public override object ConvertStringToValue(string parameter, Type parameterType)
        {
            try
            {
                return originalConverter.ConvertStringToValue(parameter, parameterType);
            }
            catch (FormatException ex)
            {
				if ((string.IsNullOrEmpty(parameter) || parameter.ToLower().Equals("null")) && 
					(parameterType == typeof(int) || parameterType == typeof(short) || parameterType == typeof(long)))
                    return originalConverter.ConvertStringToValue("0", parameterType);
                else throw ex;
            }
        }
    }
    public class GxRestService : System.Web.SessionState.IRequiresSessionState
	{
        static readonly ILog log = log4net.LogManager.GetLogger(typeof(GxRestService));
		internal const string WARNING_HEADER = "Warning";
		protected IGxContext context;
        private HttpContext httpContext;
        private WebOperationContext wcfContext;
        protected string permissionPrefix;
        bool runAsMain = true;

        public GxRestService()
        {
			context = GxContext.CreateDefaultInstance();
			wcfContext = WebOperationContext.Current;
            httpContext = HttpContext.Current;
			if (GXUtil.CompressResponse())
                GXUtil.SetGZip(httpContext);
        }
        public void Cleanup()
        {
            if (runAsMain)
                context.CloseConnections();
        }
        public bool RunAsMain
        {
            get { return runAsMain; }
            set { runAsMain = value; }
        }
        //Convert GxUnknownObjectCollection of Object[] to GxUnknownObjectCollection of GxSimpleCollections
        public GxUnknownObjectCollection TableHashList(GxUnknownObjectCollection tableHashList)
        {
			GxUnknownObjectCollection result = new GxUnknownObjectCollection();
			if (tableHashList != null && tableHashList.Count > 0)
			{
				foreach (object[] list in tableHashList)
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

		public string EmptyParm(string parmValue)
		{
			if (string.IsNullOrEmpty(parmValue) || parmValue.Equals("gxempty", StringComparison.OrdinalIgnoreCase))
				return string.Empty;
			else
				return parmValue;
		}
        public string RestStringParameter(string parameterName, string parameterValue)
        {
            try
            {
                if (WebOperationContext.Current != null)
                {
                    string pValue = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters.Get(parameterName);
                    if (pValue != null)
                    {
                        return pValue;
                    }
                }
                return parameterValue;
            }
            catch (Exception)
            {
                return parameterValue;
            }
        }
		public bool RestParameter(string parameterName, string parameterValue)
		{
			try
			{
                if (WebOperationContext.Current != null)
                {
                    string pValue = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters.Get(parameterName);
                    if (pValue != null)
                    {
                        return pValue.Equals(parameterValue, StringComparison.OrdinalIgnoreCase);
                    }
                }
                return false;
			}
			catch (Exception)
			{
				return false;
			}
		}
		[Obsolete("UploadImpl without arguments is deprecated", false)]
		public void UploadImpl()
		{
			GXObjectUploadServices gxobject = new GXObjectUploadServices(context);
			gxobject.webExecute();
		}
		public void UploadImpl(Stream stream)
		{
			GXObjectUploadServices gxobject = new GXObjectUploadServices(context);
			gxobject.WcfExecute(stream, WebOperationContext.Current.IncomingRequest.ContentType);
		}
		public void ErrorCheck(IGxSilentTrn trn)
		{
			if (trn.Errors() == 1)
			{
				msglist msg = trn.GetMessages();
				if (msg.Count > 0)
				{
					msglistItem msgItem = (msglistItem)msg[0];
					if (msgItem.gxTpr_Id.Contains("NotFound"))
						SetError("404", msgItem.gxTpr_Description);
					else if (msgItem.gxTpr_Id.Contains("WasChanged"))
						SetError("409", msgItem.gxTpr_Description);
					else
						SetError("400", msgItem.gxTpr_Description);
				}
			}

		}
		protected void SetMessages(msglist messages)
		{
			StringBuilder header = new StringBuilder();
			bool emptyHeader = true, encoded=false;
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

					header.AppendFormat("{0}299 {1} \"{2}{3}:{4}\"", emptyHeader ? string.Empty:",", context.GetServerName(), encoded ? GxRestPrefix.ENCODED_PREFIX : string.Empty, typeMsg, value);
					if (emptyHeader)
					{
						emptyHeader = false;
					}
				}
			}
			if (!emptyHeader)
			{
				AddHeader(WARNING_HEADER, header.ToString());
			}
		}
		public void SetError(string code, string message)
		{
			HttpHelper.SetResponseStatusAndJsonError(httpContext, code, message);
		}
        public void WebException(Exception ex)
		{
            GXLogging.Error(log, "Failed to complete execution of Rest Service:", ex);

            if (ex is FaultException<JsonFault>)
            {
                throw ex;
            }
            else
            {
                SetError("500", ex.Message);
            }
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
					GxRestService synchronizerService = (GxRestService)ClassLoader.GetInstance(assemblyName, restServiceName, null);
					if (synchronizerService!=null && synchronizerService.IsSynchronizer)
					{
						validSynchronizer = true;
						return IsAuthenticated(synchronizerService.IntegratedSecurityLevel, synchronizerService.IntegratedSecurityEnabled, synchronizerService.ExecutePermissionPrefix);
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
			return IsAuthenticated(IntegratedSecurityLevel, IntegratedSecurityEnabled, permissionPrefix);
		}
		private bool IsAuthenticated(GAMSecurityLevel objIntegratedSecurityLevel, bool objIntegratedSecurityEnabled, string objPermissionPrefix)
		{
			if (!objIntegratedSecurityEnabled)
			{
				return true;
			}
			else {
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
						GxResult result = GxSecurityProvider.Provider.checkaccesstoken(context, token, out isOK);
						if (!isOK)
						{
							SetError(result.Code, result.Description);
							return false;
						}
					}
					else if (objIntegratedSecurityLevel == GAMSecurityLevel.SecurityHigh)
					{
						bool sessionOk, permissionOk;
						GxResult result = GxSecurityProvider.Provider.checkaccesstokenprm(context, token, objPermissionPrefix, out sessionOk, out permissionOk);
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
								AddHeader(HttpHeader.AUTHENTICATE_HEADER, HttpHelper.OatuhUnauthorizedHeader(context.GetServerName(), result.Code, result.Description));
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
            if (wcfContext != null)
            {
                wcfContext.OutgoingResponse.StatusCode = code;
            }
            else if (httpContext != null)
            {
                httpContext.Response.StatusCode = (int)code;
            }
        }
		NameValueCollection GetHeaders()
		{
			if (wcfContext != null)
			{
				return wcfContext.IncomingRequest.Headers;
			}
			else if (httpContext != null)
			{
				return httpContext.Request.Headers;
			}
			else return null;
		}
        string GetHeader(string header)
        {
            if (wcfContext != null)
            {
                return wcfContext.IncomingRequest.Headers[header];
            }
            else if (httpContext != null)
            {
                return httpContext.Request.Headers[header];
            }
            else return null;
        }
		bool IsPost()
		{
			if (wcfContext != null)
			{
				return HttpMethod.Post.Method == wcfContext.IncomingRequest.Method;
			}
			else if (httpContext != null)
			{
				return HttpMethod.Post.Method == httpContext.Request.HttpMethod;
			}
			else return false;
		}
		void AddHeader(string header, string value)
        {
            if (wcfContext != null)
            {
                wcfContext.OutgoingResponse.Headers.Add(header, value);
            }
            else if (httpContext != null)
            {
                httpContext.Response.AddHeader(header, value);
            }
        }
		public bool ProcessHeaders(string queryId)
		{

			NameValueCollection headers = GetHeaders();
			String language = null, theme = null, etag = null;
			if (headers != null)
			{
				language = headers["GeneXus-Language"];
				theme = headers["GeneXus-Theme"];
				if (!IsPost())
					etag = headers["If-Modified-Since"];
			}

			if (!string.IsNullOrEmpty(language))
				context.SetLanguage(language);

			if (!string.IsNullOrEmpty(theme))
				context.SetTheme(theme);

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
			AddCacheHeaders();

			if (status == DataUpdateStatus.UpToDate)
			{
				SetStatusCode(HttpStatusCode.NotModified);
				return false;
			}
			return true;
		}

		private void AddCacheHeaders()
		{
			// WebPlatform Only
			if ((int)GX.ClientInformation.DeviceTypeEnum.Web == GX.ClientInformation.DeviceType)
			{
				/*
				* https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cache-Control
				* Specifying no-cache or max-age=0 indicates that 
				* clients can cache a resource and must revalidate each time before using it. 
				* This means HTTP request occurs each time, but it can skip downloading HTTP body if the content is valid.
				*/
				AddHeader("Cache-Control", "no-cache, max-age=0");
			}
		}

		DateTime HTMLDateToDatetime(string s)
        {
            // Date Format: RFC 1123
            DateTime dt;
            if (DateTime.TryParse(s, DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.AdjustToUniversal, out dt))
                return dt;
            return DateTime.MinValue;
        }
        string dateTimeToHTMLDate(DateTime dt)
        {
            return dt.ToUniversalTime().ToString(DateTimeFormatInfo.InvariantInfo.RFC1123Pattern, DateTimeFormatInfo.InvariantInfo);
        }
		protected virtual bool IsSynchronizer { get { return false; } }
		protected virtual bool IntegratedSecurityEnabled { get { return false; } }
		protected virtual GAMSecurityLevel IntegratedSecurityLevel { get { return 0; } }
		protected virtual string ExecutePermissionPrefix { get { return string.Empty; } }
	}
}