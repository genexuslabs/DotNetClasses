namespace GeneXus.Http
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;

	using GeneXus.Application;
	using GeneXus.Configuration;
	using GeneXus.Encryption;
	using GeneXus.Metadata;
	using GeneXus.Mime;
	using GeneXus.Security;
	using GeneXus.Utils;
	using GeneXus.XML;
	using GeneXus.WebControls;
#if !NETCORE
	using Jayrock.Json;
#endif
	using Helpers;
	using System.Collections.Concurrent;
	using System.Net.Http;
#if NETCORE
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Http.Extensions;
	using System.Net;
	using GeneXus.Web.Security;
	using System.Linq;
	using System.Reflection.PortableExecutable;
	using System.Web;
#else
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Net;
	using GeneXus.Notifications;
	using Web.Security;
	using System.Web.SessionState;
	using GeneXus.Data.NTier;
	using System.Security;



#endif
	using System.Threading.Tasks;
#if NETCORE
	public abstract class GXHttpHandler : GXBaseObject, IHttpHandler
#else
	public abstract class GXHttpHandler : WebControl, IHttpHandler
#endif
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<GXHttpHandler>();

		internal const string GX_AJAX_REQUEST_HEADER = "GxAjaxRequest";
		internal const string GX_SPA_GXOBJECT_RESPONSE_HEADER = "X-GXOBJECT";
		internal const string GX_SPA_MASTERPAGE_HEADER = "X-SPA-MP";
		internal const string GX_AJAX_MULTIPART_ID = "GXAjaxMultipart";
		private string ThemekbPrefix;
		private string ThemestyleSheet;
		private string ThemeurlBuildNumber;
		private const string GX_FULL_AJAX_REQUEST_HEADER = "X-FULL-AJAX-REQUEST";
		private const string GXEVENT_PARM = "gxevent";
		private const string URI_SEPARATOR = "/";
		private static Regex MULTIMEDIA_GXI_GRID_PATTERN = new Regex("(\\w+)(_\\d{4})$", RegexOptions.Compiled);
		private const int SPA_NOT_SUPPORTED_STATUS_CODE = 530;
		protected bool FullAjaxMode;
		private Exception workerException;
		private bool firstParConsumed = false;
		private StringDictionary customCSSContent = new StringDictionary();
#if !NETCORE
		[System.Diagnostics.CodeAnalysis.SuppressMessage("GxFxCopRules", "CR1000:EnforceThreadSafeType")]
		private Dictionary<string, string> callTargetsByObject = new Dictionary<string, string>();
#endif
		public GXHttpHandler()
		{
			initpars();
		}

		public virtual void SetPrefix(string s)
		{
		}

		public virtual bool IsMasterPage()
		{
			return false;
		}

		public object dyncall(String MethodName)
		{
			ParameterInfo[] pars = this.GetType().GetMethod(MethodName).GetParameters();
			int ParmsCount = pars.Length;
			object[] convertedparms = new object[ParmsCount];
			for (int i = 0; i < ParmsCount; i++)
				convertedparms[i] = convertparm(pars, i, GetNextPar());
#if NETCORE
			context.ResponseContentType(MediaTypesNames.ApplicationJson);
#endif
			return this.GetType().GetMethod(MethodName).Invoke(this, convertedparms);
		}
		protected object convertparm(ParameterInfo[] pars, int i, object value)
		{
			Type parmtype = pars[i].ParameterType.GetElementType();
			string valueS = value.ToString();

			try
			{
				if (parmtype == null)
					parmtype = pars[i].ParameterType;

				if (parmtype.Equals(typeof(Guid)))
				{
					if (valueS.Equals("{}"))
						return Guid.Empty;
					else
						return new Guid(valueS);
				}

				if (parmtype.Equals(typeof(GeneXus.Utils.Geospatial)))
				{
					return new GeneXus.Utils.Geospatial(valueS);
				}

				if (parmtype.Equals(typeof(DateTime)))
				{
					return context.localUtil.ParseDateOrDTimeParm(valueS);
				}
				if (typeof(IGxGenericCollectionItem).IsAssignableFrom(parmtype))
				{
#if NETCORE
					Type baseType = parmtype.GetTypeInfo().BaseType;
					bool isGenericType = baseType != null && baseType.IsGenericParameter;
#else
					Type baseType = parmtype.BaseType;
					bool isGenericType = baseType != null && baseType.IsGenericType;
#endif
					if (isGenericType)
					{
						IGxGenericCollectionItem sdtRest = (IGxGenericCollectionItem)Activator.CreateInstance(parmtype);
						Type parmSdtType = baseType.GetGenericArguments()[0];
						object parmSdtObj = null;
						if (typeof(GxUserType).IsAssignableFrom(parmSdtType))
						{
							parmSdtObj = Activator.CreateInstance(parmSdtType, context);
						}
						else
						{
							parmSdtObj = Activator.CreateInstance(parmSdtType);
						}
					((IGxJSONSerializable)parmSdtObj).FromJSonString(valueS);
						sdtRest.Sdt = (GxUserType)parmSdtObj;
						return sdtRest;
					}
				}
				if (typeof(IGxJSONSerializable).IsAssignableFrom(parmtype))
				{
					object parmObj = null;
					if (typeof(GxUserType).IsAssignableFrom(parmtype))
					{
						parmObj = Activator.CreateInstance(parmtype, context);
					}
					else
					{
						parmObj = Activator.CreateInstance(parmtype);
					}
					((IGxJSONSerializable)parmObj).FromJSonString(valueS);
					return parmObj;
				}

				if (parmtype.Equals(typeof(int)) && value != null)
				{
					if (string.Compare(valueS, "true", StringComparison.OrdinalIgnoreCase) == 0)
						return 1;
					if (string.Compare(valueS, "false", StringComparison.OrdinalIgnoreCase) == 0)
						return 0;
				}

				if (IsNumericType(parmtype) && String.IsNullOrEmpty(valueS.ToString()))
				{
					valueS = "0";
				}

				//Parameters in URL are always in Invariant Format
				return Convert.ChangeType(value, parmtype, CultureInfo.InvariantCulture);
			}
			catch (Exception e)
			{
				throw new Exception($"Invalid Parameter Type, value '{valueS}', type {parmtype.FullName}, Exception {e.ToString()}");
			}
		}

		public virtual object getParm(object[] parms, int index)
		{
			return parms[index];
		}

		private static bool IsNumericType(Type t)
		{
			switch (Type.GetTypeCode(t))
			{
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Decimal:
				case TypeCode.Double:
					return true;
				default:
					return false;
			}
		}

#if !NETCORE
		protected IGxContext _Context;
		bool _isMain;
#endif
		bool _isStatic;
		string staticContentBase;

		ConcurrentDictionary<string, string> _namedParms = new ConcurrentDictionary<string, string>();
		bool useOldQueryStringFormat;
		public List<string> _params = new List<string>();
		private string _strParms;
		int _currParameter;
#if NETCORE
		private GXWebRow _currentGridRow;
		private Dictionary<string,string> EventsMetadata = new Dictionary<string, string>();
#else
		private Hashtable EventsMetadata = new Hashtable();
#endif

#if NETCORE
		protected void setEventMetadata(string EventName, string Metadata)
		{
			if (EventsMetadata.ContainsKey(EventName))
				EventsMetadata[EventName] += Metadata;
			else
				EventsMetadata[EventName] = Metadata;
		}
		internal async Task WebExecuteExAsync(HttpContext httpContext)
		{
			if (IsUploadRequest(httpContext))
				new GXObjectUploadServices(context).webExecute();
			else if (IsFullAjaxRequest(httpContext))
				await WebAjaxEventAsync();
			else
				await WebExecuteAsync();
		}
#else
		protected void setEventMetadata(string EventName, string Metadata)
		{
			if (EventsMetadata[EventName] == null)
				EventsMetadata[EventName] = string.Empty;
			EventsMetadata[EventName] += Metadata;
		}
