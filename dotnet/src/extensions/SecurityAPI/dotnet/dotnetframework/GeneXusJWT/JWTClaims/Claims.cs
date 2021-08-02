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

        protected List<Claim> _claims;

        [SecuritySafeCritical]
        public Claims()
        {
            _claims = new List<Claim>();
        }

        [SecuritySafeCritical]
        public bool setClaim(string key, object value, Error error)
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
        public virtual object getClaimValue(string key, Error error)
        {
            for (int i = 0; i < _claims.Count; i++)
            {
                if (SecurityUtils.compareStrings(key, _claims.ElementAt(i).getKey()))
                {
                    return _claims.ElementAt(i).getValue();
                }
            }
            error.setError("CL001", "Could not find a claim with" + key + " key value");
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
