
using System.Collections.Generic;
using System.Linq;
using System.Security;
using SecurityAPICommons.Utils;
using SecurityAPICommons.Commons;
using System;

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
            this.error.setError("REL01", String.Format("The id {0} is not in the revocation list", id));
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
