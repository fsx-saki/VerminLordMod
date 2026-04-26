using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 大力天牛蛊 - 一转力量型蛊虫，临时牛力
    /// 原文："蛮力天牛蛊...一转力量型蛊虫，临时牛力"
    /// "那就请族长大人，赐我一只蛮力天牛蛊。有了这一牛之力..."
    /// 效果：60秒内近战伤害+50%
    /// </summary>
    internal class StrengthLongicorn : GuConsumableItem
    {
        public override int QiCost => 20;
        public override int GuLevel => 1;

        protected override void ApplyEffect(Player player, QiPlayer qiPlayer)
        {
            // 3600帧 = 60秒
            player.AddBuff(ModContent.BuffType<StrengthLongicornbuff>(), 3600);
            CombatText.NewText(player.getRect(), Color.Orange, "蛮牛之力！");
        }
    }
}
