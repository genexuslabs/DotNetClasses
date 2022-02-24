using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Data.SqlClient;

namespace GxClasses.Helpers
{

	public static class NetCoreExtensionMethods
    {
		public static void Close(this StreamReader sr)
		{
			if (sr != null)
				sr.Dispose();
		}
		public static void Close(this SqlDataReader sr)
		{
			if (sr != null)
				sr.Dispose();
		}

		public static Assembly Load(this Assembly ass, string name)
		{
			AssemblyName assName = new AssemblyName(name);
			return Assembly.Load(assName);
		}
	}
	public class AssemblyLoader : AssemblyLoadContext
	{
		public AssemblyLoader(string folderPath)
		{
		}
		public static Assembly GetAssembly(string assemblyName)
		{
			AssemblyLoadContext context = GetLoadContext(Assembly.GetExecutingAssembly());
			return  context.LoadFromAssemblyName(new AssemblyName(assemblyName));
		}
		public static Type GetType(string typeFullName)
		{
			string typeName = typeFullName.Split(',').First();
			string assemblyFullName = typeFullName.Substring(typeName.Length + 1);
			string assemblyName = assemblyFullName.Split(',').First();
			Assembly assembly = GetAssembly(assemblyName);
			return assembly.GetType(typeName);
		}
	}
}

