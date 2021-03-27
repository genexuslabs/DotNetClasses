using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace GeneXus.Http
{
	internal static class CookieContainerHelper
	{
		public static IEnumerable<Cookie> GetCookies(this CookieContainer cookieContainer)
		{
			var domainTable = GetFieldValue<dynamic>(cookieContainer, "_domainTable");
			foreach (var entry in domainTable)
			{
				string key = GetPropertyValue<string>(entry, "Key");

				var value = GetPropertyValue<dynamic>(entry, "Value");

				var internalList = GetFieldValue<SortedList<string, CookieCollection>>(value, "_list");
				foreach (var li in internalList)
				{
					foreach (Cookie cookie in li.Value)
					{
						yield return cookie;
					}
				}
			}
		}

		internal static T GetFieldValue<T>(object instance, string fieldName)
		{
			BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			FieldInfo fi = instance.GetType().GetField(fieldName, bindFlags);
			return (T)fi.GetValue(instance);
		}

		internal static T GetPropertyValue<T>(object instance, string propertyName)
		{
			var pi = instance.GetType().GetProperty(propertyName);
			return (T)pi.GetValue(instance, null);
		}
	}
	internal class HttpSessionState 
	{
		ISession session;

		public HttpSessionState(ISession session)
		{
			this.session = session;
		}

		public string SessionID { get { return session.Id; } internal set { } }
		
		public string this[string name] {
			get
			{
				return session.GetString(name);
			}
			set
			{
				session.SetString(name, value);
			}
		}
		internal void Remove(string key)
		{
			session.Remove(key);
		}

		internal void RemoveAll()
		{
			session.Clear();
		}

		internal void Clear()
		{
			session.Clear();
		}

		internal void Abandon()
		{
		}
	}
}