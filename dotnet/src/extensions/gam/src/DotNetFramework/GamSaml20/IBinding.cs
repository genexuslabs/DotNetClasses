
using System.Security;

namespace GamSaml20
{
	[SecuritySafeCritical]
	public interface IBinding
	{

		[SecuritySafeCritical]
		void Init(string input);

#if NETCORE

		abstract static string Login(SamlParms parms, string relayState);

		abstract static string Logout(SamlParms parms, string relayState);
#endif

		[SecuritySafeCritical]
		bool VerifySignatures(SamlParms parms);
		[SecuritySafeCritical]
		string GetLoginAssertions();
		[SecuritySafeCritical]
		string GetLoginAttribute(string name);

		[SecuritySafeCritical]
		string GetRoles(string name);
		[SecuritySafeCritical]
		string GetLogoutAssertions();

		[SecuritySafeCritical]
		bool IsLogout();
	}
}
