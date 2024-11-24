using Genexus.Compression;
using GeneXus.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace xUnitTesting
{
	public class TestCompression
	{
		private GXBaseCollection<SdtMessages_Message> messages = null;

		public TestCompression()
		{
		}

		[Fact]
		public void TestZip()
		{
			string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			string decompressedFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			string compressedFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");

			try
			{
				CreateTestFilesAndFolders(tempFolder);

				var filesToCompress = new List<string> { tempFolder };

				bool compressionResult = GXCompressor.Compress(filesToCompress, compressedFile, -1, ref messages);
				Assert.True(compressionResult, "Compression failed");
				Assert.True(File.Exists(compressedFile), "Compressed file does not exist");

				bool decompressionResult = GXCompressor.Decompress(compressedFile, decompressedFolder, ref messages);
				Assert.True(decompressionResult, "Decompression failed");

				bool directoriesAreEqual = CompareDirectories(tempFolder, decompressedFolder);
				Assert.True(directoriesAreEqual, "Decompressed content does not match the original");
			}
			finally
			{
				DeleteTestFilesAndFolders(tempFolder);
				DeleteTestFilesAndFolders(decompressedFolder);
				if (File.Exists(compressedFile))
				{
					File.Delete(compressedFile);
				}
			}

			bool CompareDirectories(string dir1, string dir2)
			{
				string[] dir1Files = Directory.GetFiles(dir1, "*", SearchOption.AllDirectories);
				string[] dir2Files = Directory.GetFiles(dir2, "*", SearchOption.AllDirectories);

				if (dir1Files.Length != dir2Files.Length)
					return false;

				Array.Sort(dir1Files);
				Array.Sort(dir2Files);

				for (int i = 0; i < dir1Files.Length; i++)
				{
					string relativePath1 = dir1Files[i].Substring(dir1.Length).TrimStart(Path.DirectorySeparatorChar);
					string relativePath2 = dir2Files[i].Substring(dir2.Length).TrimStart(Path.DirectorySeparatorChar);

					if (!string.Equals(relativePath1, relativePath2, StringComparison.OrdinalIgnoreCase))
						return false;

					if (!FileContentsAreEqual(dir1Files[i], dir2Files[i]))
						return false;
				}

				return true;
			}

			bool FileContentsAreEqual(string filePath1, string filePath2)
			{
				byte[] file1Bytes = File.ReadAllBytes(filePath1);
				byte[] file2Bytes = File.ReadAllBytes(filePath2);

				if (file1Bytes.Length != file2Bytes.Length)
					return false;

				for (int i = 0; i < file1Bytes.Length; i++)
				{
					if (file1Bytes[i] != file2Bytes[i])
						return false;
				}

				return true;
			}
		}

		private void CreateTestFilesAndFolders(string basePath)
		{
			Directory.CreateDirectory(basePath);

			string subDir1 = Path.Combine(basePath, "SubDir1");
			string subDir2 = Path.Combine(basePath, "SubDir2");
			Directory.CreateDirectory(subDir1);
			Directory.CreateDirectory(subDir2);

			File.WriteAllText(Path.Combine(basePath, "file1.txt"), "Content of file1.");
			File.WriteAllText(Path.Combine(basePath, "file2.txt"), "Content of file2.");

			File.WriteAllText(Path.Combine(subDir1, "file3.txt"), "Content of file3 in SubDir1.");
			File.WriteAllText(Path.Combine(subDir1, "file4.txt"), "Content of file4 in SubDir1.");

			File.WriteAllText(Path.Combine(subDir2, "file5.txt"), "Content of file5 in SubDir2.");
			File.WriteAllText(Path.Combine(subDir2, "file6.txt"), "Content of file6 in SubDir2.");
		}

		private void DeleteTestFilesAndFolders(string basePath)
		{
			if (Directory.Exists(basePath))
			{
				Directory.Delete(basePath, true);
			}
		}

		[Fact]
		public void TestTar()
		{
		}

		[Fact]
		public void TestGZip()
		{
		}


		[Fact]
		public void TestJar()
		{
		}

		[Fact]
		public void TestUnsupportedFormat()
		{
		}

	}

}