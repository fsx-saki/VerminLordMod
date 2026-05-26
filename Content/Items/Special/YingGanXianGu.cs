using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using VerminLordMod.Content.Items.Accessories;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转力道防御仙蛊", "五转", "力")]
    class YingGanXianGu : GuAccessoryItem
    {
        protected override int qiCost => 36;
        protected override int _guLevel => 5;

        public static LocalizedText UsesXQiText { get; private set; }
        public static LocalizedText GuLevel { get; private set; }

        public override void SetStaticDefaults()
        {
            UsesXQiText = this.GetLocalization("UsesXQi");
            GuLevel = this.GetLocalization("GuLevel");
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(qiCost)));
            tooltips.Insert(3, new TooltipLine(Mod, "GuLevel", GuLevel.Format(_guLevel)));
            tooltips.Add(new TooltipLine(Mod, "YingGanDesc", "+14防御，免疫击退"));
            tooltips.Add(new TooltipLine(Mod, "YingGanSpecial", "受击时20%概率只受50%伤害（硬干！）"));
            if (controlRate > 0f)
            {
                tooltips.Add(new TooltipLine(Mod, "ControlRate", $"炼化进度：{controlRate}%"));
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "ControlRate", "右键使用开始炼化"));
            }
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = 1000000;
            Item.rare = ItemRarityID.Cyan;
            Item.accessory = true;
            Item.defense = 22;
            Item.useStyle = ItemUseStyleID.Guitar;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.QiMaxCurrent -= qiCost;

            player.noKnockback = true;

            player.GetModPlayer<YingGanPlayer>().HasYingGan = true;
        }
    }
}
