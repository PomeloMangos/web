using System;

namespace Pomelo.WoW.Web.Models
{
    public class Card : ModelBase
    {
        public uint Id { get; set; }
        public string Password { get; set; }
        public uint Value { get; set; }
        public uint? Account { get; set; }
        public DateTime? UsedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
