using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    /// <summary>
    /// 毒草（物品） — 含有毒素的草
    /// </summary>
        public class PoisonWeed : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.PoisonWeed>());
            Item.width = 12;
            Item.height = 12;
            Item.value = 250;
            Item.maxStack = 999;
        }
    }
}