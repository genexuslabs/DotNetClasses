using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;

namespace SecurityAPICommons.Commons
{
    /// <summary>
    /// SecurityObject interface
    /// </summary>
    [SecuritySafeCritical]
    public class SecurityAPIObject
    {
        [SecuritySafeCritical]
        public Error error;

        /// <summary>
        /// SecurityObject constructor
        /// </summary>
        [SecuritySafeCritical]
        public SecurityAPIObject()
        {
            error = new Error();

        }


        /// <summary>
        /// Returns Error 
        /// </summary>
        /// <returns>Error type</returns>
        [SecuritySafeCritical]
        public Error GetError()
        {
            return error;
        }


        /// <summary>
        /// Return true if an error exists for the object
        /// </summary>
        /// <returns>error exists boolean</returns>
        [SecuritySafeCritical]
        public bool HasError()
        {
            return error.existsError();
        }


        /// <summary>
        /// Return error code
        /// </summary>
        /// <returns>string error code</returns>
        [SecuritySafeCritical]
        public string GetErrorCode()
        {
            return error.GetCode();
        }

        /// <summary>
        /// Return error description
        /// </summary>
        /// <returns>error description string</returns>
        [SecuritySafeCritical]
        public string GetErrorDescription()
        {
            return error.GetDescription();
        }

    }
}

