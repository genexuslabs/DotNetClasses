using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace GeneXus.Utils
{
	public class GxUserInfo
	{
		static ConcurrentDictionary<int, ArrayList> _userInfo = new ConcurrentDictionary<int, ArrayList>();
		static ConcurrentDictionary<int, object> _sponsors;
		private static object syncRoot = new Object();
		private static int hnd;

		public static ConcurrentDictionary<int, ArrayList> UserInfo
		{
			get
			{
				return _userInfo;
			}
			set { _userInfo = value; }
		}

		public static int NewHandle()
		{
			int localHnd = 0;
			lock (syncRoot)
			{
				if (hnd == int.MaxValue)
				{
					hnd = 0;
				}
				hnd++;
				localHnd = hnd;
			}
			return localHnd;
		}

		public static ConcurrentDictionary<int, object> Sponsors
		{
			get
			{
				if (_sponsors == null)
					_sponsors = new ConcurrentDictionary<int, object>();
				return _sponsors;
			}
			set { _sponsors = value; }
		}

		public static void AddHandle(int handle)
		{
			UserInfo.TryAdd(handle, new ArrayList());
		}

		public static void RemoveHandle(int handle)
		{
			ArrayList list;
			if (UserInfo.TryRemove(handle, out list))
				list.Clear();
			object l;
			Sponsors.TryRemove(handle, out l);
		}

		public static void AddSponsor(int handle, object value)
		{
			Sponsors.TryAdd(handle, value);
		}
		public static object GetSponsor(int handle)
		{
			return Sponsors[handle];
		}

		public static void setProperty(int handle, string propName, string propValue)
		{
			string[] prop = { propName, propValue };

			if (UserInfo.ContainsKey(handle))
			{
				ArrayList userProps = (ArrayList)_userInfo[handle];
				bool founded = false;

				foreach (string[] p in userProps)
				{
					if (p[0] == propName)
					{
						p[1] = propValue;
						founded = true;
					}
				}
				if (!founded)
				{
					((ArrayList)_userInfo[handle]).Add(prop);
				}
			}
			else
			{
				ArrayList props = new ArrayList();
				props.Add(prop);
				UserInfo.TryAdd(handle, props);
			}
		}
		public static string getProperty(int handle, string propName)
		{
			if (UserInfo.ContainsKey(handle))
			{
				ArrayList props = (ArrayList)_userInfo[handle];
				foreach (string[] prop in props)
				{
					if (prop[0] == propName) return prop[1];
				}
			}
			return null;
		}
	}

}
