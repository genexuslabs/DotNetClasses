using System.Collections.Generic;
using GeneXus.Utils;

namespace Genexus.Compression
{
	/**
     * Compresses files interactively; files can be added until the Save method is executed.
     */
	public class Compression
	{
		private string destinationPath;
		private CompressionConfiguration compressionConfiguration;
		private GXBaseCollection<SdtMessages_Message> messages;
		private List<string> filesToCompress;

		public Compression()
		{
			destinationPath = string.Empty;
			compressionConfiguration = new CompressionConfiguration();
			messages = new GXBaseCollection<SdtMessages_Message>();
			filesToCompress = new List<string>();
		}

		public Compression(string destinationPath, CompressionConfiguration configuration, GXBaseCollection<SdtMessages_Message> messages)
		{
			this.destinationPath = destinationPath;
			this.compressionConfiguration = configuration;
			this.messages = messages;
			filesToCompress = new List<string>();
		}

		public void SetDestinationPath(string path)
		{
			destinationPath = path;
		}

		public void AddElement(string filePath)
		{
			filesToCompress.Add(filePath);
		}

		public bool Save()
		{
			return GXCompressor.Compress(filesToCompress, destinationPath, compressionConfiguration, ref messages);
		}

		public void Clear()
		{
			destinationPath = "";
			filesToCompress = new List<string>();
		}
	}
}
