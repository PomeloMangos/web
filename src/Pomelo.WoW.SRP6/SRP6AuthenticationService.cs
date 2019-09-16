using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using FreecraftCore.Crypto;

namespace Pomelo.WoW.SRP6
{
	/// <summary>
	/// Provide SRP6-based authentication manner.
	/// </summary>
	public class SRP6AuthenticationService
	{
		/// <summary>
		/// Create account verification information.
		/// </summary>
		/// <param name="userName">The account user name.</param>
		/// <param name="password">The account password.</param>
		/// <returns>The generated account verification information.</returns>
		public AccountVerificationInfo GenerateAccountVerifier(string userName, string password)
		{
			if (userName == null) throw new ArgumentNullException(nameof(userName));
			if (password == null) throw new ArgumentNullException(nameof(password));

			// Change all chars to upper case
			userName = userName.ToUpperInvariant();
			password = password.ToUpperInvariant();

			// Compute user name and password hash
			var userHashString = $"{userName}:{password}";
			var userHashSha1Hex = GetSha1Hex(userHashString);

			// Get Salt and Verifier
			var srp6 = new SRP6();
			srp6.CalculateVerifier(userHashSha1Hex);

			return new AccountVerificationInfo
			{
				Salt = srp6.Salt.ToHexString(),
				Verifier = srp6.Verifier.ToHexString()
			};
		}

		/// <summary>
		/// Generate the SHA1 result hex-string for a string. Using <see cref="Encoding.UTF8"/> to encoding the source string.
		/// </summary>
		/// <param name="str">The source string.</param>
		/// <returns>The generated SHA1 hex-string.</returns>
		private static string GetSha1Hex(string str)
		{
			using (var sha1 = SHA1.Create())
			{
				var bytes = Encoding.UTF8.GetBytes(str);
				var hashBytes = sha1.ComputeHash(bytes);

				return GetHexString(hashBytes);
			}
		}

		/// <summary>
		/// Convert byte sequence into hex string.
		/// </summary>
		/// <param name="data">The source byte sequence.</param>
		/// <returns>The converted hex string.</returns>
		private static string GetHexString(byte[] data)
		{
			var sb = new StringBuilder(data.Length * 2);
			foreach (var b in data)
			{
				sb.AppendFormat("{0:X2}", b);
			}

			return sb.ToString();
		}
	}
}
