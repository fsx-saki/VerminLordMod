using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{    /// <summary>
    /// 七彩钟乳石（物品） — 散发七彩光芒的钟乳石
    /// </summary>
        public class GlowingStalactite : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.GlowingStalactite>());
            Item.width = 16;
            Item.height = 24;
            Item.value = 300;
            Item.maxStack = 999;
        }
    }
}
