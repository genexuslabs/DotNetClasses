using FluentFTP;
using GeneXusFtps.GeneXusCommons;
using GeneXusFtps.GeneXusFtpsUtils;
using SecurityAPICommons.Utils;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace GeneXusFtps.GeneXusFtps
{
    [SecuritySafeCritical]
    public sealed class FtpsClient : IFtpsClientObject, IDisposable
	{
        private FtpClient client;
        private string pwd;
        private ExtensionsWhiteList whiteList;

        [SecuritySafeCritical]
        public FtpsClient() : base()
        {
            this.client = null;
            this.whiteList = null;
        }

        /******** EXTERNAL OBJECT PUBLIC METHODS - BEGIN ********/
        [SecuritySafeCritical]
        public override bool Connect(FtpsOptions options)
        {
			if(options == null)
			{
				this.error.setError("FS000", "Options parameter is null");
				return false;
			}
            if (options.HasError())
            {
                this.error = options.GetError();
                return false;
            }
            if (SecurityUtils.compareStrings("", options.Host) || SecurityUtils.compareStrings("", options.User)
                    || SecurityUtils.compareStrings("", options.Password))
            {
                this.error.setError("FS001", "Empty connection data");
                return false;
            }

            this.client = new FtpClient
            {
                Host = options.Host,
                Port = options.Port,
                Credentials = new NetworkCredential(options.User, options.Password),
                DataConnectionType = SetConnectionMode(options),
                EncryptionMode = SetEncryptionMode(options),
                Encoding = Encoding.UTF8,
            };

            this.client.DownloadDataType = SetEncoding(options);
           
            SslProtocols protocols = SetProtocol(options); ;
            if (protocols ==  SslProtocols.None)
            {
                return false;
            }
            this.client.SslProtocols = protocols;
            this.client.DataConnectionEncryption = options.ForceEncryption;
            if (SecurityUtils.compareStrings("", options.TrustStorePath))
            {
                this.client.ValidateCertificate += (control, e1) =>
                {
                    e1.Accept = true;
                };
            }
            else
            {
				using (X509Certificate2 cert_grt = new X509Certificate2(options.TrustStorePath, options.TrustStorePassword))
				{
					client.ValidateCertificate += (control, e1) =>
					{

						X509Chain verify = new X509Chain();
						verify.Build(new X509Certificate2(e1.Certificate));
						e1.Accept = SecurityUtils.compareStrings(verify.ChainElements[verify.ChainElements.Count - 1].Certificate.Thumbprint, cert_grt.Thumbprint);
					};
				}
            }
            
            try
            {
                this.client.Connect();
                if (!this.client.LastReply.Success)
                {
                    this.client.Disconnect();
                    this.error.setError("FS008", "Connection error");
                    return false;
                }
            }
            catch (Exception e)
            {
                this.error.setError("FS002", "Connection error " + e.Message);
                this.client = null;
                return false;
            }
            if (!this.client.IsConnected)
            {
                this.error.setError("FS009", "Connection error");
                return false;
            }
            this.whiteList = options.WhiteList;
            return true;
        }

        [SecuritySafeCritical]
        public override bool Put(string localPath, string remoteDir)
        {
            if (this.whiteList != null)
            {
                if (!this.whiteList.IsValid(localPath))
                {
                    this.error.setError("WL001", "Invalid file extension");
                    return false;
                }
            }
			if(remoteDir == null)
			{
				this.error.setError("FS000", "RemoteDir parameter is null");
				return false;
			}
            if (this.client == null || !this.client.IsConnected)
            {
                this.error.setError("FS003", "The connection is invalid, reconect");
                return false;
            }
            try
            {
                if (!IsSameDir(remoteDir, this.client.GetWorkingDirectory()))
                {
                    this.client.SetWorkingDirectory(remoteDir);

                    this.pwd = remoteDir;
                }
            }
            catch (Exception e)
            {
                this.error.setError("FS013", "Error changing directory " + e.Message);
                return false;
            }
            bool isStored = false;
            try
            {
				using (FileStream fs = new FileStream(localPath, FileMode.Open))
				{
					isStored = this.client.Upload(fs, AddFileName(localPath, remoteDir), FtpRemoteExists.Overwrite, true);

				}

				if (!isStored)
				{
					this.error.setError("FS012", " Reply String: " + this.client.LastReply.ErrorMessage);
				}
				
            }
            catch (Exception e1)
            {
                this.error.setError("FS004", "Erorr uploading file to server " + e1.Message);
                return false;
            }
            return isStored;
        }

        [SecuritySafeCritical]
        public override bool Get(string remoteFilePath, string localDir)
        {
            if (this.whiteList != null)
            {
                if (!this.whiteList.IsValid(remoteFilePath))
                {
                    this.error.setError("WL002", "Invalid file extension");
                    return false;
                }
            }
			if(localDir == null)
			{
				this.error.setError("FS000", "LocalDir parameter is null");
				return false;
			}
            if (this.client == null || !this.client.IsConnected)
            {
                this.error.setError("FS010", "The connection is invalid, reconect");
                return false;
            }
            try
            {
                if (!IsSameDir(Path.GetDirectoryName(remoteFilePath), this.client.GetWorkingDirectory()))
                {
                    this.client.SetWorkingDirectory(Path.GetDirectoryName(remoteFilePath));

                    this.pwd = Path.GetDirectoryName(remoteFilePath);
                }
            }
            catch (Exception e)
            {
                this.error.setError("FS013", "Error changing directory " + e.Message);
                return false;
            }

			using (FileStream fileStream = File.Create(AddFileName(remoteFilePath, localDir)))
			{
				bool isDownloaded = false;

				try
				{
					isDownloaded = this.client.Download(fileStream, remoteFilePath, 0);
				}
				catch (Exception e1)
				{
					this.error.setError("FS005", "Error retrieving file " + e1.Message);
					fileStream.Close();
					return false;
				}

				if (fileStream == null || !isDownloaded)
				{
					this.error.setError("FS007", "Could not retrieve file");
					return false;
				}
			}
            return true;
        }

        [SecuritySafeCritical]
        public override void Disconnect()
        {
            try
            {
                this.client.Disconnect();
            }catch(Exception)
            {
                //NOOP
            }
        }

        [SecuritySafeCritical]
        public override string GetWorkingDirectory()
        {
            if (this.client == null || !this.client.IsConnected)
            {
                this.error.setError("FS007", "The connection is invalid, reconect");
                return "";
            }
            String pwd = "";
            try
            {
                pwd = this.client.GetWorkingDirectory();
            }
            catch (IOException)
            {
                this.error.setError("FS006", "Could not obtain working directory, try reconnect");
                return "";
            }
            if (pwd == null)
            {
                return this.pwd;
            }
            return pwd;
        }

        /******** EXTERNAL OBJECT PUBLIC METHODS - END ********/


        private FtpDataConnectionType SetConnectionMode(FtpsOptions options)
        {
            FtpConnectionMode mode = options.GetFtpConnectionMode();
            switch (mode)
            {
                case FtpConnectionMode.ACTIVE:
                    return FtpDataConnectionType.AutoActive;
                case FtpConnectionMode.PASSIVE:
                    return FtpDataConnectionType.PASV;
                default:
                    return FtpDataConnectionType.PASV;
            }
        }

        private FluentFTP.FtpEncryptionMode SetEncryptionMode(FtpsOptions options)
        {
            switch (options.GetFtpEncryptionMode())
            {
                case GeneXusFtpsUtils.FtpEncryptionMode.EXPLICIT:
                    return FluentFTP.FtpEncryptionMode.Explicit;

                case GeneXusFtpsUtils.FtpEncryptionMode.IMPLICIT:
                    return FluentFTP.FtpEncryptionMode.Implicit;
                default:
                    return FluentFTP.FtpEncryptionMode.Explicit;
            }
        }

        private FtpDataType SetEncoding(FtpsOptions options)
        {
            switch (options.GetFtpEncoding())
            {
                case FtpEncoding.BINARY:
                    return FtpDataType.Binary;
                case FtpEncoding.ASCII:
                    return FtpDataType.ASCII;
                default:
                    return FtpDataType.Binary;
            }
        }

        private SslProtocols SetProtocol(FtpsOptions options)
        {
#pragma warning disable CA5397 // Do not use deprecated SslProtocols values
			switch (options.GetFtpsProtocol())
            {
                case FtpsProtocol.TLS1_0:
                    return SslProtocols.Tls;
                case FtpsProtocol.TLS1_1:
                    return SslProtocols.Tls11;
                case FtpsProtocol.TLS1_2:
                    return SslProtocols.Tls12;
                case FtpsProtocol.SSLv2:
                    this.error.setError("FS0014", "Deprecated protocol, not implemented for .Net");
                    return SslProtocols.None;
                case FtpsProtocol.SSLv3:
                    this.error.setError("FS0015", "Deprecated protocol, not implemented for .Net");
                    return SslProtocols.None;
                default:
					return SslProtocols.Tls;
			}
#pragma warning restore CA5397 // Do not use deprecated SslProtocols values
		}

		private bool IsSameDir(String path1, String path2)
        {
            string path11 = Path.GetDirectoryName(path1);
            string path22 = Path.GetDirectoryName(path2);
			return SecurityUtils.compareStrings(path11, path22);
        }

        private Stream PathToStream(string path)
        {

            FileStream stream = new FileStream(path, FileMode.Open);
            return stream;
        }

        private string AddFileName(string originPath, string dir)
        {


            string fileName = Path.GetFileName(originPath);
            if (SecurityUtils.compareStrings("", dir))
            {
                return fileName;
            }
            string pathArr = "";
            if (dir.Contains("/"))
            {
                pathArr = dir + "/" + fileName;
            }
            else
            {
                pathArr = dir + "\\" + fileName;
            }

            return pathArr;
        }

		public void Dispose()
		{
			if(this.client != null)
			{
				this.client.Dispose();
				this.client = null;
			}
		}
	}
}
