using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace GeneXus.Cryptography.Signing.Standards
{
    public interface IPkcsSign
    {
        
        string Sign(string text);

        bool Verify(string signature, string text);

        X509Certificate2 Certificate
        {
            get;
            set;
        }

        bool ValidateCertificates
        {
            get;
            set;
        }
    }
}
