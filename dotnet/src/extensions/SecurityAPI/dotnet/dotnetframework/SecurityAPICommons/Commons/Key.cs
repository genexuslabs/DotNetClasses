using System.Security;

namespace SecurityAPICommons.Commons
{
    [SecuritySafeCritical]
    public class Key : SecurityAPIObject
    {

        [SecuritySafeCritical]
        bool Load(string path) { return false; }

        [SecuritySafeCritical]
        bool LoadPKCS12(string path, string alias, string password) { return false; }
    }
}
