using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;

namespace GeneXusCryptography.Commons
{
    [SecuritySafeCritical]
    public interface IHmacObject
    {
        string calculate(string plainText, string password, string algorithm);

        bool verify(string plainText, string password, string mac, string algorithm);
    }
}
