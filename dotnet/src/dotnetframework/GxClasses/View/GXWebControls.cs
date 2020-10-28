using System;
using System.Web;
using System.Drawing;
#if !NETCORE
using System.Web.UI;
using System.Web.UI.WebControls;
#else
using GeneXus.Http;
using System.Reflection;
using GeneXus.Metadata;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using GeneXus.Utils;
using System.Text.RegularExpressions;
using Jayrock.Json;
using System.IO;
using GeneXus.Configuration;
using GeneXus.Application;
using log4net;

namespace GeneXus.WebControls
{
    public class GXWebStdMethods
    {
        private static Type webStdType;

        private static Type GetWebStdType(GeneXus.Application.IGxContext context)
        {
			if (webStdType == null)
			{
#if !NETCORE
				List<string> namespaces = GeneXus.HttpHandlerFactory.HandlerFactory.GetGxNamespaces(context.HttpContext, "");
				foreach (string gxNamespace in namespaces)
				{
					webStdType = GeneXus.HttpHandlerFactory.HandlerFactory.GetHandlerType(gxNamespace + ".Common", gxNamespace + ".GxWebStd");
					if (webStdType != null)
					{
						break;
					}
				}
#else
				var cname = "GxWebStd";
				string mainNamespace, className;
				if (Config.GetValueOf("AppMainNamespace", out mainNamespace))
					className = mainNamespace + "." + cname;
				else
					className = "GeneXus.Programs." + cname;

				webStdType = ClassLoader.FindType(mainNamespace + ".Common", className, Assembly.GetEntryAssembly());
#endif
			}
            return webStdType;
        }

        public static void CallMethod(GeneXus.Application.IGxContext context, string controlType, object[] parms, string GridName)
        {
            if (TagHasStdMethod(controlType))
            {
                try
                {
                    Type webStdFuncs = GetWebStdType(context);
                    if (webStdFuncs != null)
                    {
                        bool addCallerPgm = true;
                        System.Reflection.MethodInfo method = null;
                        if (string.Compare(controlType, "edit", true) == 0)
                        {
                            method = webStdFuncs.GetMethod("gx_single_line_edit");
                        }
                        else if (string.Compare(controlType, "html_textarea", true) == 0)
                        {
                            method = webStdFuncs.GetMethod("gx_html_textarea");
                        }
                        else if (string.Compare(controlType, "button", true) == 0)
                        {
                            string sCtrlName = (string)parms[1];//sCtrlName
                            if (sCtrlName.LastIndexOf('_') > 0)
                            {
                                string row = sCtrlName.Substring(sCtrlName.LastIndexOf('_') + 1);
                                if (((short)parms[10]) == 7) //execCliEvt
                                {
                                    parms[12] += ",'" + GridName + "','" + row + '\''; //sEventName
                                }
                                else if (((short)parms[10]) == 5) //serverEvt
                                {
                                    parms[12] += row; //sEventName
                                }
                            }
                            method = webStdFuncs.GetMethod("gx_button_ctrl");
                        }
                        else if (string.Compare(controlType, "blob", true) == 0)
                        {
                            method = webStdFuncs.GetMethod("gx_blob_field");
                        }
                        else if (string.Compare(controlType, "label", true) == 0)
                        {
                            method = webStdFuncs.GetMethod("gx_label_ctrl");
                        }
                        else if (string.Compare(controlType, "radio", true) == 0)
                        {
                            method = webStdFuncs.GetMethod("gx_radio_ctrl");
                        }
                        else if (string.Compare(controlType, "combobox", true) == 0)
                        {
                            method = webStdFuncs.GetMethod("gx_combobox_ctrl1");
                        }
                        else if (string.Compare(controlType, "listbox", true) == 0)
                        {
                            method = webStdFuncs.GetMethod("gx_listbox_ctrl1");
                        }
                        else if (string.Compare(controlType, "checkbox", true) == 0)
                        {
                            addCallerPgm = false;
                            method = webStdFuncs.GetMethod("gx_checkbox_ctrl");
                        }
                        else if (string.Compare(controlType, "bitmap", true) == 0)
                        {
                            method = webStdFuncs.GetMethod("gx_bitmap");
                        }
                        else if (string.Compare(controlType, "table", true) == 0)
                        {
                            try
                            {
                                method = webStdFuncs.GetMethod("gx_table_start");
                                string sStyleString = "";
                                if (int.Parse(parms[1].ToString()) == 0 )
                                {
                                    sStyleString = "display:none;";
                                }
                                int nBorder = string.IsNullOrEmpty(parms[8].ToString()) ? 0 : int.Parse(parms[8].ToString());
                                method.Invoke(null, new Object[] { context,
																		parms[0],		//sCtrlName
																		parms[0],		//sHTMLid
																		"",				//sHTMLTags
																		parms[2],		//sClassString
																		nBorder,		//nBorder
																		parms[6],		//sAlign
																		parms[7],		//sTooltiptext
																		parms[9],		//nCellpadding
																		parms[10],		//nCellspacing
																		sStyleString,	//sStyleString
																		parms[13],		//sRules
																		0,				//nParentIsFreeStyle
																		});
                            }
                            catch
                            {
                                context.WriteHtmlTextNl("<table>");
                            }
                            method = null;
                        }
                        if (method != null)
                        {
                            int parmsLen = parms.Length + 1;
                            if (addCallerPgm)
                            {
                                parmsLen++;
                            }
                            Object[] allParms = new Object[parmsLen];
                            allParms[0] = context;
                            parms.CopyTo(allParms, 1);
                            if (addCallerPgm)
                            {
                                allParms[parms.Length + 1] = ""; //sCallerPgm
                            }
                            if (method.GetParameters().Length == allParms.Length)
                            {
                                method.Invoke(null, allParms);
                            }
                        }
                    }
                }
                catch { }
            }
            else
            {
                OpenTag(context, controlType, parms);
            }
        }

        private static bool TagHasStdMethod(string tag)
        {
            if (string.Compare(tag, "row", true) == 0)
                return false;
            if (string.Compare(tag, "cell", true) == 0)
                return false;
            if (string.Compare(tag, "usercontrol", true) == 0)
                return false;
            return true;
        }

