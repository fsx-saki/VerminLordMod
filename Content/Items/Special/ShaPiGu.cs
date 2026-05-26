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
    [ImplStatus(ImplStatus.Implemented, "五转水道防御蛊", "五转", "水")]
    public class ShaPiGu : GuAccessoryItem
    {
        protected override int qiCost => 25;
        protected override int _guLevel => 5;

        public static LocalizedText UsesXQiText { get; private set; }
        public static LocalizedText ControlRate { get; private set; }
        public static LocalizedText GuLevel { get; private set; }

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
            if (controlRate > 0f)
            {
                tooltips.Add(new TooltipLine(Mod, "ControlRate", ControlRate.Format(controlRate)));
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "ControlRate", "右键使用开始炼化"));
            }
            tooltips.Add(new TooltipLine(Mod, "ShaPiEffect", "+10防御，近战攻击者受到15%反伤，-8%移动速度"));
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = 50000;
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
            Item.defense = 10;
            Item.useStyle = ItemUseStyleID.Guitar;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var shaPiPlayer = player.GetModPlayer<ShaPiPlayer>();
            shaPiPlayer.HasShaPiEquipped = true;

            player.moveSpeed -= 0.08f;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.QiMaxCurrent -= qiCost;
        }
    }
}
