using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
using PushSharp;
using GeneXus.Utils;
#if !NETCOREAPP1_1
using PushSharp.Apple;
using PushSharp.Common;
using PushSharp.Android;
using PushSharp.Windows;
#endif
using System.Security.Cryptography;
using log4net;

namespace Artech.Genexus.SDAPI
{
    public sealed class Notifications
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(Artech.Genexus.SDAPI.Notifications));

        private const string IOS_HOST_SANDBOX = "gateway.sandbox.push.apple.com";
        private const string IOS_HOST_PROD = "gateway.push.apple.com";


        public const short IOS     = 0;
        public const short ANDROID = 1;
        public const short BB      = 2;
        public const short WPHONE  = 3;

		/// <summary>
		/// / error Codes
		/// </summary>
		private const short NO_ERROR = 0;
		private const short INVALID_DEVICE_TYPE = -1;
		private const short CUSTOM_ERROR = 3;


#if !NETCOREAPP1_1
		private int m_error = 0;
#endif
		private string m_customError = string.Empty;
#if !NETCOREAPP1_1
		private int SendInternal(string applicationId, short deviceType, string deviceToken, string alertMessage, string action, NotificationParameters props)
        {
            switch (deviceType)
            { 
                case IOS:
                    m_error = SendIOS(applicationId, deviceToken, alertMessage, action, props);
                    break;
                case ANDROID:
                    m_error = SendAndroid(applicationId, deviceToken, alertMessage, action, props);
                    break;
				case WPHONE:
					m_error = SendWin8(applicationId, deviceToken, alertMessage, action, props);
					break;
				default:
                    m_error = -1;
                    break;
            }
            
            return ErrorCode;
        }

        public int IOSSetBadge(string applicationId, string deviceToken, int badgeNumber, string sound)
        {
            if (string.IsNullOrEmpty(sound))
                sound = "default";
			ConfigurationProps cert = Certificates.Instance.PropertiesFor(applicationId);
            string payload = string.Format(PAYLOAD_BADGE_FORMAT, badgeNumber, sound);
            return IOSNotifications.Send(deviceToken, payload, cert.iOSuseSandboxServer ? IOS_HOST_SANDBOX : IOS_HOST_PROD, cert.iOScertificate, cert.iOScertificatePassword, out m_customError) ? 0 : 3;
        }

        public int IOSResetBadge(string applicationId, string deviceToken)
        {
			ConfigurationProps cert = Certificates.Instance.PropertiesFor(applicationId);
            return IOSNotifications.Send(deviceToken, PAYLOAD_RESET, cert.iOSuseSandboxServer ? IOS_HOST_SANDBOX : IOS_HOST_PROD, cert.iOScertificate, cert.iOScertificatePassword, out m_customError) ? 0 : 3;
        }

        public int Call(string applicationId, short deviceType, string deviceToken, string alertMessage)
        {
            return SendInternal(applicationId, deviceType, deviceToken, alertMessage, string.Empty, null);
        }

        public int CallAction(string applicationId, short deviceType, string deviceToken, string alertMessage, string action)
        {
            return CallAction(applicationId, deviceType, deviceToken, alertMessage, action, null);
        }

        public int CallAction(string applicationId, short deviceType, string deviceToken, string alertMessage, string action, NotificationParameters props)
        {            
            return SendInternal(applicationId, deviceType, deviceToken, alertMessage, action, props);
        }

        #region GX Error Handling

        public int ErrorCode { get { return m_error; } }

		private string GetErrorDescription(int errorCode)
		{
			switch (errorCode)
			{
				case 0:
					return "Ok";
				case -1:
					return "Unknown device type";
				case 2:
					return "Invalir Application ID";
				case 1:
					return "notifications.json not found";
				default:
					return string.Empty;
			}
		}

        public string ErrorDescription
        {
            get
            {
				if (m_error == CUSTOM_ERROR)
					return m_customError;

				string errDesc = GetErrorDescription(m_error);
				if (!string.IsNullOrEmpty(errDesc))
					return errDesc;

                Debug.Assert(false, "Unknown error code");
                return string.Empty;
            }
        }

        #endregion

        private const string PAYLOAD_FORMAT = "{{\"aps\":{{\"alert\":\"{0}\"}},\"m\":\"{1}\",\"a\":\"{2}\",\"t\":\"{3}\",\"p\":{4}}}";
        private const string PAYLOAD_BADGE_FORMAT = "{{\"aps\":{{\"badge\":{0},\"sound\":\"{1}\"}}}}";
        private const string PAYLOAD_RESET = "{{\"aps\":{{}}}}";

        private int SendIOS(string applicationId, string deviceToken, string alert, string action, NotificationParameters props)
        {
            if (props == null)
                props = new NotificationParameters();

			ConfigurationProps cert = Certificates.Instance.PropertiesFor(applicationId);
            string payload = string.Format(PAYLOAD_FORMAT, alert, applicationId, action, Certificates.Instance.TypeFor(applicationId), props.ToJson());
            return IOSNotifications.Send(deviceToken, payload, cert.iOSuseSandboxServer ? IOS_HOST_SANDBOX : IOS_HOST_PROD, cert.iOScertificate, cert.iOScertificatePassword, out m_customError) ? 0 : 3;
        }

        private int SendAndroid(string applicationId, string deviceToken, string alert, string action, NotificationParameters props)
        {
            if (props == null)
                props = new NotificationParameters();

			ConfigurationProps cert = Certificates.Instance.PropertiesFor(applicationId);

            // new AndroidUserToken for gcm getting from config file now
	        String AndroidUserToken = cert.androidSenderAPIKey;
        
			//TODO: Add parameters to standart call action?
            return AndroidNotifications.sendMessage(AndroidUserToken, deviceToken, alert, action, props, out m_customError) ? 0 : 3;
        }

		private int SendWin8(string applicationId, string deviceToken, string alert, string action, NotificationParameters props)
		{
			if (props == null)
				props = new NotificationParameters();

			ConfigurationProps securityInfo = Certificates.Instance.PropertiesFor(applicationId);

			return Win8Notifications.Send(securityInfo.WNSClientSecret, securityInfo.WNSPackageSecurityIdentifier, deviceToken, action, alert, props, out m_customError) ? 0 : 3;
		}


		/////////////////// U3 API
		#region XEV2U3 API

		private string m_appId;
		private ConfigurationProps m_config = null;
		private PushService m_service = null;
		private List<RemoteNotification> m_notifications = new List<RemoteNotification>();
		private List<RemoteNotificationResult> m_results = new List<RemoteNotificationResult>();
		private bool m_started_ios = false;
		private bool m_started_android = false;
		private bool m_started_w8 = false;

		private PushServiceSettings serviceSettings = new PushServiceSettings();

		private void EnsureiOS()
		{
			if (!m_started_ios)
			{
				ApplePushChannelSettings setting = new ApplePushChannelSettings(!m_config.iOSuseSandboxServer, m_config.iOScertificate, m_config.iOScertificatePassword);
				Service.StartApplePushService(setting, serviceSettings);
				m_started_ios = true;
			}
		}

		private void EnsureAndroid()
		{
			if (!m_started_android)
			{
				GcmPushChannelSettings settings = new GcmPushChannelSettings(m_config.androidSenderId, m_config.androidSenderAPIKey, m_appId);
				Service.StartGooglePushService(settings, serviceSettings);
				m_started_android = true;
			}
		}

		private void EnsureWindows()
		{
			if (!m_started_w8)
			{
				WindowsPushChannelSettings settings = new WindowsPushChannelSettings(m_appId, m_config.WNSPackageSecurityIdentifier, m_config.WNSClientSecret);
				Service.StartWindowsPushService(settings, serviceSettings);
				m_started_w8 = true;
			}
		}

		private PushService Service
		{
			get
			{
				if (m_service == null)
				{
					m_service = new PushService();
					SubscribeServiceEvents(m_service);
				}
				return m_service;
			}
		}

		private void CleanupSession()
		{
			UnsubscribeServiceEvents(m_service);
			m_service = null;
			m_started_android = false;
			m_started_ios = false;
			m_started_w8 = false;
		}

		private void SetResult(RemoteNotificationResult result)
		{
			lock (m_results)
				m_results.Add(result);
		}

        private void SetResult(int errorCode, PushSharp.Common.Notification notification, Exception ex)
        {
            RemoteNotificationResult result = RemoteNotificationResult.FromNotification(notification);
            result.ErrorCode = errorCode;
            result.ErrorDescription = GetErrorDescription(errorCode);
            if (ex != null && String.IsNullOrEmpty(result.ErrorDescription))
            {
                if (ex is NotificationFailureException)
                {
                    result.ErrorDescription = ((NotificationFailureException)ex).ErrorStatusDescription;
                }
                else
                {
                    result.ErrorDescription = ex.Message;
                }
                log.Error(String.Format("SetResult - ErrCode {0} - {1}", errorCode, result.ErrorDescription), ex);
            }
            SetResult(result);
        }

		#region Events

		private void SubscribeServiceEvents(PushService service)
		{
			service.Events.OnDeviceSubscriptionExpired += new PushSharp.Common.ChannelEvents.DeviceSubscriptionExpired(Events_OnDeviceSubscriptionExpired);
			service.Events.OnDeviceSubscriptionIdChanged += new PushSharp.Common.ChannelEvents.DeviceSubscriptionIdChanged(Events_OnDeviceSubscriptionIdChanged);
			service.Events.OnNotificationSendFailure += new PushSharp.Common.ChannelEvents.NotificationSendFailureDelegate(Events_OnNotificationSendFailure);
			service.Events.OnNotificationSent += new PushSharp.Common.ChannelEvents.NotificationSentDelegate(Events_OnNotificationSent);
			service.Events.OnChannelException += new PushSharp.Common.ChannelEvents.ChannelExceptionDelegate(Events_OnChannelException);
		}

		private void UnsubscribeServiceEvents(PushService service)
		{
			service.Events.OnDeviceSubscriptionExpired -= Events_OnDeviceSubscriptionExpired;
			service.Events.OnDeviceSubscriptionIdChanged -= Events_OnDeviceSubscriptionIdChanged;
			service.Events.OnNotificationSendFailure -= Events_OnNotificationSendFailure;
			service.Events.OnNotificationSent -= Events_OnNotificationSent;
			service.Events.OnChannelException -= Events_OnChannelException;
		}

		void Events_OnNotificationSent(PushSharp.Common.Notification notification)
		{
			SetResult(NO_ERROR,notification, null);
		}

		void Events_OnNotificationSendFailure(PushSharp.Common.Notification notification, Exception notificationFailureException)
		{
			SetResult(CUSTOM_ERROR, notification, notificationFailureException);
		}

		void Events_OnDeviceSubscriptionIdChanged(PushSharp.Common.PlatformType platform, string oldDeviceInfo, string newDeviceInfo, PushSharp.Common.Notification notification)
		{
			SetResult(CUSTOM_ERROR, notification, new Exception("DeviceSubscriptionIdChanged - old:" + oldDeviceInfo + " new: " + newDeviceInfo));
		}

		void Events_OnDeviceSubscriptionExpired(PushSharp.Common.PlatformType platform, string deviceInfo, PushSharp.Common.Notification notification)
		{
			SetResult(CUSTOM_ERROR, notification, new Exception("DeviceSubscriptionExpired - " + deviceInfo));
		}

		void Events_OnChannelException(Exception notificationFailureException, PushSharp.Common.Notification notification)
		{
			SetResult(CUSTOM_ERROR, notification, notificationFailureException);
		}

		#endregion

		public int OpenSession(string appId)
		{
			m_notifications.Clear();
			m_results.Clear();
			m_config = Certificates.Instance.PropertiesFor(appId);
			m_appId = appId;
			m_error = Certificates.Instance.ErrorCode;
			return m_error;
		}

		public void Add(GxUserType sdt)
		{
			RemoteNotification notification = RemoteNotification.FromGxUserType(sdt);
			Add(notification);
		}

		public void Add(RemoteNotification notification)
		{
			m_notifications.Add(notification);
		}

		public List<RemoteNotificationResult> Send()
		{
			foreach (RemoteNotification notif in m_notifications)
			{
				PlatformType platformType = PlatformConverter.FromDeviceType(notif.DeviceType);
				switch (platformType)
				{ 
					case PlatformType.Apple:
						EnsureiOS();
						AppleNotification n = NotificationFactory.Apple();
						n.DeviceToken = notif.DeviceToken;
						n.Payload = new AppleNotificationPayload();
						if (!string.IsNullOrEmpty(notif.Message))
							n.Payload.Alert = new AppleNotificationAlert() { Body = notif.Message };
						if (!string.IsNullOrEmpty(notif.Sound))
							n.Payload.Sound = notif.Sound;
						if (!string.IsNullOrEmpty(notif.Action) && notif.Action.Trim().Length > 0)
						{
							n.Payload.AddCustom("a", notif.Action);
							if (notif.Parameters != null && notif.Parameters.Names.Count > 0)
							{
								n.Payload.AddCustom("p", notif.Parameters.ToJObject());
							}
						}
						if (!string.IsNullOrEmpty(notif.Badge))
						{
							int badgeInt;
							if (int.TryParse(notif.Badge, out badgeInt))
							{
								n.Payload.Badge = badgeInt;
							}
							else if (notif.Badge == "!")
								n.Payload.Badge = -1;
						}
						if (notif.ExecutionTime != 0)
							n.Payload.ContentAvailable = 1;

						Service.QueueNotification(n);
						break;
					case PlatformType.AndroidGcm:
						EnsureAndroid();
						GcmNotification g = NotificationFactory.Google();
						g.RegistrationIds.Add(notif.DeviceToken);
                        g.JsonData = string.Format("{{ \"payload\":\"{0}\",\"action\":\"{1}\",\"parameters\": {2} ,\"executiontime\": {3} ,\"priority\": {4} }}"
							, notif.Message, notif.Action, notif.Parameters.ToJson(), notif.ExecutionTime.ToString(), notif.Delivery.Priority);
						g.CollapseKey = "NONE";
						Service.QueueNotification(g);
						break;
					case PlatformType.WindowsPhone:
						EnsureWindows();
						WindowsNotification w = NotificationFactory.Windows();
						w.ChannelUri = notif.DeviceToken;
						w.Message = notif.Message;
						w.Title = notif.Title;
						w.ImageUri = notif.Icon;
						w.Badge = notif.Badge;
						w.Sound = notif.Sound;
						w.Action = notif.Action;
						w.Parameters = notif.Parameters;
						w.ExecutionTime = notif.ExecutionTime;

						Service.QueueNotification(w);
						break;
					default:
						RemoteNotificationResult result = RemoteNotificationResult.ForDevice(notif.DeviceType, notif.DeviceToken);
						result.ErrorCode = INVALID_DEVICE_TYPE;
						result.ErrorDescription = GetErrorDescription(INVALID_DEVICE_TYPE);
						SetResult(result);
						break;
				}
			}

			Service.StopAllServices(true);
			return m_results;
		}

		public void SetConfiguration(string entryPoint, ConfigurationProps properties)
		{
			Certificates.Instance.MergePropertiesFor(entryPoint, properties);
		}

		#endregion
#else
		public int IOSSetBadge(string applicationId, string deviceToken, int badgeNumber, string sound)
		{
			return 0;
		}

		public int IOSResetBadge(string applicationId, string deviceToken)
		{
			return 0;
		}

		public int Call(string applicationId, short deviceType, string deviceToken, string alertMessage)
		{
			return 0;
		}
		public int OpenSession(string appId)
		{
			return 0;
		}
		public void Add(GxUserType sdt)
		{
			RemoteNotification notification = RemoteNotification.FromGxUserType(sdt);
			Add(notification);
		}
		public void Add(RemoteNotification notification)
		{
			
		}
		public List<RemoteNotificationResult> Send()
		{
			return new List<RemoteNotificationResult>();
		}
		public void SetConfiguration(string entryPoint, ConfigurationProps properties)
		{
		}
		public int ErrorCode { get { return 0; } }
		public string ErrorDescription
		{
			get
			{
				return string.Empty;
			}
		}
		public int CallAction(string applicationId, short deviceType, string deviceToken, string alertMessage, string action)
		{
			return 0;
		}

		public int CallAction(string applicationId, short deviceType, string deviceToken, string alertMessage, string action, NotificationParameters props)
		{
			return 0;
		}

#endif
	}
}
