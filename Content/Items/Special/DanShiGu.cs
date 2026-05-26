using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转魂道辅助蛊", "五转", "魂")]
    public class DanShiGu : ModItem
    {
        private const int QiCostPerUse = 40;
        private const int LifeIncrease = 20;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 10;
            Item.value = 50000;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item4;
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

            player.statLifeMax2 += LifeIncrease;
            player.statLife += LifeIncrease;
            if (player.statLife > player.statLifeMax2)
                player.statLife = player.statLifeMax2;

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DanShiEffect", $"永久增加{LifeIncrease}点最大生命值，并回复{LifeIncrease}点生命值"));
            tooltips.Add(new TooltipLine(Mod, "DanShiQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
