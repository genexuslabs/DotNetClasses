using System;
using System.Collections.Generic;
#if !NETCORE
using System.Formats.Tar;
#endif
using System.IO;
using System.IO.Compression;
using GeneXus;
using GeneXus.Utils;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using SharpCompress.Writers.Tar;

namespace Genexus.Compression
{
	public class GXCompressor : IGXCompressor
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger(typeof(GXCompressor).FullName);

		private static readonly Dictionary<string, string> errorMessages = new Dictionary<string, string>();

		static GXCompressor()
		{
			errorMessages.Add("GENERIC_ERROR", "An error occurred during the compression/decompression process: ");
			errorMessages.Add("NO_FILES_ADDED", "No files have been added for compression.");
			errorMessages.Add("FILE_NOT_EXISTS", "File does not exist: ");
			errorMessages.Add("FOLDER_NOT_EXISTS", "The specified folder does not exist: ");
			errorMessages.Add("UNSUPPORTED_FORMAT", "Unsupported compression/decompression format: ");
			errorMessages.Add("EMPTY_FILE", "The selected file is empty: ");
		}

		public static string GetMessage(string key)
		{
			if (errorMessages.TryGetValue(key, out string value))
			{
				return value;
			}
			return "An error occurred in the compression/decompression process: ";
		}

		private static void StorageMessages(string error, GXBaseCollection<SdtMessages_Message> messages)
		{
			if (messages != null)
			{
				SdtMessages_Message msg = new SdtMessages_Message()
				{
					gxTpr_Type = 1,
					gxTpr_Description = error
				};
				messages.Add(msg);
			}
		}

		public static bool Compress(List<string> files, string path, ref GXBaseCollection<SdtMessages_Message> messages)
		{
			if (files.Count == 0)
			{
				GXLogging.Error(log, $"{GetMessage("NO_FILES_ADDED")}");
				StorageMessages($"{GetMessage("NO_FILES_ADDED")}", messages);
				return false;
			}

			List<FileInfo> toCompress = new List<FileInfo>();
			foreach (string filePath in files)
			{
				FileInfo file = new FileInfo(filePath);
				if (!file.Exists)
				{
					GXLogging.Error(log, $"{GetMessage("FILE_NOT_EXISTS")}{filePath}");
					StorageMessages($"{GetMessage("FILE_NOT_EXISTS")}" + filePath, messages);
					continue;
				}
				toCompress.Add(file);
			}

			if (toCompress.Count == 0)
			{
				GXLogging.Error(log, $"{GetMessage("NO_FILES_ADDED")}");
				StorageMessages($"{GetMessage("NO_FILES_ADDED")}", messages);
				return false;
			}

			try
			{
				string compressionFormat = FileUtil.GetFileType(path).ToLower();

				switch (compressionFormat)
				{
					case "zip":
						CompressToZip(toCompress.ToArray(), path);
						break;
					case "tar":
						CompressToTar(toCompress.ToArray(), path);
						break;
					case "gz":
						CompressToGzip(toCompress.ToArray(), path);
						break;
					case "jar":
						CompressToJar(toCompress.ToArray(), path);
						break;
					default:
						GXLogging.Error(log, $"{GetMessage("UNSUPPORTED_FORMAT")}" + compressionFormat);
						StorageMessages($"{GetMessage("UNSUPPORTED_FORMAT")}" + compressionFormat, messages);
						return false;
				}
				return true;
			}
			catch (Exception e)
			{
				GXLogging.Error(log, $"{GetMessage("GENERIC_ERROR")}", e);
				StorageMessages(e.Message, messages);
				return false;
			}
		}

		public static Compression NewCompression(string path, string format, ref GXBaseCollection<SdtMessages_Message> messages)
		{
			return new Compression(path, ref messages);
		}

