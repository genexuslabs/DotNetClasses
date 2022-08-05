
using Org.BouncyCastle.Utilities.Encoders;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using System;
using System.IO;
using System.Security;

namespace SecurityAPICommons.Utils
{
	[SecuritySafeCritical]
	public static class SecurityUtils
	{

		/// <summary>
		/// Compares two strings ignoring casing
		/// </summary>
		/// <param name="one">string to compare</param>
		/// <param name="two">string to compare</param>
		/// <returns>true if both strings are equal ignoring casing</returns>
		[SecuritySafeCritical]
		public static bool compareStrings(string one, string two)
		{
			if (one != null && two != null)
			{
				return string.Compare(one, two, true, System.Globalization.CultureInfo.InvariantCulture) == 0;
			}
			else
			{
				return false;
			}

		}

		[SecuritySafeCritical]
		public static byte[] getFileBytes(string path, Error error)
		{
			byte[] aux = null;
			try
			{
				aux = System.IO.File.ReadAllBytes(path);
			}
			catch (Exception e)
			{
				if (error != null)
				{
					error.setError("SU001", e.Message);
				}
			}
			return aux;
		}

		[SecuritySafeCritical]
		public static Stream getFileStream(string pathInput, Error error)
		{
			Stream aux = null;
			try
			{
				aux = new FileStream(pathInput, FileMode.Open);
			}
			catch (Exception e)
			{
				if (error != null)
				{
					error.setError("SU002", e.Message);
				}

			}
			return aux;
		}

		/// <summary>
		/// Verifies if the file has some extension type
		/// </summary>
		/// <param name="path">path to the file</param>
		/// <param name="ext">extension of the file</param>
		/// <returns>true if the file has the extension</returns>
		[SecuritySafeCritical]
		public static bool extensionIs(string path, string ext)
		{
			return string.Compare(getFileExtension(path), ext, true, System.Globalization.CultureInfo.InvariantCulture) == 0;
		}
		/// <summary>
		/// Gets a file extension from the file's path
		/// </summary>
		/// <param name="path">path to the file</param>
		/// <returns>file extension</returns>
		[SecuritySafeCritical]
		public static string getFileExtension(string path)
		{

			string fileName = Path.GetFileName(path);
			string extension;
			try
			{
				extension = Path.GetExtension(fileName);
			}
			catch (Exception)
			{
				extension = "";
			}

			return extension;
		}

		[SecuritySafeCritical]
		public static byte[] HexaToByte(string hex, Error error)
		{
			if (error == null) return null;
			byte[] output;
			try
			{
				output = Hex.Decode(hex);
			}
			catch (Exception e)
			{
				error.setError("SU004", e.Message);
				return null;
			}
			return output;
		}

		public static Stream StringToStream(String input, Error error)
		{
			EncodingUtil eu = new EncodingUtil();
			byte[] inputText = eu.getBytes(input);
			if (eu.HasError())
			{
				error = eu.GetError();
				return null;
			}
			else
			{
				try
				{
					using (Stream inputStream = new MemoryStream(inputText))
					{
						return inputStream;
					}
				}
				catch (Exception e)
				{
					error.setError("SU003", e.Message);
					return null;
				}

			}
		}

		public static bool validateStringInput(string name, string value, Error error)
		{
			if (value == null)
			{
				error.setError("SU005", String.Format("The parameter %s cannot be empty", name));
				return false;
			}
			if (value.Length == 0)
			{
				error.setError("SU006", String.Format("The parameter %s cannot be empty", name));
				return false;
			}
			return true;
		}

		public static bool validateObjectInput(string name, Object value, Error error)
		{
			if (value == null)
			{
				error.setError("SU007", String.Format("The parameter %a cannot be empty", name));
				return false;
			}
			return true;
		}

	}
}
