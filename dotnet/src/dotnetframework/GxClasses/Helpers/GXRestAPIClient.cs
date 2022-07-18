using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Utils;
using GeneXus.Http.Client;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeneXus.Application
{
	public class GXRestAPIClient
	{

		public GXRestAPIClient()
		{
			Location = new GxLocation();
			Location.BaseUrl = "api";
			Location.Host = "www.example.com";
			Location.ResourceName = "service";
			Location.Port = 80;
		}


		private GxHttpClient httpClient = new GxHttpClient();
		public GxLocation Location { get; set; }
		public string Name { get; set; }
		public int ErrorCode { get; set; }
		public string ErrorMessage { get; set; }

		public int ResponseCode { get => responseCode; set => responseCode = value; }
		public string ResponseMessage { get => responseMessage; set => responseMessage = value; }
		public string HttpMethod { get => httpMethod; set => httpMethod = value; }

		public string protocol = "REST";

		private string httpMethod = "GET";

		private Dictionary<string, string> _queryVars = new Dictionary<string, string>();
		private Dictionary<string, string> _bodyVars = new Dictionary<string, string>();
		//private Dictionary<string, string> _pathVars = new Dictionary<string, string>();
		private Dictionary<string,object> _responseData = new Dictionary<string, object>();

		private string _contentType = "application/json; charset=utf-8";
		private string _queryString = "";
		private string _bodyString = "";

		private int responseCode = 0;
		private string responseMessage = "";

		public void AddQueryVar(String varName, String varValue)
		{
			_queryVars[varName] = varValue;
		}

		public void AddQueryVar(String varName, int varValue)
		{
			_queryVars[varName] = varValue.ToString();
		}
		public void AddQueryVar(String varName, short varValue)
		{
			_queryVars[varName] = varValue.ToString();
		}
		public void AddQueryVar(String varName, Decimal varValue)
		{
			_queryVars[varName] = varValue.ToString();
		}
		public void AddQueryVar(String varName, DateTime varValue)
		{
			_queryVars[varName] = varValue.ToString("yyyy-MM-dd");
		}
		public void AddQueryVar(String varName, DateTime varValue, bool hasMilliseconds)
		{
			_queryVars[varName] = varValue.ToString("yyyy-MM-ddTHH:mm:ss");
		}
		public void AddQueryVar(String varName, Guid varValue)
		{
			_queryVars[varName] = varValue.ToString();
		}
		public void AddQueryVar(String varName, bool varValue)
		{
			_queryVars[varName] = varValue.ToString();
		}

		public void AddBodyVar(String varName, GxUserType varValue)
		{
			if (varValue != null)
			{
				AddBodyVar(varName, varValue.ToJSonString());
			}			
		}

		public void AddBodyVar(String varName, IGxCollection varValue)
		{
			if (varValue != null)
			{
				AddBodyVar(varName, varValue.ToJSonString());
			}
		}

		public void AddBodyVar(String varName, DateTime varValue)
		{
			AddBodyVar(varName, varValue, false);
		}

		public void AddBodyVar(String varName, DateTime varValue, bool hasMilliseconds)
		{
			string fmt = "yyyy-MM-ddTHH:mm:ss";
			if (hasMilliseconds)
				fmt = "yyyy-MM-ddTYHH:mm:ss.fff";
			_bodyVars[varName] = varValue.ToString(fmt);
		}

		public void AddBodyVar(String varName, Decimal varValue)
		{
			_bodyVars[varName] = varValue.ToString();
		}

		public void AddBodyVar(String varName, string varValue)
		{
			_bodyVars[varName] = varValue;
		}
		public void AddBodyVar(String varName, int varValue)
		{
			_bodyVars[varName] = varValue.ToString();
		}

		public void AddBodyVar(String varName, bool varValue)
		{
			_bodyVars[varName] = varValue.ToString();
		}
		public void AddBodyVar(String varName, Guid varValue)
		{
			_bodyVars[varName] = varValue.ToString();
		}

		public string GetBodyString(string varName)
		{
			return  GetJsonStr(varName);			
		}

		public DateTime GetBodyDate(string varName)
		{	
		 	return DateTime.ParseExact(GetJsonStr(varName), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
		}

		public DateTime GetBodyDateTime(string varName, bool hasMilliseconds)
		{
			string fmt = "yyyy-MM-ddTHH:mm:ss";
			if (hasMilliseconds)
				fmt += ".fff";
			return DateTime.ParseExact(GetJsonStr(varName), fmt,System.Globalization.CultureInfo.InvariantCulture);
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
			return Decimal.Parse( GetJsonStr(varName));
		}

		public int GetBodyInt(string varName)
		{
			return Int32.Parse(GetJsonStr(varName));
		}

		public string GetJsonStr(string varName)
		{
			string s = "";
			if (_responseData.ContainsKey(varName.ToLower()))
			{
				s = _responseData[varName.ToLower()].ToString();
			}
			else if (_responseData.Count == 1 && _responseData.ContainsKey(""))
			{
				s = _responseData[""].ToString();
			}
			return s;	
		}
		
		public T GetBodySdt<T>(string varName) where T:GxUserType, new()
		{
			T sdt = new T();
			if (_responseData.ContainsKey(varName.ToLower()))
			{
				sdt.FromJSonString(_responseData[varName.ToLower()].ToString(), null);
			}
			else if (_responseData.Count == 1 && _responseData.ContainsKey(""))
			{
				sdt.FromJSonString(_responseData[""].ToString(), null);
			}
			else if (_responseData.Count >= 1 && !_responseData.ContainsKey(varName.ToLower()))
			{
				sdt.FromJSonString(JsonSerializer.Serialize(_responseData), null);
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
			else if (_responseData.Count == 1 && _responseData.ContainsKey(""))
			{
				collection.FromJSonString(_responseData[""].ToString(), null);
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
			else if (_responseData.Count == 1 && _responseData.ContainsKey(""))
			{
				collection.FromJSonString(_responseData[""].ToString(), null);
			}
			return collection;
		}

		public void RestExecute()
		{
			//  System.Net.ServicePointManager.SecurityProtocol = System.Net.ServicePointManager.SecurityProtocol | System.Net.SecurityProtocolType.Tls12;
			//  timeout RequestSecondsTimeout			
			_queryString = "";
			if (_queryVars.Count > 0)
			{
				string separator = "?";
				foreach (string key in _queryVars.Keys)
				{
					_queryString += string.Format("{0}{1}={2}", separator, key, _queryVars[key]);
					separator = "&";
				}
			}
			_bodyString = "";
			if (_bodyVars.Count > 0)
			{
				string separator = "";
				foreach (string key in _bodyVars.Keys)
				{
					_bodyString +=  separator + "\"" + key + "\":\"" + _bodyVars[key] + "\"";
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

			string serviceuri = ((this.Location.Secure > 0) ? "https" : "http") + "://" + this.Location.Host;
			serviceuri += (this.Location.Port != 80) ? ":" + this.Location.Port.ToString() : "";
			serviceuri += "/" + this.Location.BaseUrl.TrimEnd('/').TrimStart('/') + "/" + this.Location.ResourceName;
			serviceuri += _queryString;			
			httpClient.HttpClientExecute( this.HttpMethod, serviceuri);
			if (httpClient.StatusCode >= 300 || httpClient.ErrCode > 0)
			{
				this.ErrorCode = httpClient.ErrCode;
				this.ErrorMessage = httpClient.ErrDescription;
				_responseData = new Dictionary<string, object>();
			}
			else
			{
				_responseData = GeneXus.Utils.RestAPIHelpers.ReadRestBodyParameters(httpClient.ReceiveStream);
			}
			
		}
	}
}
