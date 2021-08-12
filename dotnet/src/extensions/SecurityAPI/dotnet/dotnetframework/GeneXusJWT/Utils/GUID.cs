using GeneXusJWT.GenexusComons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using SecurityAPICommons.Commons;

namespace GeneXusJWT.GenexusJWTUtils
{

    /*****DEPRECATED OBJECT SINCE GeneXus 16 upgrade 11******/

    [SecuritySafeCritical]
    public class GUID : GUIDObject
    {


        [SecuritySafeCritical]
        public GUID() : base()
        {

        }

        [Obsolete("GUID object is deprecated. USe Genexus GUID data type instead https://wiki.genexus.com/commwiki/servlet/wiki?31772,GUID+data+type")]
        [SecuritySafeCritical]
        public override string Generate()
        {
            return System.Guid.NewGuid().ToString();
        }
    }
}
