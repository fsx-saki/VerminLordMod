using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{    /// <summary>
    /// 饭袋草（物品） — 形似饭袋的草蛊
    /// </summary>
        public class RiceBagGrass : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.RiceBagGrass>());
            Item.width = 12;
            Item.height = 12;
            Item.value = 200;
            Item.maxStack = 999;
        }
    }
}
