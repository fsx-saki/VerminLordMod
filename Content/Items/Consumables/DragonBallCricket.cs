using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 龙珠蟋蟀 - 增加移动速度
    /// 效果：永久增加移动速度和加速度
    /// </summary>
    internal class DragonBallCricket : GuConsumableItem
    {
        public override int QiCost => 20;
        public override int GuLevel => 1;

        protected override void ApplyEffect(Player player, QiResourcePlayer qiResource)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.AddExtraSpeed(0.05f, 0.005f);
            CombatText.NewText(player.getRect(), Color.Cyan, "移速+5%");
        }
    }
}
