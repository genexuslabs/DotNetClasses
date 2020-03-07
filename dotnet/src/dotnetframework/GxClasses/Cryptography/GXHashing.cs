using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using log4net;
using GeneXus.Cryptography.Hashing;
using GeneXus.Cryptography.CryptoException;

namespace GeneXus.Cryptography
{
	public class GXIncrementalHash : IDisposable
	{
		private int _lastError;
		private string _lastErrorDescription;
		private string _algorithm;
		private IGXIncrementalHashing _hash;
		private bool isDirty;
		// Compatibility with json in previous hash algorithm 
		public const string OPEN_BRACE = "[";
		public const string CLOSE_BRACE = "]";
		public const string SEPARATOR = ",";

		public GXIncrementalHash()
		{
			isDirty = true;
			_algorithm = Constants.DefaultHashAlgorithm;
			Initialize();
		}

		public GXIncrementalHash(String algorithm)
		{
			isDirty = true;
			_algorithm = algorithm;
			Initialize();
		}

		private void Initialize()
		{
			if (isDirty)
			{
				SetError(0);
				try
				{
					if (_hash != null)
					{
						_hash.Dispose();
						_hash = null;
					}				
					_hash = new HashAlgorithmProvider(_algorithm);
					isDirty = false;
				}
				catch (AlgorithmNotSupportedException)
				{
					SetError(2);
				}

			}
		}

		public void AppendRawData(string text) {
			_hash.AppendData(text);
		}

		public void InitData(string text) {
			_hash.AppendData(OPEN_BRACE+ text);			
		}

		public void AppendData(string text)
		{
			_hash.AppendData(SEPARATOR + text);
		}

		public string GetHashRaw()
		{
			return _hash.GetIncrementalHash();
		}

		public string GetHash() {
			_hash.AppendData(CLOSE_BRACE);			
			return _hash.GetIncrementalHash();
		}

		private void SetError(int errorCode)
		{
			_lastError = errorCode;
			switch (errorCode)
			{
				case 0:
					_lastErrorDescription = string.Empty;
					break;
				case 1:
					break;
				case 2:
					_lastErrorDescription = "Algorithm not supported";
					break;
				case 3:
					_lastErrorDescription = "Algorithm does not support Hashing with Key. Please use HMAC instead or remove the Key parameter";
					break;
				case 4:
					_lastErrorDescription = "Key must be specified";
					break;
				default:
					break;
			}
		}

		public string Algorithm
		{
			get { return _algorithm; }
			set
			{
				isDirty = true;
				_algorithm = value;
			}
		}

		public bool AnyError
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
					_hash.Dispose();
			}
		}
		#endregion

	}

	public class GXHashing : IDisposable
	{
		
		private int _lastError;
		private string _lastErrorDescription;
		private string _algorithm;
		private IGXHashing _hash;
		private bool isDirty;		
		public GXHashing()
		{
			isDirty = true;
			_algorithm = Constants.DefaultHashAlgorithm;
			Initialize();
		}

		private void Initialize()
		{
			if (isDirty)
			{
				SetError(0);				
				try
				{
					if (_hash != null)
					{
						_hash.Dispose();
						_hash = null;
					}
					if (_algorithm.ToUpper().StartsWith("HMAC") || _algorithm.ToUpper().StartsWith("MAC"))
					{
						_hash = new HashedKeyAlgorithmProvider(_algorithm);						
					}
					else 
					{
						_hash = new HashAlgorithmProvider(_algorithm);
					}
					isDirty = false;
				}
				catch (AlgorithmNotSupportedException)
				{
					SetError(2);
				}

			}
		}

		public string Compute(string text, string key = "")
		{
			Initialize();
			if (!AnyError)
			{
				bool keyHashAlgorithm = _hash is HashedKeyAlgorithmProvider;
				if (keyHashAlgorithm && string.IsNullOrEmpty(key))
				{
					SetError(4);
				}
				else
				{
					if (!keyHashAlgorithm && !string.IsNullOrEmpty(key))
					{
						SetError(3);
					}
					else {
						if (keyHashAlgorithm)
							return _hash.ComputeHashKey(text, key);
						else
							return _hash.ComputeHash(text);
					}
				}

			}
			return string.Empty;
		}

		private void SetError(int errorCode)
		{
			_lastError = errorCode;
			switch (errorCode)
			{
				case 0:
					_lastErrorDescription = string.Empty;
					break;
				case 1:
					break;
				case 2:
					_lastErrorDescription = "Algorithm not supported";
					break;
				case 3:
					_lastErrorDescription = "Algorithm does not support Hashing with Key. Please use HMAC instead or remove the Key parameter";
					break;
				case 4:
					_lastErrorDescription = "Key must be specified";
					break;
				default:
					break;
			}
		}

		public string Algorithm
		{
			get { return _algorithm; }
			set
			{
				isDirty = true;
				_algorithm = value;
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

		internal static string ComputeHash(string text, string algorithm)
		{
			string hash = string.Empty;
			using (HashAlgorithmProvider hashProvider = new HashAlgorithmProvider(algorithm))
			{
				hash = hashProvider.ComputeHash(text);
			}
			return hash;
		}

		public static string ToHexString(byte[] data)
		{
			StringBuilder sBuilder = new StringBuilder();

			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}
			
			return sBuilder.ToString();
		}

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
					_hash.Dispose();
			}
		}
#endregion
	}

}
