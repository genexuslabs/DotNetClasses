using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
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
	public static class LockTracker
	{
		private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

		public static SemaphoreSlim Get(string sessionId)
		{
			return _locks.GetOrAdd(sessionId, _ => new SemaphoreSlim(1, 1));
		}
	}
	internal class HttpSyncSessionState : HttpSessionState
	{
		internal const string CTX_SESSION = "GX_CTX_SESSION";
		Dictionary<string, string> _contextValues;
		internal HttpSyncSessionState(HttpContext context):base(context.Session) 
		{
			if (context.Items.TryGetValue(CTX_SESSION, out object values))
				_contextValues = values as Dictionary<string, string>;

			if (_contextValues == null)
			{
				_contextValues = new Dictionary<string, string>();
				foreach (string item in session.Keys)
				{
					_contextValues[item] = session.GetString(item);
				}
				context.Items[CTX_SESSION] = _contextValues;
			}
		}
		public override string this[string name]
		{
			get
			{
				_contextValues.TryGetValue(name, out string value);
				return value;
			}
			set
			{
				_contextValues[name]= value;
			}
		}
		internal override void Remove(string key)
		{
			_contextValues[key] = null; 
		}

		internal override void Clear()
		{
			foreach (string key in _contextValues.Keys)
			{
				_contextValues[key] = null;
			}
		}

	}
	internal class HttpSessionState
	{
		protected ISession session;

		public HttpSessionState(ISession session)
		{
			this.session = session;
		}

		public string SessionID { get { return session.Id; } internal set { } }
		
		public virtual string this[string name] {
			get
			{
				return session.GetString(name);
			}
			set
			{
				session.SetString(name, value);
			}
		}
		internal virtual void Remove(string key)
		{
			session.Remove(key);
		}

		internal void RemoveAll()
		{
			Clear();
		}

		internal virtual void Clear()
		{
			session.Clear();
		}

		internal void Abandon()
		{
		}
	}
}