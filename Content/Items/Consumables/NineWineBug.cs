using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 九眼酒虫 - 四转珍稀蛊虫，精炼真元
    /// 原文："四转酒虫，提纯黄金真元"
    /// 效果：提升真元品质，永久增加真元恢复速度+8
    /// 需先使用七香酒虫，再使用九眼酒虫升级
    /// </summary>
    internal class NineWineBug : GuConsumableItem
    {
        public override int QiCost => 100;
        public override int GuLevel => 4;

        protected override bool CanApplyEffect(Player player, QiPlayer qiPlayer)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            if (guPerk.wineBugLevel < GuPerkSystem.WineBugLevel.SevenScent)
            {
                Text.ShowTextRed(player, "请先使用七香酒虫（三转）精炼真元！");
                return false;
            }
            if (guPerk.wineBugLevel >= GuPerkSystem.WineBugLevel.NineEye)
            {
                Text.ShowTextRed(player, "您已经拥有九眼酒虫的精炼效果！");
                return false;
            }
            return true;
        }

        protected override void ApplyEffect(Player player, QiPlayer qiPlayer)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.UpgradeWineBug(GuPerkSystem.WineBugLevel.NineEye);
            CombatText.NewText(player.getRect(), Color.Gold, "九眼酒虫精炼真元，真元恢复+8");
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.GetInstance<SevenWineBug>(), 2)
                .AddIngredient(ItemID.KingSlimeTrophy, 1)
                .AddIngredient(ItemID.EyeofCthulhuTrophy, 1)
                .AddIngredient(ItemID.EaterofWorldsTrophy, 1)
                .AddIngredient(ItemID.SkeletronTrophy, 1)
                .AddIngredient(ItemID.QueenBeeTrophy, 1)
                .AddIngredient(ItemID.WallofFleshTrophy, 1)
                .AddIngredient(ItemID.DestroyerTrophy, 1)
                .AddIngredient(ItemID.SkeletronPrimeTrophy, 1)
                .AddIngredient(ItemID.RetinazerTrophy, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
