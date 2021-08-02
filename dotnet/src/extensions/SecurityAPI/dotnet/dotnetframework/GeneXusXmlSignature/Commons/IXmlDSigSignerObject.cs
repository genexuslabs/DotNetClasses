using System;
using System.Collections.Generic;


using System.Security;
using SecurityAPICommons.Keys;

namespace GeneXusXmlSignature.GeneXusCommons
{
    /// <summary>
    /// IXMLDSigSignerObject interface for EO
    /// </summary>
    [SecuritySafeCritical]
    public interface IXmlDSigSignerObject
    {
        bool DoSignFile(string xmlFilePath, PrivateKeyManager key, CertificateX509 certificate, string outputPath, DSigOptions options);

        bool DoSignFileElement(string xmlFilePath, string xPath, PrivateKeyManager key, CertificateX509 certificate, string outputPath, DSigOptions options);

        string DoSign(string xmlInput, PrivateKeyManager key, CertificateX509 certificate, DSigOptions options);

        string DoSignElement(string xmlInput, string xPath, PrivateKeyManager key, CertificateX509 certificate, DSigOptions options);

        bool DoVerify(string xmlSigned, DSigOptions options);

        bool DoVerifyFile(string xmlFilePath, DSigOptions options);

        bool DoVerifyWithCert(string xmlSigned, CertificateX509 certificate, DSigOptions options);

        bool DoVerifyFileWithCert(string xmlFilePath, CertificateX509 certificate, DSigOptions options);
    }
}
