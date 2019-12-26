using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace Genexus.RegistryUtilities
{
	public static class RegistryHelper
	{
		internal static UIntPtr HKEY_LOCAL_MACHINE = (UIntPtr)0x80000002;
		internal const int ERROR_SUCCESS = 0;
		public static uint KEY_DEFAULT = 0;
		public static uint KEY_WOW64_64KEY = 256; //Access a 64-bit key from either a 32-bit or 64-bit application.Windows 2000:  This flag is not supported.
		public static uint KEY_WOW64_32KEY = 512; //Access a 32-bit key from either a 32-bit or 64-bit application.Windows 2000:  This flag is not supported.
		internal const int KEY_QUERY_VALUE = 1;
		internal const int KEY_ENUMERATE_SUB_KEYS = 8;
		internal const uint REG_SZ = 1;
		internal const uint REG_EXPAND_SZ = 2;
		internal const uint REG_BINARY = 3;
		internal const uint REG_DWORD_LITTLE_ENDIAN = 4;
		internal const uint REG_DWORD_BIG_ENDIAN = 5;
		internal const uint REG_MULTI_SZ = 7;
		internal const uint REG_QWORD_LITTLE_ENDIAN = 11;

	
		private static String ArrayToString(byte[] array, ref int index)
		{
			int len = 0;
			while ((index + len + 1) < array.Length &&
				  (array[index + len] != 0 ||
				   array[index + len + 1] != 0))
			{
				len += 2;
			}
			StringBuilder builder = new StringBuilder(len / 2);
			len = 0;
			while ((index + len + 1) < array.Length &&
				  (array[index + len] != 0 ||
				   array[index + len + 1] != 0))
			{
				builder.Append((char)(((int)array[index + len]) |
									  (((int)array[index + len + 1])
											<< 8)));
				len += 2;
			}
			index += len + 2;
			return builder.ToString();
		}

		private static String[] ArrayToStringArray(byte[] array)
		{
			ArrayList list = new ArrayList();
			String value;
			int index = 0;
			for (; ; )
			{
				value = ArrayToString(array, ref index);
				if (value.Length == 0)
					break;
				list.Add(value);
			}
			return (String[])(list.ToArray(typeof(String)));
		}

		public static bool IsWow64Registry()
		{
			using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Microsoft SQL Server"))
			{
				return rk != null;
			}
		}

		public static object GetValue(string subKey, string key)
		{
			object value = null;
			using (RegistryKey kr = Registry.LocalMachine.OpenSubKey(subKey))
			{
				if (kr != null)
				{
					value = kr.GetValue(key);
					kr.Close();
				}
			}
			return value;
		}

	
		private static String ArrayToString(char[] array)
		{
			int index = 0;
			while (index < array.Length && array[index] != '\0')
				++index;
			return new String(array, 0, index);
		}


	}
}
