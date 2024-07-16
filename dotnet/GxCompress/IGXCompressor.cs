using GeneXus.Utils;
using System.Collections.Generic;

namespace Genexus.Compression
{
	public interface IGXCompressor
	{
		static bool CompressFiles(List<string> files, string path, string format, ref GXBaseCollection<SdtMessages_Message> messages) => false;
		static bool CompressFolder(string folder, string path, string format, ref GXBaseCollection<SdtMessages_Message> messages) => false;
		static Compression NewCompression(string path, string format, int dictionarySize, ref GXBaseCollection<SdtMessages_Message> messages) => new Compression();
		static bool Decompress(string file, string path, ref GXBaseCollection<SdtMessages_Message> messages) => false;
	}
}

