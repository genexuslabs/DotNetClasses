using System.Collections.Generic;
using System.Linq;
using System.Security;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Utils;

namespace GeneXusJWT.GenexusJWTClaims
{
    [SecuritySafeCritical]
    public class Claims
    {

#pragma warning disable CA1051 // Do not declare visible instance fields
		protected List<Claim> _claims;
#pragma warning restore CA1051 // Do not declare visible instance fields

		[SecuritySafeCritical]
        public Claims()
        {
            _claims = new List<Claim>();
        }

        [SecuritySafeCritical]
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable CA1801 // Remove unused parameter
		public bool setClaim(string key, object value, Error error)
#pragma warning restore CA1801 // Remove unused parameter
#pragma warning restore IDE0060 // Remove unused parameter
		{
			Claim claim = new Claim(key, value);
            _claims.Add(claim);
            return true;
        }

        [SecuritySafeCritical]
        public List<Claim> getAllClaims()
        {
            return _claims;
        }

        [SecuritySafeCritical]
#pragma warning disable CA1707 // Identifiers should not contain underscores
		public virtual object getClaimValue(string key, Error _error)
#pragma warning restore CA1707 // Identifiers should not contain underscores
		{
			if (_error == null) return "";
            for (int i = 0; i < _claims.Count; i++)
            {
                if (SecurityUtils.compareStrings(key, _claims.ElementAt(i).getKey()))
                {
                    return _claims.ElementAt(i).getValue();
                }
            }
            _error.setError("CL001", "Could not find a claim with" + key + " key value");
            return "";
        }

        public bool isEmpty()
        {
            if (_claims.Count == 0)
            {
                return true;
            }
            else
            {
                return false;

            }
        }
    }
}
