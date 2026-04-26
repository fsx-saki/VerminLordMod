using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 四味酒虫 - 二转珍稀蛊虫，精炼真元
    /// 原文："二转蛊虫，精炼真元"
    /// "二转增幅蛊虫，由酒虫合炼而来"
    /// "精炼二转真元提升境界"
    /// 效果：提升真元品质，永久增加真元恢复速度+2
    /// 需先使用酒虫，再使用四味酒虫升级
    /// </summary>
    internal class FourFlavorWineBug : GuConsumableItem
    {
        public override int QiCost => 20;
        public override int GuLevel => 2;

        protected override bool CanApplyEffect(Player player, QiPlayer qiPlayer)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            if (guPerk.wineBugLevel < GuPerkSystem.WineBugLevel.Basic)
            {
                Text.ShowTextRed(player, "请先使用酒虫（一转）精炼真元！");
                return false;
            }
            if (guPerk.wineBugLevel >= GuPerkSystem.WineBugLevel.FourFlavor)
            {
                Text.ShowTextRed(player, "您已经拥有四味酒虫或更高级的精炼效果！");
                return false;
            }
            return true;
        }

        protected override void ApplyEffect(Player player, QiPlayer qiPlayer)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.UpgradeWineBug(GuPerkSystem.WineBugLevel.FourFlavor);
            CombatText.NewText(player.getRect(), Color.Gold, "四味酒虫精炼真元，真元恢复+2");
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.GetInstance<WineBug>(), 1)
                .AddIngredient(ItemID.Lemonade, 1)
                .AddIngredient(ItemID.BottledHoney, 1)
                .AddIngredient(ItemID.LovePotion, 1)
                .AddIngredient(ItemID.Ale, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
