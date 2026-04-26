using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 十斤之力蛊 - 二转力道蛊
    /// 效果：永久增加近战伤害（每只+10%）
    /// </summary>
    internal class ShiJinLiGu : GuConsumableItem
    {
        public override int QiCost => 80;
        public override int GuLevel => 2;

        protected override void ApplyEffect(Player player, QiPlayer qiPlayer)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.AddJinLiPower(10);
            CombatText.NewText(player.getRect(), Color.Gold, "十斤之力+10");
        }
    }
}
