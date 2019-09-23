using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.WoW.Web.Models
{
    public class DungeonSwitch : ModelBase
    {
        [Column("entry")]
        public uint Id { get; set; }

        [Column("disabled")]
        public bool Disabled { get; set; }

        [Column("comment")]
        public string Comment { get; set; }
    }
}
