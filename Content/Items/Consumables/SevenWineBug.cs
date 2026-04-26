using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 七香酒虫 - 三转珍稀蛊虫，精炼真元
    /// 原文："三转蛊虫，精炼真元"
    /// 效果：提升真元品质，永久增加真元恢复速度+4
    /// 需先使用四味酒虫，再使用七香酒虫升级
    /// </summary>
    internal class SevenWineBug : GuConsumableItem
    {
        public override int QiCost => 50;
        public override int GuLevel => 3;

        protected override bool CanApplyEffect(Player player, QiPlayer qiPlayer)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            if (guPerk.wineBugLevel < GuPerkSystem.WineBugLevel.FourFlavor)
            {
                Text.ShowTextRed(player, "请先使用四味酒虫（二转）精炼真元！");
                return false;
            }
            if (guPerk.wineBugLevel >= GuPerkSystem.WineBugLevel.SevenScent)
            {
                Text.ShowTextRed(player, "您已经拥有七香酒虫或更高级的精炼效果！");
                return false;
            }
            return true;
        }

        protected override void ApplyEffect(Player player, QiPlayer qiPlayer)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.UpgradeWineBug(GuPerkSystem.WineBugLevel.SevenScent);
            CombatText.NewText(player.getRect(), Color.Gold, "七香酒虫精炼真元，真元恢复+4");
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.GetInstance<FourFlavorWineBug>(), 2)
                .AddIngredient(ItemID.LifeforcePotion, 1)
                .AddIngredient(ItemID.InfernoPotion, 1)
                .AddIngredient(ItemID.WarmthPotion, 1)
                .AddIngredient(ItemID.TitanPotion, 1)
                .AddIngredient(ItemID.InvisibilityPotion, 1)
                .AddIngredient(ItemID.WaterWalkingPotion, 1)
                .AddIngredient(ItemID.MagicPowerPotion, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
