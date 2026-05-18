using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 植物 — 血毒棠
    /// 地灾中出现的花，从生长到凋零仅十个呼吸，凋谢后化成毒血，污染福地。
    /// </summary>
    public class 血毒棠 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 99;
            Item.value = 3000;
        }
    }
}
