using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 千命蛊 - 增加1000年寿命
    /// 效果：永久增加生命上限
    /// </summary>
    internal class ThousandLifeGu : GuConsumableItem
    {
        public override int QiCost => 1000;
        public override int GuLevel => 4;

        protected override void ApplyEffect(Player player, QiPlayer qiPlayer)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.AddExtraAges(1000);
            CombatText.NewText(player.getRect(), Color.Green, "寿命+1000");
        }
    }
}
