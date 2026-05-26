using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转力道辅助蛊", "一转", "力")]
    public class ZongXiongBenLiGu : ModItem
    {
        private const int QiCostPerUse = 10;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 99;
            Item.value = 1000;
            Item.consumable = true;
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

            var heiBaiShiPlayer = player.GetModPlayer<HeiBaiShiPlayer>();
            heiBaiShiPlayer.BonusMeleeDamageCount++;

            Text.ShowTextGreen(player, $"棕熊本力蛊：永久增加近战伤害3%（当前累计：{heiBaiShiPlayer.BonusMeleeDamageCount}次）");

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ZongXiongBenLiEffect", "永久增加近战伤害3%（棕熊之力）"));
            tooltips.Add(new TooltipLine(Mod, "ZongXiongBenLiQiCost", $"消耗真元：{QiCostPerUse}"));
            tooltips.Add(new TooltipLine(Mod, "ZongXiongBenLiConsumable", "一次性消耗品"));
        }
    }
}
