using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.DataStructures;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Items.Accessories;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "四转气道防御蛊", "四转", "气")]
    class QiJiaGu : GuAccessoryItem
    {
        protected override int _guLevel => 4;
        protected override int qiCost => 20;

        public static LocalizedText UsesXQiText { get; private set; }
        public static LocalizedText ControlRateText { get; private set; }
        public static LocalizedText GuLevelText { get; private set; }

        public override void SetStaticDefaults()
        {
            UsesXQiText = this.GetLocalization("UsesXQi");
            ControlRateText = this.GetLocalization("ControlRate");
            GuLevelText = this.GetLocalization("GuLevel");
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(qiCost)));
            tooltips.Insert(3, new TooltipLine(Mod, "GuLevel", GuLevelText.Format(_guLevel)));
            if (controlRate > 0f)
            {
                tooltips.Add(new TooltipLine(Mod, "ControlRate", ControlRateText.Format(controlRate)));
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
            Item.defense = 10;
            Item.useStyle = ItemUseStyleID.Guitar;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (Main.netMode == NetmodeID.Server) return;

            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            var qiResource = player.GetModPlayer<QiResourcePlayer>();

            if (_guLevel > qiRealm.GuLevel && Randommer.Roll(10))
            {
                Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
                player.Hurt(PlayerDeathReason.LegacyDefault(), (_guLevel - qiRealm.GuLevel) * Main.LocalPlayer.statLifeMax2 / 20, 0);
            }

            qiResource.QiMaxCurrent -= qiCost;

            player.GetModPlayer<QiJiaPlayer>().QiJiaActive = true;
        }
    }
}
