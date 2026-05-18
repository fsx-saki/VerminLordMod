using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{    /// <summary>
    /// 祖先牌位（物品） — 古月宗的祖先牌位
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
