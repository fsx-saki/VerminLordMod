using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.Environment
{
    /// <summary>
    /// 春梦果树（物品） — 奇特的梦道植株，存在于梦境中
    /// </summary>
    public class 春梦果树 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 24;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.春梦果树>();
        }
    }
}
