
using log4net;
using SecurityAPICommons.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;


namespace SecurityAPICommons.Keys
{
    /// <summary>
    /// Implements SymmetricKeyType enumerated
    /// </summary>
    [SecuritySafeCritical]
    public enum SymmetricKeyType
    {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        NONE, GENERICRANDOM
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
    /// <summary>
    /// Implements SymmetricKeyType associated functions
    /// </summary>
    [SecuritySafeCritical]
    public static class SymmetricKeyTypeUtils
    {
		private static readonly ILog logger = LogManager.GetLogger(typeof(SymmetricKeyTypeUtils));
		/// <summary>
		/// Mapping between string name and SymmetricKeyType enum representation
		/// </summary>
		/// <param name="symmetricKeyType">string symmetricKeyType</param>
		/// <param name="error">Error type for error management</param>
		/// <returns>SymmetricKeyType enum representation</returns>
		public static SymmetricKeyType getSymmetricKeyType(string symmetricKeyType, Error error)
        {
			logger.Debug("getSymmetricKeyType");
			if (error == null) return SymmetricKeyType.NONE;
			if(symmetricKeyType == null)
			{
				error.setError("SK001", "Unrecognized key type");
				logger.Error("Unrecognized key type");
				return SymmetricKeyType.NONE;
			}

			switch (symmetricKeyType.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Trim())
            {
                case "GENERICRANDOM":
                    return SymmetricKeyType.GENERICRANDOM;
                default:
                    error.setError("SK001", "Unrecognized key type");
					logger.Error("Unrecognized key type");
                    return SymmetricKeyType.NONE;
            }
        }
        /// <summary>
        /// Mapping between SymmetricKeyType enum representation and  string name
        /// </summary>
        /// <param name="symmetricKeyType">SymmetricKeyType enum, key type name</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>string value of key type in string</returns>
        public static string valueOf(SymmetricKeyType symmetricKeyType, Error error)
        {
			logger.Debug("valueOf");
			if (error == null) return "";
            switch (symmetricKeyType)
            {
                case SymmetricKeyType.GENERICRANDOM:
                    return "GENERICRANDOM";
                default:
                    error.setError("SK002", "Unrecognized key type");
					logger.Error("Unrecognized key type");
                    return "";
            }
        }
        /// <summary>
        /// Manage Enumerable enum 
        /// </summary>
        /// <typeparam name="SymmetricKeyType">SymmetricKeyType enum</typeparam>
        /// <returns>Enumerated values</returns>
        internal static IEnumerable<SymmetricKeyType> GetValues<SymmetricKeyType>()
        {
            return Enum.GetValues(typeof(SymmetricKeyType)).Cast<SymmetricKeyType>();
        }

    }
}
