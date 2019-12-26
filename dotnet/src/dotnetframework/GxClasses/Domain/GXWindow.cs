using GeneXus.Utils;
using System;
using Jayrock.Json;
namespace GeneXus.Application
{
	public class GXWindow : IGxJSONAble
	{
		private JArray jArr;
		private string _url;
		private string _cssClassName;
		private int _autoresize;
		private int _width;
		private int _height;
		private int _position;
		private int _top;
		private int _left;
		private Object[] _oncloseCmds;
		private Object[] _returnParms;

		public GXWindow()
		{
			jArr = new JArray();
			_url = string.Empty;
			_autoresize = 1;
			_cssClassName = string.Empty;
			_oncloseCmds = Array.Empty<object>();
			_returnParms = Array.Empty<object>();
			
		}

		public string Url
		{
			get { return _url; }
			set { _url = value; }
		}

		public string Class
		{
			get { return _cssClassName; }
			set { _cssClassName = value; }
		}

		public int Autoresize
		{
			get { return _autoresize; }
			set { _autoresize = value; }
		}

		public int Width
		{
			get { return _width; }
			set { _width = value; }
		}

		public int Height
		{
			get { return _height; }
			set { _height = value; }
		}

		public int Position
		{
			get { return _position; }
			set { _position = value; }
		}

		public int Top
		{
			get { return _top; }
			set { _top = value; }
		}

		public int Left
		{
			get { return _left; }
			set { _left = value; }
		}

		public void SetReturnParms(Object[] returnParms)
		{
			_returnParms = returnParms;
		}

		public void AddObjectProperty(string name, object prop)
		{
		}

		public object GetJSONObject()
		{
			ToJSON();
			return jArr;
		}
		public object GetJSONObject(bool includeState)
		{
			return GetJSONObject();
		}

		public void FromJSONObject(IJsonFormattable obj)
		{
		}

		private void ToJSON()
		{
			jArr.Clear();
			jArr.Add(_url);
			jArr.Add(_autoresize);
			jArr.Add(_width);
			jArr.Add(_height);
			jArr.Add(_position);
			jArr.Add(_top);
			jArr.Add(_left);
			jArr.Add(GeneXus.Http.HttpAjaxContext.GetParmsJArray(_oncloseCmds));
			jArr.Add(GeneXus.Http.HttpAjaxContext.GetParmsJArray(_returnParms));
			jArr.Add(_cssClassName);
		}

		public string ToJavascriptSource()
		{
			ToJSON();
			return jArr.ToString();
		}
	}

}