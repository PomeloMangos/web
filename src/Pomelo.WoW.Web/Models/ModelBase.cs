using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Pomelo.WoW.Web.Models
{
    public class ModelBase
    {
        public async static Task<MySqlConnection> GetAuthDbAsync()
        {
            var conn = new MySqlConnection(Startup.Config.Databases.Auth);
            await conn.OpenAsync();
            return conn;
        }

        public async static Task<MySqlConnection> GetWorldDbAsync()
        {
            var conn = new MySqlConnection(Startup.Config.Databases.World);
            await conn.OpenAsync();
            return conn;
        }

        public async static Task<MySqlConnection> GetPlayerDbAsync(long id)
        {
            var conn = new MySqlConnection(Startup.Config.Databases.Players.Single(x => x.Id == id).Value);
            await conn.OpenAsync();
            return conn;
        }
    }
}
