using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 冰晶蛊
    /// 二转（推测）
    /// </summary>
    public class BingJingGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 1;
            Item.value = 5000;
        }
    }
}
