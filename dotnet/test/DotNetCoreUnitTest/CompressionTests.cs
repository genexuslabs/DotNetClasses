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
			int result = GXCompressor.CompressFiles(files, outputPath, "ZIP");
			Assert.Equal(0, result);
			Assert.True(File.Exists(outputPath));
		}

		[Fact]
		public void TestCompressToTar()
		{
			string outputPath = Path.Combine(testDirectory.FullName, "output.tar");
			int result = GXCompressor.CompressFiles(files, outputPath, "TAR");
			Assert.Equal(0, result);
			Assert.True(File.Exists(outputPath));
		}

		[Fact]
		public void TestCompressToJar()
		{
			string outputPath = Path.Combine(testDirectory.FullName, "output.jar");
			int result = GXCompressor.CompressFiles(files, outputPath, "JAR");
			Assert.Equal(0, result);
			Assert.True(File.Exists(outputPath));
		}

		[Fact]
		public void TestUnsupportedFormat()
		{
			string outputPath = Path.Combine(testDirectory.FullName, "output.unknown");
			int result = GXCompressor.CompressFiles(files, outputPath, "UNKNOWN");
			Assert.Equal(-3, result);
		}
	}
}
