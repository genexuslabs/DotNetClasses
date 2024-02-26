using System.Security;
using GeneXusCryptography.AsymmetricUtils;

namespace GeneXusCryptography.Commons
{
	[SecuritySafeCritical]
	public interface IStandardSignerObject
	{
		string Sign(string plaintText, SignatureStandardOptions signatureStandardOptions);
		bool Verify(string signed, string plainText, SignatureStandardOptions signatureStandardOptions);
	}
}
