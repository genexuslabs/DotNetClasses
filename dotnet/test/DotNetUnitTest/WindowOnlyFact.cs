using System.Runtime.InteropServices;
using Xunit;

namespace DotNetUnitTest
{
	public sealed class WindowsOnlyFactAttribute : FactAttribute
	{
		public WindowsOnlyFactAttribute()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				Skip = "Test skipped because it runs only on Windows.";
			}
		}
	}
}
