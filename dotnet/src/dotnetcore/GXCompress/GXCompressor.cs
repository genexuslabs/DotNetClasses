using GeneXus.Utils;
using GeneXus;
using System.IO.Compression;
using SevenZip;

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
					case "7z":
						CompressToSevenZ(toCompress, path);
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
					case "7z":
						Decompress7z(toDecompress, path);
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

		private static void CompressToSevenZ(FileInfo[] files, string outputPath)
		{
			if (files == null || outputPath == null)
				throw new ArgumentException("Files and outputPath must not be null");
			if (File.Exists(outputPath))
				throw new IOException("Output file already exists");

			var compressor = new SevenZipCompressor();
			compressor.CompressionMethod = CompressionMethod.Lzma2;
			var fileDictionary = new Dictionary<string, string>();

			void AddFile(FileSystemInfo fsi, string entryName)
			{
				if (fsi is DirectoryInfo di)
				{
					foreach (var child in di.GetFileSystemInfos())
						AddFile(child, entryName + "/" + child.Name);
				}
				else if (fsi is FileInfo fi)
				{
					fileDictionary[entryName] = fi.FullName;
				}
			}

			foreach (var file in files)
			{
				if (file == null || !file.Exists)
					continue;
				AddFile(file, file.Name);
			}

			compressor.CompressFiles(outputPath, fileDictionary.Values.ToArray());
		}



		private static void CompressToTar(FileInfo[] files, string outputPath)
		{
			if (string.IsNullOrEmpty(outputPath))
				throw new ArgumentException("The output path must not be null or empty");
			if (File.Exists(outputPath))
				throw new IOException("Output file already exists");
			using (var fs = File.Create(outputPath))
			using (var tarOut = new ICSharpCode.SharpZipLib.Tar.TarOutputStream(fs))
			{
				void AddFileToTar(FileSystemInfo file, string entryName)
				{
					if (file is DirectoryInfo di)
					{
						foreach (var child in di.GetFileSystemInfos())
							AddFileToTar(child, entryName + "/" + child.Name);
					}
					else if (file is FileInfo fi)
					{
						var entry = ICSharpCode.SharpZipLib.Tar.TarEntry.CreateEntryFromFile(fi.FullName);
						entry.Name = entryName;
						tarOut.PutNextEntry(entry);
						using (var stream = fi.OpenRead())
						{
							stream.CopyTo(tarOut);
						}
						tarOut.CloseEntry();
					}
				}

				foreach (var file in files)
				{
					if (file == null || !file.Exists)
						continue;
					AddFileToTar(file, file.Name);
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
				try { using (var fs = File.OpenWrite(outputPath)) { } } catch { throw new IOException("Cannot write to output file"); }
			}
			string parentDir = Path.GetDirectoryName(outputPath) ?? "";
			if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
				Directory.CreateDirectory(parentDir);
			bool singleFile = files.Length == 1 && files[0].Exists && (files[0].Attributes & FileAttributes.Directory) == 0;
			string tempFile = Path.Combine(string.IsNullOrEmpty(parentDir) ? "." : parentDir, Path.GetRandomFileName() + ".tmp");
			if (singleFile)
			{
				using (var fis = files[0].OpenRead())
				using (var fos = File.Create(tempFile))
				using (var gzos = new GZipStream(fos, System.IO.Compression.CompressionLevel.Optimal))
				{
					byte[] buffer = new byte[8192];
					int len;
					while ((len = fis.Read(buffer, 0, buffer.Length)) > 0)
						gzos.Write(buffer, 0, len);
				}
			}
			else
			{
				using (var fos = File.Create(tempFile))
				using (var gzos = new GZipStream(fos, System.IO.Compression.CompressionLevel.Optimal))
				using (var tarOut = new ICSharpCode.SharpZipLib.Tar.TarOutputStream(gzos))
				{
					tarOut.IsStreamOwner = false;
					var fileStack = new Stack<FileSystemInfo>();
					var pathStack = new Stack<string>();
					foreach (var file in files)
					{
						if (file != null)
						{
							fileStack.Push(file);
							pathStack.Push("");
						}
					}
					while (fileStack.Count > 0)
					{
						var current = fileStack.Pop();
						string currentPath = pathStack.Pop();
						string entryName = (string.IsNullOrEmpty(currentPath) ? "" : currentPath + "/") + current.Name;
						if ((current.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
						{
							DirectoryInfo di = (DirectoryInfo)current;
							var children = di.GetFileSystemInfos();
							if (children.Length > 0)
							{
								foreach (var child in children)
								{
									fileStack.Push(child);
									pathStack.Push(entryName);
								}
							}
							else
							{
								var entry = ICSharpCode.SharpZipLib.Tar.TarEntry.CreateEntryFromFile(current.FullName);
								entry.Name = entryName.EndsWith("/") ? entryName : entryName + "/";
								tarOut.PutNextEntry(entry);
								tarOut.CloseEntry();
							}
						}
						else
						{
							var entry = ICSharpCode.SharpZipLib.Tar.TarEntry.CreateEntryFromFile(current.FullName);
							entry.Name = entryName;
							tarOut.PutNextEntry(entry);
							using (var fs = ((FileInfo)current).OpenRead())
							{
								byte[] buffer = new byte[8192];
								int len;
								while ((len = fs.Read(buffer, 0, buffer.Length)) > 0)
									tarOut.Write(buffer, 0, len);
							}
							tarOut.CloseEntry();
						}
					}
				}
			}
			string finalName = outputPath;
			if (singleFile)
			{
				if (!finalName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
					finalName += ".gz";
			}
			else
			{
				if (finalName.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))
				{
				}
				else if (finalName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
					finalName = finalName.Substring(0, finalName.Length - 3) + ".tar.gz";
				else
					finalName += ".tar.gz";
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

		private static void Decompress7z(FileInfo file, string outputPath)
		{
			if (file == null || !file.Exists)
				throw new ArgumentException("File not found", nameof(file));
			if (string.IsNullOrEmpty(outputPath))
				throw new ArgumentException("Output path is null or empty", nameof(outputPath));
			Directory.CreateDirectory(outputPath);
			using (var extractor = new SevenZipExtractor(file.FullName))
			{
				foreach (var entry in extractor.ArchiveFileData)
				{
					string fullPath = Path.Combine(outputPath, entry.FileName);
					if (entry.IsDirectory)
					{
						Directory.CreateDirectory(fullPath);
					}
					else
					{
						Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
						using (var stream = File.Create(fullPath))
						{
							extractor.ExtractFile(entry.Index, stream);
						}
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
			using (var fs = file.OpenRead())
			using (var tarIn = new ICSharpCode.SharpZipLib.Tar.TarInputStream(fs))
			{
				ICSharpCode.SharpZipLib.Tar.TarEntry entry;
				byte[] buffer = new byte[8192];
				while ((entry = tarIn.GetNextEntry()) != null)
				{
					string entryPath = Path.Combine(outputPath, entry.Name);
					if (entry.IsDirectory)
					{
						Directory.CreateDirectory(entryPath);
					}
					else
					{
						string? parent = Path.GetDirectoryName(entryPath);
						if (!string.IsNullOrEmpty(parent))
						{
							Directory.CreateDirectory(parent);
						}
						if (!string.IsNullOrEmpty(parent))
							Directory.CreateDirectory(parent);
						using (var outStream = File.Create(entryPath))
						{
							int bytesRead;
							while ((bytesRead = tarIn.Read(buffer, 0, buffer.Length)) > 0)
							{
								outStream.Write(buffer, 0, bytesRead);
							}
						}
					}
				}
			}
		}

		private static void DecompressGzip(FileInfo file, string outputPath)
		{
			if (file == null || !file.Exists)
				throw new ArgumentException("The archive file does not exist or is not a file.", nameof(file));
			if (string.IsNullOrEmpty(outputPath) || !Directory.Exists(outputPath))
				throw new ArgumentException("The specified directory does not exist or is not a directory.", nameof(outputPath));
			string tempFile = Path.Combine(Path.GetTempPath(), "decompressed_" + Guid.NewGuid().ToString() + ".tmp");
			using (var fis = file.OpenRead())
			using (var gzipStream = new GZipStream(fis, System.IO.Compression.CompressionMode.Decompress))
			using (var fos = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
			{
				byte[] buffer = new byte[8192];
				int bytesRead;
				while ((bytesRead = gzipStream.Read(buffer, 0, buffer.Length)) > 0)
					fos.Write(buffer, 0, bytesRead);
			}
			bool isTar = false;
			try
			{
				using (var tempFis = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
				using (var tarTest = new ICSharpCode.SharpZipLib.Tar.TarInputStream(tempFis))
				{
					var testEntry = tarTest.GetNextEntry();
					if (testEntry != null)
						isTar = true;
				}
			}
			catch { }
			if (isTar)
			{
				using (var tarFis = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
				using (var tarInput = new ICSharpCode.SharpZipLib.Tar.TarInputStream(tarFis))
				{
					ICSharpCode.SharpZipLib.Tar.TarEntry entry;
					byte[] buffer = new byte[8192];
					while ((entry = tarInput.GetNextEntry()) != null)
					{
						string outPath = Path.Combine(outputPath, entry.Name);
						if (entry.IsDirectory)
							Directory.CreateDirectory(outPath);
						else
						{
							string? parentDir = Path.GetDirectoryName(outPath);
							if (!string.IsNullOrEmpty(parentDir))
								Directory.CreateDirectory(parentDir);
							using (var outStream = File.Create(outPath))
							{
								int count;
								while ((count = tarInput.Read(buffer, 0, buffer.Length)) > 0)
									outStream.Write(buffer, 0, count);
							}
						}
					}
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
					using (var inStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
					using (var outStream = new FileStream(singleOutFile, FileMode.Create, FileAccess.Write))
					{
						byte[] buffer = new byte[8192];
						int bytesRead;
						while ((bytesRead = inStream.Read(buffer, 0, buffer.Length)) > 0)
							outStream.Write(buffer, 0, bytesRead);
					}
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
