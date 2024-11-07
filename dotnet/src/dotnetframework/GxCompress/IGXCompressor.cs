using GeneXus.Utils;
using System.Collections.Generic;

namespace Genexus.Compression
{
	public interface IGXCompressor
	{
		static bool Compress(List<string> files, string path, ref GXBaseCollection<SdtMessages_Message> messages) => false;
		static Compression NewCompression(string path, int dictionarySize, ref GXBaseCollection<SdtMessages_Message> messages) => new Compression();
		static bool Decompress(string file, string path, ref GXBaseCollection<SdtMessages_Message> messages) => false;
	}
}

