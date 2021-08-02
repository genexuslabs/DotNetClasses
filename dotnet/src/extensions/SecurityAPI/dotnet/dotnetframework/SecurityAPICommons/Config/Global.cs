using System.Runtime.CompilerServices;



namespace SecurityAPICommons.Config
{

    public static class Global
    {
        private static string global_encoding = "UTF_8";
        public static string GLOBAL_ENCODING
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
        public static string GLOBAL_KEY_COONTAINER_NAME
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
