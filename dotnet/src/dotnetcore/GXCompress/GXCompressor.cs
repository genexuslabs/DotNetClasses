using GeneXus.Utils;
using GeneXus;
using System.IO.Compression;
using System.Formats.Tar;

namespace Genexus.Compression
{
	public class GXCompressor
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger(typeof(GXCompressor).FullName);

		private const string GENERIC_ERROR = "An error occurred during the compression/decompression process: ";
		private const string NO_FILES_ADDED = "No files have been added for compression.";
		private const string FILE_NOT_EXISTS = "File does not exist: ";
		private const string UNSUPPORTED_FORMAT = " is an unsupported format. Supported formats are zip, tar, gz and jar.";
		private const string EMPTY_FILE = "The selected file is empty: ";
		private const string DIRECTORY_ATTACK = "Potential directory traversal attack detected: ";
		private const string MAX_FILESIZE_EXCEEDED = "The files selected for compression exceed the maximum permitted file size of ";
		private static void StorageMessages(string error, GXBaseCollection<SdtMessages_Message> messages)
		{
			if (messages != null)
			{
				SdtMessages_Message msg = new SdtMessages_Message
				{
					gxTpr_Type = 1,
					gxTpr_Description = error
				};
				messages.Add(msg);
			}
		}

		public static bool Compress(List<string> files, string path, long maxCombinedFileSize, ref GXBaseCollection<SdtMessages_Message> messages)
		{
			if (files.Count == 0)
			{
				GXLogging.Error(log, NO_FILES_ADDED);
				StorageMessages(NO_FILES_ADDED, messages);
				return false;
			}
			long totalSize = 0;
			FileInfo[] toCompress = new FileInfo[files.Count];
			int index = 0;
			foreach (string filePath in files)
			{
				FileInfo file = new FileInfo(filePath);
				try
				{
					string normalizedPath = Path.GetFullPath(file.FullName);
					if (!file.Exists)
					{
						GXLogging.Error(log, FILE_NOT_EXISTS + filePath);
						StorageMessages(FILE_NOT_EXISTS + filePath, messages);
						continue;
					}
					if (normalizedPath.Contains(Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar) ||
						normalizedPath.EndsWith(Path.DirectorySeparatorChar + "..") ||
						normalizedPath.StartsWith(".." + Path.DirectorySeparatorChar))
					{
						GXLogging.Error(log, DIRECTORY_ATTACK + filePath);
						StorageMessages(DIRECTORY_ATTACK + filePath, messages);
						return false;
					}
					long fileSize = file.Length;
					totalSize += fileSize;
					if (maxCombinedFileSize > -1 && totalSize > maxCombinedFileSize)
					{
						GXLogging.Error(log, MAX_FILESIZE_EXCEEDED + maxCombinedFileSize);
						StorageMessages(MAX_FILESIZE_EXCEEDED + maxCombinedFileSize, messages);
						return false;
					}
					toCompress[index++] = file;
				}
				catch (Exception e)
				{
					GXLogging.Error(log, "Error normalizing path for file " + filePath, e);
				}
			}
			string format = (Path.GetExtension(path)?.TrimStart('.').ToLowerInvariant()) ?? "";
			try
			{
				switch (format)
				{
					case "zip":
						CompressToZip(toCompress, path);
						break;
					case "tar":
						CompressToTar(toCompress, path);
						break;
					case "gz":
						CompressToGzip(toCompress, path);
						break;
					case "jar":
						CompressToJar(toCompress, path);
						break;
					default:
						GXLogging.Error(log, format + UNSUPPORTED_FORMAT);
						StorageMessages(format + UNSUPPORTED_FORMAT, messages);
						return false;
				}
				return true;
			}
			catch (Exception e)
			{
				GXLogging.Error(log, GENERIC_ERROR, e);
				StorageMessages(e.Message, messages);
				return false;
			}
		}

		public static Compression NewCompression(string path, long maxCombinedFileSize, ref GXBaseCollection<SdtMessages_Message> messages)
		{
			return new Compression(path, maxCombinedFileSize, messages);
		}

