using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using log4net;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Utils;

namespace GeneXusJWT.GenexusJWTClaims
{
    [SecuritySafeCritical]
    public class RegisteredClaims : Claims
    {
        private IDictionary<string, string> customTimeValidationClaims;

		private static readonly ILog logger = LogManager.GetLogger(typeof(RegisteredClaims));

		public RegisteredClaims()
        {

            customTimeValidationClaims = new Dictionary<string, string>();

        }


        public bool setClaim(string key, string value, Error error)
        {
			logger.Debug("setClaim");
			if (error == null) return false;
            if (RegisteredClaimUtils.exists(key))
            {
                return base.setClaim(key, value, error);
            }
            else
            {
                error.setError("RCS02", "Wrong registered key value");
				logger.Error("Wrong registered key value");

				return false;
            }
        }

        public bool setTimeValidatingClaim(string key, string value, string customValidationSeconds, Error error)
        {
			logger.Debug("setTimeValidatingClaim");
			if (error == null) return false;
            if (RegisteredClaimUtils.exists(key) && RegisteredClaimUtils.isTimeValidatingClaim(key))
            {
                Int32 date = 0;
                customTimeValidationClaims.Add(key, customValidationSeconds);
                try
                {
                   // date = (Int32)DateTime.ParseExact(value, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture).ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
					date = (Int32)DateTime.ParseExact(value, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
				}
                catch (Exception)
                {
                    error.setError("RCS04", "Date format error; expected yyyy/MM/dd HH:mm:ss");
					logger.Error("Date format error; expected yyyy/MM/dd HH:mm:ss");
                    return false;
                }
                return setClaim(key, date.ToString(CultureInfo.InvariantCulture), error);
            }
            else
            {
                error.setError("RCS02", "Wrong registered key value");
				logger.Error("Wrong registered key value");
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

            return long.Parse(stringTime, CultureInfo.InvariantCulture);

        }

        public bool hasCustomValidationClaims()
        {
            return customTimeValidationClaims.Count != 0;
        }


        public override object getClaimValue(string key, Error error)
        {
			logger.Debug("getClaimValue");
			if (error == null) return "";
            if (RegisteredClaimUtils.exists(key))
            {
                for (int i = 0; i < _claims.Count; i++)
                {
                    if (SecurityUtils.compareStrings(key, _claims[i].getKey()))
                    {
                        return _claims[i].getValue();
                    }
                }
                error.setError("RCS03", String.Format("Could not find a claim with {0} key value", key));
				logger.Error(String.Format("Could not find a claim with {0} key value", key));
                return "";
            }
            else
            {
                error.setError("RC002", "Wrong registered key value");
				logger.Error("Wrong registered key value");
                return "";
            }
        }
    }
}
