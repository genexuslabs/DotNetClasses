using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace GeneXus.Http
{
	internal static class CookieContainerHelper
	{
		public static CookieCollection GetCookies(this CookieContainer container)
		{
			CookieCollection allCookies = new CookieCollection();
			Hashtable domainTable = (Hashtable)container.GetType()
				.GetRuntimeFields()
				.First(x => x.Name == "m_domainTable")
				.GetValue(container);

			FieldInfo pathListField = null;
			foreach (object domain in domainTable.Values)
			{
				SortedList pathList = (SortedList)(pathListField ??= domain.GetType()
					.GetRuntimeFields()
					.First(x => x.Name == "m_list"))
					.GetValue(domain);

				foreach (CookieCollection cookies in pathList.GetValueList())
					allCookies.Add(cookies);
			}
			return allCookies;
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