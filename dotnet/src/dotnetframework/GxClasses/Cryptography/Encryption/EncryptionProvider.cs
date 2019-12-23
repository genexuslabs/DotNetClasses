using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using GeneXus.Cryptography.CryptoException;
using System.IO;

namespace GeneXus.Cryptography.Encryption
{
    #region Symmetric Encryption
    public interface IGXSymmetricEncryption
    {
        
        string Encrypt(string text);

        string Decrypt(string text);

        string IV
        {
            get;
            set;
        }

        string Key
        {
            get;
            set;
        }

        int KeySize
        {
            get;
            set;
        }
        int BlockSize
        {
            get;
            set;
        }
    }

    public class NativeSymmetricEncryption : IGXSymmetricEncryption
    {
        SymmetricAlgorithm _symAlg;     
        byte[] _key;     
        byte[] _iv;      

        public NativeSymmetricEncryption(string algorithm)
            : this(algorithm, 0)
        {
        }

        public NativeSymmetricEncryption(string algorithm, int keySize)
        {
            switch (algorithm)
            {
                case "AES":
                    _symAlg = Aes.Create();
                    break;
                default:
                    _symAlg = SymmetricAlgorithm.Create(algorithm);
                    break;
            }
            if (_symAlg != null)
            {
                _symAlg.Padding = PaddingMode.PKCS7;
#pragma warning disable SCS0011 // CBC mode is weak
                _symAlg.Mode = CipherMode.CBC;                
#pragma warning restore SCS0011 // CBC mode is weak
                if (keySize > 0)
                {
                    _symAlg.KeySize = keySize;
                }
                _symAlg.GenerateKey();
                _symAlg.GenerateIV();
                _key = _symAlg.Key;
                _iv = _symAlg.IV;
            }
            else
            {
                throw new AlgorithmNotSupportedException();
            }           
        }

        #region IGXSymmetricEncryption Members


        public string Encrypt(string text)
        {
            string encrypted = string.Empty;
            MemoryStream sIn = new MemoryStream(Constants.DEFAULT_ENCODING.GetBytes(text));
            MemoryStream sOut = new MemoryStream();
            byte[] bin = new byte[100];
            long rdlen = 0;
            long totlen = sIn.Length;
            int len;
            try
            {

                CryptoStream encStream = new CryptoStream(sOut, _symAlg.CreateEncryptor(_key, _iv), CryptoStreamMode.Write);
                while (rdlen < totlen)
                {
                    len = sIn.Read(bin, 0, 100);
                    encStream.Write(bin, 0, len);
                    rdlen = rdlen + len;
                }
                encStream.Close();
            }
            catch (Exception e)
            {
                throw new EncryptionException(e);
            }
            sOut.Close();
            sIn.Close();
            encrypted = Convert.ToBase64String(sOut.ToArray());
            return encrypted;
        }



        public string Decrypt(string text)
        {
            MemoryStream sIn = new MemoryStream(Convert.FromBase64String(text));
            MemoryStream sOut = new MemoryStream();

            byte[] bin = new byte[100];
            int len;
            try
            {
                CryptoStream decStream = new CryptoStream(sIn, _symAlg.CreateDecryptor(_key, _iv), CryptoStreamMode.Read);
                while (true)
                {
                    len = decStream.Read(bin, 0, 100);
                    if (len == 0)
                        break;
                    sOut.Write(bin, 0, len);
                }
                decStream.Close();
            }
            catch (Exception e)
            {
                throw new EncryptionException(e);
            }
            string decrypted = Constants.DEFAULT_ENCODING.GetString(sOut.ToArray());
            sOut.Close();
            sIn.Close();
            return decrypted;
        }


        public string Key
        {
            get { return Convert.ToBase64String(_key); }
            set { _key = Convert.FromBase64String(value); }
        }
        public string IV
        {
            get { return Convert.ToBase64String(_iv); }
            set { _iv = Convert.FromBase64String(value); }
        }

        #endregion

        #region IGXSymmetricEncryption Members


        public int KeySize
        {
            get
            {
                return _symAlg.KeySize;
            }
            set
            {
                try
                {
                    _symAlg.KeySize = value;
                }
                catch (CryptographicException e)
                {
                    throw new EncryptionException(e);
                }
            }
        }

        public int BlockSize
        {
            get
            {
                return _symAlg.BlockSize;
            }
            set
            {
                _symAlg.BlockSize = value;
            }
        }

        #endregion
    }


    #endregion

    #region Asymmetric Encryption

    public interface IGXAsymmetricEncryption
    {
        
        string Encrypt(string text);

        string Decrypt(string text);
    }

    public class RSAEncryption : IGXAsymmetricEncryption
    {
        private X509Certificate2 _cert;
        public RSAEncryption(X509Certificate2 cert)
        {
            _cert = cert;
        }

        public string Encrypt(string text)
        {
            string encrypted = string.Empty;
            if (!AnyError())
            {
                try
                {
                    byte[] cipherbytes = Constants.DEFAULT_ENCODING.GetBytes(text);
                    RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)_cert.PublicKey.Key;
                    byte[] cipher = rsa.Encrypt(cipherbytes, false);
                    return Convert.ToBase64String(cipher);
                }
                catch (Exception e)
                {
                    throw new EncryptionException(e);
                }
            }
            return encrypted;
        }

        public string Decrypt(string text)
        {
            string encrypted = string.Empty;
            if (!AnyError())
            {
                if (_cert.HasPrivateKey)
                {
                    try
                    {
                        byte[] cipherbytes = Convert.FromBase64String(text);
                        RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)_cert.PrivateKey;
                        byte[] plainbytes = rsa.Decrypt(cipherbytes, false);
                        return Constants.DEFAULT_ENCODING.GetString(plainbytes);
                    }
                    catch (Exception e)
                    {
                        throw new EncryptionException(e);
                    }
                }
                else
                {
                    throw new PrivateKeyNotFoundException();
                }
            }
            return encrypted;
        }

        private bool AnyError()
        {
            if (_cert == null)
            {
                throw new CertificateNotLoadedException();
            }
            return false;
        }
    }

    #endregion
}
