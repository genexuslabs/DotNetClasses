using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security;
using GeneXus;
using GeneXus.Utils;

#if NETCORE
using System.Formats.Tar;
#endif

namespace Genexus.Compression
{
	public class GXCompressor
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger(typeof(GXCompressor).FullName);

		private const string GENERIC_ERROR = "An error occurred during the compression/decompression process: ";
		private const string NO_FILES_ADDED = "No files have been added for compression.";
		private const string FILE_NOT_EXISTS = "File does not exist: ";
		private const string UNSUPPORTED_FORMAT = " is an unsupported format. Supported formats are zip, 7z, tar, gz and jar.";
		private const string EMPTY_FILE = "The selected file is empty: ";
		private const string PURGED_ARCHIVE = "After performing security checks, no valid files where left to compress";
		private const string DIRECTORY_ATTACK = "Potential directory traversal attack detected: ";
		private const string MAX_FILESIZE_EXCEEDED = "The file(s) selected for (de)compression exceed the maximum permitted file size of ";
		private const string TOO_MANY_FILES = "Too many files have been added for (de)compression. Maximum allowed is ";
		private const string BIG_SINGLE_FILE = "Individual file exceeds maximum allowed size: ";
		private const string PROCESSING_ERROR = "Error checking archive safety for file: ";
		private const string ZIP_SLIP_DETECTED = "Zip slip or path traversal attack detected in archive: ";
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

