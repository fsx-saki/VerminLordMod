using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    /// <summary>
    /// 古月学堂书桌物品 - 可放置为书桌家具
    /// </summary>
    /// <summary>
    /// 学堂桌（物品） — 古月宗学堂的桌子
    /// </summary>
        public class SchoolDesk : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.SchoolDesk>());
            Item.width = 36;
            Item.height = 24;
            Item.value = 300;
            Item.maxStack = 99;
        }
    }
}
