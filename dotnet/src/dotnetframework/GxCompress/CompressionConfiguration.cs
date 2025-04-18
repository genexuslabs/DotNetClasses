
namespace Genexus.Compression
{
	public class CompressionConfiguration
	{
		public long maxCombinedFileSize = -1;
		public long maxIndividualFileSize = -1;
		public int maxFileCount = -1;
		public string targetDirectory = "";

		public CompressionConfiguration() { }
	}
}
