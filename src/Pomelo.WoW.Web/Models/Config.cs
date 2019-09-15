using System.Collections.Generic;

namespace Pomelo.WoW.Web.Models
{
    public class Config
    {
        public ConfigDatabases Databases { get; set; }
    }

    public class ConfigDatabases
    {
        public string Auth { get; set; }
        public string World { get; set; }
        public IEnumerable<ConfigDatabasesRealm> Players { get; set; }
    }

    public class ConfigDatabasesRealm
    {
        public long Id { get; set; }
        public string Value { get; set; }
    }
}
