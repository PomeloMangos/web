using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Pomelo.WoW.Web.Lib
{
    public static class SHA256
    {
        private static System.Security.Cryptography.SHA256 hasher = System.Security.Cryptography.SHA256.Create();
        private static Random random = new Random();

        public static (byte[] hash, byte[] salt) Generate(string password)
        {
            var salt = new byte[32];
            random.NextBytes(salt);
            var mixed = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
            return (hasher.ComputeHash(mixed), salt);
        }

        public static byte[] Generate(string username, string password, byte[] salt)
        {
            var mixed = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
            return hasher.ComputeHash(mixed);
        }
    }
}
