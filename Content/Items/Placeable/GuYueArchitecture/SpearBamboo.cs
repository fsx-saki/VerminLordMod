using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{    /// <summary>
    /// 青矛竹（物品） — 青茅山的竹子品种
    /// </summary>
        public class SpearBamboo : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.SpearBamboo>());
            Item.width = 12;
            Item.height = 36;
            Item.value = 100;
            Item.maxStack = 999;
        }
    }
}
