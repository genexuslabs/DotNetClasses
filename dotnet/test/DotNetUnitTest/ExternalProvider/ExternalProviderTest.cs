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
		private String testRunId;
		private String testFileName;
		private String testFilePath;
		private bool defaultAclPrivate;

		public ExternalProviderTest(string providerName, Type externalProviderType, bool isPrivate)
		{
			defaultAclPrivate = isPrivate;
			Environment.SetEnvironmentVariable($"STORAGE_{providerName}_DEFAULT_ACL", defaultAclPrivate ? GxFileType.Private.ToString() : GxFileType.PublicRead.ToString());
			bool testEnabled = Environment.GetEnvironmentVariable(providerName + "_TEST_ENABLED") == "true";


			if (providerName == GeneXus.Storage.GXAmazonS3.ExternalProviderS3.Name)
			{
				testEnabled = true;
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_ACCESS_KEY", "AKIA4EZIEMMO6XZFQCUU");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_SECRET_KEY", "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");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_BUCKET_NAME", "genexus-s3-test");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_FOLDER_NAME", "gxclasses");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_REGION", "us-east-1");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_ENDPOINT", string.Empty);
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_CUSTOM_ENDPOINT", string.Empty);
			}

			if (providerName == GeneXus.Storage.GXGoogleCloud.ExternalProviderGoogle.Name)
			{
				testEnabled = true;
				Environment.SetEnvironmentVariable(providerName + "_TEST_ENABLED", "true");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_KEY", "{\"type\": \"service_account\",\"project_id\": \"gxjavacloudstorageunittests\",\"private_key_id\": \"37e8c0566cd56c7bb07542fdddd9f46ac64a5ba0\",\"private_key\": \"-----BEGIN PRIVATE KEY-----\nMIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQDQFJ0NHKkmZNCP\n824SBcXHa0DeoDy1i7zToD1DYiJzQXZAZg9tm91SKEbbCZWMgslBdUP10c7UsnuU\nkvnZTUpZ2YQ680ySiykjAWildp28e/nLBDVZy0f8N7H94484kOS4fxQs2VoW8MH6\nVzsxxm+3yhHjQfQOD+Obes3YBMXUxb7rs/blp7fYWbXNzyGGcpM0kEm4mRNslTbi\nZmVnuwpkaaYfQEPPYLfxWcwfgfpbXEPIJB+bNw+EN5JOUvYvCdpwdPgNyA6MvQFj\nGLTAbsXCOtFh6TtRjE9LkjTZrLTAT0HJ0kODxCyM4pBgag87iZAJ4nsq/355DIMJ\nJtEy3ZtTAgMBAAECggEAYmS89wxMeBlH/inwLJmKMohm/l7rFjXjrnahQZHQFIwp\n7L3WIdCIUWc2SjE4BF9753YaEs2Jbk6P3Wu6taS0udP/kRinZsxjQWhTIZr7b7t4\nHSX6TGGxwnRbuGC4wtjRLuT4l1SYIyzprQU+uoTJIzFsT/hJ/bRJvqXNXI61Na0J\n/ahzB640y75xe8H5Yw06yWXisqD+eiAxX8TU2SRdcZgcIVWaWLDiKhERk7sBctol\nWYgxm3qzpkA+dcvIoykrhLMZGaX9yDu7V9ueXeksqehZrQjSVx0CYSRi5ONy9bcJ\nnQIO4B0lo5oCh6EzKsYlvzJlAPCLJbs4K9Fxviu7QQKBgQDuRBiqAG9J7CR06TqW\nohdCGG6byZZ6x9wmWz3QVLsijFpiQTfu6XHDgh2A4NDau1hOzoJ21rGN5H3XNC9/\ni+8k4Je1nM14aiomlsgNFEp+gUhR7uR54da31q7Vf1DqcQ8ykqOCZitb6/AF0zhU\nlm8syJF1QAi3H7LMN0Jk3LnQsQKBgQDfkV14rRp0nUIdEp0O49bmD+qphKXFAsYn\nWCt37jq3mgZ2IVOfX94+m6bLUAjb+Ipwn9dT+3PrDlZqHRPMKTGvWdm9+41EyBK3\nvjKiNSyqnw/G/gapJn3aN/Nhp8qzOC+xNUTT9xXRf4cEuw9f19zXSm9niDgN5LS3\nE163+ZMNQwKBgB+J7gXaxuBvHKhJExNLY27BUyrV9VBNUkvVegownRDGqVQmM+Qx\nDHkHqSYdHChH8jmERmq6oogYvbuV0c+9Uyt7ezl0BxKwYuH2xYZNsEqsjEkkKSQl\nC8oL5dqm3qwZyRw1ouUo5wZk5cGvot43h4HTDsYJct3imUVE70nwmbwRAoGAQjh4\ni0oaz/fUoW/l/YcXHEYSp+uWfmh38Sd4mKmD0uZYi50Le+WVms3X9djbBuzzdLCj\nw0hz6WfxyLScLJj3Eo12pYNhMMJiaPJ5ZPqDJHbA4ZxUtL2mAYEZIg/lRniaB89T\nd8V0PP2dLJWL1EPIMizmGrCKifL4ZFHkeHIAUKkCgYAhJsPSmUc852/arUFqzhzM\nmtI/bWUUFeULygoARTSN4SVz/RjDj2MJYSV2N9MAskUKFXmmwNMq/NpJ2KkxlHrV\ned1O6cGAg8VYPpQvf6GOmpnHauePQXWXR3xVB++omRH4lD+J4gMT73dOU5o70V5e\nwY7LJ21G5lim4X1G3zRPOQ==\n-----END PRIVATE KEY-----\n\",\"client_email\": \"github-access@gxjavacloudstorageunittests.iam.gserviceaccount.com\",\"client_id\": \"110489372256464678127\",\"auth_uri\": \"https://accounts.google.com/o/oauth2/auth\",\"token_uri\": \"https://oauth2.googleapis.com/token\",\"auth_provider_x509_cert_url\": \"https://www.googleapis.com/oauth2/v1/certs\",\"client_x509_cert_url\": \"https://www.googleapis.com/robot/v1/metadata/x509/github-access%40gxjavacloudstorageunittests.iam.gserviceaccount.com\"}");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_PROJECT_ID", "gxjavacloudstorageunittests");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_BUCKET_NAME", "javaclasses-unittests");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_FOLDER_NAME", "gxclasses");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_APPLICATION_NAME", "gxjavacloudstorageunittests");
			}

			if (providerName == GeneXus.Storage.GXAzureStorage.AzureStorageExternalProvider.Name)
			{
				testEnabled = true;
				Environment.SetEnvironmentVariable(providerName + "_TEST_ENABLED", "true");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_ACCESS_KEY", "DNiutn2Evl+MNs0TsGUqgg0IDzYWMoMAhLM7Oju/PLi4BEsIsrSY917M6li3Ml7sxj8W+KFD/LZU49OGI40Slg==");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_ACCOUNT_NAME", "luistest1");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_FOLDER_NAME", "luistest1");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_PUBLIC_CONTAINER_NAME", "contluispublic");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_PRIVATE_CONTAINER_NAME", "contluisprivate");
			}

			if (providerName == "ORACLE")
			{
				testEnabled = true;
				Environment.SetEnvironmentVariable("AWSS3" + "_TEST_ENABLED", "true");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_ACCESS_KEY", "2be038125c3ca277cfdb4cea8677868692e9fa52");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_SECRET_KEY", "kTnFOaOvE799NFpQC9Zl0NJfcdUfMNPrFOB7OgWAtJA=");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_BUCKET_NAME", "Hanel_FoccoLOJAS_Teste");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_FOLDER_NAME", "gxclasses");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_REGION", "us-east-1");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_ENDPOINT", "custom");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_CUSTOM_ENDPOINT", "https://compat.objectstorage.sa-saopaulo-1.oraclecloud.com");
			}

			if (this is ExternalProviderMinioTest)
			{
				testEnabled = true;
				Environment.SetEnvironmentVariable(providerName + "_TEST_ENABLED", "true");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_ACCESS_KEY", "desaint");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_SECRET_KEY", "6YafTT3U2YtHS7am");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_BUCKET_NAME", "java-classes-unittests");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_FOLDER_NAME", "test-minio");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_ENDPOINT", "custom");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_CUSTOM_ENDPOINT", "http://192.168.254.78:9000");
			}

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