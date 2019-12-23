using System.Security.Cryptography;

namespace GeneXus.Encryption
{
	internal class AesCryptoServiceProvider
	{
		Aes _aes;
		public AesCryptoServiceProvider()
		{
			_aes = Aes.Create();
		}

		public ICryptoTransform CreateEncryptor() {
			return _aes.CreateEncryptor();
		}

		public ICryptoTransform CreateDecryptor()
		{
			return _aes.CreateDecryptor();
		}
		
		public CipherMode Mode
		{
			get { return _aes.Mode; }
			set { _aes.Mode = value; }
		}

		public PaddingMode Padding
		{
			get { return _aes.Padding; }
			set { _aes.Padding = value; }
		}

		public byte[] Key
		{
			get { return _aes.Key; }
			set { _aes.Key = value; }
		}

		public void Clear()
		{
			_aes.Dispose();
		}

	}
}