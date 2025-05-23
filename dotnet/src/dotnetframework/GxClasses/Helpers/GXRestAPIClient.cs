using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Utils;
using GeneXus.Http.Client;
using System.Web;
using GeneXus.Mime;
#if NETCORE
using System.Text.Json;
using System.Text.Json.Serialization;
#endif
using System.IO;

namespace GeneXus.Application
{
	public class GXRestAPIClient
	{
		private const string DATE_NULL = "0000-00-00";
		private const string DATETIME_NULL = "0000-00-00'T'00:00:00";
		private const string DATE_FORMAT = "yyyy-MM-dd";
		private const string DATETIME_FORMAT = "yyyy-MM-ddTHH:mm:ss";
		private const string DATETIME_MS_FORMAT = "yyyy-MM-ddTHH:mm:ss.fff";
		
		public GXRestAPIClient()
		{
			Location = new GxLocation();
			Location.BaseUrl = "api";
			Location.Host = "www.example.com";
			Location.ResourceName = "service";
#if NETCORE
			Location.Port = 8082;
#else
			Location.Port = 80;
#endif
		}


		private GxHttpClient httpClient = new GxHttpClient();
		public GxLocation Location { get; set; }
		public string Name { get; set; }
		public int ErrorCode { get; set; }
		public string ErrorMessage { get; set; }

		public int StatusCode { get; set; }
		public string StatusMessage { get ; set; }
		public int ResponseCode { get => responseCode; set => responseCode = value; }
		public string ResponseMessage { get => responseMessage; set => responseMessage = value; }
		public string HttpMethod { get => httpMethod; set => httpMethod = value; }

		public int protocol = 1;

		private string httpMethod = "GET";

		private Dictionary<string, string> _queryVars = new Dictionary<string, string>();
		private Dictionary<string, string> _headerVars = new Dictionary<string, string>();
		private Dictionary<string, string> _bodyVars = new Dictionary<string, string>();
		//private Dictionary<string, string> _pathVars = new Dictionary<string, string>();
		private Dictionary<string,object> _responseData = new Dictionary<string, object>();

		private string _contentType = "application/json; charset=utf-8";
		private string _queryString = String.Empty;
		private string _bodyString = String.Empty;

		private int responseCode = 0;
		private string responseMessage = String.Empty;


		#region "Header Vars"

		public void AddHeaderVar(String varName, String varValue)
		{
			_headerVars[varName] = GXUtil.UrlEncode(varValue);
		}
		public void AddHeaderVar(String varName, int varValue)
		{
			_headerVars[varName] = varValue.ToString();
		}
		public void AddHeaderVar(String varName, long varValue)
		{
			_headerVars[varName] = varValue.ToString();
		}

		public void AddHeaderVar(String varName, short varValue)
		{
			_headerVars[varName] = varValue.ToString();
		}
		public void AddHeaderVar(String varName, Decimal varValue)
		{
			_headerVars[varName] = varValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
		}
		public void AddHeaderVar(String varName, DateTime varValue)
		{
			_headerVars[varName] = varValue.ToString(DATE_FORMAT);
		}
		public void AddHeaderVar(String varName, DateTime varValue, bool hasMilliseconds)
		{
			string fmt = DATETIME_FORMAT;
			if (hasMilliseconds)
				fmt = DATETIME_MS_FORMAT;
			_headerVars[varName] = varValue.ToString(fmt);
		}
		public void AddHeaderVar(String varName, Guid varValue)
		{
			_headerVars[varName] = varValue.ToString();
		}
		public void AddHeaderVar(String varName, Geospatial varValue)
		{
			_headerVars[varName] = GXUtil.UrlEncode(varValue.ToString());
		}
		public void AddHeaderVar(String varName, bool varValue)
		{
			_headerVars[varName] = StringUtil.BoolToStr(varValue);
		}
		public void AddHeaderVar(String varName, GxUserType varValue)
		{
			if (varValue != null)
			{
				_headerVars[varName] = varValue.ToJSonString();
			}
		}
		public void AddHeaderVar(String varName, IGxCollection varValue)
		{
			if (varValue != null)
			{
				_headerVars[varName] = varValue.ToJSonString();
			}
		}
		#endregion

		#region "Query Vars"
		public void AddQueryVar(String varName, String varValue)
		{
			_queryVars[varName] = GXUtil.UrlEncode(varValue);
		}

