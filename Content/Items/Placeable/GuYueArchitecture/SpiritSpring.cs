using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{    /// <summary>
    /// 灵泉（物品） — 蕴含灵气的泉水
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
