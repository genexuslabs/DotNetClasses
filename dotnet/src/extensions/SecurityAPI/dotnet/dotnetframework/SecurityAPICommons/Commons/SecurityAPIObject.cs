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


        
#pragma warning disable CA1051 // Do not declare visible instance fields
		private Error _error;
#pragma warning restore CA1051 // Do not declare visible instance fields

		
		public Error error
		{
			get { return _error; }
			set { _error = value; }
		}

		/// <summary>
		/// SecurityObject constructor
		/// </summary>
		[SecuritySafeCritical]
        public SecurityAPIObject()
        {
            _error = new Error();

        }


        /// <summary>
        /// Returns Error 
        /// </summary>
        /// <returns>Error type</returns>
        [SecuritySafeCritical]
        public Error GetError()
        {
            return _error;
        }


        /// <summary>
        /// Return true if an error exists for the object
        /// </summary>
        /// <returns>error exists boolean</returns>
        [SecuritySafeCritical]
        public bool HasError()
        {
            return _error.existsError();
        }


        /// <summary>
        /// Return error code
        /// </summary>
        /// <returns>string error code</returns>
        [SecuritySafeCritical]
        public string GetErrorCode()
        {
            return _error.GetCode();
        }

        /// <summary>
        /// Return error description
        /// </summary>
        /// <returns>error description string</returns>
        [SecuritySafeCritical]
        public string GetErrorDescription()
        {
            return _error.GetDescription();
        }

    }
}

