namespace Pomelo.WoW.SRP6
{
	/// <summary>
	/// Account verification information used to authenticate with user accounts.
	/// </summary>
	public class AccountVerificationInfo
	{
		/// <summary>
		/// The salt.
		/// </summary>
		public string Salt { get; set; }
		/// <summary>
		/// The verifier.
		/// </summary>
		public string Verifier { get; set; }
	}
}