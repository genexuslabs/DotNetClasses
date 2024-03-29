using System;
using System.Security.Cryptography;
using System.Text;
using GeneXus.Cryptography.CryptoException;
using GeneXus.Utils;

namespace GeneXus.Cryptography.Hashing
{
	public class HashedKeyAlgorithmProvider : IGXHashing
	{
		private KeyedHashAlgorithm _keyedHash;

		public HashedKeyAlgorithmProvider(string algorithm)
		{
			if (GXUtil.IsWindowsPlatform)
			{
#pragma warning disable SYSLIB0045 // Type or member is obsolete
				_keyedHash = KeyedHashAlgorithm.Create(algorithm);
#pragma warning restore SYSLIB0045 // Type or member is obsolete

			}
			if (_keyedHash == null)
			{
				throw new AlgorithmNotSupportedException();
			}
		}

		public string ComputeHash(string text)
		{
			throw new NotSupportedException();
		}

		public string ComputeHashKey(string text, string key)
		{
			_keyedHash.Key = Encoding.ASCII.GetBytes(key);
			byte[] data = _keyedHash.ComputeHash(Constants.DEFAULT_ENCODING.GetBytes(text));
			return GXHashing.ToHexString(data);
		}

		public void Dispose()
		{
			_keyedHash.Dispose();
		}
	}
}
