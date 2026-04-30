using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 黑豕蛊 - 一转珍稀豕蛊，永久加力
    /// 原文："黑、白豕蛊的能力，就是改造蛊师的身躯，从根本上增长蛊师的气力。"
    /// "一转珍稀豕蛊，永久加力"
    /// 效果：永久增加近战伤害1%，上限一猪之力（1%），可与白豕蛊叠加
    /// </summary>
    internal class BlackPigGu : GuConsumableItem
    {
        public override int QiCost => 10;
        public override int GuLevel => 1;

        protected override bool CanApplyEffect(Player player, QiResourcePlayer qiResource)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            if (guPerk.blackPigPower >= GuPerkSystem.MAX_BLACK_PIG_POWER)
            {
                Text.ShowTextRed(player, "黑豕蛊已达上限（一猪之力）！");
                return false;
            }
            return true;
        }

        protected override void ApplyEffect(Player player, QiResourcePlayer qiResource)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.TryAddBlackPigPower(1);
            CombatText.NewText(player.getRect(), Color.Gold, "黑豕之力+1");
        }
    }
}
