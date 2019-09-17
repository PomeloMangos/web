using Pomelo.WoW.Web.Models;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static class HtmlHelperExtension
    {
        public static string ColorizeClass(this IHtmlHelper html, Class @class)
        {
            switch (@class)
            {
                case Class.Warrior:
                    return "#C69B6D";
                case Class.Paladin:
                    return "#F48CBA";
                case Class.Hunter:
                    return "#AAD372";
                case Class.Rogue:
                    return "#FFF468";
                case Class.Priest:
                    return "#F0EBE0";
                case Class.DeathKnight:
                    return "#C41E3B";
                case Class.Shaman:
                    return "#2359FF";
                case Class.Mage:
                    return "#68CCEF";
                case Class.Warlock:
                    return "#9382C9";
                case Class.Druid:
                    return "#FF7C0A";
                default:
                    return "inherit";
            }
        }

        public static string GetClassName(this IHtmlHelper html, Class @class)
        {
            switch (@class)
            {
                case Class.Warrior:
                    return "战士";
                case Class.Paladin:
                    return "圣骑士";
                case Class.Hunter:
                    return "猎人";
                case Class.Rogue:
                    return "潜行者";
                case Class.Priest:
                    return "牧师";
                case Class.DeathKnight:
                    return "死亡骑士";
                case Class.Shaman:
                    return "萨满祭司";
                case Class.Mage:
                    return "法师";
                case Class.Warlock:
                    return "术士";
                case Class.Druid:
                    return "德鲁伊";
                default:
                    return "未知";
            }
        }
        
        public static string GetFactionClassName(this IHtmlHelper html, Faction faction)
        {
            if (faction == Faction.Alliance)
                return "alliance";
            else if (faction == Faction.Horde)
                return "horde";
            else
                return "";
        }

        public static string GetRaceName(this IHtmlHelper html, Race race)
        {
            switch (race)
            {
                case Race.BloodElf:
                    return "血精灵";
                case Race.Draenei:
                    return "德莱尼";
                case Race.Gnome:
                    return "侏儒";
                case Race.Human:
                    return "人类";
                case Race.NightElf:
                    return "暗夜精灵";
                case Race.Orc:
                    return "兽人";
                case Race.Tauren:
                    return "牛头人";
                case Race.Troll:
                    return "巨魔";
                case Race.Undead:
                    return "亡灵";
                default:
                    return "未知";
            }
        }

    }
}