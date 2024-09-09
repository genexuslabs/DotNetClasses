using System;
using System.Globalization;
using System.Security;

namespace GamUtils.Utils.Json
{
	[SecuritySafeCritical]
	internal class UnixTimestamp
	{
		[SecuritySafeCritical]
		internal static long Create(DateTime gxdate)
		{
			return (long)gxdate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
		}
	}
}
