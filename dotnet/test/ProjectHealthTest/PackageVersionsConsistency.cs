using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Xunit;

namespace ProjectHealthTest
{
	public class PackageVersionTest
	{
		private const string PROJECTS = "*.csproj";

		private const string PACKAGES_NODE_NAME = "Project/ItemGroup/PackageReference";
		private const string PACKAGE_NAME = "Include";
		private const string SRC_DIR = @"..\..\..\..\..\src";
		private const string PACKAGE_VERSION_ATTRIBUTE_NAME = "Version";
		private const string TARGET_FRAMEWORK = "Project/PropertyGroup/TargetFramework";
		private const string TARGET_FRAMEWORKS = "Project/PropertyGroup/TargetFrameworks";
		private const string NET6 = "net6.0";
		private const string NET_FRAMEWORK = "net462";
		private static HashSet<string> ExcludedFromTransitiveDependenciesControl = new HashSet<string> {"runtime"};
		private static HashSet<string> ProjectTemporaryExcludedFromDependenciesControl = new HashSet<string> { "GeneXus.Deploy.AzureFunctions.Handlers.csproj", "AzureFunctionsTest.csproj" };
		private Regex DependencyRegEx = new Regex(@"\>\s(.+)\s+((\d+\.)?(\d+\.)?(\d+\.)?(\*|\d+))");

		/// <summary>
		/// Tests that all referenced packages have the same version by doing:
		/// - Get all projects files contained in the backend with a given targetFramework
		/// - Retrieve the id and version of all packages and transitive dependencies
		/// - Fail this test if any referenced package as direct reference has a lower version than a transitive reference to the same package
		/// - Output a message mentioning the different versions for each package 
		/// </summary>
		[Fact]
		public void TestPackageVersionConsistencyAcrossNETProjectsAndTransitives()
		{
			TestPackageVersionConsistencyAcrossProjects(NET6, true);
		}
		/// <summary>
		/// Tests that all referenced packages have the same version by doing:
		/// - Get all projects files contained in the backend with a given targetFramework
		/// - Retrieve the id and version of all packages
		/// - Fail this test if any referenced package has referenced to more than one version accross projects
		/// - Output a message mentioning the different versions for each package 
		/// </summary>
		[Fact]
		public void TestPackageVersionConsistencyAcrossNETProjects()
		{
			TestPackageVersionConsistencyAcrossProjects(NET6, false);
		}
		[Fact]
		public void TestPackageVersionConsistencyAcrossNETFrameworkProjects()
		{
			TestPackageVersionConsistencyAcrossProjects(NET_FRAMEWORK, false);
		}

		private List<string> BuildDepsJson(string projectPath, string targetFramework)
		{
			Process process = new Process();
			List<string> outputLines = new List<string>();
			process.StartInfo.FileName = "dotnet.exe";
			process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
			process.StartInfo.Arguments = $"list {projectPath} package --include-transitive --framework {targetFramework}";

			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.OutputDataReceived += (s, e) =>
					{
						string line = e.Data;
						if (!string.IsNullOrEmpty(line))
							outputLines.Add(line);
					};
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			process.WaitForExit();
			return outputLines;
		}

