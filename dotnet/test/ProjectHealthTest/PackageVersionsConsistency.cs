using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System;
using Xunit;
using System.Linq;

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
		private const string NET6 = "net6";
		private const string NET_FRAMEWORK = "net462";

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
			TestPackageVersionConsistencyAcrossProjects(NET6);
		}
		[Fact]
		public void TestPackageVersionConsistencyAcrossNETFrameworkProjects()
		{
			TestPackageVersionConsistencyAcrossProjects(NET_FRAMEWORK);
		}
		private void TestPackageVersionConsistencyAcrossProjects(string targetFramework)
		{
			IDictionary<string, ICollection<PackageVersionItem>> packageVersionsById = new Dictionary<string, ICollection<PackageVersionItem>>();
			foreach (string packagesConfigFilePath in GetAllProjects())
			{
				FileInfo fileInfo = new FileInfo(packagesConfigFilePath);
				XmlDocument doc = new XmlDocument();
				doc.Load(fileInfo.FullName);

				if (IsTargetFramework(doc, targetFramework))
				{
					XmlNodeList packagesNodes = doc.SelectNodes(PACKAGES_NODE_NAME);
					if (packagesNodes != null)
					{
						foreach (XmlNode packageNode in packagesNodes)
						{
							if (packageNode.Attributes == null)
							{
								continue;
							}

							XmlAttribute packageIdAtt = packageNode.Attributes[PACKAGE_NAME];
							string packageId = packageIdAtt.Value;
							Assert.True(packageId!=null, $"{fileInfo.FullName} contains an invalid Package Id for a packageReference");
							XmlAttribute packageVersionAtt = packageNode.Attributes[PACKAGE_VERSION_ATTRIBUTE_NAME];
							Assert.True(packageVersionAtt!=null, $"{fileInfo.FullName} contains an invalid Package Version for a packageReference");
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
				}
			}

			List<KeyValuePair<string, ICollection<PackageVersionItem>>> packagesWithIncoherentVersions = packageVersionsById.Where(kv => kv.Value.Count > 1).ToList();

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

		private static IEnumerable<string> GetAllProjects()
		{
			return Directory.GetFiles(SRC_DIR, PROJECTS, SearchOption.AllDirectories);
		}
	}

	public class PackageVersionItem
	{
		public string SourceFile { get; set; }
		public string Version { get; set; }

		public override string ToString()
		{
			return $"{Version} in {SourceFile}";
		}
	}
}