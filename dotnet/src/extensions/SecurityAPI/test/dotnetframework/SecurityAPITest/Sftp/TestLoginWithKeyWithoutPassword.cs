using SecurityAPITest.SecurityAPICommons.commons;
using NUnit.Framework;
using Sftp.GeneXusSftp;
using System.IO;
using System;

namespace SecurityAPITest.Sftp
{
	[TestFixture]
	[RunIfRunSettingsConfigured]
	public class TestLoginWithKeyWithoutPassword : SecurityAPITestObject
	{
		protected static string host;
		protected static string user;
		protected static string keyPath;
		protected static string keyPassword;
		protected static string localPath;
		protected static string remoteDir;
		protected static string remoteFilePath;
		protected static string localDir;


		[SetUp]
		public virtual void SetUp()
		{

			host = TestContextParameter("gx_ftp_host");
			user = TestContextParameter("gx_sftp_user");
			string known_hosts_content_base64 = TestContextParameter("gx_ftp_known_hosts_content_base64");
			keyPath = Path.Combine(BASE_PATH, "Temp", "sftptest", "key", "id_rsa");
			string id_rsaConentBase64 = TestContextParameter("gx_ftp_id_rsa_content_base64");
			File.WriteAllBytes(keyPath, Convert.FromBase64String(id_rsaConentBase64));
			keyPassword = TestContextParameter("gx_sftp_key_password");
			localPath = Path.Combine(BASE_PATH, "Temp", "sftptest", "sftptest1.txt");
			remoteDir = "sftp";
			remoteFilePath = "sftp/sftptest1.txt";
			localDir = Path.Combine(BASE_PATH, "Temp", "sftptest", "back");
		}

		private SftpClient TestConnection(SftpOptions options)
		{
			SftpClient client = new SftpClient();
			bool connected = client.Connect(options);
			True(connected, client);
			return client;
		}

		private void TestPut(SftpClient client)
		{
			bool put = client.Put(localPath, remoteDir);
			True(put, client);
		}

		private void TestGet(SftpClient client)
		{
			bool get = client.Get(remoteFilePath, localDir);
			True(get, client);
		}

		[Test]
		public void TestWithKey()
		{
			SftpOptions options = new SftpOptions();
			options.Host = host;
			options.User = user;
			options.AllowHostKeyChecking = false;
			options.KeyPassword = keyPassword;
			SftpClient client = TestConnection(options);
			TestPut(client);
			TestGet(client);
			client.Disconnect();
		}
	}
}
