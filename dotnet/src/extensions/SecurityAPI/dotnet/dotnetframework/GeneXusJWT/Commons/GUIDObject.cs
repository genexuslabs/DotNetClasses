
using SecurityAPICommons.Commons;
using System;
using System.Security;

namespace GeneXusJWT.GenexusComons
{
    [SecuritySafeCritical]
    public abstract class GUIDObject : SecurityAPIObject

    {
        [Obsolete("GUID object is deprecated. USe Genexus GUID data type instead https://wiki.genexus.com/commwiki/servlet/wiki?31772,GUID+data+type")]
        public abstract string Generate();
    }
}
