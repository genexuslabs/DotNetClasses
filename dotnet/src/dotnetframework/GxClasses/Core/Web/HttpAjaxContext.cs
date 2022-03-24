namespace GeneXus.Http
{
	using System;
	using System.IO;
	using Jayrock.Json;
	using System.Web;
	using GeneXus.Application;
	using GeneXus.Utils;
	using System.Text;
	using System.Collections;
	using System.Collections.Generic;
	using GeneXus.Encryption;
	using System.Collections.Specialized;
	using System.Globalization;
#if !NETCORE
	using GeneXus.WebControls;
#else
	using Microsoft.AspNetCore.Http;
#endif
	using System.Diagnostics;
	using GeneXus.Configuration;
	using log4net;

	public interface IHttpAjaxContext
	{
		void appendAjaxCommand(String cmtType, Object cmdData);
		void ajax_rspStartCmp( String CmpId);
		void ajax_rspEndCmp();
		void ajax_rsp_assign_attri( String CmpPrefix, bool IsMasterPage, String AttName, Object AttValue);
        void ajax_rsp_assign_sdt_attri(String CmpPrefix, bool IsMasterPage, String AttName, Object SdtObj);
#if !NETCORE
		void ajax_sending_grid_row(GXWebRow row);
#endif
		void ajax_rsp_assign_boolean_hidden(String AttName, bool AttValue);
		void ajax_rsp_assign_hidden(String AttName, String AttValue);
		void ajax_rsp_assign_hidden_sdt(String SdtName, Object SdtObj);		
		void ajax_rsp_assign_prop(String CmpPrefix, bool IsMasterPage, String Control, String Property, String Value, bool sendAjax);
		void ajax_rsp_assign_uc_prop(String CmpPrefix, bool IsMasterPage, String Control, String Property, String Value);
#pragma warning disable CA1707 // Identifiers should not contain underscores
        void ajax_rsp_assign_grid(String ControlName, Object GridObj, String Control);
		void ajax_rsp_assign_grid(String ControlName, Object GridObj);
#pragma warning restore CA1707 // Identifiers should not contain underscores
		void AddStylesheetToLoad(String url);
		void AddStylesHidden();
		void ajax_rsp_clear();

		JArray AttValues { get;}
		JObject HiddenValues { get;}
		JArray PropValues { get; }
		JObject WebComponents { get;}
		JObject Messages { get;}
		JArray Grids { get; }
		void LoadFormVars(HttpContext localHttpContext);
		void disableJsOutput();
		void enableJsOutput();
	}

	internal enum SessionType
	{
		RW_SESSION,
		RO_SESSION,
		NO_SESSION,
	}

	public class HttpAjaxContext : IHttpAjaxContext
	{
        private IGxContext _context;
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(HttpAjaxContext));
		private Stack cmpContents = new Stack();
		private GXAjaxCommandCollection commands = new GXAjaxCommandCollection();
		private JArray _AttValues = new JArray();
		private JObject _HiddenValues = new JObject();
		private JArray _PropValues = new JArray();
		private JObject _WebComponents = new JObject();
		private Hashtable _LoadCommands = new Hashtable();
		private JObject _Messages = new JObject();
		private JArray _Grids = new JArray();
		private Dictionary<String, int> DicGrids = new Dictionary<String, int>();
		private JObject _ComponentObjects = new JObject();
		private JArray _StylesheetsToLoad = new JArray();
        private NameValueCollection _formVars;
#if !NETCORE
		private GXWebRow _currentGridRow;
#endif
		private bool _isJsOutputEnabled = true;

		internal SessionType SessionType { get; set; }

		public string FormCaption { get; set; }

		public JObject HiddenValues
		{ get { return _HiddenValues; } }
		public JArray AttValues
		{ get { return _AttValues; } }
		public JArray PropValues
		{ get { return _PropValues; } }
		public JObject WebComponents
		{ get { return _WebComponents; } }
		public Hashtable LoadCommands
		{ get { return _LoadCommands; } }
		public JObject Messages
		{ get {
				return _Messages;
			}
		}
		public JArray Grids
		{ get { return _Grids; } }
		public JObject ComponentObjects
		{ get { return _ComponentObjects; } }
		public GXAjaxCommandCollection Commands
		{ get { return commands; } }

        public NameValueCollection FormVars
        {
            get { return _formVars; }
            set { _formVars = value; }
        }

        public IGxContext context
        { 
            set { _context = value; }
            get { return _context; }
        }

		public bool isJsOutputEnabled
		{
			get { return _isJsOutputEnabled; }
			set { _isJsOutputEnabled = value; }
		}

		protected msglist GX_msglist;

		public void setMsgList( msglist msglist)
		{
			this.GX_msglist = msglist;
		}

		public void writeAjaxContent( String Content)
		{
            _context.PushAjaxCmpContent(Content);
            if (context.CmpDrawLvl > 0)
                ((GXCmpContent)cmpContents.Peek()).Content += Content;
		}

		public bool isAjaxContent()
		{
            return (context.CmpDrawLvl > 0 && (context.isAjaxEventMode() || context.isAjaxCallMode()));
		}

		public void appendAjaxCommand(String cmdType, Object cmdData)
		{
			commands.AppendCommand(new GXAjaxCommand(cmdType, cmdData));
		}

		public void appendLoadData(int SId, JObject Data)
		{
			LoadCommands[SId] = Data;
		}

		public void executeUsercontrolMethod(String CmpContext, bool IsMasterPage, String containerName, String methodName, String input, Object[] parms)
		{
			GXUsercontrolMethod method = new GXUsercontrolMethod(CmpContext, IsMasterPage, containerName, methodName, input, parms);
			commands.AppendCommand(new GXAjaxCommand("ucmethod", method.GetJSONObject()));
		}

		public void setExternalObjectProperty(String cmpContext, bool isMasterPage, String objectName, String propertyName, object value)
		{
			var obj = new JObject();
			obj.Put("CmpContext", cmpContext);
			obj.Put("IsMasterPage", isMasterPage);
			obj.Put("ObjectName", objectName);
			obj.Put("PropertyName", propertyName);
			obj.Put("Value", value);
			commands.AppendCommand(new GXAjaxCommand("exoprop", obj));
		}

		public void executeExternalObjectMethod(String cmpContext, bool isMasterPage, String objectName, String methodName, object[] parms, bool isEvent)
		{
			var obj = new JObject();
			obj.Put("CmpContext", cmpContext);
			obj.Put("IsMasterPage", isMasterPage);
			obj.Put("ObjectName", objectName);
			obj.Put("Method", methodName);
			obj.Put("Parms", HttpAjaxContext.GetParmsJArray(parms));
			obj.Put("IsEvent", isEvent);
			commands.AppendCommand(new GXAjaxCommand("exomethod", obj));
		}

		public void ajax_rspStartCmp(String CmpId)
		{
			if (isJsOutputEnabled)
			{
				try
				{
					WebComponents.Put(CmpId, "");
				}
				catch (Exception ex)
				{
					GXLogging.Error(log, "ajax_rspStartCmp error", ex);
				}
			}
			context.CmpDrawLvl++;
            cmpContents.Push(new GXCmpContent(CmpId));
		}

		public void ajax_rspEndCmp()
		{
			context.CmpDrawLvl--;
			try
			{
				GXCmpContent cmp = (GXCmpContent)cmpContents.Pop();
				WebComponents.Put(cmp.Id, cmp.Content);
				if (context.isSpaRequest())
				{
					if (context.CmpDrawLvl > 0)
						((GXCmpContent)cmpContents.Peek()).Content += cmp.Content;
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "ajax_rspEndCmp error", ex);
			}
		}

		private JObject GetGxObject(JArray array, String CmpContext, bool IsMasterPage)
        {
            try
            {
                JObject obj;
				for (int i = 0; i < array.Count; i++)
                {
					obj = array.GetObject(i);
                    if (obj["CmpContext"].ToString().Equals(CmpContext) && obj["IsMasterPage"].ToString().Equals(IsMasterPage.ToString()))
                    {
                        return obj;
                    }
                }
                obj = new JObject();
                obj.Put("CmpContext", CmpContext);
                obj.Put("IsMasterPage", IsMasterPage.ToString());
				array.Add(obj);
                return obj;
            }
			catch (Exception ex)
			{
				GXLogging.Error(log, "GetGxObject error", ex);
			}

			return null;
        }        

        public void ajax_rsp_assign_attri(String CmpContext, bool IsMasterPage, String AttName, Object AttValue)
		{
			if (isJsOutputEnabled)
			{
				if (!context.isSpaRequest() || (context.isSpaRequest() && String.IsNullOrEmpty(CmpContext)))
				{
					try
					{
						JObject obj = GetGxObject(AttValues, CmpContext, IsMasterPage);
						if (obj != null)
						{
							obj.Put(AttName, AttValue);
						}
					}
					catch (Exception ex)
					{
						GXLogging.Error(log, "ajax_rsp_assign_attri error", ex);
					}
				}
			}
		}
        public void ajax_rsp_assign_sdt_attri(String CmpContext, bool IsMasterPage, String AttName, Object SdtObj)
        {
			if (isJsOutputEnabled)
			{
				if (!context.isSpaRequest() || (context.isSpaRequest() && String.IsNullOrEmpty(CmpContext)))
				{
					try
					{
						JObject obj = GetGxObject(AttValues, CmpContext, IsMasterPage);
						if (obj != null)
						{
							IGxJSONAble SdtObjJson = SdtObj as IGxJSONAble;
							if (SdtObjJson != null)
							{
								obj.Put(AttName, SdtObjJson.GetJSONObject());
							}
							else
							{
								Array array = SdtObj as Array;
								if (array != null)
								{
									JArray jArray = new JArray(array);
									obj.Put(AttName, jArray);
								}
							}
						}
					}
					catch (Exception ex)
					{
						GXLogging.Error(log, "ajax_rsp_assign_sdt_attri error", ex);
					}
				}
			}
        }
#if !NETCORE
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
		public void ajax_rsp_assign_boolean_hidden(String AttName, bool AttValue)
		{
			ajax_rsp_assign_hidden(AttName, (object)AttValue);
		}
		public void ajax_rsp_assign_hidden(String AttName, String AttValue)
		{
			ajax_rsp_assign_hidden(AttName, (object)AttValue);
		}
		private void ajax_rsp_assign_hidden(String AttName, object AttValue)
		{
			try
			{
#if !NETCORE
				if (_currentGridRow != null)
					_currentGridRow.AddHidden(AttName, AttValue);
				else
#endif
					HiddenValues.Put(AttName, AttValue);
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "ajax_rsp_assign_hidden error", ex);
			}
		}
		public void ajax_rsp_assign_hidden_sdt(String SdtName, Object SdtObj)
		{
			try
			{
				IGxJSONAble SdtObjJson = SdtObj as IGxJSONAble;
				if (SdtObjJson != null)
				{
					HiddenValues.Put(SdtName, SdtObjJson.GetJSONObject());
				}
				else
				{
					Array array = SdtObj as Array;
					if (array != null)
					{
						JArray jArray = new JArray(array);
						HiddenValues.Put(SdtName, jArray);
					}
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "ajax_rsp_assign_hidden_sdt error", ex);
			}

		}

        public string ajax_rsp_get_hiddens()
        {
            return HiddenValues.ToString();
        }

        private JObject GetControlProps(JObject obj, String Control)
		{
			JObject ctrlProps = null;
			try
			{
				ctrlProps = obj[Control] as JObject;
				if (ctrlProps == null)
				{
					ctrlProps = new JObject();
					obj.Put(Control, ctrlProps);
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "GetControlProps error", ex);
			}
			return ctrlProps; 
		}

		public void ajax_rsp_assign_prefixed_prop(String Control, String Property, String Value)
		{
			// Already prefixed control properties are sent in the master page object.
			ajax_rsp_assign_prop("", true, Control, Property, Value);
		}

		public void ajax_rsp_clear()
		{ 
			_PropValues = new JArray();
		}

		public void ajax_rsp_assign_prop(String CmpContext, bool IsMasterPage, String Control, String Property, String Value, bool SendAjax = true)
		{
			if (SendAjax && ShouldLogAjaxControlProperty(Property))
			{
				if (!context.isSpaRequest() || (context.isSpaRequest() && String.IsNullOrEmpty(CmpContext)))
				{
					try
					{
						// Avoid sending to the client side tmp media directory paths
						if (Property == "URL" && (Value.StartsWith(Preferences.getTMP_MEDIA_PATH()) || Value.StartsWith(context.PathToRelativeUrl(Preferences.getTMP_MEDIA_PATH()))))
						{
							return;
						}
						if (Control == "FORM" && Property == "Caption")
						{
							FormCaption = Value;
						}
						JObject obj = GetGxObject(PropValues, CmpContext, IsMasterPage);
						if (obj != null)
						{
							JObject ctrlProps = GetControlProps(obj, Control);
							if (ctrlProps != null)
							{
								ctrlProps.Put(Property, Value);
							}
						}
						if (!context.isAjaxRequest())
						{
							ajax_rsp_assign_hidden(Control + "_" + Property.Substring(0, 1) + Property.Substring(1).ToLower(), Value);
						}
					}
					catch (Exception ex)
					{
						GXLogging.Error(log, "ajax_rsp_assign_prop error", ex);
					}
				}
			}
		}

		public void ajax_rsp_assign_uc_prop(String CmpContext, bool IsMasterPage, String Control, String Property, String Value)
		{
			ajax_rsp_assign_prop(CmpContext, IsMasterPage, Control, Property, Value);

			if (!context.isAjaxRequest())
			{
				ajax_rsp_assign_hidden(Control + "_" + Property, Value);
			}
		}

		public void ajax_rsp_assign_grid(String ControlName, Object GridObj) {
			try
			{
				Grids.Add(((IGxJSONAble)GridObj).GetJSONObject());
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "ajax_rsp_assign_grid error", ex);
			}
		}

		public void ajax_rsp_assign_grid(String GridName, Object GridObj, String Control)
        {
            try
            {
				if (DicGrids.ContainsKey(Control))
				{
					Grids[DicGrids[Control]] = ((IGxJSONAble)GridObj).GetJSONObject();
				}
				else
				{
					Grids.Add(((IGxJSONAble)GridObj).GetJSONObject());
					DicGrids.Add(Control, Grids.Length - 1);
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "ajax_rsp_assign_grid error", ex);
			}
		}

		private bool ShouldLogAjaxControlProperty(string Property)
		{
			return isJsOutputEnabled || (context.isSpaRequest() && Property == "Enabled");
		}

		public void AddStylesheetToLoad(String url)
		{
			_StylesheetsToLoad.Add(url);
		}

		public void AddStylesHidden()
		{
			if (_StylesheetsToLoad.Count > 0)
			{
				HiddenValues.Put("GX_STYLE_FILES", _StylesheetsToLoad);
			}
		}

		public void PrintReportAtClient(string reportFile, string printerRule)
		{
            JObject obj = new JObject();
            try
            {
                obj.Put("reportFile", reportFile);
                obj.Put("printerRule", printerRule);
            }
			catch (Exception ex)
			{
				GXLogging.Error(log, "PrintReportAtClient error", ex);
			}
			commands.AppendCommand(new GXAjaxCommand("print", obj));
		}

		public void AddResourceProvider(string provider)
		{
			HiddenValues.Put("GX_RES_PROVIDER", provider);
		}

		public void AddThemeHidden(string theme)
		{
			HiddenValues.Put("GX_THEME", theme);
		}

        public void AddNavigationHidden()
        {
			GxContext ctx = ((GxContext)context);
            if (ctx.IsLocalStorageSupported())
            {
                HiddenValues.Put("GX_CLI_NAV", "true");
                GXNavigationHelper nav = ctx.GetNavigationHelper();
                if (nav != null && nav.Count() > 0)
                {
                    string sUrl = ctx.GetRequestNavUrl().Trim();
                    int popupLevel = nav.GetUrlPopupLevel(sUrl);
                    HiddenValues.Put("GX_NAV", nav.ToJSonString(popupLevel));
                    nav.DeleteStack(popupLevel);
                }
            }
        }

		public string getJSONResponse()
		{
			return getJSONResponse(string.Empty);
		}
        public string getJSONContainerResponse(IGxJSONAble Container)
        {
            GXJObject jsonCmdWrapper = new GXJObject(context.IsMultipartRequest);
            try
            {
                jsonCmdWrapper.Put("gxHiddens", HiddenValues);
                jsonCmdWrapper.Put("gxContainer", Container.GetJSONObject());

            }
			catch (Exception ex)
			{
				GXLogging.Error(log, "getJSONContainerResponse error", ex);
			}
			return jsonCmdWrapper.ToString();
        }

        internal string getJSONResponse(string cmpContext)
		{
            GXJObject jsonCmdWrapper = new GXJObject(context.IsMultipartRequest);
			try 
			{
				if (commands.AllowUIRefresh)
				{
					if (string.IsNullOrEmpty(cmpContext))
					{
						cmpContext = "MAIN";
					}
					context.SaveComponentMsgList(cmpContext);
					Grids.Reverse();

					jsonCmdWrapper.Put("gxProps", PropValues);
					jsonCmdWrapper.Put("gxHiddens", HiddenValues);
					jsonCmdWrapper.Put("gxValues", AttValues);
					jsonCmdWrapper.Put("gxMessages", Messages);
					jsonCmdWrapper.Put("gxComponents", WebComponents);
					jsonCmdWrapper.Put("gxGrids", Grids);
				}
				foreach( DictionaryEntry LoadCommand in LoadCommands)
					appendAjaxCommand("load", (JObject)LoadCommand.Value);
				if (commands.Count > 0)
				{
					jsonCmdWrapper.Put("gxCommands", commands.JSONArray);
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "getJSONResponse error", ex);
			}
			return jsonCmdWrapper.ToString();
		}

		public static JArray GetParmsJArray(Object[] parms)
		{
			JArray inputs = new JArray();
			for (int i = 0; i < parms.Length; i++)
			{
				Object parm = parms[i];
				IGxJSONAble jparm = parm as IGxJSONAble;
				if (jparm != null)
				{
					inputs.Add(jparm.GetJSONObject());
				}
				else
				{
					inputs.Add(parm);
				}
			}
			return inputs;
		}

		public string GetAjaxEncryptionKey()
		{
			if (context.ReadSessionKey<string>(CryptoImpl.AJAX_ENCRYPTION_KEY) == null)
			{
                if(!RecoverEncryptionKey())
				    context.WriteSessionKey(CryptoImpl.AJAX_ENCRYPTION_KEY,CryptoImpl.GetRijndaelKey());
			}
			return context.ReadSessionKey<string>(CryptoImpl.AJAX_ENCRYPTION_KEY);
		}
		private bool RecoverEncryptionKey()
		{
			if ( (context.ReadSessionKey<string>(CryptoImpl.AJAX_ENCRYPTION_KEY) == null))
			{
				if (context.HttpContext != null)
				{
					String clientKey = context.HttpContext.Request.Headers[CryptoImpl.AJAX_SECURITY_TOKEN];
					if (!string.IsNullOrEmpty(clientKey))
					{
						bool correctKey = false;
						clientKey = CryptoImpl.DecryptRijndael(CryptoImpl.GX_AJAX_PRIVATE_IV + clientKey, CryptoImpl.GX_AJAX_PRIVATE_KEY, out correctKey);
						if (correctKey)
						{
							context.WriteSessionKey(CryptoImpl.AJAX_ENCRYPTION_KEY, clientKey);
							return true;
						}
					}
				}
			}
			return false;
		}
		public void LoadFormVars(HttpContext localHttpContext)
        {
            _formVars = new NameValueCollection();

            if (!_context.IsForward() && localHttpContext != null)
            {
#if NETCORE
				if (localHttpContext.Request.HasFormContentType)
				{
					foreach (string key in localHttpContext.Request.Form.Keys)
					{
						_formVars.Add(key, localHttpContext.Request.Form[key][0]);
					}
				}
#else
	            string key;
				for (int i = 0; i < localHttpContext.Request.Form.Count; i++)
                {
					key = localHttpContext.Request.Form.GetKey(i);
                    _formVars.Add(key, localHttpContext.Request.Form.GetValues(i)[0]);
				}
#endif
				string state = _formVars["GXState"];
				JObject tokenValues = GetGXStateTokens(state);
				ParseGXState(tokenValues);

            }
        }
		public void ParseGXState(JObject tokenValues)
		{
			if (tokenValues != null)
			{
				foreach (string name in tokenValues.Names)
				{
					object value = tokenValues[name];
					if (value is string)
						_formVars.Add(name, value.ToString());
					else if (value is double)
						_formVars.Add(name, ((double)value).ToString(CultureInfo.InvariantCulture.NumberFormat));
					else if (value is decimal)
						_formVars.Add(name, ((decimal)value).ToString(CultureInfo.InvariantCulture.NumberFormat));
					else if (value is float)
						_formVars.Add(name, ((float)value).ToString(CultureInfo.InvariantCulture.NumberFormat));
					else
						_formVars.Add(name, value.ToString());
				}
			}
		}

		private JObject GetGXStateTokens(string state)
        {
            try
            {
                if (!string.IsNullOrEmpty(state))
                {
                    if (Preferences.UseBase64ViewState())
                    {
                        state = Encoding.UTF8.GetString(Convert.FromBase64String(state));
                    }
					return JSONHelper.ReadJSON<JObject>(state); 
                }
            }
            catch (Exception e)
            {
                GXLogging.Debug(log, "Error parsing GXState");
                GXLogging.Debug(log, e.ToString());
                GXLogging.Debug(log, state);
            }
            return null;
        }

		public void disableJsOutput()
		{
			this.isJsOutputEnabled = false;
		}

		public void enableJsOutput()
		{
			this.isJsOutputEnabled = true;
		}
	}

    class GXCmpContent
    {
        string id;
        string content;

        public GXCmpContent(string id)
        {
            this.id = id;
            this.content = String.Empty;
        }

        public string Id
        {
            get { return id; }
        }

        public string Content
        {
            set { content = value; }
            get { return content; }
        }
    }

    public class GXJObject : JObject
    {
        private bool base64Encoded;

        public GXJObject(bool base64Encoded)
        {
            this.base64Encoded = base64Encoded;
        }
        public bool Base64Encoded
        {
            get { return base64Encoded; }
            set { base64Encoded = value; }
        }

        public override string ToString()
        {
            if (base64Encoded)
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(base.ToString()));
            else
                return base.ToString();
        }
    }

	public class GXAjaxCommand
	{
		private static string[] canManyCmds = new string[] { "print", "load", "popup", "refresh", "ucmethod", "cmp_refresh", "addlines", "set_focus", "calltarget", "exoprop", "exomethod", "refresh_form" };
		private string type;
		private object data;

		public GXAjaxCommand(string type)
		{
			this.type = type;
			this.data = "";
		}

		public GXAjaxCommand(string type, object data)
		{
			this.type = type;
			this.data = data;
		}

		public string Type
		{
			get
			{
				return type;
			}
		}

		public object Data
		{
			get
			{
				return data;
			}
			set
			{
				data = value;
			}
		}

		public JObject JSONObject
		{
			get
			{
				JObject jObj = new JObject();
				jObj.Put(type, data);
				return jObj;
			}
		}

		public bool CanHaveMany
		{
			get
			{
				for (int i = 0; i < canManyCmds.Length; i++)
				{
					if (string.Compare(type, canManyCmds[i]) == 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
            GXAjaxCommand ajaxCmd = obj as GXAjaxCommand;
			if (ajaxCmd!=null)
			{
				if (!CanHaveMany)
				{
					return (string.Compare(type, ajaxCmd.Type) == 0);
				}
			}
			return base.Equals(obj);
		}

		public override string ToString()
		{
			return "{ type:" + type + ", data:" + data + " }";
		}
	}

	public class GXUsercontrolMethod : IGxJSONAble
	{
		JObject wrapper;

		public GXUsercontrolMethod(String CmpContext, bool IsMasterPage, String containerName, String methodName, String output, Object[] parms)
		{
			wrapper = new JObject();
			AddObjectProperty("CmpContext", CmpContext);
			AddObjectProperty("IsMasterPage", IsMasterPage);
			AddObjectProperty("Control", containerName);
			AddObjectProperty("Method", methodName);
			AddObjectProperty("Output", output);
			AddObjectProperty("Parms", HttpAjaxContext.GetParmsJArray(parms));
		}

#region IGxJSONAble Members
		public void AddObjectProperty(string name, object prop)
		{
			wrapper.Put(name, prop);
		}

		public object GetJSONObject()
		{
			return wrapper;
		}
		public object GetJSONObject(bool includeState)
		{
			return GetJSONObject();
		}

		public void FromJSONObject(dynamic obj)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public string ToJavascriptSource()
		{
			throw new Exception("The method or operation is not implemented.");
		}
#endregion
	}

	public class GXAjaxCommandCollection
	{
		private List<GXAjaxCommand> commands;
		private bool allowUIRefresh;

		public GXAjaxCommandCollection()
		{
			commands = new List<GXAjaxCommand>();
			allowUIRefresh = true;
		}

		public int Count
		{
			get
			{
				return commands.Count;
			}
		}

		public bool AllowUIRefresh
		{
			get
			{
				return allowUIRefresh;
			}
		}

		public void AppendCommand(GXAjaxCommand cmd)
		{
			GXAjaxCommand cmd1 = GetCommand(cmd);
			if (cmd1 == null)
			{
				if (allowUIRefresh)
				{
					allowUIRefresh = cmd.CanHaveMany;
				}
				commands.Add(cmd);
			}
			else 
			{
				cmd1.Data = cmd.Data;
			}
		}

		private GXAjaxCommand GetCommand(GXAjaxCommand cmd)
		{
			int cIdx = commands.IndexOf(cmd);
			if (cIdx > 0)
			{
				return commands[cIdx];
			}
			return null;
		}

		public JArray JSONArray
		{
			get
			{
				JArray jArr = new JArray();
				foreach (GXAjaxCommand cmd in commands)
				{
					jArr.Add(cmd.JSONObject);
				}
				return jArr;
			}
		}
	}
}
