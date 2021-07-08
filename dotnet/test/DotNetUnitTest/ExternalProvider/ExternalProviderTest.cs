using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
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
		private String TEST_SAMPLE_FILE_NAME = $"text{new Random().Next(1, 10000)}.txt";
		private String TEST_SAMPLE_FILE_PATH;
		private bool defaultAclPrivate;
		
		public ExternalProviderTest(string providerName, Type externalProviderType, bool isPrivate)
		{			
			defaultAclPrivate = isPrivate;
			Environment.SetEnvironmentVariable($"STORAGE_{providerName}_DEFAULT_ACL", defaultAclPrivate ? GxFileType.Private.ToString() : GxFileType.PublicRead.ToString());
			bool testEnabled = Environment.GetEnvironmentVariable(providerName + "_TEST_ENABLED") == "true";

						
			Skip.IfNot(testEnabled, "Environment variables not set");
			provider = (GeneXus.Services.ExternalProvider)Activator.CreateInstance(externalProviderType);
			
			Assert.NotNull(provider);

			TEST_SAMPLE_FILE_PATH = Path.Combine("resources", TEST_SAMPLE_FILE_NAME).ToString(CultureInfo.InvariantCulture);
			File.WriteAllText(TEST_SAMPLE_FILE_PATH, "This is a Sample Test from External Storage GeneXus .NET Generator Unit Tests");
		}

		public ExternalProviderTest(string providerName, Type externalProviderType) : this(providerName, externalProviderType, false)
		{

		}

		[SkippableFact]
		public void TestUploadPublicMethod()
		{
			String upload = provider.Upload(TEST_SAMPLE_FILE_PATH, TEST_SAMPLE_FILE_NAME, GxFileType.PublicRead);
			EnsureUrl(upload, GxFileType.PublicRead);
		}

		[SkippableFact]
		public void TestUploadPrivateSubfolderMethod()
		{
			String upload = provider.Upload(TEST_SAMPLE_FILE_PATH, $"folder/folder2/folder3/{TEST_SAMPLE_FILE_NAME}", GxFileType.Private);
			EnsureUrl(upload, GxFileType.Private);
		}

		[SkippableFact]
		public void TestUploadDefaultMethod()
		{
			String upload = provider.Upload(TEST_SAMPLE_FILE_PATH, TEST_SAMPLE_FILE_NAME, GxFileType.Default);
			EnsureUrl(upload, GxFileType.Default);
		}

		[SkippableFact]
		public void TestUploadDefaultAttributeMethod()
		{
			String upload = provider.Upload(TEST_SAMPLE_FILE_PATH, TEST_SAMPLE_FILE_NAME, GxFileType.DefaultAttribute);
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
			string copySourceName = $"test-source-upload-and-copy_{new Random().Next()}.txt";
			String copyTargetName = $"test-upload-and-copy_{new Random().Next()}.txt";
			DeleteSafe(TEST_SAMPLE_FILE_PATH);
			DeleteSafe(copyTargetName);
			String upload = provider.Upload(TEST_SAMPLE_FILE_PATH, copySourceName, aclUpload);
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
			String copyFileName = "copy-text.txt";
			Copy(copyFileName, GxFileType.PublicRead);
		}

		[SkippableFact]
		public void TestCopyPrivateMethod()
		{
			String copyFileName = "copy-text-private.txt";
			Copy(copyFileName, GxFileType.Private);
		}

		[SkippableFact]
		public void TestMultimediaUpload()
		{
			string sourceFile = $"folder1/folder2/folder3{TEST_SAMPLE_FILE_NAME}";
			String copyFileName = "copy-text-private.txt";
			GxFileType acl = GxFileType.Private;

			provider.Upload(TEST_SAMPLE_FILE_PATH, sourceFile, acl);
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
			String url = provider.Get(TEST_SAMPLE_FILE_NAME, GxFileType.PublicRead, 10);
			EnsureUrl(url, GxFileType.PublicRead);
		}

		[SkippableFact]
		public void TestGetObjectName()
		{
			TestUploadPublicMethod();
			string url = provider.Get(TEST_SAMPLE_FILE_NAME, GxFileType.PublicRead, 10);
			Assert.True(UrlExists(url));
			string objectName;
			provider.TryGetObjectNameFromURL(url, out objectName);
			Assert.Equal(TEST_SAMPLE_FILE_NAME, objectName);
		}

		[SkippableFact]
		public void TestDownloadMethod()
		{
			TestUploadPublicMethod();

			String downloadPath = Path.Combine("resources", "test", TEST_SAMPLE_FILE_NAME);
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
			provider.Download(TEST_SAMPLE_FILE_NAME, downloadPath, GxFileType.PublicRead);
			Assert.True(File.Exists(downloadPath));
		}

		[SkippableFact]
		public void TestDeleteFile()
		{
			GxFileType acl = GxFileType.PublicRead;
			TestUploadPublicMethod();
			String url = TryGet(TEST_SAMPLE_FILE_NAME, acl);
			EnsureUrl(url, acl);
			provider.Delete(TEST_SAMPLE_FILE_NAME, acl);

			url = TryGet(TEST_SAMPLE_FILE_NAME, acl);
			Assert.False(UrlExists(url));
		}


		[SkippableFact]
		public void TestDeleteFilePrivate()
		{			
			GxFileType acl = GxFileType.Private;
			provider.Upload(TEST_SAMPLE_FILE_PATH, TEST_SAMPLE_FILE_NAME, acl);
			provider.Delete(TEST_SAMPLE_FILE_NAME, acl);
			String url = TryGet(TEST_SAMPLE_FILE_NAME, acl);
			Assert.False(UrlExists(url));
		}

		[SkippableFact]
		public void TestUploadPrivateMethod()
		{
			GxFileType acl = GxFileType.Private;
			String externalFileName = "text-private-2.txt";

			DeleteSafe(externalFileName);
			String signedUrl = provider.Upload(TEST_SAMPLE_FILE_PATH, externalFileName, acl);
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

			urls.Add(provider.Upload(TEST_SAMPLE_FILE_PATH, $"{folderName}/test1.png", acl));
			urls.Add(provider.Upload(TEST_SAMPLE_FILE_PATH, $"{folderName}/text2.txt", acl));
			urls.Add(provider.Upload(TEST_SAMPLE_FILE_PATH, $"{folderName}/text3.txt", acl));
			urls.Add(provider.Upload(TEST_SAMPLE_FILE_PATH, $"{folderName}/text4.txt", acl));
			urls.Add(provider.Upload(TEST_SAMPLE_FILE_PATH, $"{folderName}/test1.png", acl));


			var files = provider.GetFiles(folderName);
			Assert.Equal(4, files.Count);
	
			provider.DeleteDirectory(folderName);

			files = provider.GetFiles(folderName);
			Assert.Empty(files);
			
		}

		private void Copy(String copyFileName, GxFileType acl)
		{
			provider.Upload(TEST_SAMPLE_FILE_PATH, TEST_SAMPLE_FILE_NAME, acl);
			String upload = provider.Get(TEST_SAMPLE_FILE_NAME, acl, 100);
			EnsureUrl(upload, acl);

			DeleteSafe(copyFileName);
			Wait(500); //Google CDN replication seems to be delayed.

			String urlCopy = TryGet(copyFileName, GxFileType.PublicRead);
			Assert.False(UrlExists(urlCopy), "URL cannot exist: " + urlCopy);

			provider.Copy(TEST_SAMPLE_FILE_NAME, acl, copyFileName, acl);
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
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
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