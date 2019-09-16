using System;
using System.Security.Cryptography;
using FreecraftCore.Crypto;

namespace Pomelo.WoW.SRP6
{
	/// <summary>
	/// Provide helper methods for <see cref="BigInteger"/>. This class is static.
	/// </summary>
	public static class BigIntegerExtensions
	{
		/// <summary>
		/// Generate a <see cref="BigInteger"/> of specified byte length with random bits. Uses a new instance of <see cref="Random"/> to generate bytes.
		/// </summary>
		/// <param name="byteCount">The count of bytes which the new <see cref="BigInteger"/> should be.</param>
		/// <returns>The created <see cref="BigInteger"/>.</returns>
		public static BigInteger GenerateRandom(int byteCount) => GenerateRandom(byteCount, new Random());

		/// <summary>
		/// Generate a <see cref="BigInteger"/> of specified byte length with random bits. 
		/// </summary>
		/// <param name="byteCount">The count of bytes which the new <see cref="BigInteger"/> should be.</param>
		/// <param name="random">A <see cref="random"/> instance used to generate random bytes.</param>
		/// <returns>The created <see cref="BigInteger"/>.</returns>
		public static BigInteger GenerateRandom(int byteCount, Random random)
		{
			if (random == null) throw new ArgumentNullException(nameof(random));

			if (byteCount < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(byteCount), byteCount,
					"The byte count cannot be negative.");
			}

			var buffer = new byte[byteCount];
			random.NextBytes(buffer);

			return buffer.ToBigInteger();
		}

		/// <summary>
		/// Generate a <see cref="BigInteger"/> of specified byte length with random bits. 
		/// </summary>
		/// <param name="byteCount">The count of bytes which the new <see cref="BigInteger"/> should be.</param>
		/// <param name="randomNumberGenerator">A <see cref="RandomNumberGenerator"/> instance used to generate random bytes.</param>
		/// <returns>The created <see cref="BigInteger"/>.</returns>
		public static BigInteger GenerateRandom(int byteCount, RandomNumberGenerator randomNumberGenerator)
		{
			if (randomNumberGenerator == null) throw new ArgumentNullException(nameof(randomNumberGenerator));

			if (byteCount < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(byteCount), byteCount,
					"The byte count cannot be negative.");
			}

			var buffer = new byte[byteCount];
			randomNumberGenerator.GetBytes(buffer);

			return buffer.ToBigInteger();
		}
	}
}