using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    /// <summary>
    /// 青矛竹物品 - 可放置为青矛竹装饰植物
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
