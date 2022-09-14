
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Security;
using GeneXusCryptography.Commons;
using SecurityAPICommons.Commons;
using GeneXusCryptography.SymmetricUtils;
using SecurityAPICommons.Config;
using SecurityAPICommons.Utils;
using System.IO;

namespace GeneXusCryptography.Symmetric
{
	/// <summary>
	/// Implements Symmetric block cipher engines and the methos to encrypt and decrypt strings
	/// </summary>
	[SecuritySafeCritical]
	public class SymmetricBlockCipher : SecurityAPIObject, ISymmetricBlockCipherObject
	{




		/// <summary>
		/// SymmetricBlockCipher class constructor
		/// </summary>
		public SymmetricBlockCipher() : base()
		{

		}


		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/



		/// <summary>
		/// Encrypts the given text with an AEAD encryption algorithm
		/// </summary>
		/// <param name="symmetricBlockAlgorithm">string SymmetricBlockAlgorithm enum, symmetric block algorithm name</param>
		/// <param name="symmetricBlockMode">string SymmetricBlockModes enum, symmetric block mode name</param>
		/// <param name="key">string Hexa key for the algorithm excecution</param>
		/// <param name="macSize">int macSize in bits for MAC length for AEAD Encryption algorithm</param>
		/// <param name="nonce">string Hexa nonce for MAC length for AEAD Encryption algorithm</param>
		/// <param name="plainText"> string UTF-8 plain text to encrypt</param>
		/// <returns></returns>
		public string DoAEADEncrypt(string symmetricBlockAlgorithm, string symmetricBlockMode,
		 string key, int macSize, string nonce, string plainText)
		{
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("symmetricBlockAlgorithm", symmetricBlockAlgorithm, this.error);
			SecurityUtils.validateStringInput("symmetricBlockMode", symmetricBlockMode, this.error);
			SecurityUtils.validateStringInput("key", key, this.error);
			SecurityUtils.validateStringInput("nonce", nonce, this.error);
			SecurityUtils.validateStringInput("plainText", plainText, this.error);
			if (this.HasError()) { return ""; };
			/*******INPUT VERIFICATION - END*******/

			EncodingUtil eu = new EncodingUtil();
			byte[] txtBytes = eu.getBytes(plainText);
			if (eu.HasError()) { this.error = eu.GetError(); }
			if (this.HasError()) { return ""; }

			byte[] encryptedBytes = SetUp(symmetricBlockAlgorithm, symmetricBlockMode, null, nonce, key, txtBytes, macSize, true, true, false, null, null);
			if (this.HasError()) { return ""; }

			return Base64.ToBase64String(encryptedBytes);

		}
		/// <summary>
		/// Decrypts the given encrypted text with an AEAD encryption algorithm
		/// </summary>
		/// <param name="symmetricBlockAlgorithm">string SymmetricBlockAlgorithm enum, symmetric block algorithm name</param>
		/// <param name="symmetricBlockMode">string SymmetricBlockModes enum, symmetric block mode name</param>
		/// <param name="key">string Hexa key for the algorithm excecution</param>
		/// <param name="macSize">int macSize in bits for MAC length for AEAD Encryption algorithm</param>
		/// <param name="nonce">string Hexa nonce for MAC length for AEAD Encryption algorithm</param>
		/// <param name="encryptedInput">string Base64 text to decrypt</param>
		/// <returns></returns>
		public string DoAEADDecrypt(string symmetricBlockAlgorithm, string symmetricBlockMode,
		string key, int macSize, string nonce, string encryptedInput)
		{
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("symmetricBlockAlgorithm", symmetricBlockAlgorithm, this.error);
			SecurityUtils.validateStringInput("symmetricBlockMode", symmetricBlockMode, this.error);
			SecurityUtils.validateStringInput("key", key, this.error);
			SecurityUtils.validateStringInput("nonce", nonce, this.error);
			SecurityUtils.validateStringInput("encryptedInput", encryptedInput, this.error);
			if (this.HasError()) { return ""; };
			/*******INPUT VERIFICATION - END*******/

			byte[] input = null;
			try
			{
				input = Base64.Decode(encryptedInput);
			}
			catch (Exception e)
			{
				this.error.setError("SB001", e.Message);
				return "";
			}

			byte[] decryptedBytes = SetUp(symmetricBlockAlgorithm, symmetricBlockMode, null, nonce, key, input, macSize, false, true, false, null, null);
			if (this.HasError()) { return ""; }

			EncodingUtil eu = new EncodingUtil();
			String result = eu.getString(decryptedBytes);
			if (eu.HasError())
			{
				this.error = eu.GetError();
				return "";
			}
			return result.Trim();

		}
		/// <summary>
		/// Encrypts the given text with a block encryption algorithm
		/// </summary>
		/// <param name="symmetricBlockAlgorithm">string SymmetricBlockAlgorithm enum, symmetric block algorithm name</param>
		/// <param name="symmetricBlockMode">string SymmetricBlockModes enum, symmetric block mode name</param>
		/// <param name="symmetricBlockPadding">string SymmetricBlockPadding enum, symmetric block padding name</param>
		/// <param name="key">string Hexa key for the algorithm excecution</param>
		/// <param name="IV">string IV for the algorithm execution, must be the same length as the blockSize</param>
		/// <param name="plainText">string UTF-8 plain text to encrypt</param>
		/// <returns>string base64 encrypted text</returns>
		[SecuritySafeCritical]
		public string DoEncrypt(string symmetricBlockAlgorithm, string symmetricBlockMode,
		string symmetricBlockPadding, string key, string IV, string plainText)
		{
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("symmetricBlockAlgorithm", symmetricBlockAlgorithm, this.error);
			SecurityUtils.validateStringInput("symmetricBlockMode", symmetricBlockMode, this.error);
			SecurityUtils.validateStringInput("symmetricBlockPadding", symmetricBlockPadding, this.error);
			SecurityUtils.validateStringInput("key", key, this.error);
			SecurityUtils.validateStringInput("IV", IV, this.error);
			SecurityUtils.validateStringInput("plainText", plainText, this.error);
			if (this.HasError()) { return ""; };
			/*******INPUT VERIFICATION - END*******/

			EncodingUtil eu = new EncodingUtil();
			byte[] inputBytes = eu.getBytes(plainText);
			if (eu.HasError())
			{
				this.error = eu.GetError();
				return "";
			}

			byte[] encryptedBytes = SetUp(symmetricBlockAlgorithm, symmetricBlockMode, symmetricBlockPadding, IV, key, inputBytes, 0, true, false, false, null, null);
			if (this.HasError()) { return ""; }

			return Base64.ToBase64String(encryptedBytes);
		}
		/// <summary>
		/// Decrypts the given encrypted text with a block encryption algorithm
		/// </summary>
		/// <param name="symmetricBlockAlgorithm">string SymmetricBlockAlgorithm enum, symmetric block algorithm name</param>
		/// <param name="symmetricBlockMode">string SymmetricBlockModes enum, symmetric block mode name</param>
		/// <param name="symmetricBlockPadding">string SymmetricBlockPadding enum, symmetric block padding name</param>
		/// <param name="key">string Hexa key for the algorithm excecution</param>
		/// <param name="IV">string IV for the algorithm execution, must be the same length as the blockSize</param>
		/// <param name="encryptedInput">string Base64 text to decrypt</param>
		/// <returns>sting plaintext UTF-8</returns>
		[SecuritySafeCritical]
		public string DoDecrypt(string symmetricBlockAlgorithm, string symmetricBlockMode,
		string symmetricBlockPadding, string key, string IV, string encryptedInput)
		{
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("symmetricBlockAlgorithm", symmetricBlockAlgorithm, this.error);
			SecurityUtils.validateStringInput("symmetricBlockMode", symmetricBlockMode, this.error);
			SecurityUtils.validateStringInput("symmetricBlockPadding", symmetricBlockPadding, this.error);
			SecurityUtils.validateStringInput("key", key, this.error);
			SecurityUtils.validateStringInput("IV", IV, this.error);
			SecurityUtils.validateStringInput("encryptedInput", encryptedInput, this.error);
			if (this.HasError()) { return ""; };
			/*******INPUT VERIFICATION - END*******/

			byte[] input = null;
			try
			{
				input = Base64.Decode(encryptedInput);
			}
			catch (Exception e)
			{
				this.error.setError("SB002", e.Message);
				return "";
			}

			byte[] decryptedBytes = SetUp(symmetricBlockAlgorithm, symmetricBlockMode, symmetricBlockPadding, IV, key, input, 0, false, false, false, null, null);
			if (this.HasError()) { return ""; }

			EncodingUtil eu = new EncodingUtil();
			String result = eu.getString(decryptedBytes);
			if (eu.HasError())
			{
				this.error = eu.GetError();
				return "";
			}
			return result.Trim();
		}

