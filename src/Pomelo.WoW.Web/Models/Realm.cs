using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pomelo.WoW.Web.Models
{
    [Flags]
    public enum RealmStatus
    {
        Invalid = 2,
        Offline = 4,
        New = 32,
        Recommend = 64
    }

    public enum RealmIcon
    {
        PvE = 0,
        PvP = 1,
        RP = 6,
        RPPvP = 8,
        FFA = 16
    }

    public class Realm : ModelBase
    {
        [Column("entry")]
        public uint Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("realmflags")]
        public RealmStatus Status { get; set; }

        [Column("icon")]
        public RealmIcon Icon { get; set; }
    }
}
