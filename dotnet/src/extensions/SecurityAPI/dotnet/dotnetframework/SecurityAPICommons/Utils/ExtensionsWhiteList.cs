using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace SecurityAPICommons.Utils
{
	[SecuritySafeCritical]
	public class ExtensionsWhiteList
	{
		private List<string> whitelist;

		public ExtensionsWhiteList()
		{
			this.whitelist = new List<string>();
		}


		[SecuritySafeCritical]
		public void SetExtension(string value)
		{
			if (value[0] != '.')
			{
				value = "." + value;
			}
			this.whitelist.Add(value);
		}

		[SecuritySafeCritical]
		public bool IsValid(string path)
		{
			string ext = SecurityUtils.getFileExtension(path);
			for (int i = 0; i < this.whitelist.Count; i++)
			{
				if (SecurityUtils.compareStrings(ext, this.whitelist.ElementAt(i)))
				{
					return true;
				}
			}
			return false;
		}

		[SecuritySafeCritical]
		public bool IsEmpty()
		{
			if (this.whitelist.Count == 0)
			{
				return true;
			}
			return false;
		}

	}
}
