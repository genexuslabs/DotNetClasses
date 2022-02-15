using System;
using System.Security.Cryptography;
using GeneXus.Cryptography.CryptoException;
using GeneXus.Utils;

namespace GeneXus.Cryptography.Hashing
{
    public interface IGXHashing : IDisposable
    {
        string ComputeHash(string text);
		string ComputeHashKey(string key, string text);

	}
	public interface IGXIncrementalHashing : IDisposable
	{
	
		void AppendData(string text);
		string GetIncrementalHash();
		HashAlgorithm GetAlgo();
	}

	public class HashAlgorithmProvider : IGXHashing, IGXIncrementalHashing
	{
        private HashAlgorithm _hash;

		public HashAlgorithm GetAlgo() {
			return _hash;
		}
        
        public HashAlgorithmProvider(string algorithm)
        {
			switch (algorithm)
			{
				//FIPS compatible implementations
				case "SHA256":
					_hash = new SHA256CryptoServiceProvider();
					break;
				case "SHA384":
					_hash = new SHA384CryptoServiceProvider();
					break;
				case "SHA512":
					_hash = new SHA512CryptoServiceProvider();
					break;
				default:
					_hash = HashAlgorithm.Create(algorithm);
					break;
			}

			if (_hash == null)
            {
                throw new AlgorithmNotSupportedException();
            }
        }

#region IGXHashing Members

        public string ComputeHash(string text)
		{
			byte[] bin = Constants.DEFAULT_ENCODING.GetBytes(text);			
			return GXHashing.ToHexString(_hash.ComputeHash(bin));
		}
		
		public string ComputeHashKey(string key, string text)
		{
			throw new NotSupportedException();
		}

		public void AppendData(string text) {
			byte[] bin = Constants.DEFAULT_ENCODING.GetBytes(text);
			_hash.TransformBlock(bin, 0, bin.Length,null,0);
		}
		
		public string GetIncrementalHash() {
			byte[] bytes = Constants.DEFAULT_ENCODING.GetBytes("");
			_hash.TransformFinalBlock(bytes, 0, 0);
			return GXHashing.ToHexString(_hash.Hash);			
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
				if (_hash != null)
					_hash.Clear();
            }
        }

#endregion
    }
}
