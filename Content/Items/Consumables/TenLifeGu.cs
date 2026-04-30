using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 十命蛊 - 增加10年寿命
    /// 效果：永久增加生命上限
    /// </summary>
    internal class TenLifeGu : GuConsumableItem
    {
        public override int QiCost => 20;
        public override int GuLevel => 2;

        protected override void ApplyEffect(Player player, QiResourcePlayer qiResource)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.AddExtraAges(10);
            CombatText.NewText(player.getRect(), Color.Green, "寿命+10");
        }
    }
}
