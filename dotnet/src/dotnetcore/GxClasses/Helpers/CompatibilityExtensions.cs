using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace GxClasses.Helpers
{
	public class AssemblyLoader 
	{
		static AssemblyLoadContext _loadContext;
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
		/*Try to resolve by name when resolution fails. Keep compatibility with previous resolver*/
		private static Assembly LoadContext_Resolving(AssemblyLoadContext arg1, AssemblyName assemblyName)
		{
			string[] foundDlls = Directory.GetFileSystemEntries(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"{assemblyName.Name}.dll");
			if (foundDlls.Any())
			{
				return LoadContext.LoadFromAssemblyPath(foundDlls[0]);
			}
			else
				return null;
		}

		[Obsolete("LoadFromAssemblyName is deprecated, use static method LoadAssembly instead", false)]
		public Assembly LoadFromAssemblyName(AssemblyName assemblyName)
		{
			return LoadAssembly(assemblyName);
		}
		static AssemblyLoadContext LoadContext
		{
			get {
				if (_loadContext == null)
				{
					_loadContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
					_loadContext.Resolving += LoadContext_Resolving;
				}
				return _loadContext;
			}
		}
		public static Assembly LoadAssembly(AssemblyName assemblyName) {
			return LoadContext.LoadFromAssemblyName(assemblyName);
		}

	}
}

