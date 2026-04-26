using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 十钧之力蛊 - 四转力道蛊
    /// 原文："十钧之力蛊，外形并不惹眼，仿佛普通的铁秤砣。方源催动起来，它便悬浮在方源的头顶，散发玄光，照在方源全身，将某段力量的道纹，刻印在方源的身上。"
    /// 效果：永久增加近战伤害（每只+300%）
    /// </summary>
    internal class ShiJunLiGu : GuConsumableItem
    {
        public override int QiCost => 2000;
        public override int GuLevel => 4;

        protected override void ApplyEffect(Player player, QiPlayer qiPlayer)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.AddJunLiPower(300);
            CombatText.NewText(player.getRect(), Color.Gold, "十钧之力+300");
        }
    }
}