		public static bool Decompress(string file, string path, ref GXBaseCollection<SdtMessages_Message> messages)
		{
			FileInfo fileInfo = new FileInfo(file);
			if (!fileInfo.Exists)
			{
				GXLogging.Error(log, $"{GetMessage("FILE_NOT_EXISTS")}" + fileInfo.FullName);
				StorageMessages($"{GetMessage("FILE_NOT_EXISTS")}" + fileInfo.FullName, messages);
				return false;
			}
			if (fileInfo.Length == 0)
			{
				GXLogging.Error(log, $"{GetMessage("EMPTY_FILE")}");
				StorageMessages($"{GetMessage("EMPTY_FILE")}", messages);
				return false;
			}
			string extension = FileUtil.GetFileType(fileInfo.Name).ToLower();
			try
			{
				switch (extension)
				{
					case "zip":
						DecompressZip(fileInfo, path);
						break;
					case "tar":
						DecompressTar(fileInfo, path);
						break;
					case "gz":
						DecompressGzip(fileInfo, path);
						break;
					case "jar":
						DecompressJar(fileInfo, path);
						break;
					case "rar":
						DecompressRar(fileInfo, path);
						break;
					case "7z":
						Decompress7z(fileInfo, path);
						break;
					default:
						GXLogging.Error(log, $"{GetMessage("UNSUPPORTED_FORMAT")}" + extension);
						StorageMessages($"{GetMessage("UNSUPPORTED_FORMAT")}" + extension, messages);
						return false;
				}
				return true;
			}
			catch (Exception e)
			{
				GXLogging.Error(log, $"{GetMessage("GENERIC_ERROR")}", e);
				StorageMessages(e.Message, messages);
				return false;
			}
		}

		private static void CompressToZip(FileSystemInfo[] files, string outputPath)
		{
			using (FileStream zipToOpen = new FileStream(outputPath, FileMode.Create))
			{
				using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
				{
					foreach (FileSystemInfo fsi in files)
					{
						if (fsi is FileInfo fileInfo)
						{
							string entryName = fileInfo.Name;
							AddFileToArchive(fileInfo.FullName, entryName, archive);
						}
						else if (fsi is DirectoryInfo dirInfo)
						{
							AddDirectoryToArchive(dirInfo, archive, dirInfo.FullName);
						}
					}
				}
			}
		}

		private static void AddFileToArchive(string filePath, string entryName, ZipArchive archive)
		{
			ZipArchiveEntry entry = archive.CreateEntry(entryName);
			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			using (Stream entryStream = entry.Open())
			{
				fs.CopyTo(entryStream);
			}
		}

		private static void AddDirectoryToArchive(DirectoryInfo directory, ZipArchive archive, string basePath)
		{
			foreach (var file in directory.GetFiles())
			{
				string entryName = file.FullName.Substring(basePath.Length + 1).Replace('\\', '/');
				AddFileToArchive(file.FullName, entryName, archive);
			}
			foreach (var subDirectory in directory.GetDirectories())
			{
				AddDirectoryToArchive(subDirectory, archive, basePath);
			}
		}

		private static void CompressToTar(FileSystemInfo[] files, string outputPath)
		{
#if !NETCORE
			using (FileStream tarStream = new FileStream(outputPath, FileMode.Create))
			using (TarWriter tarWriter = new TarWriter(tarStream, TarEntryFormat.Pax, leaveOpen: false))
			{
				foreach (FileSystemInfo fsi in files)
				{
					if (fsi is FileInfo fileInfo)
					{
						tarWriter.WriteEntry(fileInfo.FullName, fileInfo.Name);
					}
					else if (fsi is DirectoryInfo dirInfo)
					{
						AddDirectoryToTar(tarWriter, dirInfo, dirInfo.FullName);
					}
				}
			}

			void AddDirectoryToTar(TarWriter tarWriter, DirectoryInfo directory, string basePath)
			{
				foreach (var file in directory.GetFiles())
				{
					string entryName = file.FullName.Substring(basePath.Length + 1).Replace('\\', '/');
					tarWriter.WriteEntry(file.FullName, entryName);
				}
				foreach (var subDirectory in directory.GetDirectories())
				{
					AddDirectoryToTar(tarWriter, subDirectory, basePath);
				}
			}
#else
			using (FileStream tarStream = File.Create(outputPath))
			using (var tarOutputStream = new ICSharpCode.SharpZipLib.Tar.TarOutputStream(tarStream))
			{
				foreach (FileSystemInfo fsi in files)
				{
					if (fsi is FileInfo fileInfo)
					{
						AddFileToTar(tarOutputStream, fileInfo.FullName, fileInfo.Name);
					}
					else if (fsi is DirectoryInfo dirInfo)
					{
						AddDirectoryFilesToTar(tarOutputStream, dirInfo, dirInfo.FullName);
					}
				}
			}

			void AddFileToTar(ICSharpCode.SharpZipLib.Tar.TarOutputStream tarOutputStream, string filePath, string entryName)
			{
				var entry = ICSharpCode.SharpZipLib.Tar.TarEntry.CreateEntryFromFile(filePath);
				entry.Name = entryName.Replace('\\', '/');
				tarOutputStream.PutNextEntry(entry);

				using (FileStream fs = File.OpenRead(filePath))
				{
					fs.CopyTo(tarOutputStream);
				}
				tarOutputStream.CloseEntry();
			}

			void AddDirectoryFilesToTar(ICSharpCode.SharpZipLib.Tar.TarOutputStream tarOutputStream, DirectoryInfo directory, string basePath)
			{
				foreach (var file in directory.GetFiles())
				{
					string entryName = file.FullName.Substring(basePath.Length + 1).Replace('\\', '/');
					AddFileToTar(tarOutputStream, file.FullName, entryName);
				}
				foreach (var subDirectory in directory.GetDirectories())
				{
					AddDirectoryFilesToTar(tarOutputStream, subDirectory, basePath);
				}
			}
#endif
		}

