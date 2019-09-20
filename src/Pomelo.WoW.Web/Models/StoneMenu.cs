using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.WoW.Web.Models
{
    public enum CostType
    {
        Free,
        Money,
        CustomCurrency
    }

    public class StoneMenu : ModelBase
    {
        public static string[] FunctionText = new string[]
        {
            "打开新菜单",
            "传送玩家",
            "打开商人购买界面",
            "打开银行界面",
            "",
            "查询自定义货币",
            "打开幻化菜单",
            "打开多天赋菜单"
        };

        [Column("entry")]
        public uint Id { get; set; }

        [Column("menu_id")]
        public uint MenuId { get; set; }

        [Column("action_id")]
        public uint ActionId { get; set; }

        [Column("icon")]
        public uint Icon { get; set; }

        [Column("menu_item_text")]
        public string MenuItemText { get; set; }

        [Column("function")]
        public uint Function { get; set; }

        [Column("teleport_x")]
        public float TeleportX { get; set; }

        [Column("teleport_y")]
        public float TeleportY { get; set; }

        [Column("teleport_z")]
        public float TeleportZ { get; set; }

        [Column("teleport_map")]
        public uint TeleportMap { get; set; }

        [Column("level_required")]
        public uint LevelRequired { get; set; }

        [Column("permission_required")]
        public uint PermissionRequired { get; set; }

        [Column("faction_order")]
        public uint FactionOrder { get; set; }

        [Column("trigger_menu")]
        public uint TriggerMenu { get; set; }

        [Column("cost_amount")]
        public uint CostAmount { get; set; }

        [Column("cost_type")]
        public CostType CostType { get; set; }

        [Column("cost_currency_id")]
        public uint CostCurrencyId { get; set; }
    }
}
