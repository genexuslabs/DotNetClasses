using System.Collections.Generic;
using System.Security;

namespace GeneXusJWT.JWTClaims
{
	[SecuritySafeCritical]
	public class HeaderParameters
	{
		/*
         * Cannot avoid typ=JWT because of RFC 7519 https://tools.ietf.org/html/rfc7519
         * https://github.com/auth0/java-jwt/issues/369
         */
		private Dictionary<string, object> map;


		[SecuritySafeCritical]
		public HeaderParameters()
		{
			map = new Dictionary<string, object>();
		}

		[SecuritySafeCritical]
		public void SetParameter(string name, string value)
		{
			map.Add(name, value);
		}


		[SecuritySafeCritical]
		public Dictionary<string, object> GetMap()
		{
			return this.map;
		}

		[SecuritySafeCritical]
		public List<string> GetAll()
		{
			return new List<string>(this.map.Keys);
		}

		[SecuritySafeCritical]
		public bool IsEmpty()
		{
			if(GetAll().Count == 0)
			{
				return true;
			}
			return false;
		}
	}
}
