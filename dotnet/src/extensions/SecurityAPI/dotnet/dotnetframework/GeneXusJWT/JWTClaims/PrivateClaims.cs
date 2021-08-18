using SecurityAPICommons.Commons;
using System.Collections.Generic;
using System.Security;

namespace GeneXusJWT.GenexusJWTClaims
{
    [SecuritySafeCritical]
    public class PrivateClaims : Claims
    {
        [SecuritySafeCritical]
        public bool setClaim(string key, string value)
        {
            return base.setClaim(key, value, new Error());
        }

        [SecuritySafeCritical]
        public bool setBooleanClaim(string key, bool value)
        {
            return base.setClaim(key, value, new Error());
        }

        [SecuritySafeCritical]
        public bool setNumericClaim(string key, int value)
        {
            return base.setClaim(key, value, new Error());
        }

        [SecuritySafeCritical]
        public bool setDateClaim(string key, long value)
        {
            return base.setClaim(key, value, new Error());
        }

        [SecuritySafeCritical]
        public bool setDoubleClaim(string key, double value)
        {
            return base.setClaim(key, value, new Error());
        }

        [SecuritySafeCritical]
        public bool setClaim(string key, PrivateClaims value)
        {
            return base.setClaim(key, value, new Error());
        }

        [SecuritySafeCritical]
        public Dictionary<string, object> getNestedMap()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            // System.out.println("size: "+getAllClaims().size());
            foreach (Claim c in getAllClaims())
            {
                if (c.getValue() != null)
                {

                    result.Add(c.getKey(), c.getValue());
                }
                else
                {
                    result.Add(c.getKey(), ((PrivateClaims)c.getNestedClaims()).getNestedMap());
                }
            }

            return result;
        }
    }
}
