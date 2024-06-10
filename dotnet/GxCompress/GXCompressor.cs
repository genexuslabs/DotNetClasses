using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security;
using GeneXus;
using ICSharpCode.SharpZipLib.Tar;

namespace Genexus.Compression
{
	public enum CompressionFormat
	{
		GZIP,
		TAR,
		ZIP
	}

	public class GXCompressor : IGXCompressor
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger(typeof(GXCompressor).FullName);

		public static int CompressFiles(List<string> files, string path, string format)
		{
			if (files.Count == 0)
			{
				GXLogging.Error(log, "No files have been added for compression.");
				return -4;
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
						return 0;
					case CompressionFormat.TAR:
						CompressToTar(toCompress, path);
						return 0;
					case CompressionFormat.GZIP:
						CompressToGzip(toCompress, path);
						return 0;
				}
			}
			catch (ArgumentException)
			{
				GXLogging.Error(log, "Unsupported compression format for compression: {0}", format);
				return -3;
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "An error occurred during the compression process: ", e);
				return -1;
			}
			return -1;
		}

		public static int CompressFolder(string folder, string path, string format)
		{
			DirectoryInfo toCompress = new DirectoryInfo(folder);
			if (!toCompress.Exists)
			{
				GXLogging.Error(log, "The specified folder does not exist: {0}", toCompress.FullName);
				return -2;
			}
			List<string> list = new List<string> { folder };
			return CompressFiles(list, path, format);
		}

		public static Compression NewCompression(string path, string format)
		{
			return new Compression(path, format);
		}

		public static int Decompress(string file, string path)
		{
			FileInfo toCompress = new FileInfo(file);
			if (!toCompress.Exists)
			{
				GXLogging.Error(log, "The specified archive does not exist: {0}", toCompress.FullName);
				return -2;
			}
			if (toCompress.Length == 0)
			{
				GXLogging.Error(log, "The archive located at {0} is empty", path);
				return -4;
			}
			string extension = Path.GetExtension(toCompress.Name).Substring(1);
			try
			{
				switch (extension.ToLower())
				{
					case "zip":
						DecompressZip(toCompress, path);
						return 0;
					case "tar":
						DecompressTar(toCompress, path);
						return 0;
					case "gz":
						DecompressGzip(toCompress, path);
						return 0;
					default:
						GXLogging.Error(log, "Unsupported compression format for decompression: {0}", extension);
						return -3;
				}
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Decompression failed: {0}", e.Message);
				return -1;
			}
		}

		private static void CompressToZip(FileInfo[] files, string outputPath)
		{
			try
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
			catch (IOException e)
			{
				GXLogging.Error(log, "Error while compressing to zip", e);
				throw new Exception("Failed to compress files", e);
			}
		}

		private static void AddFileToZip(ZipArchive zos, FileInfo file, string baseDir)
		{
			string entryName = baseDir + file.Name;
			if (file.Attributes.HasFlag(FileAttributes.Directory))
			{
				DirectoryInfo dir = new DirectoryInfo(file.FullName);
				FileInfo[] children = dir.GetFiles();
				DirectoryInfo[] directories = dir.GetDirectories();
				foreach (DirectoryInfo childDir in directories)
				{
					AddFileToZip(zos, new FileInfo(childDir.FullName), entryName + "/");
				}
				foreach (FileInfo childFile in children)
				{
					AddFileToZip(zos, childFile, entryName + "/");
				}
			}
			else
			{
				ZipArchiveEntry zipEntry = zos.CreateEntryFromFile(file.FullName, entryName);
				using (Stream entryStream = zipEntry.Open())
				using (FileStream fis = File.OpenRead(file.FullName))
				{
					fis.CopyTo(entryStream);
				}
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

			try
			{
				using (FileStream inStream = inputFile.OpenRead())
				using (FileStream fout = outputFile.Create())
				using (BufferedStream outStream = new BufferedStream(fout))
				using (GZipStream gzOut = new GZipStream(outStream, CompressionMode.Compress))
				{
					byte[] buffer = new byte[4096];
					int n;
					while ((n = inStream.Read(buffer, 0, buffer.Length)) != -1)
					{
						gzOut.Write(buffer, 0, n);
					}
				}
			}
			catch (IOException e)
			{
				GXLogging.Error(log, "Error while compressing to gzip", e);
				throw new Exception("Error compressing file with GZIP", e);
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

						if (entry.FullName.EndsWith("/"))
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
				while ((bytesRead = gzis.Read(buffer, 0, buffer.Length)) != -1)
				{
					fos.Write(buffer, 0, bytesRead);
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

