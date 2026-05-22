using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 狮吼蛊
    /// 三转（推测）
    /// </summary>
    public class ShiHouGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 10000;
        }
    }
}
