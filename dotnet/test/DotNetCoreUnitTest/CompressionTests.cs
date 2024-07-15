using System;
using System.Collections.Generic;
using System.IO;
using Genexus.Compression;
using Xunit;

namespace xUnitTesting
{
	public class TestCompression : IDisposable
	{
		private List<string> files;
		private DirectoryInfo testDirectory;

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
			CompressionMessage result = GXCompressor.CompressFiles(files, outputPath, "ZIP");
			Assert.True(result.IsSuccessfulOperation);
			Assert.True(File.Exists(outputPath));
		}

		[Fact]
		public void TestCompressToTar()
		{
			string outputPath = Path.Combine(testDirectory.FullName, "output.tar");
			CompressionMessage result = GXCompressor.CompressFiles(files, outputPath, "TAR");
			Assert.True(result.IsSuccessfulOperation);
			Assert.True(File.Exists(outputPath));
		}

		[Fact]
		public void TestCompressToGzip()
		{
			string outputPath = Path.Combine(testDirectory.FullName, "output.gz");
			string inputFilePath = Path.Combine(testDirectory.FullName, "test.txt");
			File.WriteAllText(inputFilePath, "This is a sample text to test the compression functionality.");
			List<string> singleFileCollection = new List<string> { inputFilePath };
			CompressionMessage result = GXCompressor.CompressFiles(singleFileCollection, outputPath, "GZIP");
			Assert.True(result.IsSuccessfulOperation);
			Assert.True(File.Exists(outputPath));
		}

		[Fact]
		public void TestCompressToJar()
		{
			string outputPath = Path.Combine(testDirectory.FullName, "output.jar");
			CompressionMessage result = GXCompressor.CompressFiles(files, outputPath, "JAR");
			Assert.True(result.IsSuccessfulOperation);
			Assert.True(File.Exists(outputPath));
		}

		[Fact]
		public void TestUnsupportedFormat()
		{
			string outputPath = Path.Combine(testDirectory.FullName, "output.unknown");
			CompressionMessage result = GXCompressor.CompressFiles(files, outputPath, "UNKNOWN");
			Assert.False(result.IsSuccessfulOperation);
		}

		[Fact]
		public void TestDecompressZip()
		{
			string compressedPath = Path.Combine(testDirectory.FullName, "output.zip");
			GXCompressor.CompressFiles(files, compressedPath, "ZIP");
			string decompressDirectory = Path.Combine(testDirectory.FullName, "decompressZip");
			Directory.CreateDirectory(decompressDirectory);
			CompressionMessage result = GXCompressor.Decompress(compressedPath, decompressDirectory);
			Assert.True(result.IsSuccessfulOperation);
			Assert.True(Directory.GetFiles(decompressDirectory).Length > 0);
		}

		[Fact]
		public void TestDecompressTar()
		{
			string compressedPath = Path.Combine(testDirectory.FullName, "output.tar");
			GXCompressor.CompressFiles(files, compressedPath, "TAR");
			string decompressDirectory = Path.Combine(testDirectory.FullName, "decompressTar");
			Directory.CreateDirectory(decompressDirectory);
			CompressionMessage result = GXCompressor.Decompress(compressedPath, decompressDirectory);
			Assert.True(result.IsSuccessfulOperation);
			Assert.True(Directory.GetFiles(decompressDirectory).Length > 0);
		}

		[Fact]
		public void TestDecompressGzip()
		{
			string compressedPath = Path.Combine(testDirectory.FullName, "output.gz");
			List<string> singleFileCollection = new List<string> { files[0] };
			GXCompressor.CompressFiles(singleFileCollection, compressedPath, "GZIP");
			string decompressDirectory = Path.Combine(testDirectory.FullName, "decompressGzip");
			Directory.CreateDirectory(decompressDirectory);
			CompressionMessage result = GXCompressor.Decompress(compressedPath, decompressDirectory);
			Assert.True(result.IsSuccessfulOperation);
			Assert.True(Directory.GetFiles(decompressDirectory).Length > 0);
		}

		[Fact]
		public void TestDecompressJar()
		{
			string compressedPath = Path.Combine(testDirectory.FullName, "output.jar");
			GXCompressor.CompressFiles(files, compressedPath, "JAR");
			string decompressDirectory = Path.Combine(testDirectory.FullName, "decompressJar");
			Directory.CreateDirectory(decompressDirectory);
			CompressionMessage result = GXCompressor.Decompress(compressedPath, decompressDirectory);
			Assert.True(result.IsSuccessfulOperation);
			Assert.True(Directory.GetFiles(decompressDirectory).Length > 0);
		}
	}
}
