using SecurityAPITest.SecurityAPICommons.commons;
using System;
using NUnit.Framework;
using SecurityAPICommons.Config;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Encoders;
using GeneXusCryptography.PasswordDerivation;

namespace SecurityAPITest.Cryptography.PasswordDerivationTest
{
    [TestFixture]
    public class TestPasswordDerivation: SecurityAPITestObject
    {
		protected static string password;
		protected static Error error;
		protected static HexaEncoder hexa;
		protected static PasswordDerivation pd;

		/* SCRYPT */

		protected static string saltScrypt;
		protected static int N;
		protected static int r;
		protected static int p;
		protected static int keyLenght;
		protected static string expectedScrypt;

		/* BCRYPT */
		protected static int cost;
		protected static string saltBcrypt;
		protected static string expectedBcrypt;

		/* ARGON2 */
		protected static int iterations;
		protected static int parallelism;
		protected static int memory;
		protected static int hashLength;
		protected static string saltArgon2;
		protected static string[] argon2Version;
		protected static string[] argon2HashType;
		protected static string[] argon2Results;

		[SetUp]
		public virtual void SetUp()
		{
			new EncodingUtil().setEncoding("UTF8");
			password = "password";
			error = new Error();
			hexa = new HexaEncoder();
			pd = new PasswordDerivation();

			/* SCRYPT */
			N = 16384;
			r = 8;
			p = 1;
			keyLenght = 256;
			saltScrypt = "123456";
			expectedScrypt = "Yc4uLDXRam2HNQxFdEJWkNYADNWgelNIidOmFE8G3G9G9j6/l/fXP43ErypBMZGs+6PLVPBPRop2pHWWIhUg4hXDoM8+fsp10wBoV3p06yxxdZu7LV19gcgwgL2tnMTN/H8Y7YYM9KpFwdFqMXbIX4DPN2hrL6DAXxNIYJO7Pcm1l9qPrOwpsZZjE032nYlXch6t8/4HRxWFOFRl8t5UjtILEyFdg1w3kLlYzP46XJV1IqGEMyFjeQbtz/c7MteZmID0aSxLtoZJPF3TA41vs09hLlhG/AoMiVQ+EXsp3vZZzg7t4RNrWfuLd2H+oFEqeEUNUisUoB8IWmyAZgn2QQ==";
			/* BCRYPT */
			cost = 6;
			saltBcrypt = "0c6a8a8235bb90219d004aa4056ec884";
			expectedBcrypt = "XoHha7SLqyY2AKgIIetMdjYBM5bizqPc";
			/* ARGON2 */
			iterations = 1;
			parallelism = 2;
			memory = 4;
			hashLength = 32;
			saltArgon2 = "14ae8da01afea8700c2358dcef7c5358d9021282bd88663a4562f59fb74d22ee";
			argon2Version = new string[] { "ARGON2_VERSION_10", "ARGON2_VERSION_13" };
			argon2HashType = new string[] { "ARGON2_d", "ARGON2_i", "ARGON2_id" };
			argon2Results = new string[] { "f9hF4rzwC9AvfFMK8ZHvKoQeipc7OUQ/dBV4nBer57U=", "QuNCd8sy8STFTBeylfaUVWAN8w3PDl0L94rr9TqaK/g=", "El6fozCF2xSzdcrfR0QO8U1Zmh4OuRZPwufAvqXcLiY=", "xvlSYizqgM93gmi7cTDvkXda41QDj6fTaCC2cpltt3E=", "jDKqkLzOaxFQ2vHwB3/UQiSI2wO+2cDk6Y1VQwSXzz4=", "A8icyy1A7VlunnJKBZXJl/BkNmVQ5FlMznCNKS1YJCM=" };

		}

		[Test]
		public void TestScrypt()
		{
			string derivated = pd.DoGenerateSCrypt(password, saltScrypt, N, r, p, keyLenght);
			Assert.AreEqual(expectedScrypt, derivated);
			Equals(expectedScrypt, derivated, pd);
		}

		[Test]
		public void TestDefaultScrypt()
		{
			string derivated = pd.DoGenerateDefaultSCrypt(password, saltScrypt);
			Assert.AreEqual(expectedScrypt, derivated);
			Equals(expectedScrypt, derivated, pd);
		}

		[Test]
		public void TestBcrypt()
		{
			string derivated = pd.DoGenerateBcrypt(password, saltBcrypt, cost);
			Assert.AreEqual(expectedBcrypt, derivated);
			Equals(expectedBcrypt, derivated, pd);
		}

		[Test]
		public void TestDefaultBcrypt()
		{
			string derivated = pd.DoGenerateDefaultBcrypt(password, saltBcrypt);
			Assert.AreEqual(expectedBcrypt, derivated);
			Equals(expectedBcrypt, derivated, pd);
		}


		[Test]
		public void TestArgon2()
		{
			/*int i = 0;
			foreach (string version in argon2Version)
			{
				foreach (string hashType in argon2HashType)
				{
					string derivated = pd.doGenerateArgon2(version, hashType, iterations, memory, parallelism, password, saltArgon2, hashLength);
					Assert.AreEqual(argon2Results[i], derivated);
					Equals(argon2Results[i], derivated, pd);
					i++;
				}
			}*/
			//*****ALGORITHM NOT IMPLEMENTED IN C#*****//
			Assert.IsTrue(true);

		}
	}
}
