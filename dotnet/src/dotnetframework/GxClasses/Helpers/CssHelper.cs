using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GeneXus.Utils
{
	public class CSSHelper
	{
		private static Regex multiSemicolonRegex = new Regex(";{2,}", RegexOptions.Compiled);

		public static string Prettify(string uglyCSS)
		{
			return CleanupSemicolons(uglyCSS);
		}

		public static string CleanupSemicolons(string uglyCSS)
		{
			if (uglyCSS.Length > 1)
			{
				string betterCSS = multiSemicolonRegex.Replace(uglyCSS, ";");
				return betterCSS[0] == ';' ? betterCSS.Substring(1) : betterCSS;
			}
			return uglyCSS;
		}
	}
}


