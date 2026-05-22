using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 矩蛊
    /// 概念级
    /// </summary>
    public class JuGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Red;
            Item.maxStack = 1;
            Item.value = 10000000;
        }
    }
}
