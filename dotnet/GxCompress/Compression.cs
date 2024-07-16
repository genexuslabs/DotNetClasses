using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeneXus;
using GeneXus.Utils;

namespace Genexus.Compression
{
	public class Compression
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger(typeof(Compression).FullName);

		private string path;
		private string format;
		private GXBaseCollection<SdtMessages_Message> messages;
		private List<FileInfo> filesToCompress;

		public Compression()
		{
			filesToCompress = new List<FileInfo>();
		}

		public Compression(string path, string format, ref GXBaseCollection<SdtMessages_Message> messages)
		{
			this.path = path;
			this.format = format;
			this.messages = messages;
			this.filesToCompress = new List<FileInfo>();
		}

		public void SetPath(string path)
		{
			this.path = path;
		}

		public void SetFormat(string format)
		{
			this.format = format;
		}

		public void AddFile(string filePath)
		{
			FileInfo file = new FileInfo(filePath);
			if (file.Exists)
			{
				filesToCompress.Add(file);
			}
			else
			{
				StorageMessages("File does not exist: " + file.FullName, messages);
				GXLogging.Error(log, $"File does not exist: {0}", file.FullName);
			}
		}

		public void AddFolder(string folderPath)
		{
			DirectoryInfo folder = new DirectoryInfo(folderPath);
			if (folder.Exists && folder.Attributes.HasFlag(FileAttributes.Directory))
			{
				FileInfo[] files = folder.GetFiles();
				DirectoryInfo[] directories = folder.GetDirectories();
				foreach (DirectoryInfo dir in directories)
				{
					AddFolder(dir.FullName);
				}
				foreach (FileInfo file in files)
				{
					AddFile(file.FullName);
				}
			}
			else
			{
				StorageMessages("Folder does not exist or is not a directory: " +  folder.FullName, messages);
				GXLogging.Error(log, "Folder does not exist or is not a directory: {0}", folder.FullName);
			}
		}

		public bool Save()
		{
			List<string> paths = new List<string>();
			foreach (FileInfo file in filesToCompress)
			{
				paths.Add(file.FullName);
			}
			return GXCompressor.CompressFiles(paths, path, format, ref this.messages);
		}

		public void Clear()
		{
			this.path = string.Empty;
			this.format = string.Empty;
			this.filesToCompress = new List<FileInfo>();
		}

		private void StorageMessages(string error, GXBaseCollection<SdtMessages_Message> messages)
		{
			if (messages != null && messages.Count > 0)
			{
				SdtMessages_Message msg = new()
				{
					gxTpr_Type = 1,
					gxTpr_Description = error
				};
				messages.Add(msg);
			}
		}
	}
}
