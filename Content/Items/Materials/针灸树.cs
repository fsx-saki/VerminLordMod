using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 植物 — 针灸树
    /// 一种特殊的树木，栽种在天露绿洲外围，内含针灸蛊，用于协防。
    /// </summary>
    public class 针灸树 : ModItem
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
