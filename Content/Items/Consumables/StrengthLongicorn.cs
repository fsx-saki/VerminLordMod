using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 大力天牛 - 增加近战伤害
    /// 效果：永久增加近战伤害
    /// </summary>
    internal class StrengthLongicorn : GuConsumableItem
    {
        public override int QiCost => 30;
        public override int GuLevel => 1;

        protected override void ApplyEffect(Player player, QiResourcePlayer qiResource)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.AddJinLiPower(2);
            CombatText.NewText(player.getRect(), Color.Gold, "力道+2");
        }
    }
}