		private static void CompressToGzip(FileSystemInfo[] files, string outputPath)
		{
#if !NETCORE
			string tempTarPath = Path.GetTempFileName();

			using (FileStream tarStream = new FileStream(tempTarPath, FileMode.Create))
			using (TarWriter tarWriter = new TarWriter(tarStream, TarEntryFormat.Pax, leaveOpen: false))
			{
				foreach (FileSystemInfo fsi in files)
				{
					if (fsi is FileInfo fileInfo)
					{
						tarWriter.WriteEntry(fileInfo.FullName, fileInfo.Name);
					}
					else if (fsi is DirectoryInfo dirInfo)
					{
						AddDirectoryToTar(tarWriter, dirInfo, dirInfo.FullName);
					}
				}
			}

			using (FileStream tarStream = new FileStream(tempTarPath, FileMode.Open))
			using (FileStream gzipStream = new FileStream(outputPath, FileMode.Create))
			using (GZipStream compressionStream = new GZipStream(gzipStream, CompressionLevel.Optimal))
			{
				tarStream.CopyTo(compressionStream);
			}

			File.Delete(tempTarPath);

			void AddDirectoryToTar(TarWriter tarWriter, DirectoryInfo directory, string basePath)
			{
				foreach (var file in directory.GetFiles())
				{
					string entryName = Path.GetRelativePath(basePath, file.FullName).Replace('\\', '/');
					tarWriter.WriteEntry(file.FullName, entryName);
				}
				foreach (var subDirectory in directory.GetDirectories())
				{
					AddDirectoryToTar(tarWriter, subDirectory, basePath);
				}
			}
#else
			// Code for .NET Framework 4.6.2 using SharpZipLib
			string tempTarPath = Path.GetTempFileName();

			using (FileStream tarStream = File.Create(tempTarPath))
			using (var tarOutputStream = new ICSharpCode.SharpZipLib.Tar.TarOutputStream(tarStream))
			{
				foreach (FileSystemInfo fsi in files)
				{
					if (fsi is FileInfo fileInfo)
					{
						AddFileToTar(tarOutputStream, fileInfo.FullName, fileInfo.Name);
					}
					else if (fsi is DirectoryInfo dirInfo)
					{
						AddDirectoryFilesToTar(tarOutputStream, dirInfo, dirInfo.FullName);
					}
				}
			}

			using (FileStream tarStream = File.OpenRead(tempTarPath))
			using (FileStream gzipStream = File.Create(outputPath))
			using (var gzipOutputStream = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(gzipStream))
			{
				tarStream.CopyTo(gzipOutputStream);
			}

			File.Delete(tempTarPath);

			void AddFileToTar(ICSharpCode.SharpZipLib.Tar.TarOutputStream tarOutputStream, string filePath, string entryName)
			{
				var entry = ICSharpCode.SharpZipLib.Tar.TarEntry.CreateEntryFromFile(filePath);
				entry.Name = entryName.Replace('\\', '/');
				tarOutputStream.PutNextEntry(entry);

				using (FileStream fs = File.OpenRead(filePath))
				{
					fs.CopyTo(tarOutputStream);
				}
				tarOutputStream.CloseEntry();
			}

			void AddDirectoryFilesToTar(ICSharpCode.SharpZipLib.Tar.TarOutputStream tarOutputStream, DirectoryInfo directory, string basePath)
			{
				foreach (var file in directory.GetFiles())
				{
					string entryName = Path.GetRelativePath(basePath, file.FullName).Replace('\\', '/');
					AddFileToTar(tarOutputStream, file.FullName, entryName);
				}
				foreach (var subDirectory in directory.GetDirectories())
				{
					AddDirectoryFilesToTar(tarOutputStream, subDirectory, basePath);
				}
			}
#endif
		}

