using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace GamSaml20Net
{
    public interface IBinding
    {

		abstract void Init(string input);
		abstract static string Login(SamlParms parms, string relayState);

		abstract static string Logout(SamlParms parms, string relayState);

		public bool VerifySignatures(SamlParms parms);
		string GetLoginAssertions();
		string GetLoginAttribute(string name);

		string GetRoles(string name);
		string GetLogoutAssertions(); 

	}
}
