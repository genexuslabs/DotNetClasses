using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using Jayrock.Json;
using PushSharp.Windows;

namespace Artech.Genexus.SDAPI
{
	internal class Win8Notifications
	{

		const int retries = 3;
		OAuthToken accessToken = null;

		static Win8Notifications currentInstance;

		const string TEXT_TEMPLATE = "<text id=\"{0}\">{1}</text>";
		const string IMAGE_TEMPLATE = "<image id=\"1\" src=\"{0}\"/>";
		const string AUDIO_TEMPLATE = "<audio src=\"{0}\"/>";
		const string BINDING_TEMPLATE = "<binding template=\"{0}\">{1}</binding>";

		const string MEDIUM_TILE_NAME = "TileSquare150x150";
		const string WIDE_TILE_NAME = "TileWide310x150";
		const string LARGE_TILE_NAME = "TileSquare310x310";

		internal static bool Send(string secret, string sid, string uri, string action, string text, NotificationParameters props, out string log)
		{
			WindowsNotification notif = new WindowsNotification { Action = action, Message = text, Parameters = props };

			string xml = GetMessage(notif);
			log = "";
			string notificationType = "wns/toast";
			string contentType = "text/xml";
			string tag = props.ValueOf("tag");

			if (action == "toast" || String.IsNullOrEmpty(action))
			{
				notificationType = "wns/toast";
				contentType = "text/xml";
			}
			else if (action == "badge")
			{
				notificationType = "wns/badge";
			}
			else if (action == "tile")
			{
				notificationType = "wns/tile";
			}
			else if (action == "raw")
			{
				notificationType = "wns/raw";
				contentType = "application/octet-stream";
			}
			return new Win8Notifications().SendToWns(secret, sid, uri, xml, notificationType, contentType, tag, retries, ref log);
		}

		public static string GetMessage(WindowsNotification notif)
		{
			if (!string.IsNullOrEmpty(notif.Badge))
				return GetBadgeTemplate(notif);

			if (string.IsNullOrEmpty(notif.Title) && string.IsNullOrEmpty(notif.Message) && !string.IsNullOrEmpty(notif.Action))
				return GetRawAction(notif);

			if (string.IsNullOrEmpty(notif.Action) && notif.Parameters.Count == 1 && notif.Parameters.Names[0] == "tile")
				return GetTileTemplate(notif);

			return GetToastTemplate(notif);
		}

		public static void GetHeaders(WindowsNotification notif, out string notificationType, out string contentType, out string tag)
		{
			notificationType = "wns/toast";
			contentType = "text/xml";
			tag = "";

			if (!string.IsNullOrEmpty(notif.Badge))
				notificationType = "wns/badge";

			if (string.IsNullOrEmpty(notif.Title) && string.IsNullOrEmpty(notif.Message) && !string.IsNullOrEmpty(notif.Action))
			{
				notificationType = "wns/raw";
				contentType = "application/octet-stream";
			}

			if (string.IsNullOrEmpty(notif.Action) && notif.Parameters.Count == 1 && notif.Parameters.Names[0] == "tile")
				notificationType = "wns/tile";

			if (currentInstance == null)
				currentInstance = new Win8Notifications();

		}

		static string GetToastTemplate(WindowsNotification notif)
		{
			bool hasImage;
			bool hasTitle;
			int numLines;
			string templateContent = GetNotifContent(notif, out hasImage, out hasTitle, out numLines);

			int template = 0;
			if (numLines == 1)
				template = !hasTitle ? 1 : 2;
			else if (numLines == 2 || numLines == 3)
				template = 4;
			else
				throw new Exception("Not supported");

			string templateName = string.Format("Toast{0}Text0{1}", hasImage ? "ImageAnd" : "", template);

			string sound = "<audio silent=\"true\"/>";
			if (!string.IsNullOrEmpty(notif.Sound))
				sound = notif.Sound.ToLower() == "default" ? "" : string.Format(AUDIO_TEMPLATE, notif.Sound);

			string launchTemplate = " launch=\"{0}\"";
			string launch = "";
			if (!string.IsNullOrEmpty(notif.Action))
				launch = string.Format(launchTemplate, HttpUtility.HtmlEncode(GetRawAction(notif)));

			string binding = string.Format(BINDING_TEMPLATE, templateName, templateContent);

			return string.Format("<toast{0}><visual>{1}</visual>{2}</toast>", launch, binding, sound);
		}

