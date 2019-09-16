using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Pomelo.WoW.Web.Models
{
    public class ModelBase
    {
        public static MySqlConnection GetAuthDb()
        {
            return new MySqlConnection(Startup.Config.Databases.Auth);
        }

        public static MySqlConnection GetWorldDb()
        {
            return new MySqlConnection(Startup.Config.Databases.World);
        }

        public static MySqlConnection GetPlayerDb(long id)
        {
            return new MySqlConnection(Startup.Config.Databases.Players.Single(x => x.Id == id).Value);
        }
    }
}
