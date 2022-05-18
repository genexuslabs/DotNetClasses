using System;
using System.Collections;
using GeneXus.Utils;

namespace GX
{
	public class GXRuntime
	{
		public static short Environment
		{
			get
			{
				return 1; 
			}
		}
		public static int ExitCode { get; set; }
		public static bool HasEnvironmentVariable(string varName)
		{
			if (string.IsNullOrEmpty(varName))
				return false;
			else
			{
				string value = System.Environment.GetEnvironmentVariable(varName);
				if (value == null)
					return false;
				else
					return true;
			}
		}
		public static GXProperties GetEnvironmentVariables()
		{
			IDictionary variables = System.Environment.GetEnvironmentVariables();
			GXProperties gXProperties = new GXProperties();
			foreach (DictionaryEntry dentry in variables)
			{
				string key = (String)dentry.Key;
				string value = (String)dentry.Value;
				gXProperties.Add(key, value);
				
			}
			return (gXProperties);
		}
		public static string GetEnvironmentVariable(string varName)
		{
			if (string.IsNullOrEmpty(varName))
				return string.Empty;
			else
			{
				string value = System.Environment.GetEnvironmentVariable(varName);
				if (value == null)
					return string.Empty;
				else
					return value;
			}
		}
	}
}
