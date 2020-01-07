using System;
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
using System.Web;

namespace PushSharp.Windows
{
	internal class WindowsMessageTransportAsync
	{
		public event Action<WindowsNotificationStatus> MessageResponseReceived;
		public event Action<WindowsNotification, Exception> UnhandledException;

		string AccessToken { get; set; }
		string TokenType { get; set; }

		static WindowsMessageTransportAsync()
		{
			ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, policyErrs) => { return true; };
		}

		void RenewAccessToken(string packageSecurityIdentifier, string clientSecret)
		{
			var postData = new StringBuilder();

			postData.AppendFormat("{0}={1}&", "grant_type", "client_credentials");
			postData.AppendFormat("{0}={1}&", "client_id", HttpUtility.UrlEncode(packageSecurityIdentifier));
			postData.AppendFormat("{0}={1}&", "client_secret", HttpUtility.UrlEncode(clientSecret));
			postData.AppendFormat("{0}={1}", "scope", "notify.windows.com");

			var wc = new WebClient();
			wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

			var response = string.Empty;

			response = wc.UploadString("https://login.live.com/accesstoken.srf", postData.ToString());
			OAuthToken oauthTok = OAuthToken.GetOAuthTokenFromJson(response);

			if (oauthTok != null)
			{
				this.AccessToken = oauthTok.AccessToken;
				this.TokenType = oauthTok.TokenType;
			}
			else
			{
				throw new UnauthorizedAccessException("Could not retrieve access token for the supplied Package Security Identifier (SID) and client secret");
			}
		}

		// (INotification notification, SendNotificationCallbackDelegate callback)
		public void Send(WindowsNotification winNotification, string packageName, string packageSecurityIdentifier, string clientSecret)
		{
			//See if we need an access token
			if (string.IsNullOrEmpty(AccessToken))
				RenewAccessToken(packageSecurityIdentifier, clientSecret);

			//https://cloud.notify.windows.com/?token=.....
			//Authorization: Bearer {AccessToken}
			//

			string wnsType, contentType, tag;
			Win8Notifications.GetHeaders(winNotification, out wnsType, out contentType, out tag);

			var request = (HttpWebRequest)HttpWebRequest.Create(winNotification.ChannelUri); // "https://notify.windows.com");
			request.Method = "POST";
			request.Headers.Add("X-WNS-Type", wnsType);
			request.Headers.Add("Authorization", string.Format("Bearer {0}", this.AccessToken));
			request.ContentType = contentType;


			if (!string.IsNullOrEmpty(tag))
				request.Headers.Add("X-WNS-Tag", tag);

			//Microsoft recommends we disable expect-100 to improve latency
			request.ServicePoint.Expect100Continue = false;

			var payload = winNotification.GetPayload();
			var data = Encoding.UTF8.GetBytes(payload);

			request.ContentLength = data.Length;

			using (var rs = request.GetRequestStream())
				rs.Write(data, 0, data.Length);

			try
			{
				request.BeginGetResponse(new AsyncCallback(getResponseCallback), new object[] { request, winNotification });
			}
			catch (WebException wex)
			{
				//Handle different httpstatuses
				var status = ParseStatus(wex.Response as HttpWebResponse, winNotification);

				// If token expired, reset AccessToken in order to obtain a new one
				if (status.NotificationStatus == WindowsNotificationSendStatus.TokenExpired)
					AccessToken = null;

				HandleStatus(status);
			}
			catch (Exception ex)
			{
				UnhandledException(winNotification, ex);
			}
		}

		void getResponseCallback(IAsyncResult asyncResult)
		{
			//Good list of statuses:
			//http://msdn.microsoft.com/en-us/library/ff941100(v=vs.92).aspx

			var objs = (object[])asyncResult.AsyncState;

			var wr = (HttpWebRequest)objs[0];
			var winNotification = (WindowsNotification)objs[1];

			var resp = wr.EndGetResponse(asyncResult) as HttpWebResponse;

			var status = ParseStatus(resp, winNotification);

			HandleStatus(status);
		}

		WindowsNotificationStatus ParseStatus(HttpWebResponse resp, WindowsNotification notification)
		{
			var result = new WindowsNotificationStatus();

			result.Notification = notification;
			result.HttpStatus = resp.StatusCode;

			var wnsDebugTrace = resp.Headers["X-WNS-Debug-Trace"];
			var wnsDeviceConnectionStatus = resp.Headers["X-WNS-DeviceConnectionStatus"] ?? "connected";
			var wnsErrorDescription = resp.Headers["X-WNS-Error-Description"];
			var wnsMsgId = resp.Headers["X-WNS-Msg-ID"];
			var wnsNotificationStatus = resp.Headers["X-WNS-NotificationStatus"] ?? "";
			var wnsAuthenticate = resp.Headers["WWW-Authenticate"];

			result.DebugTrace = wnsDebugTrace;
			result.ErrorDescription = wnsErrorDescription;
			result.MessageID = wnsMsgId;

			if (wnsNotificationStatus.Equals("received", StringComparison.InvariantCultureIgnoreCase))
				result.NotificationStatus = WindowsNotificationSendStatus.Received;
			else if (wnsNotificationStatus.Equals("dropped", StringComparison.InvariantCultureIgnoreCase))
				result.NotificationStatus = WindowsNotificationSendStatus.Dropped;
			else if (wnsAuthenticate != null && wnsAuthenticate.Contains("Token expired"))
				result.NotificationStatus = WindowsNotificationSendStatus.TokenExpired;
			else
				result.NotificationStatus = WindowsNotificationSendStatus.ChannelThrottled;

			if (wnsDeviceConnectionStatus.Equals("connected", StringComparison.InvariantCultureIgnoreCase))
				result.DeviceConnectionStatus = WindowsDeviceConnectionStatus.Connected;
			else if (wnsDeviceConnectionStatus.Equals("tempdisconnected", StringComparison.InvariantCultureIgnoreCase))
				result.DeviceConnectionStatus = WindowsDeviceConnectionStatus.TempDisconnected;
			else
				result.DeviceConnectionStatus = WindowsDeviceConnectionStatus.Disconnected;

			return result;
		}

		void HandleStatus(WindowsNotificationStatus status)
		{

			if (MessageResponseReceived == null)
				return;

			//RESPONSE HEADERS
			// X-WNS-Debug-Trace string
			// X-WNS-DeviceConnectionStatus connected | disconnected | tempdisconnected (if RequestForStatus was set to true)
			// X-WNS-Error-Description string
			// X-WNS-Msg-ID string (max 16 char)
			// X-WNS-NotificationStatus received | dropped | channelthrottled
			//

			// 200 OK
			// 400 One or more headers were specified incorrectly or conflict with another header.
			// 401 The cloud service did not present a valid authentication ticket. The OAuth ticket may be invalid.
			// 403 The cloud service is not authorized to send a notification to this URI even though they are authenticated.
			// 404 The channel URI is not valid or is not recognized by WNS. - Raise Expiry
			// 405 Invalid Method - never will get
			// 406 The cloud service exceeded its throttle limit.
			// 410 The channel expired. - Raise Expiry
			// 413 The notification payload exceeds the 5000 byte size limit.
			// 500 An internal failure caused notification delivery to fail.
			// 503 The server is currently unavailable.
			MessageResponseReceived(status);
		}
	}

}
