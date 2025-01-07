using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Data;
using GeneXus.Http;
using GeneXus.Metadata;
using GeneXus.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace GeneXus.Utils
{
	public class CustomActionFilter : IActionFilter
	{
		public void OnActionExecuted(ActionExecutedContext context)
		{
		}

		public void OnActionExecuting(ActionExecutingContext context)
		{
			(context.Controller as GxRestService).Initialize();
		}
	}
	[TypeFilter(typeof(CustomActionFilter))]
	public class GxRestService : ControllerBase
	{
        static readonly IGXLogger log = GXLoggerFactory.GetLogger<GxRestService>();

		internal const string WARNING_HEADER = "Warning";
		protected IGxContext context;
        protected string permissionPrefix;
		protected string permissionMethod;
        bool runAsMain = true;
		HttpStatusCode _statusCode = HttpStatusCode.OK;
		WrappedJsonError _errorDetail;

		protected GxRestService()
		{
			context = GxContext.CreateDefaultInstance();
		}
		[NonAction]
		internal void Initialize()
		{
			context.HttpContext = HttpContext;
			context.HttpContext.NewSessionCheck();
			ServiceHeaders();
		}

		protected void Cleanup()
        {
			if (runAsMain)
                context.CloseConnections();
        }
		protected bool RunAsMain
        {
            get { return runAsMain; }
            set { runAsMain = value; }
        }
		//Convert GxUnknownObjectCollection of Object[] to GxUnknownObjectCollection of GxSimpleCollections
		protected GxUnknownObjectCollection TableHashList(GxUnknownObjectCollection tableHashList)
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

		protected string EmptyParm(string parmValue)
		{
			if (string.IsNullOrEmpty(parmValue) || parmValue.Equals("gxempty", StringComparison.OrdinalIgnoreCase))
				return string.Empty;
			else
				return parmValue;
		}
		protected string RestStringParameter(string parameterName, string parameterValue)
        {
            try
			{
				if (HttpContext.Request.Query.TryGetValue(parameterName, out var value))
					return value.FirstOrDefault();
				else
					return parameterValue;
            }
            catch (Exception)
            {
                return parameterValue;
            }
        }
		protected bool RestParameter(string parameterName, string parameterValue)
		{
			try
			{
				if (HttpContext.Request.Query.TryGetValue(parameterName, out var value))
					return value.FirstOrDefault().Equals(parameterValue, StringComparison.OrdinalIgnoreCase);
				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}
		protected ActionResult UploadImpl()
		{
			GXObjectUploadServices gxobject = new GXObjectUploadServices(context);
			string fileGuid;
			string fileToken;
			using (Stream stream = Request.Body)
			{
				fileToken = gxobject.ReadFileFromStream(stream, Request.ContentType, Request.ContentLength.GetValueOrDefault(), Request.Headers[HttpHeader.XGXFILENAME], out fileGuid);
			}
			Response.Headers.Append(HttpHeader.GX_OBJECT_ID, fileGuid);
			SetStatusCode(HttpStatusCode.Created);
			return GetResponse(new {object_id = fileToken});
		}
		protected ActionResult ErrorCheck(IGxSilentTrn trn)
		{
			if (trn.Errors() == 1)
			{
				msglist msg = trn.GetMessages();
				if (msg.Count > 0)
				{
					msglistItem msgItem = (msglistItem)msg[0];
					if (msgItem.gxTpr_Id.Contains("NotFound"))
						_errorDetail = HandleError(HttpContext, HttpStatusCode.NotFound.ToString(HttpHelper.INT_FORMAT), msgItem.gxTpr_Description);
					else if (msgItem.gxTpr_Id.Contains("WasChanged"))
						_errorDetail = HandleError(HttpContext, HttpStatusCode.Conflict.ToString(HttpHelper.INT_FORMAT), msgItem.gxTpr_Description);
					else
						_errorDetail = HandleError(HttpContext, HttpStatusCode.BadRequest.ToString(HttpHelper.INT_FORMAT), msgItem.gxTpr_Description);
				}
			}
			if (_errorDetail == null)
				return NoContent();
			else
				return GetResponse(_errorDetail);

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
				AddHeader(WARNING_HEADER, StringUtil.Sanitize(header.ToString(), StringUtil.HttpHeaderWhiteList));
			}
		}
		protected void SetError(string code, string message)
		{
			HttpHelper.SetError(HttpContext, code, message);
		}
		protected ActionResult GetResponse(object data)
		{
			if (_statusCode != HttpStatusCode.OK)
				return StatusCode((int)_statusCode, data);
			else
				return Ok(data);
		}
		protected ActionResult EmptyResult()
		{
				return Ok();
		}
		protected ActionResult EmptyObjectResult()
		{
			return Ok(new { });
		}
		protected ActionResult NullResult()
		{
			return Ok("null");
		}
		protected ObjectResult HandleException(Exception ex)
		{
			GXLogging.Error(log, "Failed to complete execution of Rest Service:", ex);

			if (ex is FormatException || ex is NullReferenceException)
			{
				WrappedJsonError jsonError = HttpHelper.HandleUnexpectedError(HttpContext, HttpStatusCode.BadRequest, ex);
				return BadRequest(jsonError);
			}
			else
			{
				WrappedJsonError jsonError = HttpHelper.HandleUnexpectedError(HttpContext, HttpStatusCode.InternalServerError, ex);
				return StatusCode((int)HttpStatusCode.InternalServerError, jsonError);
			}
		}
		protected void WebException(Exception ex)
		{
            GXLogging.Error(log, "Failed to complete execution of Rest Service:", ex);

            if (ex is FormatException)
			{
				HttpHelper.SetUnexpectedError(HttpContext, HttpStatusCode.BadRequest, ex);
			}
			else
            {
				HttpHelper.SetUnexpectedError(HttpContext, HttpStatusCode.InternalServerError, ex);
			}
        }

		protected bool IsAuthenticated(string synchronizer)
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
					string assemblyName = synchronizer.ToLower();
					string restServiceName = nspace + "." + assemblyName + "_services";
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
					HttpHelper.SetError(HttpContext, "0", "Invalid Synchronizer " + synchronizer);
			}
		}
		protected bool IsAuthenticated()
		{
			if (!string.IsNullOrEmpty(permissionMethod))
			{
				GAMSecurityLevel securityLevel = ApiIntegratedSecurityLevel(permissionMethod);
				bool integratedSecurityEnabled = IntegratedSecurityEnabled && securityLevel != GAMSecurityLevel.SecurityNone;
				return IsAuthenticated(securityLevel, integratedSecurityEnabled, permissionPrefix);
			}
			else
			{
				return IsAuthenticated(IntegratedSecurityLevel, IntegratedSecurityEnabled, permissionPrefix);
			}
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
					_errorDetail = HandleError(HttpContext, "0", "This service needs an Authorization Header");
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
							_errorDetail = HandleGamError(HttpContext, result.Code, result.Description);
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
							_errorDetail = HandleGamError(HttpContext, result.Code, result.Description);
							if (sessionOk)
							{
								SetStatusCode(HttpStatusCode.Forbidden);
							}
							else
							{
								AddHeader(HttpHeader.AUTHENTICATE_HEADER, StringUtil.Sanitize(HttpHelper.OatuhUnauthorizedHeader(/*context.GetServerName()*/"SERVER", result.Code, result.Description), StringUtil.HttpHeaderWhiteList));
								SetStatusCode(HttpStatusCode.Unauthorized);
							}
							return false;
						}
					}
				}
				return true;
			}
		}
		internal WrappedJsonError HandleGamError(HttpContext httpContext, string code, string message, HttpStatusCode defaultCode = HttpStatusCode.Unauthorized)
		{
			HttpStatusCode httpStatusCode = HttpHelper.GamCodeToHttpStatus(code, defaultCode);
			SetErrorHeaders(httpContext, httpStatusCode, message);
			return HttpHelper.GetJsonError(code, message);
		}
		internal WrappedJsonError HandleError(HttpContext httpContext, string code, string message)
		{
			HttpStatusCode httpStatusCode = HttpHelper.MapStatusCode(code);
			SetErrorHeaders(httpContext, httpStatusCode, message);
			return HttpHelper.GetJsonError(code, message);
		}

		private void SetErrorHeaders(HttpContext httpContext, HttpStatusCode httpStatusCode, string message)
		{
			if (httpContext != null)
			{
				SetStatusCode(httpStatusCode);
				HttpHelper.HandleUnauthorized(httpStatusCode, httpContext);
				httpContext.SetReasonPhrase(message);
				GXLogging.Error(log, String.Format("ErrCode {0}, ErrDsc {1}", httpStatusCode, message));
			}
		}
		protected ObjectResult Unauthenticated(object data=null)
		{
			return StatusCode((int)_statusCode, _errorDetail);
		}

		protected void SetStatusCode(HttpStatusCode code)
        {
			if (code != 0)
			{
				_statusCode = code;
			}
        }
		IHeaderDictionary GetHeaders()
		{
			if (HttpContext != null)
			{
				return HttpContext.Request.Headers;
			}
			else return null;
		}
        string GetHeader(string header)
        {
            if (HttpContext != null)
            {
                return HttpContext.Request.Headers[header];
            }
            else return null;
        }
		bool IsPost()
		{
			if (HttpContext != null)
			{
				return HttpMethod.Post.Method == HttpContext.Request.GetMethod();
			}
			else return false;
		}
		void AddHeader(string header, string value)
        {
            if (HttpContext != null)
            {
                HttpContext.Response.Headers[header]= value;
            }
        }
		protected bool ProcessHeaders(string queryId)
		{
			
			IHeaderDictionary headers = GetHeaders();
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

			if (status == DataUpdateStatus.UpToDate)
			{
				SetStatusCode(HttpStatusCode.NotModified);
				return false;
			}
			return true;
		}
		protected StatusCodeResult GetNotModified()
		{
			return StatusCode((int)HttpStatusCode.NotModified);
		}
		private void SendCacheHeaders()
		{
			if (string.IsNullOrEmpty(context.GetHeader(HttpHeader.CACHE_CONTROL)))
				AddHeader("Cache-Control", HttpHelper.CACHE_CONTROL_HEADER_NO_CACHE);
		}
		private void ServiceHeaders()
		{
			SendCacheHeaders();
			if (HttpContext != null)
			{
				HttpHelper.CorsHeaders(HttpContext);
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
		protected virtual GAMSecurityLevel ApiIntegratedSecurityLevel(string gxMethod) { return IntegratedSecurityLevel; }
		protected virtual GAMSecurityLevel IntegratedSecurityLevel { get { return 0; } }
		protected virtual string ExecutePermissionPrefix { get { return string.Empty; } }
	}
}