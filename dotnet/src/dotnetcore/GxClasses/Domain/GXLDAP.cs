using System;

using System.Text;
using log4net;

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
		public short Connect()
		{
			
			return 0;
		}
		public void Disconnect()
		{
			
		}
		string getPath()
		{
			string path = _server;
			if (_port != 0)
				path += ":"+_port.ToString().Trim();
			return path;
		}
		void setAuthentication()
		{
			
		}
		public GxSimpleCollection<string> GetAttribute( string name, string context, GXProperties atts)
		{
			
			GxSimpleCollection<string> sc = new GxSimpleCollection<string>();
			
			return sc;

		}
	}

}
