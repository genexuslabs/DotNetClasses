
using Org.BouncyCastle.Utilities.Encoders;
using SecurityAPICommons.Commons;
using System;
using System.IO;
using System.Security;

namespace SecurityAPICommons.Utils
{
    [SecuritySafeCritical]
    public static class SecurityUtils
    {

        /// <summary>
        /// Compares two strings ignoring casing
        /// </summary>
        /// <param name="one">string to compare</param>
        /// <param name="two">string to compare</param>
        /// <returns>true if both strings are equal ignoring casing</returns>
        [SecuritySafeCritical]
        public static bool compareStrings(string one, string two)
        {
            if(one != null && two != null)
            {
                return string.Compare(one, two, true, System.Globalization.CultureInfo.InvariantCulture) == 0;
            }
            else
            {
                return false;
            }
            
        }

        /// <summary>
        /// Verifies if the file has some extension type
        /// </summary>
        /// <param name="path">path to the file</param>
        /// <param name="ext">extension of the file</param>
        /// <returns>true if the file has the extension</returns>
        [SecuritySafeCritical]
        public static bool extensionIs(string path, string ext)
        {
            return string.Compare(getFileExtension(path), ext, true, System.Globalization.CultureInfo.InvariantCulture) == 0;
        }
        /// <summary>
        /// Gets a file extension from the file's path
        /// </summary>
        /// <param name="path">path to the file</param>
        /// <returns>file extension</returns>
        [SecuritySafeCritical]
        public static string getFileExtension(string path)
        {

            string fileName =  Path.GetFileName(path);
            string extension;
			try
			{
                extension = Path.GetExtension(fileName);
            }
             catch(Exception)
			{
                extension = "";
			}

            return extension;
        }

		[SecuritySafeCritical]
		public static byte[] GetHexa(string hex, string code, Error error)
		{
			if (error == null) return null;
			byte[] output;
			try
			{
				output = Hex.Decode(hex);
			}
			catch (Exception e)			{
				error.setError(code, e.Message);
				return null;
			}
			return output;
		}
	}
}
