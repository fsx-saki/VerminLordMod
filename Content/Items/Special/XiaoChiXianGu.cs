using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转人道辅助仙蛊", "五转", "人")]
    public class XiaoChiXianGu : ModItem
    {
        private const int QiCostPerUse = 15;
        private const int HealAmount = 40;
        private const int RegenDuration = 300;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 1;
            Item.value = 100000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            return qiResource.QiCurrent >= QiCostPerUse;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            player.Heal(HealAmount);
            player.AddBuff(BuffID.Regeneration, RegenDuration);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "XiaoChiEffect", $"小吃仙蛊：回复{HealAmount}HP，获得生命回复效果"));
            tooltips.Add(new TooltipLine(Mod, "XiaoChiDuration", $"生命回复持续：{RegenDuration / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "XiaoChiQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
