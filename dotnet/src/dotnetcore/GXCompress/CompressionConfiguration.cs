
namespace Genexus.Compression
{
	public class CompressionConfiguration
	{
		public long maxCombinedFileSize = -1;
		public long maxIndividualFileSize = -1;
		public int maxFileCount = -1;
		public string targetDirectory = "";

		public CompressionConfiguration() { }

		public CompressionConfiguration(long maxCombinedFileSize, long maxIndividualFileSize, int maxFileCount, string targetDirectory)
		{
			this.maxCombinedFileSize = maxCombinedFileSize;
			this.maxIndividualFileSize = maxIndividualFileSize;
			this.maxFileCount = maxFileCount;
			this.targetDirectory = (!string.IsNullOrEmpty(targetDirectory)) ? targetDirectory : "";
		}
	}
}
