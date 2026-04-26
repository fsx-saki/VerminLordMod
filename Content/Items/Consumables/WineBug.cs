using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 酒虫 - 一转珍稀蛊虫，精炼真元
    /// 原文："酒虫，一转蛊虫，喜酒水，能飞行"
    /// "一转蛊虫，可精炼真元"
    /// "酒虫精炼真元，提升修行"
    /// 效果：提升真元品质，永久增加真元恢复速度+1
    /// 后续可升级为四味酒虫（二转）、七香酒虫（三转）、九眼酒虫（四转）
    /// </summary>
    internal class WineBug : GuConsumableItem
    {
        public override int QiCost => 10;
        public override int GuLevel => 1;

        protected override bool CanApplyEffect(Player player, QiPlayer qiPlayer)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            if (guPerk.wineBugLevel >= GuPerkSystem.WineBugLevel.Basic)
            {
                Text.ShowTextRed(player, "您已经拥有酒虫精炼真元的效果！");
                return false;
            }
            return true;
        }

        protected override void ApplyEffect(Player player, QiPlayer qiPlayer)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.UpgradeWineBug(GuPerkSystem.WineBugLevel.Basic);
            CombatText.NewText(player.getRect(), Color.Gold, "酒虫精炼真元，真元恢复+1");
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Ale, 5)
                .AddIngredient(ItemID.BottledHoney, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