		[SecuritySafeCritical]
		public bool DoAEADEncryptFile(String symmetricBlockAlgorithm, String symmetricBlockMode, String key, int macSize,
			String nonce, String pathInputFile, String pathOutputFile)
		{
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("symmetricBlockAlgorithm", symmetricBlockAlgorithm, this.error);
			SecurityUtils.validateStringInput("symmetricBlockMode", symmetricBlockMode, this.error);
			SecurityUtils.validateStringInput("key", key, this.error);
			SecurityUtils.validateStringInput("nonce", nonce, this.error);
			if (this.HasError()) { return false; };
			/*******INPUT VERIFICATION - END*******/

			return SetUpFile(symmetricBlockAlgorithm, symmetricBlockMode, null, nonce, key, pathInputFile, pathOutputFile, macSize, true, true);
		}

		[SecuritySafeCritical]
		public bool DoAEADDecryptFile(String symmetricBlockAlgorithm, String symmetricBlockMode, String key, int macSize,
			String nonce, String pathInputFile, String pathOutputFile)
		{
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("symmetricBlockAlgorithm", symmetricBlockAlgorithm, this.error);
			SecurityUtils.validateStringInput("symmetricBlockMode", symmetricBlockMode, this.error);
			SecurityUtils.validateStringInput("key", key, this.error);
			SecurityUtils.validateStringInput("nonce", nonce, this.error);
			if (this.HasError()) { return false; };
			/*******INPUT VERIFICATION - END*******/

			return SetUpFile(symmetricBlockAlgorithm, symmetricBlockMode, null, nonce, key, pathInputFile, pathOutputFile, macSize, false, true);
		}

