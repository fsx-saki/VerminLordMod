using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转木道辅助蛊", "五转", "木")]
    public class 白荔仙元 : ModItem
    {
        private const int QiCostPerUse = 30;
        private const int LifeBoost = 25;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 10;
            Item.value = 100000;
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

            player.statLifeMax2 += LifeBoost;
            player.statLife += LifeBoost;
            if (player.statLife > player.statLifeMax2)
                player.statLife = player.statLifeMax2;

            if (Main.myPlayer == player.whoAmI)
            {
                player.HealEffect(LifeBoost);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "BaiLiXianYuanEffect", $"永久增加{LifeBoost}点最大生命值"));
            tooltips.Add(new TooltipLine(Mod, "BaiLiXianYuanQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
