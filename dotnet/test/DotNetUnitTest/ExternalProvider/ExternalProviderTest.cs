using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using DotNetUnitTest;
using GeneXus.Services;
using GeneXus.Storage;
using Xunit;


#pragma warning disable CA1031 // Do not catch general exception types
namespace UnitTesting
{
	[Collection("Sequential")]
	public abstract class ExternalProviderTest
	{

		private GeneXus.Services.ExternalProvider provider;
		private String testRunId;
		private String testFileName;
		private String testFilePath;
		private bool defaultAclPrivate;

		public ExternalProviderTest(string providerName, Type externalProviderType, bool isPrivate)
		{
			defaultAclPrivate = isPrivate;
			Environment.SetEnvironmentVariable($"STORAGE_{providerName}_DEFAULT_ACL", defaultAclPrivate ? GxFileType.Private.ToString() : GxFileType.PublicRead.ToString());
			bool testEnabled = Environment.GetEnvironmentVariable(providerName + "_TEST_ENABLED") == "true";


			Skip.IfNot(testEnabled, "Environment variables not set");
			provider = (GeneXus.Services.ExternalProvider)Activator.CreateInstance(externalProviderType);

			Assert.NotNull(provider);

			testRunId = new Random().Next(1, 10000).ToString(CultureInfo.InvariantCulture);
			testFileName = $"text{testRunId}.txt";
			testFilePath = Path.Combine("resources", testFileName).ToString(CultureInfo.InvariantCulture);

			File.WriteAllText(testFilePath, "This is a Sample Test from External Storage GeneXus .NET Generator Unit Tests");
		}

		public ExternalProviderTest(string providerName, Type externalProviderType) : this(providerName, externalProviderType, false)
		{

		}

		[SkippableFact]
		public void TestUploadPublicMethod()
		{
			String upload = provider.Upload(testFilePath, testFileName, GxFileType.PublicRead);
			EnsureUrl(upload, GxFileType.PublicRead);
		}

		[SkippableFact]
		public void TestUploadPrivateSubfolderMethod()
		{
			String upload = provider.Upload(testFilePath, $"folder/folder2/folder3/{testFileName}", GxFileType.Private);
			EnsureUrl(upload, GxFileType.Private);
		}

		[SkippableFact]
		public void TestUploadDefaultMethod()
		{
			String upload = provider.Upload(testFilePath, testFileName, GxFileType.Default);
			EnsureUrl(upload, GxFileType.Default);
		}

		[SkippableFact]
		public void TestUploadDefaultAttributeMethod()
		{
			String upload = provider.Upload(testFilePath, testFileName, GxFileType.DefaultAttribute);
			EnsureUrl(upload, GxFileType.DefaultAttribute);
		}

		[SkippableFact]
		public void TestUploadAndCopyDefault()
		{
			TestUploadAndCopyByAcl(GxFileType.Default, GxFileType.Default);
		}

		[SkippableFact]
		public void TestUploadAndCopyPrivate()
		{
			TestUploadAndCopyByAcl(GxFileType.Private, GxFileType.Private);
		}

		[SkippableFact]
		public void TestUploadAndCopyPublic()
		{
			TestUploadAndCopyByAcl(GxFileType.PublicRead, GxFileType.PublicRead);
		}

		[SkippableFact]
		public void TestUploadAndCopyMixed()
		{
			TestUploadAndCopyByAcl(GxFileType.Default, GxFileType.Private);
		}

		[SkippableFact]
		public void TestUploadAndCopyPrivateToPublic()
		{
			TestUploadAndCopyByAcl(GxFileType.Private, GxFileType.PublicRead);
		}

		[SkippableFact]
		public void TestUploadAndCopyPublicToPrivate()
		{
			TestUploadAndCopyByAcl(GxFileType.PublicRead, GxFileType.Private);
		}

		public void TestUploadAndCopyByAcl(GxFileType aclUpload, GxFileType aclCopy)
		{
			string copySourceName = BuildRandomTextFileName("test-source-upload-and-copy");
			String copyTargetName = BuildRandomTextFileName("test-upload-and-copy");
			DeleteSafe(testFilePath);
			DeleteSafe(copyTargetName);
			String upload = provider.Upload(testFilePath, copySourceName, aclUpload);
			EnsureUrl(upload, aclUpload);

			String copyUrl = TryGet(copyTargetName, aclCopy);
			Assert.False(UrlExists(copyUrl), "URL cannot exist: " + copyUrl);

			provider.Copy(copySourceName, aclUpload, copyTargetName, aclCopy);
			upload = provider.Get(copyTargetName, aclCopy, 100);
			EnsureUrl(upload, aclCopy);
		}

