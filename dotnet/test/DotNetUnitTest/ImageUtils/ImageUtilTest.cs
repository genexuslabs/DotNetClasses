using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Utils;
using UnitTesting;
using Xunit;
#if NETCORE
using Image = GeneXus.Drawing.Image;
using GeneXus.Drawing.Imaging;
#else
using Image = System.Drawing.Image;
using System.Drawing.Imaging;
#endif

namespace DotNetCoreUnitTest.ImageUtils
{
	public class ImageUtilTest : FileSystemTest
	{
		private readonly string IMAGE_FILE_PATH = System.IO.Path.Combine(BaseDir, "resources", "bird-thumbnail.jpg");//Minions-PNG-Photo.png
		private readonly string IMAGE_FILE_PATH_OUTPUT = System.IO.Path.Combine(BaseDir, "resources", "bird-thumbnail-{0}-{1}.jpg");//Minions-PNG-Photo
		private readonly int IMAGE_HEIGHT = 900;
		private readonly int IMAGE_WIDTH = 720;

		[Fact]
		public void TestImageWidth()
		{
			int imageWidth = GxImageUtil.GetImageWidth(IMAGE_FILE_PATH);
			Assert.Equal(IMAGE_WIDTH, imageWidth);
		}

		private string Initialize(string name = "original")
		{
			string fileName = string.Format(CultureInfo.InvariantCulture, IMAGE_FILE_PATH_OUTPUT, name, Guid.NewGuid().ToString());

			File.Copy(IMAGE_FILE_PATH, fileName, true);
			return fileName;
		}

		[Fact]
		public void TestImageHeight()
		{
			string fileName = Initialize();
			int imageHeight = GxImageUtil.GetImageHeight(fileName);
			Assert.Equal(IMAGE_HEIGHT, imageHeight);
		}

		[Fact]
		public void TestImageScale()
		{
			string fileName = Initialize("scaled");
			int scale = 50;

			string imagePath = GxImageUtil.Scale(fileName, scale);

			int imageHeight = GxImageUtil.GetImageHeight(imagePath);
			Assert.Equal(IMAGE_HEIGHT * scale / 100, imageHeight);

			int imageWidth = GxImageUtil.GetImageWidth(imagePath);
			Assert.Equal(IMAGE_WIDTH * scale / 100, imageWidth);
		}

		[Fact]
		public void TestImageCrop()
		{
			string fileName = Initialize("croped");
			string imagePath = GxImageUtil.Crop(fileName, 10, 10, 300, 400);

			int imageHeight = GxImageUtil.GetImageHeight(imagePath);
			Assert.Equal(400, imageHeight);

			int imageWidth = GxImageUtil.GetImageWidth(imagePath);
			Assert.Equal(300, imageWidth);
		}


		[Fact]
		public void TestImageResize()
		{
			string fileName = Initialize("resized");
			string imagePath = GxImageUtil.Resize(fileName, 300, 400, false);

			int imageHeight = GxImageUtil.GetImageHeight(imagePath);
			Assert.Equal(400, imageHeight);

			int imageWidth = GxImageUtil.GetImageWidth(imagePath);
			Assert.Equal(300, imageWidth);
		}
		[Fact]
		public void TestImageRotate()
		{
			string fileName = Initialize("rotated");
			string imagePath = GxImageUtil.Rotate(fileName, 45);

			int imageHeight = GxImageUtil.GetImageHeight(imagePath);
			Assert.Equal(900, imageHeight);

			int imageWidth = GxImageUtil.GetImageWidth(imagePath);
			Assert.Equal(720, imageWidth);
		}


		[Fact]
		public void TestImageFlipHorizontally()
		{
			string fileName = Initialize("flippedHorizontally");
			string imagePath = GxImageUtil.FlipHorizontally(fileName);
			
		}

		[Fact]
		public void TestImageFlipVertically()
		{
			string fileName = Initialize("flippedVertically");
			string imagePath = GxImageUtil.FlipVertically(fileName);

		}

		[Fact]
		public void TestImageFileSize()
		{
			string fileName = Initialize();
			long fileSize = GxImageUtil.GetFileSize(fileName);

			Assert.Equal(113974, fileSize);

		}

		[Fact]
		public void TestImageSaveFromSignedUrlDoesNotEmbedQueryString()
		{
			string signedUrl = "https://bucket.s3.amazonaws.com/folder/bird-thumbnail.jpg?X-Amz-Expires=86400&X-Amz-Signature=abc123def456&X-Amz-Algorithm=AWS4-HMAC-SHA256";

			string destinationPath;
			using (Image image = Image.FromFile(IMAGE_FILE_PATH))
			{
				destinationPath = GxImageUtil.Save(image, signedUrl, ImageFormat.Jpeg);
			}

			Assert.False(string.IsNullOrEmpty(destinationPath));
			string destinationFileName = Path.GetFileName(destinationPath);
			Assert.DoesNotContain("?", destinationFileName, StringComparison.Ordinal);
			Assert.DoesNotContain("%3F", destinationFileName, StringComparison.Ordinal);
			Assert.DoesNotContain("X-Amz-Expires", destinationFileName, StringComparison.Ordinal);
			Assert.DoesNotContain("X-Amz-Signature", destinationFileName, StringComparison.Ordinal);

			Assert.Equal(IMAGE_HEIGHT, GxImageUtil.GetImageHeight(destinationPath));
			Assert.Equal(IMAGE_WIDTH, GxImageUtil.GetImageWidth(destinationPath));
		}
	}
}
