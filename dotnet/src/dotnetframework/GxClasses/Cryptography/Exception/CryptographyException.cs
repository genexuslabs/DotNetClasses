using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneXus.Cryptography.CryptoException
{

    public class CertificateNotLoadedException : System.Exception
    {
    }

    public class PrivateKeyNotFoundException : System.Exception
    {
    }

    public class EncryptionException : System.Exception
    {
        public EncryptionException(System.Exception inner) : base(inner.Message, inner) { }
    }
    public class DigitalSignException : System.Exception
    {
        public DigitalSignException(System.Exception inner) : base(inner.Message, inner) { }
    }

    public class InvalidSignatureException : System.Exception
    {
        public InvalidSignatureException(System.Exception inner) : base(inner.Message, inner) { }
        public InvalidSignatureException(string msg) : base(msg) { }
    }

    public class AlgorithmNotSupportedException : System.Exception
    {
        public AlgorithmNotSupportedException() : base("") { }
        public AlgorithmNotSupportedException(string msg, System.Exception inner) : base(inner.Message, inner) { }
        public AlgorithmNotSupportedException(string msg) : base(msg) { }
    }

}
