using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using PushSharp.Common;
using Jayrock.Json;
using System.IO;
using Artech.Genexus.SDAPI;

namespace PushSharp.Windows
{
	public class WindowsNotification : PushSharp.Common.Notification
	{
		public WindowsNotification()
		{
			this.Platform = PlatformType.Windows;
			this.ChannelUri = string.Empty;
		}

		/// <summary>
		/// Registration ID of the Device
		/// </summary>
		public string ChannelUri
		{
			get;
			set;
		}
		/// <summary>
		/// Notification type: toast, tile, badge
		/// </summary>
		public string NotificationType
		{
			get;
			set;
		}
		/// <summary>
		/// Message to send
		/// </summary>
		public string Message
		{
			get;
			set;
		}

		public string Title { get; set; }

		public string ImageUri { get; set; }

		public string Badge { get; set; }

		public string Sound { get; set; }

		public string Action { get; set; }

		public short ExecutionTime { get; set; }

		/// <summary>
		/// Parameters (template for tiles, images, etc)
		/// </summary>
		public NotificationParameters Parameters
		{
			get;
			set;
		}
		
		public string GetPayload()
		{
			return Win8Notifications.GetMessage(this);
		}
	}
}