		[SkippableFact]
		public void TestCopyMethod()
		{
			string copyFileName = BuildRandomTextFileName("text-copy");
			Copy(copyFileName, GxFileType.PublicRead);
		}

		private string BuildRandomTextFileName(string name)
		{
			return $"{name}_{testRunId}.txt";
		}

		[SkippableFact]
		public void TestCopyPrivateMethod()
		{
			String copyFileName = BuildRandomTextFileName("copy-text-private");
			Copy(copyFileName, GxFileType.Private);
		}

		[SkippableFact]
		public void TestDirectoryNotExists()
		{
			string folderName = $"ThisFolderDoesNotExists";
			DeleteSafe(folderName);
			bool exist = provider.ExistsDirectory(folderName);
			Assert.False(exist);
		}
		[SkippableFact]
		public void TestDirectoryExists()
		{
			string folderName = $"ThisFolderDoesExists";
			string fileName = "file.tmp";
			provider.Upload(testFilePath, $"{folderName}/{fileName}", GxFileType.PublicRead);
						
			bool exist = provider.ExistsDirectory(folderName);
			Assert.True(exist);
			provider.ExistsDirectory(folderName + "/");
			Assert.True(exist);
		}

		[SkippableFact]
		public void TestFileNotExists()
		{
			string fileName = $"ThisFolderDoesNotExists/file.pdf";
			DeleteSafe(fileName);
			bool exist = provider.Exists(fileName, GxFileType.Private);
			Assert.False(exist);
		}

		[SkippableFact]
		public void TestFileExists()
		{
			string folderName = $"ThisFolderDoesExists";
			string fileName = "file.tmp";
			string objectPath = $"{folderName}/{fileName}";
			provider.Upload(testFilePath, objectPath, GxFileType.PublicRead);

			bool exist = provider.Exists(objectPath, GxFileType.PublicRead);
			Assert.True(exist);
		}

		[SkippableFact]
		public void TestMultimediaUpload()
		{
			string sourceFile = $"folder1/folder2/folder3{testFileName}";
			String copyFileName = BuildRandomTextFileName("copy-text-private");
			GxFileType acl = GxFileType.Private;

			provider.Upload(testFilePath, sourceFile, acl);
			String upload = provider.Get(sourceFile, acl, 100);
			EnsureUrl(upload, acl);

			DeleteSafe(copyFileName);
			upload = provider.Copy(sourceFile, copyFileName, "Table", "Field", acl);

			copyFileName = StorageFactory.GetProviderObjectAbsoluteUriSafe(provider, upload);
			if (!copyFileName.StartsWith("http", StringComparison.OrdinalIgnoreCase))
			{
				upload = TryGet(copyFileName, acl);
			}
			EnsureUrl(upload, acl);
		}

		[SkippableFact]
		public void TestGetMethod()
		{
			TestUploadPublicMethod();
			String url = provider.Get(testFileName, GxFileType.PublicRead, 10);
			EnsureUrl(url, GxFileType.PublicRead);
		}

		[SkippableFact]
		public void TestGetObjectName()
		{
			TestUploadPublicMethod();
			string url = provider.Get(testFileName, GxFileType.PublicRead, 10);
			Assert.True(UrlExists(url));
			string objectName;
			provider.TryGetObjectNameFromURL(url, out objectName);
			Assert.Equal(testFileName, objectName);
		}

		[SkippableFact]
		public void TestDownloadMethod()
		{
			TestUploadPublicMethod();

			String downloadPath = Path.Combine("resources", "test", testFileName);
			try
			{
				File.Delete(downloadPath);
			}
			catch (Exception) { }
			try
			{
				Directory.CreateDirectory(Path.Combine("resources", "test"));
			}
			catch (Exception) { }
			provider.Download(testFileName, downloadPath, GxFileType.PublicRead);
			Assert.True(File.Exists(downloadPath));
		}

		[SkippableFact]
		public void TestDeleteFile()
		{
			GxFileType acl = GxFileType.PublicRead;
			TestUploadPublicMethod();
			String url = TryGet(testFileName, acl);
			EnsureUrl(url, acl);
			provider.Delete(testFileName, acl);

			url = TryGet(testFileName, acl);
			Assert.False(UrlExists(url));
		}


		[SkippableFact]
		public void TestDeleteFilePrivate()
		{
			GxFileType acl = GxFileType.Private;
			provider.Upload(testFilePath, testFileName, acl);
			provider.Delete(testFileName, acl);
			String url = TryGet(testFileName, acl);
			Assert.False(UrlExists(url));
		}

		[SkippableFact]
		public void TestUploadPrivateMethod()
		{
			GxFileType acl = GxFileType.Private;
			String externalFileName = BuildRandomTextFileName("text-private-2");

			DeleteSafe(externalFileName);
			String signedUrl = provider.Upload(testFilePath, externalFileName, acl);
			EnsureUrl(signedUrl, acl);
			signedUrl = provider.Get(externalFileName, acl, 10);
			EnsureUrl(signedUrl, acl);

		}


