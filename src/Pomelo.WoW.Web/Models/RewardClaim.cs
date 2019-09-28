using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.WoW.Web.Models
{
    public class RewardClaim : ModelBase
    {
        [Column("character_id")]
        public uint CharacterId { get; set; }

        [Column("reward_id")]
        public uint RewardId { get; set; }
    }
}
