using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{    /// <summary>
    /// 青竹酒坛（物品） — 青竹制成的酒坛
    /// </summary>
        public class GreenBambooWineJar : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.GreenBambooWineJar>());
            Item.width = 16;
            Item.height = 24;
            Item.value = 200;
            Item.maxStack = 99;
        }
    }
}
