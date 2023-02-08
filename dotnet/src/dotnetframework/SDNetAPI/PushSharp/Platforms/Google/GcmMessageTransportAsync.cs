﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Net.Security;
using Jayrock.Json;
using Artech.Genexus.SDAPI;
using PushSharp.Common;

namespace PushSharp.Android
{
	internal class GcmMessageTransportAsync
	{
		public event Action<string> UpdateGoogleClientAuthToken;
		public event Action<GcmMessageTransportResponse> MessageResponseReceived;
		public event Action<GcmNotification, Exception> UnhandledException;

		static GcmMessageTransportAsync()
		{
			ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, policyErrs) => { return true; };
		}

		private const string GCM_SEND_URL = "https://android.googleapis.com/gcm/send";

		public void Send(GcmNotification msg, string senderAuthToken, string senderID, string applicationID)
		{
			try
			{
				send(msg, senderAuthToken, senderID, applicationID);
			}
			catch (Exception ex)
			{
				if (UnhandledException != null)
					UnhandledException(msg, ex);
				else
					throw ex;
			}
		}

		void send(GcmNotification msg, string senderAuthToken, string senderID, string applicationID)
		{
			var result = new GcmMessageTransportResponse();
			result.Message = msg;
						
			var postData = msg.GetJson();

			var webReq = (HttpWebRequest)WebRequest.Create(GCM_SEND_URL);
			//webReq.ContentLength = postData.Length;
			webReq.Method = "POST";
			webReq.ContentType = "application/json";
			//webReq.ContentType = "application/x-www-form-urlencoded;charset=UTF-8   can be used for plaintext bodies
			webReq.UserAgent = "PushSharp (version: 1.0)";
			webReq.Headers.Add("Authorization: key=" + senderAuthToken);

			webReq.BeginGetRequestStream(new AsyncCallback(requestStreamCallback), new GcmAsyncParameters()
			{
				WebRequest = webReq,
				WebResponse = null,
				Message = msg,
				SenderAuthToken = senderAuthToken,
				SenderId = senderID,
				ApplicationId = applicationID
			});
		}

		void requestStreamCallback(IAsyncResult result)
		{
			var msg = new GcmNotification();

			try
			{
				var asyncParam = result.AsyncState as GcmAsyncParameters;
				msg = asyncParam.Message;

				if (asyncParam != null)
				{
					var wrStream = asyncParam.WebRequest.EndGetRequestStream(result);

					using (var webReqStream = new StreamWriter(wrStream))
					{
						var data = asyncParam.Message.GetJson();
						webReqStream.Write(data);
						webReqStream.Close();
					}

					try
					{
						asyncParam.WebRequest.BeginGetResponse(new AsyncCallback(responseCallback), asyncParam);
					}
					catch (WebException wex)
					{
						asyncParam.WebResponse = wex.Response as HttpWebResponse;
						processResponseError(asyncParam);
					}
				}
			}
			catch (Exception ex)
			{
				if (UnhandledException != null)
					UnhandledException(msg, ex);
				else
					throw ex;
			}
		}

		void responseCallback(IAsyncResult result)
		{
			var msg = new GcmNotification();

			try
			{
				var asyncParam = result.AsyncState as GcmAsyncParameters;
				msg = asyncParam.Message;

				try
				{
					asyncParam.WebResponse = asyncParam.WebRequest.EndGetResponse(result) as HttpWebResponse;
					processResponseOk(asyncParam);
				}
				catch (WebException wex)
				{
					asyncParam.WebResponse = wex.Response as HttpWebResponse;
					processResponseError(asyncParam);
				}
			}
			catch (Exception ex)
			{
				if (UnhandledException != null)
					UnhandledException(msg, ex);
				else
					throw ex;
			}
		}

