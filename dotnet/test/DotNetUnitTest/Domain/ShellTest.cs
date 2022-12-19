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
		[Fact]
		public void WorkingDirForFullQualifiedBat()
		{
			string pathName = Directory.GetParent(Directory.GetCurrentDirectory()).FullName; //bat is in a different directory to the current dir
			string fileName = Path.Combine(pathName, "TestCurrentDir.bat");
			string outputFileName = Path.Combine(Directory.GetCurrentDirectory(), "output.txt"); //Current dir of the process must be the main current dir
			File.WriteAllText(fileName, "cd > output.txt");
			if (File.Exists(outputFileName))
			{
				File.Delete(outputFileName);
			}
			int errorCode = GXUtil.Shell($"{fileName}", 1, 0);
			string outputTxt = File.ReadAllText(outputFileName);
			Assert.Equal(Directory.GetCurrentDirectory(), outputTxt.Trim());
			Assert.Equal(0, errorCode);
		}

	}
	
}
