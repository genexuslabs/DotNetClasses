using System;
using System.Collections.Generic;
using System.IO;
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
			filesToCompress = new List<FileInfo>();
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
				GXLogging.Error(log, $"File does not exist: {0}", file.FullName);
			}
		}

		public void AddFolder(string folderPath)
		{
			try
			{
				var files = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories);
				foreach (string file in files)
				{
					AddFile(Path.GetFullPath(file));
				}
			}
			catch (Exception e)
			{
				GXLogging.Error(log, $"Failed to add foler", e);
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
			filesToCompress = new List<FileInfo>();
		}
	}
}