        public static void OpenTag(GeneXus.Application.IGxContext context, string tag, object[] parms)
        {
            if (string.Compare(tag, "row", true) == 0)
            {
                context.WriteHtmlTextNl("<tr>");
            }
            else if (string.Compare(tag, "cell", true) == 0)
            {
                context.WriteHtmlText("<td ");
                string parm = parms[0].ToString();
                if (!string.IsNullOrEmpty(parm))
                {
                    context.WriteHtmlText(" background=\"" + parm + "\" ");
                }
                parm = parms[1].ToString();
                if (!string.IsNullOrEmpty(parm))
                {
                    context.WriteHtmlText(" " + parm + " ");
                }
                context.WriteHtmlTextNl(">");
            }
            else if (string.Compare(tag, "usercontrol", true) == 0)
            {
                context.WriteHtmlTextNl("<div class=\"gx_usercontrol\" id=\"" + parms[0].ToString() + "\"></div>");
            }
        }

        public static void CloseTag(GeneXus.Application.IGxContext context, string tag)
        {
            if (string.Compare(tag, "table", true) == 0)
            {
                context.WriteHtmlTextNl("</table>");
            }
            else if (string.Compare(tag, "row", true) == 0)
            {
                context.WriteHtmlTextNl("</tr>");
            }
            else if (string.Compare(tag, "cell", true) == 0)
            {
                context.WriteHtmlTextNl("</td>");
            }
        }
    }

	public class GXWebGrid : IGxJSONAble
    {
        GeneXus.Application.IGxContext context;
        bool wrapped;
        bool isFreestyle;
        JArray _ColsProps;
        List<JArray> _ColsPropsCommon;
        JObject _Rows;
        int _Count;
        List<GXWebRow> _rowObjs;
        int _PageSize;
        bool writingTableContent;

        public GXWebGrid()
        {
            _Rows = new JObject();
            _ColsProps = new JArray();
            _rowObjs = new List<GXWebRow>();
            _ColsPropsCommon = new List<JArray>();
        }

        public GXWebGrid(GeneXus.Application.IGxContext context)
            : this()
        {
            this.context = context;
        }
        public string GridName
        {
            get { if (_Rows != null) return (string)_Rows["GridName"]; else return ""; }
        }
        public bool WritingTableContent
        {
            get { return writingTableContent; }
            set { writingTableContent = value; }
        }

        public int PageSize
        {
            get { return _PageSize; }
            set { _PageSize = value; }
        }
        public void Clear()
        {
            _ColsProps.Clear();
            _Rows.Clear();
            _rowObjs.Clear();
            _ColsPropsCommon.Clear();
            _Count = 0;
            writingTableContent = false;
        }

        public void ClearRows()
        {
            _Rows.Clear();
            _rowObjs.Clear();
            _Count = 0;
        }

        public void SetIsFreestyle(bool isFreestyle)
        {
            this.isFreestyle = isFreestyle;
        }

        public bool IsFreestyle()
        {
            return this.isFreestyle;
        }

        public void SetWrapped(int wrapped)
        {
            this.wrapped = (wrapped == 1);
            if (!this.wrapped)
            {
                this.wrapped = this.context.DrawGridsAtServer;
            }
            if (!this.wrapped)
            {
                this.wrapped = this.context.GetWrapped();
            }
            this.context.SetWrapped(this.wrapped);
        }

        public int GetWrapped()
        {
            if (this.wrapped || this.context.isCrawlerRequest())
            {
                return 1;
            }
            return 0;
        }

        public List<JArray> GetColsPropsCommon()
        {
            return this._ColsPropsCommon;
        }

        public void CloseTag(string tag)
        {
            if (this.IsFreestyle() && tag == "row" && !this.WritingTableContent)
            {
                return;
            }
            GXWebStdMethods.CloseTag(this.context, tag);
            if (this.IsFreestyle() && tag == "table")
            {
                this.WritingTableContent = false;
            }
        }

        private void ResetRows()
        {
            for (int i = 0; i < _Count; i++)
            {
                _Rows.Remove(_Count.ToString());
            }
            _rowObjs.Clear();
            _Count = 0;
        }

        public void AddRow(GXWebRow row)
        {
            if (!this.isFreestyle && this.wrapped && this.context != null)
            {
                GXWebStdMethods.CloseTag(this.context, "row");
            }
            else
            {
                if (_PageSize > 0 && _Count + 1 > _PageSize)
                    ResetRows();
                _rowObjs.Add(row);
                AddObjectProperty(_Count.ToString(), row.GetJSONObject());
                _Count++;
            }
        }

        public string ToJavascriptSource()
        {
            return GetJSONObject().ToString();
        }

        public string GridValuesHidden()
        {
            string values = GetValues().ToString();
            if (!this.context.isAjaxRequest() && !this.context.isSpaRequest())
            {
                values = GXUtil.HtmlEncodeInputValue(values);
            }
            return values;
        }

        internal JArray GetValues()
        {
            JArray values = new JArray();
            if (!this.wrapped)
            {
                foreach (GXWebRow row in _rowObjs)
                {
                    values.Add(row.GetValues());
                }
            }
            return values;
        }

        public void ToJSON()
        {
            AddObjectProperty("Wrapped", this.wrapped);
            if (!this.wrapped)
            {
                AddObjectProperty("Columns", _ColsProps);
                AddObjectProperty("Count", _Count);
            }
        }

        public void AddObjectProperty(string name, object prop)
        {
            IGxJSONAble jprop = prop as IGxJSONAble;
            if (jprop != null)
                prop = jprop.GetJSONObject();
            _Rows.Put(name, prop);
        }

        public void AddColumnProperties(object colProps)
        {
            _ColsProps.Add(((IGxJSONAble)colProps).GetJSONObject());
        }

        public Object GetJSONObject()
		{
			ToJSON();
			return _Rows;
		}
        public Object GetJSONObject(bool includeState)
        {
			return GetJSONObject();
        }

        public void FromJSONObject(dynamic obj)
        {
        }
    }

