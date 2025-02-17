using System.IO;
using GeneXus.Utils;

namespace GeneXus.Mail.Util
{
	internal class GXMailHelper
	{
		internal static string FixAndEnsureUniqueFileName(string attachDir, string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				name = Path.GetRandomFileName();
			}
			int idx = 1;
			string nameOri = Path.GetFileNameWithoutExtension(name);
			while (File.Exists(Path.Combine(attachDir, name)))
			{
				name = $"{nameOri} ({idx}).{Path.GetExtension(name)}";
				idx = idx + 1;
			}

			if (Path.Combine(attachDir, name).Length > 200)
			{
				name = Path.GetRandomFileName().Replace(".", "") + "." + Path.GetExtension(name);
			}
			return FileUtil.FixFileName(name, attachDir);
		}

	}
}
