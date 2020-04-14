using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeneXus.Encryption;

namespace GxEncryptCMD
{
	class Program
	{
		enum Action
		{
			Encript,
			Decrypt
		}

		const string ENCRIPT = "/e";
		const string DECRYPT = "/d";
		const string KEY = "/k:";
		const string REVERSE = "/r";

		static void Main(string[] args)
		{
			try
			{
				PrintHeader();

				CheckParams(args);

				Action action = Action.Encript;
				string key = string.Empty;
				string entry = string.Empty;
				bool reverse = false;
				foreach (string a in args)
				{
					if (a == ENCRIPT)
						action = Action.Encript;
					else if (a == DECRYPT)
						action = Action.Decrypt;
					else if (a.Contains(KEY))
						key = a.Substring(KEY.Length);
					else if (a == REVERSE)
						reverse = true;
					else
						entry = a;
				}

				bool useKey = !string.IsNullOrEmpty(key);
				string result = "";
				switch (action)
				{
					case Action.Encript:
						result = useKey ? CryptoImpl.Encrypt(entry, key, reverse) : CryptoImpl.Encrypt(entry, reverse);
						break;
					case Action.Decrypt:
						result = useKey ? CryptoImpl.Decrypt(entry, key, reverse) : CryptoImpl.Decrypt(entry, reverse);
						break;
					default:
						break;
				}

				Console.WriteLine(result);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.Message);
				Usage();
			}
		}

		static void Usage()
		{
			Console.WriteLine($"Usage: GxEncryptCMD /e|/d [/k:key] input");
			Console.WriteLine("");
			Console.WriteLine("/e		: Encrypt the input");
			Console.WriteLine("/d		: Decrypt the input");
			Console.WriteLine("/k		: Use this key for encryption/decryption");
			Console.WriteLine("input		: Text to encrypt/decrypt");

		}

		static void CheckParams(string[] args)
		{
			if (args.Length == 0)
				throw new Exception("No arguments received");
			if (args.Contains(ENCRIPT) && args.Contains(DECRYPT))
				throw new Exception("Action must be either Encrypt or Decrypt, never both");
			if (!args.Contains(ENCRIPT) && !args.Contains(DECRYPT))
				throw new Exception("Action must be either Encrypt or Decrypt, never both");
		}

		private static void PrintHeader()
		{
			Assembly assembly = Assembly.GetEntryAssembly();
			IEnumerable<Attribute> assemblyAtt = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute));
			IEnumerable<Attribute> assemblyCop = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute));
			string title = ((AssemblyTitleAttribute)assemblyAtt.First()).Title;
			string copyRight = ((AssemblyCopyrightAttribute)assemblyCop.First()).Copyright;
			string version = assembly.GetName().Version.ToString();

			Console.WriteLine($"{title} {version}");
			Console.WriteLine(copyRight);
			Console.WriteLine("");
		}
	}
}