		[SecuritySafeCritical]
		public bool DoEncryptFile(String symmetricBlockAlgorithm, String symmetricBlockMode, String symmetricBlockPadding,
			String key, String IV, String pathInputFile, String pathOutputFile)
		{
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("symmetricBlockAlgorithm", symmetricBlockAlgorithm, this.error);
			SecurityUtils.validateStringInput("symmetricBlockMode", symmetricBlockMode, this.error);
			SecurityUtils.validateStringInput("symmetricBlockPadding", symmetricBlockPadding, this.error);
			SecurityUtils.validateStringInput("key", key, this.error);
			SecurityUtils.validateStringInput("IV", IV, this.error);
			if (this.HasError()) { return false; };
			/*******INPUT VERIFICATION - END*******/

			return SetUpFile(symmetricBlockAlgorithm, symmetricBlockMode, symmetricBlockPadding, IV, key, pathInputFile, pathOutputFile, 0, true, false);
		}

		[SecuritySafeCritical]
		public bool DoDecryptFile(String symmetricBlockAlgorithm, String symmetricBlockMode, String symmetricBlockPadding,
				String key, String IV, String pathInputFile, String pathOutputFile)
		{
			this.error.cleanError();
			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("symmetricBlockAlgorithm", symmetricBlockAlgorithm, this.error);
			SecurityUtils.validateStringInput("symmetricBlockMode", symmetricBlockMode, this.error);
			SecurityUtils.validateStringInput("symmetricBlockPadding", symmetricBlockPadding, this.error);
			SecurityUtils.validateStringInput("key", key, this.error);
			SecurityUtils.validateStringInput("IV", IV, this.error);
			if (this.HasError()) { return false; };
			/*******INPUT VERIFICATION - END*******/

			return SetUpFile(symmetricBlockAlgorithm, symmetricBlockMode, symmetricBlockPadding, IV, key, pathInputFile, pathOutputFile, 0, false, false);
		}


