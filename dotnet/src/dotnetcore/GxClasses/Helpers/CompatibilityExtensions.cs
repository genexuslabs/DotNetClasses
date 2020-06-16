using Microsoft.Data.SqlClient;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using System.Runtime.Loader;
using System.Linq;
using System;
using System.Collections.Generic;
using log4net;
using GeneXus;
using Microsoft.Extensions.DependencyModel.Resolution;
using System.Collections.Concurrent;

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
	internal sealed class AssemblyResolver : IDisposable
	{
		static readonly ILog log = LogManager.GetLogger(typeof(AssemblyResolver));
		private readonly ICompilationAssemblyResolver assemblyResolver;
		private readonly DependencyContext dependencyContext;
		private readonly AssemblyLoadContext loadContext;

		public AssemblyResolver(string path)
		{
			GXLogging.Debug(log, "AssemblyResolver constructor for ", path);
			this.Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
			this.dependencyContext = DependencyContext.Load(this.Assembly);

			this.assemblyResolver = new CompositeCompilationAssemblyResolver
									(new ICompilationAssemblyResolver[]
			{
			
			new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)),
			new ReferenceAssemblyPathResolver(),
			new PackageCompilationAssemblyResolver()
			});

			this.loadContext = AssemblyLoadContext.GetLoadContext(this.Assembly);
			this.loadContext.Resolving += OnResolving;
		}

		public Assembly Assembly { get; }

		public void Dispose()
		{
			this.loadContext.Resolving -= this.OnResolving;
		}

		private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
		{
			GXLogging.Debug(log, "AssemblyResolver OnResolving for ", name.FullName);
			bool NamesMatch(RuntimeLibrary runtime)
			{
				return string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase);
			}

			RuntimeLibrary library =
				this.dependencyContext.RuntimeLibraries.FirstOrDefault(NamesMatch);
			if (library != null)
			{
				var wrapper = new CompilationLibrary(
					library.Type,
					library.Name,
					library.Version,
					library.Hash,
					library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
					library.Dependencies,
					library.Serviceable);

				var assemblies = new List<string>();
				this.assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);
				if (assemblies.Count > 0)
				{
					return this.loadContext.LoadFromAssemblyPath(assemblies[0]);
				}
			}

			return null;
		}
	}
	public class AssemblyLoader : AssemblyLoadContext
	{
		static ConcurrentDictionary<string, Assembly> loadedAssemblies = new ConcurrentDictionary<string, Assembly>(); 
		private string folderPath;

		public AssemblyLoader(string folderPath)
		{
			this.folderPath = folderPath;
		}
		public Type GetType(string typeFullName)
		{
			string typeName = typeFullName.Split(',').First();
			string assemblyName = typeFullName.Substring(typeName.Length + 1);
			AssemblyName assName = new AssemblyName(assemblyName);
			return LoadFromAssemblyName(assName).GetType(typeName);
		}
		protected override Assembly Load(AssemblyName assemblyName)
		{
			Assembly ass;

			//Assemblies with a different case of the Name are considered the same assembly.
			string assemblyLowerName = assemblyName.Name.ToLower();

			loadedAssemblies.TryGetValue(assemblyLowerName, out ass);
			if (ass != null)
				return ass;

			try
			{
				var deps = DependencyContext.Default;
				var res = deps.CompileLibraries.Where(d => d.Name.Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase)).ToList();
				if (res.Count > 0)
				{
					ass = Assembly.Load(new AssemblyName(res.First().Name));
					loadedAssemblies[assemblyLowerName] = ass;
					return ass;
				}
				else
				{
					var runtimeLibs = deps.RuntimeLibraries.Where(d => d.Name.Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase)).ToList();
					if (runtimeLibs.Count > 0)
					{
						ass = Assembly.Load(new AssemblyName(runtimeLibs.First().Name));
						loadedAssemblies[assemblyLowerName] = ass;
						return ass;
					}
					else
					{
						var foundDlls = Directory.GetFileSystemEntries(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), assemblyName.Name + ".dll", SearchOption.AllDirectories);
						if (foundDlls.Any())
						{
							ass = LoadFromAssemblyPath(foundDlls[0]);
							loadedAssemblies[assemblyLowerName] = ass;
							return ass;
						}

						return Assembly.Load(assemblyName);
					}
				}
			}
			catch (FileNotFoundException) //>ExecutingAssembly>.deps.json does not exist (p.e. deployed procs command line)
			{
				var assemblyPath = Path.Combine(folderPath, assemblyName.Name + ".dll");
				if (File.Exists(assemblyPath))
				{
					ass = Default.LoadFromAssemblyPath(assemblyPath);
					if (ass != null)
					{
						loadedAssemblies[assemblyLowerName] = ass;
						return ass;
					}
				}
				return null;
			}
		}
	}
}

