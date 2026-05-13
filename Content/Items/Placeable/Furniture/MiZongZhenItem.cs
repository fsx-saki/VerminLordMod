using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Tiles.GuYueArchitecture;

namespace VerminLordMod.Content.Items.Placeable.Furniture
{
    public class MiZongZhenItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 5;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.value = 500000;
            Item.createTile = ModContent.TileType<MiZongZhenTile>();
            Item.rare = ItemRarityID.LightRed;
        }

        public override void AddRecipes()
        {
            // TODO: 添加迷踪阵配方
        }
    }
}