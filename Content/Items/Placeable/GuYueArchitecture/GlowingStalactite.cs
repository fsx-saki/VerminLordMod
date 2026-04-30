using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    /// <summary>
    /// 七彩钟乳石物品 - 可放置为发光钟乳石装饰
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
