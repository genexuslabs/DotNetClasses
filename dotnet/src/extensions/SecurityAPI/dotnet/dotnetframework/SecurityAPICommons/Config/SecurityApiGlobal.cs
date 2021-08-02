using System.Runtime.CompilerServices;



namespace SecurityAPICommons.Config
{

    public static class SecurityApiGlobal
    {
        private static string global_encoding = "UTF_8";
        public static string GLOBALENCODING
        {
            get
            {
                if (global_encoding == null)
                {
                    return "UTF_8";
                }
                return global_encoding;
            }
            set
            {
                global_encoding = value;
            }
        }
        private static string global_keyContainerName = "";
        public static string GLOBALKEYCOONTAINERNAME
        {
            get
            {
                if (global_keyContainerName == null)
                {
                    return "UTF_8";
                }
                return global_keyContainerName;
            }
            set
            {
                global_keyContainerName = value;
            }
        }


    }
}
