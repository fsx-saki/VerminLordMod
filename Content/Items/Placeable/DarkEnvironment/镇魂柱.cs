using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.DarkEnvironment
{
    /// <summary>
    /// 镇魂柱（物品） — 镇压魂魄的石柱，刻满符文
    /// </summary>
    public class 镇魂柱 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.镇魂柱>();
        }
    }
}
