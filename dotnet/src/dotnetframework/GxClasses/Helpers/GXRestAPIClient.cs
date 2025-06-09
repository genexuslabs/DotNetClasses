using System;
using System.Collections.Generic;
using GeneXus.Utils;
using GeneXus.Http.Client;
using System.Web;
using GeneXus.Mime;
using System.Globalization;
using GeneXus.Http;
using System.Threading;




#if NETCORE
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace GeneXus.Application
{
	public class GXRestAPIClient
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<GXRestAPIClient>();

		private const string DATE_EMPTY = "0000-00-00";
		private const string DATETIME_EMPTY = "0000-00-00T00:00:00";
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
		public string StatusMessage { get; set; }
		public int ResponseCode { get => responseCode; set => responseCode = value; }
		public string ResponseMessage { get => responseMessage; set => responseMessage = value; }
		public string HttpMethod { get => httpMethod; set => httpMethod = value; }

		public int protocol = 1;

		private string httpMethod = "GET";

		private Dictionary<string, string> _queryVars = new Dictionary<string, string>();
		private Dictionary<string, string> _headerVars = new Dictionary<string, string>();
		private Dictionary<string, string> _bodyVars = new Dictionary<string, string>();
		//private Dictionary<string, string> _pathVars = new Dictionary<string, string>();
		private Dictionary<string, object> _responseData = new Dictionary<string, object>();

		// Internal properties for testing purposes
		internal Dictionary<string, string> BodyVars => _bodyVars;
		internal Dictionary<string, object> ResponseData { get => _responseData; set => _responseData = value; }

		private string _contentType = "application/json; charset=utf-8";
		private string _queryString = string.Empty;
		private string _bodyString = string.Empty;

		private int responseCode = 0;
		private string responseMessage = string.Empty;


		#region "Header Vars"

		public void AddHeaderVar(string varName, string varValue)
		{
			_headerVars[varName] = GXUtil.UrlEncode(varValue);
		}
		public void AddHeaderVar(string varName, int varValue)
		{
			_headerVars[varName] = varValue.ToString();
		}
		public void AddHeaderVar(string varName, long varValue)
		{
			_headerVars[varName] = varValue.ToString();
		}

		public void AddHeaderVar(string varName, short varValue)
		{
			_headerVars[varName] = varValue.ToString();
		}
		public void AddHeaderVar(string varName, decimal varValue)
		{
			_headerVars[varName] = varValue.ToString(CultureInfo.InvariantCulture);
		}
		public void AddHeaderVar(string varName, DateTime varValue)
		{
			_headerVars[varName] = varValue.ToString(DATE_FORMAT);
		}
		public void AddHeaderVar(string varName, DateTime varValue, bool hasMilliseconds)
		{
			string fmt = DATETIME_FORMAT;
			if (hasMilliseconds)
				fmt = DATETIME_MS_FORMAT;
			_headerVars[varName] = varValue.ToString(fmt);
		}
		public void AddHeaderVar(string varName, Guid varValue)
		{
			_headerVars[varName] = varValue.ToString();
		}
		public void AddHeaderVar(string varName, Geospatial varValue)
		{
			_headerVars[varName] = GXUtil.UrlEncode(varValue.ToString());
		}
		public void AddHeaderVar(string varName, bool varValue)
		{
			_headerVars[varName] = StringUtil.BoolToStr(varValue);
		}
		public void AddHeaderVar(string varName, GxUserType varValue)
		{
			if (varValue != null)
			{
				_headerVars[varName] = varValue.ToJSonString();
			}
		}
		public void AddHeaderVar(string varName, IGxCollection varValue)
		{
			if (varValue != null)
			{
				_headerVars[varName] = varValue.ToJSonString();
			}
		}
		#endregion

		#region "Query Vars"
		public void AddQueryVar(string varName, string varValue)
		{
			_queryVars[varName] = GXUtil.UrlEncode(varValue);
		}

		public void AddQueryVar(string varName, int varValue)
		{
			_queryVars[varName] = varValue.ToString();
		}
		public void AddQueryVar(string varName, long varValue)
		{
			_queryVars[varName] = varValue.ToString();
		}

		public void AddQueryVar(string varName, short varValue)
		{
			_queryVars[varName] = varValue.ToString();
		}

		public void AddQueryVar(string varName, decimal varValue)
		{
			_queryVars[varName] = varValue.ToString(CultureInfo.InvariantCulture);
		}

		public void AddQueryVar(string varName, DateTime varValue)
		{
			_queryVars[varName] = varValue.ToString(DATE_FORMAT);
		}

		public void AddQueryVar(string varName, DateTime varValue, bool hasMilliseconds)
		{
			string fmt = DATETIME_FORMAT;
			if (hasMilliseconds)
				fmt = DATETIME_MS_FORMAT;
			_queryVars[varName] = varValue.ToString(fmt);
		}
		public void AddQueryVar(string varName, Guid varValue)
		{
			_queryVars[varName] = varValue.ToString();
		}

		public void AddQueryVar(string varName, Geospatial varValue)
		{
			_queryVars[varName] = GXUtil.UrlEncode(varValue.ToString());
		}

		public void AddQueryVar(string varName, bool varValue)
		{
			_queryVars[varName] = StringUtil.BoolToStr(varValue);
		}

		public void AddQueryVar(string varName, GxUserType varValue)
		{
			if (varValue != null)
			{
				_bodyVars[varName] = varValue.ToJSonString();
			}
		}

		public void AddQueryVar(string varName, IGxCollection varValue)
		{
			if (varValue != null)
			{
				_bodyVars[varName] = varValue.ToJSonString();
			}
		}
		#endregion

		#region "Body Vars"

		public void AddBodyVar(string varName, DateTime varValue)
		{
			_bodyVars[varName] = "\"" + varValue.ToString(DATE_FORMAT) + "\"";
		}

		public void AddBodyVar(string varName, DateTime varValue, bool hasMilliseconds)
		{
			string fmt = DATETIME_FORMAT;
			if (hasMilliseconds)
				fmt = DATETIME_MS_FORMAT;
			_bodyVars[varName] = "\"" + varValue.ToString(fmt) + "\"";
		}

		public void AddBodyVar(string varName, decimal varValue)
		{
			_bodyVars[varName] = varValue.ToString(CultureInfo.InvariantCulture);
		}

		public void AddBodyVar(string varName, string varValue)
		{
			_bodyVars[varName] = "\"" + varValue + "\"";
		}
		public void AddBodyVar(string varName, int varValue)
		{
			_bodyVars[varName] = varValue.ToString();
		}
		public void AddBodyVar(string varName, short varValue)
		{
			_bodyVars[varName] = varValue.ToString();
		}
		public void AddBodyVar(string varName, long varValue)
		{
			_bodyVars[varName] = varValue.ToString();
		}
		public void AddBodyVar(string varName, bool varValue)
		{
			_bodyVars[varName] = StringUtil.BoolToStr(varValue);
		}
		public void AddBodyVar(string varName, Guid varValue)
		{
			_bodyVars[varName] = "\"" + varValue.ToString() + "\"";
		}

		public void AddBodyVar(string varName, Geospatial varValue)
		{
			_bodyVars[varName] = "\"" + varValue.ToString() + "\"";
		}

		public void AddBodyVar(string varName, GxUserType varValue)
		{
			if (varValue != null)
			{
				_bodyVars[varName] = varValue.ToJSonString();
			}
		}

		public void AddBodyVar(string varName, IGxCollection varValue)
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
			if (val.StartsWith(DATE_EMPTY))
				return DateTimeUtil.NullDate();
			return DateTime.ParseExact(val, DATE_FORMAT, CultureInfo.InvariantCulture);
		}

		public DateTime GetHeaderDateTime(string varName, bool hasMilliseconds)
		{
			string val = GetHeaderString(varName);
			if (val.StartsWith(DATETIME_EMPTY))
				return DateTimeUtil.NullDate();
			string fmt = DATETIME_FORMAT;
			if (hasMilliseconds)
				fmt = DATETIME_MS_FORMAT;
			return DateTime.ParseExact(val, fmt, CultureInfo.InvariantCulture);
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

		public decimal GetHeaderNum(string varName)
		{
			httpClient.GetHeader(varName, out decimal val);
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
			return GetJsonStr(varName);
		}

		public DateTime GetBodyDate(string varName)
		{
			string val = GetJsonStr(varName);
			if (val.StartsWith(DATE_EMPTY))
				return DateTimeUtil.NullDate();
			return DateTime.ParseExact(val, DATE_FORMAT, CultureInfo.InvariantCulture);
		}

		public DateTime GetBodyDateTime(string varName, bool hasMilliseconds)
		{
			string val = GetJsonStr(varName);
			if (val.StartsWith(DATETIME_EMPTY))
				return DateTimeUtil.NullDate();
			string fmt = DATETIME_FORMAT;
			if (hasMilliseconds)
				fmt = DATETIME_MS_FORMAT;
			return DateTime.ParseExact(val, fmt, CultureInfo.InvariantCulture);
		}

		public bool GetBodyBool(string varName)
		{
			return Boolean.Parse(GetJsonStr(varName));
		}
		public Guid GetBodyGuid(string varName)
		{
			return Guid.Parse(GetJsonStr(varName));
		}

		internal object GetBodyValue(string varName)
		{
			try
			{
				if (_responseData.TryGetValue(varName.ToLower(), out object value))
				{
					return value;
				}
				else if (_responseData.Count == 1 && _responseData.ContainsKey(String.Empty))
				{
					return _responseData[String.Empty];
				}

				return null;
			}
			catch (Exception ex)
			{
				GXLogging.Warn(log, "Failed to get value from response:" + varName, ex);
				return null;
			}
		}

		public decimal GetBodyNum(string varName)
		{
			object value;
			try
			{
				value = GetBodyValue(varName);

				if (value != null)
				{
					if (value is decimal decimalValue)
						return decimalValue;

					if (value is double doubleValue)
						return (decimal)doubleValue;

					if (value is int intValue)
						return intValue;

					if (value is long longValue)
						return longValue;

					if (value is short shortValue)
						return shortValue;

					if (value is float floatValue)
						return (decimal)floatValue;

				}

				return decimal.Parse(GetJsonStr(varName), NumberStyles.Float, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Failed to get number value:", GetJsonStr(varName), ex);
			}
			return 0m;
		}
		public long GetBodyLong(string varName)
		{
			try
			{
				object value = GetBodyValue(varName);

				if (value != null)
				{
					if (value is long longValue)
						return longValue;

					if (value is int intValue)
						return intValue;

					if (value is short shortValue)
						return shortValue;

					if (value is decimal decimalValue)
						return (long)decimalValue;

					if (value is double doubleValue)
						return (long)doubleValue;

				}

				return long.Parse(GetJsonStr(varName));
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Failed to get long value:", GetJsonStr(varName), ex);
				return 0L;
			}
		}

		public int GetBodyInt(string varName)
		{
			try
			{
				object value = GetBodyValue(varName);

				if (value != null)
				{
					if (value is int intValue)
						return intValue;

					if (value is short shortValue)
						return shortValue;

					if (value is long longValue)
					{
						if (longValue >= int.MinValue && longValue <= int.MaxValue)
							return (int)longValue;
						else
							throw new OverflowException("Long value is outside the range of Int32");
					}

					if (value is decimal decimalValue)
						return (int)decimalValue;

					if (value is double doubleValue)
						return (int)doubleValue;

				}

				return int.Parse(GetJsonStr(varName));
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Failed to get int value:", GetJsonStr(varName), ex);
				return 0;
			}
		}

		public short GetBodyShort(string varName)
		{
			try
			{
				object value = GetBodyValue(varName);

				if (value != null)
				{
					if (value is short shortValue)
						return shortValue;

					if (value is int intValue)
					{
						if (intValue >= short.MinValue && intValue <= short.MaxValue)
							return (short)intValue;
						else
							throw new OverflowException("Int value is outside the range of Int16");
					}

					if (value is long longValue)
					{
						if (longValue >= short.MinValue && longValue <= short.MaxValue)
							return (short)longValue;
						else
							throw new OverflowException("Long value is outside the range of Int16");
					}

					if (value is decimal decimalValue)
					{
						if (decimalValue >= short.MinValue && decimalValue <= short.MaxValue)
							return (short)decimalValue;
						else
							throw new OverflowException("decimal value is outside the range of Int16");
					}

					if (value is double doubleValue)
					{
						if (doubleValue >= short.MinValue && doubleValue <= short.MaxValue)
							return (short)doubleValue;
						else
							throw new OverflowException("Double value is outside the range of Int16");
					}

				}

				return short.Parse(GetJsonStr(varName));
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Failed to get short value:", GetJsonStr(varName), ex);
				return 0;
			}
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
			string s = string.Empty;
			object value = GetBodyValue(varName);
			if (value != null)
				s = value.ToString();
			return s;
		}

		public T GetBodySdt<T>(string varName) where T : GxUserType, new()
		{
			T sdt = new T();
			if (_responseData.ContainsKey(varName.ToLower()) && _responseData.Count == 1) //wrapped sdt
			{
				sdt.FromJSonString(_responseData[varName.ToLower()].ToString(), null);
			}
			else if (_responseData.Count == 1 && _responseData.ContainsKey(string.Empty)) // unwrapped 
			{
				sdt.FromJSonString(_responseData[string.Empty].ToString(), null);
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

		public GXBaseCollection<T> GetBodySdtCollection<T>(string varName) where T : GxUserType, new()
		{
			GXBaseCollection<T> collection = new GXBaseCollection<T>();
			if (_responseData.ContainsKey(varName.ToLower()))
			{
				collection.FromJSonString(_responseData[varName.ToLower()].ToString(), null);
			}
			else if (_responseData.Count == 1 && _responseData.ContainsKey(string.Empty))
			{
				collection.FromJSonString(_responseData[string.Empty].ToString(), null);
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
			else if (_responseData.Count == 1 && _responseData.ContainsKey(string.Empty))
			{
				collection.FromJSonString(_responseData[string.Empty].ToString(), null);
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
			_queryString = string.Empty;
			if (_queryVars.Count > 0)
			{
				string separator = "?";
				foreach (string key in _queryVars.Keys)
				{
					_queryString += string.Format("{0}{1}={2}", separator, key, _queryVars[key]);
					separator = "&";
				}
			}
			_bodyString = string.Empty;
			if (_bodyVars.Count > 0)
			{
				string separator = string.Empty;
				foreach (string key in _bodyVars.Keys)
				{
					_bodyString += separator + "\"" + key + "\":" + _bodyVars[key];
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
			if (this.Location.AuthenticationMethod == 4 && !string.IsNullOrEmpty(this.Location.AccessToken))
			{
				httpClient.AddHeader("Authorization", this.Location.AccessToken);
			}
			string serviceuri = ((this.Location.Secure > 0) ? "https" : "http") + "://" + this.Location.Host;
			serviceuri += (this.Location.Port != 80) ? ":" + this.Location.Port.ToString() : string.Empty;
			serviceuri += "/" + this.Location.BaseUrl.TrimEnd('/').TrimStart('/') + "/" + this.Location.ResourceName;
			serviceuri += _queryString;
			httpClient.HttpClientExecute(this.HttpMethod, serviceuri);
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
