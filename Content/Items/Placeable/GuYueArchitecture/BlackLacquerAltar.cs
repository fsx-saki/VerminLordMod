using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{    /// <summary>
    /// 黑漆祭坛（物品） — 黑漆涂装的祭坛
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
