using System;
using System.Collections.Generic;
using System.IO;
using Genexus.Compression;
using GeneXus.Utils;
using Xunit;

namespace xUnitTesting
{
	public class TestCompression : IDisposable
	{
		private List<string> files;
		private DirectoryInfo testDirectory;
		private GXBaseCollection<SdtMessages_Message> messages = null;

		public TestCompression()
		{
			testDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "testCompressor"));
			testDirectory.Create();
			files = new List<string>();
			string content = "This is a sample text to test the compression functionality.";
			for (int i = 0; i < 3; i++)
			{
				string filePath = Path.Combine(testDirectory.FullName, $"testFile{i}.txt");
				File.WriteAllText(filePath, content);
				files.Add(filePath);
			}
		}

		public void Dispose()
		{
			foreach (string filePath in files)
			{
				File.Delete(filePath);
			}
			testDirectory.Delete(true);
		}

		[Fact]
		public void TestCompressToZip()
		{
			string outputPath = Path.Combine(testDirectory.FullName, "output.zip");
			bool result = GXCompressor.CompressFiles(files, outputPath, "ZIP", ref messages);
			Assert.True(result);
			Assert.True(File.Exists(outputPath));
		}

		[Fact]
		public void TestCompressToTar()
		{
			string outputPath = Path.Combine(testDirectory.FullName, "output.tar");
			bool result = GXCompressor.CompressFiles(files, outputPath, "TAR", ref messages);
			Assert.True(result);
			Assert.True(File.Exists(outputPath));
		}

		[Fact]
		public void TestCompressToGzip()
		{
			string outputPath = Path.Combine(testDirectory.FullName, "output.gz");
			string inputFilePath = Path.Combine(testDirectory.FullName, "test.txt");
			File.WriteAllText(inputFilePath, "This is a sample text to test the compression functionality.");
			List<string> singleFileCollection = new List<string> { inputFilePath };
			bool result = GXCompressor.CompressFiles(singleFileCollection, outputPath, "GZIP", ref messages);
			Assert.True(result);
			Assert.True(File.Exists(outputPath));
		}

		[Fact]
		public void TestCompressToJar()
		{
			string outputPath = Path.Combine(testDirectory.FullName, "output.jar");
			bool result = GXCompressor.CompressFiles(files, outputPath, "JAR", ref messages);
			Assert.True(result);
			Assert.True(File.Exists(outputPath));
		}

		[Fact]
		public void TestUnsupportedFormat()
		{
			string outputPath = Path.Combine(testDirectory.FullName, "output.unknown");
			bool result = GXCompressor.CompressFiles(files, outputPath, "UNKNOWN", ref messages);
			Assert.False(result);
		}

		[Fact]
		public void TestDecompressZip()
		{
			string compressedPath = Path.Combine(testDirectory.FullName, "output.zip");
			GXCompressor.CompressFiles(files, compressedPath, "ZIP", ref messages);
			string decompressDirectory = Path.Combine(testDirectory.FullName, "decompressZip");
			Directory.CreateDirectory(decompressDirectory);
			bool result = GXCompressor.Decompress(compressedPath, decompressDirectory, ref messages);
			Assert.True(result);
			Assert.True(Directory.GetFiles(decompressDirectory).Length > 0);
		}

		[Fact]
		public void TestDecompressTar()
		{
			string compressedPath = Path.Combine(testDirectory.FullName, "output.tar");
			GXCompressor.CompressFiles(files, compressedPath, "TAR", ref messages);
			string decompressDirectory = Path.Combine(testDirectory.FullName, "decompressTar");
			Directory.CreateDirectory(decompressDirectory);
			bool result = GXCompressor.Decompress(compressedPath, decompressDirectory, ref messages);
			Assert.True(result);
			Assert.True(Directory.GetFiles(decompressDirectory).Length > 0);
		}

		[Fact]
		public void TestDecompressGzip()
		{
			string compressedPath = Path.Combine(testDirectory.FullName, "output.gz");
			List<string> singleFileCollection = new List<string> { files[0] };
			GXCompressor.CompressFiles(singleFileCollection, compressedPath, "GZIP", ref messages);
			string decompressDirectory = Path.Combine(testDirectory.FullName, "decompressGzip");
			Directory.CreateDirectory(decompressDirectory);
			bool result = GXCompressor.Decompress(compressedPath, decompressDirectory, ref messages);
			Assert.True(result);
			Assert.True(Directory.GetFiles(decompressDirectory).Length > 0);
		}

		[Fact]
		public void TestDecompressJar()
		{
			string compressedPath = Path.Combine(testDirectory.FullName, "output.jar");
			GXCompressor.CompressFiles(files, compressedPath, "JAR", ref messages);
			string decompressDirectory = Path.Combine(testDirectory.FullName, "decompressJar");
			Directory.CreateDirectory(decompressDirectory);
			bool result = GXCompressor.Decompress(compressedPath, decompressDirectory, ref messages);
			Assert.True(result);
			Assert.True(Directory.GetFiles(decompressDirectory).Length > 0);
		}
	}
}