		static string GetNotifContent(WindowsNotification notif, out bool hasImage, out bool hasTitle, out int numlines)
		{
			hasImage = false;
			numlines = 1;
			hasTitle = false;

			string imageContent = string.IsNullOrEmpty(notif.ImageUri) ? "" : string.Format(IMAGE_TEMPLATE, notif.ImageUri);
			hasImage = !string.IsNullOrEmpty(imageContent);

			string textContent = "";
			int currentLine = 1;

			hasTitle = false;
			if (!string.IsNullOrEmpty(notif.Title))
			{
				textContent = string.Format(TEXT_TEMPLATE, currentLine++, notif.Title);
				hasTitle = true;
			}

			string[] lines = notif.Message.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			numlines = lines.Length;

			foreach (string line in lines)
				textContent += string.Format(TEXT_TEMPLATE, currentLine++, line);

			return imageContent + textContent;
		}

		static string GetRawAction(WindowsNotification notif)
		{
			JObject js = new JObject();
			js.Put("action", notif.Action);
			js.Put("executionTime", notif.ExecutionTime);
			js.Put("params", notif.Parameters.ToJObject());

			return js.ToString();
		}

		static string GetTileTemplate(WindowsNotification notif)
		{
			string medium = GetMediumTile(notif);
			string wide = GetWideTile(notif);
			string large = GetLargeTile(notif);

			return string.Format("<tile><visual version=\"2\">{0}{1}{2}</visual></tile>", medium, wide, large);
		}

		static string GetMediumTile(WindowsNotification notif)
		{
			bool hasImage;
			bool hasTitle;
			int numLines;
			string content = GetNotifContent(notif, out hasImage, out hasTitle, out numLines);

			string nameTemplate = MEDIUM_TILE_NAME + "{0}Text0{1}";

			int template = 0;
			if (numLines == 1)
				template = hasTitle ? 2 : 4;
			else if (numLines == 2 || numLines == 3)
				template = 3;
			else
				return "";

			string name = string.Format(nameTemplate, hasImage ? "PeekImageAnd" : "", template);
			if (!hasTitle && numLines == 0 && hasImage)
				name = MEDIUM_TILE_NAME + "Image";

			return string.Format(BINDING_TEMPLATE, name, content);
		}

		static string GetWideTile(WindowsNotification notif)
		{
			bool hasImage;
			bool hasTitle;
			int numLines;
			string content = GetNotifContent(notif, out hasImage, out hasTitle, out numLines);

			string nameTemplate = WIDE_TILE_NAME + "{0}0{1}";
			string extra = "Text";

			int template = 0;
			if (numLines == 0 && hasTitle)
			{
				template = 3;
				if (hasImage)
					extra = "PeekImage";
			}
			else if (numLines == 1)
			{
				template = hasTitle ? 9 : 4;
				if (hasImage)
				{
					template = 1;
					extra = hasTitle ? "PeekImage" : "PeekImageAndText";
				}
			}
			else if (numLines == 2 || numLines == 3 || numLines == 4)
			{
				template = hasTitle ? 1 : 5;
				if (hasImage)
				{
					template = 2;
					extra = hasTitle ? "PeekImage" : "PeekImageAndText";
				}
			}
			else
				return "";

			string name = string.Format(nameTemplate, extra, template);
			if (!hasTitle && numLines == 0 && hasImage)
				name = WIDE_TILE_NAME + "Image";

			return string.Format(BINDING_TEMPLATE, name, content);
		}

