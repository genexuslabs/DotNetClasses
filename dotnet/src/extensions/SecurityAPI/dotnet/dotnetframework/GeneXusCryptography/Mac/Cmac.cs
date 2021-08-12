using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using SecurityAPICommons.Commons;
using GeneXusCryptography.Commons;
using GeneXusCryptography.SymmetricUtils;
using GeneXusCryptography.Symmetric;
using Org.BouncyCastle.Crypto;
using SecurityAPICommons.Utils;
using Org.BouncyCastle.Utilities.Encoders;
using SecurityAPICommons.Config;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Macs;

namespace GeneXusCryptography.Mac
{
    [SecuritySafeCritical]
    public class Cmac : SecurityAPIObject, ICmacObject
    {
        public Cmac() : base()
        {

        }

        /********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/


        [SecuritySafeCritical]
        public string calculate(string plainText, string key, string algorithm, int macSize)
        {
            if (!isValidAlgorithm(algorithm))
            {
                this.error.setError("CM001", "Invalid Symmetric block algorithm for CMAC");
                return "";
            }
            SymmetricBlockAlgorithm symmetricBlockAlgorithm = SymmetricBlockAlgorithmUtils.getSymmetricBlockAlgorithm(algorithm,
                    this.error);
            SymmetricBlockCipher symCipher = new SymmetricBlockCipher();
            IBlockCipher blockCipher = symCipher.getCipherEngine(symmetricBlockAlgorithm);
            if (symCipher.HasError())
            {
                this.error = symCipher.GetError();
                return "";
            }
            if (macSize > blockCipher.GetBlockSize() * 8)
            {
                this.error.setError("CM002", "The mac length must be less or equal than the algorithm block size.");
                return "";
            }
			byte[] byteKey = SecurityUtils.GetHexa(key, "CM003", this.error);
			if (this.HasError())
			{
				return "";
			}
			
            EncodingUtil eu = new EncodingUtil();
            byte[] byteInput = eu.getBytes(plainText);

            ICipherParameters parms = new KeyParameter(byteKey);

            CMac mac = null;
            if (macSize != 0)
            {
                mac = new CMac(blockCipher, macSize);
            }
            else
            {
                mac = new CMac(blockCipher);
            }
			try
			{
				mac.Init(parms);
			}catch(Exception e)
			{
				this.error.setError("CM004", e.Message);
				return "";
			}
            byte[] resBytes = new byte[mac.GetMacSize()];
            mac.BlockUpdate(byteInput, 0, byteInput.Length);
            mac.DoFinal(resBytes, 0);
            string result = toHexastring(resBytes);
            if (!this.error.existsError())
            {
                return result;
            }
            return "";

        }


        [SecuritySafeCritical]
        public bool verify(string plainText, string key, string mac, string algorithm, int macSize)
        {
            string res = calculate(plainText, key, algorithm, macSize);
            return SecurityUtils.compareStrings(res, mac);
        }


        /********EXTERNAL OBJECT PUBLIC METHODS  - END ********/

        /// <summary>
        /// Gets the hexadecimal encoded representation of the digest input
        /// </summary>
        /// <param name="digest">byte array</param>
        /// <returns>string Hexa respresentation of the byte array digest</returns>
        private string toHexastring(byte[] digest)
        {
            string result = BitConverter.ToString(digest).Replace("-", string.Empty);
            if (result == null || result.Length == 0)
            {
                this.error.setError("HS001", "Error encoding hexa");
                return "";
            }
            return result;
        }

        private bool isValidAlgorithm(string algorithm)
        {
            SymmetricBlockAlgorithm symmetricBlockAlgorithm = SymmetricBlockAlgorithmUtils.getSymmetricBlockAlgorithm(algorithm,
                    this.error);
            int blockSize = SymmetricBlockAlgorithmUtils.getBlockSize(symmetricBlockAlgorithm, this.error);
            if (this.HasError())
            {

                return false;
            }
            if (blockSize != 64 && blockSize != 128)
            {

                return false;
            }

            return true;
        }
    }
}