		[SkippableFact]
		public void TestEmptyFolder()
		{
			GxFileType acl = GxFileType.PublicRead;
			string folderName = $"folderTemp{new Random().Next(1, 100)}";

			provider.DeleteDirectory(folderName);

			List<string> urls = new List<string>();

			urls.Add(provider.Upload(testFilePath, $"{folderName}/test1.png", acl));
			urls.Add(provider.Upload(testFilePath, $"{folderName}/text2.txt", acl));
			urls.Add(provider.Upload(testFilePath, $"{folderName}/text3.txt", acl));
			urls.Add(provider.Upload(testFilePath, $"{folderName}/text4.txt", acl));
			urls.Add(provider.Upload(testFilePath, $"{folderName}/test1.png", acl));


			var files = provider.GetFiles(folderName);
			Assert.Equal(4, files.Count);

			provider.DeleteDirectory(folderName);

			files = provider.GetFiles(folderName);
			Assert.Empty(files);

		}

		private void Copy(String copyFileName, GxFileType acl)
		{
			provider.Upload(testFilePath, testFileName, acl);
			String upload = provider.Get(testFileName, acl, 100);
			EnsureUrl(upload, acl);

			DeleteSafe(copyFileName);
			Wait(500); //Google CDN replication seems to be delayed.

			String urlCopy = TryGet(copyFileName, GxFileType.PublicRead);
			Assert.False(UrlExists(urlCopy), "URL cannot exist: " + urlCopy);

			provider.Copy(testFileName, acl, copyFileName, acl);
			upload = provider.Get(copyFileName, acl, 100);
			EnsureUrl(upload, acl);
		}



		private String TryGet(String objectName, GxFileType acl)
		{
			String getValue = "";
			try
			{
				if (((ExternalProviderBase)provider).GetName() == GeneXus.Storage.GXGoogleCloud.ExternalProviderGoogle.Name)
				{
					objectName = objectName.Replace("%2F", "/"); //Google Cloud Storage Bug. https://github.com/googleapis/google-cloud-dotnet/pull/3677
				}
				getValue = provider.Get(objectName, acl, 5);
			}
			catch (Exception)
			{

			}
			return getValue;
		}

		private String GetSafe(String objectName, GxFileType acl)
		{
			try
			{
				return provider.Get(objectName, acl, 100);
			}
			catch (Exception)
			{

			}
			return String.Empty;
		}

		private bool DeleteSafe(String objectName)
		{
			DeleteSafeImpl(objectName, GxFileType.Private);
			DeleteSafeImpl(objectName, GxFileType.PublicRead);
			return true;
		}

		private bool DeleteSafeImpl(String objectName, GxFileType acl)
		{
			try
			{
				provider.Delete(objectName, acl);
			}
			catch (Exception)
			{

			}
			return true;
		}

		private static void Wait(int milliseconds)
		{
			System.Threading.Thread.Sleep(milliseconds);
		}

		private void EnsureUrl(String signedOrUnsignedUrl, GxFileType acl)
		{
			Assert.True(UrlExists(signedOrUnsignedUrl), "URL not found: " + signedOrUnsignedUrl);
			if (IsPrivateFile(acl))
			{
				if (!(this is ExternalProviderMinioTest)) //Minio local installation not supported
				{
					Skip.If(this is ExternalProviderMinioTest);
					String noSignedUrl = signedOrUnsignedUrl.Substring(0, signedOrUnsignedUrl.IndexOf('?') + 1);
					Assert.False(UrlExists(noSignedUrl), "URL must be private: " + noSignedUrl);
				}
			}
			else
			{
				Assert.False(signedOrUnsignedUrl.Contains("?"), "URL cannot be signed");
			}
		}
		private bool IsPrivateFile(GxFileType acl)
		{
			if (acl.HasFlag(GxFileType.Private))
			{
				return true;
			}
			else if (acl.HasFlag(GxFileType.PublicRead))
			{
				return false;
			}
			return defaultAclPrivate;
		}

#pragma warning disable CA1054 // Uri parameters should not be strings
		protected static bool UrlExists(string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				return false;
			}
			bool exists = false;

#pragma warning disable SYSLIB0014 // Type or member is obsolete
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
#pragma warning restore SYSLIB0014 // Type or member is obsolete
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

			try
			{
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				{
					exists = response.StatusCode == HttpStatusCode.OK;
				}
			}
			catch (WebException)
			{

			}
			return exists;
		}
#pragma warning restore CA1054 // Uri parameters should not be strings
	}
}
#pragma warning restore CA1031 // Do not catch general exception types