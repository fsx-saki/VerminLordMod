using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{    /// <summary>
    /// 吊脚楼（物品） — 古月宗的吊脚楼建筑
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
