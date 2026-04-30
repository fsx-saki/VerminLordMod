using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    /// <summary>
    /// 龙丸蛐蛐罐物品 - 可放置为龙丸蛐蛐罐装饰
    /// </summary>
    public class DragonCricketJar : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.DragonCricketJar>());
            Item.width = 16;
            Item.height = 16;
            Item.value = 150;
            Item.maxStack = 99;
        }
    }
}
