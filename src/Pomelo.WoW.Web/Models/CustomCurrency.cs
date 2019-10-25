using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.WoW.Web.Models
{
    public class CustomCurrency : ModelBase
    {
        [Column("entry")]
        public uint Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("amount")]
        public uint Amount { get; set; }

        [Column("comment")]
        public string Comment { get; set; }
    }
}
