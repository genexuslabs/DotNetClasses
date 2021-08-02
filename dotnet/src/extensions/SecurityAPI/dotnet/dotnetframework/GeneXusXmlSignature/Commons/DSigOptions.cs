using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using GeneXusXmlSignature.GeneXusUtils;
using SecurityAPICommons.Commons;

namespace GeneXusXmlSignature.GeneXusCommons
{
    [SecuritySafeCritical]
    public class DSigOptions : SecurityAPIObject
    {

        [SecuritySafeCritical]
        private TransformsWrapper _dSigSignatureType;
        public string DSigSignatureType
        {
            get
            {
                return TransformsWrapperUtils.valueOf(this._dSigSignatureType, this.error);
            }
            set
            {
                _dSigSignatureType = TransformsWrapperUtils.getTransformsWrapper(value, this.error);
            }
        }
        [SecuritySafeCritical]
        private CanonicalizerWrapper _canonicalization;
        public string Canonicalization
        {
            get
            {
                return CanonicalizerWrapperUtils.valueOf(_canonicalization, this.error);
            }
            set
            {
                _canonicalization = CanonicalizerWrapperUtils.getCanonicalizerWrapper(value, this.error);
            }

        }
        [SecuritySafeCritical]
        private KeyInfoType _keyInfoType;
        public string KeyInfoType
        {
            get
            {
                return KeyInfoTypeUtils.valueOf(_keyInfoType, this.error);
            }
            set
            {
                _keyInfoType = KeyInfoTypeUtils.getKeyInfoType(value, this.error);
            }
        }
        [SecuritySafeCritical]
        private string _xmlSchemaPath;
        public string XmlSchemaPath
        {
            get
            {
                return _xmlSchemaPath;
            }
            set
            {
                _xmlSchemaPath = value;
            }
        }
        [SecuritySafeCritical]
        private string _identifierAttribute;
        public string IdentifierAttribute
        {
            get
            {
                return _identifierAttribute;
            }
            set
            {
                _identifierAttribute = value;
            }
        }

        [SecuritySafeCritical]
        public DSigOptions() : base()
        {
            this.XmlSchemaPath = null;
            this.DSigSignatureType = "ENVELOPED";
            this.Canonicalization = "C14n_OMIT_COMMENTS";
            this.KeyInfoType = "X509Certificate";
            this.IdentifierAttribute = null;
        }
    }
}
