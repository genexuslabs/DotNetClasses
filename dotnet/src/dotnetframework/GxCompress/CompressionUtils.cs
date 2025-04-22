#if NETCORE
using System.Formats.Tar;
#else
using System;
using System.IO;
#endif
using System.IO.Compression;

namespace Genexus.Compression
{
	public class CompressionUtils
	{
		/**
		 * Counts the number of entries in an archive file.
		 *
		 * @param archiveFile The archive file to analyze
		 * @return The number of entries in the archive
		 */
		public static int CountArchiveEntries(FileInfo archiveFile)
		{
			string ext = (Path.GetExtension(archiveFile.Name)?.TrimStart('.').ToLowerInvariant()) ?? "";
			switch (ext)
			{
				case "zip":
				case "jar":
					using (FileStream stream = archiveFile.OpenRead())
					using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Read))
					{
						return zip.Entries.Count;
					}
#if NETCORE
				case "tar":
					int count = 0;
					using (FileStream stream = archiveFile.OpenRead())
					using (TarReader reader = new TarReader(stream))
					{
						while (reader.GetNextEntry() != null)
						{
							count++;
						}
					}
					return count;
#endif
				case "gz":
					return 1;
				default:
					throw new ArgumentException("Unsupported archive format: " + ext);
			}
		}

		/**
		 * Checks if an archive is safe to extract (no path traversal/zip slip).
		 *
		 * @param archiveFile The archive file to check
		 * @param targetDir The target directory for extraction
		 * @return true if the archive is safe, false otherwise
		 */
		public static bool IsArchiveSafe(FileInfo archiveFile, string targetDir)
		{
			string ext = (Path.GetExtension(archiveFile.Name)?.TrimStart('.').ToLowerInvariant()) ?? "";
			string normalizedTarget = Path.GetFullPath(targetDir);
			switch (ext)
			{
				case "zip":
				case "jar":
					using (FileStream stream = archiveFile.OpenRead())
					using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Read))
					{
						foreach (ZipArchiveEntry entry in zip.Entries)
						{
							string destinationPath = Path.GetFullPath(Path.Combine(normalizedTarget, entry.FullName));
							if (!destinationPath.StartsWith(normalizedTarget + Path.DirectorySeparatorChar, StringComparison.Ordinal) && destinationPath != normalizedTarget)
							{
								return false;
							}
						}
					}
					return true;
#if NETCORE
				case "tar":
					using (FileStream stream = archiveFile.OpenRead())
					using (TarReader reader = new TarReader(stream))
					{
						TarEntry? entry;
						while ((entry = reader.GetNextEntry()) != null)
						{
							string destinationPath = Path.GetFullPath(Path.Combine(normalizedTarget, entry.Name));
							if (!destinationPath.StartsWith(normalizedTarget + Path.DirectorySeparatorChar, StringComparison.Ordinal) && destinationPath != normalizedTarget)
							{
								return false;
							}
						}
					}
					return true;
#endif
				case "gz":
					string fileName = archiveFile.Name;
					if (fileName.EndsWith(".gz") && fileName.Length > 3)
					{
						string extractedName = fileName.Substring(0, fileName.Length - 3);
						string destinationPath = Path.GetFullPath(Path.Combine(normalizedTarget, extractedName));
						return destinationPath.StartsWith(normalizedTarget + Path.DirectorySeparatorChar, StringComparison.Ordinal) || destinationPath == normalizedTarget;
					}
					return true;
				default:
					throw new ArgumentException("Unsupported archive format: " + ext);
			}
		}

		/**
		 * Gets the maximum file size of any entry in the archive.
		 *
		 * @param archiveFile The archive file to analyze
		 * @return The size of the largest file in the archive
		 */
		public static long GetMaxFileSize(FileInfo archiveFile)
		{
			string ext = (Path.GetExtension(archiveFile.Name)?.TrimStart('.').ToLowerInvariant()) ?? "";
			long maxSize = 0;
			switch (ext)
			{
				case "zip":
				case "jar":
					using (FileStream stream = archiveFile.OpenRead())
					using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Read))
					{
						foreach (ZipArchiveEntry entry in zip.Entries)
						{
							if (!IsDirectory(entry) && entry.Length > maxSize)
							{
								maxSize = entry.Length;
							}
						}
					}
					break;
#if NETCORE
				case "tar":
					using (FileStream stream = archiveFile.OpenRead())
					using (TarReader reader = new TarReader(stream))
					{
						TarEntry? entry;
						while ((entry = reader.GetNextEntry()) != null)
						{
							if (entry.EntryType != TarEntryType.Directory && entry.Length > maxSize)
							{
								maxSize = entry.Length;
							}
						}
					}
					break;
#endif
				case "gz":
					long size = 0;
					byte[] buffer = new byte[8192];
					using (FileStream fs = archiveFile.OpenRead())
					using (GZipStream gz = new GZipStream(fs, CompressionMode.Decompress))
					{
						int read;
						while ((read = gz.Read(buffer, 0, buffer.Length)) > 0)
						{
							size += read;
						}
					}
					maxSize = size;
					break;
				default:
					throw new ArgumentException("Unsupported archive format: " + ext);
			}
			return maxSize;
		}

		/**
		 * Estimates the total size of all files after decompression.
		 *
		 * @param archiveFile The archive file to analyze
		 * @return The estimated total size after decompression
		 */
		public static long EstimateDecompressedSize(FileInfo archiveFile)
		{
			string ext = (Path.GetExtension(archiveFile.Name)?.TrimStart('.').ToLowerInvariant()) ?? "";
			long totalSize = 0;
			switch (ext)
			{
				case "zip":
				case "jar":
					using (FileStream stream = archiveFile.OpenRead())
					using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Read))
					{
						foreach (ZipArchiveEntry entry in zip.Entries)
						{
							if (!IsDirectory(entry))
							{
								totalSize += entry.Length >= 0 ? entry.Length : entry.CompressedLength * 3;
							}
						}
					}
					break;
#if NETCORE
				case "tar":
					using (FileStream stream = archiveFile.OpenRead())
					using (TarReader reader = new TarReader(stream))
					{
						TarEntry? entry;
						while ((entry = reader.GetNextEntry()) != null)
						{
							if (entry.EntryType != TarEntryType.Directory)
							{
								totalSize += entry.Length;
							}
						}
					}
					break;
#endif
				case "gz":
					try
					{
						using (FileStream fs = archiveFile.OpenRead())
						{
							if (fs.Length < 4)
							{
								totalSize = archiveFile.Length * 5;
							}
							else
							{
								fs.Seek(fs.Length - 4, SeekOrigin.Begin);
								byte[] buffer = new byte[4];
								int bytesRead = fs.Read(buffer, 0, 4);
								if (bytesRead == 4)
								{
									int size = BitConverter.ToInt32(buffer, 0);
									totalSize = size > 0 ? size : (long)archiveFile.Length * 5;
								}
								else
								{
									totalSize = archiveFile.Length * 5;
								}
							}
						}
					}
					catch
					{
						totalSize = archiveFile.Length * 5;
					}
					break;
				default:
					throw new ArgumentException("Unsupported archive format: " + ext);
			}
			return totalSize;
		}

		private static bool IsDirectory(ZipArchiveEntry entry)
		{
			return entry.FullName.EndsWith("/");
		}
	}
}
