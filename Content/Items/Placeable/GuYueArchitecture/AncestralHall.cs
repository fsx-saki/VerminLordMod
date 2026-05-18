using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{    /// <summary>
    /// 祖师殿（物品） — 古月宗的祖师殿
    /// </summary>
        public class AncestralHall : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.AncestralHall>());
            Item.width = 80;
            Item.height = 64;
            Item.value = 5000;
            Item.maxStack = 99;
            Item.rare = Terraria.ID.ItemRarityID.Blue;
        }
    }
}
