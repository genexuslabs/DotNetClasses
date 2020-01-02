using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Jayrock.Json;
using GeneXus.Utils;

namespace Artech.Genexus.SDAPI
{
    public static class Utils
    {
        public static bool TryGetFile(ref string filename)
        {
            string file = filename;
#if NETCOREAPP1_1
			string baseDirectory = FileUtil.GetStartupDirectory();
#else
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
#endif

			filename = Path.Combine(baseDirectory, file);
            bool found = File.Exists(filename);
            if (!found) /* If started from bin directory, file may be on parent 'web' folder. */
            {
                DirectoryInfo dInfo = new DirectoryInfo(baseDirectory);
                filename = Path.Combine(dInfo.Parent.FullName, file);
                found = File.Exists(filename);
            }
            return found;
        }
        public static JObject FromJSonString(string s)
        {
            JObject _jsonArr = null;
            if (!string.IsNullOrEmpty(s))
            {
                StringReader sr = new StringReader(s);
                JsonTextReader tr = new JsonTextReader(sr);
                _jsonArr = (JObject)(tr.DeserializeNext());
            }
            return _jsonArr;
        }
    }
}