		/********EXTERNAL OBJECT PUBLIC METHODS  - END ********/


		/// <summary>
		/// Gets the BufferedBlockCipher loaded with Padding, Mode and Engine to Encrypt with a Symmetric Block Algorithm
		/// </summary>
		/// <param name="algorithm">string SymmetricBlockAlgorithm enum, symmetric block algorithm name</param>
		/// <param name="mode">string SymmetricBlockModes enum, symmetric block mode name</param>
		/// <param name="padding">string SymmetricBlockPadding enum, symmetric block padding name</param>
		/// <returns>BufferedBlockCipher loaded with Padding, Mode and Engine to Encrypt with a Symmetric Block Algorithm</returns>
		private BufferedBlockCipher getCipher(SymmetricBlockAlgorithm algorithm, SymmetricBlockMode mode,
		SymmetricBlockPadding padding)
		{
			IBlockCipher engine = getCipherEngine(algorithm);
			IBlockCipherPadding paddingCipher = getPadding(padding);
			IBlockCipher bc;
			if (mode != SymmetricBlockMode.ECB)
			{
				bc = getCipherMode(engine, mode);
			}
			else
			{
				bc = engine;
			}
			// si el padding es WITHCTS el paddingCipher es null
			if (usesCTS(mode, padding))
			{
				return new CtsBlockCipher(bc); // no usa el paddingCipher que es el null
			}
			if (padding == SymmetricBlockPadding.NOPADDING)
			{
				return new BufferedBlockCipher(bc);
			}
			else
			{
				return new PaddedBufferedBlockCipher(bc, paddingCipher);
			}

		}
		/// <summary>
		/// True if it uses CTS
		/// </summary>
		/// <param name="mode">string SymmetricBlockModes enum, symmetric block mode name</param>
		/// <param name="padding">string SymmetricBlockPadding enum, symmetric block padding name</param>
		/// <returns>boolean true if it uses CTS</returns>
		private static bool usesCTS(SymmetricBlockMode mode, SymmetricBlockPadding padding)
		{
			return mode == SymmetricBlockMode.CTS || padding == SymmetricBlockPadding.WITHCTS;
		}
		/// <summary>
		/// Build the engine
		/// </summary>
		/// <param name="algorithm">SymmetricBlockAlgorithm enum, algorithm name</param>
		/// <returns>IBlockCipher with the algorithm Engine</returns>
		internal IBlockCipher getCipherEngine(SymmetricBlockAlgorithm algorithm)
		{

			IBlockCipher engine = null;

			switch (algorithm)
			{
				case SymmetricBlockAlgorithm.AES:
					engine = new AesEngine();
					break;
				case SymmetricBlockAlgorithm.BLOWFISH:
					engine = new BlowfishEngine();
					break;
				case SymmetricBlockAlgorithm.CAMELLIA:
					engine = new CamelliaEngine();
					break;
				case SymmetricBlockAlgorithm.CAST5:
					engine = new Cast5Engine();
					break;
				case SymmetricBlockAlgorithm.CAST6:
					engine = new Cast6Engine();
					break;
				case SymmetricBlockAlgorithm.DES:
					engine = new DesEngine();
					break;
				case SymmetricBlockAlgorithm.TRIPLEDES:
					engine = new DesEdeEngine();
					break;
				case SymmetricBlockAlgorithm.DSTU7624_128:
					engine = new Dstu7624Engine(SymmetricBlockAlgorithmUtils.getBlockSize(SymmetricBlockAlgorithm.DSTU7624_128, this.error));
					break;
				case SymmetricBlockAlgorithm.DSTU7624_256:
					engine = new Dstu7624Engine(SymmetricBlockAlgorithmUtils.getBlockSize(SymmetricBlockAlgorithm.DSTU7624_256, this.error));
					break;
				case SymmetricBlockAlgorithm.DSTU7624_512:
					engine = new Dstu7624Engine(SymmetricBlockAlgorithmUtils.getBlockSize(SymmetricBlockAlgorithm.DSTU7624_512, this.error));
					break;
				case SymmetricBlockAlgorithm.GOST28147:
					engine = new Gost28147Engine();
					break;
				case SymmetricBlockAlgorithm.NOEKEON:
					engine = new NoekeonEngine();
					break;
				case SymmetricBlockAlgorithm.RC2:
					engine = new RC2Engine();
					break;
				case SymmetricBlockAlgorithm.RC532:
					engine = new RC532Engine();
					break;
				case SymmetricBlockAlgorithm.RC564:
					engine = new RC564Engine();
					break;
				case SymmetricBlockAlgorithm.RC6:
					engine = new RC6Engine();
					break;
				case SymmetricBlockAlgorithm.RIJNDAEL_128:
					engine = new RijndaelEngine(SymmetricBlockAlgorithmUtils.getBlockSize(SymmetricBlockAlgorithm.RIJNDAEL_128, this.error));
					break;
				case SymmetricBlockAlgorithm.RIJNDAEL_160:
					engine = new RijndaelEngine(SymmetricBlockAlgorithmUtils.getBlockSize(SymmetricBlockAlgorithm.RIJNDAEL_160, this.error));
					break;
				case SymmetricBlockAlgorithm.RIJNDAEL_192:
					engine = new RijndaelEngine(SymmetricBlockAlgorithmUtils.getBlockSize(SymmetricBlockAlgorithm.RIJNDAEL_192, this.error));
					break;
				case SymmetricBlockAlgorithm.RIJNDAEL_224:
					engine = new RijndaelEngine(SymmetricBlockAlgorithmUtils.getBlockSize(SymmetricBlockAlgorithm.RIJNDAEL_224, this.error));
					break;
				case SymmetricBlockAlgorithm.RIJNDAEL_256:
					engine = new RijndaelEngine(SymmetricBlockAlgorithmUtils.getBlockSize(SymmetricBlockAlgorithm.RIJNDAEL_256, this.error));
					break;
				case SymmetricBlockAlgorithm.SEED:
					engine = new SeedEngine();
					break;
				case SymmetricBlockAlgorithm.SERPENT:
					engine = new SerpentEngine();
					break;
				case SymmetricBlockAlgorithm.SKIPJACK:
					engine = new SkipjackEngine();
					break;
				case SymmetricBlockAlgorithm.SM4:
					engine = new SM4Engine();
					break;
				case SymmetricBlockAlgorithm.TEA:
					engine = new TeaEngine();
					break;
				case SymmetricBlockAlgorithm.THREEFISH_256:
					engine = new ThreefishEngine(SymmetricBlockAlgorithmUtils.getBlockSize(SymmetricBlockAlgorithm.THREEFISH_256, this.error));
					break;
				case SymmetricBlockAlgorithm.THREEFISH_512:
					engine = new ThreefishEngine(SymmetricBlockAlgorithmUtils.getBlockSize(SymmetricBlockAlgorithm.THREEFISH_512, this.error));
					break;
				case SymmetricBlockAlgorithm.THREEFISH_1024:
					engine = new ThreefishEngine(SymmetricBlockAlgorithmUtils.getBlockSize(SymmetricBlockAlgorithm.THREEFISH_1024, this.error));
					break;
				case SymmetricBlockAlgorithm.TWOFISH:
					engine = new TwofishEngine();
					break;
				case SymmetricBlockAlgorithm.XTEA:
					engine = new XteaEngine();
					break;
				default:
					this.error.setError("SB003", "Unrecognized symmetric block algoritm");
					break;
			}
			return engine;

		}
		/// <summary>
		/// Builds an IBlockCipherPadding
		/// </summary>
		/// <param name="padding">SymmetricBlockPadding enum, padding name</param>
		/// <returns>IBlockCipherPadding with loaded padding type, if padding is WITHCTS returns null</returns>
		private IBlockCipherPadding getPadding(SymmetricBlockPadding padding)
		{

			IBlockCipherPadding paddingCipher = null;

			switch (padding)
			{
				case SymmetricBlockPadding.NOPADDING:
					paddingCipher = null;
					break;
				case SymmetricBlockPadding.ISO7816D4PADDING:
					paddingCipher = new ISO7816d4Padding();
					break;
				case SymmetricBlockPadding.ISO10126D2PADDING:
					paddingCipher = new ISO10126d2Padding();
					break;
				case SymmetricBlockPadding.PKCS7PADDING:
					paddingCipher = new Pkcs7Padding();
					break;
				case SymmetricBlockPadding.WITHCTS:
					break;
				case SymmetricBlockPadding.X923PADDING:
					paddingCipher = new X923Padding();
					break;
				case SymmetricBlockPadding.ZEROBYTEPADDING:
					paddingCipher = new ZeroBytePadding();
					break;
				default:
					this.error.setError("SB004", "Unrecognized symmetric block padding.");
					break;
			}
			return paddingCipher;
		}
		/// <summary>
		/// Buils an AEADBlockCipher engine
		/// </summary>
		/// <param name="blockCipher">BlockCipher engine</param>
		/// <param name="mode">SymmetricBlockModes enum, symmetric block mode name</param>
		/// <returns>AEADBlockCipher loaded with a given BlockCipher</returns>
		private IAeadBlockCipher getAEADCipherMode(IBlockCipher blockCipher, SymmetricBlockMode mode)
		{

			IAeadBlockCipher bc = null;

			switch (mode)
			{
				case SymmetricBlockMode.AEAD_CCM:
					bc = new CcmBlockCipher(blockCipher);
					break;
				case SymmetricBlockMode.AEAD_EAX:
					bc = new EaxBlockCipher(blockCipher);
					break;
				case SymmetricBlockMode.AEAD_GCM:
					bc = new GcmBlockCipher(blockCipher);
					break;
				case SymmetricBlockMode.AEAD_KCCM:
					bc = new KCcmBlockCipher(blockCipher);
					break;
				default:
					this.error.setError("SB005", "Unrecognized symmetric AEAD mode");
					break;
			}
			return bc;

		}
		/// <summary>
		/// Buisl a BlockCipher with a mode
		/// </summary>
		/// <param name="blockCipher">BlockCipher loaded with the algorithm Engine</param>
		/// <param name="mode">SymmetricBlockModes enum, mode name</param>
		/// <returns>BlockCipher with mode loaded</returns>
		private IBlockCipher getCipherMode(IBlockCipher blockCipher, SymmetricBlockMode mode)
		{

			IBlockCipher bc = null;

			switch (mode)
			{
				case SymmetricBlockMode.ECB:
				case SymmetricBlockMode.NONE:
					bc = blockCipher;
					break;
				case SymmetricBlockMode.CBC:
					bc = new CbcBlockCipher(blockCipher);
					break;
				case SymmetricBlockMode.CFB:
					bc = new CfbBlockCipher(blockCipher, blockCipher.GetBlockSize());
					break;
				case SymmetricBlockMode.CTR:
					bc = new SicBlockCipher(blockCipher);
					break;
				case SymmetricBlockMode.CTS:
					bc = new CbcBlockCipher(blockCipher);
					break;
				case SymmetricBlockMode.GOFB:
					bc = new GOfbBlockCipher(blockCipher);
					break;
				case SymmetricBlockMode.OFB:
					bc = new OfbBlockCipher(blockCipher, blockCipher.GetBlockSize());
					break;
				case SymmetricBlockMode.OPENPGPCFB:
					bc = new OpenPgpCfbBlockCipher(blockCipher);
					break;
				case SymmetricBlockMode.SIC:
					blockCipher = new SicBlockCipher(blockCipher);
					break;

				default:
					this.error.setError("SB006", "Unrecognized symmetric block mode");
					break;
			}
			return bc;
		}

