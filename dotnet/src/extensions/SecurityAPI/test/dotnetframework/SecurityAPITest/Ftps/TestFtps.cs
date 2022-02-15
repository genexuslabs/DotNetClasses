using GeneXusFtps.GeneXusFtps;
using NUnit.Framework;
using SecurityAPICommons.Utils;
using SecurityAPITest.SecurityAPICommons.commons;
using System.IO;

namespace SecurityAPITest.Ftps
{
	[TestFixture]
	[RunIfRunSettingsConfigured]
	public class TestFtps: SecurityAPITestObject
    {
		protected static string host;
		protected static string user;
		protected static string password;
		protected static string localPath;
		protected static string remoteDir;
		protected static string remoteFilePath;
		protected static string localDir;
		protected static string pwd;
		protected static int port;
		protected static string connectionMode;
		protected static bool forceEncryption;
		protected static string encoding;
		protected static string encryptionMode;
		protected static string protocol;
		protected static string trustStorePath;
		protected static string trustStorePassword;
		protected static ExtensionsWhiteList whiteList;
		protected static string imagePath;

		[SetUp]
		public virtual void SetUp()
		{
			host = TestContextParameter("gx_ftp_host");
			user = TestContextParameter("gx_ftps_user");
			password = TestContextParameter("gx_ftps_password");
			localPath = Path.Combine(BASE_PATH, "Temp", "ftps", "ftpstest.txt");
			remoteDir = "/files";
			remoteFilePath = "/files/ftpstest.txt";
			localDir = Path.Combine(BASE_PATH, "Temp", "ftps", "back");
			port = 21;
			connectionMode = "PASSIVE";
			forceEncryption = true;
			encoding = "BINARY";
			encryptionMode = "EXPLICIT";
			protocol = "TLS1_2";
			string certificateConentBase64 = TestContextParameter("gx_ftp_certificate_content_base64");
			trustStorePath = Path.Combine(BASE_PATH, "Temp", "ftps", "ftps_cert.pfx");
			File.WriteAllBytes(trustStorePath, System.Convert.FromBase64String(certificateConentBase64));
			trustStorePassword = TestContextParameter("gx_ftps_trust_store_password");
			whiteList = new ExtensionsWhiteList();
			whiteList.SetExtension(".txt");
			whiteList.SetExtension(".pdf");
			imagePath = Path.Combine(BASE_PATH, "Temp", "icon.png");
		}

		private FtpsClient TestConnection(FtpsOptions options)
		{
			FtpsClient client = new FtpsClient();
			bool connected = client.Connect(options);
			True(connected, client);
			return client;
		}

		private void TestPut(FtpsClient client)
		{
			bool put = client.Put(localPath, remoteDir);
			True(put, client);
		}

		private void TestGet(FtpsClient client)
		{
			bool get = client.Get(remoteFilePath, localDir);
			True(get, client);
		}

		[Test]
		public void TestWithoutCert()
		{
			FtpsOptions options = new FtpsOptions();
			options.Host = host;
			options.User = user;
			options.Password = password;
			options.Port = port;
			options.ForceEncryption = forceEncryption;
			options.ConnectionMode = connectionMode;
			options.EncryptionMode = encryptionMode;
			options.Protocol = protocol;
			options.WhiteList = whiteList;
			FtpsClient client = TestConnection(options);
			TestPut(client);
			TestGet(client);
			client.Disconnect();
		}

		[Test]
		public void TestWithCert()
		{
			FtpsOptions options = new FtpsOptions();
			options.Host = host;
			options.User = user;
			options.Password = password;
			options.Port = port;
			options.ForceEncryption = forceEncryption;
			options.ConnectionMode = connectionMode;
			options.EncryptionMode = encryptionMode;
			options.Protocol = protocol;
			options.WhiteList = whiteList;
			options.TrustStorePath = trustStorePath;
			options.TrustStorePassword = trustStorePassword;
			FtpsClient client = TestConnection(options);
			TestPut(client);
			TestGet(client);
			client.Disconnect();
		}

		[Test]
		public void TestWhiteList()
		{
			FtpsOptions options = new FtpsOptions();
			options.Host = host;
			options.User = user;
			options.Password = password;
			options.Port = port;
			options.ForceEncryption = forceEncryption;
			options.ConnectionMode = connectionMode;
			options.EncryptionMode = encryptionMode;
			options.Protocol = protocol;
			options.WhiteList = whiteList;
			FtpsClient client = TestConnection(options);
			bool put = client.Put(imagePath, remoteDir);
			Assert.IsFalse(put);
			Assert.IsTrue(client.HasError());
			client.Disconnect();

		}
	}
}
