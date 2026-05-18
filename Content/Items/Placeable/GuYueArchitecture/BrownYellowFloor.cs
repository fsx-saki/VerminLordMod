using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{    /// <summary>
    /// 棕黄地板（物品） — 棕黄色的地板
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
