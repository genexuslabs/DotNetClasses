namespace Genexus.Compression
{
	public interface IGXCompressor
	{
		static int CompressFiles(List<string> files, string path, string format) => 0;
		static int CompressFolder(string folder, string path, string format) => 0;
		static Compression NewCompression(string path, string format, int dictionarySize) => new Compression();
		static int Decompress(string file, string path) => 0;
	}
}

