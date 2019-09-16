using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pomelo.WoW.Web.Models
{
    public enum AccountLevel
    {
        Player,
        Premium,
        Master,
        Administrator
    }

    public class Account : ModelBase
    {
        public ulong Id { get; set; }

        public string Username { get; set; }

        public byte[] Hash { get; set; }

        public byte[] Salt { get; set; }

        public AccountLevel Role { get; set; }

        public string Email { get; set; }

        public string Contact { get; set; }
    }
}
