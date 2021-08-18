using System.Security;

namespace GeneXusJWT.GenexusJWTClaims
{
    [SecuritySafeCritical]
    public class Claim
    {

        private string key;
        private object value;

        [SecuritySafeCritical]
        public Claim(string valueKey, object valueOfValue)
        {
            key = valueKey;
            value = valueOfValue;
        }

        [SecuritySafeCritical]
        public object getValue()
        {
            if (value.GetType() == typeof(string))
            {
                return (string)value;
            }else if(value.GetType() == typeof(long))
			{
                return (long)value;
			}else if(value.GetType() == typeof(int))
			{
                return (int)value;
			}else if(value.GetType() == typeof(double))
			{
                return (double)value;
			}else if(value.GetType() == typeof(bool))
			{
                return (bool)value;
			}

            else { return null; }

        }

        [SecuritySafeCritical]
        public string getKey()
        {
            return key;
        }

        [SecuritySafeCritical]
        public PrivateClaims getNestedClaims()
        {
            if (value.GetType() == typeof(PrivateClaims))
            {
                return (PrivateClaims)value;
            }
            else
            {
                return null;
            }
        }
    }
}
