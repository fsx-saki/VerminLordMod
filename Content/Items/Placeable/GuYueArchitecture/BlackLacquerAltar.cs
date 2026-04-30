using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    /// <summary>
    /// 黑漆台案物品 - 可放置为三层供案
    /// </summary>
    public class BlackLacquerAltar : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.BlackLacquerAltar>());
            Item.width = 36;
            Item.height = 24;
            Item.value = 500;
        }
    }
}
