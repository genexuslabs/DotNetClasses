using System;
using Microsoft.AspNetCore.Http;

namespace GeneXus.Http
{
	internal class HttpSessionState 
	{
		ISession session;

		public HttpSessionState(ISession session)
		{
			this.session = session;
		}

		public string SessionID { get { return session.Id; } internal set { } }
		
		public object this[string name] { get { return session.GetString(name); } set { session.SetString(name, value.ToString()); } }

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