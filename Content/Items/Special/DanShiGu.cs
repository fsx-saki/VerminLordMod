using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 胆识蛊
    /// 未明确，推断为四转或五转
    /// </summary>
    public class DanShiGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 1;
            Item.value = 50000;
        }
    }
}
