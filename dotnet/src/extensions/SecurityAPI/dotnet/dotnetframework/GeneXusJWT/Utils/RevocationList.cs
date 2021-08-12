
using System.Collections.Generic;
using System.Linq;
using System.Security;
using GenexusJWT.GenexusJWTUtils;
using SecurityAPICommons.Utils;
using SecurityAPICommons.Commons;

namespace GeneXusJWT.GenexusJWTUtils
{
    [SecuritySafeCritical]
    public class RevocationList : SecurityAPIObject
    {
        private List<string> revocationList;


        public RevocationList() : base()
        {
            revocationList = new List<string>();
        }

        /******** EXTERNAL OBJECT PUBLIC METHODS - BEGIN ********/
        public bool deleteFromRevocationList(string id)
        {
            for (int i = 0; i < revocationList.Count; i++)
            {
                if (SecurityUtils.compareStrings(id, revocationList.ElementAt(i)))
                {

                    revocationList.RemoveAt(i);
                    return true;
                }
            }
            this.error.setError("OP001", "The " + id + " id is not in the revocation list");
            return false;
        }

        public void addIDToRevocationList(string id)
        {
            revocationList.Add(id);
        }

        public bool isInRevocationList(string id)
        {
            for (int i = 0; i < revocationList.Count; i++)
            {
                if (SecurityUtils.compareStrings(id, revocationList.ElementAt(i)))
                {
                    return true;
                }
            }
            return false;
        }
        /******** EXTERNAL OBJECT PUBLIC METHODS - END ********/
    }
}
