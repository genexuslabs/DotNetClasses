using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GeneXus.Storage.GXAmazonS3
{
	public class S3UploadStream : Stream
	{
		/* Note the that maximum size (as of now) of a file in S3 is 5TB so it isn't
         * safe to assume all uploads will work here.  MAX_PART_SIZE times MAX_PART_COUNT
         * is ~50TB, which is too big for S3. */
		const long MIN_PART_LENGTH = 5L * 1024 * 1024; // all parts but the last this size or greater
		const long MAX_PART_LENGTH = 5L * 1024 * 1024 * 1024; // 5GB max per PUT
		const long MAX_PART_COUNT = 10000; // no more than 10,000 parts total
		const long DEFAULT_PART_LENGTH = MIN_PART_LENGTH;

		internal class Metadata
		{
			public string BucketName { get; set; }
			public string Key { get; set; }
			public long PartLength { get; set; } = DEFAULT_PART_LENGTH;

			public int PartCount { get; set; } = 0;
			public string UploadId { get; set; }
			public MemoryStream CurrentStream { get; set; }
			public S3CannedACL Acl { get; set; }
			public string ContentType { get; set; }

			public long Position { get; set; } = 0;
			public long Length { get; set; } = 0;

			public List<Task> Tasks = new List<Task>();
			public ConcurrentDictionary<int, string> PartETags = new ConcurrentDictionary<int, string>();
		}

		Metadata _metadata = new Metadata();
		IAmazonS3 _s3 = null;

		public S3UploadStream(IAmazonS3 s3, string s3uri, long partLength = DEFAULT_PART_LENGTH)
			: this(s3, new Uri(s3uri), partLength)
		{
		}

		public S3UploadStream(IAmazonS3 s3, Uri s3uri, long partLength = DEFAULT_PART_LENGTH)
			: this(s3, s3uri.Host, s3uri.LocalPath.Substring(1), partLength)
		{
		}
		public S3UploadStream(IAmazonS3 s3, string bucket, string key, long partLength = DEFAULT_PART_LENGTH)
			: this(s3, bucket, key, null, null, partLength)
		{

		}
		public S3UploadStream(IAmazonS3 s3, string bucket, string key, S3CannedACL acl, string cType = null, long partLength = DEFAULT_PART_LENGTH)
		{
			_s3 = s3;
			_metadata.BucketName = bucket;
			_metadata.Key = key;
			_metadata.PartLength = partLength;
			_metadata.Acl = acl;
			_metadata.ContentType = cType;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_metadata != null)
				{
					Flush(true);
					CompleteUpload();
				}
			}
			_metadata = null;
			base.Dispose(disposing);
		}

		public override bool CanRead => false;
		public override bool CanSeek => false;
		public override bool CanWrite => true;
		public override long Length => _metadata.Length = Math.Max(_metadata.Length, _metadata.Position);

		public override long Position
		{
			get => _metadata.Position;
			set => throw new NotImplementedException();
		}

		public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();
		public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

		public override void SetLength(long value)
		{
			_metadata.Length = Math.Max(_metadata.Length, value);
			_metadata.PartLength = Math.Max(MIN_PART_LENGTH, Math.Min(MAX_PART_LENGTH, _metadata.Length / MAX_PART_COUNT));
		}

		private void StartNewPart()
		{
			if (_metadata.CurrentStream != null)
			{
				Flush(false);
			}
			_metadata.CurrentStream = new MemoryStream();
			_metadata.PartLength = Math.Min(MAX_PART_LENGTH, Math.Max(_metadata.PartLength, (_metadata.PartCount / 2 + 1) * MIN_PART_LENGTH));
		}

		public override void Flush()
		{
			Flush(false);
		}

		private void Flush(bool disposing)
		{
			if ((_metadata.CurrentStream == null || _metadata.CurrentStream.Length < MIN_PART_LENGTH) &&
				!disposing)
				return;

			if (_metadata.UploadId == null)
			{

				InitiateMultipartUploadRequest uploadRequest = new InitiateMultipartUploadRequest()
				{
					BucketName = _metadata.BucketName,
					Key = _metadata.Key					
				};

				if (_metadata.Acl != null)
				{
					uploadRequest.CannedACL = _metadata.Acl;
				}

				if (!string.IsNullOrEmpty(_metadata.ContentType))
				{
					uploadRequest.ContentType = _metadata.ContentType;
				}
				_metadata.UploadId = _s3.InitiateMultipartUploadAsync(uploadRequest).GetAwaiter().GetResult().UploadId;
			}

			if (_metadata.CurrentStream != null)
			{
				int i = ++_metadata.PartCount;

				_metadata.CurrentStream.Seek(0, SeekOrigin.Begin);
				var request = new UploadPartRequest()
				{
					BucketName = _metadata.BucketName,
					Key = _metadata.Key,
					UploadId = _metadata.UploadId,
					PartNumber = i,
					IsLastPart = disposing,
					InputStream = _metadata.CurrentStream
				};
				_metadata.CurrentStream = null;

				var upload = Task.Run(() =>
				{
					UploadPartResponse response = _s3.UploadPartAsync(request).GetAwaiter().GetResult();
					_metadata.PartETags.AddOrUpdate(i, response.ETag,
						(n, s) => response.ETag);
					request.InputStream.Dispose();
				});
				_metadata.Tasks.Add(upload);
			}
		}

		private void CompleteUpload()
		{
			Task.WaitAll(_metadata.Tasks.ToArray());

			if (Length > 0)
			{
				_s3.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest()
				{
					BucketName = _metadata.BucketName,
					Key = _metadata.Key,
					PartETags = _metadata.PartETags.Select(e => new PartETag(e.Key, e.Value)).ToList(),
					UploadId = _metadata.UploadId
				}).GetAwaiter().GetResult();
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (count == 0) return;

			// write as much of the buffer as will fit to the current part, and if needed
			// allocate a new part and continue writing to it (and so on).
			int o = offset;
			int c = Math.Min(count, buffer.Length - offset); // don't over-read the buffer, even if asked to
			do
			{
				if (_metadata.CurrentStream == null || _metadata.CurrentStream.Length >= _metadata.PartLength)
					StartNewPart();

				long remaining = _metadata.PartLength - _metadata.CurrentStream.Length;
				int w = Math.Min(c, (int)remaining);
				_metadata.CurrentStream.Write(buffer, o, w);

				_metadata.Position += w;
				c -= w;
				o += w;
			} while (c > 0);
		}
	}
}