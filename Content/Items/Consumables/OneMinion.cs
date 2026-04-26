using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 一胎蛊 - 增加一个召唤栏
    /// 效果：永久增加一个召唤栏位（仅一次）
    /// </summary>
    internal class OneMinion : GuConsumableItem
    {
        public override int QiCost => 20;
        public override int GuLevel => 1;

        protected override bool CanApplyEffect(Player player, QiPlayer qiPlayer)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            if (guPerk.hasOneMinion)
            {
                Text.ShowTextRed(player, "您已经使用过一胎蛊了！");
                return false;
            }
            return true;
        }

        protected override void ApplyEffect(Player player, QiPlayer qiPlayer)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.SetOneMinion();
            CombatText.NewText(player.getRect(), Color.Purple, "召唤栏+1");
        }
    }
}
