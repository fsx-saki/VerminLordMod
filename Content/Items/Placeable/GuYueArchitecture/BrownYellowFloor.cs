using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    /// <summary>
    /// 棕黄色地板物品 - 可放置为棕黄色地板物块
    /// </summary>
    public class BrownYellowFloor : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.BrownYellowFloor>());
            Item.width = 12;
            Item.height = 12;
            Item.value = 50;
        }
    }
}
