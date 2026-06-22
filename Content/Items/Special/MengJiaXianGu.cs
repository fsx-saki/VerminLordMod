using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Items.Accessories;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转暗道防御仙蛊", "五转", "暗")]
    class MengJiaXianGu : GuBaseItem
    {
        protected override int qiCost => 36;
        protected override int _guLevel => 5;
        public new static LocalizedText UsesXQiText { get; private set; }
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
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.value = 120000;
            Item.rare = ItemRarityID.LightRed;
            Item.accessory = true;
            Item.defense = 22;
            Item.useStyle = ItemUseStyleID.Guitar;
        }
        public override void OnActiveTick(Player player)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            player.moveSpeed -= 0.05f;
            player.GetModPlayer<MengJiaDodgePlayer>().MengJiaDodgeChance += 0.20f;

            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (_guLevel > qiRealm.GuLevel && Randommer.Roll(10))
            {
                Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
                player.Hurt(PlayerDeathReason.LegacyDefault(), (_guLevel - qiRealm.GuLevel) * Main.LocalPlayer.statLifeMax2 / 20, 0);
            }
            qiResource.QiMaxCurrent -= qiCost;
        }
    }
}
