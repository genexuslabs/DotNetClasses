using System;
using System.Globalization;
using System.IO;
using System.Net;
using GeneXus.Storage.GXAmazonS3;
using GeneXus.Storage.GXAzureStorage;
using GeneXus.Storage.GXGoogleCloud;
using Xunit;

#pragma warning disable CA1031 // Do not catch general exception types
namespace UnitTesting
{
	public abstract class ExternalProviderTest
	{		
		private GeneXus.Services.ExternalProvider provider;
		private static String TEST_SAMPLE_FILE_NAME = "text.txt";
		private static String TEST_SAMPLE_FILE_PATH = Path.Combine("resources", TEST_SAMPLE_FILE_NAME).ToString(CultureInfo.InvariantCulture);


		public ExternalProviderTest(string providerName, Type externalProviderType)
		{
			if (externalProviderType == typeof(ExternalProviderS3))
			{
				Environment.SetEnvironmentVariable(providerName + "_TEST_ENABLED", "true");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_ACCESS_KEY", "AKIAJMQ6SF3Y4IULKD5A");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_SECRET_KEY", "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");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_BUCKET_NAME", "genexuss3test");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_FOLDER_NAME", "gxclasses");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_REGION", "us-east-1");
				Environment.SetEnvironmentVariable("STORAGE_AWSS3_ENDPOINT", "s3.amazonaws.com");
			}
			if (externalProviderType == typeof(AzureStorageExternalProvider))
			{
				Environment.SetEnvironmentVariable(providerName + "_TEST_ENABLED", "true");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_ACCESS_KEY", "DNiutn2Evl+MNs0TsGUqgg0IDzYWMoMAhLM7Oju/PLi4BEsIsrSY917M6li3Ml7sxj8W+KFD/LZU49OGI40Slg==");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_ACCOUNT_NAME", "luistest1");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_FOLDER_NAME", "luistest1");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_PUBLIC_CONTAINER_NAME", "contluispublic");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_PRIVATE_CONTAINER_NAME", "contluisprivate");
			}

			if (externalProviderType == typeof(ExternalProviderGoogle))
			{
				Environment.SetEnvironmentVariable(providerName + "_TEST_ENABLED", "true");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_KEY", "{\"type\": \"service_account\",\"project_id\": \"gxjavacloudstorageunittests\",\"private_key_id\": \"37e8c0566cd56c7bb07542fdddd9f46ac64a5ba0\",\"private_key\": \"-----BEGIN PRIVATE KEY-----\nMIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQDQFJ0NHKkmZNCP\n824SBcXHa0DeoDy1i7zToD1DYiJzQXZAZg9tm91SKEbbCZWMgslBdUP10c7UsnuU\nkvnZTUpZ2YQ680ySiykjAWildp28e/nLBDVZy0f8N7H94484kOS4fxQs2VoW8MH6\nVzsxxm+3yhHjQfQOD+Obes3YBMXUxb7rs/blp7fYWbXNzyGGcpM0kEm4mRNslTbi\nZmVnuwpkaaYfQEPPYLfxWcwfgfpbXEPIJB+bNw+EN5JOUvYvCdpwdPgNyA6MvQFj\nGLTAbsXCOtFh6TtRjE9LkjTZrLTAT0HJ0kODxCyM4pBgag87iZAJ4nsq/355DIMJ\nJtEy3ZtTAgMBAAECggEAYmS89wxMeBlH/inwLJmKMohm/l7rFjXjrnahQZHQFIwp\n7L3WIdCIUWc2SjE4BF9753YaEs2Jbk6P3Wu6taS0udP/kRinZsxjQWhTIZr7b7t4\nHSX6TGGxwnRbuGC4wtjRLuT4l1SYIyzprQU+uoTJIzFsT/hJ/bRJvqXNXI61Na0J\n/ahzB640y75xe8H5Yw06yWXisqD+eiAxX8TU2SRdcZgcIVWaWLDiKhERk7sBctol\nWYgxm3qzpkA+dcvIoykrhLMZGaX9yDu7V9ueXeksqehZrQjSVx0CYSRi5ONy9bcJ\nnQIO4B0lo5oCh6EzKsYlvzJlAPCLJbs4K9Fxviu7QQKBgQDuRBiqAG9J7CR06TqW\nohdCGG6byZZ6x9wmWz3QVLsijFpiQTfu6XHDgh2A4NDau1hOzoJ21rGN5H3XNC9/\ni+8k4Je1nM14aiomlsgNFEp+gUhR7uR54da31q7Vf1DqcQ8ykqOCZitb6/AF0zhU\nlm8syJF1QAi3H7LMN0Jk3LnQsQKBgQDfkV14rRp0nUIdEp0O49bmD+qphKXFAsYn\nWCt37jq3mgZ2IVOfX94+m6bLUAjb+Ipwn9dT+3PrDlZqHRPMKTGvWdm9+41EyBK3\nvjKiNSyqnw/G/gapJn3aN/Nhp8qzOC+xNUTT9xXRf4cEuw9f19zXSm9niDgN5LS3\nE163+ZMNQwKBgB+J7gXaxuBvHKhJExNLY27BUyrV9VBNUkvVegownRDGqVQmM+Qx\nDHkHqSYdHChH8jmERmq6oogYvbuV0c+9Uyt7ezl0BxKwYuH2xYZNsEqsjEkkKSQl\nC8oL5dqm3qwZyRw1ouUo5wZk5cGvot43h4HTDsYJct3imUVE70nwmbwRAoGAQjh4\ni0oaz/fUoW/l/YcXHEYSp+uWfmh38Sd4mKmD0uZYi50Le+WVms3X9djbBuzzdLCj\nw0hz6WfxyLScLJj3Eo12pYNhMMJiaPJ5ZPqDJHbA4ZxUtL2mAYEZIg/lRniaB89T\nd8V0PP2dLJWL1EPIMizmGrCKifL4ZFHkeHIAUKkCgYAhJsPSmUc852/arUFqzhzM\nmtI/bWUUFeULygoARTSN4SVz/RjDj2MJYSV2N9MAskUKFXmmwNMq/NpJ2KkxlHrV\ned1O6cGAg8VYPpQvf6GOmpnHauePQXWXR3xVB++omRH4lD+J4gMT73dOU5o70V5e\nwY7LJ21G5lim4X1G3zRPOQ==\n-----END PRIVATE KEY-----\n\",\"client_email\": \"github-access@gxjavacloudstorageunittests.iam.gserviceaccount.com\",\"client_id\": \"110489372256464678127\",\"auth_uri\": \"https://accounts.google.com/o/oauth2/auth\",\"token_uri\": \"https://oauth2.googleapis.com/token\",\"auth_provider_x509_cert_url\": \"https://www.googleapis.com/oauth2/v1/certs\",\"client_x509_cert_url\": \"https://www.googleapis.com/robot/v1/metadata/x509/github-access%40gxjavacloudstorageunittests.iam.gserviceaccount.com\"}");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_PROJECT_ID", "gxjavacloudstorageunittests");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_BUCKET_NAME", "javaclasses-unittests");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_FOLDER_NAME", "gxclasses");
				Environment.SetEnvironmentVariable($"STORAGE_{providerName}_APPLICATION_NAME", "gxjavacloudstorageunittests");
			}

			bool testEnabled = Environment.GetEnvironmentVariable(providerName + "_TEST_ENABLED") == "true";

			Assume.True(testEnabled);

			provider = (GeneXus.Services.ExternalProvider)Activator.CreateInstance(externalProviderType);

			Assert.NotNull(provider);
		}

		[Fact]
		public void TestUploadPublicMethod()
		{
			String upload = provider.Upload(TEST_SAMPLE_FILE_PATH, TEST_SAMPLE_FILE_NAME, GxFileType.PublicRead);
			EnsureUrl(upload, GxFileType.PublicRead);
		}

		[Fact]
		public void TestUploadDefaultMethod()
		{
			String upload = provider.Upload(TEST_SAMPLE_FILE_PATH, TEST_SAMPLE_FILE_NAME, GxFileType.Default);
			EnsureUrl(upload, GxFileType.Default);
		}

		[Fact]
		public void TestUploadAndCopyDefault()
		{
			TestUploadAndCopyByAcl(GxFileType.Default, GxFileType.Default);
		}

		[Fact]
		public void TestUploadAndCopyPrivate()
		{
			TestUploadAndCopyByAcl(GxFileType.Private, GxFileType.Private);
		}

		[Fact]
		public void TestUploadAndCopyPublic()
		{
			TestUploadAndCopyByAcl(GxFileType.PublicRead, GxFileType.PublicRead);
		}

		[Fact]
		public void TestUploadAndCopyMixed()
		{
			TestUploadAndCopyByAcl(GxFileType.Default, GxFileType.Private);
		}

		[Fact]
		public void TestUploadAndCopyPrivateToPublic()
		{
			TestUploadAndCopyByAcl(GxFileType.Private, GxFileType.PublicRead);
		}

		[Fact]
		public void TestUploadAndCopyPublicToPrivate()
		{
			TestUploadAndCopyByAcl(GxFileType.PublicRead, GxFileType.Private);
		}

		public void TestUploadAndCopyByAcl(GxFileType aclUpload, GxFileType aclCopy)
		{
			String copyFileName = "test-upload-and-copy.txt";
			DeleteSafe(TEST_SAMPLE_FILE_PATH);
			DeleteSafe(copyFileName);
			String upload = provider.Upload(TEST_SAMPLE_FILE_PATH, TEST_SAMPLE_FILE_NAME, aclUpload);
			Assert.True(UrlExists(upload), "Not found URL: " + upload);

			String copyUrl = TryGet(copyFileName, aclCopy);
			Assert.False(UrlExists(copyUrl), "URL cannot exist: " + copyUrl);

			provider.Copy(TEST_SAMPLE_FILE_NAME, aclUpload, copyFileName, aclCopy);
			upload = provider.Get(copyFileName, aclCopy, 100);
			EnsureUrl(upload, aclCopy);
		}

		[Fact]
		public void TestCopyMethod()
		{
			String copyFileName = "copy-text.txt";
			Copy(copyFileName, GxFileType.PublicRead);
		}

		[Fact]
		public void TestCopyPrivateMethod()
		{
			String copyFileName = "copy-text-private.txt";
			Copy(copyFileName, GxFileType.Private);
		}

		private void Copy(String copyFileName, GxFileType acl)
		{
			provider.Upload(TEST_SAMPLE_FILE_PATH, TEST_SAMPLE_FILE_NAME, acl);
			String upload = provider.Get(TEST_SAMPLE_FILE_NAME, acl, 100);
			EnsureUrl(upload, acl);

			DeleteSafe(copyFileName);
			Wait(1000); //Google CDN replication seems to be delayed.

			String urlCopy = TryGet(copyFileName, GxFileType.PublicRead);
			Assert.False(UrlExists(urlCopy), "URL cannot exist: " + urlCopy);

			provider.Copy("text.txt", acl, copyFileName, acl);
			upload = provider.Get(copyFileName, acl, 100);
			EnsureUrl(upload, acl);
		}

		[Fact]
		public void TestMultimediaUpload()
		{
			String copyFileName = "copy-text-private.txt";
			GxFileType acl = GxFileType.Private;

			provider.Upload(TEST_SAMPLE_FILE_PATH, TEST_SAMPLE_FILE_NAME, acl);
			String upload = provider.Get(TEST_SAMPLE_FILE_NAME, acl, 100);
			EnsureUrl(upload, acl);

			provider.Delete(copyFileName, acl);
			provider.Copy("text.txt", acl, copyFileName, acl);
			upload = TryGet(copyFileName, acl);
			EnsureUrl(upload, acl);
		}

		[Fact]
		public void TestGetMethod()
		{
			TestUploadPublicMethod();
			String url = provider.Get("text.txt", GxFileType.PublicRead, 10);
			EnsureUrl(url, GxFileType.PublicRead);
		}

		[Fact]
		public void TestGetObjectName()
		{
			TestUploadPublicMethod();
			String url = provider.Get(TEST_SAMPLE_FILE_NAME, GxFileType.PublicRead, 10);
			Assert.True(UrlExists(url));
			String objectName = "";
			provider.GetObjectNameFromURL(url, out objectName);
			Assert.Equal("text.txt", objectName);
		}

		[Fact]
		public void TestDownloadMethod()
		{
			String downloadPath = Path.Combine("resources", "test", TEST_SAMPLE_FILE_NAME);
			TestUploadPublicMethod();

			try
			{
				File.Delete(downloadPath);
			}
			catch (Exception) { }

			provider.Download(TEST_SAMPLE_FILE_NAME, downloadPath, GxFileType.PublicRead);
			Assert.True(File.Exists(downloadPath));
		}

		[Fact]
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

		[Fact]
		public void TestDeleteFilePrivate()
		{
			GxFileType acl = GxFileType.Private;
			TestUploadPrivateMethod();
			String url = TryGet(TEST_SAMPLE_FILE_NAME, acl);
			EnsureUrl(url, acl);
			provider.Delete(TEST_SAMPLE_FILE_NAME, acl);

			url = TryGet(TEST_SAMPLE_FILE_NAME, acl);
			Assert.False(UrlExists(url));
		}

		[Fact]
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


		private String TryGet(String objectName, GxFileType acl)
		{
			String getValue = "";
			try
			{
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

		private static void EnsureUrl(String signedOrUnsignedUrl, GxFileType acl)
		{
			Assert.True(UrlExists(signedOrUnsignedUrl), "Resource not found: " + signedOrUnsignedUrl);
			if (acl == GxFileType.Private)
			{
				String noSignedUrl = signedOrUnsignedUrl.Substring(0, signedOrUnsignedUrl.IndexOf('?') + 1);
				Assert.False(UrlExists(noSignedUrl), "Resource must be private: " + noSignedUrl);
			}
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