using GeneXus;
using GeneXus.Application;
using GeneXus.Data;
using GeneXus.Utils;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GX
{
	public class ClientInformation
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GX.ClientInformation));

        public static string AppVersionCode
        {
            get
            {
                if (GxContext.Current.HttpContext != null)
                    return GxContext.Current.HttpContext.Request.Headers["GXAppVersionCode"];
                else
                    return string.Empty;
            }
        }
        public static string AppVersionName
        {
            get
            {
                if (GxContext.Current.HttpContext != null)
                    return GxContext.Current.HttpContext.Request.Headers["GXAppVersionName"];
                else
                    return string.Empty;
            }
        }
		public static string ApplicationId
		{
			get
			{
				if (GxContext.Current.HttpContext != null)
					return GxContext.Current.HttpContext.Request.Headers["GXApplicationId"];
				else
					return string.Empty;
			}
		}
		
        public static string Id
		{
			get
			{
				string id = string.Empty;
				GxContext ctx = GxContext.Current;
				if (ctx != null && ctx.HttpContext != null)
				{
					id = ctx.HttpContext.Request.Headers["DeviceId"];
					if (string.IsNullOrEmpty(id))
					{
						id = ctx.ClientID;
					}					
				}
				return id;
			}
		}
		public static string OSName 
		{
			get
			{
                if (GxContext.Current.HttpContext != null)
                    return GxContext.Current.HttpContext.Request.Headers["DeviceOSName"];
                else
                    return string.Empty;
			}
		}
		public static string OSVersion
		{
            get
            {
                if (GxContext.Current.HttpContext != null)
                    return GxContext.Current.HttpContext.Request.Headers["DeviceOSVersion"];
                else
                    return string.Empty;
            }
		}
		public static string Language
		{
            get
            {
                if (GxContext.Current.HttpContext != null)
                    return GxContext.Current.HttpContext.Request.Headers["Accept-Language"];
                else
                    return string.Empty;
            }
		}
		public static string PlatformName
		{
			get
			{
				if (GxContext.Current.HttpContext != null) {
					string platformName = GxContext.Current.HttpContext.Request.Headers["PlatformName"];
					if (string.IsNullOrEmpty(platformName))
						platformName = GxContext.Current.HttpContext.Request.Headers["DevicePlatform"];
					return platformName;
				}
				else
					return string.Empty;
			}
		}
		public static short DeviceType
		{
			get
			{
				short deviceType = 0;
				try
				{
					if (GxContext.Current.HttpContext != null)
						short.TryParse(GxContext.Current.HttpContext.Request.Headers["DeviceType"], out deviceType);
				}
				catch(Exception ex)
				{
					GXLogging.Error(log, "Failed DeviceType", ex);
				}
				return deviceType;
			}
		}
	}
	public class SDServerAPI
	{
		public static void InvalidateCache()
		{
			GxSmartCacheProvider.InvalidateAll();
		}
		public static void InvalidateCacheItem(string item)
		{
			GxSmartCacheProvider.Invalidate(item);
		}
	}

	public class Store
	{
		public static bool IsEnabled(string productId)
		{
			
			return false;
		}
	}
} 