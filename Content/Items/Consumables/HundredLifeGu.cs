using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 百命蛊 - 增加100年寿命
    /// 效果：永久增加生命上限
    /// </summary>
    internal class HundredLifeGu : GuConsumableItem
    {
        public override int QiCost => 100;
        public override int GuLevel => 3;

        protected override void ApplyEffect(Player player, QiResourcePlayer qiResource)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.AddExtraAges(100);
            CombatText.NewText(player.getRect(), Color.Green, "寿命+100");
        }
    }
}
