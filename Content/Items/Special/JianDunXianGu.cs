using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 剑遁仙蛊
    /// 七转
    /// </summary>
    public class JianDunXianGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Lime;
            Item.maxStack = 1;
            Item.value = 500000;
        }
    }
}
