using System.ComponentModel.DataAnnotations.Schema;
using MySql.Data.MySqlClient;

namespace Pomelo.WoW.Web.Models
{
    public class Database : ModelBase
    {
        [Column("entry")]
        public uint Id { get; set; }

        [Column("realmid")]
        public uint? RealmId { get; set; }

        [Column("host")]
        public string Host { get; set; }

        [Column("catalog")]
        public string Catalog { get; set; }

        [Column("uid")]
        public string Uid { get; set; }

        [Column("pwd")]
        public string Pwd { get; set; }

        [Column("comment")]
        public string Comment { get; set; }

        public MySqlConnection GetConn()
        {
            return new MySqlConnection($"Server={Host};Database={Catalog};Uid={Uid};Pwd={Pwd};Allow User Variables=true");
        }
    }
}
