using System.Security;
namespace SecurityAPICommons.Commons
{
	[SecuritySafeCritical]
	public class PrivateKey : Key
	{
		[SecuritySafeCritical]
		bool LoadEncrypted (string path, string password) { return false; }

	}
}
