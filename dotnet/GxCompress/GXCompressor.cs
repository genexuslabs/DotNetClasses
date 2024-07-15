using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security;
using GeneXus;
using ICSharpCode.SharpZipLib.Tar;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;

namespace Genexus.Compression
{
	public enum CompressionFormat
	{
		GZIP,
		TAR,
		ZIP,
		JAR
	}

	public class GXCompressor : IGXCompressor
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger(typeof(GXCompressor).FullName);

		public static CompressionMessage CompressFiles(List<string> files, string path, string format)
		{
			if (files.Count == 0)
			{
				GXLogging.Error(log, "No files have been added for compression.");
				return new CompressionMessage(false, "No files have been added for compression.");
			}

			FileInfo[] toCompress = new FileInfo[files.Count];
			int index = 0;
			foreach (string filePath in files)
			{
				toCompress[index++] = new FileInfo(filePath);
			}

			try
			{
				CompressionFormat compressionFormat = GetCompressionFormat(format);
				switch (compressionFormat)
				{
					case CompressionFormat.ZIP:
						CompressToZip(toCompress, path);
						return GetSuccessfulCompressionMessage();
					case CompressionFormat.TAR:
						CompressToTar(toCompress, path);
						return GetSuccessfulCompressionMessage();
					case CompressionFormat.GZIP:
						CompressToGzip(toCompress, path);
						return GetSuccessfulCompressionMessage();
					case CompressionFormat.JAR:
						CompressToJar(toCompress, path);
						return GetSuccessfulCompressionMessage();
				}
			}
			catch (ArgumentException ae)
			{
				GXLogging.Error(log, "Unsupported compression format for compression: " + format, ae);
				return new CompressionMessage(false, "Unsupported compression format for compression: " + format);
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "An error occurred during the compression process: ", e);
				return new CompressionMessage(false, "An error occurred during the compression process: " + e.Message);
			}
			return new CompressionMessage(false, "An error occurred during the compression process");
		}

		public static CompressionMessage CompressFolder(string folder, string path, string format)
		{
			DirectoryInfo toCompress = new DirectoryInfo(folder);
			if (!toCompress.Exists)
			{
				GXLogging.Error(log, "The specified folder does not exist: {0}", toCompress.FullName);
				return new CompressionMessage(false, "The specified folder does not exist: " + toCompress.FullName);
			}
			List<string> list = new List<string> { folder };
			return CompressFiles(list, path, format);
		}

		public static Compression NewCompression(string path, string format)
		{
			return new Compression(path, format);
		}

		public static CompressionMessage Decompress(string file, string path)
		{
			FileInfo toCompress = new FileInfo(file);
			if (!toCompress.Exists)
			{
				GXLogging.Error(log, "The specified archive does not exist: " + toCompress.FullName);
				return new CompressionMessage(false, "The specified archive does not exist: " + toCompress.FullName);
			}
			if (toCompress.Length == 0)
			{
				GXLogging.Error(log, "The archive located at " + toCompress.FullName +  " is empty");
				return new CompressionMessage(false, "The archive located at " + toCompress.FullName +  " is empty");
			}
			string extension = Path.GetExtension(toCompress.Name).Substring(1);
			try
			{
				switch (extension.ToLower())
				{
					case "zip":
						DecompressZip(toCompress, path);
						return GetSuccessfulCompressionMessage();
					case "tar":
						DecompressTar(toCompress, path);
						return GetSuccessfulCompressionMessage();
					case "gz":
						DecompressGzip(toCompress, path);
						return GetSuccessfulCompressionMessage();
					case "jar":
						DecompressJar(toCompress, path);
						return GetSuccessfulCompressionMessage();
					case "rar":
						DecompressRar(toCompress, path);
						return GetSuccessfulCompressionMessage();
					case "7z":
						Decompress7z(toCompress, path);
						return GetSuccessfulCompressionMessage();
					default:
						GXLogging.Error(log, "Unsupported compression format for decompression: " + extension);
						return new CompressionMessage(false, "Unsupported compression format for decompression: " + extension);
				}
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Decompression failed: ", e);
				return new CompressionMessage(false, "An error occurred during the decompression process: " + e.Message);
			}
		}

		private static CompressionMessage GetSuccessfulCompressionMessage()
		{
			return new CompressionMessage(true, "The operation was successful");
		}

		private static void CompressToZip(FileInfo[] files, string outputPath)
		{
			using (FileStream fos = new FileStream(outputPath, FileMode.Create))
			using (ZipArchive zos = new ZipArchive(fos, ZipArchiveMode.Create))
			{
				foreach (FileInfo file in files)
				{
					if (file.Exists)
					{
						AddFileToZip(zos, file, string.Empty);
					}
				}
			}
		}

		private static void AddFileToZip(ZipArchive zos, FileInfo file, string baseDir)
		{
			string entryName = baseDir + file.Name;
			if ((file.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
			{
				DirectoryInfo dirInfo = file.Directory;
				foreach (DirectoryInfo dir in dirInfo.GetDirectories())
				{
					AddFileToZip(zos, new FileInfo(dir.FullName), entryName + Path.DirectorySeparatorChar);
				}
				foreach (FileInfo childFile in dirInfo.GetFiles())
				{
					AddFileToZip(zos, childFile, entryName + Path.DirectorySeparatorChar);
				}
			}
			else
			{
				ZipArchiveEntry zipEntry = zos.CreateEntryFromFile(file.FullName, entryName);
			}
		}

		private static void CompressToTar(FileInfo[] files, string outputPath)
		{
			FileInfo outputTarFile = new FileInfo(outputPath);
			try
			{
				using (FileStream fos = outputTarFile.Create())
				using (TarOutputStream taos = new TarOutputStream(fos))
				{
					taos.IsStreamOwner = false;

					foreach (FileInfo file in files)
					{
						AddFileToTar(taos, file, string.Empty);
					}
					taos.Close();
				}
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Error while compressing to tar", e);
				throw new Exception("Error creating TAR archive", e);
			}
		}

		private static void AddFileToTar(TarOutputStream taos, FileInfo file, string baseDir)
		{
			if ((file.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
			{
				DirectoryInfo dir = new DirectoryInfo(file.FullName);
				FileInfo[] children = dir.GetFiles();
				DirectoryInfo[] directories = dir.GetDirectories();
				foreach (DirectoryInfo childDir in directories)
				{
					AddFileToTar(taos, new FileInfo(childDir.FullName), Path.Combine(baseDir, file.Name, Path.DirectorySeparatorChar.ToString()));
				}
				foreach (FileInfo childFile in children)
				{
					AddFileToTar(taos, childFile, Path.Combine(baseDir, file.Name, Path.DirectorySeparatorChar.ToString()));
				}
			}
			else
			{
				string entryName = baseDir + file.Name;
				TarEntry entry = TarEntry.CreateEntryFromFile(file.FullName);
				entry.Name = entryName;
				entry.Size = file.Length;
				taos.PutNextEntry(entry);

				using (FileStream fis = File.OpenRead(file.FullName))
				{
					byte[] buffer = new byte[4096];
					int bytesRead;
					while ((bytesRead = fis.Read(buffer, 0, buffer.Length)) > 0)
					{
						taos.Write(buffer, 0, bytesRead);
					}
				}
				taos.CloseEntry();
			}
		}

		private static void CompressToGzip(FileInfo[] files, string outputPath)
		{
			if (files.Length > 1)
			{
				throw new ArgumentException("GZIP does not support multiple files. Consider archiving the files first.");
			}

			FileInfo inputFile = files[0];
			FileInfo outputFile = new FileInfo(outputPath);

			using (FileStream inStream = inputFile.OpenRead())
			using (FileStream fout = outputFile.Create())
			using (GZipStream gzOut = new GZipStream(fout, CompressionMode.Compress))
			{
				inStream.CopyTo(gzOut);
			}
		}

		private static void DecompressZip(FileInfo zipFile, string directory)
		{
			string zipFilePath = zipFile.FullName;
			string targetDir = Path.GetFullPath(directory);

			try
			{
				using (FileStream fis = File.OpenRead(zipFilePath))
				using (ZipArchive zipIn = new ZipArchive(fis, ZipArchiveMode.Read))
				{
					foreach (ZipArchiveEntry entry in zipIn.Entries)
					{
						string resolvedPath = Path.Combine(targetDir, entry.FullName);

						if (!resolvedPath.StartsWith(targetDir))
						{
							throw new SecurityException("Zip entry is outside of the target dir: " + entry.FullName);
						}

						string fullResolvedPath = Path.GetFullPath(resolvedPath);

						if (entry.FullName.EndsWith(Path.DirectorySeparatorChar.ToString()))
						{
							if (!Directory.Exists(fullResolvedPath))
							{
								Directory.CreateDirectory(fullResolvedPath);
							}
						}
						else
						{
							string parentDir = Path.GetDirectoryName(fullResolvedPath);
							if (!Directory.Exists(parentDir))
							{
								Directory.CreateDirectory(parentDir);
							}
							using (FileStream outFile = new FileStream(fullResolvedPath, FileMode.Create, FileAccess.Write))
							{
								entry.Open().CopyTo(outFile);
							}
						}
					}
				}
			}
			catch (IOException e)
			{
				GXLogging.Error(log, "Error while decompressing to zip", e);
				throw new Exception("Failed to decompress ZIP file: " + zipFilePath, e);
			}

		}

		private static void DecompressTar(FileInfo file, string outputPath)
		{
			string targetDir = Path.GetFullPath(outputPath);
			try
			{
				using (FileStream fi = File.OpenRead(file.FullName))
				using (TarInputStream ti = new TarInputStream(fi))
				{
					TarEntry entry;
					while ((entry = ti.GetNextEntry()) != null)
					{
						string entryPath = Path.Combine(targetDir, entry.Name);
						FileInfo outputFile = new FileInfo(Path.GetFullPath(entryPath));

						if (!outputFile.FullName.StartsWith(targetDir))
						{
							throw new IOException("Entry is outside of the target directory: " + entry.Name);
						}

						if (entry.IsDirectory)
						{
							if (!outputFile.Exists)
							{
								Directory.CreateDirectory(outputFile.FullName);
							}
						}
						else
						{
							DirectoryInfo parent = outputFile.Directory;
							if (!parent.Exists)
							{
								Directory.CreateDirectory(parent.FullName);
							}

							using (FileStream outStream = File.Create(outputFile.FullName))
							{
								ti.CopyEntryContents(outStream);
							}
						}
					}
				}
			}
			catch (IOException e)
			{
				GXLogging.Error(log, "Error while decompressing to zip", e);
				throw new Exception("Failed to decompress TAR file: " + file.Name, e);
			}
		}

		private static void DecompressGzip(FileInfo inputFile, string outputPath)
		{
			DirectoryInfo outputDir = new DirectoryInfo(outputPath);
			if (!outputDir.Exists)
			{
				outputDir.Create();
			}

			string outputFileName = inputFile.Name;
			if (outputFileName.EndsWith(".gz"))
				outputFileName = outputFileName.Substring(0, outputFileName.Length - 3);
			else
				throw new ArgumentException("The input file does not have a .gz extension.");

			FileInfo outputFile = new FileInfo(Path.Combine(outputDir.FullName, outputFileName));
			using (FileStream fis = inputFile.OpenRead())
			using (GZipStream gzis = new GZipStream(fis, CompressionMode.Decompress))
			using (FileStream fos = outputFile.Create())
			{
				byte[] buffer = new byte[4096];
				int bytesRead;
				while ((bytesRead = gzis.Read(buffer, 0, buffer.Length)) > 0)
				{
					fos.Write(buffer, 0, bytesRead);
				}
			}
		}

		private static void Decompress7z(FileInfo file, string outputPath)
		{
			string targetDir = Path.GetFullPath(outputPath);
			using (var sevenZFile = SevenZipArchive.Open(file.FullName))
			{
				foreach (var entry in sevenZFile.Entries)
				{
					if (!entry.IsDirectory)
					{
						string resolvedPath = Path.Combine(targetDir, entry.Key);
						FileInfo outputFile = new FileInfo(resolvedPath);

						if (!outputFile.FullName.StartsWith(targetDir, StringComparison.OrdinalIgnoreCase))
						{
							throw new IOException("Entry is outside of the target dir: " + entry.Key);
						}

						Directory.CreateDirectory(outputFile.DirectoryName);

						using (var outStream = outputFile.Open(FileMode.Create, FileAccess.Write))
						{
							entry.WriteTo(outStream);
						}
					}
					else
					{
						string dirPath = Path.Combine(targetDir, entry.Key);
						if (!Directory.Exists(dirPath))
						{
							Directory.CreateDirectory(dirPath);
						}
					}
				}
			}
		}

		private static void CompressToJar(FileInfo[] files, string outputPath)
		{
			using (FileStream outputStream = new FileStream(outputPath, FileMode.Create))
			using (ZipArchive jos = new ZipArchive(outputStream, ZipArchiveMode.Create))
			{
				byte[] buffer = new byte[1024];
				foreach (FileInfo file in files)
				{
					using (FileStream fis = file.OpenRead())
					{
						ZipArchiveEntry entry = jos.CreateEntry(file.Name);
						using (Stream entryStream = entry.Open())
						{
							int length;
							while ((length = fis.Read(buffer, 0, buffer.Length)) > 0)
							{
								entryStream.Write(buffer, 0, length);
							}
						}
					}
				}
			}
		}

		public static void DecompressJar(FileInfo jarFile, string outputPath)
		{
			if (!jarFile.Exists)
			{
				throw new IOException("The jar file does not exist.");
			}

			DirectoryInfo outputDir = new DirectoryInfo(outputPath);
			if (!outputDir.Exists)
			{
				outputDir.Create();
			}

			using (FileStream jarFileStream = jarFile.OpenRead())
			using (ZipArchive jis = new ZipArchive(jarFileStream, ZipArchiveMode.Read))
			{
				foreach (ZipArchiveEntry entry in jis.Entries)
				{
					string entryPath = Path.Combine(outputPath, entry.FullName);
					FileInfo outputFile = new FileInfo(entryPath);

					if (entry.FullName.EndsWith(Path.DirectorySeparatorChar.ToString()))
					{
						if (!Directory.Exists(outputFile.FullName))
						{
							Directory.CreateDirectory(outputFile.FullName);
						}
					}
					else
					{
						Directory.CreateDirectory(outputFile.DirectoryName);
						using (FileStream fos = new FileStream(outputFile.FullName, FileMode.Create))
						{
							using (Stream entryStream = entry.Open())
							{
								entryStream.CopyTo(fos);
							}
						}
					}
				}
			}
		}
		public static void DecompressRar(FileInfo rarFile, string destinationPath)
		{
			using (var archive = RarArchive.Open(rarFile.FullName))
			{
				foreach (var entry in archive.Entries)
				{
					if (!entry.IsDirectory)
					{
						entry.WriteToDirectory(destinationPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
					}
				}
			}
		}

		private static CompressionFormat GetCompressionFormat(string format)
		{
			try
			{
				return (CompressionFormat)Enum.Parse(typeof(CompressionFormat), format.ToUpper());
			}
			catch (ArgumentException)
			{
				throw new ArgumentException("Invalid compression format: " + format);
			}
		}

	}

}

