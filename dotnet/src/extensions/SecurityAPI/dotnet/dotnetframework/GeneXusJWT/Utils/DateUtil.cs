using System;
using System.Security;
using System.Globalization;
using GeneXusJWT.GenexusComons;


namespace GeneXusJWT.GenexusJWTUtils
{
    /*****DEPRECATED OBJECT SINCE GeneXus 16 upgrade 11******/

    [SecuritySafeCritical]
    public class DateUtil : DateUtilObject
    {


        [SecuritySafeCritical]
        public DateUtil() : base()
        {

        }

		/******** EXTERNAL OBJECT PUBLIC METHODS - BEGIN ********/
#pragma warning disable CA1305 // Specify IFormatProvider
		[Obsolete("DateUtil object is deprecated. Use GeneXus DateTime data type instead https://wiki.genexus.com/commwiki/servlet/wiki?7370,DateTime%20data%20type")]
		[SecuritySafeCritical]
        public override string GetCurrentDate()
        {
            DateTime date = DateTime.ParseExact(DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss"), "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return date.ToString("yyyy/MM/dd HH:mm:ss");
        }

        [Obsolete("DateUtil object is deprecated. Use GeneXus DateTime data type instead https://wiki.genexus.com/commwiki/servlet/wiki?7370,DateTime%20data%20type")]
        [SecuritySafeCritical]
        public override string CurrentMinusSeconds(long seconds)
        {
            DateTime date = DateTime.ParseExact(DateTime.UtcNow.AddSeconds(-seconds).ToString("yyyy/MM/dd HH:mm:ss"), "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return date.ToString("yyyy/MM/dd HH:mm:ss");
        }

        [Obsolete("DateUtil object is deprecated. Use GeneXus DateTime data type instead https://wiki.genexus.com/commwiki/servlet/wiki?7370,DateTime%20data%20type")]
        [SecuritySafeCritical]
        public override string CurrentPlusSeconds(long seconds)
        {
            DateTime date = DateTime.ParseExact(DateTime.UtcNow.AddSeconds(seconds).ToString("yyyy/MM/dd HH:mm:ss"), "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
			return date.ToString("yyyy/MM/dd HH:mm:ss");

		}
#pragma warning restore CA1305 // Specify IFormatProvider
		/******** EXTERNAL OBJECT PUBLIC METHODS - END ********/
	}
}
