using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.WoW.Web.Models
{
    public class Parameter : ModelBase
    {
        [Column("entry")]
        public string Id { get; set; }

        [Column("value")]
        public string Value { get; set; }

        [Column("comment")]
        public string Comment { get; set; }
    }
}