		private byte[] SetUp(string symmetricBlockAlgorithm, string symmetricBlockMode, string symmetricBlockPadding, string nonce, string key, byte[] input, int macSize, bool toEncrypt, bool isAEAD, bool isFile, string pathInput, string pathOutput)
		{
			SymmetricBlockAlgorithm algorithm = SymmetricBlockAlgorithmUtils.getSymmetricBlockAlgorithm(symmetricBlockAlgorithm,
					this.error);
			SymmetricBlockMode mode = SymmetricBlockModeUtils.getSymmetricBlockMode(symmetricBlockMode, this.error);
			SymmetricBlockPadding padding = SymmetricBlockPadding.NOPADDING;
			if (!isAEAD)
			{
				padding = SymmetricBlockPaddingUtils.getSymmetricBlockPadding(symmetricBlockPadding,
					   this.error);
			}

			byte[] nonceBytes = SecurityUtils.HexaToByte(nonce, this.error);
			byte[] keyBytes = SecurityUtils.HexaToByte(key, this.error);

			if (this.HasError()) { return null; }

			return isAEAD ? encryptAEAD(algorithm, mode, keyBytes, nonceBytes, input, macSize, toEncrypt, isFile, pathInput, pathOutput) : encrypt(algorithm, mode, padding, keyBytes, nonceBytes, input, toEncrypt, isFile, pathInput, pathOutput);

		}


