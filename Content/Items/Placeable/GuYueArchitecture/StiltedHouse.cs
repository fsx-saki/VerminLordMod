using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    /// <summary>
    /// 高脚吊楼物品 - 可放置为吊脚楼建筑
    /// </summary>
    public class StiltedHouse : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.StiltedHouse>());
            Item.width = 48;
            Item.height = 48;
            Item.value = 2000;
            Item.maxStack = 99;
        }
    }
}
