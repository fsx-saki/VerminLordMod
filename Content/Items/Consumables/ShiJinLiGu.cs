using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 十斤力蛊 - 增加10倍斤力
    /// 效果：永久增加近战伤害（每只+10%）
    /// </summary>
    internal class ShiJinLiGu : GuConsumableItem
    {
        public override int QiCost => 100;
        public override int GuLevel => 2;

        protected override void ApplyEffect(Player player, QiResourcePlayer qiResource)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.AddJinLiPower(10);
            CombatText.NewText(player.getRect(), Color.Gold, "斤力+10");
        }
    }
}
