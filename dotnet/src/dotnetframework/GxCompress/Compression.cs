using System.Collections.Generic;
using GeneXus.Utils;

namespace Genexus.Compression
{
	public class Compression
	{

		private string path;
		private List<string> filesToCompress;
		private GXBaseCollection<SdtMessages_Message> messages;

		public Compression()
		{
			filesToCompress = new List<string>();
		}

		public Compression(string path, ref GXBaseCollection<SdtMessages_Message> messages)
		{
			this.path = path;
			this.messages = messages;
			filesToCompress = new List<string>();
		}

		public void AddElement(string filePath)
		{
			filesToCompress.Add(filePath);
		}

		public bool Save()
		{
			return GXCompressor.Compress(filesToCompress, path, ref this.messages);
		}

		public void Clear()
		{
			this.path = string.Empty;
			filesToCompress = new List<string>();
		}
	}
}
