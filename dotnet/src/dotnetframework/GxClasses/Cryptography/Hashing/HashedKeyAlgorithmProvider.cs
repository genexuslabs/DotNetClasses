using GeneXus.Cryptography.CryptoException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace GeneXus.Cryptography.Hashing
{
	public class HashedKeyAlgorithmProvider : IGXHashing
	{
		private KeyedHashAlgorithm _keyedHash;

		public HashedKeyAlgorithmProvider(string algorithm)
		{
			_keyedHash = KeyedHashAlgorithm.Create(algorithm);
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
