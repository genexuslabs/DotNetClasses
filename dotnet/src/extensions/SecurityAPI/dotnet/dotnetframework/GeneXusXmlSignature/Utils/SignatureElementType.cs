﻿
using System.Security;
using SecurityAPICommons.Commons;

namespace GeneXusXmlSignature.GeneXusUtils
{
    [SecuritySafeCritical]
    public enum SignatureElementType
    {
        id, path, document
    }

    [SecuritySafeCritical]
    public class SignatureElementTypeUtils
    {
        public static string ValueOf(SignatureElementType signatureElementType, Error error)
        {
            switch (signatureElementType)
            {
                case SignatureElementType.id:
                    return "id";
                case SignatureElementType.path:
                    return "path";
                case SignatureElementType.document:
                    return "document";
                default:
                    error.setError("SE001", "Unrecognized SignatureElementType");
                    return "";
            }
        }
    }
}
