using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.WoW.Web.Models
{
    public enum Class
    {
        Warrior = 1,
        Paladin = 2,
        Hunter = 3,
        Rogue = 4,
        Priest = 5,
        DeathKnight = 6,
        Shaman = 7,
        Mage = 8,
        Warlock = 9,
        Monk = 10,
        Druid = 11,
        DemonHunter = 12,
    }

    public enum Faction
    {
        Alliance,
        Horde,
        Unknown
    }

    public enum Race
    {
        Human = 1,
        Orc = 2,
        Dwarf = 3,
        NightElf = 4,
        Undead = 5,
        Tauren = 6,
        Gnome = 7,
        Troll = 8,
        Goblin = 9,
        BloodElf = 10,
        Draenei = 11,
        FelOrc = 12,
        Naga = 13,
        Broken = 14,
        Skeleton = 15,
        Vrykul = 16,
        Tuskarr = 17,
        ForestTroll = 18,
    }

    public enum Gender
    {
        Male,
        Female,
        Unknown
    }

    public class Character : ModelBase
    {
        [Column("guid")]
        public ulong Id { get; set; }

        [Column("account")]
        public uint AccountId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("race")]
        public Race Race { get; set; }

        [Column("class")]
        public Class Class { get; set; }

        [Column("gender")]
        public Gender Gender { get; set; }

        [Column("level")]
        public int Level { get; set; }

        [Column("position_x")]
        public double X { get; set; }

        [Column("position_y")]
        public double Y { get; set; }

        [Column("position_z")]
        public double Z { get; set; }

        [Column("map")]
        public int Map { get; set; }

        [Column("online")]
        public bool Online { get; set; }

        [Column("deleteDate")]
        public DateTime? DeleteDate { get; set; }

        [NotMapped]
        public uint RealmId { get; set; }
    }
}
