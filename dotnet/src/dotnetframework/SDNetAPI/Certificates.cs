using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;

namespace Artech.Genexus.SDAPI
{
	internal class Certificates
	{
		#region Private Data

		private const int FILE_NOT_FOUND = 1;
		private const int INVALID_ID = 2;

		internal int ErrorCode { get; set; }

		internal const string DATA_FOLDER = @"private\";

		private Dictionary<string, string> m_types = new Dictionary<string, string>();
		private Dictionary<string, ConfigurationProps> m_data = new Dictionary<string, ConfigurationProps>();
		private static Certificates m_instance = new Certificates();
		internal static Certificates Instance { get { return m_instance; } }

		private static Dictionary<string, string> m_AndroidUserTokens = new Dictionary<string, string>();

		private Certificates()
		{
			ErrorCode = 0;
			Load();
		}

		#endregion

		internal string getAndroidUserTokenFor(string applicationId)
		{
			applicationId = applicationId.ToLower();
			if (m_AndroidUserTokens.ContainsKey(applicationId))
				return m_AndroidUserTokens[applicationId];
			return null;
		}

		internal void setAndroidUserTokenFor(string applicationId, string userToken)
		{
			applicationId = applicationId.ToLower();
			if (!m_AndroidUserTokens.ContainsKey(applicationId))
				m_AndroidUserTokens[applicationId] = userToken;
		}


		internal string TypeFor(string entryPoint)
		{
			entryPoint = entryPoint.ToLower();
			if (m_types.ContainsKey(entryPoint))
				return m_types[entryPoint];

			return string.Empty;
		}

		internal ConfigurationProps PropertiesFor(string entryPoint)
		{
			entryPoint = entryPoint.ToLower();
			if (m_data.ContainsKey(entryPoint))
				return m_data[entryPoint];

			if (ErrorCode != FILE_NOT_FOUND)
				ErrorCode = INVALID_ID;
			return new ConfigurationProps();
		}

		internal void MergePropertiesFor(string entryPoint, ConfigurationProps properties)
		{
			entryPoint = entryPoint.ToLower();
			if (m_data.ContainsKey(entryPoint))
			{
				ConfigurationProps prev = m_data[entryPoint];
				if (!string.IsNullOrEmpty(properties.iOScertificate))
				{
					prev.iOScertificate = properties.iOScertificate;
					prev.iOSuseSandboxServer = properties.iOSuseSandboxServer;
				}
				if (!string.IsNullOrEmpty(properties.iOScertificatePassword))
					prev.iOScertificatePassword = properties.iOScertificatePassword;

				if (!string.IsNullOrEmpty(properties.androidSenderAPIKey))
					prev.androidSenderAPIKey = properties.androidSenderAPIKey;

				if (!string.IsNullOrEmpty(properties.androidSenderId))
					prev.androidSenderId = properties.androidSenderId;

				if (!string.IsNullOrEmpty(properties.WNSClientSecret))
					prev.WNSClientSecret = properties.WNSClientSecret;

				if (!string.IsNullOrEmpty(properties.WNSPackageSecurityIdentifier))
					prev.WNSPackageSecurityIdentifier = properties.WNSPackageSecurityIdentifier;
			}
			else
			{
				m_data[entryPoint] = properties;
			}
		}

		private void Load()
		{
			string fileName = DATA_FOLDER + @"notifications.json";
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CertificatesData));
			if (Utils.TryGetFile(ref fileName))
			{
				using (FileStream fs = File.OpenRead(fileName))
				{
					CertificatesData data = serializer.ReadObject(fs) as CertificatesData;
					foreach (Notification main in data.Notifications)
					{
						main.Properties.ResolvePaths();
						m_data[main.Name.ToLower()] = main.Properties;
						m_types[main.Name.ToLower()] = main.Type;
					}
				}
			}
			else
			{
				ErrorCode = FILE_NOT_FOUND;
			}
		}
	}

	#region Json Object

	[Serializable]
	public class CertificatesData
	{
		public Notification[] Notifications;
	}

	[Serializable]
	public class Notification
	{
		public string Type;
		public string Name;
		public ConfigurationProps Properties = new ConfigurationProps();
	}

	[Serializable]
	public class ConfigurationProps
	{
		//Must match SDPattern properties definitions in \Interop\GX\ptys\sdpatterns.gxp
		public string iOScertificate = string.Empty;
		public string iOScertificatePassword = string.Empty;
		public bool iOSuseSandboxServer = false;
		public string androidSenderId = string.Empty;
		public string androidSenderAPIKey = string.Empty;
		public string WNSPackageSecurityIdentifier = string.Empty;
		public string WNSClientSecret = string.Empty;


		internal void ResolvePaths()
		{
			iOScertificate = ScanFile(iOScertificate);
		}

		private static string ScanFile(string file)
		{
			if (string.IsNullOrEmpty(file))
				return string.Empty;
			if (File.Exists(file))
				return file;
			string certDir = Path.Combine(Certificates.DATA_FOLDER, file);
			if (Utils.TryGetFile(ref certDir))
				return certDir;
			return string.Empty;
		}
	}

	#endregion

}
