using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 斤力蛊 - 一转力道蛊
    /// 原文："有斤力蛊、钧力蛊等，增长蛊师力气。"
    /// 效果：永久增加近战伤害（每只+1%）
    /// </summary>
    internal class JinLiGu : GuConsumableItem
    {
        public override int QiCost => 20;
        public override int GuLevel => 1;

        protected override void ApplyEffect(Player player, QiPlayer qiPlayer)
        {
            var guPerk = player.GetModPlayer<GuPerkSystem>();
            guPerk.AddJinLiPower(1);
            CombatText.NewText(player.getRect(), Color.Gold, "斤力+1");
        }
    }
}
