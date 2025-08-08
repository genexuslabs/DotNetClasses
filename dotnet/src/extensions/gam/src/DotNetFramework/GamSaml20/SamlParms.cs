
using System.Security;

namespace GamSaml20
{
	[SecuritySafeCritical]
	public class SamlParms
	{
		[SecuritySafeCritical]  private string _id;
		public string Id { get { return _id; } set { _id = value; } }

		[SecuritySafeCritical] private string _endPointLocation;
		public string EndPointLocation { get { return _endPointLocation; } set { _endPointLocation = value; } }

		[SecuritySafeCritical] private string _singleLogoutEndpoint;
		public string SingleLogoutEndpoint { get { return _singleLogoutEndpoint; } set { _singleLogoutEndpoint = value; } }

		[SecuritySafeCritical] private string _acs;
		public string Acs { get { return _acs; } set { _acs = value; } }

		[SecuritySafeCritical] private string _identityProviderEntityID;
		public string IdentityProviderEntityID { get { return _identityProviderEntityID; } set { _identityProviderEntityID = value; } }

		[SecuritySafeCritical] private string _certPath;
		public string CertPath { get { return _certPath; } set { _certPath = value; } }
		[SecuritySafeCritical] private string _certPass;
		public string CertPass { get { return _certPass; } set { _certPass = value; } }

		[SecuritySafeCritical] private string _certAlias;
		public string CertAlias { get { return _certAlias; } set { _certAlias = value; } }

		[SecuritySafeCritical] private string _policyFormat;
		public string PolicyFormat { get { return _policyFormat; } set { _policyFormat = value; } }

		[SecuritySafeCritical] private string _authContext;
		public string AuthContext { get { return _authContext; } set { _authContext = value; } }
		[SecuritySafeCritical] private string _serviceProviderEntityID;
		public string ServiceProviderEntityID { get { return _serviceProviderEntityID; } set { _serviceProviderEntityID = value; } }

		[SecuritySafeCritical] private bool _forceAuthn;
		public bool ForceAuthn { get { return _forceAuthn; } set { _forceAuthn = value; } }

		[SecuritySafeCritical] private string _nameID;
		public string NameID { get { return _nameID; } set { _nameID = value; } }
		[SecuritySafeCritical] private string _sessionIndex;
		public string SessionIndex { get { return _sessionIndex; } set { _sessionIndex = value; } }
	
		[SecuritySafeCritical] private string _trustedCertPath;

		public string TrustedCertPath { get { return _trustedCertPath; } set { _trustedCertPath = value; } }

		[SecuritySafeCritical] private string _trustedCertPass;
		public string TrustedCertPass { get { return _trustedCertPass; } set { _trustedCertPass = value; } }

		[SecuritySafeCritical] private string _trustedCertAlias;

		public string TrustedCertAlias { get { return _trustedCertAlias; } set { _trustedCertAlias = value; } }

		[SecuritySafeCritical]
		public SamlParms()
		{
			_id = string.Empty;
			_endPointLocation = string.Empty;
			_singleLogoutEndpoint = string.Empty;
			_identityProviderEntityID = string.Empty;
			_acs = string.Empty;
			_policyFormat = string.Empty;
			_authContext = string.Empty;
			_serviceProviderEntityID = string.Empty;
			_forceAuthn = false;
			_authContext = string.Empty;
			_certAlias = string.Empty;
			_certPass = string.Empty;
			_certPath = string.Empty;
			_trustedCertPath = string.Empty;
			_trustedCertPass = string.Empty;
			_trustedCertAlias = string.Empty;
			_nameID = string.Empty;
			_sessionIndex = string.Empty;
		}
	}
}
