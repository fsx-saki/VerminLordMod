using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    public class QiHerb : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.QiHerb>());
            Item.width = 12;
            Item.height = 12;
            Item.value = 300;
            Item.maxStack = 999;
        }
    }
}