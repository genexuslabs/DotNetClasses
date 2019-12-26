using System;
using System.Collections.Generic;
using System.Text;

namespace GeneXus.Utils
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]

	public class GxRestPrefix
	{
        public static int UPLOAD_TIMEOUT = 10;
        public static string UPLOAD_PREFIX = "gxupload:";
        public static string ENCODED_PREFIX = "Encoded:";
	}
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	internal class GXFormData
	{
		public static string FORMDATA_REFERENCE = "gxformdataref:";
	}
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	internal class GxRestUtil
	{
		internal static bool IsUpload(string filename)
		{
			return (!string.IsNullOrEmpty(filename) && filename.StartsWith(GxRestPrefix.UPLOAD_PREFIX));
		}

		internal static string UploadPath(string filename)
		{
			if (!string.IsNullOrEmpty(filename) && filename.StartsWith(GxRestPrefix.UPLOAD_PREFIX))
			{
                string fkey =  filename.Substring(filename.IndexOf(':') + 1);
                if (CacheAPI.FilesCache.Contains(fkey)) {
                    return CacheAPI.FilesCache.Get(fkey);
                }
                else {
                    return string.Empty;
                }
			}
			else
				return string.Empty;
		}
	}

}
