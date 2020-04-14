using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using Mono.Cecil;

namespace ChangePublicKeyToken
{
	public class Options
	{
		[Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
		public bool Verbose { get; set; }
		[Option('a', "assembly", Required = true, HelpText = "The name of the assembly that changed its strong name.")]
		public string Assembly { get; set; }

		[Option('d', "directory", Required = true, HelpText = "Specify a directory to search for assemblies that reference the assembly which changed its strong name. These assemblies will be modified to link the new assembly strong name.")]
		public string TargetDirectory { get; set; }

		[Usage()]
		public static IEnumerable<Example> Examples
		{
			get
			{
				return new List<Example>() {new Example("Given an assembly A with a public key token B, it searches for all the assemblies in a directory that reference A with public key token Z (different from P) and replaces the public key token of the reference by P", UnParserSettings.WithGroupSwitchesOnly(), new Options { Assembly = @"C:\DotNetClasses\GxClasses.dll", TargetDirectory=@"C:\Model\KB\Web\bin"})};
			}
		}
		
	}

	public class UpdateAssemblyReference
	{
		static void Main(string[] args)
		{
			var commandLineParser = new Parser(x =>
			{
				x.HelpWriter = Console.Out;
				x.IgnoreUnknownArguments = false;
				x.CaseSensitive = false;
				
			});
			commandLineParser.ParseArguments<Options>(args)
			  .WithParsed(RunOptions);
		}
		static void RunOptions(Options opts)
		{
			
			MetadataHandler handler = new MetadataHandler(opts.Assembly, opts.TargetDirectory);
			handler.PatchAssemblies();
		}
	}
	public class MetadataHandler {
		public MetadataHandler(string assemblyPath, string targetDirectory)
		{
			using (AssemblyDefinition a = AssemblyDefinition.ReadAssembly(assemblyPath))
			{
				NewPublicKeyToken = a.Name.PublicKeyToken;
				NewAssemblyName = a.Name.Name;
			}
			TargetDirectory = targetDirectory;
		}
		public string NewAssemblyName { get; set; }
		public string TargetAssemblyName { get; set; }
		public string TargetDirectory { get; set; }
		public byte[] NewPublicKeyToken { get; set; }

		public void PatchAssemblies()
		{
			var files = Directory.EnumerateFiles(TargetDirectory, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".dll") || s.EndsWith(".exe"));
			var resolver = new DefaultAssemblyResolver();
			resolver.AddSearchDirectory(TargetDirectory);
			Dictionary<string, string> notPatched = new Dictionary<string, string>();
			var parameters = new ReaderParameters
			{
				AssemblyResolver = resolver,
				ReadWrite = true,
			};
			foreach (string assembly in files)
			{
				try
				{
					using (AssemblyDefinition targetAssembly = AssemblyDefinition.ReadAssembly(assembly, parameters))
					{
						if (targetAssembly.Name.Name != NewAssemblyName)
						{
							var assemblyReferences = targetAssembly.MainModule.AssemblyReferences;
							var targetAssemblyReference = assemblyReferences.SingleOrDefault(a => StructuralComparisons.StructuralEqualityComparer.Equals(a.Name, NewAssemblyName) &&
									!StructuralComparisons.StructuralEqualityComparer.Equals(a.PublicKeyToken, NewPublicKeyToken));

							if (targetAssemblyReference != null)
							{
								Console.WriteLine($"Modifying {targetAssembly.MainModule.Name}: replacing reference to ");
								Console.WriteLine($"\t{targetAssemblyReference.FullName} by ");
								assemblyReferences.Remove(targetAssemblyReference);
								targetAssemblyReference.PublicKeyToken = NewPublicKeyToken;
								assemblyReferences.Insert(0, targetAssemblyReference);
								Console.WriteLine($"\t{targetAssemblyReference.FullName}");
								targetAssembly.Write();
							}
						}
					}
				}catch(Exception ex)
				{
					notPatched.Add(assembly, ex.Message);
				}
			}
			if (notPatched.Count > 0)
			{
				Console.WriteLine($"Could not process the following assemblies:");
				foreach (string key in notPatched.Keys) {
					Console.WriteLine($"  {key}: {notPatched[key]}");
				}
			}
		}
	}
}
