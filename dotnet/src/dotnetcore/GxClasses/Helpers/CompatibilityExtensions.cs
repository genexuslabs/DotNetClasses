using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace GxClasses.Helpers
{
	public class AssemblyLoader 
	{
		public AssemblyLoader(string path)
		{

		}
		internal static Type GetType(string typeFullName)
		{
			string typeName = typeFullName.Split(',').First();
			string assemblyName = typeFullName.Substring(typeName.Length + 1);
			AssemblyName assName = new AssemblyName(assemblyName);
			return LoadContext.LoadFromAssemblyName(assName).GetType(typeName);
		}
		[Obsolete("LoadFromAssemblyName is deprecated, use static method LoadAssembly instead", false)]
		public Assembly LoadFromAssemblyName(AssemblyName assemblyName)
		{
			return LoadAssembly(assemblyName);
		}
		static AssemblyLoadContext LoadContext
		{
			get => AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
		}
		public static Assembly LoadAssembly(AssemblyName assemblyName) {
			return LoadContext.LoadFromAssemblyName(assemblyName);
		}

	}
}

