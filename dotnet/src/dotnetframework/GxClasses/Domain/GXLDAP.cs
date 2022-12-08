using System;
using System.Collections;
using System.DirectoryServices;
using System.Text;
using log4net;
#if NETCORE
using Novell.Directory.Ldap;
#endif

namespace GeneXus.Utils
{
	public class GXLDAPClient
	{
        static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		string _server;
		int _port;
		string _authenticationMethod;
		string _user;
		string _password;
        int _secure;
		DirectoryEntry _entry;
#if NETCORE
		LdapConnection _conn;
#endif
		public GXLDAPClient()
		{
			_server = "localhost";
			_authenticationMethod = "";
			_user = "";
			_password = "";
		}

		public string Host
		{
			get {return _server;}
			set {_server = value;}
		}
		public int Port
		{
			get {return _port;}
			set {_port = value;}
		}
		public string AuthenticationMethod
		{
			get {return _authenticationMethod;}
			set {_authenticationMethod = value;}
		}
		public string User
		{
			get {return _user;}
			set {_user = value;}
		}
		public string Password
		{
			get {return _password;}
			set {_password = value;}
		}
        public int Secure
        {
            get { return _secure; }
            set { _secure = value; }
        }
#if NETCORE
		internal int getLdapConnectionPort() {
			int port = 0;
			if (_port != 0)
			{
				port = _port;
			}
			else
			{
				if (_secure == 1)
				{
					port = LdapConnection.DefaultSslPort;
				}
				else
				{
					port = LdapConnection.DefaultPort;
				}
			}
			return port;
		}
		internal short NovellConnect()
		{
			try
			{
				if (_conn != null)
				{
					_conn.Disconnect();
					_conn = null;
				}
				_conn = new LdapConnection();
				_conn.SecureSocketLayer = (_secure == 1);
				_conn.Connect(_server, getLdapConnectionPort());
				_conn.Bind(_user, _password);
				return (short)((_conn.Connected) ? 1 : 0);
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Connect Method Error.", ex);
				return 0;
			}
		}
		void NovellDisconnect()
		{
			if (_conn != null)
			{
				_conn.Disconnect();
				_conn = null;
			}
		}
#endif
		public short Connect()
		{
#if NETCORE

			if (!GXUtil.IsWindowsPlatform)
			{
				return NovellConnect();
			}
			else
#endif
			{

				if (_entry != null)
				{
					_entry.Close();
					_entry = null;
				}
				_entry = new DirectoryEntry("LDAP://" + getPath(), _user, _password, getAuthentication());
				try
				{
					if (_entry.NativeObject != null)
						return 1;
					else
						return 0;
				}
				catch (Exception ex) //DirectoryServicesCOMException
				{
					GXLogging.Error(log, "Connect Method Error.", ex);
					return 0;
				}
			}
		}
		public void Disconnect()
		{
#if NETCORE
			if (GXUtil.IsWindowsPlatform)
			{
				NovellDisconnect();
			}
			else
#endif
			{
				if (_entry != null)
				{
					_entry.Close();
					_entry = null;
				}
			}
		}
		string getPath()
		{
			string path = _server;
			if (_port != 0)
				path += ":"+_port.ToString().Trim();
			return path;
		}
		AuthenticationTypes getAuthentication()
		{
			if (_authenticationMethod.Trim().ToUpper() == "SSL")
				return AuthenticationTypes.SecureSocketsLayer;
			if (_authenticationMethod.Trim().ToUpper() == "SASL")
				return AuthenticationTypes.Secure;

            if (_secure == 1)
            {
                return AuthenticationTypes.Secure;
            }

			return AuthenticationTypes.None;
		}
		public GxSimpleCollection<string> GetAttribute(string name, string context, GXProperties atts)
		{
			string filter = "";
			if (atts.Count == 0)
				filter = "(" + name + "=*)";
			else
			{
				for (int i = 0; i < atts.Count; i++)
					filter += "(" + atts.GetKey(i).Trim() + "=" + atts[i].Trim() + ")";
				if (atts.Count > 1)
					filter = "(&" + filter + ")";
			}
			GxSimpleCollection<string> sc = new GxSimpleCollection<string>();

			try
			{
#if NETCORE
				if (!GXUtil.IsWindowsPlatform)
				{
					NovellConnect();

					string searchBase = context;
					int searchScope = LdapConnection.ScopeSub;
					string searchFilter = filter;
					ILdapSearchResults lsc = _conn.Search(searchBase, searchScope, searchFilter, new string[] { name }, false);

					while (lsc.HasMore())
					{
						LdapEntry nextEntry = null;
						try
						{
							nextEntry = lsc.Next();
						}
						catch (LdapException)
						{
							continue;
						}
						LdapAttributeSet attributeSet = nextEntry.GetAttributeSet();
						IEnumerator ienum = attributeSet.GetEnumerator();
						StringBuilder sb = new StringBuilder();
						while (ienum.MoveNext())
						{
							LdapAttribute attribute = (LdapAttribute)ienum.Current;
							string attributeVal = attribute.StringValue;
							sb.Append(attributeVal + " ");
						}
						sc.Add(sb.ToString() + " ");
					}
				}
				else
#endif
				{
					if (_entry != null)
					{
						_entry.Close();
						_entry = null;
					}
					string context1;
					if (context.Trim().Length == 0)
						context1 = "";
					else
						context1 = "/" + context;
					AuthenticationTypes at = getAuthentication();
					_entry = new DirectoryEntry("LDAP://" + getPath() + context1, _user, _password, at);
					using (DirectorySearcher ds = new DirectorySearcher(_entry, filter, new string[] { name }))
					{
						foreach (SearchResult result in ds.FindAll())
						{
							PropertyValueCollection values = (PropertyValueCollection)(result.GetDirectoryEntry().Properties[name]);
							StringBuilder sb = new StringBuilder();
							for (int i = 0; i < values.Count; i++)
								sb.Append(values[i].ToString() + " ");
							sc.Add(sb.ToString());
						}
					}
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "GetAttribute Method Error.", ex);
			}
			return sc;
		}
	}

}