using System.IO;
using GeneXus.Utils;
using Xunit;

namespace xUnitTesting
{

	public class ShellTest
	{
		[Fact]
		public void ExecutableTest()
		{
			string fileName = "hello.bat";
			File.WriteAllText(fileName, "echo \"hello %1\"");
			int errorCode = GXUtil.Shell($"{fileName}  test", 1, 1);
			Assert.Equal(0, errorCode);
		}
		[Fact]
		public void ExecutableWithSpacesTest()
		{
			string fileName = "hello world.bat";
			File.WriteAllText(fileName, "echo \"hello %1\"");
			int errorCode = GXUtil.Shell($"'{fileName}'  test", 1, 1);
			Assert.Equal(0, errorCode);
		}

		[Fact]
		public void WorkingDirWithSpacesTest()
		{
			string fileName = Path.Combine(Directory.GetCurrentDirectory(), "my dir", "hello world.bat");
			FileInfo fi = new FileInfo(fileName);
			Directory.CreateDirectory(fi.DirectoryName);
			File.WriteAllText(fileName, "echo \"hello %1\"");
			int errorCode = GXUtil.Shell($"'{fileName}'  test", 1, 1);
			Assert.Equal(0, errorCode);
		}
	}
}
