using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Utils;

namespace GeneXusJWT.GenexusJWTClaims
{
    [SecuritySafeCritical]
    public class RegisteredClaims : Claims
    {
        private IDictionary<string, string> customTimeValidationClaims;

        public RegisteredClaims()
        {

            customTimeValidationClaims = new Dictionary<string, string>();

        }


        public bool setClaim(string key, string value, Error error)
        {
            if (RegisteredClaimUtils.exists(key))
            {
                return base.setClaim(key, value, error);
            }
            else
            {
                error.setError("RC001", "Wrong registered key value");
                return false;
            }
        }

        public bool setTimeValidatingClaim(string key, string value, string customValidationSeconds, Error error)
        {
            if (RegisteredClaimUtils.exists(key) && RegisteredClaimUtils.isTimeValidatingClaim(key))
            {
                Int32 date = 0;
                customTimeValidationClaims.Add(key, customValidationSeconds);
                try
                {
                    date = (Int32)DateTime.ParseExact(value, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                }
                catch (Exception)
                {
                    error.setError("RC004", "Incorrect date format. Expected yyyy/MM/dd HH:mm:ss");
                    return false;
                }
                return setClaim(key, date.ToString(), error);
            }
            else
            {
                error.setError("RC001", "Wrong registered key value");
                return false;
            }
        }

        public long getClaimCustomValidationTime(string key)
        {
            string stringTime = "";

            if (customTimeValidationClaims.ContainsKey(key))
            {
                try
                {
                    customTimeValidationClaims.TryGetValue(key, out stringTime);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }

            return long.Parse(stringTime);

        }

        public bool hasCustomValidationClaims()
        {
            return customTimeValidationClaims.Count != 0;
        }


        public override object getClaimValue(string key, Error error)
        {
            if (RegisteredClaimUtils.exists(key))
            {
                for (int i = 0; i < _claims.Count; i++)
                {
                    if (SecurityUtils.compareStrings(key, _claims[i].getKey()))
                    {
                        return _claims[i].getValue();
                    }
                }
                error.setError("RC001", "Could not find a claim with" + key + " key value");
                return "";
            }
            else
            {
                error.setError("RC002", "Wrong registered key value");
                return "";
            }
        }
    }
}