		public void AddQueryVar(String varName, int varValue)
		{
			_queryVars[varName] = varValue.ToString();
		}
		public void AddQueryVar(String varName, long varValue)
		{
			_queryVars[varName] = varValue.ToString();
		}

		public void AddQueryVar(String varName, short varValue)
		{
			_queryVars[varName] = varValue.ToString();
		}

		public void AddQueryVar(String varName, Decimal varValue)
		{
			_queryVars[varName] = varValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
		}

		public void AddQueryVar(String varName, DateTime varValue)
		{
			_queryVars[varName] = varValue.ToString(DATE_FORMAT);
		}

		public void AddQueryVar(String varName, DateTime varValue, bool hasMilliseconds)
		{
			string fmt = DATETIME_FORMAT;
			if (hasMilliseconds)
				fmt = DATETIME_MS_FORMAT;
			_queryVars[varName] = varValue.ToString(fmt);
		}
		public void AddQueryVar(String varName, Guid varValue)
		{
			_queryVars[varName] = varValue.ToString();
		}

		public void AddQueryVar(String varName, Geospatial varValue)
		{
			_queryVars[varName] = GXUtil.UrlEncode(varValue.ToString());
		}

		public void AddQueryVar(String varName, bool varValue)
		{
			_queryVars[varName] = StringUtil.BoolToStr(varValue);
		}

		public void AddQueryVar(String varName, GxUserType varValue)
		{
			if (varValue != null)
			{
				_bodyVars[varName] = varValue.ToJSonString();
			}			
		}

		public void AddQueryVar(String varName, IGxCollection varValue)
		{
			if (varValue != null)
			{
				_bodyVars[varName] = varValue.ToJSonString();
			}
		}
		#endregion

		#region "Body Vars"

		public void AddBodyVar(String varName, DateTime varValue)
		{
			_bodyVars[varName] = "\"" +  varValue.ToString(DATE_FORMAT) + "\"";
		}

		public void AddBodyVar(String varName, DateTime varValue, bool hasMilliseconds)
		{
			string fmt = DATETIME_FORMAT;
			if (hasMilliseconds)
				fmt = DATETIME_MS_FORMAT;
			_bodyVars[varName] = "\"" + varValue.ToString(fmt) + "\"";
		}

		public void AddBodyVar(String varName, Decimal varValue)
		{
			_bodyVars[varName] = varValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
		}

		public void AddBodyVar(String varName, string varValue)
		{
			_bodyVars[varName] = "\"" + varValue + "\"" ;
		}
		public void AddBodyVar(String varName, int varValue)
		{
			_bodyVars[varName] = varValue.ToString();
		}
		public void AddBodyVar(String varName, short varValue)
		{
			_bodyVars[varName] = varValue.ToString();
		}
		public void AddBodyVar(String varName, long varValue)
		{
			_bodyVars[varName] = varValue.ToString();
		}
		public void AddBodyVar(String varName, bool varValue)
		{
			_bodyVars[varName] = StringUtil.BoolToStr(varValue);
		}
		public void AddBodyVar(String varName, Guid varValue)
		{
			_bodyVars[varName] = "\"" + varValue.ToString() + "\"";
		}

		public void AddBodyVar(String varName, Geospatial varValue)
		{
			_bodyVars[varName] = "\"" + varValue.ToString() + "\"";
		}

		public void AddBodyVar(String varName, GxUserType varValue)
		{
			if (varValue != null)
			{
				_bodyVars[varName] = varValue.ToJSonString();
			}
		}

		public void AddBodyVar(String varName, IGxCollection varValue)
		{
			if (varValue != null)
			{
				_bodyVars[varName] = varValue.ToJSonString();
			}
		}

		#endregion

		#region "Get Header Vars"
		public string GetHeaderString(string varName)
		{
			return httpClient.GetHeader(varName);
		}

		public DateTime GetHeaderDate(string varName)
		{
			string val = GetHeaderString(varName);
			if (val.StartsWith(DATE_NULL))
				return DateTimeUtil.NullDate();
			return DateTime.ParseExact(val, DATE_FORMAT, System.Globalization.CultureInfo.InvariantCulture);
		}

