using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{    /// <summary>
    /// 族长阁（物品） — 古月宗族长的居所
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