		private byte[] encryptAEAD(SymmetricBlockAlgorithm algorithm, SymmetricBlockMode mode, byte[] key, byte[] nonce, byte[] txt, int macSize, bool toEncrypt, bool isFile, string pathInput, string pathOutput)
		{
			IBlockCipher engine = getCipherEngine(algorithm);
			IAeadBlockCipher bbc = getAEADCipherMode(engine, mode);
			if (this.HasError()) { return null; }

			KeyParameter keyParam = new KeyParameter(key);
			AeadParameters AEADparams = new AeadParameters(keyParam, macSize, nonce);

			try
			{
				bbc.Init(toEncrypt, AEADparams);
			}
			catch (Exception e)
			{
				this.error.setError("SB007", e.Message);
				return null;
			}
			byte[] outputBytes = null;
			if (isFile)
			{
				try
				{
					byte[] inBuffer = new byte[1024];
					byte[] outBuffer = new byte[bbc.GetOutputSize(1024)];
					outBuffer = new byte[bbc.GetBlockSize() + bbc.GetOutputSize(inBuffer.Length)];
					int inCount = 0;
					int outCount = 0;
					using (FileStream inputStream = new FileStream(pathInput, FileMode.Open, FileAccess.Read))
					{
						using (FileStream outputStream = new FileStream(pathOutput, FileMode.Create, FileAccess.Write))
						{
							while ((inCount = inputStream.Read(inBuffer, 0, inBuffer.Length)) > 0)
							{
								outCount = bbc.ProcessBytes(inBuffer, 0, inCount, outBuffer, 0);
								outputStream.Write(outBuffer, 0, outCount);
							}
							outCount = bbc.DoFinal(outBuffer, 0);

							outputStream.Write(outBuffer, 0, outCount);
						}
					}
				}
				catch (Exception e)
				{
					this.error.setError("SB011", e.Message);
					return null;
				}
				outputBytes = new byte[1];
			}
			else
			{
				outputBytes = new byte[bbc.GetOutputSize(txt.Length)];
				try
				{

					int length = bbc.ProcessBytes(txt, 0, txt.Length, outputBytes, 0);
					bbc.DoFinal(outputBytes, length);
				}
				catch (Exception e)
				{
					this.error.setError("SB008", e.Message);
					return null;
				}

			}
			return outputBytes;

		}



