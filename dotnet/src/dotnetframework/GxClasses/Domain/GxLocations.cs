namespace GeneXus.Utils
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;

	public class GxLocation
	{
		string _name = "";
		string _host = "";
		int _port;
		short _secure;
		short _timeout;
		string _wsdlUrl = "";
		string _baseUrl = "";
		string _resourceName = "";
		short _authentication;
		short _authenticationMethod;
		string _authenticationUser = "";
		string _authenticationRealm = "";
		string _authenticationPassword = "";
		string _proxyAuthenticationUser = "";
		string _proxyAuthenticationPassword = "";
		List<CertificateDescription> _certificates = new List<CertificateDescription>();
		short _cancelOnError;
		string _groupLocation = "";
		string _proxyHost;
		int _proxyPort;
		public string Name
		{
			get { return _name;}
			set { _name = value;}
		}
		public string Host
		{
			get {return _host;}
			set {_host = value;}
		}
		public int Port
		{
			get {return _port;}
			set {_port = value;}
		}
		public short Secure
		{
			get {return _secure;}
			set {_secure = value;}
		}
		public short Timeout
		{
			get {return _timeout;}
			set {_timeout = value;}
		}
		public string Wsdlurl
		{
			get { return _wsdlUrl; }
			set { _wsdlUrl = value; }
		}
		public string BaseUrl
		{
			get {return _baseUrl;}
			set {_baseUrl = value;}
		}
		public string ResourceName
		{
			get { return _resourceName; }
			set { _resourceName = value; }
		}
		public short Authentication
		{
			get {return _authentication;}
			set {_authentication = value;}
		}
		public short AuthenticationMethod
		{
			get {return _authenticationMethod;}
			set {_authenticationMethod = value;}
		}
		public string AuthenticationUser
		{
			get {return _authenticationUser;}
			set {_authenticationUser = value;}
		}
		public string AuthenticationRealm
		{
			get {return _authenticationRealm;}
			set {_authenticationRealm = value;}
		}
		public string AuthenticationPassword
		{
			get {return _authenticationPassword;}
			set {_authenticationPassword = value;}
		}
		public short CancelOnError
		{
			get {return _cancelOnError;}
			set {_cancelOnError = value;}
		}
		public string GroupLocation
		{
			get {return _groupLocation;}
			set {_groupLocation = value;}
		}
		public string ProxyServerHost
		{
			get {return _proxyHost;}
			set {_proxyHost = value;}
		}
		public int ProxyServerPort
		{
			get {return _proxyPort;}
			set {_proxyPort = value;}
		}
		public short ProxyAuthentication
		{
			get {return _authentication;}
			set {_authentication = value;}
		}
		public short ProxyAuthenticationMethod
		{
			get {return _authenticationMethod;}
			set {_authenticationMethod = value;}
		}
		public string ProxyAuthenticationUser
		{
			get {return _proxyAuthenticationUser;}
			set {_proxyAuthenticationUser = value;}
		}
		public string ProxyAuthenticationRealm
		{
			get {return _authenticationRealm;}
			set {_authenticationRealm = value;}
		}
		public string ProxyAuthenticationPassword
		{
			get {return _proxyAuthenticationPassword;}
			set {_proxyAuthenticationPassword = value;}
		}
		public string Certificate
		{
			get 
			{ 
				string txt = "";
				foreach(CertificateDescription cd in _certificates)
				{
					if (cd.Password.Trim().Length == 0)
						txt += cd.FileName + " ";
					else 
						txt += "("+cd.FileName + "," + cd.Password + ") ";
				}
				return txt; 
			}
			set 
			{
				Regex r = new Regex( @"(\s*\((?'fName'\S+)\s*\,\s*(?'pass'\S+)\s*\)|(?'fName'\S+))");
				_certificates.Clear();
				foreach( Match m in r.Matches(value))
					AddCertificate( m.Groups["fName"].Value,  m.Groups["pass"].Value);
			}
		}
		public void AddCertificate( string name, string pass)
		{
			_certificates.Add( new CertificateDescription( name, pass));
		}
		public string Configuration { get; set; }
		public GXWSAddressing WSAddressing { get; set; } = new GXWSAddressing();
		public GXWSSecurity WSSecurity { get; set; } = new GXWSSecurity();
	}
	public class GxLocationCollection : ArrayList
	{
		public void Add( GxLocation obj, string colName)
		{
			this.Add( obj);
		}
		public GxLocation GetItem( string colName)
		{
			for ( int i = 0; i < Count; i ++)
				if (((GxLocation)this[i]).Name.ToUpper() == colName.ToUpper())
					return (GxLocation)this[i];
			return null;
		}
	}
	public class CertificateDescription
	{
		string fileName;
		string pass;

		public CertificateDescription(string n, string p)
		{
			fileName = n;
			pass = p;
		}
		public string FileName
		{
			get { return fileName; }
			set { fileName = value; }
		}

		public string Password
		{
			get { return pass; }
			set { pass = value; }
		}
	}

	public class GXWSAddressing
	{
		public string Action { get; set; }
		public GXWSAddressingEndPoint FaultTo { get; set; }
		public GXWSAddressingEndPoint From { get; set; }
		public string MessageID { get; set; }
		public string RelatesTo { get; set; }
		public GXWSAddressingEndPoint ReplyTo { get; set; }
		public string To { get; set; }
	}

	public class GXWSAddressingEndPoint
	{
		public string Address { get; set; }
		public string Parameters { get; set; }
		public string PortType { get; set; }
		public string Properties { get; set; }
		public string ServiceName { get; set; }
	}

	public class GXWSSecurity
	{
		public GXWSSignature Signature { get; set; }
		public GXWSEncryption Encryption { get; set; }
	}

	public class GXWSSignature
	{
		public GXWSSecurityKeyStore Keystore { get; set; }
		public string Alias { get; set; }
		public int KeyIdentifierType { get; set; }
	}

	public class GXWSEncryption
	{
		public GXWSSecurityKeyStore Keystore { get; set; }
		public string Alias { get; set; }
		public int KeyIdentifierType { get; set; }
	}

	public class GXWSSecurityKeyStore
	{
		public string Type { get; set; }
		public string Password { get; set; }
		public string Source { get; set; }
	}

	public enum IdentifierType
	{
		BINARY_SECURITY_TOKEN,
		ISSUER_SERIAL,
		X509_KEY_IDENTIFIER,
		SKI_KEY_IDENTIFIER,
		THUMBPRINT_IDENTIFIER,
		KEY_VALUE
	}

	public enum KeyStoreType
	{
		JKS,
		JCEKS,
		PKCS11
	}
}