using Pomelo.WoW.SRP6;

namespace Pomelo.WoW.Web.Lib
{
    public static class SRP6
    {
        private static SRP6AuthenticationService instance = new SRP6AuthenticationService();

        public static (string v, string s) Generate(string username, string password)
        {
            var hash = instance.GenerateAccountVerifier(username, password);
            return (hash.Verifier, hash.Salt);
        }
    }
}
