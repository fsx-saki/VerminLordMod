using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.Environment
{
    /// <summary>
    /// 针灸树（物品） — 特殊树木，内含针灸蛊，用于协防
    /// </summary>
    public class 针灸树 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 24;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.针灸树>();
        }
    }
}
