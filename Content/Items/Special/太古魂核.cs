using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.DataStructures;
using VerminLordMod.Common.Configs;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content;
using VerminLordMod.Content.Items.Accessories;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转魂道防御蛊", "五转", "魂")]
    class 太古魂核 : GuBaseItem
    {
        protected override int _guLevel => 5;
        protected override int qiCost => 25;

        public new static LocalizedText UsesXQiText { get; private set; }
        public new static LocalizedText ControlRateText { get; private set; }
        public new static LocalizedText GuLevelText { get; private set; }

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
            tooltips.Add(new TooltipLine(Mod, "TaiGuHunHeDef", "装备时：防御+15，伤害减免+10%"));
            tooltips.Add(new TooltipLine(Mod, "TaiGuHunHeReflect", "20%几率将受到的伤害以魂伤害反弹给攻击者（无视防御）"));
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
            Item.height = 24;
            Item.value = 50000;
            Item.rare = ItemRarityID.LightPurple;
            Item.accessory = true;
            Item.defense = 15;
            Item.useStyle = ItemUseStyleID.Guitar;
        }

        public override void OnActiveTick(Player player)
        {
            if (Main.netMode == NetmodeID.Server) return;

            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            var qiResource = player.GetModPlayer<QiResourcePlayer>();

            if (_guLevel > qiRealm.GuLevel && Randommer.Roll(10))
            {
                Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
                player.Hurt(PlayerDeathReason.LegacyDefault(), (_guLevel - qiRealm.GuLevel) * Main.LocalPlayer.statLifeMax2 / 20, 0);
            }

            player.endurance += 0.10f;
            qiResource.QiMaxCurrent -= qiCost;

            player.GetModPlayer<TaiGuHunHePlayer>().TaiGuHunHeActive = true;
        }
    }
}
