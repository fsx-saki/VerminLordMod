using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    /// <summary>
    /// 家主阁物品 - 可放置为家主阁大型建筑
    /// </summary>
    public class PatriarchPavilion : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.PatriarchPavilion>());
            Item.width = 80;
            Item.height = 64;
            Item.value = 5000;
            Item.maxStack = 99;
            Item.rare = Terraria.ID.ItemRarityID.Blue;
        }
    }
}
