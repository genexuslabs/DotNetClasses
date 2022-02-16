using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;

namespace GeneXusCryptography.Commons
{
    [SecuritySafeCritical]
    public interface ICmacObject
    {
        string calculate(string plainText, string key, string algorithm, int macSize);
        bool verify(string plainText, string key, string mac, string algorithm, int macSize);
    }
}