	public class GXWebRow : IGxJSONAble
    {
        GeneXus.Application.IGxContext context;
		GXWebGrid _parentGrid; 
        public GXWebGrid parentGrid 
		{
			get { return this._parentGrid; }
		}
        JObject _ThisRow;
        JArray _Columns;
        JArray _RenderProps;
        JObject _Hiddens;
        JObject _Grids;
        int _Count;
        JArray _Values;
        bool firstRowAdded;

        public GXWebRow()
        {
            _ThisRow = new JObject();
            _Columns = new JArray();
            _RenderProps = new JArray();
            _Hiddens = new JObject();
            _Grids = new JObject();
            _Values = new JArray();
        }

        public GXWebRow(GeneXus.Application.IGxContext context, GXWebGrid parentGrid)
            : this()
        {
            this.context = context;
            this._parentGrid = parentGrid;
        }

        public void Clear()
        {
            _ThisRow.Clear();
            _Columns.Clear();
            _RenderProps.Clear();
            _Hiddens.Clear();
            _Grids.Clear();
            _Values.Clear();
            _Count = 0;
            firstRowAdded = false;
        }

        public void AddColumnProperties(string controlType, int valueIndex, bool valueWithProps, object[] props)
        {
            if (this._parentGrid != null && this._parentGrid.GetWrapped() == 1 && this.context != null)
            {
                if (this._parentGrid.IsFreestyle() && controlType == "table")
                {
                    this._parentGrid.WritingTableContent = true;
                }
                if (this._parentGrid.IsFreestyle() && controlType == "row" && !firstRowAdded)
                {
                    this.firstRowAdded = true;
                    return;
                }
                this.context.DrawingGrid = true;
                GXWebStdMethods.CallMethod(this.context, controlType, props, parentGrid.GridName);
                this.context.DrawingGrid = false;
                if (!this._parentGrid.IsFreestyle())
                {
                    GXWebStdMethods.CloseTag(this.context, "cell");
                }
            }
            else
            {
                AddColumnProperties(valueIndex, valueWithProps, props);
            }
        }
        
        public IEnumerator initializePptyIterator()
        {
            IEnumerator it = null;
            if (this._parentGrid != null && this._parentGrid.GetColsPropsCommon().Count > this._Count)
            {
                it = (this._parentGrid.GetColsPropsCommon()[_Count]).GetEnumerator();
            }
            return it;
        }
        public void AddColumnProperties(int valueIndex, bool valueWithProps, object[] props)
        {
            IEnumerator it = this.initializePptyIterator();
            JArray colPropsRev = new JArray(); //ColProps Reversed
            Object value = "";
            JArray colProps = new JArray();
            bool equal = it != null && !this.context.isAjaxCallMode();
            Object current = null;

            for (int i = props.Length - 1; i >= 0; i--)
            {
                object prop = props[i];
                IGxJSONAble jprop = prop as IGxJSONAble;
                if (jprop != null)
                    prop = jprop.GetJSONObject();

                if (i != valueIndex)
                {
                    equal = equal && it.MoveNext();
                    if (equal)
                        current = it.Current;
                    if (!(equal && (current.Equals(prop))))
                    {
                        equal = false;
                        colProps.Add(0, prop);
                        if (it == null)
                            colPropsRev.Put(prop);
                    }
                }
                else if (valueWithProps)
                    value = prop;
                else
                {
                    prop = prop.ToString().Replace("'", "\'");
                    _Values.Add(prop);
                }
            }
            if (this._parentGrid != null && it == null) // If is the first Row. 
            {
                this._parentGrid.GetColsPropsCommon().Add(colPropsRev);
            }

            if (valueWithProps)
                colProps.Put(value);
            else if (valueIndex < 0)
                _Values.Put("");

            _Columns.Put(colProps);
            _Count++;

        }

        public void AddRenderProperties(GXWebColumn column)
        {
            _RenderProps.Add(column.GetJSONObject());
        }

        public static GXWebRow GetNew()
        {
            return GetNew(null);
        }

        public static GXWebRow GetNew(GeneXus.Application.IGxContext context)
        {
            return GetNew(null, null);
        }

        public static GXWebRow GetNew(GeneXus.Application.IGxContext context, GXWebGrid parentGrid)
        {
            return new GXWebRow(context, parentGrid);
        }

        public void AddGrid(string gridName, GXWebGrid grid)
        {
            _Grids.Put(gridName, grid.GetJSONObject());
        }

		public void AddHidden(string name, bool value)
		{
			AddHidden(name, (object)value);
		}

		public void AddHidden(string name, string value)
		{
			AddHidden(name, (object)value);
		}

		internal void AddHidden(string name, object value)
		{
			_Hiddens.Put(name, value);
		}

		public string ToJavascriptSource()
        {
            return GetJSONObject().ToString();
        }

        internal JArray GetValues()
        {
            return _Values;
        }

        public void ToJSON()
        {
            AddObjectProperty("Props", _Columns);
            if (_RenderProps.Count > 0)
                AddObjectProperty("RenderProps", _RenderProps);
            if (_Hiddens.Count > 0)
                AddObjectProperty("Hiddens", _Hiddens);
            AddObjectProperty("Grids", _Grids);
            AddObjectProperty("Count", _Count);
        }

        public void AddObjectProperty(string name, object prop)
        {
            _ThisRow.Put(name, prop);
        }

        public Object GetJSONObject()
        {
            ToJSON();
            return _ThisRow;
        }

        public Object GetJSONObject(bool inlcudeState)
        {
			return GetJSONObject();
		}

        public void FromJSONObject(dynamic obj)
        {
        }
    }

    public class GXWebColumn : IGxJSONAble
    {
        JObject _Properties;

        public String ToJavascriptSource()
        {
            return GetJSONObject().ToString();
        }

        public GXWebColumn()
        {
            _Properties = new JObject();
        }

        public void Clear()
        {
            _Properties.Clear();
        }

        public static GXWebColumn GetNew()
        {
            return GetNew(false);
        }

        public static GXWebColumn GetNew(bool includeValue)
        {
            GXWebColumn col = new GXWebColumn();
            return col;
        }

        public void ToJSON()
        {
        }

