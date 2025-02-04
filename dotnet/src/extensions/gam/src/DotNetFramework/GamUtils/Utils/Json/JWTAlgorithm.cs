using Jose;
using log4net;

namespace GamUtils.Utils.Json
{
	public enum JWTAlgorithm
	{
		none, HS256, HS384, HS512, RS256, RS512
	}

	public class JWTAlgorithmUtils
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(JWTAlgorithmUtils));
		internal static JwsAlgorithm GetJWSAlgorithm(JWTAlgorithm alg)
		{
			logger.Debug("GetJWSAlgorithm");
			switch (alg)
			{
				case JWTAlgorithm.HS256:
					return JwsAlgorithm.HS256;
				case JWTAlgorithm.HS512:
					return JwsAlgorithm.HS512;
				case JWTAlgorithm.HS384:
					return JwsAlgorithm.HS384;
				case JWTAlgorithm.RS256:
					return JwsAlgorithm.RS256;
				case JWTAlgorithm.RS512:
					return JwsAlgorithm.RS512;
				default:
					logger.Error("GetJWSAlgorithm - not implemented algorithm");
					return JwsAlgorithm.none;
			}
		}

		internal static JWTAlgorithm GetJWTAlgoritm(string alg)
		{
			logger.Debug("GetJWTAlgoritm");
			switch (alg.Trim().ToUpper())
			{
				case "HS256":
					return JWTAlgorithm.HS256;
				case "HS512":
					return JWTAlgorithm.HS512;
				case "HS384":
					return JWTAlgorithm.HS384;
				case "RS256":
					return JWTAlgorithm.RS256;
				case "RS512":
					return JWTAlgorithm.RS512;
				default:
					logger.Error("GetJWTAlgoritm- not implemented algorithm");
					return JWTAlgorithm.none;
			}
		}

		internal static bool IsSymmetric(JWTAlgorithm alg)
		{
			logger.Debug("IsSymmetric");
			switch (alg)
			{
				case JWTAlgorithm.HS256:
				case JWTAlgorithm.HS384:
				case JWTAlgorithm.HS512:
					return true;
				case JWTAlgorithm.RS256:
				case JWTAlgorithm.RS512:
					return false;
				default:
					logger.Error("IsSymmetric - not implemented algorithm");
					return false;
			}
		}
	}
}
