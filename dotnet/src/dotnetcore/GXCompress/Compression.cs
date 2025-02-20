using GeneXus.Utils;

namespace Genexus.Compression
{
	public class Compression
	{

		private string absolutePath;
		private long maxCombinedFileSize;
		private GXBaseCollection<SdtMessages_Message> messages;
		private List<string> filesToCompress;

		public Compression()
		{
			absolutePath = string.Empty;
			maxCombinedFileSize = 0;
			messages = new GXBaseCollection<SdtMessages_Message>();
			filesToCompress = new List<string>();
		}

		public Compression(string absolutePath, long maxCombinedFileSize, GXBaseCollection<SdtMessages_Message> messages)
		{
			this.absolutePath = absolutePath;
			this.maxCombinedFileSize = maxCombinedFileSize;
			this.messages = messages;
			this.filesToCompress = new List<string>();
		}

		public void SetAbsolutePath(string path)
		{
			this.absolutePath = path;
		}

		public void AddElement(string filePath)
		{
			filesToCompress.Add(filePath);
		}

		public bool Save()
		{
			return GXCompressor.Compress(filesToCompress, absolutePath, maxCombinedFileSize, ref messages);
		}

		public void Clear()
		{
			absolutePath = "";
			filesToCompress = new List<string>();
		}
	}
}
