using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{    /// <summary>
    /// 月兰花（物品） — 古月山寨栽培的花卉
    /// </summary>
        public class MoonOrchid : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.MoonOrchid>());
            Item.width = 12;
            Item.height = 12;
            Item.value = 150;
            Item.maxStack = 999;
        }
    }
}
