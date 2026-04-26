using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 钧力蛊 - 三转力道蛊
    /// 原文："有斤力蛊、钧力蛊等，增长蛊师力气。"
    /// 效果：永久增加近战伤害（每只+30%）
    /// </summary>
    internal class JunLiGu : GuConsumableItem
    {
        public override int QiCost => 300;
        public override int GuLevel => 3;

        protected override void ApplyEffect(Player player, QiPlayer qiPlayer)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.AddJunLiPower(30);
            CombatText.NewText(player.getRect(), Color.Gold, "钧力+30");
        }
    }
}
