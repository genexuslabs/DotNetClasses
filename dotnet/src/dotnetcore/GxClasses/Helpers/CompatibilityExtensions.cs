using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace GxClasses.Helpers
{
	public class AssemblyLoader 
	{
		internal static Type GetType(string typeFullName)
		{
			string typeName = typeFullName.Split(',').First();
			string assemblyName = typeFullName.Substring(typeName.Length + 1);
			AssemblyName assName = new AssemblyName(assemblyName);
			return LoadContext.LoadFromAssemblyName(assName).GetType(typeName);
		}
		static AssemblyLoadContext LoadContext
		{
			get => AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
		}
		internal static Assembly LoadAssembly(AssemblyName assemblyName) {
			return LoadContext.LoadFromAssemblyName(assemblyName);
		}

	}
}