		private static string GetLargeTile(WindowsNotification notif)
		{
			bool hasImage;
			bool hasTitle;
			int numLines;
			string content = GetNotifContent(notif, out hasImage, out hasTitle, out numLines);

			string nameTemplate = LARGE_TILE_NAME + "{0}0{1}";
			string extra = "Text";

			int template = 0;
			if (numLines == 0 && hasTitle & hasImage)
			{
				template = 1;
				extra = "ImageAndTextOverlay";
			}
			else if (numLines == 1)
			{
				template = hasTitle ? 1 : 3;
				if (hasImage)
				{
					template = hasTitle ? 2 : 1;
					extra = "ImageAndTextOverlay";
				}
			}
			else if (numLines == 2 || numLines == 3 || numLines == 4)
			{
				template = hasTitle ? 1 : 3;
				if (hasImage)
				{
					template = hasTitle? 3 : 2;
					extra = hasTitle ? "ImageAndTextOverlay" : "ImageAndText";
				}
			}
			else
				return "";

			string name = string.Format(nameTemplate, extra, template);
			if (!hasTitle && numLines == 0 && hasImage)
				name = LARGE_TILE_NAME + "Image";

			return string.Format(BINDING_TEMPLATE, name, content);
		}

		static string GetBadgeTemplate(WindowsNotification notif)
		{
			return "<badge value=\"" + notif.Badge + "\"/>";
		}

		bool SendToWns(string secret, string sid, string uri, string xml, string notificationType, string contentType, string tag, int retry, ref string log)
		{
			try
			{
				if (this.accessToken == null)
				{
					this.accessToken = GetAccessToken(secret, sid);
				}

				byte[] contentInBytes = Encoding.UTF8.GetBytes(xml);

				HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
				request.Method = "POST";
				request.Headers.Add("X-WNS-Type", notificationType);
				request.ContentType = contentType;
				if (!String.IsNullOrEmpty(tag))
					request.Headers.Add("X-WNS-Tag", tag);
				request.Headers.Add("Authorization", String.Format("Bearer {0}", accessToken.AccessToken));

				using (Stream requestStream = request.GetRequestStream())
					requestStream.Write(contentInBytes, 0, contentInBytes.Length);

				using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
				{
					log = webResponse.StatusCode.ToString();
					return true;
				}
			}
			catch (WebException webException)
			{
				string exceptionDetails = webException.Response.Headers["WWW-Authenticate"];
				if (exceptionDetails != null && exceptionDetails.Contains("Token expired"))
				{
					this.accessToken = GetAccessToken(secret, sid);
					retry--;
					if (retry > 0)
					{
						return SendToWns(secret, sid, uri, xml, notificationType, contentType, tag, retry, ref log);
					}
					else
					{
						log += "EXCEPTION: Token Expired";
						return false;
					}

				}
				else
				{
					log += "EXCEPTION: " + webException.Message;
					return false;
				}
			}
			catch (Exception ex)
			{
				log += "EXCEPTION: " + ex.Message;
				return false;
			}
		}

		private OAuthToken GetAccessToken(string secret, string sid)
		{
			var urlEncodedSecret = HttpUtility.UrlEncode(secret);
			var urlEncodedSid = HttpUtility.UrlEncode(sid);

			var body = String.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=notify.windows.com",
									 urlEncodedSid,
									 urlEncodedSecret);

			string response;
			using (var client = new WebClient())
			{
				client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
				response = client.UploadString("https://login.live.com/accesstoken.srf", body);
			}
			return OAuthToken.GetOAuthTokenFromJson(response);
		}
	}
	// Authorization
	[DataContract]
	public class OAuthToken
	{
		[DataMember(Name = "access_token")]
		public string AccessToken { get; set; }
		[DataMember(Name = "token_type")]
		public string TokenType { get; set; }

		static public OAuthToken GetOAuthTokenFromJson(string jsonString)
		{
			using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonString)))
			{
				var ser = new DataContractJsonSerializer(typeof(OAuthToken));
				var oAuthToken = (OAuthToken)ser.ReadObject(ms);
				return oAuthToken;
			}
		}

	}

}