		public static bool Decompress(string file, string path, ref GXBaseCollection<SdtMessages_Message> messages)
		{
			FileInfo toDecompress = new FileInfo(file);
			if (!toDecompress.Exists)
			{
				GXLogging.Error(log, FILE_NOT_EXISTS + toDecompress.FullName);
				StorageMessages(FILE_NOT_EXISTS + toDecompress.FullName, messages);
				return false;
			}
			if (toDecompress.Length == 0L)
			{
				GXLogging.Error(log, EMPTY_FILE + file);
				StorageMessages(EMPTY_FILE + file, messages);
				return false;
			}
			string extension = (Path.GetExtension(toDecompress.Name)?.TrimStart('.').ToLowerInvariant()) ?? "";
			try
			{
				switch (extension)
				{
					case "zip":
						DecompressZip(toDecompress, path);
						break;
					case "tar":
						DecompressTar(toDecompress, path);
						break;
					case "gz":
						DecompressGzip(toDecompress, path);
						break;
					case "jar":
						DecompressJar(toDecompress, path);
						break;
					default:
						GXLogging.Error(log, extension + UNSUPPORTED_FORMAT);
						StorageMessages(extension + UNSUPPORTED_FORMAT, messages);
						return false;
				}
				return true;
			}
			catch (Exception e)
			{
				GXLogging.Error(log, GENERIC_ERROR, e);
				StorageMessages(e.Message, messages);
				return false;
			}
		}
		private static void CompressToZip(FileInfo[] files, string outputPath)
		{
			using (FileStream fs = new FileStream(outputPath, FileMode.Create))
			using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Create))
			{
				foreach (FileInfo file in files)
				{
					ZipFile(file, file.Name, archive);
				}
			}
		}

		private static void ZipFile(FileSystemInfo fileToZip, string entryName, ZipArchive archive)
		{
			if ((fileToZip.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
				return;
			if ((fileToZip.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
			{
				if (!entryName.EndsWith("/"))
					entryName += "/";
				archive.CreateEntry(entryName);
				DirectoryInfo dir = new DirectoryInfo(fileToZip.FullName);
				foreach (FileSystemInfo child in dir.GetFileSystemInfos())
				{
					ZipFile(child, entryName + child.Name, archive);
				}
			}
			else
			{
				ZipArchiveEntry entry = archive.CreateEntry(entryName);
				using (Stream entryStream = entry.Open())
				using (FileStream fileStream = new FileStream(fileToZip.FullName, FileMode.Open, FileAccess.Read))
				{
					fileStream.CopyTo(entryStream);
				}
			}
		}

		private static void CompressToTar(FileInfo[] files, string outputPath)
		{
			if (string.IsNullOrEmpty(outputPath)) throw new ArgumentException("The output path must not be null or empty");
			if (File.Exists(outputPath)) throw new IOException("Output file already exists");
			using
			var output = File.Create(outputPath);
			using
			var tarWriter = new TarWriter(output, TarEntryFormat.Pax);
			void AddFileToTar(FileSystemInfo file, string entryName)
			{
				if (file is DirectoryInfo di)
				{
					foreach (var child in di.GetFileSystemInfos())
					{
						AddFileToTar(child, entryName + "/" + child.Name);
					}
				}
				else if (file is FileInfo fi)
				{
					tarWriter.WriteEntry(entryName, fi.FullName);
				}
			}
			foreach (var file in files)
			{
				if (file == null || !file.Exists) continue;
				AddFileToTar(file, file.Name);
			}
		}


		private static void CompressToGzip(FileInfo[] files, string outputPath)
		{
			if (files == null || files.Length == 0)
				throw new ArgumentException("No files to compress");
			if (string.IsNullOrEmpty(outputPath))
				throw new ArgumentException("Output path is null or empty");
			if (File.Exists(outputPath))
			{
				try { using (File.OpenWrite(outputPath)) { } } catch { throw new IOException("Cannot write to output file"); }
			}
			string parentDir = Path.GetDirectoryName(outputPath) ?? "";
			if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
				Directory.CreateDirectory(parentDir);
			bool singleFile = files.Length == 1 && files[0].Exists && (files[0].Attributes & FileAttributes.Directory) == 0;
			string tempFile = Path.Combine(string.IsNullOrEmpty(parentDir) ? "." : parentDir, Path.GetRandomFileName() + ".tmp");
			if (singleFile)
			{
				using FileStream fis = files[0].OpenRead();
				using FileStream fos = File.Create(tempFile);
				using GZipStream gzos = new GZipStream(fos, CompressionLevel.Optimal);
				fis.CopyTo(gzos);
			}
			else
			{
				string tarTemp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				Directory.CreateDirectory(tarTemp);
				foreach (var f in files)
				{
					if (f == null) continue;
					string destPath = Path.Combine(tarTemp, f.Name);
					if ((f.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
					{
						DirectoryInfo di = new DirectoryInfo(f.FullName);
						CopyDirectory(di, new DirectoryInfo(destPath));
					}
					else
					{
						File.Copy(f.FullName, destPath, true);
					}
				}
				string tarFilePath = Path.GetTempFileName();
				TarFile.CreateFromDirectory(tarTemp, tarFilePath, false);
				Directory.Delete(tarTemp, true);
				using FileStream tfs = File.OpenRead(tarFilePath);
				using FileStream fos = File.Create(tempFile);
				using GZipStream gzs = new GZipStream(fos, CompressionLevel.Optimal);
				tfs.CopyTo(gzs);
				File.Delete(tarFilePath);
			}
			string finalName = outputPath;
			if (singleFile)
			{
				if (!finalName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
					finalName += ".gz";
			}
			else
			{
				if (!finalName.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))
				{
					if (finalName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
						finalName = finalName.Substring(0, finalName.Length - 3) + ".tar.gz";
					else
						finalName += ".tar.gz";
				}
			}
			if (File.Exists(finalName))
			{
				try { File.Delete(finalName); } catch { throw new IOException("Failed to delete existing file with desired name"); }
			}
			try
			{
				File.Move(tempFile, finalName);
			}
			catch (Exception ex)
			{
				throw new IOException("Failed to rename archive to desired name", ex);
			}
			if (!File.Exists(finalName))
				throw new IOException("Failed to create the archive");
		}

		private static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
		{
			Directory.CreateDirectory(target.FullName);
			foreach (FileInfo fi in source.GetFiles())
			{
				fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
			}
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{
				DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
				CopyDirectory(diSourceSubDir, nextTargetSubDir);
			}
		}

		private static void CompressToJar(FileInfo[] files, string outputPath)
		{
			if (string.IsNullOrEmpty(outputPath))
				throw new ArgumentException("Output path is null or empty");
			if (File.Exists(outputPath))
				throw new IOException("Output file already exists");
			using (var fs = new FileStream(outputPath, FileMode.CreateNew))
			using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
			{
				foreach (var file in files)
				{
					if (file == null || !file.Exists)
						continue;
					string basePath = (((file.Attributes & FileAttributes.Directory) == FileAttributes.Directory) ? file.FullName : file.DirectoryName) ?? "";
					if (!string.IsNullOrEmpty(basePath) && !basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
						basePath += Path.DirectorySeparatorChar;
					var stack = new Stack<FileSystemInfo>();
					stack.Push(file);
					while (stack.Count > 0)
					{
						var current = stack.Pop();
						string canonical = current.FullName ?? "";
						string entryName = canonical.Length > basePath.Length ? canonical.Substring(basePath.Length) : "";
						entryName = entryName.Replace(Path.DirectorySeparatorChar, '/');
						if ((current.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
						{
							var di = (DirectoryInfo)current;
							foreach (var child in di.GetFileSystemInfos())
								stack.Push(child);
							if (!string.IsNullOrEmpty(entryName))
							{
								if (!entryName.EndsWith("/"))
									entryName += "/";
								archive.CreateEntry(entryName);
							}
						}
						else
						{
							var entry = archive.CreateEntry(entryName);
							using (var entryStream = entry.Open())
							using (var fileStream = new FileStream(current.FullName!, FileMode.Open, FileAccess.Read))
								fileStream.CopyTo(entryStream);
						}
					}
				}
			}
		}

		private static void DecompressZip(FileInfo file, string outputPath)
		{
			if (file == null || !file.Exists)
				throw new ArgumentException("File not found", nameof(file));
			if (string.IsNullOrEmpty(outputPath))
				throw new ArgumentException("Output path is null or empty", nameof(outputPath));
			Directory.CreateDirectory(outputPath);
			using (var archiveStream = file.OpenRead())
			using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read))
			{
				foreach (var entry in archive.Entries)
				{
					string fullPath = Path.Combine(outputPath, entry.FullName);
					if (string.IsNullOrEmpty(entry.Name))
					{
						Directory.CreateDirectory(fullPath);
					}
					else
					{
						Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
						entry.ExtractToFile(fullPath, true);
					}
				}
			}
		}

		private static void DecompressTar(FileInfo file, string outputPath)
		{
			if (file == null || !file.Exists)
				throw new ArgumentException("Archive file not found", nameof(file));
			if (string.IsNullOrEmpty(outputPath))
				throw new ArgumentException("Output path is null or empty", nameof(outputPath));
			Directory.CreateDirectory(outputPath);
			using FileStream fs = file.OpenRead();
			TarFile.ExtractToDirectory(fs, outputPath, overwriteFiles: true);
		}

		private static void DecompressGzip(FileInfo file, string outputPath)
		{
			if (file == null || !file.Exists)
				throw new ArgumentException("The archive file does not exist or is not a file.", nameof(file));
			if (string.IsNullOrEmpty(outputPath) || !Directory.Exists(outputPath))
				throw new ArgumentException("The specified directory does not exist or is not a directory.", nameof(outputPath));

			string tempFile = Path.Combine(Path.GetTempPath(), "decompressed_" + Guid.NewGuid() + ".tmp");
			using (FileStream fs = file.OpenRead())
			using (GZipStream gz = new GZipStream(fs, CompressionMode.Decompress))
			using (FileStream fos = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
			{
				gz.CopyTo(fos);
			}

			bool isTar = false;
			try
			{
				using FileStream tfs = new FileStream(tempFile, FileMode.Open, FileAccess.Read);
				using TarReader tarReader = new TarReader(tfs);
				if (tarReader.GetNextEntry() != null)
					isTar = true;
			}
			catch { }

			if (isTar)
			{
				using FileStream tfs = new FileStream(tempFile, FileMode.Open, FileAccess.Read);
				TarFile.ExtractToDirectory(tfs, outputPath, overwriteFiles: true);
			}
			else
			{
				string name = file.Name;
				if (name.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
					name = name.Substring(0, name.Length - 3);
				string singleOutFile = Path.Combine(outputPath, name);
				try
				{
					File.Move(tempFile, singleOutFile);
				}
				catch
				{
					using FileStream inStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read);
					using FileStream outStream = new FileStream(singleOutFile, FileMode.Create, FileAccess.Write);
					inStream.CopyTo(outStream);
					File.Delete(tempFile);
				}
			}

			if (File.Exists(tempFile))
				File.Delete(tempFile);
		}


		private static void DecompressJar(FileInfo file, string outputPath)
		{
			if (file == null || !file.Exists)
				throw new IOException("Invalid archive file.");
			if (string.IsNullOrEmpty(outputPath))
				throw new ArgumentException("Output path is null or empty", nameof(outputPath));
			Directory.CreateDirectory(outputPath);
			using (var zipStream = file.OpenRead())
			using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
			{
				foreach (var entry in archive.Entries)
				{
					string destinationPath = Path.Combine(outputPath, entry.FullName);
					if (string.IsNullOrEmpty(entry.Name))
					{
						Directory.CreateDirectory(destinationPath);
					}
					else
					{
						string? destinationDir = Path.GetDirectoryName(destinationPath);
						if (!string.IsNullOrEmpty(destinationDir))
							Directory.CreateDirectory(destinationDir);
						entry.ExtractToFile(destinationPath, true);
					}
				}
			}
		}

	}
}