        public void AddObjectProperty(string name, object prop)
        {
            IGxJSONAble jprop = prop as IGxJSONAble;
            if (jprop != null)
                prop = jprop.GetJSONObject();
            _Properties.Put(name, prop);
        }

        public Object GetJSONObject()
        {
            ToJSON();
            return _Properties;
		}
        public Object GetJSONObject(bool includeState)
        {
			return GetJSONObject();
        }

        public void FromJSONObject(dynamic obj)
        {
        }
    }

    public abstract class GXWebControl : IGxJSONAble, IGxJSONSerializable
    {
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GXWebControl));
		bool ForeColorFlag;
        bool BackColorFlag;
        Color _ForeColor;
        Color _BackColor;
		FontInfo _Font;
        bool _Enabled;
        bool _Visible;
        string _ID;
        int _BackStyle;
        bool _IsPassword;
        string _Link = "";
        string _LinkTarget = "";
        string _EventJsCode = "";
        string _OnClickCode = "";
        string _CssClass;
        string _Style;
        string _StyleString;
        GxWebControlTitle _title;
        string _TooltipText;
        Unit _BorderWidth = Unit.Empty;
        Unit _Width = Unit.Empty;
        Unit _Height = Unit.Empty;
        ListDictionary _attributes = new ListDictionary();
        ListDictionary _styleAttributes = new ListDictionary();
        protected JObject jsonObj = new JObject();
        string _WebTags;
        static Regex rAttributes = new Regex("\\s*(?<att>\\S*)\\s*=\\s*\"(?<value>[^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public GXWebControl()
        {
            _ForeColor = new Color();
            _BackColor = new Color();
            _BackStyle = 1;
#if !NETCORE
			_Font = (new Style()).Font;
#else
			_Font = new FontInfo();
#endif
			_Enabled = true;
            _Visible = true;
            _ID = "";
            _title = new GxWebControlTitle();
        }
        public string CssClass
        {
            get
            {
                return (_CssClass != null) ? _CssClass : "";
            }
            set
            {
                _CssClass = value;
            }
        }
        public string Style
        {
            get
            {
                return (_Style != null) ? _Style : "";
            }
            set
            {
                _Style = value;
            }
        }
        public string StyleString
        {
            get
            {
                return (_StyleString != null) ? _StyleString : "";
            }
            set
            {
                ParseAndSetStyleProperties(value);
                _StyleString = value;
            }
        }
        public int ForeColor
        {
            get
            {
                return _ForeColor.ToArgb();
            }
            set
            {
                _ForeColor = Color.FromArgb(value);
                ForeColorFlag = true;
            }
        }
        public int BackColor
        {
            get
            {
                return _BackColor.ToArgb();
            }
            set
            {
                _BackColor = Color.FromArgb(value);
                BackColorFlag = true;
            }
        }
        public string FontName
        {
            get
            {
                return _Font.Name;
            }
            set
            {
                _Font.Name = value;
            }
        }
		public int FontSize
        {
            get
            {
                return (int)_Font.Size.Unit.Value;
            }
            set
            {
                _Font.Size = FontUnit.Point(value);
            }
        }
        public int FontBold
        {
            get
            {
                return _Font.Bold ? 1 : 0;
            }
            set
            {
                _Font.Bold = (value == 1 ? true : false);
            }
        }
        public int FontItalic
        {
            get
            {
                return _Font.Italic ? 1 : 0;
            }
            set
            {
                _Font.Italic = (value == 1 ? true : false);
            }
        }
        public int FontStrikethru
        {
            get
            {
                return _Font.Strikeout ? 1 : 0;
            }
            set
            {
                _Font.Strikeout = (value == 1 ? true : false);
            }
        }
        public int FontUnderline
        {
            get
            {
                return _Font.Underline ? 1 : 0;
            }
            set
            {
                _Font.Underline = (value == 1 ? true : false);
            }
        }

		public int Enabled
        {
            get
            {
                return _Enabled ? 1 : 0;
            }
            set
            {
                _Enabled = (value == 1 ? true : false);
            }
        }
        public int Visible
        {
            get
            {
                return _Visible ? 1 : 0;
            }
            set
            {
                
                _Visible = (value == 0 ? false : true);
            }
        }
        public string Name
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }
        public int BackStyle
        {
            get
            {
                return _BackStyle;
            }
            set
            {
                _BackStyle = value;
            }
        }

		public int Height
        {
            get
            {
                return (int)_Height.Value;
            }
            set
            {
                _Height = Unit.Point(value);
            }
        }
        public int Width
        {
            get
            {
                return (int)_Width.Value;
            }
            set
            {
                _Width = Unit.Point(value);
            }
        }
        public int BorderWidth
        {
            get
            {
                return (int)_BorderWidth.Value;
            }
            set
            {
                _BorderWidth = Unit.Point(value);
            }
        }

        public int IsPassword
        {
            get
            {
                return _IsPassword ? 1 : 0;
            }
            set
            {
                _IsPassword = (value == 1 ? true : false);
            }
        }
        public string Link
        {
            get
            {
                return _Link;
            }
            set
            {
                _Link = value;
            }
        }
        public string LinkTarget
        {
            get
            {
                return _LinkTarget;
            }
            set
            {
                _LinkTarget = value;
            }
        }
        public string EventJsCode
        {
            get
            {
                return _EventJsCode;
            }
            set
            {
                _EventJsCode = value;
            }
        }
        public string OnClickCode
        {
            get
            {
                return _OnClickCode;
            }
            set
            {
                _OnClickCode = value;
            }
        }
        public GxWebControlTitle Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }
        public string TooltipText
        {
            get
            {
                return _TooltipText;
            }
            set
            {
                _TooltipText = value;
            }
        }
        public string WebTags
        {
            set
            {
                _WebTags = value;
                ParseAndAddAttributes(value);
            }
            get
            {
                return _WebTags;
            }
        }

		public void AddAttribute(string Attribute, string Value)
        {
            if (_attributes.Contains(Attribute))
                _attributes[Attribute] = _attributes[Attribute] + ";" + Value;
            else
                _attributes.Add(Attribute, Value);
        }
        public void ParseAndAddAttributes(string sTag)
        {
            _attributes.Clear();
            if (sTag.Length > 0)
                for (Match m = rAttributes.Match(sTag); m.Success; m = m.NextMatch())
                {
                    AddAttribute(m.Groups["att"].Value, m.Groups["value"].Value);
                }
        }
        public void AddStyle(string Attribute, string Value)
        {
            if (_styleAttributes.Contains(Attribute))
                _styleAttributes[Attribute] = Value;
            else
                _styleAttributes.Add(Attribute, Value);
        }
        
        bool ParseAndSetStyleProperties(string sStyleString)
        {
            Regex r1;
            Match m1;
            _Style = sStyleString;

            r1 = new Regex(@"\s*(?<stylekey>[^;]*)\s*:\s*(?<stylevalue>[^;]*);*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            m1 = r1.Match(_Style);
            while (m1.Success)
            {
                this.AddStyle(m1.Groups["stylekey"].Value, m1.Groups["stylevalue"].Value);
                m1 = m1.NextMatch();
            }
            return true;
        }
        protected virtual void ApplyProperties(WebControl control)
        {
            control.ID = _ID;
            if (ForeColorFlag) control.ForeColor = _ForeColor;
            if (BackColorFlag) control.BackColor = _BackColor;
			if (_BackStyle == 0) control.BackColor = Color.Transparent;
			if (!_BorderWidth.IsEmpty) control.BorderWidth = _BorderWidth;
            if (!_Width.IsEmpty) control.Width = _Width;
            if (!_Height.IsEmpty) control.Height = _Height;
			control.Font.CopyFrom(_Font);
			control.Enabled = _Enabled;
            control.Visible = _Visible;
            if (TooltipText != null) control.ToolTip = TooltipText;
            if (_CssClass != null) control.CssClass = _CssClass;

			IDictionaryEnumerator enumerator = _attributes.GetEnumerator();
			while (enumerator.MoveNext())
			{
				control.Attributes.Add((string)enumerator.Key, (string)enumerator.Value);
			}

			enumerator = _styleAttributes.GetEnumerator();
			while (enumerator.MoveNext())
			{
				control.Style.Add((string)enumerator.Key, (string)enumerator.Value);
			}
		}
		protected void SendHidden(string sCtrlName, string sValue, HtmlTextWriter ControlOutputWriter)
        {
            ControlOutputWriter.WriteLine("");
            ControlOutputWriter.Write("<input type=hidden ");
            ControlOutputWriter.Write("name=\"" + sCtrlName + "\" ");
            ControlOutputWriter.Write(" value=\"");
            ControlOutputWriter.Write(GXUtil.ValueEncode(sValue));
            ControlOutputWriter.Write("\"" + GXUtil.HtmlEndTag(HTMLElement.INPUT));
        }
        protected void SendStartSpan(string sCtrlName, string cssClass, string style, HtmlTextWriter ControlOutputWriter)
        {
            ControlOutputWriter.WriteLine("");
            string classString = "";
            string styleString = "";
            if (cssClass.Trim().Length > 0)
                classString = " class=" + cssClass;
            if (style.Trim().Length > 0)
                styleString = " style=" + style;
            ControlOutputWriter.Write("<span id=" + sCtrlName + classString + styleString + ">");
        }
        protected void SendEndSpan(HtmlTextWriter ControlOutputWriter)
        {
            ControlOutputWriter.Write("</span>");
        }
        protected void AddEvent(string EventName, WebControl obj)
        {
            if (OnClickCode.Trim().Length > 0 || EventJsCode.Trim().Length > 0)
            {
                if (!String.IsNullOrEmpty(OnClickCode))
                    if (!String.IsNullOrEmpty(EventJsCode))
                        obj.Attributes[EventName] = "javascript:if(" + OnClickCode + ") {" + EventJsCode + "} else return false;";
                    else
                        obj.Attributes[EventName] = "javascript:if(!(" + OnClickCode + ")) return false;";
                else
                    obj.Attributes[EventName] = "javascript:" + EventJsCode;
            }
        }


        public Object GetJSONObject()
        {
            ToJSON();
            return jsonObj;
		}
		public Object GetJSONObject(bool includeState)
        {
			return GetJSONObject();
        }
        public void AddObjectProperty(String Desc, object Value)
        {
            try
            {
                jsonObj.Put(Desc, Value);
            }
			catch (Exception ex)
			{
				GXLogging.Error(log, "AddObjectProperty error", ex);
			}
		}

        public virtual void FromJSONObject(dynamic Obj)
        {

        }
		public bool FromJSonString(string s)
		{
			return FromJSonString(s, null);
		}
		public bool FromJSonString(string s, GXBaseCollection<SdtMessages_Message> Messages=null)
		{
			jsonObj = JSONHelper.ReadJSON<JObject>(s, Messages);
			bool result = jsonObj != null;
			this.FromJSONObject(jsonObj);
			return result;
		}

        public String ToJavascriptSource()
        {
            return GetJSONObject().ToString();
        }

        public string ToJSonString()
        {
            return this.ToJavascriptSource();
        }

        public abstract void ToJSON();

		public bool FromJSonFile(GxFile file)
		{
			return FromJSonFile(file);
		}
		public bool FromJSonFile(GxFile file, GXBaseCollection<SdtMessages_Message> Messages)
		{
			if (GXUtil.CheckFile(file, Messages))
				return FromJSonString(file.ReadAllText(string.Empty), Messages);
			else
				return false;
		}
	}
	
	public class GXWebEditBox : GXWebControl
    {

        private int _MaxLength;
        private int _Columns;
        private int _Rows = 1;
        private string _Value = "";
        private string _FormatedValue = "";
        private short _HtmlFormat;

        public int MaxLength
        {
            get
            {
                return _MaxLength;
            }
            set
            {
                _MaxLength = value;
            }
        }
        public int Columns
        {
            get
            {
                return _Columns;
            }
            set
            {
                _Columns = value;
            }
        }
        public int Rows
        {
            get
            {
                return _Rows;
            }
            set
            {
                _Rows = value;
            }
        }
        public string Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
            }
        }
        public string FormatedValue
        {
            get
            {
                return _FormatedValue;
            }
            set
            {
                _FormatedValue = value;
            }
        }
        public short Format
        {
            get
            {
                return _HtmlFormat;
            }
            set
            {
                _HtmlFormat = value;
            }
        }
        public void Render(HtmlTextWriter ControlOutputWriter)
        {

			if (Visible == 0)
            {
                SendHidden(Name, Value, ControlOutputWriter);
                return;
            }
            if (Enabled != 0 || Rows > 1)
            {
                TextBox tbox = new TextBox();
                ApplyProperties(tbox);
                tbox.MaxLength = MaxLength;
                tbox.Enabled = Enabled != 0;
                
                tbox.Columns = Columns;
                if (IsPassword != 0) tbox.TextMode = TextBoxMode.Password;
                if (Rows > 1)
                {
                    tbox.TextMode = TextBoxMode.MultiLine;
                    tbox.Rows = Rows;
                }
                
                tbox.Attributes["value"] = Value;
                tbox.Visible = Visible != 0;
                tbox.RenderControl(ControlOutputWriter);
            }
            else
            {
                SendHidden(Name, Value, ControlOutputWriter);
                if (OnClickCode.Trim().Length > 0 || EventJsCode.Trim().Length > 0)
                {
                    SendHidden(Name + "_gxc", "", ControlOutputWriter);
                    SendStartSpan("span_" + Name, CssClass, StyleString, ControlOutputWriter);
                    HyperLink hl = new HyperLink();
                    
                    if (!String.IsNullOrEmpty(OnClickCode))
                        if (!String.IsNullOrEmpty(EventJsCode))
                            hl.NavigateUrl = "javascript:if(" + OnClickCode + ") {" + EventJsCode + "}";
                        else
                            hl.NavigateUrl = "javascript:if(!(" + OnClickCode + ")) return false;";
                    else
                    {
                        if (!String.IsNullOrEmpty(EventJsCode))
                            hl.NavigateUrl = "javascript:" + EventJsCode;
                        else
                        {
                            hl.NavigateUrl = Link;
                            hl.Target = LinkTarget;
                        }
                    }
                    if ((IsPassword == 0))
                        hl.Text = FormatedValue;
                    else
                        hl.Text = StringUtil.PadR("", (short)(Columns), "*");
                    hl.Visible = Visible != 0;
                    hl.Enabled = true;
                    hl.Attributes.Remove("onFocus");
                    hl.RenderControl(ControlOutputWriter);
                    SendEndSpan(ControlOutputWriter);
                }
                else
                {
                    if (Link.Trim().Length > 0)
                    {
                        if (_HtmlFormat != 2)
                            SendStartSpan("span_" + Name, CssClass, StyleString, ControlOutputWriter);
                        HyperLink hl = new HyperLink();
                        
                        hl.NavigateUrl = Link;
                        hl.Target = LinkTarget;
                        if ((IsPassword == 0))
                            hl.Text = FormatedValue;
                        else
                            hl.Text = StringUtil.PadR("", (short)(Columns), "*");
                        hl.Visible = Visible != 0;
                        hl.Enabled = true;
                        hl.Attributes.Remove("onFocus");
                        hl.RenderControl(ControlOutputWriter);
                        if (_HtmlFormat != 2)
                            SendEndSpan(ControlOutputWriter);
                    }
                    else
                    {
                        if (_HtmlFormat == 0 || (CssClass.Length > 0 && _HtmlFormat != 2))
                        {
                            Label lab = new Label();
                            ApplyProperties(lab);
                            lab.ID = "span_" + Name;
                            if (IsPassword == 0)
                            {
                                if (_HtmlFormat == 0)
                                    lab.Text = GXUtil.ValueEncode(FormatedValue);
                                else
                                    lab.Text = FormatedValue;
                            }
                            else
                                lab.Text = StringUtil.PadR("", (short)(Columns), "*");
                            lab.Visible = Visible != 0;
                            lab.Enabled = true;
                            lab.Attributes.Remove("onFocus");
                            lab.RenderControl(ControlOutputWriter);
                        }
                        else
                        {
                            LiteralControl lit = new LiteralControl();
                            lit.ID = "lit_" + Name;
                            if (IsPassword == 0)
                                if (_HtmlFormat == 0)
                                    lit.Text = GXUtil.ValueEncode(FormatedValue);
                                else
                                    lit.Text = FormatedValue;
                            else
                                lit.Text = StringUtil.PadR("", (short)(Columns), "*");
                            lit.Visible = Visible != 0;
                            lit.RenderControl(ControlOutputWriter);
                        }
                    }
                }
            }
        }

        public override void ToJSON()
        {
            jsonObj.Put(Value, Value);
        }

    }

    public class GXWebListControl : GXWebControl
    {
        ListItemCollection _Items;
        int _SelectedIndex = -1;
        string _LastSetValue = "";
        private short _HtmlFormat;
        private bool _IsSet;
        public string Caption
        {
            get;
            set;
        }
        public short Format
        {
            get
            {
                return _HtmlFormat;
            }
            set
            {
                _HtmlFormat = value;
            }
        }
        public GXWebListControl()
            : base()
        {
            _Items = new ListItemCollection();
        }
        public ListItemCollection Items
        {
            get
            {
                return _Items;
            }
            set
            {
                _Items = value;
            }
        }
        public int SelectedIndex
        {
            get
            {
                return _SelectedIndex;
            }
            set
            {
                _SelectedIndex = value;
            }
        }
        public string SelectedItemText
        {
            get
            {
                if (_SelectedIndex != -1)
                    return Items[_SelectedIndex].Text;
                else
                    return "";
            }
            set
            {
                if (_SelectedIndex != -1)
                    Items[_SelectedIndex].Text = value;
            }
        }
        public string SelectedItemValue
        {
            get
            {
                if (_SelectedIndex != -1)
                    return Items[_SelectedIndex].Value;
                else
                {
                    if (Items.Count == 0 || Visible == 0)
                        return _LastSetValue;
                    else
                        return "";
                }
            }
            set
            {
                if (_SelectedIndex != -1)
                    Items[_SelectedIndex].Value = value;
            }
        }
        
        public void addItem(string value, string text, int pos)
        {
            ListItem item = Items.FindByValue(value);
            if (item == null)
            {
                _IsSet = true;
                if (pos == 0)
                    Items.Add(new ListItem(text, value));
                else
                    Items.Insert(pos - 1, new ListItem(text, value));
			}
			else
			{
				item.Text = text;
			}
        }
        public void removeAllItems()
        {
            _IsSet = true;
            Items.Clear();
            jsonObj = new JObject();
            _SelectedIndex = -1;
        }
        public void removeItem(string itemValue)
        {
            _IsSet = true;
			foreach (ListItem item in Items)
			{
				if (item.Value.Trim() == itemValue.Trim())
				{
					Items.Remove(item);
					break;
				}
			}
			if (_SelectedIndex >= Items.Count)
			{
				_SelectedIndex = -1;
			}
        }
        public string getItemText(int pos)
        {
            return Items[pos - 1].Text;
        }
        public string getItemValue(int pos)
        {
            return Items[pos - 1].Value;
        }

        public string getValidValue(string value)
        {
            return getValidValueImp(value);
        }

        private string getValidValueImp(object valid)
        {
			if (valid != null)
			{
				foreach (ListItem item in Items)
				{
					if (item.Value.Trim().Equals(valid.ToString().Trim()))
						return valid.ToString();
				}
			}
            return getItemValue(1);
        }

        public int ItemCount
        {
            get
            {
                return Items.Count;
            }
        }
        public string Description
        {
            get
            {
                return SelectedItemText;
            }
            set
            {
                SelectedItemText = value;
            }
        }
        public string CurrentValue
        {
            set
            {
                int i = 0;
                foreach (ListItem item in Items)
                {
                    if (item.Value.Trim() == value.Trim())
                    {
                        SelectedIndex = i;
                        break;
                    }
                    i++;
                }
                _LastSetValue = value.Trim();
            }
            get
            {
                return _LastSetValue;
            }
        }
        protected virtual void ApplyProperties(ListControl control)
        {
            int i;
            string txt;
            base.ApplyProperties(control);
            for (i = 1; i <= ItemCount; i++)
            {
                txt = GXUtil.ValueDecode(getItemText(i));
                control.Items.Add(new ListItem(txt, getItemValue(i)));
            }
            control.SelectedIndex = SelectedIndex;
        }

        public override void ToJSON()
        {
            jsonObj.Put("isset", _IsSet);
            jsonObj.Put("s", SelectedItemValue.Trim());
            JArray jsonArrValues = new JArray();
            Dictionary<string, JArray> itemsHash = new Dictionary<string, JArray>();
            foreach (ListItem Item in Items)
            {
				string itemValue = Item.Value.TrimEnd();
				if (!itemsHash.ContainsKey(itemValue))
				{
					JArray jsonItem = new JArray();
					jsonItem.Add(itemValue);
					jsonItem.Add(Item.Text);
					itemsHash.Add(itemValue, jsonItem);
					jsonArrValues.Add(jsonItem);
				}
				else
				{
					itemsHash[itemValue][1] = Item.Text;
				}
            }
            jsonObj.Put("v", jsonArrValues);
        }

        public override void FromJSONObject(dynamic Obj)
        {
            this.removeAllItems();
            JObject jsonObj = (JObject)Obj;
            JArray jsonArrValues = (JArray)jsonObj["v"];
            if (jsonArrValues != null)
            {
                var idx = 1;
                for (int i = 0; i < jsonArrValues.Count; i++)
                {
                    JArray jsonItem = jsonArrValues.GetArray(i);
                    this.addItem(jsonItem[0].ToString(), jsonItem[1].ToString(), idx);
                    idx++;
                }
                object selected = jsonObj["s"];
                if (selected != null)
                {
                    this.CurrentValue = selected.ToString();
                }
            }
        }
    }
    
    public class GXCheckbox : GXWebControl
    {
        string _Caption;
        string _Value;
        bool _Checked;

        public GXCheckbox()
            : base()
        {
            _Caption = "";
            _Value = "";
        }
        public bool Checked
        {
            get
            {
                return _Checked;
            }
            set
            {
                _Checked = value;
            }
        }
        public string Caption
        {
            get
            {
                return _Caption;
            }
            set
            {
                _Caption = value;
            }
        }
        public string CheckedValue
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
            }
        }
        public void Render(HtmlTextWriter ControlOutputWriter)
        {
            CheckBox chkbox = new CheckBox();
            if (Visible == 0 || Enabled == 0)
                SendHidden(Name, (Checked ? "1" : "0"), ControlOutputWriter);
            ApplyProperties(chkbox);
            chkbox.Checked = Checked;
            chkbox.Text = Caption;
            chkbox.RenderControl(ControlOutputWriter);
        }

        public override void ToJSON()
        {
            jsonObj.Put(CheckedValue, Checked);
        }

    }
    
    public class GXListbox : GXWebListControl
    {
        private int _Rows;
        public int Rows
        {
            get { return _Rows; }
            set { _Rows = value; }
        }
        public void Render(HtmlTextWriter ControlOutputWriter)
        {
            ListBox list = new ListBox();
            if (Visible == 0 || Enabled == 0)
            {
                SendHidden(Name, SelectedItemValue, ControlOutputWriter);
                Name = Name + "_disabled";
            }
            else
            {
                if (EventJsCode.Length > 0)
                    SendHidden(Name + "_gxc", "", ControlOutputWriter);
            }
            ApplyProperties(list);
            list.Rows = Rows;
            AddEvent("OnChange", list);
            list.RenderControl(ControlOutputWriter);
        }
    }
    
    public class GXCombobox : GXWebListControl
    {
        public void Render(HtmlTextWriter ControlOutputWriter)
        {
            DropDownList combo = new DropDownList();

            if (Visible == 0 || Enabled == 0)
            {
                SendHidden(Name, SelectedItemValue, ControlOutputWriter);
                Name = Name + "_disabled";
            }
            else
            {
                if (EventJsCode.Length > 0)
                    SendHidden(Name + "_gxc", "", ControlOutputWriter);
            }
            if (Enabled != 0)
            {
                ApplyProperties(combo);
                AddEvent("OnChange", combo);
                combo.RenderControl(ControlOutputWriter);
            }
            else
            {
                Label lab = new Label();
                ApplyProperties(lab);
                lab.Text = SelectedItemText;
                lab.Visible = Visible != 0;
                lab.Enabled = true;
                lab.Attributes.Remove("onFocus");
                lab.RenderControl(ControlOutputWriter);
            }
        }
    }
    
    public class GXRadio : GXWebListControl
    {
        public const int Horizontal = 0;
        public const int Vertical = 1;

        int _Direction = Vertical;
        short _Columns;

        public int Orientation
        {
            get
            {
                return _Direction;
            }
            set
            {
                _Direction = value;
            }
        }
        public short Columns
        {
            get
            {
                return _Columns;
            }
            set
            {
                _Columns = value;
            }
        }
        public void Render(HtmlTextWriter ControlOutputWriter)
        {
            RadioButtonList radlist = new RadioButtonList();
            if (Visible == 0 || Enabled == 0)
                SendHidden(Name, SelectedItemValue, ControlOutputWriter);
            ApplyProperties(radlist);
            radlist.RepeatColumns = _Columns;
            if (Orientation == Horizontal) radlist.RepeatDirection = RepeatDirection.Horizontal;
            radlist.RenderControl(ControlOutputWriter);
        }
    }
    public class GXWebPicture : GXWebControl
    {
        private string _AlternateText = "";
        private ImageAlign _ImageAlign;
        private string _SourceURL = "";
        private int _HSpace;
        private int _VSpace;

        public int HSpace
        {
            get
            {
                return _HSpace;
            }
            set
            {
                _HSpace = value;
            }
        }
        public int VSpace
        {
            get
            {
                return _VSpace;
            }
            set
            {
                _VSpace = value;
            }
        }
        public string SourceURL
        {
            get
            {
                return _SourceURL;
            }
            set
            {
                _SourceURL = value;
            }
        }
        public string AlternateText
        {
            get
            {
                return _AlternateText;
            }
            set
            {
                _AlternateText = value;
            }
        }
        public ImageAlign ImageAlign
        {
            get
            {
                return _ImageAlign;
            }
            set
            {
                _ImageAlign = value;
            }
        }
        public void Render(HtmlTextWriter ControlOutputWriter)
        {
#if !NETCORE
			System.Web.UI.WebControls.Image image = (Link.Trim().Length > 0) ? new ImageButton() : new System.Web.UI.WebControls.Image();
            ApplyProperties(image);
            image.AlternateText = AlternateText;
            image.ImageAlign = ImageAlign;
            image.ImageUrl = SourceURL;
            if (HSpace > 0) image.Attributes.Add("hspace", HSpace.ToString());
            if (VSpace > 0) image.Attributes.Add("vspace", VSpace.ToString());

            if (Link.Trim().Length > 0)
            {
                HyperLink hl = new HyperLink();
                ApplyProperties(hl);
                hl.NavigateUrl = Link;
                hl.Target = LinkTarget;
                hl.Controls.Add(image);
                hl.RenderControl(ControlOutputWriter);
            }
            else
                image.RenderControl(ControlOutputWriter);
#endif
		}

        public override void ToJSON()
        {

        }

    }
    public class GXWebButton : GXWebControl
    {
        private string _Caption;
        public string Caption
        {
            get
            {
                return _Caption;
            }
            set
            {
                _Caption = value;
            }
        }
        public void Render(HtmlTextWriter ControlOutputWriter)
        {
            Button button = new Button();
            ApplyProperties(button);
            button.Text = Caption;
            AddEvent("OnClick", button);
            
            button.RenderControl(ControlOutputWriter);
        }
        public override void ToJSON()
        {
        }

    }
    public class GxWebControlTitle
    {
        string _Text;
        int _ForeColor;
        int _BackColor;
        short _FontBold;
        short _FontItalic;
        short _FontUnderline;
        short _FontStrikethru;
        int _FontSize;
        string _FontName;
        short _BackStyle;

        public GxWebControlTitle()
        {
            _Text = "";
            _FontName = "";
        }
        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }
        public int ForeColor
        {
            get { return _ForeColor; }
            set { _ForeColor = value; }
        }
        public int BackColor
        {
            get { return _BackColor; }
            set { _BackColor = value; }
        }
        public short FontBold
        {
            get { return _FontBold; }
            set { _FontBold = value; }
        }
        public short FontItalic
        {
            get { return _FontItalic; }
            set { _FontItalic = value; }
        }
        public short FontUnderline
        {
            get { return _FontUnderline; }
            set { _FontUnderline = value; }
        }
        public short FontStrikethru
        {
            get { return _FontStrikethru; }
            set { _FontStrikethru = value; }
        }
        public int FontSize
        {
            get { return _FontSize; }
            set { _FontSize = value; }
        }
        public string FontName
        {
            get { return _FontName; }
            set { _FontName = value; }
        }
        public short BackStyle
        {
            get { return _BackStyle; }
            set { _BackStyle = value; }
        }
    }
	public class GXUserControl
	{
		GxDictionary propertyBag = new GxDictionary();
		public void SetProperty(String propertyName, object propertyValue)
		{
			string stringValue = propertyValue?.ToString();
			if (propertyValue is Boolean)
			{
				stringValue = stringValue?.ToLower();
			}
			if (!String.IsNullOrEmpty(stringValue))
				propertyBag[propertyName] = stringValue;
		}
		public void SendProperty(IGxContext context, String componentPrefix, bool isMasterPage, String internalName, String propertyName, String propertyValue) 
		{
			context.httpAjaxContext.ajax_rsp_assign_uc_prop(componentPrefix, isMasterPage, internalName, propertyName, propertyValue);
			SetProperty(propertyName, propertyValue);
		}
		public void Render(IGxContext context, String controlType, String internalName, String htmlId)
		{
			context.RenderUserControl(controlType, internalName, htmlId, propertyBag);
		}
	}
}
