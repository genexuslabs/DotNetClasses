using System.Security;

namespace GeneXusCryptography.Commons
{
	[SecuritySafeCritical]
	public interface IChecksumObject
	{
		  string GenerateChecksum(string input, string inputType, string checksumAlgorithm);
		  bool VerifyChecksum(string input, string inputType, string checksumAlgorithm, string digest);
	}
}
