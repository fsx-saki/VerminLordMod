using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    public class HealingHerb : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.HealingHerb>());
            Item.width = 12;
            Item.height = 12;
            Item.value = 200;
            Item.maxStack = 999;
        }
    }
}