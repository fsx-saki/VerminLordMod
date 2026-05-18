using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuYueArchitecture
{
    /// <summary>
    /// 疗伤草（物品） — 具有疗伤效果的草药
    /// </summary>
        public class HealingHerb : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.GuYueArchitecture.HealingHerb>());
            Item.width = 12;
            Item.height = 12;
            Item.value = 200;
            Item.maxStack = 999;
        }
    }
}