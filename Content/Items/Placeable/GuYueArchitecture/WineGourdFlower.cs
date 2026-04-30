using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    /// <summary>
    /// 酒囊花蛊物品 - 可放置为酒囊花蛊装饰植物
    /// </summary>
    public class WineGourdFlower : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.WineGourdFlower>());
            Item.width = 12;
            Item.height = 24;
            Item.value = 200;
            Item.maxStack = 999;
        }
    }
}
