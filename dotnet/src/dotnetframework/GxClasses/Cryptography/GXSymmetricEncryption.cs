using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using log4net;
using GeneXus.Cryptography.CryptoException;
using GeneXus.Cryptography.Encryption;

namespace GeneXus.Cryptography
{
    public class GXSymmetricEncryption
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string DEFAULT_SYM_ALGORITHM = "DES";
        private int _lastError;
        private string _lastErrorDescription;
        private IGXSymmetricEncryption _symAlg;     // Algorithm instance  
        private string _algorithm;
        string _key;     // key
        string _iv;      // initialization vector
        private bool isDirty;
        private int _keySize;
        private int _blockSize;

        public GXSymmetricEncryption()
        {
            isDirty = true;
            _algorithm = DEFAULT_SYM_ALGORITHM;
            _key = String.Empty;
            _iv = String.Empty;
        }

        private void Initialize()
        {
            if (isDirty)
            {
                // Supported algorithms = {Rijndael, DES, RC2, TripleDES, AES}
                SetError(0);

                try
                {
                    _symAlg = new NativeSymmetricEncryption(_algorithm);
                    if (!string.IsNullOrEmpty(_key))
                    {
                        _symAlg.Key = _key;
                    }
                    if (!string.IsNullOrEmpty(_iv))
                    {
                        _symAlg.IV = _iv;
                    }
                    if (_blockSize > 0)
                    {
                        _symAlg.BlockSize = _blockSize;
                    }
                    if (_keySize > 0)
                    {
                        SetKeySize();
                    }
                    isDirty = false;
                }
                catch (AlgorithmNotSupportedException)
                {
                    SetError(2);
                }

            }
        }

        public string Encrypt(string text)
        {
            Initialize();
            string encrypted = string.Empty;
            if (!AnyError)
            {
                try
                {
                    encrypted = _symAlg.Encrypt(text);
                }
                catch (EncryptionException e)
                {
                    SetError(2, e.Message);
                    _log.Error(e);
                }
            }
            return encrypted;
        }

        public string Decrypt(string text)
        {
            Initialize();
            string decrypted = string.Empty;
            if (!AnyError)
            {
                try
                {
                    decrypted = _symAlg.Decrypt(text);
                }
                catch (EncryptionException e)
                {
                    SetError(3, e.Message);
                    _log.Error(e);
                }
            }
            return decrypted;
        }

        public string Algorithm
        {
            get
            {
                return _algorithm;
            }
            set
            {
                isDirty = isDirty || value != _algorithm;
                _algorithm = value;
            }
        }

        public string Key
        {
            get
            {
                if (_symAlg != null)
                    return _symAlg.Key;
                else
                    return _key;
            }
            set
            {
                try
                {
                    Convert.FromBase64String(value);
                    isDirty = isDirty || (_symAlg == null) || value != _key;
                    _key = value;
                    if (_symAlg != null)
                        _symAlg.Key = value;
                    SetError(0);
                }
                catch (FormatException)
                {
                    SetError(4);
                }
            }
        }

        public string IV
        {
            get
            {
                if (_symAlg != null)
                    return _symAlg.IV;
                else
                    return _iv;
            }
            set
            {
                try
                {
                    Convert.FromBase64String(value);

                    isDirty = isDirty || (_symAlg == null) || value != _iv;
                    _iv = value;
                    if (_symAlg != null)
                        _symAlg.IV = value;
                    SetError(0);
                }
                catch (FormatException)
                {
                    SetError(5);
                }
            }
        }

        public int KeySize
        {
            get
            {
                if (_symAlg != null)
                    return _symAlg.KeySize;
                else
                    return _keySize;
            }
            set
            {
                isDirty = isDirty || (_symAlg == null) || value != _keySize;
                _keySize = value;
                SetKeySize();
            }
        }

        private void SetKeySize()
        {
            if (_symAlg != null)
            {
                try
                {
                    _symAlg.KeySize = _keySize;
                }
                catch (EncryptionException e)
                {
                    SetError(1, e.Message);
                }
            }
        }

        public int BlockSize
        {
            get
            {
                if (_symAlg != null)
                    return _symAlg.BlockSize;
                else
                    return _blockSize;
            }
            set
            {
                isDirty = isDirty || (_symAlg == null) || value != _blockSize;
                _blockSize = value;
                if (_symAlg != null)
                    _symAlg.BlockSize = _blockSize;
            }
        }

        private void SetError(int errorCode)
        {
            SetError(errorCode, string.Empty);
        }
        private void SetError(int errorCode, string errDsc)
        {
            _lastError = errorCode;
            switch (errorCode)
            {
                case 0:
                    _lastErrorDescription = string.Empty;
                    break;
                case 1:
                    _lastErrorDescription = "Unknown encryption error";
                    break;
                case 2:
                    _lastErrorDescription = "Algorithm not supported";
                    break;
                case 3:
                    _lastErrorDescription = "Unknown decryption error";
                    break;
                case 4:
                    _lastErrorDescription = "Key not valid base64 string";
                    break;
                case 5:
                    _lastErrorDescription = "IV not valid base64 string";
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(errDsc))
            {
                _lastErrorDescription = errDsc;
            }
        }

        private bool AnyError
        {
            get
            {
                return _lastError != 0;
            }
        }


        public int ErrCode
        {
            get
            {
                return _lastError;
            }
        }

        public string ErrDescription
        {
            get
            {
                return _lastErrorDescription;
            }
        }
    }
}
