using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 花猪蛊 - 一转豕蛊系列，临时加力
    /// 原文："花豕蛊的作用，和蛮力天牛蛊类似，都是临时性增加蛊师的力气。"
    /// "他的对手空有一枚花豕蛊，虽然能暂时暴涨一猪之力，但是却英雄无用武之地。"
    /// 效果：60秒内近战伤害+30%
    /// </summary>
    internal class FlowerPig : GuConsumableItem
    {
        public override int QiCost => 20;
        public override int GuLevel => 1;

        // 花猪蛊是临时Buff，不消耗物品（战斗中反复使用）
        public override bool IsConsumed => true;

        protected override void ApplyEffect(Player player, QiPlayer qiPlayer)
        {
            // 3600帧 = 60秒，战斗中临时爆发
            player.AddBuff(ModContent.BuffType<FlowerPigbuff>(), 3600);
            CombatText.NewText(player.getRect(), Color.Orange, "花猪之力！");
        }
    }
}
