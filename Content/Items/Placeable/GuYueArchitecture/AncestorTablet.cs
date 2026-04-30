using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    /// <summary>
    /// 先祖牌位物品 - 可放置为供奉牌位
    /// </summary>
    public class AncestorTablet : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.AncestorTablet>());
            Item.width = 16;
            Item.height = 32;
            Item.value = 200;
        }
    }
}