		public DateTime GetHeaderDateTime(string varName, bool hasMilliseconds)
		{
			string val = GetHeaderString(varName);
			if (val.StartsWith(DATETIME_NULL))
				return DateTimeUtil.NullDate();
			string fmt = DATETIME_FORMAT;
			if (hasMilliseconds)
				fmt = DATETIME_MS_FORMAT;
			return DateTime.ParseExact(val, fmt, System.Globalization.CultureInfo.InvariantCulture);
		}

		public bool GetHeaderBool(string varName)
		{
			httpClient.GetHeader(varName, out bool val);
			return val;
		}
		public Guid GetHeaderGuid(string varName)
		{
			return Guid.Parse(GetHeaderString(varName));
		}

		public Decimal GetHeaderNum(string varName)
		{
			httpClient.GetHeader(varName, out Decimal val);
			return val;
		}
		public long GetHeaderLong(string varName)
		{
			httpClient.GetHeader(varName, out long val);
			return val;
		}
		public int GetHeaderInt(string varName)
		{
			httpClient.GetHeader(varName, out int val);
			return val;
		}

		public short GetHeaderShort(string varName)
		{
			httpClient.GetHeader(varName, out short val);
			return val;
		}

		public Geospatial GetHeaderGeospatial(string varName)
		{
			Geospatial g = new Geospatial(GetHeaderString(varName));
			if (Geospatial.IsNullOrEmpty(g))
			{
				g.FromGeoJSON(GetJsonStr(varName));
			}
			return g;
		}

		#endregion

		#region "Get Body Vars"

		public string GetBodyString(string varName)
		{
			return  GetJsonStr(varName);			
		}

		public DateTime GetBodyDate(string varName)
		{
			string val = GetJsonStr(varName);
			if (val.StartsWith(DATE_NULL))
				return DateTimeUtil.NullDate();
			return DateTime.ParseExact(val, DATE_FORMAT, System.Globalization.CultureInfo.InvariantCulture);
		}

		public DateTime GetBodyDateTime(string varName, bool hasMilliseconds)
		{
			string val = GetJsonStr(varName);
			if (val.StartsWith(DATETIME_NULL))
				return DateTimeUtil.NullDate();
			string fmt = DATETIME_FORMAT;
			if (hasMilliseconds)
				fmt = DATETIME_MS_FORMAT;
			return DateTime.ParseExact(val, fmt,System.Globalization.CultureInfo.InvariantCulture);
		}

		public bool GetBodyBool(string varName)
		{			
			return  Boolean.Parse(GetJsonStr(varName));
		}
		public Guid GetBodyGuid(string varName)
		{			
			return Guid.Parse(GetJsonStr(varName));
		}

		public Decimal GetBodyNum(string varName)
		{			
			return Decimal.Parse( GetJsonStr(varName), System.Globalization.NumberStyles.Float);
		}
		public long GetBodyLong(string varName)
		{
			return long.Parse(GetJsonStr(varName));
		}
		public int GetBodyInt(string varName)
		{
			return Int32.Parse(GetJsonStr(varName));
		}

		public short GetBodyShort(string varName)
		{			
			return (short)Int16.Parse(GetJsonStr(varName));
		}

		public Geospatial GetBodyGeospatial(string varName)
		{
			Geospatial g = new Geospatial(GetJsonStr(varName));
			if (Geospatial.IsNullOrEmpty(g))
			{
				g.FromGeoJSON(GetJsonStr(varName));
			}
			return g;
		}

		public string GetJsonStr(string varName)
		{
			string s = String.Empty;
			if (_responseData.ContainsKey(varName.ToLower()))
			{
				s = _responseData[varName.ToLower()].ToString();
			}
			else if (_responseData.Count == 1 && _responseData.ContainsKey(String.Empty))
			{
				s = _responseData[String.Empty].ToString();
			}
			return s;	
		}
		
		public T GetBodySdt<T>(string varName) where T:GxUserType, new()
		{
			T sdt = new T();
			if (_responseData.ContainsKey(varName.ToLower()) && _responseData.Count == 1) //wrapped sdt
			{
				sdt.FromJSonString(_responseData[varName.ToLower()].ToString(), null);
			}
			else if (_responseData.Count == 1 && _responseData.ContainsKey(String.Empty)) // unwrapped 
			{
				sdt.FromJSonString(_responseData[String.Empty].ToString(), null);
			}			
			else if (_responseData.Count >= 1) // can contain the same key (recursive unwrapped)
			{

#if NETCORE
				string rData = JsonSerializer.Serialize(_responseData);
				if (sdt.FromJSonString(rData, null))
					return sdt;
				else
					sdt.FromJSonString("{" + varName + ":" + rData + "}", null);
#else
				string rData = JSONHelper.Serialize(_responseData);
				if (sdt.FromJSonString(rData, null))
					return sdt;
				else
					sdt.FromJSonString("{" + varName + ":" + rData + "}", null);
#endif
			}
			return sdt;
		}

