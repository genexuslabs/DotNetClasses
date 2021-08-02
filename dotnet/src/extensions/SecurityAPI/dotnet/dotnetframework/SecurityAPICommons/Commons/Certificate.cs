using System.Security;

namespace SecurityAPICommons.Commons
{
    [SecuritySafeCritical]
    public class Certificate : Key
    {
        [SecuritySafeCritical]
        bool FromBase64(string base64Data) { return false; }

        [SecuritySafeCritical]
        string ToBase64() { return string.Empty; }
    }
}
