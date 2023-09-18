using System;
using System.Globalization;
using System.Security;
using SecurityAPICommons.Commons;

namespace GeneXusJWT.Utils
{
	[SecuritySafeCritical]
	public class UnixTimeStampCreator: SecurityAPIObject
	{
		[SecuritySafeCritical]
		public UnixTimeStampCreator() : base() { }

		[SecuritySafeCritical]
		public string Create(string date)
		{
			long newdate;
			try
			{
				//newdate= (long)DateTime.ParseExact(date, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToUniversalTime()).TotalSeconds;
				newdate = (long)DateTime.ParseExact(date, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture).Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
			}
			catch (Exception)
			{
				error.setError("UTS01", "Date format error; expected yyyy/MM/dd HH:mm:ss");
				return "";
			}
			return newdate.ToString(CultureInfo.InvariantCulture);
		}
	}
}