		private void TestPackageVersionConsistencyAcrossProjects(string targetFramework, bool checkTransitiveDeps)
		{
			IDictionary<string, ICollection<PackageVersionItem>> packageVersionsById = new Dictionary<string, ICollection<PackageVersionItem>>();
			string[] allProjects = GetAllProjects();
			Assert.True(allProjects.Length > 0, $"No projects found for {targetFramework} to analyze. Check that {targetFramework} is correct");
			Console.WriteLine($"Analyzing {allProjects.Length} projects for {targetFramework}");
			foreach (string packagesConfigFilePath in allProjects)
			{
				FileInfo fileInfo = new FileInfo(packagesConfigFilePath);
				XmlDocument doc = new XmlDocument();
				doc.Load(fileInfo.FullName);

				if (IsTargetFramework(doc, targetFramework))
				{
					XmlNodeList packagesNodes = doc.SelectNodes(PACKAGES_NODE_NAME);
					if (packagesNodes != null && packagesNodes.Count>0)
					{
						foreach (XmlNode packageNode in packagesNodes)
						{
							if (packageNode.Attributes == null)
							{
								continue;
							}

							XmlAttribute packageIdAtt = packageNode.Attributes[PACKAGE_NAME];
							if (packageIdAtt != null)
							{
								Assert.True(packageIdAtt != null, $"{fileInfo.FullName} contains an invalid Package Id for a packageReference");
								string packageId = packageIdAtt.Value;
								XmlAttribute packageVersionAtt = packageNode.Attributes[PACKAGE_VERSION_ATTRIBUTE_NAME];
								Assert.True(packageVersionAtt != null, $"{fileInfo.FullName} contains an invalid Package Version for a packageReference");
								string packageVersion = packageVersionAtt.Value;

								if (!packageVersionsById.TryGetValue(packageId, out ICollection<PackageVersionItem> packageVersions))
								{
									packageVersions = new List<PackageVersionItem>();
									packageVersionsById.Add(packageId, packageVersions);
								}

								if (!packageVersions.Any(o => o.Version.Equals(packageVersion, StringComparison.OrdinalIgnoreCase)))
								{
									packageVersions.Add(new PackageVersionItem()
									{
										SourceFile = fileInfo.FullName,
										Version = packageVersion
									});
								}
							}
						}


						if (checkTransitiveDeps && !ProjectTemporaryExcludedFromDependenciesControl.Contains(fileInfo.Name))
						{
							List<string> outputLines = BuildDepsJson(fileInfo.FullName, targetFramework);
							bool readingTransitivePackages = false;
							foreach (string line in outputLines)
							{
								if (readingTransitivePackages)
								{
									foreach (Match m in DependencyRegEx.Matches(line))
									{
										if (m.Groups != null && m.Groups.Count >= 2)
										{
											string packageId = m.Groups[1].Value.Trim();
											string packageVersion = m.Groups[2].Value;
											if (!ExcludedFromTransitiveDependenciesControl.Contains(packageId.Split('.').First()))
											{
												if (!packageVersionsById.TryGetValue(packageId, out ICollection<PackageVersionItem> packageVersions))
												{
													packageVersions = new List<PackageVersionItem>();
													packageVersionsById.Add(packageId, packageVersions);
												}

												if (!packageVersions.Any(o => o.Version.Equals(packageVersion, StringComparison.OrdinalIgnoreCase)))
												{
													packageVersions.Add(new PackageVersionItem()
													{
														SourceFile = $"{fileInfo.FullName} (Transitive dependency)",
														Version = packageVersion,
														Transitive = true
													});
												}
											}
										}
									}

								}else if (line.TrimStart().StartsWith("Transitive Package"))
								{
									readingTransitivePackages = true;
								}
							}
						}
					}
				}
			}

			List<KeyValuePair<string, ICollection<PackageVersionItem>>> packagesWithIncoherentVersions = packageVersionsById.Where(kv => kv.Value.Count > 1 && AnyDirectReferenceLessThanTransitiveVersion(kv.Value)).ToList();

			string errorMessage = string.Empty;
			if (packagesWithIncoherentVersions.Any())
			{
				errorMessage = $"Some referenced packages have incoherent versions:{Environment.NewLine}";
				foreach (KeyValuePair<string, ICollection<PackageVersionItem>> packagesWithIncoherentVersion in packagesWithIncoherentVersions)
				{
					string packageName = packagesWithIncoherentVersion.Key;
					string packageVersions = string.Join("\n  ", packagesWithIncoherentVersion.Value);
					errorMessage += $"{packageName}:\n  {packageVersions}\n\n";
				}
			}

			Assert.True(packagesWithIncoherentVersions.Count == 0, errorMessage);
		}

		private bool AnyDirectReferenceLessThanTransitiveVersion(ICollection<PackageVersionItem> value)
		{
			if (!value.Any(k => !k.Transitive))
				return false;
			PackageVersionItem directReference = value.First(k => !k.Transitive);
			if (directReference == null)
				return false;
			else
			{
				Version directVersion = new Version(directReference.Version);
				return value.Any(k => new Version(k.Version) > directVersion);
			}
		}

		private bool IsTargetFramework(XmlDocument doc, string targetFramework)
		{
			XmlNode targetFrameworkNode = doc.SelectSingleNode(TARGET_FRAMEWORK);
			if (targetFrameworkNode == null)
			{
				targetFrameworkNode = doc.SelectSingleNode(TARGET_FRAMEWORKS);
			}
			if (targetFrameworkNode != null)
			{
				return targetFrameworkNode.InnerText.Contains(targetFramework, StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				return false;
			}
		}

		private static string[] GetAllProjects()
		{
			return Directory.GetFiles(SRC_DIR, PROJECTS, SearchOption.AllDirectories);
		}
	}

	public class PackageVersionItem
	{
		public string SourceFile { get; set; }
		public string Version { get; set; }

		public bool Transitive { get; set; }

		public override string ToString()
		{
			return $"{Version} in {SourceFile}";
		}
	}
}