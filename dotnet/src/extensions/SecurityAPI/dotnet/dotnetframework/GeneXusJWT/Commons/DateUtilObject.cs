using SecurityAPICommons.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;

namespace GeneXusJWT.GenexusComons
{
    [SecuritySafeCritical]
    public abstract class DateUtilObject : SecurityAPIObject
    {
        [Obsolete("DateUtil object is deprecated. Use GeneXus DateTime data type instead https://wiki.genexus.com/commwiki/servlet/wiki?7370,DateTime%20data%20type")]
        public abstract string GetCurrentDate();
        [Obsolete("DateUtil object is deprecated. Use GeneXus DateTime data type instead https://wiki.genexus.com/commwiki/servlet/wiki?7370,DateTime%20data%20type")]
        public abstract string CurrentPlusSeconds(long seconds);
        [Obsolete("DateUtil object is deprecated. Use GeneXus DateTime data type instead https://wiki.genexus.com/commwiki/servlet/wiki?7370,DateTime%20data%20type")]
        public abstract string CurrentMinusSeconds(long seconds);
    }
}