	/**
     * Compresses specified files into an archive at the given path based on configuration parameters.
     *
     * @param files List of file paths to compress
     * @param path Target path for the compressed archive
     * @param configuration Configuration parameters for compression
     * @param messages Collection to store output messages
     * @return Boolean indicating success or failure of compression operation
     */
		[SecurityCritical]
		public static bool Compress(List<string> files, string path, CompressionConfiguration configuration, ref GXBaseCollection<SdtMessages_Message> messages)
		{
			if (files.Count == 0)
			{
				GXLogging.Error(log, NO_FILES_ADDED);
				StorageMessages(NO_FILES_ADDED, messages);
				return false;
			}
			List<FileInfo> validFiles = new List<FileInfo>();
			long totalSize = 0;
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
					string parentPath = file.DirectoryName != null ? Path.GetFullPath(file.DirectoryName) : normalizedPath;
					if (!normalizedPath.StartsWith(parentPath, StringComparison.Ordinal))
					{
						GXLogging.Error(log, DIRECTORY_ATTACK + filePath);
						StorageMessages(DIRECTORY_ATTACK + filePath, messages);
						return false;
					}
					long fileSize = file.Length;
					if (configuration.maxIndividualFileSize > -1 && fileSize > configuration.maxIndividualFileSize)
					{
						GXLogging.Error(log, BIG_SINGLE_FILE + filePath);
						StorageMessages(BIG_SINGLE_FILE + filePath, messages);
						continue;
					}
					totalSize += fileSize;
					validFiles.Add(file);
				}
				catch (Exception e)
				{
					GXLogging.Error(log, "Error normalizing path for file: " + filePath, e);
					StorageMessages("Error normalizing path for file: " + filePath, messages);
					return false;
				}
			}
			if (validFiles.Count == 0)
			{
				GXLogging.Error(log, PURGED_ARCHIVE);
				StorageMessages(PURGED_ARCHIVE, messages);
				return false;
			}
			if (configuration.maxCombinedFileSize > -1 && totalSize > configuration.maxCombinedFileSize)
			{
				GXLogging.Error(log, MAX_FILESIZE_EXCEEDED + configuration.maxCombinedFileSize);
				StorageMessages(MAX_FILESIZE_EXCEEDED + configuration.maxCombinedFileSize, messages);
				return false;
			}
			if (configuration.maxFileCount > -1 && validFiles.Count > configuration.maxFileCount)
			{
				GXLogging.Error(log, TOO_MANY_FILES + configuration.maxFileCount);
				StorageMessages(TOO_MANY_FILES + configuration.maxFileCount, messages);
				return false;
			}
			try
			{
				FileInfo targetFile = new FileInfo(path);
				DirectoryInfo targetDir = targetFile.Directory ?? new DirectoryInfo(Path.GetDirectoryName(targetFile.FullName) ?? "");
				if (path.Contains("/../") || path.Contains("../") || path.Contains("/.."))
				{
					GXLogging.Error(log, DIRECTORY_ATTACK + path);
					StorageMessages(DIRECTORY_ATTACK + path, messages);
					return false;
				}
				if (!string.IsNullOrEmpty(configuration.targetDirectory))
				{
					DirectoryInfo configTargetDir = new DirectoryInfo(configuration.targetDirectory);
					string normalizedTargetPath = Path.GetFullPath(targetDir.FullName);
					string normalizedConfigPath = Path.GetFullPath(configTargetDir.FullName);
					if (!normalizedTargetPath.StartsWith(normalizedConfigPath, StringComparison.Ordinal))
					{
						GXLogging.Error(log, DIRECTORY_ATTACK + path);
						StorageMessages(DIRECTORY_ATTACK + path, messages);
						return false;
					}
				}
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Error validating target path: " + path, e);
				StorageMessages("Error validating target path: " + path, messages);
				return false;
			}
			FileInfo[] toCompress = validFiles.ToArray();
			string format = (Path.GetExtension(path)?.TrimStart('.').ToLowerInvariant()) ?? "";
			try
			{
				switch (format)
				{
					case "zip":
						CompressToZip(toCompress, path);
						break;
#if NETCORE
					case "tar":
						CompressToTar(toCompress, path);
						break;
					case "gz":
						CompressToGzip(toCompress, path);
						break;
#endif
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

		/**
		 * Compresses files interactively, add files to a collection until the GXCompressor.compress method is executed
		 *
		 * @param path Target path for the compressed archive
		 * @param configuration Configuration parameters for decompression
		 * @param messages Collection to store output messages
		 * @return Compression object
		*/
		public static Compression NewCompression(string path, CompressionConfiguration configuration, ref GXBaseCollection<SdtMessages_Message> messages)
		{
			return new Compression(path, configuration, messages);
		}

		/**
		 * Decompresses an archive file to the specified path based on configuration parameters.
		 *
		 * @param file Path to the archive file to decompress
		 * @param path Target path for the decompressed files
		 * @param configuration Configuration parameters for decompression
		 * @param messages Collection to store output messages
		 * @return Boolean indicating success or failure of decompression operation
		 */
		[SecurityCritical]
		public static bool Decompress(string file, string path, CompressionConfiguration configuration, ref GXBaseCollection<SdtMessages_Message> messages)
		{
			FileInfo archiveFile = new FileInfo(file);
			if (!archiveFile.Exists)
			{
				GXLogging.Error(log, FILE_NOT_EXISTS + archiveFile.FullName);
				StorageMessages(FILE_NOT_EXISTS + archiveFile.FullName, messages);
				return false;
			}
			if (archiveFile.Length == 0L)
			{
				GXLogging.Error(log, EMPTY_FILE + file);
				StorageMessages(EMPTY_FILE + file, messages);
				return false;
			}
			int fileCount;
			try
			{
				fileCount = CompressionUtils.CountArchiveEntries(archiveFile);
				if (fileCount <= 0)
				{
					GXLogging.Error(log, EMPTY_FILE + file);
					StorageMessages(EMPTY_FILE + file, messages);
					return false;
				}
			}
			catch (Exception e)
			{
				GXLogging.Error(log, PROCESSING_ERROR + file, e);
				StorageMessages(PROCESSING_ERROR + file, messages);
				return false;
			}
			try
			{
				DirectoryInfo targetDir = new DirectoryInfo(path);
				if (path.Contains("/../") || path.Contains("../") || path.Contains("/.."))
				{
					GXLogging.Error(log, DIRECTORY_ATTACK + path);
					StorageMessages(DIRECTORY_ATTACK + path, messages);
					return false;
				}
				if (!string.IsNullOrEmpty(configuration.targetDirectory))
				{
					DirectoryInfo configTargetDir = new DirectoryInfo(configuration.targetDirectory);
					string normalizedTargetPath = Path.GetFullPath(targetDir.FullName);
					string normalizedConfigPath = Path.GetFullPath(configTargetDir.FullName);
					if (!normalizedTargetPath.StartsWith(normalizedConfigPath, StringComparison.Ordinal))
					{
						GXLogging.Error(log, DIRECTORY_ATTACK + path);
						StorageMessages(DIRECTORY_ATTACK + path, messages);
						return false;
					}
				}
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Error validating target path: " + path, e);
				StorageMessages("Error validating target path: " + path, messages);
				return false;
			}
			try
			{
				if (!CompressionUtils.IsArchiveSafe(archiveFile, path))
				{
					GXLogging.Error(log, ZIP_SLIP_DETECTED + file);
					StorageMessages(ZIP_SLIP_DETECTED + file, messages);
					return false;
				}
			}
			catch (Exception e)
			{
				GXLogging.Error(log, PROCESSING_ERROR + file, e);
				StorageMessages(PROCESSING_ERROR + file, messages);
				return false;
			}
			try
			{
				if (configuration.maxIndividualFileSize > -1)
				{
					long maxFileSize = CompressionUtils.GetMaxFileSize(archiveFile);
					if (maxFileSize > configuration.maxIndividualFileSize)
					{
						GXLogging.Error(log, BIG_SINGLE_FILE + maxFileSize + " bytes");
						StorageMessages(BIG_SINGLE_FILE + maxFileSize + " bytes", messages);
						return false;
					}
				}
				if (configuration.maxCombinedFileSize > -1)
				{
					long totalSizeEstimate = CompressionUtils.EstimateDecompressedSize(archiveFile);
					if (totalSizeEstimate > configuration.maxCombinedFileSize)
					{
						GXLogging.Error(log, MAX_FILESIZE_EXCEEDED + configuration.maxCombinedFileSize);
						StorageMessages(MAX_FILESIZE_EXCEEDED + configuration.maxCombinedFileSize, messages);
						return false;
					}
				}
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Error estimating archive size: " + file, e);
				StorageMessages("Error estimating archive size: " + file, messages);
				return false;
			}
			if (configuration.maxFileCount > -1 && fileCount > configuration.maxFileCount)
			{
				GXLogging.Error(log, TOO_MANY_FILES + configuration.maxFileCount);
				StorageMessages(TOO_MANY_FILES + configuration.maxFileCount, messages);
				return false;
			}
			string ext = (Path.GetExtension(archiveFile.Name)?.TrimStart('.').ToLowerInvariant()) ?? "";
			try
			{
				switch (ext)
				{
					case "zip":
						DecompressZip(archiveFile, path);
						break;
#if NETCORE
					case "tar":
						DecompressTar(archiveFile, path);
						break;
					case "gz":
						DecompressGzip(archiveFile, path);
						break;
#endif
					case "jar":
						DecompressJar(archiveFile, path);
						break;
					default:
						GXLogging.Error(log, ext + UNSUPPORTED_FORMAT);
						StorageMessages(ext + UNSUPPORTED_FORMAT, messages);
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



#if NETCORE
		private static void CompressToTar(FileSystemInfo[] inputs, string outputPath)
		{
			if (string.IsNullOrEmpty(outputPath)) throw new ArgumentException("The output path must not be null or empty");
			if (File.Exists(outputPath)) throw new IOException("Output file already exists");

			using (var output = File.Create(outputPath))
			using (var tarWriter = new TarWriter(output, TarEntryFormat.Pax))
			{
				void AddFileToTar(FileSystemInfo fileOrDir, string archivePathPrefix)
				{
					if (fileOrDir is DirectoryInfo dir)
					{
						foreach (var child in dir.GetFileSystemInfos())
						{
							string archivePath = Path.Combine(archivePathPrefix, child.Name).Replace('\\', '/');
							AddFileToTar(child, archivePath);
						}
					}
					else if (fileOrDir is FileInfo file)
					{
						tarWriter.WriteEntry(file.FullName, archivePathPrefix);
					}
				}

				foreach (var item in inputs)
				{
					if (item == null || !item.Exists) continue;

					if (item is DirectoryInfo dir)
					{
						foreach (var child in dir.GetFileSystemInfos())
						{
							string archivePath = Path.Combine(dir.Name, child.Name).Replace('\\', '/');
							AddFileToTar(child, archivePath);
						}
					}
					else if (item is FileInfo file)
					{
						tarWriter.WriteEntry(file.FullName, file.Name);
					}
				}
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

			string tempFile;
			do
			{
				tempFile = Path.Combine(string.IsNullOrEmpty(parentDir) ? "." : parentDir, Path.GetRandomFileName() + ".tmp");
			} while (File.Exists(tempFile));

			try
			{
				if (singleFile)
				{
					using (FileStream fis = files[0].OpenRead())
					using (FileStream fos = File.Create(tempFile))
					using (GZipStream gzos = new GZipStream(fos, CompressionLevel.Optimal))
					{
						fis.CopyTo(gzos);
					}
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
					string tarFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
					TarFile.CreateFromDirectory(tarTemp, tarFilePath, false);
					Directory.Delete(tarTemp, true);
					using (FileStream tfs = File.OpenRead(tarFilePath))
					using (FileStream fos = File.Create(tempFile))
					using (GZipStream gzs = new GZipStream(fos, CompressionLevel.Optimal))
					{
						tfs.CopyTo(gzs);
					}
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
				File.Move(tempFile, finalName);
				if (!File.Exists(finalName))
					throw new IOException("Failed to create the archive");
			}
			finally
			{
				if (File.Exists(tempFile))
					File.Delete(tempFile);
			}
		}

#endif

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
							{
								string filePath = current.FullName ?? throw new ArgumentNullException(nameof(current.FullName));
								using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
									fileStream.CopyTo(entryStream);
							}
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
					string destFileName = Path.GetFullPath(fullPath);
					string fullDestDirPath = Path.GetFullPath(outputPath + Path.DirectorySeparatorChar);
					if (!destFileName.StartsWith(fullDestDirPath))
					{
						throw new InvalidOperationException("Entry is outside the target dir: " + destFileName);
					}


					if (string.IsNullOrEmpty(entry.Name))
					{
						Directory.CreateDirectory(destFileName);
					}
					else
					{
#if NETCORE
						string directoryPath = Path.GetDirectoryName(destFileName)!;
#else
						string directoryPath = Path.GetDirectoryName(destFileName);
						if (directoryPath != null)
						{
							Directory.CreateDirectory(directoryPath);
						}
#endif

						entry.ExtractToFile(destFileName, true);
					}
				}
			}
		}
#if NETCORE

		private static void DecompressTar(FileInfo file, string outputPath)
		{
			if (file == null || !file.Exists)
				throw new ArgumentException("Archive file not found");
			if (string.IsNullOrEmpty(outputPath))
				throw new ArgumentException("Output path is null or empty", outputPath);
			Directory.CreateDirectory(outputPath);
			using (FileStream fs = file.OpenRead())
			{
				TarFile.ExtractToDirectory(fs, outputPath, overwriteFiles: true);
			}
		}

		private static void DecompressGzip(FileInfo file, string outputPath)
		{
			if (file == null || !file.Exists)
				throw new ArgumentException("The archive file does not exist or is not a file.");

			string fullOutputPath = outputPath;

			if (string.IsNullOrEmpty(outputPath))
				throw new ArgumentException("The specified output path is null or empty.", nameof(outputPath));

			bool isDirectoryPath = !Path.HasExtension(outputPath);

			if (isDirectoryPath)
			{
				if (!Path.IsPathRooted(outputPath))
				{
					string baseDir = AppDomain.CurrentDomain.BaseDirectory;
					fullOutputPath = Path.Combine(baseDir, outputPath);
				}

				if (!Directory.Exists(fullOutputPath))
				{
					Directory.CreateDirectory(fullOutputPath);
				}
			}
			else
			{
				string dir = Path.GetDirectoryName(fullOutputPath) ?? string.Empty;
				if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
					Directory.CreateDirectory(dir);
			}

			string tempFile = Path.Combine(Path.GetTempPath(), "decompressed_" + Guid.NewGuid() + ".tmp");
			using (FileStream fs = file.OpenRead())
			using (GZipStream gz = new GZipStream(fs, CompressionMode.Decompress))
			using (FileStream fos = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
			{
				gz.CopyTo(fos);
			}

			bool isTar = true;
			try
			{
				using (FileStream tfs = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
				{
					using (TarReader tarReader = new TarReader(tfs))
					{
						if (tarReader.GetNextEntry() == null)
							isTar = false;
					}
				}
			}
			catch { }

			if (isTar)
			{
				using (FileStream tfs = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
				{
					TarFile.ExtractToDirectory(tfs, fullOutputPath, overwriteFiles: true);
				}
			}
			else
			{
				string name = file.Name;
				if (name.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
					name = name.Substring(0, name.Length - 3);
				string singleOutFile = isDirectoryPath
					? Path.Combine(fullOutputPath, name)
					: fullOutputPath;

				try
				{
					File.Move(tempFile, singleOutFile);
				}
				catch
				{
					using (FileStream inStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
					using (FileStream outStream = new FileStream(singleOutFile, FileMode.Create, FileAccess.Write))
					{
						inStream.CopyTo(outStream);
						File.Delete(tempFile);
					}
				}
			}

			if (File.Exists(tempFile))
				File.Delete(tempFile);
		}

#endif
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
					string destFileName = Path.GetFullPath(destinationPath);
					string fullDestDirPath = Path.GetFullPath(outputPath + Path.DirectorySeparatorChar);
					if (!destFileName.StartsWith(fullDestDirPath))
					{
						throw new InvalidOperationException("Entry is outside the target dir: " + destFileName);
					}


					if (string.IsNullOrEmpty(entry.Name))
					{
						Directory.CreateDirectory(destFileName);
					}
					else
					{
#if NETCORE
						string destinationDir = Path.GetDirectoryName(destFileName)!;
#else
						string destinationDir = Path.GetDirectoryName(destFileName);
						if (!string.IsNullOrEmpty(destinationDir))
							Directory.CreateDirectory(destinationDir);
#endif
						entry.ExtractToFile(destFileName, true);
					}
				}
			}
		}

	}
}
