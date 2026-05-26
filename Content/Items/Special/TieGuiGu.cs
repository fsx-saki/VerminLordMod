using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Items.Accessories;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转金道防御蛊", "三转", "金")]
    public class TieGuiGu : GuAccessoryItem
    {
        public static LocalizedText UsesXQiText { get; private set; }
        public static LocalizedText ControlRate { get; private set; }
        public static LocalizedText GuLevel { get; private set; }

        protected override int _guLevel => 3;
        protected override int qiCost => 20;

        public override void SetStaticDefaults()
        {
            UsesXQiText = this.GetLocalization("UsesXQi");
            ControlRate = this.GetLocalization("ControlRate");
            GuLevel = this.GetLocalization("GuLevel");
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(qiCost)));
            tooltips.Insert(3, new TooltipLine(Mod, "GuLevel", GuLevel.Format(_guLevel)));
            tooltips.Add(new TooltipLine(Mod, "TieGuiDefense", "增加12点防御力"));
            tooltips.Add(new TooltipLine(Mod, "TieGuiSlow", "降低10%移动速度（铁柜沉重）"));
            tooltips.Add(new TooltipLine(Mod, "TieGuiBlock", "20%几率完全格挡一次攻击（如铁柜门）"));
            if (controlRate > 0f)
            {
                tooltips.Add(new TooltipLine(Mod, "ControlRate", ControlRate.Format(controlRate)));
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "ControlRate", "右键使用开始炼化"));
            }
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.value = 50000;
            Item.rare = ItemRarityID.LightRed;
            Item.accessory = true;
            Item.defense = 12;
            Item.useStyle = ItemUseStyleID.Guitar;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.QiMaxCurrent -= qiCost;

            player.moveSpeed *= 0.9f;

            var tieGuiPlayer = player.GetModPlayer<TieGuiPlayer>();
            tieGuiPlayer.HasTieGuiEquipped = true;
        }
    }
}
