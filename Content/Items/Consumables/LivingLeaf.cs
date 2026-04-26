using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Debuff;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 活叶 - 治疗蛊虫
    /// 效果：立即恢复200生命，有冷却时间
    /// </summary>
    internal class LivingLeaf : GuConsumableItem
    {
        public override int QiCost => 20;
        public override int GuLevel => 1;

        protected override bool CanApplyEffect(Player player, QiPlayer qiPlayer)
        {
            if (player.HasBuff<LivingLeaveCDbuff>())
            {
                Text.ShowTextRed(player, "活叶正在冷却中！");
                return false;
            }
            return true;
        }

        protected override void ApplyEffect(Player player, QiPlayer qiPlayer)
        {
            player.Heal(200);
            // 5分钟冷却（18000帧）
            player.AddBuff(ModContent.BuffType<LivingLeaveCDbuff>(), 18000);
            CombatText.NewText(player.getRect(), Color.Green, "+200生命");
        }
    }
}
