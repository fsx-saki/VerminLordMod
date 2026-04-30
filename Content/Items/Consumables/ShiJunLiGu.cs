using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 十钧力蛊 - 增加10倍钧力
    /// 效果：永久增加近战伤害（每只+100%）
    /// </summary>
    internal class ShiJunLiGu : GuConsumableItem
    {
        public override int QiCost => 500;
        public override int GuLevel => 3;

        protected override void ApplyEffect(Player player, QiResourcePlayer qiResource)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.AddJunLiPower(10);
            CombatText.NewText(player.getRect(), Color.Gold, "钧力+10");
        }
    }
}