		void processResponseOk(GcmAsyncParameters asyncParam)
		{
			var result = new GcmMessageTransportResponse()
			{
				ResponseCode = GcmMessageTransportResponseCode.Ok,
				Message = asyncParam.Message
			};

			var updateClientAuth = asyncParam.WebResponse.GetResponseHeader("Update-Client-Auth");

			if (!string.IsNullOrEmpty(updateClientAuth) && UpdateGoogleClientAuthToken != null)
				UpdateGoogleClientAuthToken(updateClientAuth);

			//Get the response body
			var json = new JObject();

            try { json = Utils.FromJSonString((new StreamReader(asyncParam.WebResponse.GetResponseStream())).ReadToEnd()); }
			catch { }

			long jsonResultLong = 0;
			long.TryParse(json["canonical_ids"].ToString(), out jsonResultLong);
			result.NumberOfCanonicalIds = jsonResultLong;
			jsonResultLong = 0;
			long.TryParse(json["failure"].ToString(), out jsonResultLong);
			result.NumberOfFailures = jsonResultLong;
			jsonResultLong = 0;
			long.TryParse(json["success"].ToString(), out jsonResultLong);
			result.NumberOfSuccesses = jsonResultLong;
			
			var jsonResults = json["results"] as JArray;

			if (jsonResults == null)
				jsonResults = new JArray();

			foreach (JObject r in (JArray)json["results"])
			{
				var msgResult = new GcmMessageResult();
								                
				msgResult.MessageId = (string)r["message_id"];
				msgResult.CanonicalRegistrationId = (string)r["registration_id"];

				if (!string.IsNullOrEmpty(msgResult.CanonicalRegistrationId))
				{
					msgResult.ResponseStatus = GcmMessageTransportResponseStatus.CanonicalRegistrationId;
				}
				else if (r["error"] != null)
				{
					var err = (string)r["error"] ?? "";

					switch (err.ToLower().Trim())
					{
						case "missingregistration":
							msgResult.ResponseStatus = GcmMessageTransportResponseStatus.MissingRegistrationId;
							break;
						case "unavailable":
							msgResult.ResponseStatus = GcmMessageTransportResponseStatus.Unavailable;
							break;
						case "notregistered":
							msgResult.ResponseStatus = GcmMessageTransportResponseStatus.NotRegistered;
							break;
						case "invalidregistration":
							msgResult.ResponseStatus = GcmMessageTransportResponseStatus.InvalidRegistration;
							break;
						case "mismatchsenderid":
							msgResult.ResponseStatus = GcmMessageTransportResponseStatus.MismatchSenderId;
							break;
						case "messagetoobig":
							msgResult.ResponseStatus = GcmMessageTransportResponseStatus.MessageTooBig;
							break;
						default:
							msgResult.ResponseStatus = GcmMessageTransportResponseStatus.Error;
							break;
					}
				}

				result.Results.Add(msgResult);
			}

			asyncParam.WebResponse.Close();

			var evtmrr = MessageResponseReceived;

			if (evtmrr != null)
				evtmrr(result);
		}

		void processResponseError(GcmAsyncParameters asyncParam)
		{
			var result = new GcmMessageTransportResponse();
			result.ResponseCode = GcmMessageTransportResponseCode.Error;

			if (asyncParam.WebResponse.StatusCode == HttpStatusCode.Unauthorized)
			{
				//401 bad auth token
				result.ResponseCode = GcmMessageTransportResponseCode.InvalidAuthToken;
				throw new GcmAuthenticationErrorTransportException(result);
			}
			else if (asyncParam.WebResponse.StatusCode == HttpStatusCode.BadRequest)
			{
				result.ResponseCode = GcmMessageTransportResponseCode.BadRequest;
				throw new GcmBadRequestTransportException(result);
			}
			else if (asyncParam.WebResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
			{
				//First try grabbing the retry-after header and parsing it.
				TimeSpan retryAfter = new TimeSpan(0, 0, 120);

				var wrRetryAfter = asyncParam.WebResponse.GetResponseHeader("Retry-After");

				if (!string.IsNullOrEmpty(wrRetryAfter))
				{
					DateTime wrRetryAfterDate = DateTime.UtcNow;

					if (DateTime.TryParse(wrRetryAfter, out wrRetryAfterDate))
						retryAfter = wrRetryAfterDate - DateTime.UtcNow;
					else
					{
						int wrRetryAfterSeconds = 120;
						if (int.TryParse(wrRetryAfter, out wrRetryAfterSeconds))
							retryAfter = new TimeSpan(0, 0, wrRetryAfterSeconds);
					}
				}

				//503 exponential backoff, get retry-after header
				result.ResponseCode = GcmMessageTransportResponseCode.ServiceUnavailable;
			
				throw new GcmServiceUnavailableTransportException(retryAfter, result);
			}

			asyncParam.WebResponse.Close();

			throw new GcmMessageTransportException("Unknown Transport Error", result);
		}

		class GcmAsyncParameters
		{
			public GcmNotification Message
			{
				get;
				set;
			}

			public HttpWebRequest WebRequest
			{
				get;
				set;
			}

			public HttpWebResponse WebResponse
			{
				get;
				set;
			}

			public string SenderAuthToken
			{
				get;
				set;
			}

			public string SenderId
			{
				get;
				set;
			}

			public string ApplicationId
			{
				get;
				set;
			}
		}
	}

	public class GcmMessageResult
	{
		public string MessageId { get; set; }

		public string CanonicalRegistrationId {	get; set; }

		public GcmMessageTransportResponseStatus ResponseStatus { get; set; }
	}
}
