using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 黄罗天牛 - 增加真元恢复
    /// 效果：永久增加真元恢复速度
    /// </summary>
    internal class HuangLuoLongicorn : GuConsumableItem
    {
        public override int QiCost => 30;
        public override int GuLevel => 1;

        protected override void ApplyEffect(Player player, QiResourcePlayer qiResource)
        {
            // 黄罗天牛Buff：临时增加真元恢复，持续120秒
            player.AddBuff(ModContent.BuffType<Content.Buffs.AddToSelf.Pobuff.HuangLuoLongicornbuff>(), 7200);
            CombatText.NewText(player.getRect(), Color.Purple, "真元恢复提升");
        }
    }
}
