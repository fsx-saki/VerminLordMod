using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 花豕蛊 - 特殊豕蛊，增加力量
    /// 效果：永久增加近战伤害
    /// </summary>
    internal class FlowerPig : GuConsumableItem
    {
        public override int QiCost => 15;
        public override int GuLevel => 1;

        protected override void ApplyEffect(Player player, QiResourcePlayer qiResource)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.AddJinLiPower(1);
            CombatText.NewText(player.getRect(), Color.Gold, "花豕之力+1");
        }
    }
}
