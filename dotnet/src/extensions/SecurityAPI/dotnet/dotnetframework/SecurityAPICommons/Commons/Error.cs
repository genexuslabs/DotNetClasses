
using System.Security;

namespace SecurityAPICommons.Commons
{
    /// <summary>
    /// Implements logging functions
    /// </summary>
    [SecuritySafeCritical]
#pragma warning disable CA1716 // Identifiers should not match keywords
	public class Error
#pragma warning restore CA1716 // Identifiers should not match keywords
	{
        private bool exists;
        private string code;

        /// <summary>
        /// Error.code getter
        /// </summary>
        public string SecApiCode
        {
            get { return code; }
        }
        private string description;

        /// <summary>
        /// Error constructor
        /// </summary>
        public Error()
        {
            this.exists = false;
            this.code = "";
            this.description = "";
        }

		public Error (string code, string description)
		{
			this.code = code;
			this.description = description;
			this.exists = true;
		}

        /// <summary>
        /// Gets error code
        /// </summary>
        /// <returns> Error code</returns>
        public string GetCode()
        {
            return code;
        }

        /// <summary>
        /// Gets description 
        /// </summary>
        /// <returns> Error description </returns>
        public string GetDescription()
        {
            return description;
        }

        /// <summary>
        /// Set error values
        /// </summary>
        /// <param name="errorCode">string error internal code</param>
        /// <param name="errorDescription">string error internal description</param>
        public void setError(string errorCode, string errorDescription)
        {
            this.exists = true;
            this.code = errorCode;
            this.description = errorDescription;

        }



        /// <summary>
        /// If an error exists
        /// </summary>
        /// <returns>1 if an error exists, 0 if not</returns>
        public bool existsError()
        {
            return this.exists;
        }


        /// <summary>
        /// Sets initial parameters
        /// </summary>
        public void cleanError()
        {
            this.exists = false;
            this.code = "";
            this.description = "";
        }

    }
}