		private static void CompressToJar(FileSystemInfo[] files, string outputPath)
		{
			using (FileStream jarStream = new FileStream(outputPath, FileMode.Create))
			using (ZipArchive archive = new ZipArchive(jarStream, ZipArchiveMode.Create))
			{
				// Add the manifest file
				ZipArchiveEntry manifestEntry = archive.CreateEntry("META-INF/MANIFEST.MF");
				using (StreamWriter writer = new StreamWriter(manifestEntry.Open()))
				{
					writer.WriteLine("Manifest-Version: 1.0");
				}

				foreach (FileSystemInfo fsi in files)
				{
					if (fsi is FileInfo fileInfo)
					{
						string entryName = fileInfo.Name;
						AddFileToArchive(fileInfo.FullName, entryName, archive);
					}
					else if (fsi is DirectoryInfo dirInfo)
					{
						AddDirectoryToArchive(dirInfo, archive, dirInfo.FullName);
					}
				}
			}

			void AddFileToArchive(string filePath, string entryName, ZipArchive archive)
			{
				ZipArchiveEntry entry = archive.CreateEntry(entryName);
				using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				using (Stream entryStream = entry.Open())
				{
					fs.CopyTo(entryStream);
				}
			}

			void AddDirectoryToArchive(DirectoryInfo directory, ZipArchive archive, string basePath)
			{
				foreach (var file in directory.GetFiles())
				{
					string entryName = Path.GetRelativePath(basePath, file.FullName).Replace('\\', '/');
					AddFileToArchive(file.FullName, entryName, archive);
				}
				foreach (var subDirectory in directory.GetDirectories())
				{
					AddDirectoryToArchive(subDirectory, archive, basePath);
				}
			}
		}

		private static void DecompressZip(FileInfo file, string directory)
		{
			ZipFile.ExtractToDirectory(file.FullName, directory);
		}

		private static void DecompressTar(FileInfo file, string directory)
		{
#if !NETCORE
			TarFile.ExtractToDirectory(file.FullName, directory, overwriteFiles: true);
#else
			using (Stream inStream = File.OpenRead(file.FullName))
			using (var tarInputStream = new ICSharpCode.SharpZipLib.Tar.TarInputStream(inStream))
			{
				ICSharpCode.SharpZipLib.Tar.TarEntry tarEntry;
				while ((tarEntry = tarInputStream.GetNextEntry()) != null)
				{
					string outPath = Path.Combine(directory, tarEntry.Name);
					if (tarEntry.IsDirectory)
					{
						Directory.CreateDirectory(outPath);
					}
					else
					{
						Directory.CreateDirectory(Path.GetDirectoryName(outPath));
						using (FileStream outStream = File.Create(outPath))
						{
							tarInputStream.CopyEntryContents(outStream);
						}
					}
				}
			}
#endif
		}

		private static void DecompressGzip(FileInfo file, string directory)
		{
			string decompressedFileName = Path.GetFileNameWithoutExtension(file.Name);
			string outputPath = Path.Combine(directory, decompressedFileName);

			using (FileStream originalFileStream = file.OpenRead())
			using (FileStream decompressedFileStream = File.Create(outputPath))
			using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
			{
				decompressionStream.CopyTo(decompressedFileStream);
			}
		}

		private static void Decompress7z(FileInfo file, string directory)
		{
			using (ArchiveFile archiveFile = new ArchiveFile(file.FullName))
			{
				archiveFile.Extract(directory);
			}
		}

		private static void DecompressJar(FileInfo file, string directory)
		{
			ZipFile.ExtractToDirectory(file.FullName, directory);
		}

		private static void DecompressRar(FileInfo file, string directory)
		{
			using (var archive = RarArchive.Open(file.FullName))
			{
				foreach (var entry in archive.Entries)
				{
					if (!entry.IsDirectory)
					{
						entry.WriteToDirectory(directory, new ExtractionOptions()
						{
							ExtractFullPath = true,
							Overwrite = true
						});
					}
				}
			}
		}
	}
}