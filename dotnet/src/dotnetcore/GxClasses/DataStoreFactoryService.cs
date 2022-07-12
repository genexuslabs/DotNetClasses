using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Data;

namespace GxClasses
{
	internal class DataStoreFactoryService
	{
		private static Dictionary<string, Func<GxDataRecord>> m_Providers = new Dictionary<string, Func<GxDataRecord>>();

		internal static GxDataRecord CreateDataRecord(string d)
		{
			if (m_Providers.TryGetValue(d, out Func<GxDataRecord> func))
				return func();
			return null;
		}
		public static bool AddProvider(string d, Func<GxDataRecord> func)
		{
			m_Providers[d] = func;
			return true;
		}

		public static void LoadProvidersFromDirectory(string directory)
		{
			foreach (string dllFile in Directory.GetFiles(Path.Combine(Assembly.GetExecutingAssembly().Location, directory)))
			{
				if (File.Exists(dllFile))
				{
					LoadFromAssembly(Assembly.LoadFrom(dllFile));
				}
			}
		}

		public static void LoadFromAssembly(Assembly ass )
		{
			foreach (var att in ass.GetCustomAttributes<GxDataRecordAttribute>())
			{
				AddProvider(att.Dbms, att.GetDataRecordClassFunction());
			}
		}

	}

	public sealed class GxDataRecordAttribute : Attribute
	{
		private Type dataRecordClass;

		public string Dbms { get; set; }
		public GxDataRecordAttribute(string dbms, Type dataRecordType)
		{
			dataRecordClass = dataRecordType;
			Dbms = dbms;
		}


		public Func<GxDataRecord> GetDataRecordClassFunction()
		{
			return () =>  Activator.CreateInstance(dataRecordClass) as GxDataRecord;
		}

	}
}
