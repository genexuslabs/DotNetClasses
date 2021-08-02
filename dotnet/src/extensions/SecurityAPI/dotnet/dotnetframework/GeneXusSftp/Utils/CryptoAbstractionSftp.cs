﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Sftp.GeneXusSftpUtils
{
    [SecuritySafeCritical]
    internal class CryptoAbstractionSftp
    {
        /// <summary>
        /// Generates a <see cref="Byte"/> array of the specified length, and fills it with a
        /// cryptographically strong random sequence of values.
        /// </summary>
        /// <param name="length">The length of the array generate.</param>
        public static byte[] GenerateRandom(int length)
        {
            var random = new byte[length];
            GenerateRandom(random);
            return random;
        }

        /// <summary>
        /// Fills an array of bytes with a cryptographically strong random sequence of values.
        /// </summary>
        /// <param name="data">The array to fill with cryptographically strong random bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        /// <remarks>
        /// The length of the byte array determines how many random bytes are produced.
        /// </remarks>
        public static void GenerateRandom(byte[] data)
        {
#if FEATURE_RNG_CREATE || FEATURE_RNG_CSP
            Randomizer.GetBytes(data);
#else
            if (data == null)
                throw new ArgumentNullException("data");

            System.Security.Cryptography.RNGCryptoServiceProvider Crypto = new System.Security.Cryptography.RNGCryptoServiceProvider();
            byte[] Buffer = new byte[256];
            Crypto.GetBytes(Buffer);
            /*var buffer = Windows.Security.Cryptography.CryptographicBuffer.GenerateRandom((uint)data.Length);
            System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.CopyTo(buffer, data);*/
#endif
        }
    }
}