		private byte[] encrypt(SymmetricBlockAlgorithm algorithm, SymmetricBlockMode mode, SymmetricBlockPadding padding, byte[] key, byte[] iv, byte[] input, bool toEncrypt, bool isFile, string pathInput, string pathOutput)
		{

			BufferedBlockCipher bbc = getCipher(algorithm, mode, padding);
			KeyParameter keyParam = new KeyParameter(key);
			if (this.HasError()) { return null; }

			try
			{
				if (SymmetricBlockMode.ECB != mode && SymmetricBlockMode.OPENPGPCFB != mode)
				{
					ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, iv);
					bbc.Init(toEncrypt, keyParamWithIV);
				}
				else
				{
					bbc.Init(toEncrypt, keyParam);
				}
			}
			catch (Exception e)
			{
				this.error.setError("SB009", e.Message);
				return null;
			}
			byte[] outputBytes = null;
			if (isFile)
			{
				try
				{
					byte[] inBuffer = new byte[1024];
					byte[] outBuffer = new byte[bbc.GetOutputSize(1024)];
					outBuffer = new byte[bbc.GetBlockSize() + bbc.GetOutputSize(inBuffer.Length)];
					int inCount = 0;
					int outCount = 0;
					using (FileStream inputStream = new FileStream(pathInput, FileMode.Open, FileAccess.Read))
					{
						using (FileStream outputStream = new FileStream(pathOutput, FileMode.Create, FileAccess.Write))
						{
							while ((inCount = inputStream.Read(inBuffer, 0, inBuffer.Length)) > 0)
							{
								outCount = bbc.ProcessBytes(inBuffer, 0, inCount, outBuffer, 0);
								outputStream.Write(outBuffer, 0, outCount);
							}
							outCount = bbc.DoFinal(outBuffer, 0);

							outputStream.Write(outBuffer, 0, outCount);
						}
					}
				}
				catch (Exception e)
				{
					this.error.setError("SB012", e.Message);
					return null;
				}
				outputBytes = new byte[1];

			}
			else
			{
				outputBytes = new byte[bbc.GetOutputSize(input.Length)];
				try
				{

					int length = bbc.ProcessBytes(input, 0, input.Length, outputBytes, 0);
					int length2 = bbc.DoFinal(outputBytes, length);

				}
				catch (Exception e)
				{
					this.error.setError("SB010", e.Message);
					return null;
				}
			}
			return outputBytes;
		}



		private bool SetUpFile(string symmetricBlockAlgorithm, string symmetricBlockMode, string symmetricBlockPadding, string nonce, string key, string pathInput, string pathOutput, int macSize, bool toEncrypt, bool isAEAD)
		{
			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("pathInputFile", pathInput, this.error);
			SecurityUtils.validateStringInput("pathOutputFile", pathOutput, this.error);
			if (this.HasError()) { return false; };
			/*******INPUT VERIFICATION - END*******/
			byte[] output = SetUp(symmetricBlockAlgorithm, symmetricBlockMode, symmetricBlockPadding, nonce, key, null, macSize, toEncrypt, isAEAD, true, pathInput, pathOutput);
			return output == null ? false : true;
		}
	}
}
