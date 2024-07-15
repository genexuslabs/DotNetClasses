using System.Collections.Generic;

namespace Genexus.Compression
{
	public interface IGXCompressor
	{
		static CompressionMessage CompressFiles(List<string> files, string path, string format) => null;
		static CompressionMessage CompressFolder(string folder, string path, string format) => null;
		static Compression NewCompression(string path, string format, int dictionarySize) => new Compression();
		static CompressionMessage Decompress(string file, string path) => null;
	}
}

