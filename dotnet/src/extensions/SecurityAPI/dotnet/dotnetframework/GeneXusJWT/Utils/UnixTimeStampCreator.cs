using System;
using System.Globalization;
using System.Security;
using SecurityAPICommons.Commons;
using log4net;

namespace GeneXusJWT.Utils
{
	[SecuritySafeCritical]
	public class UnixTimeStampCreator: SecurityAPIObject
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(UnixTimeStampCreator));
		[SecuritySafeCritical]
		public UnixTimeStampCreator() : base() { }

		[SecuritySafeCritical]
		public string Create(string date)
		{
			logger.Debug("Create");
			long newdate;
			try
			{
				//newdate= (long)DateTime.ParseExact(date, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToUniversalTime()).TotalSeconds;
				newdate = (long)DateTime.ParseExact(date, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture).Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
			}
			catch (Exception)
			{
				error.setError("UTS01", "Date format error; expected yyyy/MM/dd HH:mm:ss");
				logger.Error("Date format error; expected yyyy/MM/dd HH:mm:ss");
				return "";
			}
			return newdate.ToString(CultureInfo.InvariantCulture);
		}
	}
}
