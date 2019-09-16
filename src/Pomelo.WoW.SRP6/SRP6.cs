using System;
using System.IO;
using System.Security.Cryptography;
using FreecraftCore.Crypto;

namespace Pomelo.WoW.SRP6
{
	/// <summary>
	/// Provide implementation for SRP6 protocol used by Blizzard.com.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public class SRP6
	{
		/// <summary>
		/// length of salt.
		/// </summary>
		public const int SaltByteSize = 32;

		/// <summary>
		/// Initializes SRP with a predefined prime (N) and generator module (g)
		/// </summary>
		public SRP6()
		{
			N = "894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7".ToBigInteger();
			g = 7;
		}

		/// <summary>
		/// calculates the host public ephemeral (B),  generates also a random number as host private ephemeral (b)
		/// </summary>
		public void CalculateHostPublicEphemeral()
		{
			b = BigIntegerExtensions.GenerateRandom(19);
			var gMod = g.ModPow(b, N);
			B = ((v * 3) + gMod) % N;
		}

		/// <summary>
		/// calculates proof (M) of the strong session key (K).
		/// </summary>
		/// <param name="username">the unique identity of the account to authenticate.</param>
		public void CalculateProof(string username)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// calculates a session key (S) based on client public ephemeral (A), safeguard conditions are (A != 0) and (A % N != 0)
		/// </summary>
		/// <param name="ipA">the client public ephemeral (A)</param>
		/// <param name="l">the length of client public ephemeral (A)</param>
		/// <returns>true on valid safeguard conditions otherwise false</returns>
		public bool CalculateSessionKey(string ipA, int l)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// calculates the password verifier (v)
		/// </summary>
		/// <param name="ri">a sha1 hash of USERNAME:PASSWORD</param>
		/// <returns>true on success otherwise false if s is faulty</returns>
		public bool CalculateVerifier(string ri)
		{
			var salt = BigIntegerExtensions.GenerateRandom(SaltByteSize);
			var saltStr = salt.ToHexString();
			return CalculateVerifier(ri, saltStr);
		}

		/// <summary>
		/// calculates the password verifier (v) based on a predefined salt (s)
		/// </summary>
		/// <param name="ri">a sha1 hash of USERNAME:PASSWORD</param>
		/// <param name="salt">a predefined salt (s)</param>
		/// <returns>true on success otherwise false if s is faulty</returns>
		public bool CalculateVerifier(string ri, string salt)
		{
			s = salt.ToBigInteger();

			if (s == BigInteger.Zero)
			{
				return false;
			}

			const int shaDigestLength = 20;

			var i = ri.ToBigInteger();
			var iBytes = i.ToCleanByteArray();

			// Copy digest to buffer ane restore any padding zero bytes.
			var digest = new byte[shaDigestLength];

			if (iBytes.Length <= shaDigestLength)
			{
				Buffer.BlockCopy(iBytes, 0, digest, 0, iBytes.Length);

			}
			Array.Reverse(digest, 0, shaDigestLength);

			using (var ms = new MemoryStream())
			using (var sha1 = SHA1.Create())
			{
				// write slat and digest.
				ms.Write(s.ToCleanByteArray());
				ms.Write(digest);

				// reset the stream to the start location in order to read.
				ms.Seek(0, SeekOrigin.Begin);

				// Compute SHA1
				var sha1Result = sha1.ComputeHash(ms);

				var x = sha1Result.ToBigInteger();
				v = g.ModPow(x, N);

				return true;
			}
		}

		/// <summary>
		/// generates a strong session key (K) of session key (S)
		/// </summary>
		public void HashSessionKey()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// compares proof (M) of strong session key (K)
		/// </summary>
		/// <param name="lpM">client proof (M) of the strong session key (K)</param>
		/// <param name="l">the length of client proof (M)</param>
		/// <returns>true if client and server proof matches otherwise false</returns>
		public bool CompareProof(string lpM, int l)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// compare password verifier (v). verifies if provided password matches the password verifier (v). requires to use the same salt (s) which was initially used to compute v.
		/// </summary>
		/// <param name="vc">predefined password verifier (v) read from database</param>
		/// <returns>true if password verifier matches otherwise false</returns>
		public bool ProofVerifier(string vc)
		{
			throw new NotImplementedException();

		}

		/// <summary>
		/// generate hash for proof of strong session key (K). this hash has to be send to the client for client-side proof. client has to show it's proof first. If the server detects an incorrect proof. it must abort without showing it's proof.
		/// </summary>
		/// <param name="sha">reference to an empty Sha1Hash object</param>
		public void Finalize(byte sha)
		{
			throw new NotImplementedException();
		}


		#region Private Algorithm Parameters


		// ReSharper disable InconsistentNaming

		private BigInteger A, u, S;
		private BigInteger N, s, g, v;
		private BigInteger b, B;
		private BigInteger K;
		private BigInteger M;

		// ReSharper restore InconsistentNaming


		#endregion

		#region Public Properties

		public BigInteger HostPublicEphemeral => B;
		public BigInteger GeneratorModulo => g;
		public BigInteger Prime => N;
		public BigInteger Proof => M;


		public BigInteger Salt
		{
			get => s;
			set => s = value;
		}

		public BigInteger StrongSessionKey
		{
			get => K;
			set => K = value;
		}
		public BigInteger Verifier
		{
			get => v;
			set => v = value;
		}

		#endregion

	}
}