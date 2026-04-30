using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    /// <summary>
    /// 灵泉物品 - 可放置为灵泉装饰物块
    /// </summary>
    public class SpiritSpring : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.SpiritSpring>());
            Item.width = 24;
            Item.height = 24;
            Item.value = 500;
            Item.maxStack = 999;
        }
    }
}
