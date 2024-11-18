using Genexus.Compression;
using GeneXus.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace xUnitTesting
{
	public class TestCompression
	{
		private GXBaseCollection<SdtMessages_Message> messages = null;

		public TestCompression()
		{
		}

		private void CreateTestFiles(string rootPath, out List<string> filePaths, out List<string> dirPaths)
		{
			filePaths = new List<string>();
			dirPaths = new List<string>();

			// Create files in the root directory
			for (int i = 0; i < 3; i++)
			{
				string filePath = Path.Combine(rootPath, $"testfile_{i}.txt");
				File.WriteAllText(filePath, $"This is test file {i}");
				filePaths.Add(filePath);
			}

			// Create subdirectories with files
			for (int i = 0; i < 2; i++)
			{
				string dirPath = Path.Combine(rootPath, $"testdir_{i}");
				Directory.CreateDirectory(dirPath);
				dirPaths.Add(dirPath);

				for (int j = 0; j < 2; j++)
				{
					string filePath = Path.Combine(dirPath, $"testfile_{i}_{j}.txt");
					File.WriteAllText(filePath, $"This is test file {i}_{j}");
					filePaths.Add(filePath);
				}
			}
		}

		private void CleanupTestFiles(List<string> filePaths, List<string> dirPaths, string rootPath)
		{
			foreach (string filePath in filePaths)
			{
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
				}
			}

			foreach (string dirPath in dirPaths)
			{
				if (Directory.Exists(dirPath))
				{
					Directory.Delete(dirPath, true);
				}
			}

			if (Directory.Exists(rootPath))
			{
				Directory.Delete(rootPath, true);
			}
		}

		[Fact]
		public void TestZip()
		{
			string rootPath = Path.Combine(Path.GetTempPath(), "TestZip_" + Guid.NewGuid().ToString());
			Directory.CreateDirectory(rootPath);

			List<string> filePaths;
			List<string> dirPaths;

			CreateTestFiles(rootPath, out filePaths, out dirPaths);

			try
			{
				List<string> itemsToCompress = new List<string>();
				itemsToCompress.AddRange(filePaths);
				itemsToCompress.AddRange(dirPaths);

				string zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");

				bool compressResult = GXCompressor.Compress(itemsToCompress, zipFilePath, -1, ref messages);
				Assert.True(compressResult);

				CleanupTestFiles(filePaths, dirPaths, rootPath);
				Directory.CreateDirectory(rootPath);

				bool decompressResult = GXCompressor.Decompress(zipFilePath, rootPath, ref messages);
				Assert.True(decompressResult);

				foreach (string filePath in filePaths)
				{
					Assert.True(File.Exists(filePath));
				}

				foreach (string dirPath in dirPaths)
				{
					Assert.True(Directory.Exists(dirPath));
				}

				CleanupTestFiles(filePaths, dirPaths, rootPath);

				if (File.Exists(zipFilePath))
				{
					File.Delete(zipFilePath);
				}
			}
			finally
			{
				if (Directory.Exists(rootPath))
				{
					Directory.Delete(rootPath, true);
				}
			}
		}

		[Fact]
		public void TestTar()
		{
			string rootPath = Path.Combine(Path.GetTempPath(), "TestTar_" + Guid.NewGuid().ToString());
			Directory.CreateDirectory(rootPath);

			List<string> filePaths;
			List<string> dirPaths;

			CreateTestFiles(rootPath, out filePaths, out dirPaths);

			try
			{
				List<string> itemsToCompress = new List<string>();
				itemsToCompress.AddRange(filePaths);
				itemsToCompress.AddRange(dirPaths);

				string tarFilePath = Path.Combine(Path.GetTempPath(), "test.tar");

				bool compressResult = GXCompressor.Compress(itemsToCompress, tarFilePath, -1, ref messages);
				Assert.True(compressResult);

				CleanupTestFiles(filePaths, dirPaths, rootPath);
				Directory.CreateDirectory(rootPath);

				bool decompressResult = GXCompressor.Decompress(tarFilePath, rootPath, ref messages);
				Assert.True(decompressResult);

				foreach (string filePath in filePaths)
				{
					Assert.True(File.Exists(filePath));
				}

				foreach (string dirPath in dirPaths)
				{
					Assert.True(Directory.Exists(dirPath));
				}

				CleanupTestFiles(filePaths, dirPaths, rootPath);

				if (File.Exists(tarFilePath))
				{
					File.Delete(tarFilePath);
				}
			}
			finally
			{
				if (Directory.Exists(rootPath))
				{
					Directory.Delete(rootPath, true);
				}
			}
		}

		[Fact]
		public void TestGZip()
		{
			string rootPath = Path.Combine(Path.GetTempPath(), "TestGZip_" + Guid.NewGuid().ToString());
			Directory.CreateDirectory(rootPath);

			List<string> filePaths;
			List<string> dirPaths;

			string testFilePath = Path.Combine(rootPath, "testfile.txt");
			File.WriteAllText(testFilePath, "This is a test file for GZip compression.");
			filePaths = new List<string> { testFilePath };
			dirPaths = new List<string>();

			try
			{
				string gzipFilePath = Path.Combine(Path.GetTempPath(), "test.gz");

				bool compressResult = GXCompressor.Compress(new List<string> { testFilePath }, gzipFilePath, 0, ref messages);
				Assert.True(compressResult);

				CleanupTestFiles(filePaths, dirPaths, rootPath);
				Directory.CreateDirectory(rootPath);

				bool decompressResult = GXCompressor.Decompress(gzipFilePath, rootPath, ref messages);
				Assert.True(decompressResult);

				Assert.True(File.Exists(testFilePath));

				CleanupTestFiles(filePaths, dirPaths, rootPath);

				if (File.Exists(gzipFilePath))
				{
					File.Delete(gzipFilePath);
				}
			}
			finally
			{
				if (Directory.Exists(rootPath))
				{
					Directory.Delete(rootPath, true);
				}
			}
		}

		[Fact]
		public void TestJar()
		{
			string rootPath = Path.Combine(Path.GetTempPath(), "TestJar_" + Guid.NewGuid().ToString());
			Directory.CreateDirectory(rootPath);

			List<string> filePaths;
			List<string> dirPaths;

			CreateTestFiles(rootPath, out filePaths, out dirPaths);

			try
			{
				List<string> itemsToCompress = new List<string>();
				itemsToCompress.AddRange(filePaths);
				itemsToCompress.AddRange(dirPaths);

				string jarFilePath = Path.Combine(Path.GetTempPath(), "test.jar");

				bool compressResult = GXCompressor.Compress(itemsToCompress, jarFilePath, -1, ref messages);
				Assert.True(compressResult);

				CleanupTestFiles(filePaths, dirPaths, rootPath);
				Directory.CreateDirectory(rootPath);

				bool decompressResult = GXCompressor.Decompress(jarFilePath, rootPath, ref messages);
				Assert.True(decompressResult);

				foreach (string filePath in filePaths)
				{
					Assert.True(File.Exists(filePath));
				}

				foreach (string dirPath in dirPaths)
				{
					Assert.True(Directory.Exists(dirPath));
				}

				CleanupTestFiles(filePaths, dirPaths, rootPath);

				if (File.Exists(jarFilePath))
				{
					File.Delete(jarFilePath);
				}
			}
			finally
			{
				if (Directory.Exists(rootPath))
				{
					Directory.Delete(rootPath, true);
				}
			}
		}

		[Fact]
		public void TestUnsupportedFormat()
		{
			string rootPath = Path.Combine(Path.GetTempPath(), "TestUnsupportedFormat_" + Guid.NewGuid().ToString());
			Directory.CreateDirectory(rootPath);

			List<string> filePaths;
			List<string> dirPaths;

			CreateTestFiles(rootPath, out filePaths, out dirPaths);

			try
			{
				List<string> itemsToCompress = new List<string>();
				itemsToCompress.AddRange(filePaths);
				itemsToCompress.AddRange(dirPaths);

				string unsupportedFilePath = Path.Combine(Path.GetTempPath(), "test.unsupported");

				bool compressResult = GXCompressor.Compress(itemsToCompress, unsupportedFilePath, -1, ref messages);
				Assert.False(compressResult);
			}
			finally
			{
				CleanupTestFiles(filePaths, dirPaths, rootPath);
			}
		}
	}
}