#endif
		public void webExecuteEx(HttpContext httpContext)
		{
			if (IsUploadRequest(httpContext))
				new GXObjectUploadServices(context).webExecute();
			else  if (IsFullAjaxRequest(httpContext))
				webAjaxEvent();
			else
				webExecute();
		}

		private bool IsUploadRequest(HttpContext httpContext)
		{
			if (UploadEnabled())
			{
				return httpContext.Request.GetRawUrl().EndsWith(HttpHelper.GXOBJECT, StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		private bool IsFullAjaxRequest(HttpContext httpContext)
		{
#if NETCORE
			String contentType = (localHttpContext != null && localHttpContext.Request.ContentType != null) ? localHttpContext.Request.ContentType : string.Empty;
#else
			String contentType = httpContext != null ? httpContext.Request.ContentType : string.Empty;
#endif
			bool supportAjaxEvent = SupportAjaxEvent() || httpContext.Request.Headers[GX_FULL_AJAX_REQUEST_HEADER] == "1";
			bool fullAjaxRequest = IsGxAjaxRequest() && supportAjaxEvent && contentType.Contains(MediaTypesNames.ApplicationJson);
			fullAjaxRequest = fullAjaxRequest || (context.IsMultipartRequest && !string.IsNullOrEmpty(cgiGet(GX_AJAX_MULTIPART_ID)));
			return fullAjaxRequest;
		}

		public virtual void InitializeDynEvents() { throw new Exception("The method or operation is not implemented."); }
		public virtual void initialize_properties() { throw new Exception("The method or operation is not implemented."); }
		public virtual void webExecute() { throw new Exception("The method or operation is not implemented."); }
		protected virtual Task WebExecuteAsync()
		{
			GXLogging.Warn(log, this.GetType().FullName + " not generated as async service");
			webExecute();
			return Task.CompletedTask;
		}

#if !NETCORE
		public virtual void initialize() { throw new Exception("The method or operation is not implemented."); }
		public virtual void cleanup() { }
		virtual public bool UploadEnabled() { return false; }

		protected virtual void ExecuteEx()
		{
			ExecutePrivate();
		}
		protected virtual void ExecutePrivate()
		{

		}
		protected virtual void ExecutePrivateCatch(object stateInfo)
		{
			try
			{
				((GXHttpHandler)stateInfo).ExecutePrivate();
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
			this.context = submitContext;
			initialize();
			Submit(ExecutePrivateCatch, this);
		}
		protected virtual void CloseCursors()
		{

		}
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
		protected virtual string[] GetParameters()
		{
			return null;
		}
#endif
		public virtual bool SupportAjaxEvent() { return false; }
		public virtual String AjaxOnSessionTimeout() { return "Ignore"; }
#if NETCORE
		public void DoAjaxLoad(int SId, GXWebRow row)
		{
			JObject JSONRow = new JObject();
			JSONRow.Put("grid", SId);
			JSONRow.Put("props", row.parentGrid.GetJSONObject());
			JSONRow.Put("values", row.parentGrid.GetValues());
			context.httpAjaxContext.appendLoadData(SId, JSONRow);
		}
		public void ajax_sending_grid_row(GXWebRow row)
		{
			if (context.isAjaxCallMode())
			{
				_currentGridRow = row;
			}
			else
			{
				_currentGridRow = null;
			}
		}
#endif
		public void ajax_rsp_clear()
		{
			_Context.ajax_rsp_clear();
		}

		public class DataIntegrityException : Exception
		{
			public DataIntegrityException(string message) : base(message)
			{
			}
		}

		internal class DynAjaxEvent
		{
			JArray inParmsValues;
			JArray inHashValues;
			JArray events;
			GXHttpHandler targetObj;
			string[] eventHandlers;
			bool[] eventUseInternalParms;
			string cmpContext = string.Empty;
			int grid;
			string row, pRow = string.Empty;
			bool anyError;
			internal IDynAjaxEventContext DynAjaxEventContext;

			internal DynAjaxEvent(IDynAjaxEventContext DynAjaxEventContext)
			{
				this.DynAjaxEventContext = DynAjaxEventContext;
			}

			private void ParseInputJSonMessage(JObject objMessage, GXHttpHandler targetObj)
			{
				inParmsValues = (JArray)objMessage["parms"];
				inHashValues = (JArray)objMessage["hsh"];
				if (inHashValues == null)
				{
					inHashValues = new JArray();
				}
				events = (JArray)objMessage["events"];
				cmpContext = (string)objMessage["cmpCtx"];
				this.targetObj = targetObj;
				if ((bool)objMessage["MPage"])
				{
					if (objMessage.Contains("objClass"))
					{
						string nspace;
						if (!Config.GetValueOf("AppMainNamespace", out nspace))
							nspace = "GeneXus.Programs";
						nspace = (!string.IsNullOrEmpty((string)objMessage["pkgName"])) ? (string)objMessage["pkgName"] : nspace;
						this.targetObj = (GXHttpHandler)ClassLoader.GetInstance((string)objMessage["objClass"], nspace + "." + (string)objMessage["objClass"], new Object[] { targetObj.context });
					}
				}
				else
				{
					if (!String.IsNullOrEmpty(cmpContext) && objMessage.Contains("objClass"))
					{
						string nspace = (!string.IsNullOrEmpty((string)objMessage["pkgName"])) ? (string)objMessage["pkgName"] : "GeneXus.Programs";
						GXWebComponent webComponent = getWebComponent(GetType(), nspace, (string)objMessage["objClass"], new Object[] { targetObj.context });
						webComponent.ComponentInit();
						this.targetObj = webComponent;
						if (this.targetObj == null)
						{
							GXLogging.Error(log, String.Format("Could not load target WebComponent (fullName '{0}.{1}') for FullAjax Event", nspace, (string)objMessage["objClass"]));
						}
					}
				}
				if (objMessage.Contains("grids"))
					ParseGridsDataParms((JObject)objMessage["grids"]);
				if (objMessage.Contains("grid"))
					grid = Convert.ToInt32(objMessage["grid"]);
				else
					grid = 0;
				if (objMessage.Contains("row"))
					row = (string)objMessage["row"];
				else
					row = string.Empty;
				if (objMessage.Contains("pRow"))
					pRow = (string)objMessage["pRow"];
				if (objMessage.Contains("gxstate"))
				{
					ParseGXStateParms((JObject)objMessage["gxstate"]);
				}
				if (objMessage.Contains("fullPost"))
				{
					this.targetObj._Context.httpAjaxContext.ParseGXState((JObject)objMessage["fullPost"]);
				}
			}
			private void ParseGridsDataParms(JObject gxGrids)
			{
				foreach (String gridName in gxGrids.Names)
				{
					JObject grid = (JObject)gxGrids[gridName];
					if (int.Parse(grid["id"].ToString()) != 0 && !String.IsNullOrEmpty(grid["lastRow"].ToString()))
					{
						int lastRow = int.Parse(grid["lastRow"].ToString()) + 1;
						SetFieldValue("sGXsfl_" + grid["id"].ToString() + "_idx", lastRow.ToString().PadLeft(4, '0'));
						SetFieldValue("nGXsfl_" + grid["id"].ToString() + "_idx", lastRow.ToString());
					}
				}

			}
			private void ParseGXStateParms(JObject gxState)
			{
				foreach (string item in gxState.Names)
				{
					string value = gxState[item].ToString();
					if (!string.IsNullOrEmpty(value))
					{
						this.targetObj.FormVars.Set(item, value);
					}
				}
			}

			private string BuildOutputJSonMessage()
			{
				return ((GxContext)(targetObj._Context)).getJSONResponse(this.cmpContext);
			}

			private bool IsInternalParm(JObject parm)
			{
				return parm.Contains("sPrefix") || parm.Contains("sSFPrefix") || parm.Contains("sCompEvt");
			}

			private void AddParmsMetadata(JObject inputParm, JArray ParmsList, HashSet<string> ParmsListHash)
			{
				string key = string.Empty;

				if (inputParm.Contains("av") && inputParm.Contains("ctrl") && inputParm.Contains("prop"))
				{
					key = (string)inputParm["av"] + (string)inputParm["ctrl"] + (string)inputParm["prop"];
				}
				else if (inputParm.Contains("av"))
				{
					key = (string)inputParm["av"];
				}
				else if (inputParm.Contains("ctrl") && inputParm.Contains("prop"))
				{
					key = (string)inputParm["ctrl"] + (string)inputParm["prop"];
				}
				else if (inputParm.Contains("ctrl"))
				{
					key = (string)inputParm["ctrl"];
				}

				if (String.IsNullOrEmpty(key) || !ParmsListHash.Contains(key))
				{
					ParmsList.Add(inputParm);
					if (!String.IsNullOrEmpty(key))
					{
						ParmsListHash.Add(key);
					}
				}
			}

			private void ParseMetadata()
			{
				try
				{
					DynAjaxEventContext.ClearParmsMetadata();
					eventHandlers = new string[events.Length];
					eventUseInternalParms = new bool[events.Length];
					int eventCount = 0;
					foreach (string eventName in events)
					{
#if NETCORE

						JObject eventMetadata = JSONHelper.ReadJSON<JObject>(targetObj.EventsMetadata[eventName.ToString()]);
#else
						JObject eventMetadata = JSONHelper.ReadJSON<JObject>((string)targetObj.EventsMetadata[eventName.ToString()]);
#endif
						if (eventMetadata != null)
						{
							eventHandlers[eventCount] = (string)eventMetadata["handler"];
							JArray eventInputParms = (JArray)eventMetadata["iparms"];
							foreach (JObject inputParm in eventInputParms)
							{
								AddParmsMetadata(inputParm, DynAjaxEventContext.inParmsMetadata, DynAjaxEventContext.inParmsMetadataHash);
								eventUseInternalParms[eventCount] = eventUseInternalParms[eventCount] || IsInternalParm(inputParm);
							}
							JArray eventOutputParms = (JArray)eventMetadata["oparms"];
							if (eventOutputParms != null)
							{
								foreach (JObject outputParm in eventOutputParms)
								{
									AddParmsMetadata(outputParm, DynAjaxEventContext.outParmsMetadata, DynAjaxEventContext.outParmsMetadataHash);
								}
							}
						}
						else
						{
							GXLogging.Warn(log, "The event ", eventName, " is not implemented");
							eventHandlers[eventCount] = string.Empty;
						}
						eventCount++;
					}
				}
				catch (Exception ex)
				{
					anyError = true;
					GXLogging.Error(log, "Failed to parse event metadata");
					GXLogging.Error(log, ex.ToString());
					GXLogging.Error(log, ex.StackTrace);
				}
			}

			private FieldInfo getfieldInfo(object targetObj, string fieldName)
			{
				return targetObj.GetType().GetField((string)fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			}

			private PropertyInfo getpropertyInfo(object targetObj, string fieldName, ref object target)
			{
				PropertyInfo propertyInfo = null;
				object ret = targetObj;
				string[] split = fieldName.Split(new Char[] { '.' });
				foreach (string field in split)
				{
					FieldInfo fieldInfo = getfieldInfo(ret, field);
					propertyInfo = ret.GetType().GetProperty(field);
					target = ret;
					if (fieldInfo != null)
						ret = fieldInfo.GetValue(ret);
					else
					{
						if (propertyInfo != null)
							ret = propertyInfo.GetValue(ret, null);
					}
				}
				return propertyInfo;
			}

			private void SetNullableScalarOrCollectionValue(JObject parm, object value, JArray columnValues)
			{
				string nullableAttribute = parm.Contains("nullAv") ? (string)parm["nullAv"] : null;
				if (nullableAttribute != null && string.IsNullOrEmpty(JSONHelper.WriteJSON<dynamic>(value)))
				{
					SetScalarOrCollectionValue(nullableAttribute, true, null);
				}
				else
				{
					SetScalarOrCollectionValue((string)parm["av"], value, columnValues);
				}
			}

			private void SetScalarOrCollectionValue(string fieldName, object value, JArray values)
			{
				FieldInfo fieldInfo = getfieldInfo(targetObj, fieldName);
				if (fieldInfo != null)
				{
					if (typeof(IGxCollection).IsAssignableFrom(fieldInfo.FieldType)) 
						SetCollectionFieldValue(fieldInfo, values);
					else
						SetFieldValue(fieldInfo, value);
				}
				else
				{
					object target = null;
					PropertyInfo propertyInfo = getpropertyInfo(targetObj, fieldName, ref target);
					SetPropertyValue(target, propertyInfo, value);
				}
			}
			private object getFieldValue(object targetObj, string fieldName)
			{
				FieldInfo fieldInfo = getfieldInfo(targetObj, fieldName);
				if (fieldInfo != null)
				{
					return fieldInfo.GetValue(targetObj);
				}
				else
				{
					object target = null;
					PropertyInfo propertyInfo = getpropertyInfo(targetObj, fieldName, ref target);
					return GetPropertyValue(target, propertyInfo);
				}
			}

			private void SetCollectionFieldValue(FieldInfo fieldInfo, JArray values)
			{
				if (fieldInfo != null)
				{

					MethodInfo mth = fieldInfo.FieldType.GetMethod("FromJSONObject");
					if (mth != null)
						mth.Invoke(fieldInfo.GetValue(targetObj), new Object[] { values });
				}
			}

			private void SetFieldValue(string fieldName, object value)
			{
				FieldInfo fieldInfo = targetObj.GetType().GetField((string)fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
				SetFieldValue(fieldInfo, value);
			}

			private void SetPropertyValue(object target, PropertyInfo propertyInfo, object value)
			{
				if (propertyInfo != null)
				{
					if (propertyInfo.PropertyType == typeof(DateTime) && value is String)
					{
						value = targetObj._Context.localUtil.CToT(value.ToString(), 0, 0);
					}
					else if (propertyInfo.PropertyType == typeof(System.Guid))
					{
						value = new Guid(value.ToString());
					}
					else if (propertyInfo.PropertyType == typeof(GeneXus.Utils.Geospatial))
					{
						value = new GeneXus.Utils.Geospatial(value.ToString());
					}
					else
					{
						value = Convert.ChangeType(value, propertyInfo.PropertyType);
					}
					propertyInfo.SetValue(target, value, null);
				}
			}
			private object GetPropertyValue(object target, PropertyInfo propertyInfo)
			{
				return (propertyInfo != null) ? propertyInfo.GetValue(target, null) : null;
			}

			private void SetFieldValue(FieldInfo fieldInfo, object value)
			{
				if (fieldInfo != null)
				{
					MethodInfo mth = fieldInfo.FieldType.GetMethod("FromJSONObject");
					if (mth != null)
						mth.Invoke(fieldInfo.GetValue(targetObj), new Object[] { value });

					else
					{
						if (fieldInfo.FieldType.IsArray)
						{
							Array tempArray = GetArrayFieldValue(fieldInfo, value);
							if (tempArray != null)
								value = tempArray;
						}
						else if (fieldInfo.FieldType == typeof(DateTime) && value is String)
						{
							value = targetObj._Context.localUtil.CToT(value.ToString(), 0, 0);
						}
						else if (fieldInfo.FieldType == typeof(System.Guid))
						{
							value = new Guid(value.ToString());
						}
						else if (fieldInfo.FieldType == typeof(GeneXus.Utils.Geospatial))
						{
							value = new GeneXus.Utils.Geospatial(value.ToString());
						}
						if (fieldInfo.FieldType == typeof(Boolean))
						{
							Boolean val = false;
							if (!Boolean.TryParse(value.ToString(), out val))
							{
								GXLogging.Error(log, $"Could not parse boolean value '{value.ToString()}'");
							}
							value = val;
						}
						else
						{
#if NETCORE
							IFormatProvider provider = CultureInfo.InvariantCulture;
#else
							IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-US");
#endif
							value = Convert.ChangeType(value, fieldInfo.FieldType, provider);
						}
						fieldInfo.SetValue(targetObj, value);
					}
				}
			}

			private Array GetArrayFieldValue(FieldInfo fieldInfo, object value)
			{
				JArray jArray = value as JArray;
				if (jArray != null && jArray.Length > 0)
				{
					if (jArray[0] is JArray)    // Matrix
					{
						Array returnArray = Array.CreateInstance(fieldInfo.FieldType.GetElementType(), jArray.Length);
						int tempIndex = 0;
						foreach (JArray innerItem in jArray)
						{
							returnArray.SetValue(innerItem.ToArray(fieldInfo.FieldType.GetElementType().GetElementType()), tempIndex);
							tempIndex++;
						}
						return returnArray;
					}
					else    // Vector
					{
						return jArray.ToArray(fieldInfo.FieldType.GetElementType());
					}
				}

				return null;
			}
			private void initializeOutParms() {
				DynAjaxEventContext.Clear();
				foreach (JObject parm in DynAjaxEventContext.outParmsMetadata) {
					string parmName = (string)parm["av"];
					if (!String.IsNullOrEmpty(parmName))
					{
						object TypedValue = getFieldValue(targetObj, parmName);
						DynAjaxEventContext.SetParmHash(parmName, TypedValue);
						if (!DynAjaxEventContext.isInputParm(parmName) && TypedValue is IGXAssigned param)
						{
							param.IsAssigned = false;
						}
					}
				}
			}
			private object[] BeforeInvoke()
			{
				List<object> MethodParms = new List<object>();
				if (!anyError)
				{
					int nParm = 0;
					bool multipart = targetObj.context.IsMultipartRequest;
					int hash_i = 0;
					int parm_i = 0;
					foreach (JObject parm in DynAjaxEventContext.inParmsMetadata)
					{

						if (parm["postForm"] != null)
						{
						}
						else
						{
							object value = null;
							JObject jValue;
							try
							{
								value = inParmsValues[nParm];
								jValue = value as JObject;
								nParm++;
								if (multipart)
								{
									//Input Parameter de tipo file vienen en los files del request
									string sValue = value as string;
									string fld = $"{cmpContext}{(string)parm["fld"]}";
									if (sValue != null && string.IsNullOrEmpty(sValue) && !string.IsNullOrEmpty(targetObj.CGIGetFileName(fld)))
										value = targetObj.cgiGet(fld);
								}
								if (IsInternalParm(parm))
								{
									MethodParms.Add(value);
								}
								else
								{
									JArray columnValues = new JArray();
									JArray columnHashes = new JArray();
									JArray hideCodeValues;
									JArray AllCollData = value as JArray;
									string Picture = parm.Contains("pic") ? (string)parm["pic"] : string.Empty;
									if (parm.Contains("grid"))
									{
										string parentRow = string.Empty;
										//Case for each line command or collection based grid
										if (AllCollData != null)
										{
											foreach (JObject columnData in AllCollData)
											{
												parentRow = (string)columnData["pRow"];
												columnValues = (JArray)columnData["c"];
												columnHashes = (JArray)columnData["hsh"];
												hideCodeValues = (JArray)columnData["hc"];
												value = columnData["v"];
												int rowIdx = 1;
												foreach (object columnVal in columnValues)
												{
													string varName = $"{cmpContext}{(string)parm["fld"]}_{rowIdx.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0')}{parentRow}";

													ReadColumnVarValue(columnVal, targetObj, varName);
													rowIdx++;
												}
												if (parm["hsh"] != null)
												{
													try
													{
														rowIdx = 1;
														if (columnHashes != null)
														{
															foreach (object columnHash in columnHashes)
															{
																string hashName = $"{cmpContext}gxhash_{(string)parm["fld"]}_{StrRowIdx(rowIdx)}{parentRow}";
																string sRow = $"{StrRowIdx(rowIdx)}{parentRow}";

																ReadColumnVarValue(columnHash, targetObj, hashName);
																if (!Config.GetValueOf("ValidateSecurityToken"))
																{
																	SetScalarOrCollectionValue((string)parm["av"], columnValues[rowIdx - 1], columnValues);
																	object TypedValue = getFieldValue(targetObj, (string)parm["av"]);
																	CheckParmIntegrity(TypedValue, (string)columnHash, sRow, DynAjaxEventContext.inParmsMetadata[parm_i], hash_i, Picture);
																}
																rowIdx++;
															}
														}
													}
													catch (Exception ex)
													{
														ForbiddenAction(ex);
													}
												}
												if (hideCodeValues != null)
												{
													rowIdx = 1;
													foreach (object columnVal in hideCodeValues)
													{
														string varName = $"{cmpContext}GXHC{(string)parm["fld"]}_{StrRowIdx(rowIdx)}{parentRow}";
														ReadColumnVarValue(columnVal, targetObj, varName);
														rowIdx++;
													}
												}
												int gridId = (int)parm["grid"];
												string pRowRCSuffix = (!String.IsNullOrEmpty(parentRow)) ? $"_{parentRow}" : string.Empty;
												targetObj.FormVars[$"{cmpContext}nRC_GXsfl_{gridId.ToString(CultureInfo.InvariantCulture)}{pRowRCSuffix}"] = columnValues.Count.ToString(CultureInfo.InvariantCulture);
											}
										}
										if (parm.Contains("prop") && ((string)parm["prop"]) == "GridRC")
										{
											string sRC = string.Empty;
											string rowsufix = string.Empty;
											string varname = string.Empty;
											if (jValue != null)
											{
												sRC = (string)(jValue["gridRC"]);
												rowsufix = (string)jValue["rowSuffix"];
												varname = (string)parm["av"];
											}
											targetObj.FormVars[$"{cmpContext}{varname}{rowsufix}"] = sRC;
											value = null;
										}
									}
									else
									{
										columnValues = AllCollData;
										if (parm["hsh"] != null && !Config.GetValueOf("ValidateSecurityToken"))
										{
											try
											{
												JObject hashObj = (JObject)(hash_i < inHashValues.Length ? inHashValues[hash_i] : new JObject());
												string sRow = hashObj.Contains("row") ? (string)hashObj["row"] : string.Empty;
												string hash = hashObj.Contains("hsh") ? (string)hashObj["hsh"] : string.Empty;
												SetScalarOrCollectionValue((string)parm["av"], inParmsValues[parm_i], columnValues);
												object TypedValue = getFieldValue(targetObj, (string)parm["av"]);
												CheckParmIntegrity(TypedValue, hash, sRow, DynAjaxEventContext.inParmsMetadata[parm_i], hash_i, Picture);
											}
											catch (Exception ex)
											{
												ForbiddenAction(ex);
											}
										}
										if (parm["hsh"] != null && (Boolean)parm["hsh"])
										{
											hash_i++;
										}
									}
									if (value != null && value != JNull.Value)
									{
										SetNullableScalarOrCollectionValue(parm, value, columnValues);
									}
								}
							}
							catch (DataIntegrityException)
							{
								anyError = true;
								break;
							}
							catch (Exception ex)
							{
								GXLogging.Warn(log, $"(BeforeInvoke) Error setting DynAjaxEvent parameter:'{(string)parm["av"]} with value '{((value != null) ? value : "null")}'' ", ex);
							}
							if (parm["postForm"] == null)
							{
								parm_i++;
							}
						}

					}

					if (grid != 0 && !String.IsNullOrEmpty(row))
					{
						SetFieldValue("sGXsfl_" + grid.ToString(CultureInfo.InvariantCulture) + "_idx", row + pRow);
						SetFieldValue("nGXsfl_" + grid.ToString(CultureInfo.InvariantCulture) + "_idx", int.Parse(row));
					}
					SetFieldValue("wbLoad", true);
				}
				initializeOutParms();
				return MethodParms.ToArray();
			}

			private void ForbiddenAction(Exception ex)
			{
				anyError = true;
				GXLogging.Error(log, "Failed checkParmsIntegrity 403 Forbidden action Exception");
				GXLogging.Error(log, ex.ToString());
				GXLogging.Error(log, ex.StackTrace);
				targetObj.SendResponseStatus(403, "Forbidden action");
				throw new DataIntegrityException("Failed checkParmIntegrity 403 Forbidden action Exception");
			}

			private void ReadColumnVarValue(object columnVal, GXHttpHandler targetObj, string varName)
			{
				//FormVars are then read in the foreach line according to the current language of the application
				if (columnVal is double)
					targetObj.FormVars[varName] = ((double)columnVal).ToString(targetObj.context.localUtil.CultureInfo.NumberFormat);
				else if (columnVal is decimal)
					targetObj.FormVars[varName] = ((decimal)columnVal).ToString(targetObj.context.localUtil.CultureInfo.NumberFormat);
				else if (columnVal is float)
					targetObj.FormVars[varName] = ((float)columnVal).ToString(targetObj.context.localUtil.CultureInfo.NumberFormat);
				else
					targetObj.FormVars[varName] = columnVal.ToString();
			}
			private string StrRowIdx(int rowIdx)
			{
				return rowIdx.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0');
			}
			private void CheckParmIntegrity(Object parm, string jwt, string sRow, object inParmMetadata, int hash_i, string Pic)
			{
				string sContext = targetObj.IsMasterPage() ? "gxmpage_" : cmpContext;
				if (!targetObj.VerifySecureSignedToken(sContext + sRow, parm, Pic, jwt, targetObj._Context) && !targetObj.VerifySecureSignedToken(sContext, parm, Pic, jwt, targetObj._Context))
				{
					GXLogging.Error(log, "Failed checkParmsIntegrity 403 Forbidden action with parm:" + inParmMetadata);
					GXLogging.Error(log, "ParmValue: " + targetObj.Serialize(parm, Pic));
					GXLogging.Error(log, "row: " + sRow);
					GXLogging.Error(log, "hash_i:" + hash_i + " inHashValues.Length:" + inHashValues.Length);
					GXLogging.Error(log, "Received jwt:" + jwt);
					targetObj.SendResponseStatus(403, "Forbidden action");
					throw new DataIntegrityException("Failed checkParmIntegrity 403 Forbidden action with parm:" + inParmMetadata);
				}
			}

			private void AfterInvoke()
			{
				targetObj._Context.httpAjaxContext.AddStylesHidden();
				targetObj.SendComponentObjects();
			}

			private void DoInvoke(object[] MethodParms)
			{
				if (!anyError)
				{
					for (int i = 0; i < eventHandlers.Length; i++)
					{
						string handler = eventHandlers[i];
						if (!string.IsNullOrEmpty(handler))
						{
							if (i > 0)
							{
								targetObj.PrepareForReuse();
							}
							_ = targetObj.GetType().InvokeMember(handler, BindingFlags.Public |
							BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
							Type.DefaultBinder,
							targetObj, (eventUseInternalParms[i] ? MethodParms : null));
						}
					}
				}
			}

			internal string Invoke(string JsonMessage, GXHttpHandler targetObj)
			{
				JObject objMessage = JSONHelper.ReadJSON<JObject>(JsonMessage);
				ParseInputJSonMessage(objMessage, targetObj);
				this.targetObj.setFullAjaxMode();
				this.targetObj.createObjects();
				this.targetObj.initialize();
				this.targetObj.InitializeDynEvents();
				this.targetObj.SetPrefix(cmpContext);
				this.targetObj.initialize_properties();
				this.targetObj.setAjaxEventMode();
				this.targetObj.ajax_rsp_clear();

				bool iSecEnabled = this.targetObj.IntegratedSecurityEnabled;
				if (this.targetObj.ValidateObjectAccess(cmpContext) && (iSecEnabled ? this.targetObj.CheckCmpSecurityAccess() : true))
				{
					ParseMetadata();
					DoInvoke(BeforeInvoke());
					AfterInvoke();
				}
				string response = BuildOutputJSonMessage();
				this.targetObj.cleanup();
				if (this.targetObj != targetObj)
				{
					targetObj.context.CloseConnections();
				}
				return response;
			}
		}
#if NETCORE
		internal virtual async Task WebAjaxEventAsync()
		{
			bool isMultipartRequest = context.IsMultipartRequest;
			if (isMultipartRequest)
			{
				localHttpContext.Response.ContentType = MediaTypesNames.TextHtml;
			}
			else
			{
				localHttpContext.Response.ContentType = MediaTypesNames.ApplicationJson;
			}
			setAjaxCallMode();
			context.setFullAjaxMode();
			DynAjaxEvent dynAjaxEvent = new DynAjaxEvent(context.httpAjaxContext.DynAjaxEventContext);
			string jsonRequest;
			if (context.IsMultipartRequest)
				jsonRequest = cgiGet(GX_AJAX_MULTIPART_ID);
			else
			{
				using (StreamReader reader = new StreamReader(localHttpContext.Request.GetInputStream()))
				{
					jsonRequest = await reader.ReadToEndAsync(); ;
				}
			}
			string jsonResponse = dynAjaxEvent.Invoke(jsonRequest, this);


			if (!redirect(context))
			{
				((GxContext)context).SendFinalJSONResponse(jsonResponse);
			}
		}
#endif

		public virtual void webAjaxEvent()
		{
			bool isMultipartRequest = context.IsMultipartRequest;
			if (isMultipartRequest)
			{
				localHttpContext.Response.ContentType = MediaTypesNames.TextHtml;
			}
			else
			{
				localHttpContext.Response.ContentType = MediaTypesNames.ApplicationJson;
			}
			setAjaxCallMode();
			context.setFullAjaxMode();
			DynAjaxEvent dynAjaxEvent = new DynAjaxEvent( context.httpAjaxContext.DynAjaxEventContext);
			string jsonRequest;
			if (context.IsMultipartRequest)
				jsonRequest = cgiGet(GX_AJAX_MULTIPART_ID);
			else
			{
				using (StreamReader reader = new StreamReader(localHttpContext.Request.GetInputStream()))
				{
					jsonRequest = reader.ReadToEnd();
				}
			}
			string jsonResponse = dynAjaxEvent.Invoke(jsonRequest, this);


			if (!redirect(context))
			{
				((GxContext)context).SendFinalJSONResponse(jsonResponse);
			}
		}

		private bool redirect(IGxContext context)
		{
			if (context.WillRedirect())
			{
				context.Redirect(context.wjLoc);
				context.DispatchAjaxCommands();
				return true;
			}
			else if (context.nUserReturn == 1)
			{
				context.ajax_rsp_command_close();
				context.DispatchAjaxCommands();
				return true;
			}
			return false;
		}



		protected virtual void createObjects() { throw new Exception("The method or operation is not implemented."); }
		public virtual String GetPgmname() { throw new Exception("The method or operation is not implemented."); }
		public virtual String GetPgmdesc() { throw new Exception("The method or operation is not implemented."); }
#if !NETCORE
		protected virtual bool IntegratedSecurityEnabled { get { return false; } }
		protected virtual GAMSecurityLevel IntegratedSecurityLevel { get { return 0; } }
		[Obsolete("IntegratedSecurityPermissionName is deprecated, it is here for compatibility. Use ExecutePermissionPrefix instead.", false)]
		protected virtual string IntegratedSecurityPermissionName { get { return string.Empty; } }
		protected virtual string ExecutePermissionPrefix { get { return string.Empty; } }
		public bool IntegratedSecurityEnabled2 { get { return IntegratedSecurityEnabled; } }
		public GAMSecurityLevel IntegratedSecurityLevel2 { get { return IntegratedSecurityLevel; } }
#endif
		private bool disconnectUserAtCleanup;
		private bool validEncryptedParm;

		protected virtual void setPortletMode()
		{ context.setPortletMode(); }

		protected virtual void setAjaxCallMode()
		{ context.setAjaxCallMode(); }

		protected virtual void setAjaxEventMode()
		{ context.setAjaxEventMode(); }

		protected virtual void setFullAjaxMode()
		{ FullAjaxMode = true; }

		public virtual bool isPortletMode()
		{ return context.isPortletMode(); }

		public virtual bool isAjaxCallMode()
		{ return context.isAjaxCallMode(); }

		public virtual bool isAjaxEventMode()
		{ return context.isAjaxEventMode(); }

		public virtual bool isFullAjaxMode()
		{ return FullAjaxMode; }

		public virtual bool isPopUpObject()
		{ return context.isPopUpObject(); }

		public bool IsSameComponent(string oldName, string newName)
		{
			if (string.Compare(oldName.Trim(), newName.Trim(), true) == 0)
			{
				return true;
			}
			else if (newName.Trim().ToLower().StartsWith(oldName.Trim().ToLower() + HttpHelper.ASPX))
			{

				return true;
			}
			return false;
		}

		public void disableOutput()
		{
			context.disableOutput();
		}

		public void enableOutput()
		{
			context.enableOutput();
		}

		public bool isOutputEnabled()
		{
			return context.isOutputEnabled();
		}

		public void disableJsOutput()
		{
			context.httpAjaxContext.disableJsOutput();
		}

		public void enableJsOutput()
		{
			context.httpAjaxContext.enableJsOutput();
		}

		public bool isJsOutputEnabled()
		{
			return context.httpAjaxContext.isJsOutputEnabled;
		}

		[Obsolete("AddJavascriptSource in HttpHandler is deprecated", false)]
		public void AddJavascriptSource(string jsSrc, string urlBuildNumber)
		{
			context.AddJavascriptSource(jsSrc, urlBuildNumber);
		}

		public void AddJavascriptSource(string jsSrc, string urlBuildNumber, bool userDefined, bool isInlined)
		{
			context.AddJavascriptSource(jsSrc, urlBuildNumber, userDefined, isInlined);
		}

		public void AddDeferredJavascriptSource(string jsSrc, string urlBuildNumber)
		{
			context.AddDeferredJavascriptSource(jsSrc, urlBuildNumber);
		}

		public void AddStyleSheetFile(string styleSheet)
		{
			AddStyleSheetFile(styleSheet, string.Empty);
		}

		public void AddStyleSheetFile(string styleSheet, string urlBuildNumber)
		{
			List<string[]> userStyleSheetFiles = context.userStyleSheetFiles;
			urlBuildNumber = context.GetURLBuildNumber(styleSheet, urlBuildNumber);
			userStyleSheetFiles.Add(new string[] { styleSheet, urlBuildNumber });
		}
		public void AddStyleSheetFile(string styleSheet, string urlBuildNumber, bool isDeferred)
		{
			urlBuildNumber = context.GetURLBuildNumber(styleSheet, urlBuildNumber);
			AddStyleSheetFile(styleSheet, urlBuildNumber, false, isDeferred);
		}

		//isGxThemeHidden: true if it is the theme to be sent in GX_THEME, in that case it is not added to the GX_STYLE_FILES list, only in the hidden GX_THEME
		private void AddStyleSheetFile(string styleSheet, string urlBuildNumber, bool isGxThemeHidden, bool isDeferred = false)
		{
			if (!context.StyleSheetAdded(styleSheet))
			{
				context.AddStyleSheetFile(styleSheet);
				string sUncachedURL = context.GetCompleteURL(styleSheet) + urlBuildNumber;
				string sLayerName = styleSheet.Replace("/", "_").Replace(".","_");
				if (!context.HtmlHeaderClosed && context.isEnabled)
				{
					string sRelAtt = (isDeferred ? "rel=\"preload\" as=\"style\" " : "rel=\"stylesheet\"");
					if (isGxThemeHidden)
						context.WriteHtmlTextNl("<link id=\"gxtheme_css_reference\" " + sRelAtt + " type=\"text/css\" href=\"" + sUncachedURL + "\" " + GXUtil.HtmlEndTag(HTMLElement.LINK));
					else
					{
						if (context.GetThemeisDSO())
						{
							context.WriteHtmlTextNl("<style data-gx-href=\"" + sUncachedURL + "\"> @import url(\"" + sUncachedURL + "\") layer(" + sLayerName + ");</style>");
						}
						else
						{
							context.WriteHtmlTextNl("<link " + sRelAtt + " type=\"text/css\" href=\"" + context.GetCompleteURL(styleSheet) + "\"" + GXUtil.HtmlEndTag(HTMLElement.LINK));
						}
					}
				}
				else
				{
					if (!isGxThemeHidden) context.httpAjaxContext.AddStylesheetToLoad(context.GetCompleteURL(styleSheet) + urlBuildNumber);
				}
			}
		}

		private Boolean FetchCustomCSS(ref string cssContent)
		{
			cssContent = string.Empty;
			Boolean bSuccess = true;
			if (customCSSContent.ContainsKey(GetPgmname()))
			{
				cssContent = customCSSContent[GetPgmname()];
			}
			else
			{
				try
				{
					string path = Path.Combine(context.GetPhysicalPath(), GetPgmname().Replace('.', '/') + ".css");
					bSuccess = File.Exists(path);
					if (bSuccess)
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
						cssContent = File.ReadAllText(path);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				}
				catch (Exception)
				{
					bSuccess = false;
				}
				finally
				{
					if (!bSuccess)
						customCSSContent.Add(GetPgmname(), string.Empty);
				}
			}
			return bSuccess && !String.IsNullOrEmpty(cssContent);
		}

		public void CloseStyles() {
			string cssContent = string.Empty;
			Boolean bHasCustomContent = FetchCustomCSS(ref cssContent);

			if (bHasCustomContent && !context.StyleSheetAdded(GetPgmname()))
			{
				context.WriteHtmlTextNl("<style id=\"gx-inline-css\">" + cssContent + "</style>");
				context.AddStyleSheetFile(GetPgmname());
			}

			string[] referencedFiles = ThemeHelper.GetThemeCssReferencedFiles(Path.GetFileNameWithoutExtension(this.ThemestyleSheet));
			foreach (string file in referencedFiles)
			{
				string extension = Path.GetExtension(file);
				if (extension == ".css")
				{
					AddStyleSheetFile(file, this.ThemeurlBuildNumber, bHasCustomContent);
				}
				else if (extension == ".js")
					AddDeferredJavascriptSource(file, this.ThemeurlBuildNumber);
			}
			List<string[]> userStyleSheetFiles = context.userStyleSheetFiles;
			foreach (string[] data in userStyleSheetFiles)
			{
				AddStyleSheetFile(data[0], data[1], false, false);
			}
			AddStyleSheetFile(this.ThemekbPrefix + "Resources/" + context.GetLanguage() + "/" + this.ThemestyleSheet, this.ThemeurlBuildNumber, true, bHasCustomContent);
		}
		public void AddThemeStyleSheetFile(String kbPrefix, String styleSheet, string urlBuildNumber)
		{
			this.ThemekbPrefix = kbPrefix;
			this.ThemestyleSheet = styleSheet;
			this.ThemeurlBuildNumber = urlBuildNumber;
		}

		public string GetCacheInvalidationToken()
		{
			return context.GetCacheInvalidationToken();
		}

		public void AddComponentObject(string cmpCtx, string objName, bool justCreated)
		{

			if (justCreated)
			{
				DeletePostValuePrefix(cmpCtx);
			}
			context.AddComponentObject(cmpCtx, objName);
		}

		public void SaveComponentMsgList(string cmpCtx)
		{
			context.SaveComponentMsgList(cmpCtx);
		}

		public void SendComponentObjects()
		{
			context.SendComponentObjects();
		}

		public void SendServerCommands()
		{
			context.SendServerCommands();
		}

		public void PopReferer()
		{
			context.PopReferer();
		}

		public void DeleteReferer(int popupLevel)
		{
			context.DeleteReferer(popupLevel);
		}

		public void executeUsercontrolMethod(String CmpContext, bool IsMasterPage, String containerName, String methodName, String input, Object[] parms)
		{
			context.httpAjaxContext.executeUsercontrolMethod(CmpContext, IsMasterPage, containerName, methodName, input, parms);
		}

		public void setExternalObjectProperty(String CmpContext, bool IsMasterPage, String objectName, String propertyName, object value)
		{
			context.httpAjaxContext.setExternalObjectProperty(CmpContext, IsMasterPage, objectName, propertyName, value);
		}

		public void executeExternalObjectMethod(String CmpContext, bool IsMasterPage, String objectName, String methodName, object[] parms, bool isEvent)
		{
			context.httpAjaxContext.executeExternalObjectMethod(CmpContext, IsMasterPage, objectName, methodName, parms, isEvent);
		}

		public bool DisconnectAtCleanup
		{
			get { return disconnectUserAtCleanup; }
			set { disconnectUserAtCleanup = value; }
		}
#if NETCORE
		public override IGxContext context
#else
		public IGxContext context
#endif
		{
			set
			{
				_Context = value;
				if (context != null)
					context.httpAjaxContext.context = _Context;
			}
			get { return _Context; }
		}
		public msglist GX_msglist
		{
			get
			{
				return context.GX_msglist;
			}
			set
			{
				context.GX_msglist = value; context.httpAjaxContext.setMsgList(context.GX_msglist);
			}
		}
		protected HttpContext localHttpContext
		{
			set { _Context.HttpContext = value; }
			get { return _Context.HttpContext; }
		}

		protected void _Write(String Content)
		{
			if (context.isEnabled == false)
			{
				if (context.httpAjaxContext.isAjaxContent())
					context.httpAjaxContext.writeAjaxContent(Content);
				return;
			}
			ControlOutputWriter.Write(Content);
		}

		public HtmlTextWriter ControlOutputWriter
		{
			get
			{
				return _Context.OutputWriter;

			}
			set
			{
				_Context.OutputWriter = value;
			}
		}
		public bool isStatic
		{
			get { return _isStatic; }
			set { _isStatic = value; }
		}
		public string Parms
		{
			set { _strParms = value; }
			get { return _strParms; }
		}

		public string StaticContentBase
		{
			get
			{
				if (staticContentBase == null)
				{
					string dir = string.Empty;
					if (Config.GetValueOf("STATIC_CONTENT", out dir))
					{
						if (!(dir.EndsWith("/") || dir.EndsWith("\\")) && !String.IsNullOrEmpty(dir))
							staticContentBase = dir + "/";
						else
							staticContentBase = dir;
					}
					else
					{
						staticContentBase = string.Empty;
					}
				}
				return staticContentBase;
			}

			set { staticContentBase = value; }

		}
		protected void ExitApp()
		{
			if (disconnectUserAtCleanup)
			{
				try
				{
					context.Disconnect();
				}
				catch (Exception) {; }
			}
		}

		protected void exitApplication()
		{
			ExitApp();
		}

		private bool IsGxAjaxRequest()
		{
			if (context.IsMultipartRequest && !(context.DrawGridsAtServer))
			{
				return true;
			}
			if (!string.IsNullOrEmpty(context.HttpContext.Request.Headers[GX_AJAX_REQUEST_HEADER]))
			{
				return true;
			}
			return false;
		}

		protected bool IsSpaRequest()
		{
			return context.isSpaRequest();
		}

		protected virtual bool IsSpaSupported()
		{
			return true;
		}

		protected virtual void ValidateSpaRequest()
		{
			if (IsSpaRequest())
			{
				context.DisableSpaRequest();
				sendSpaHeaders();
			}
		}
#if !NETCORE

		protected string GetEncryptedHash(string value, string key)
		{
			return Encrypt64(GXUtil.GetHash(WebSecurityHelper.StripInvalidChars(value), Cryptography.Constants.SecurityHashAlgorithm), key);
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
#endif
		protected string DecryptAjaxCall(string encrypted)
		{
			this.validEncryptedParm = false;

			if (IsGxAjaxRequest())
			{
				string key = context.httpAjaxContext.GetAjaxEncryptionKey();
				string decrypted = CryptoImpl.DecryptRijndael(encrypted, key, out this.validEncryptedParm);
				if (!this.validEncryptedParm)
				{
					GXLogging.Error(log, string.Format("403 Forbidden error. Could not decrypt Ajax parameter: '{0}' with key: '{1}'", encrypted, key));
					SendResponseStatus(403, "Forbidden action");
					return string.Empty;
				}
				if (this.validEncryptedParm && context.HttpContext.Request.GetMethod() != "POST")
				{
					SetQueryString(decrypted);
					decrypted = GetNextPar();
				}
				return decrypted;
			}
			return encrypted;
		}

		protected bool IsValidAjaxCall()
		{
			return IsValidAjaxCall(true);
		}

		protected bool IsValidAjaxCall(bool insideAjaxCall)
		{
			if (insideAjaxCall && !this.validEncryptedParm)
			{
				GXLogging.Error(log, "Failed IsValidAjaxCall 403 Forbidden action");
				SendResponseStatus(403, "Forbidden action");
				return false;
			}
			else if (!insideAjaxCall && IsGxAjaxRequest() && !isFullAjaxMode())
			{
				GXLogging.Error(log, "440 Session timeout - Not valid Ajax Call");
				SendResponseStatus(440, "Session timeout");
				return false;
			}
			return true;
		}

		protected void SendResponseStatus(HttpStatusCode statusCode)
		{
			SendResponseStatus((int)statusCode, string.Empty);
		}
#if !NETCORE
		protected void SendResponseStatus(int statusCode, string statusDescription)
		{
			context.HttpContext.Response.StatusCode = statusCode;
			if (!string.IsNullOrEmpty(statusDescription))
				context.HttpContext.Response.StatusDescription = statusDescription;
			this.setAjaxCallMode();
			this.disableOutput();
		}
#else
		protected override void SendResponseStatus(int statusCode, string statusDescription)
		{
			context.HttpContext.Response.StatusCode = statusCode;
			this.setAjaxCallMode();
			this.disableOutput();
		}
#endif
		private void SendReferer()
		{
			context.httpAjaxContext.ajax_rsp_assign_hidden("sCallerURL", context.GetReferer());
		}

		protected void SendWebComponentState()
		{
			context.httpAjaxContext.AddStylesHidden();
		}

		protected void SendState()
		{
			if (context.HttpContext != null)
			{
				SendReferer();
				SendWebSocketParms();
				context.httpAjaxContext.AddNavigationHidden();
				context.httpAjaxContext.AddThemeHidden(context.GetTheme());
				context.httpAjaxContext.AddStylesHidden();
				if (IsSpaRequest())
				{
					context.WriteHtmlTextNl("<script>gx.ajax.saveJsonResponse('" + HttpHelper.HtmlEncodeJsonValue(context.getJSONResponse()) + "');</script>");
				}
				else
				{
					if (context.DrawGridsAtServer)
					{
						context.WriteHtmlTextNl("<script type=\"text/javascript\">gx.grid.drawAtServer=true;</script>");
					}
					skipLines(1);
					string value1 = context.httpAjaxContext.HiddenValues.ToString();
					if (Preferences.UseBase64ViewState())
					{
						value1 = Convert.ToBase64String(Encoding.UTF8.GetBytes(value1));
						context.WriteHtmlTextNl("<script type=\"text/javascript\">gx.http.useBase64State=true;</script>");
					}
					context.WriteHtmlText("<div><input type=\"hidden\" name=\"GXState\" value='");
					context.WriteHtmlText(GXUtil.HtmlEncodeInputValue(value1));
					context.WriteHtmlTextNl("'" + GXUtil.HtmlEndTag(HTMLElement.INPUT) + "</div>");
				}
			}
		}
		const string GX_SEC_TOKEN_PREFIX = "GX_AUTH";

		public void SendSecurityToken(String cmpCtx)
		{
			if (context.httpAjaxContext != null && !context.WillRedirect())
			{
				context.httpAjaxContext.ajax_rsp_assign_hidden(GetSecurityObjTokenId(cmpCtx), GetObjectAccessWebToken(cmpCtx));
			}
		}

		public string Serialize(object Value, string Pic)
		{
			if (!string.IsNullOrEmpty(Pic))
			{
				if (Value is Decimal)
				{
					return Serialize(context.localUtil.Format((decimal)Value, Pic));
				}
				else
				{
					if (Value is Double)
					{
						return Serialize(context.localUtil.Format((decimal)Value, Pic));
					}
					else
					{
						if (Value is DateTime)
						{
							return Serialize(context.localUtil.Format((DateTime)Value, Pic));
						}
						else
						{
							if (Value is int)
							{
								return Serialize(context.localUtil.Format((decimal)(int)Value, Pic));
							}
							else
							{
								if (Value is short)
								{
									return Serialize(context.localUtil.Format((decimal)(short)Value, Pic));
								}
								else
								{
									if (Value is long)
									{
										return Serialize(context.localUtil.Format((decimal)(long)Value, Pic));
									}
									else
									{
										string sValue = Value as string;
										if (sValue != null)
										{
											return Serialize(context.localUtil.Format(sValue, Pic));
										}
									}
								}
							}
						}
					}
				}
			}
			return Serialize(Value);
		}

		private static string Serialize(object Value)
		{
			string strValue;

			Regex rgx = new Regex("0*$");
			Regex rgx2 = new Regex("\\.$");

			if (Value is Decimal)
			{
				strValue = ((Decimal)Value).ToString(CultureInfo.InvariantCulture);
				if (strValue.IndexOf('.') != -1)
					strValue = rgx2.Replace(rgx.Replace(strValue, string.Empty), string.Empty);
			}
			else
			{
				if (Value is DateTime)
				{
					if ((DateTime)Value == (DateTime.MinValue))
						strValue = "    /  /   00:00:00";
					else
						strValue = ((DateTime)Value).ToString("yyyy/MM/dd HH:mm:ss");
				}
				else
				{
					if (Value is Double)
					{
						strValue = ((Double)Value).ToString(CultureInfo.InvariantCulture);
						if (strValue.IndexOf('.') != -1)
							strValue = rgx2.Replace(rgx.Replace(strValue, string.Empty), string.Empty);
					}
					else
					{
						IGxJSONSerializable sdtValue = Value as IGxJSONSerializable;
						if (sdtValue != null)
							strValue = sdtValue.ToJSonString();
						else
							strValue = Value.ToString();
					}
				}
			}

			return strValue;
		}

		private string GetSecurityObjTokenId(String cmpCtx)
		{
			return GX_SEC_TOKEN_PREFIX + "_" + cmpCtx + GetPgmname().ToUpper();
		}
		protected string GetObjectAccessWebToken(String cmpCtx)
		{
			return GetSecureSignedToken(cmpCtx, string.Empty, this.context);
		}

		protected string GetSecureSignedToken(String cmpCtx, object Value, IGxContext context)
		{
			return GetSecureSignedToken(cmpCtx, Serialize(Value), context);
		}

		protected string GetSecureSignedToken(String cmpCtx, GxUserType Value, IGxContext context)
		{
			return GetSecureSignedToken(cmpCtx, Serialize(Value), context);
		}


		protected string GetSecureSignedToken(string cmpCtx, string value, IGxContext context)
		{
			return WebSecurityHelper.Sign(PgmInstanceId(cmpCtx), string.Empty, value, SecureTokenHelper.SecurityMode.Sign, context);
		}
		protected bool VerifySecureSignedToken(string cmpCtx, Object value, string pic, string signedToken, IGxContext context)
		{
			GxUserType SDT = value as GxUserType;
			if (SDT != null)
				return WebSecurityHelper.VerifySecureSignedSDTToken(cmpCtx, SDT, signedToken, context);
			IGxCollection SDTColl = value as IGxCollection;
			if (SDTColl != null)
				return WebSecurityHelper.VerifySecureSignedSDTToken(cmpCtx, SDTColl, signedToken, context);
			return this.VerifySecureSignedToken(cmpCtx, Serialize(value, pic), signedToken, context);
		}
		protected bool VerifySecureSignedToken(string cmpCtx, Object value, string signedToken, IGxContext context)
		{
			return this.VerifySecureSignedToken(cmpCtx, Serialize(value), signedToken, context);
		}
		protected bool VerifySecureSignedToken(string cmpCtx, GxUserType value, string signedToken, IGxContext context)
		{
			return WebSecurityHelper.VerifySecureSignedSDTToken(cmpCtx, value, signedToken, context);
		}
		protected bool VerifySecureSignedToken(string cmpCtx, string Value, string signedToken, IGxContext context)
		{
			return WebSecurityHelper.Verify(PgmInstanceId(cmpCtx), string.Empty, Value, signedToken, context);
		}

		private string PgmInstanceId(string cmpCtx)
		{
			return String.Format("{0}", cmpCtx + this.GetPgmname().ToUpper());
		}

		private bool ValidateObjectAccess(String cmpCtx)
		{
			if (!Config.GetValueOf("ValidateSecurityToken"))
			{
#if NETCORE
				string val = (context.IsMultipartRequest) ? cgiGet("X-GXAUTH-TOKEN") : this.context.HttpContext.Request.Headers["X-GXAUTH-TOKEN"].First();
#else
				string val = (context.IsMultipartRequest) ? cgiGet("X-GXAUTH-TOKEN") : this.context.HttpContext.Request.Headers["X-GXAUTH-TOKEN"];
#endif
				if (!WebSecurityHelper.Verify(PgmInstanceId(cmpCtx), string.Empty, string.Empty, val, context))
				{
					SendResponseStatus(HttpStatusCode.Unauthorized);
					if (context.GetBrowserType() != GxContext.BROWSER_INDEXBOT)
					{
						if (log.IsWarningEnabled)
						{
							GXLogging.Warn(log, $"Validation security token '{GetObjectAccessWebToken(cmpCtx)}' failed for program: '{cmpCtx + this.GetPgmname().ToUpper()}'");
						}
					}
					return false;
				}
			}
			return true;
		}

		private void SendWebSocketParms()
		{
			if (!_Context.isAjaxRequest() || _Context.isSpaRequest())
			{
				_Context.httpAjaxContext.ajax_rsp_assign_hidden("GX_WEBSOCKET_ID", context.ClientID);
			}
		}

		protected void SendAjaxEncryptionKey()
		{
			string key = context.httpAjaxContext.GetAjaxEncryptionKey();
			_Context.httpAjaxContext.ajax_rsp_assign_hidden(CryptoImpl.AJAX_ENCRYPTION_KEY, key);
			_Context.httpAjaxContext.ajax_rsp_assign_hidden(CryptoImpl.AJAX_ENCRYPTION_IV, CryptoImpl.GX_AJAX_PRIVATE_IV);
			_Context.httpAjaxContext.ajax_rsp_assign_hidden(CryptoImpl.AJAX_SECURITY_TOKEN, CryptoImpl.EncryptRijndael(key, CryptoImpl.GX_AJAX_PRIVATE_KEY));
		}

		public void ajax_req_read_hidden_sdt(String jsonStr, Object SdtObj)
		{
			dynamic jsonObj;
			try
			{
				if (SdtObj != null && !string.IsNullOrEmpty(jsonStr) && !jsonStr.Equals("undefined") && !jsonStr.Equals("null"))
				{
					if (jsonStr.StartsWith("["))
						jsonObj = JSONHelper.ReadJSON<JArray>(jsonStr);
					else
						jsonObj = JSONHelper.ReadJSON<JObject>(jsonStr);
					((IGxJSONAble)SdtObj).FromJSONObject(jsonObj);
				}
			}
			catch (Exception ex)
			{
				GXLogging.Warn(log, "Error parsing jsonObj:" + jsonStr + " for type " + SdtObj.GetType().FullName, ex);
			}
		}

		public NameValueCollection FormVars
		{
			set
			{
				_Context.httpAjaxContext.FormVars = value;
			}
			get
			{
				return _Context.httpAjaxContext.FormVars;
			}
		}

		public virtual bool UseBigStack()
		{ return false; }

#if !NETCORE
		private const int STACKSIZE = 1024 * 1024 * 2;
		public bool IsMain
		{
			set { _isMain = value; }
			get { return _isMain; }
		}
#endif
#if NETCORE
		internal async Task ProcessRequestAsync(HttpContext httpContext)
		{
			localHttpContext = httpContext;

			if (IsSpaRequest() && !IsSpaSupported())
			{
				this.SendResponseStatus(SPA_NOT_SUPPORTED_STATUS_CODE, "SPA not supported by the object");
				context.CloseConnections();
				await Task.CompletedTask;
			}
			ControlOutputWriter = new HtmlTextWriter(localHttpContext);
			LoadParameters(localHttpContext.Request.QueryString.Value);
			context.httpAjaxContext.GetAjaxEncryptionKey(); //Save encryption key in session
			InitPrivates();
			try
			{
				SetStreaming();
				SendHeaders();
				string clientid = context.ClientID; //Send clientid cookie (before response HasStarted) if necessary, since UseResponseBuffering is not in .netcore3.0

				bool validSession = ValidWebSession();
				if (validSession && IntegratedSecurityEnabled)
					validSession = ValidSession();
				if (validSession)
				{
					if (UseBigStack())
					{
						Thread ts = new Thread(new ParameterizedThreadStart(webExecuteWorker));
						ts.Start(httpContext);
						ts.Join();
						if (workerException != null)
							throw workerException;
					}
					else
					{
						await WebExecuteExAsync(httpContext);
					}
				}
				else
				{
					context.CloseConnections();
					if (IsGxAjaxRequest() || context.isAjaxRequest())
						context.DispatchAjaxCommands();
				}
				SetCompression(httpContext);
				context.ResponseCommited = true;
			}
			catch (Exception e)
			{
				try
				{
					context.CloseConnections();
				}
				catch { }
				{
					Exception exceptionToHandle = e.InnerException ?? e;
					handleException(exceptionToHandle.GetType().FullName, exceptionToHandle.Message, exceptionToHandle.StackTrace);
					throw new Exception("GXApplication exception", e);
				}
			}
		}

#endif
#if !NETCORE
		[SecuritySafeCritical]
#endif
		public void ProcessRequest(HttpContext httpContext)
		{
			localHttpContext = httpContext;

			if (IsSpaRequest() && !IsSpaSupported())
			{
				this.SendResponseStatus(SPA_NOT_SUPPORTED_STATUS_CODE, "SPA not supported by the object");
#if !NETCORE
				context.HttpContext.Response.TrySkipIisCustomErrors = true;
#endif
				context.CloseConnections();
				return;
			}
#if NETCORE
			ControlOutputWriter = new HtmlTextWriter(localHttpContext);
			LoadParameters(localHttpContext.Request.QueryString.Value);
			context.httpAjaxContext.GetAjaxEncryptionKey(); //Save encryption key in session
#else
			ControlOutputWriter = new HtmlTextWriter(localHttpContext.Response.Output);
			LoadParameters(localHttpContext.Request.Url.Query);
#endif
			InitPrivates();
			try
			{
				SetStreaming();
				SendHeaders();
				string clientid = context.ClientID; //Send clientid cookie (before response HasStarted) if necessary, since UseResponseBuffering is not in .netcore3.0
#if !NETCORE
				CSRFHelper.ValidateAntiforgery(httpContext);
#endif

				bool validSession = ValidWebSession();
				if (validSession && IntegratedSecurityEnabled)
					validSession = ValidSession();
				if (validSession)
				{
					if (UseBigStack())
					{
#if !NETCORE
						Thread ts = new Thread(new ParameterizedThreadStart(webExecuteWorker), STACKSIZE);
						object currentCultureInfo = Thread.CurrentThread.CurrentUICulture.Clone();
						if (currentCultureInfo != null)
						{
							ts.CurrentUICulture = (CultureInfo)currentCultureInfo;
						}
#else
						Thread ts = new Thread(new ParameterizedThreadStart(webExecuteWorker));
#endif
						ts.Start(httpContext);
						ts.Join();
						if (workerException != null)
							throw workerException;
					}
					else
					{
						webExecuteEx(httpContext);
					}
				}
				else
				{
					context.CloseConnections();
					if (IsGxAjaxRequest() || context.isAjaxRequest())
						context.DispatchAjaxCommands();
				}
				SetCompression(httpContext);
				context.ResponseCommited = true;
			}
			catch (Exception e)
			{
				try
				{
					context.CloseConnections();
				}
				catch { }
#if !NETCORE
				if (CSRFHelper.HandleException(e, httpContext))
				{
					GXLogging.Error(log, $"Validation of antiforgery failed", e);
				}
				else
#endif
				{
					Exception exceptionToHandle = e.InnerException ?? e;
					handleException(exceptionToHandle.GetType().FullName, exceptionToHandle.Message, exceptionToHandle.StackTrace);
					throw new Exception("GXApplication exception", e);
				}
			}
		}
		protected virtual bool ChunkedStreaming() { return false; }

		private void SetStreaming()
		{
#if !NETCORE
			if (ChunkedStreaming())
			{
				BringSessionStateToLife();
				context.HttpContext.Response.BufferOutput = false;
			}
#endif
		}
#if !NETCORE
		private void BringSessionStateToLife()
		{
			string sessionId = context.HttpContext.Session.SessionID;
			GXLogging.Debug(log, "Session is alive: " + sessionId);
		}
#endif
		internal string DumpHeaders(HttpContext httpContext)
		{
			StringBuilder str = new StringBuilder();
#if NETCORE
			foreach (string key in httpContext.Request.Headers.Keys)
			{
				str.Append(key + ":" + httpContext.Request.Headers[key]);
			}
			str.Append(StringUtil.NewLine() + "HttpCookies: ");
			foreach (string key in httpContext.Request.Cookies.Keys)
			{
				str.Append(StringUtil.NewLine() + key + ":" + httpContext.Request.Cookies[key]);
			}
			return str.ToString();
#else
			foreach (string key in httpContext.Request.Headers)
			{
				str.Append(key + ":" + httpContext.Request.Headers[key]);
			}
			str.Append(StringUtil.NewLine() + "HttpCookies: ");
			foreach (string key in httpContext.Request.Cookies)
			{
				str.Append(StringUtil.NewLine() + key + ":" + httpContext.Request.Cookies[key].Value);
			}
			return str.ToString();
#endif
		}

		protected bool CheckCmpSecurityAccess()
		{
			return ValidGAMSession(false);
		}

		private bool ValidSession()
		{
			return ValidGAMSession(true);
		}

		private bool ValidWebSession()
		{
#if NETCORE
			bool isExpired = IsFullAjaxRequest(localHttpContext) && this.AjaxOnSessionTimeout() == "Warn" && GxWebSession.IsSessionExpired(localHttpContext);
#else
			bool isExpired = IsFullAjaxRequest(HttpContext.Current) && this.AjaxOnSessionTimeout() == "Warn" && GxWebSession.IsSessionExpired(localHttpContext);
#endif
			if (isExpired)
			{
				GXLogging.Info(log, "440 Session timeout. Web Session has expired and GX' OnSessionTimeout' Pty is set to 'WARN'");
				this.SendResponseStatus(440, "Session Timeout");
			}
			return !isExpired;
		}

		private bool ValidGAMSession(bool bRedirectIfNotAuth)
		{
			bool isOK = false;
			if (IntegratedSecurityLevel == GAMSecurityLevel.SecurityObject)
			{
				String token = localHttpContext.Request.Headers["Authorization"];
				if (!string.IsNullOrEmpty(token))
				{
					token = token.Replace("OAuth ", string.Empty);
					GxSecurityProvider.Provider.checkaccesstoken(context, token, out isOK);
				}
				else
				{
					token = string.Empty;
					GxSecurityProvider.Provider.checksession(context, context.CleanAbsoluteUri, out isOK);
				}
				if (!isOK)
				{
#if NETCORE
					localHttpContext.Response.Headers[HttpHeader.AUTHENTICATE_HEADER] = HttpHelper.OatuhUnauthorizedHeader(context.GetServerName(), string.Empty, string.Empty);
#else
					HttpContext.Current.Response.AddHeader(HttpHeader.AUTHENTICATE_HEADER, HttpHelper.OatuhUnauthorizedHeader(context.GetServerName(), string.Empty, string.Empty));
#endif
					this.SendResponseStatus(401, "Unauthorized");
				}
			}
			else if (IntegratedSecurityLevel == GAMSecurityLevel.SecurityLow)
			{
				GxSecurityProvider.Provider.checksession(context, context.CleanAbsoluteUri, out isOK);

				if (!isOK)
				{
					string loginObject = GetGAMLoginWebObject();
					if (bRedirectIfNotAuth && !string.IsNullOrEmpty(loginObject))
						context.Redirect(loginObject, true);
				}
			}
			else if (IntegratedSecurityLevel == GAMSecurityLevel.SecurityHigh)
			{
				isOK = checkAuthorization(ExecutePermissionPrefix, context.CleanAbsoluteUri, bRedirectIfNotAuth);
			}
			return isOK;
		}

		public bool IsAuthorized(String permissionPrefix)
		{

			bool isOK = false;
			bool isPermissionOK;

#if NETCORE
			GxSecurityProvider.Provider.checksessionprm(context, localHttpContext.Request.Path + '?' + localHttpContext.Request.QueryString.Value, permissionPrefix, out isOK, out isPermissionOK);
#else
			GxSecurityProvider.Provider.checksessionprm(context, localHttpContext.Request.Url.PathAndQuery, permissionPrefix, out isOK, out isPermissionOK);
#endif

			if (isPermissionOK)
			{
				return true;
			}
			else
			{
				if (isOK)
				{
					return false;
				}
				else
				{
					string loginObject = GetGAMLoginWebObject();
					context.Redirect(loginObject, true);
					return false;
				}
			}

		}

		private bool checkAuthorization(string permissionPrefix, string reqUrl, bool bRedirectIfNotAuth)
		{
			bool isOK = false;
			bool isPermissionOK;
			GxSecurityProvider.Provider.checksessionprm(context, reqUrl, permissionPrefix, out isOK, out isPermissionOK);

			if (!isOK)
			{
				string loginObject = GetGAMLoginWebObject();
				if (bRedirectIfNotAuth && !string.IsNullOrEmpty(loginObject))
				{
					context.Redirect(loginObject, true);
					if (context.isAjaxRequest())
						context.DispatchAjaxCommands();
				}
			}
			else if (!isPermissionOK)
			{
				string notAuthorizedObject = GetGAMNotAuthorizedWebObject();
				if (!string.IsNullOrEmpty(notAuthorizedObject) && bRedirectIfNotAuth)
				{
					context.Redirect(notAuthorizedObject, true);
					if (context.isAjaxRequest())
						context.DispatchAjaxCommands();
				}
			}
			return isOK && isPermissionOK;
		}

		private string GetGAMLoginWebObject()
		{
			string loginObject = string.Empty;
			if (Config.GetValueOf("IntegratedSecurityLoginWeb", out loginObject))
			{
				string[] loginObjParts = loginObject.Split(',');
				if (loginObjParts.Length > 0)
					loginObject = AddExtension(loginObjParts[0]);
			}
			if (IsUploadRequest(this.localHttpContext))
				return formatLink($"{context.GetScriptPath()}{loginObject}");
			else
				return formatLink(loginObject);
		}
		private string GetGAMNotAuthorizedWebObject()
		{
			string loginObject = string.Empty;
			if (Config.GetValueOf("IntegratedSecurityNotAuthorizedWeb", out loginObject))
			{
				string[] loginObjParts = loginObject.Split(',');
				if (loginObjParts.Length > 0)
					loginObject = AddExtension(loginObjParts[0]);
			}
			if (IsUploadRequest(this.localHttpContext))
				return formatLink($"{context.GetScriptPath()}{loginObject}");
			else
				return formatLink(loginObject);
		}
		private string AddExtension(string objectName)
		{
#if NETCORE
			return objectName;
#else
			return $"{objectName}{HttpHelper.ASPX}";
#endif
		}

		private void SendHeaders()
		{
			sendCacheHeaders();
			GXLogging.DebugSanitized(log, "HttpHeaders: ", () => DumpHeaders(localHttpContext));
			sendAdditionalHeaders();
			HttpHelper.CorsHeaders(localHttpContext);
			HttpHelper.AllowHeader(localHttpContext, new List<string>() { $"{HttpMethod.Get.Method},{HttpMethod.Post.Method}" });
		}

		protected virtual void sendCacheHeaders()
		{
			if (IsSpaRequest())
			{
#if NETCORE
				localHttpContext.Response.AddHeader("Cache-Control", HttpHelper.CACHE_CONTROL_HEADER_NO_CACHE_REVALIDATE);
#else
				localHttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
				localHttpContext.Response.Cache.AppendCacheExtension("no-store, must-revalidate");
#endif
				localHttpContext.Response.AppendHeader("Pragma", "no-cache");
				localHttpContext.Response.AppendHeader("Expires", "0");
			}
			else
			{
				string utcNow = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH':'mm':'ss 'GMT'", CultureInfo.GetCultureInfo("en-US"));
				localHttpContext.Response.AddHeader("Expires", utcNow);
				localHttpContext.Response.AddHeader("Last-Modified", utcNow);
				localHttpContext.Response.AddHeader("Cache-Control", HttpHelper.CACHE_CONTROL_HEADER_NO_CACHE_REVALIDATE);
			}
		}
		const string IE_COMP_EmulateIE7 = "EmulateIE7";
		const string IE_COMP_Edge = "edge";
		public virtual void sendAdditionalHeaders()
		{
			if (IsSpaRequest())
				sendSpaHeaders();
			if (context.GetBrowserType() == GxContext.BROWSER_IE && !context.isPopUpObject())
			{
				string IECompMode = string.Empty;
				Config.GetValueOf("IE_COMPATIBILITY_VIEW", out IECompMode);
				if (!string.IsNullOrEmpty(IECompMode))
				{
					if (IECompMode.Equals(IE_COMP_EmulateIE7) && !context.GetBrowserVersion().StartsWith("8")) //compatibility
						return;

					string safeIECompMode = IE_COMP_Edge.Equals(IE_COMP_EmulateIE7) ? IE_COMP_Edge : IE_COMP_Edge;
					localHttpContext.Response.Headers["X-UA-Compatible"] = "IE=" + safeIECompMode;
				}
			}

		}

		protected virtual void sendSpaHeaders()
		{
			localHttpContext.Response.Headers[GX_SPA_GXOBJECT_RESPONSE_HEADER] = GetPgmname().ToLower();
		}

		private void webExecuteWorker(object target)
		{
			HttpContext httpContext = (HttpContext)target;
#if !NETCORE
			HttpContext.Current = httpContext;
#endif
			try
			{
				webExecuteEx(httpContext);
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Error in webExecuteWorker", e);
				try
				{
					context.CloseConnections();
				}
				catch { }
				workerException = e;
			}
		}

		public bool IsReusable
		{
			get { return false; }
		}


		protected virtual void SetCompression(HttpContext httpContext)
		{
#if NETCORE
			if (CompressHtmlResponse())
#else
			if (CompressHtmlResponse() && httpContext.Response.BufferOutput)
#endif
			{
				GXUtil.SetGZip(httpContext);
			}
		}

		public virtual bool CompressHtmlResponse()
		{
			return GXUtil.CompressResponse();
		}
#if !NETCORE
		protected override void Render(HtmlTextWriter output)
		{
			localHttpContext = Context;
			ControlOutputWriter = output;
			LoadParameters(Parms);
			InitPrivates();
			webExecuteEx(localHttpContext);
		}
#endif
		public void InitPrivates()
		{
			context.GX_msglist = new msglist();
			context.httpAjaxContext.setMsgList(context.GX_msglist);
			context.httpAjaxContext.context = context;
			context.localCookies = new HttpCookieCollection();
			context.httpAjaxContext.LoadFormVars(localHttpContext);
			_isStatic = false;
#if !NETCORE
			GXWebNotification.Start();
			if (this is IReadOnlySessionState) //It must be first because IReadOnlySessionState extends IRequiresSessionState 
				context.httpAjaxContext.SessionType = SessionType.RO_SESSION;
			else if (this is IRequiresSessionState)
				context.httpAjaxContext.SessionType = SessionType.RW_SESSION;
			else
				context.httpAjaxContext.SessionType = SessionType.NO_SESSION;
#endif
		}

		protected void LoadParameters(string value)
		{
			initpars();
			_params.Clear();
			_namedParms.Clear();
			string parmValue;

			if (!string.IsNullOrEmpty(value))
			{
				value = GxContext.RemoveInternalSuffixes(value).TrimStart('?');
				useOldQueryStringFormat = !(Preferences.UseNamedParameters && value.Contains("="));
				if (!string.IsNullOrEmpty(value))
				{
					string[] elements = useOldQueryStringFormat ? value.Split(',') : value.Split('&');

					for (int i = 0; i < elements.Length; i++)
					{

						if (useOldQueryStringFormat)
							_params.Add(GXUtil.UrlDecode(elements[i]));
						else
						{
							string[] parmNameValue = elements[i].Split('=');
							if (parmNameValue.Length > 1)
							{
								parmValue = GXUtil.UrlDecode(parmNameValue[1]);
								_namedParms[NormalizeParameterName(parmNameValue[0])] = parmValue;
							}
							else
							{
								parmValue = GXUtil.UrlDecode(parmNameValue[0]);
							}
							_params.Add(parmValue);
						}
					}
				}
			}

			if (localHttpContext.Request.GetMethod() == "POST"
								&& _params.Count == 0) // If it is a call ajax made through a POST is has 1 parameter (the one used to avoid cache)
			{
				TryLoadAjaxCallParms();
			}
		}
		protected void TryLoadAjaxCallParms()
		{
			NameValueCollection postParms = localHttpContext.Request.GetParams(); //PostParams are automatically URL Decoded
			string gxEvent = postParms["GXEvent"];
			string gxAction = postParms["GXAction"];
			if (!string.IsNullOrEmpty(gxEvent))
			{
				Regex parmRE = new Regex("GXParm[0-9]+");
				_params.Clear();
				_params.Add(gxEvent);
				if (!string.IsNullOrEmpty(gxAction))
					_params.Add(gxAction);
				for (int i = 0; i < postParms.AllKeys.Length; i++)
				{
					string key = postParms.AllKeys[i];
					if (!string.IsNullOrEmpty(key) && parmRE.IsMatch(key))
					{
						_params.Add(postParms[i]);
					}
				}
			}
		}

		public virtual string getresponse(string sGXDynURL)
		{
			return string.Empty;
		}
		public virtual void setparameters(object[] parms)
		{
		}
		public void initpars()
		{
			_currParameter = -1;
		}
		public string GetNextPar()
		{
			_currParameter++;
			if (_currParameter < _params.Count)
				return _params[_currParameter];
			else
				return string.Empty;
		}
		public string GetPar(string parameterName)
		{
			if (useOldQueryStringFormat)
				return GetNextPar();
			else if (_namedParms.TryGetValue(NormalizeParameterName(parameterName), out string value))
				return value;
			else
				return string.Empty;
		}
		public string GetFirstPar(string parameterName)
		{
			if (useOldQueryStringFormat)
				return GetNextPar();
			else if (!firstParConsumed && _namedParms.TryGetValue(GXEVENT_PARM, out string value))
			{
				firstParConsumed = true;
				return value;
			}
			else return GetPar(parameterName);
		}
		string NormalizeParameterName(string parameterName)
		{
			if (!string.IsNullOrEmpty(parameterName))
				return parameterName.ToLower();
			else
				return parameterName;
		}
		public void SetQueryString(string value)
		{
			LoadParameters(value);
		}
		public virtual void skipLines(long nToSkip)
		{
			context.skipLines(nToSkip);
		}

		const char NULL_CHARACTER = (char)0;

		public string cgiGet(string sVar)
		{
			string sValue = null;
			try
			{
				if (GxContext.GetHttpRequestPostedFile(context, sVar, out sValue))
					return sValue;

				if (FormVars != null)
				{
					string[] vars = FormVars.GetValues(sVar);
					if (vars != null && vars.Length > 0)
						sValue = vars[0];
				}
				if (sValue == null)
				{
					return string.Empty;
				}

			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "cgiGet(" + sVar + ") Error.", ex);
				return string.Empty;
			}
			//Remove known malicious chars
			return sValue.Replace(NULL_CHARACTER.ToString(), null);
		}

		public string CGIGetFileName(string sVar)
		{
			return GxContext.GetHttpRequestPostedFileName(localHttpContext, sVar);
		}

		public string CGIGetFileType(string sVar)
		{
			return GxContext.GetHttpRequestPostedFileType(localHttpContext, sVar);
		}

		public void ChangePostValue(string sCtrl, string sValue)
		{
			FormVars[sCtrl] = sValue;
		}
		public void AddString(string sValue)
		{
			context.GX_webresponse.AddString(sValue);
		}
		public void AssignAttri(String CmpContext, bool IsMasterPage, String AttName, Object AttValue)
		{
			context.httpAjaxContext.ajax_rsp_assign_attri(CmpContext, IsMasterPage, AttName, AttValue);
		}
		public void AssignProp(String CmpContext, bool IsMasterPage, String Control, String Property, String Value, bool SendAjax = true)
		{
			context.httpAjaxContext.ajax_rsp_assign_prop(CmpContext, IsMasterPage, Control, Property, Value, SendAjax);
		}
		public void DeletePostValue(string sCtrl)
		{
			FormVars.Remove(sCtrl);
		}
		public void DeletePostValuePrefix(string sPrefix)
		{
			StringCollection toDelete = new StringCollection();
			foreach (string key in FormVars)
			{
				if (key != null && key.StartsWith(sPrefix + "nRC_GXsfl_", StringComparison.OrdinalIgnoreCase))
				{
					toDelete.Add(key);
				}
			}
			foreach (string key in toDelete)
			{
				FormVars.Remove(key);
			}
		}
#if !NETCORE
		public virtual string UrlEncode(string s)
		{
			return GXUtil.UrlEncode(s);
		}
#endif
		public void gxhtml_str()
		{
			_Write("<TR>");
		}
		public void gxhtml_etr()
		{
			_Write("</TR>");
		}
		public void flushBuffer()
		{

		}
#if !NETCORE
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
#endif
		public void Msg(string s)
		{
			GX_msglist.addItem(s);
		}
		public void Msg(msglist obj, string s)
		{
			obj.addItem(s);
		}
		////////////////////////////////////////////////////////////////////////////////////////

		static public bool isUrlName(string name)
		{
			if (name.IndexOf('?') > -1)   // Tiene querystring
			{
				return true;
			}
			if (name.IndexOf('.') > -1)   // extension aspx
			{
				if (name.EndsWith("aspx"))
					return true;
			}
			return false;
		}
		static public GXWebComponent getWebComponent(Object caller, string nameSpace, string name, Object[] ctorParms)
		{
			String objName = CleanObjectFromUrl(name.ToLower());
			GXWebComponent objComponent = null;

			if (!isUrlName(objName))
			{
				try
				{

					objComponent = (GXWebComponent)ClassLoader.GetInstance(objName, nameSpace + "." + objName, ctorParms);
				}
				catch { }
				try
				{

					if (objComponent == null)
#if NETCORE
						objComponent = (GXWebComponent)ClassLoader.CreateInstance(Assembly.GetEntryAssembly(), nameSpace + "." + objName, ctorParms);
#else
						objComponent = (GXWebComponent)ClassLoader.CreateInstance(Assembly.GetCallingAssembly(), nameSpace + "." + objName, ctorParms);
#endif
				}
				catch { }
			}
			if (objComponent == null)
			{

				string url = name;
				string completeName, queryParameters;
				name = ObjectSignatureFromUrl(url, out completeName, out queryParameters);

				if (url.Equals(name))
					return new GXErrorWebComponent(name);
				GXWebComponent webComp = getWebComponent(caller, nameSpace, name, ctorParms);
				object[] urlParms = webComp.GetParmsFromUrl(completeName, queryParameters);
				webComp.setParms(urlParms);
				objComponent = webComp;
			}
			return objComponent;
		}

		private static string CleanObjectFromUrl(string url)
		{
			if (url.StartsWith(URI_SEPARATOR))
			{
				int idx = url.LastIndexOf(URI_SEPARATOR);
				if (idx > 0)
					return url.Substring(idx + 1);
			}
			return url;
		}

		internal static string ObjectSignatureFromUrl(string url, out string completeName, out string queryParameters)
		{
			string name;
			try
			{
				url = CleanObjectFromUrl(url);
				Uri uri = new System.Uri(url, UriKind.RelativeOrAbsolute);
				if (!uri.IsAbsoluteUri)
					uri = new Uri("http://gxhost/" + url);
				name = uri.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
				queryParameters = uri.GetComponents(UriComponents.Query, UriFormat.UriEscaped);
			}
			catch (Exception)
			{
				string regexPattern = @"^(?<s1>(?<s0>[^:/\?#]+):)?(?<a1>"
				  + @"//(?<a0>[^/\?#]*))?(?<p0>[^\?#]*)"
				  + @"(?<q1>\?(?<q0>[^#]*))?"
				  + @"(?<f1>#(?<f0>.*))?";
				Regex r = new Regex(regexPattern);
				Match m = r.Match(url);
				name = m.Groups["p0"].Value;
				queryParameters = m.Groups["q0"].Value;
			}

			completeName = name;
			if (string.IsNullOrEmpty(name))
				return url;

			if (name.LastIndexOf('.') > 0)
				name = name.Substring(0, name.LastIndexOf('.'));
			return name;
		}

		public string prefixURL(string s)
		{
			return s;
		}
		public void eventLevelResetContext()
		{
		}

		public void getMultimediaValue(string InternalName, ref string BlobVar, ref string UriVar)
		{
			string Type = cgiGet(InternalName + "Option");
			if (Type == "file")
			{

				if (String.IsNullOrEmpty(StringUtil.RTrim(BlobVar)))
				{
					BlobVar = cgiGet(InternalName + "_gxBlob");
					UriVar = string.Empty;
				}

				if (!String.IsNullOrEmpty(StringUtil.RTrim(BlobVar)))
				{
					string filename = (String)(CGIGetFileName(InternalName));
					string filetype = (String)(CGIGetFileType(InternalName));
					UriVar = FileUtil.GetCompleteFileName(filename, filetype);
				}
			}
			else
			{
				Match match = MULTIMEDIA_GXI_GRID_PATTERN.Match(InternalName);
				if (match.Success)
				{
					UriVar = cgiGet(match.Groups[1] + "_GXI" + match.Groups[2]);
				}
				else
				{
					UriVar = cgiGet(InternalName + "_GXI");
				}
				BlobVar = string.Empty;
			}
		}

#if !NETCORE
		public void SetCallTarget(string objClass, string target)
		{
			callTargetsByObject[objClass.ToLower().Replace("\\", ".")] = target.ToLower();
		}
		private string GetCallTargetFromUrl(string url)
		{
			Uri parsedUri;
			if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out parsedUri))
			{
				if (parsedUri.IsAbsoluteUri || (!parsedUri.IsAbsoluteUri && Uri.TryCreate(context.HttpContext.Request.Url, url, out parsedUri)))
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
			return string.Empty;
		}

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
		private string RemoveExtensionFromUrlPath(string urlPath)
		{
			if (urlPath.EndsWith(HttpHelper.ASPX))
				return urlPath.Substring(0, urlPath.Length - 5);
			return urlPath;
		}
		private bool ShouldLoadTarget(string target)
		{
			return (target == "top" || target == "right" || target == "bottom" || target == "left");
		}
#endif

		private XMLPrefixes currentNamespacePrefixes = new XMLPrefixes();

		public void SetNamedPrefixesFromReader(GXXMLReader rdr)
		{
			currentNamespacePrefixes.SetNamedPrefixesFromReader(rdr);
		}
		public void SetPrefixesFromReader(GXXMLReader rdr)
		{
			currentNamespacePrefixes.SetPrefixesFromReader(rdr);
		}
		public Dictionary<string, string> GetPrefixesInContext()
		{
			return currentNamespacePrefixes.GetPrefixes();
		}
		public void SetPrefixes(Dictionary<string, string> pfxs)
		{
			currentNamespacePrefixes.SetPrefixes(new Dictionary<string, string>(pfxs));
		}

		public void PrepareForReuse()
		{
			((GxContext)this.context).ClearJavascriptSources();
		}
#if !NETCORE
		public virtual void handleException(String gxExceptionType, String gxExceptionDetails, String gxExceptionStack)
		{

		}
#endif

		private Diagnostics.GXDebugInfo dbgInfo;
		protected void initialize(int objClass, int objId, int dbgLines, long hash)
		{
			dbgInfo = Diagnostics.GXDebugManager.Instance?.GetDbgInfo(context, objClass, objId, dbgLines, hash);
		}

		protected void trkCleanup() => dbgInfo?.OnCleanup();

		protected void trk(int lineNro) => dbgInfo?.Trk(lineNro);
		protected void trk(int lineNro, int colNro) => dbgInfo?.Trk(lineNro, colNro);
		protected void trkrng(int lineNro, int lineNro2) => dbgInfo?.TrkRng(lineNro, 0, lineNro2, 0);
		protected void trkrng(int lineNro, int colNro, int lineNro2, int colNro2) => dbgInfo?.TrkRng(lineNro, colNro, lineNro2, colNro2);
	}
	public abstract class GXWebComponent : GXHttpHandler
	{

		public abstract void componentstart();
		public abstract void componentdraw();
		public abstract void componentprepare(Object[] parms);
		public abstract void componentbind(Object[] values);
		public abstract String getstring(String s);

		public bool IsUrlCreated()
		{
			return mFixedParms != null;
		}

		string _prefixId;
		object[] mFixedParms;
		bool justCreated;

		public bool GetJustCreated()
		{
			return justCreated;
		}

		public void setjustcreated()
		{
			justCreated = true;
		}

		public void setparmsfromurl(string url)
		{
			string parameters, completeName;
			ObjectSignatureFromUrl(url, out completeName, out parameters);
			if (parameters != null)
			{
				object[] urlParms = GetParmsFromUrl(completeName, parameters);
				setParms(urlParms);
			}
		}

		public void setParms(object[] parms)
		{
			if (parms != null)
			{
				object[] FixedParms = new object[parms.Length];
				ParameterInfo[] pars = GetType().GetMethod("execute").GetParameters();
				for (int i = 0; i < pars.Length; i++)
				{
					if (i >= parms.Length)
						break;
					FixedParms[i] = convertparm(pars, i, parms[i]);
				}
				mFixedParms = FixedParms;
			}
		}
		internal object[] GetParmsFromUrl(string completeName, string queryParameters)
		{
			if (!string.IsNullOrEmpty(queryParameters))
			{

				EncryptionType keySourceType = GetEncryptionType();
				string encKey;
				switch (keySourceType)
				{
					case EncryptionType.SESSION:
						encKey = Crypto.Decrypt64(context.GetCookie("GX_SESSION_ID"), Crypto.GetServerKey());
						break;
					case EncryptionType.SITE:
						encKey = Crypto.GetSiteKey();
						break;
					default:
						encKey = null;
						break;
				}

				queryParameters = GXUtil.DecryptParm(queryParameters, encKey);

				// Remove "salt" part from parameter
				if (!String.IsNullOrEmpty(encKey) && queryParameters.StartsWith(completeName))
				{
					queryParameters = queryParameters.Substring(completeName.Length);
				}
				object[] parms = HttpHelper.GetParameterValues(queryParameters);
				for (int i = 0; i < parms.Length; i++)
					parms[i] = GXUtil.UrlDecode((string)parms[i]);

				return parms;
			}
			else
			{
				return Array.Empty<Object>();
			}
		}

		public override object getParm(object[] parms, int index)
		{
			if (mFixedParms == null)
				return parms[index];
			if (index == 0)
			{
				object[] newFixedParms = new object[mFixedParms.Length + parms.Length];
				parms.CopyTo(newFixedParms, 0);
				mFixedParms.CopyTo(newFixedParms, parms.Length);
				mFixedParms = newFixedParms;
			}

			return mFixedParms[index];
		}

		public virtual string Name
		{
			get { return _prefixId; }
			set { _prefixId = value; }
		}

		protected virtual EncryptionType GetEncryptionType()
		{
			return EncryptionType.NONE;
		}
		public void ComponentInit()
		{
			IsMain = false;
			createObjects();
			initialize();
		}
#if !NETCORE
		protected override void Render(HtmlTextWriter output)
		{
			ControlOutputWriter = output;
			localHttpContext = Context;
			LoadParameters(Parms);
			InitPrivates();
			SetPrefix(_prefixId + "_");         // Load Prefix from Name property
			initpars();                         // Initialize Iterator Parameters
			webExecuteEx(localHttpContext);
		}
#endif
		public virtual void componentdrawstyles()
		{
		}
		public virtual void componentrestorestate(string sPPrefix, string sPSFPrefix)
		{
		}
		public virtual void componentprocess(string sPPrefix, string sPSFPrefix)
		{
		}
		public virtual void componentprocess(string sPPrefix, string sPSFPrefix, string sEvt)
		{
		}
		public virtual void componentjscripts()
		{
		}
		public virtual void componentthemes()
		{
		}
	}

	public class GXErrorWebComponent : GXWebComponent
	{
		string _DllName;
		public GXErrorWebComponent(string s)
		{
			context = new GxContext();
#if NETCORE
			ControlOutputWriter = new HtmlTextWriter(context.HttpContext);
#else
			ControlOutputWriter = new HtmlTextWriter(context.HttpContext.Response.Output);
#endif
			_DllName = s;
		}
		public override void componentstart() { }
		public override void componentdraw()
		{
			context.WriteHtmlText(string.Format("ERROR: {0} is not a web component or could not load {0}.dll ", _DllName));
		}
		public override String getstring(String s) { return string.Empty; }
		public override void componentdrawstyles() { }
		public override void componentprocess(string sPPrefix, string sPSFPrefix, string sEvt) { }
		public override void componentprocess(string sPPrefix, string sPSFPrefix) { }
		public override void SetPrefix(string s) { }
		public override void componentprepare(Object[] parms) { }
		public override void componentbind(Object[] values) { }
		public override void webExecute() { }
		public void execute() { }
		public override void initialize() { }
		protected override void createObjects() { }
	}

	public class GXNullWebComponent : GXWebComponent
	{
		public GXNullWebComponent() { }
		public override void componentstart() { }
		public override void componentdraw() { }
		public override String getstring(String s) { return string.Empty; }
		public override void componentdrawstyles() { }
		public override void componentprocess(string sPPrefix, string sPSFPrefix, string sEvt) { }
		public override void componentprocess(string sPPrefix, string sPSFPrefix) { }
		public override void SetPrefix(string s) { }
		public override void componentprepare(Object[] parms) { }
		public override void componentbind(Object[] values) { }
		public override void webExecute() { }
		public override void initialize() { }
		public override string Name
		{
			get { return string.Empty; }
		}
		protected override void createObjects() { }
	}

	public abstract class GXMasterPage : GXHttpHandler
	{
		GXDataArea DataAreaObject;
		private bool _ShowMPWhenPopUp;

		public bool ShowMPWhenPopUp()
		{
			return _ShowMPWhenPopUp;
		}

		override public bool IsMasterPage()
		{
			return true;
		}

		public void setDataArea(GXDataArea DataAreaObject, bool ShowMPWhenPopUp)
		{
			this.DataAreaObject = DataAreaObject;
			this._ShowMPWhenPopUp = ShowMPWhenPopUp;
			this.context = DataAreaObject.context;
		}

		public void setDataArea(GXDataArea DataAreaObject)
		{
			this.DataAreaObject = DataAreaObject;
			this._ShowMPWhenPopUp = true;
			this.context = DataAreaObject.context;
		}

		public GXDataArea getDataAreaObject()
		{
			return DataAreaObject;
		}
		public override bool isPortletMode()
		{ return context.isPortletMode(); }

		public override bool isAjaxCallMode()
		{ return context.isAjaxCallMode(); }

		public override bool isAjaxEventMode()
		{ return context.isAjaxEventMode(); }

		public override bool isFullAjaxMode()
		{ return FullAjaxMode; }

		protected override void setPortletMode()
		{ context.setPortletMode(); }

		protected override void setAjaxCallMode()
		{ context.setAjaxCallMode(); }

		protected override void setAjaxEventMode()
		{ context.setAjaxEventMode(); }

		protected override void setFullAjaxMode()
		{ FullAjaxMode = true; }

		public virtual void master_styles() { }
	}
	public abstract class GXDataArea : GXHttpHandler
	{
		abstract public short ExecuteStartEvent();
		abstract public void RenderHtmlHeaders();
		abstract public void RenderHtmlOpenForm();
		abstract public void RenderHtmlCloseForm();
		abstract public void RenderHtmlContent();
		abstract public void DispatchEvents();
		abstract public String GetSelfLink();
		abstract public bool HasEnterEvent();
		abstract public GXWebForm GetForm();

		protected GXMasterPage MasterPageObj { get; set; }

		protected override void ValidateSpaRequest()
		{
			string sourceMasterPage = localHttpContext.Request.Headers[GX_SPA_MASTERPAGE_HEADER];
			if (IsSpaRequest() && (String.IsNullOrEmpty(sourceMasterPage) || sourceMasterPage.ToLower() != MasterPageObj.GetPgmname().ToLower()))
			{
				context.DisableSpaRequest();
				sendSpaHeaders();
			}
			if (MasterPageObj != null)
			{
				localHttpContext.Response.Headers[GX_SPA_MASTERPAGE_HEADER] = MasterPageObj.GetPgmname();
			}
		}
	}
	public class GXDataAreaControl
	{
		GXDataArea DataAreaObject;

		public GXDataAreaControl()
		{
		}

		public void setDataArea(GXDataArea DataAreaObject)
		{
			this.DataAreaObject = DataAreaObject;
		}

		public virtual string Pgmname
		{
			get
			{
				return DataAreaObject.GetPgmname();
			}
		}

		public virtual string Pgmdesc
		{
			get
			{
				return DataAreaObject.GetPgmdesc();
			}
		}
	}

	public class GXWebForm
	{
		GXRadio meta = new GXRadio();
		GXRadio metaequiv = new GXRadio();
		GxStringCollection jscriptsrc = new GxStringCollection();
		string caption = string.Empty;
		int backcolor;
		int textcolor;
		string background = string.Empty;
		int visible;
		int windowstate;
		int enabled;
		int top;
		int left;
		int width;
		int height;
		string internalname = string.Empty;
		string bitmap = string.Empty;
		string tag = string.Empty;
		string _class = string.Empty;
		string headerrawhtml = string.Empty;

		public GXRadio Meta
		{
			get { return meta; }
			set { meta = value; }
		}

		public GXRadio Metaequiv
		{
			get { return metaequiv; }
			set { metaequiv = value; }
		}

		public GxStringCollection Jscriptsrc
		{
			get { return jscriptsrc; }
			set { jscriptsrc = value; }
		}
		public string Caption
		{
			get { return caption; }
			set { caption = value; }
		}
		public int Backcolor
		{
			get { return backcolor; }
			set { backcolor = value; }
		}
		public int Textcolor
		{
			get { return textcolor; }
			set { textcolor = value; }
		}
		public string Background
		{
			get { return background; }
			set { background = value; }
		}
		public int Visible
		{
			get { return visible; }
			set { visible = value; }
		}
		public int Windowstate
		{
			get { return windowstate; }
			set { windowstate = value; }
		}
		public int Enabled
		{
			get { return enabled; }
			set { enabled = value; }
		}
		public int Top
		{
			get { return top; }
			set { top = value; }
		}
		public int Left
		{
			get { return left; }
			set { left = value; }
		}
		public int Width
		{
			get { return width; }
			set { width = value; }
		}
		public int Height
		{
			get { return height; }
			set { height = value; }
		}
		public string Internalname
		{
			get { return internalname; }
			set { internalname = value; }
		}
		public string Bitmap
		{
			get { return bitmap; }
			set { bitmap = value; }
		}
		public string Tag
		{
			get { return tag; }
			set { tag = value; }
		}
		public string Class
		{
			get { return _class; }
			set { _class = value; }
		}
		public string Headerrawhtml
		{
			get { return headerrawhtml; }
			set { headerrawhtml = value; }
		}

		public static void AddResponsiveMetaHeaders(GXRadio meta)
		{
			TryAddMetaHeader(meta, "viewport", "width=device-width, initial-scale=1, maximum-scale=4");
			TryAddMetaHeader(meta, "mobile-web-app-capable", "yes");
		}

		private static void TryAddMetaHeader(GXRadio meta, string key, string value)
		{
			ListItem item = meta.Items.FindByValue(key);
			if (item == null)
			{
				meta.addItem(key, value, 0);
			}
		}
	}


}



