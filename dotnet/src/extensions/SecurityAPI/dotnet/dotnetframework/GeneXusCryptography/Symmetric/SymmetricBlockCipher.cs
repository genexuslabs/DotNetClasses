
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
            SymmetricBlockAlgorithm algorithm = SymmetricBlockAlgorithmUtils.getSymmetricBlockAlgorithm(symmetricBlockAlgorithm, this.error);
            SymmetricBlockMode mode = SymmetricBlockModeUtils.getSymmetricBlockMode(symmetricBlockMode, this.error);
            if (this.error.existsError())
            {
                return "";
            }

            IBlockCipher engine = getCipherEngine(algorithm);
            IAeadBlockCipher bbc = getAEADCipherMode(engine, mode);
            if (this.error.existsError() && !(string.Compare(this.error.Code, "SB016", true) == 0))
            {
                return "";
            }
			byte[] nonceBytes = SecurityUtils.GetHexa(nonce, "SB024", this.error);
			byte[] keyBytes = SecurityUtils.GetHexa(key, "SB024", this.error);
			if(this.HasError())
			{
				return "";
			}

			KeyParameter keyParam = new KeyParameter(keyBytes);
            
            AeadParameters AEADparams = new AeadParameters(keyParam, macSize, nonceBytes);
			try
			{
				bbc.Init(true, AEADparams);
			}catch(Exception e)
			{
				this.error.setError("SB029", e.Message);
				return "";
			}
            EncodingUtil eu = new EncodingUtil();
            byte[] inputBytes = eu.getBytes(plainText);
            if (eu.GetError().existsError())
            {
                this.error = eu.GetError();
                return "";
            }
            byte[] outputBytes = new byte[bbc.GetOutputSize(inputBytes.Length)];
            int length = bbc.ProcessBytes(inputBytes, 0, inputBytes.Length, outputBytes, 0);
            try
            {
                bbc.DoFinal(outputBytes, length);
            }
            catch (Exception)
            {
                this.error.setError("SB010", "AEAD encryption exception");
                return "";

            }
            string result = Base64.ToBase64String(outputBytes);
            if (result == null || result.Length == 0)
            {
                this.error.setError("SB011", "Error encoding base64");
                return "";
            }
            this.error.cleanError();
            return result;
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
            SymmetricBlockAlgorithm algorithm = SymmetricBlockAlgorithmUtils.getSymmetricBlockAlgorithm(symmetricBlockAlgorithm, this.error);
            SymmetricBlockMode mode = SymmetricBlockModeUtils.getSymmetricBlockMode(symmetricBlockMode, this.error);
            if (this.error.existsError())
            {
                return "";
            }

            IBlockCipher engine = getCipherEngine(algorithm);
            IAeadBlockCipher bbc = getAEADCipherMode(engine, mode);
            if (this.error.existsError() && !(string.Compare(this.error.Code, "SB016", true) == 0))
            {
                return "";
            }
			byte[] nonceBytes = SecurityUtils.GetHexa(nonce, "SB025", this.error);
			byte[] keyBytes = SecurityUtils.GetHexa(key, "SB025", this.error);
			if(this.HasError())
			{
				return "";
			}
			KeyParameter keyParam = new KeyParameter(keyBytes);
            
            AeadParameters AEADparams = new AeadParameters(keyParam, macSize, nonceBytes);
			try
			{
				bbc.Init(false, AEADparams);
			}catch(Exception e)
			{
				this.error.setError("SB030", e.Message);
				return "";
			}
            byte[] out2 = Base64.Decode(encryptedInput);
            byte[] comparisonBytes = new byte[bbc.GetOutputSize(out2.Length)];
            int length = bbc.ProcessBytes(out2, 0, out2.Length, comparisonBytes, 0);
            try
            {
                bbc.DoFinal(comparisonBytes, length);
            }
            catch (Exception)
            {
                this.error.setError("SB012", "AEAD decryption exception");
                return "";
            }
            this.error.cleanError();
            // return System.Text.Encoding.UTF8.GetString(comparisonBytes).Trim();
            EncodingUtil eu = new EncodingUtil();
            this.error = eu.GetError();

            return eu.getString(comparisonBytes);

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
            SymmetricBlockAlgorithm algorithm = SymmetricBlockAlgorithmUtils.getSymmetricBlockAlgorithm(symmetricBlockAlgorithm, this.error);
            SymmetricBlockMode mode = SymmetricBlockModeUtils.getSymmetricBlockMode(symmetricBlockMode, this.error);
            SymmetricBlockPadding padding = SymmetricBlockPaddingUtils.getSymmetricBlockPadding(symmetricBlockPadding, this.error);
            if (this.error.existsError())
            {
                return "";
            }

            BufferedBlockCipher bbc = getCipher(algorithm, mode, padding);
            if (this.error.existsError() && !(string.Compare(this.error.Code, "SB016", true) == 0))
            {
                return "";
            }
			byte[] byteIV = SecurityUtils.GetHexa(IV, "SB022", this.error);
			byte[] byteKey = SecurityUtils.GetHexa(key, "SB022", this.error);
			if (this.HasError())
			{
				return "";
			}
			KeyParameter keyParam = new KeyParameter(byteKey);

            if (SymmetricBlockMode.ECB != mode && SymmetricBlockMode.OPENPGPCFB != mode)
            {
                ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, byteIV);
				try{
					bbc.Init(true, keyParamWithIV);
				}catch(Exception e)
				{
					this.error.setError("SB025", e.Message);
					return "";
				}
            }
            else
            {
				try
				{
					bbc.Init(true, keyParam);
				}catch(Exception e)
				{
					this.error.setError("SB026", e.Message);
					return "";
				}
            }

            EncodingUtil eu = new EncodingUtil();
            byte[] inputBytes = eu.getBytes(plainText);
            if (eu.GetError().existsError())
            {
                this.error = eu.GetError();
                return "";
            }
            byte[] outputBytes = new byte[bbc.GetOutputSize(inputBytes.Length)];
            int length = bbc.ProcessBytes(inputBytes, 0, inputBytes.Length, outputBytes, 0);
            try
            {
                bbc.DoFinal(outputBytes, length);
            }
            catch (Exception)
            {
                this.error.setError("SB013", "Block encryption exception");
                return "";
            }
            string result = Base64.ToBase64String(outputBytes);
            if (result == null || result.Length == 0)
            {
                this.error.setError("SB014", "Error encoding base64");
                return "";
            }
            this.error.cleanError();
            return result;
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
            SymmetricBlockAlgorithm algorithm = SymmetricBlockAlgorithmUtils.getSymmetricBlockAlgorithm(symmetricBlockAlgorithm, this.error);
            SymmetricBlockMode mode = SymmetricBlockModeUtils.getSymmetricBlockMode(symmetricBlockMode, this.error);
            SymmetricBlockPadding padding = SymmetricBlockPaddingUtils.getSymmetricBlockPadding(symmetricBlockPadding, this.error);
            if (this.error.existsError())
            {
                return "";
            }

            BufferedBlockCipher bbc = getCipher(algorithm, mode, padding);
            if (this.error.existsError() && !(string.Compare(this.error.Code, "SB016", true) == 0))
            {
                return "";
            }
			byte[] bytesKey = SecurityUtils.GetHexa(key, "SB023", this.error);
			byte[] bytesIV = SecurityUtils.GetHexa(IV, "SB023", this.error);
			if (this.HasError())
			{
				return "";
			}

			KeyParameter keyParam = new KeyParameter(bytesKey);
            if (SymmetricBlockMode.ECB != mode && SymmetricBlockMode.OPENPGPCFB != mode)
            {
                ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, bytesIV);
				try
				{
					bbc.Init(false, keyParamWithIV);
				}catch(Exception e)
				{
					this.error.setError("SB027", e.Message);
					return "";
				}
            }
            else
            {
				try
				{
					bbc.Init(false, keyParam);
				}catch(Exception e)
				{
					this.error.setError("SB028", e.Message);
					return "";
				}
            }

            byte[] out2 = Base64.Decode(encryptedInput);
            byte[] comparisonBytes = new byte[bbc.GetOutputSize(out2.Length)];
            int length = bbc.ProcessBytes(out2, 0, out2.Length, comparisonBytes, 0);
            try
            {
                bbc.DoFinal(comparisonBytes, length);
            }
            catch (Exception)
            {
                this.error.setError("SB015", "Block decryption exception");
                return "";
            }
            this.error.cleanError();

            EncodingUtil eu = new EncodingUtil();
            this.error = eu.GetError();
            return eu.getString(comparisonBytes);
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
        private bool usesCTS(SymmetricBlockMode mode, SymmetricBlockPadding padding)
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
                    this.error.setError("SB020", "Cipher " + algorithm + " not recognised.");
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
                    this.error.setError("SB018", "Cipher " + padding + " not recognised.");
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
                    this.error.setError("SB017", "AEADCipher " + mode + " not recognised.");
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
                    if (blockCipher.GetBlockSize() < 16)
                    {
                        this.error.setError("SB016",
                        "Warning: SIC-Mode can become a twotime-pad if the blocksize of the cipher is too small. Use a cipher with a block size of at least 128 bits (e.g. AES)");
                    }
                    blockCipher = new SicBlockCipher(blockCipher);
                    break;
            }
            return bc;
        }

    }
}
