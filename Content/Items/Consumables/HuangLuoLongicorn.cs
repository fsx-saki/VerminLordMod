using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 黄罗天牛蛊 - 一转耐力型蛊虫，提升持续力
    /// 原文："黄骆天牛蛊...一转耐力型蛊虫，提升持续力"
    /// 效果：120秒内真元恢复速度提升（耐力）
    /// </summary>
    internal class HuangLuoLongicorn : GuConsumableItem
    {
        public override int QiCost => 20;
        public override int GuLevel => 1;

        protected override void ApplyEffect(Player player, QiPlayer qiPlayer)
        {
            // 7200帧 = 120秒，耐力型持续更久
            player.AddBuff(ModContent.BuffType<HuangLuoLongicornbuff>(), 7200);
            CombatText.NewText(player.getRect(), Color.Lime, "耐力提升！");
        }
    }
}
