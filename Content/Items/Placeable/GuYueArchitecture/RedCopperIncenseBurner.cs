using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    /// <summary>
    /// 赤铜香炉物品 - 可放置为香炉，发光并产生烟雾粒子
    /// </summary>
    public class RedCopperIncenseBurner : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.RedCopperIncenseBurner>());
            Item.width = 20;
            Item.height = 20;
            Item.value = 300;
        }
    }
}
