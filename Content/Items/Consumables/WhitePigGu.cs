using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 白豕蛊 - 一转珍稀豕蛊，永久加力
    /// 原文："黑、白豕蛊的能力，就是改造蛊师的身躯，从根本上增长蛊师的气力。"
    /// "方源已经利用白豕蛊，为自己增添了一猪之力。如果他继续使用第二只白豕蛊，那么不会有任何力量增加的效果。"
    /// "而黑白豕蛊增幅给蛊师的力量，不需要真元，就能发挥作用。"
    /// 效果：永久增加近战伤害1%，上限一猪之力（1%）
    /// </summary>
    internal class WhitePigGu : GuConsumableItem
    {
        public override int QiCost => 10;
        public override int GuLevel => 1;

        protected override bool CanApplyEffect(Player player, QiResourcePlayer qiResource)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            if (guPerk.whitePigPower >= GuPerkSystem.MAX_WHITE_PIG_POWER)
            {
                Text.ShowTextRed(player, "白豕蛊已达上限（一猪之力）！");
                return false;
            }
            return true;
        }

        protected override void ApplyEffect(Player player, QiResourcePlayer qiResource)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.TryAddWhitePigPower(1);
            CombatText.NewText(player.getRect(), Color.Gold, "白豕之力+1");
        }
    }
}
