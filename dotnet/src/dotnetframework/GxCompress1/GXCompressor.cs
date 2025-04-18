using GeneXus.Utils;
using GeneXus;
using System.IO.Compression;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Security;

#if NETCORE
using System.Formats.Tar;
#else
using SharpCompress.Writers.Tar;
using SharpCompress.Readers.Tar;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
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
					case "tar":
						DecompressTar(archiveFile, path);
						break;
					case "gz":
						DecompressGzip(archiveFile, path);
						break;
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
		private static void CompressToTar(FileInfo[] files, string outputPath)
		{
			if (string.IsNullOrEmpty(outputPath)) throw new ArgumentException("The output path must not be null or empty");
			if (File.Exists(outputPath)) throw new IOException("Output file already exists");

			using (var output = File.Create(outputPath))
			{
				using (var tarWriter = new TarWriter(output, TarEntryFormat.Pax))
				{
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
			}
		}
#else
		[SecurityCritical]
		private static void CompressToTar(FileInfo[] files, string outputPath)
		{
			if (string.IsNullOrEmpty(outputPath))
				throw new ArgumentException("The output path must not be null or empty");

			if (File.Exists(outputPath))
				throw new IOException("Output file already exists");

			using (var output = File.Create(outputPath))
			using (var tarWriter = WriterFactory.Open(output, ArchiveType.Tar, CompressionType.None))
			{
				foreach (var file in files)
				{
					if (file == null || !file.Exists) continue;
					AddFileToTar(tarWriter, file, file.Name);
				}
			}
		}
		[SecurityCritical]
		private static void AddFileToTar(IWriter writer, FileSystemInfo file, string entryName)
		{
			if (file is DirectoryInfo dir)
			{
				foreach (var child in dir.GetFileSystemInfos())
				{
					string childEntryName = Path.Combine(entryName, child.Name).Replace("\\", "/"); // Normalizar nombres en TAR
					AddFileToTar(writer, child, childEntryName);
				}
			}
			else if (file is FileInfo fileInfo)
			{
				writer.Write(entryName, fileInfo.FullName);
			}
		}

#endif
		[SecurityCritical]
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
					string tarFilePath = Path.GetTempFileName();
#if NETCORE
            TarFile.CreateFromDirectory(tarTemp, tarFilePath, false);
#else
					CreateTarFromDirectory(tarTemp, tarFilePath);
#endif
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

#if !NETCORE
		[SecurityCritical]
		private static void CreateTarFromDirectory(string sourceDirectory, string tarFilePath)
		{
			using (var stream = File.Create(tarFilePath))
			using (var writer = WriterFactory.Open(stream, ArchiveType.Tar, CompressionType.None))
			{
				foreach (string filePath in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
				{
					string entryName = filePath.Substring(sourceDirectory.Length).TrimStart(Path.DirectorySeparatorChar);
					writer.Write(entryName, filePath);
				}
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
					if (string.IsNullOrEmpty(entry.Name))
					{
						Directory.CreateDirectory(fullPath);
					}
					else
					{
#if NETCORE
						string directoryPath = Path.GetDirectoryName(fullPath)!;
#else
						string directoryPath = Path.GetDirectoryName(fullPath);
						if (directoryPath != null)
						{
							Directory.CreateDirectory(directoryPath);
						}
#endif

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
			using (FileStream fs = file.OpenRead())
			{
#if NETCORE
				TarFile.ExtractToDirectory(fs, outputPath, overwriteFiles: true);
#else
				var reader = ReaderFactory.Open(fs);
				while (reader.MoveToNextEntry())
				{
					if (!reader.Entry.IsDirectory)
					{
						ExtractionOptions opt = new ExtractionOptions
						{
							ExtractFullPath = true,
							Overwrite = true
						};
						reader.WriteEntryToDirectory(outputPath, opt);
					}
				}
#endif
			}
		}
		[SecurityCritical]
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

			bool isTar = true;
			try
			{
#if NETCORE
				using (FileStream tfs = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
				{
					using (TarReader tarReader = new TarReader(tfs))
					{

						if (tarReader.GetNextEntry() == null)
							isTar = false;
					}
				}
#endif
			}
			catch { }

			if (isTar)
			{
				using (FileStream tfs = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
				{
#if NETCORE
					TarFile.ExtractToDirectory(tfs, outputPath, overwriteFiles: true);
#else
					var reader = ReaderFactory.Open(tfs);
					while (reader.MoveToNextEntry())
					{
						if (!reader.Entry.IsDirectory)
						{
							ExtractionOptions opt = new ExtractionOptions
							{
								ExtractFullPath = true,
								Overwrite = true
							};
							reader.WriteEntryToDirectory(outputPath, opt);
						}
					}
#endif
				}
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
					using (FileStream inStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
					{
						using (FileStream outStream = new FileStream(singleOutFile, FileMode.Create, FileAccess.Write))
						{
							inStream.CopyTo(outStream);
							File.Delete(tempFile);
						}
					}

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
#if NETCORE
						string destinationDir = Path.GetDirectoryName(destinationPath)!;
#else
						string destinationDir = Path.GetDirectoryName(destinationPath);
						if (!string.IsNullOrEmpty(destinationDir))
							Directory.CreateDirectory(destinationDir);
#endif
						entry.ExtractToFile(destinationPath, true);
					}
				}
			}
		}

	}
}
