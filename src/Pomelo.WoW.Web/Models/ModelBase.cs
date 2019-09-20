using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Dapper;

namespace Pomelo.WoW.Web.Models
{
    public class ModelBase
    {
        private static IEnumerable<Database> _db;

        static ModelBase()
        {
            GetDatabases();
        }

        public static IEnumerable<Database> EnumerateWorldDbs()
        {
            return _db.Where(x => !x.RealmId.HasValue);
        }

        public static MySqlConnection GetAuthDb()
        {
            return new MySqlConnection(Startup.Configuration["Database"]);
        }

        public static MySqlConnection GetWorldDb(uint id)
        {
            return _db.SingleOrDefault(x => x.Id == id && x.RealmId == null).GetConn();
        }

        public static MySqlConnection GetCharacterDb(uint id)
        {
            return _db.SingleOrDefault(x => x.RealmId == id).GetConn();
        }

        public static IEnumerable<Database> GetCharacterDbs()
        {
            return _db.Where(x => x.RealmId.HasValue);
        }

        private static void GetDatabases()
        {
            using (var conn = GetAuthDb())
            {
                var query = conn.Query<Database>(
                    "SELECT * FROM `pomelo_database`;");
                _db = query.ToList();
            }
        }
    }
}