		public GXBaseCollection<T> GetBodySdtCollection<T>(string varName) where T:GxUserType , new()
		{			
			GXBaseCollection<T> collection = new GXBaseCollection<T>();
			if (_responseData.ContainsKey(varName.ToLower()))
			{				
				collection.FromJSonString(_responseData[varName.ToLower()].ToString(), null);
			}
			else if (_responseData.Count == 1 && _responseData.ContainsKey(String.Empty))
			{
				collection.FromJSonString(_responseData[String.Empty].ToString(), null);
			}
			return collection;
		}

		public GxSimpleCollection<T> GetBodySimpleCollection<T>(string varName) 
		{			
			GxSimpleCollection<T> collection = new GxSimpleCollection<T>();			
			if (_responseData.ContainsKey(varName.ToLower()))
			{
				collection.FromJSonString(_responseData[varName.ToLower()].ToString(), null);
			}
			else if (_responseData.Count == 1 && _responseData.ContainsKey(String.Empty))
			{
				collection.FromJSonString(_responseData[String.Empty].ToString(), null);
			}
			return collection;
		}
		#endregion

		public void AddUploadFile(string FilePath, string name)
		{
			httpClient.AddFile(FilePath, name);
			string mimeType = MimeMapping.GetMimeMapping(FilePath);
			_contentType = mimeType;
		}

		public void RestExecute()
		{
			this.ErrorCode = 0;
      this.ErrorMessage = "";
			this.StatusCode = 0;
			this.StatusMessage = "";

			if (_headerVars.Count > 0)
			{
				foreach (string key in _headerVars.Keys)
				{
					httpClient.AddHeader(key, _headerVars[key]);
				}
			}
			_queryString = String.Empty;
			if (_queryVars.Count > 0)
			{
				string separator = "?";
				foreach (string key in _queryVars.Keys)
				{
					_queryString += string.Format("{0}{1}={2}", separator, key, _queryVars[key]);
					separator = "&";
				}
			}
			_bodyString = String.Empty;
			if (_bodyVars.Count > 0)
			{
				string separator = String.Empty;
				foreach (string key in _bodyVars.Keys)
				{
					_bodyString +=  separator + "\"" + key + "\":" + _bodyVars[key];
					separator = ",";
				}
			}
			if (_bodyString.Length > 0)
			{
				_bodyString = "{" + _bodyString + "}";
				httpClient.AddString(_bodyString);
				httpClient.AddHeader("Content-Type", _contentType);
			}
			else
			{
				if (this.httpMethod == "POST" || this.httpMethod == "PUT")
				{
					_bodyString = "{}";
					httpClient.AddString(_bodyString);
					httpClient.AddHeader("Content-Type", _contentType);
				}
			}
			if (this.Location.AuthenticationMethod == 4 && !String.IsNullOrEmpty(this.Location.AccessToken))
			{
				httpClient.AddHeader("Authorization", this.Location.AccessToken);
			}
			string serviceuri = ((this.Location.Secure > 0) ? "https" : "http") + "://" + this.Location.Host;
			serviceuri += (this.Location.Port != 80) ? ":" + this.Location.Port.ToString() : String.Empty;
			serviceuri += "/" + this.Location.BaseUrl.TrimEnd('/').TrimStart('/') + "/" + this.Location.ResourceName;
			serviceuri += _queryString;			
			httpClient.HttpClientExecute( this.HttpMethod, serviceuri);
			this.ErrorCode = httpClient.ErrCode;
			this.ErrorMessage = httpClient.ErrDescription;
			this.StatusCode = httpClient.StatusCode;
			this.StatusMessage = httpClient.ReasonLine;
			if (httpClient.StatusCode >= 300 || httpClient.ErrCode > 0)
			{
				_responseData = new Dictionary<string, object>();
			}
			else
			{
				_responseData = GeneXus.Utils.RestAPIHelpers.ReadRestParameters(httpClient.ToString());
			}
		}
	}
}
