using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    /// <summary>
    /// 青竹酒坛物品 - 可放置为青竹酒坛装饰
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
