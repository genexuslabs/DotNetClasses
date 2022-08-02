using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Utils;

namespace UnitTesting
{
	public class FileSystemTest
	{
#pragma warning disable CA2211 // Non-constant fields should not be visible
		protected static string BaseDir = FileUtil.GetStartupDirectory();
#pragma warning restore CA2211 // Non-constant fields should not be visible
	}
	
}
